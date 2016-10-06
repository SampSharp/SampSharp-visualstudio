using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.OLE.Interop;

namespace SampSharp.VisualStudio.DebugEngine
{
    public class MonoCallbackWrapper : IDebugEventCallback2
    {
        private readonly object _cacheLock = new object();
        // If we store the event callback in a normal IDebugEventCallback2 member, COM interop will attempt to call back to the main UI
        // thread the first time that we invoke the call back. To work around this, we will instead store the event call back in the 
        // global interface table like we would if we implemented this code in native.
        private readonly uint _cookie;

        // NOTE: The GIT doesn't aggregate the free threaded marshaler, so we can't store it in an RCW
        // or the CLR will just call right back to the main thread to try and marshal it.
        private readonly IntPtr _pGit;
        private int _cachedEventCallbackThread;
        private IDebugEventCallback2 _cacheEventCallback;
        private Guid _iidIDebugEventCallback2 = typeof(IDebugEventCallback2).GUID;

        internal MonoCallbackWrapper(IDebugEventCallback2 ad7Callback)
        {
            // Obtain the GIT from COM, and store the event callback in it
            var clsidStdGlobalInterfaceTable = new Guid("00000323-0000-0000-C000-000000000046");
            var iidIGlobalInterfaceTable = typeof(IGlobalInterfaceTable).GUID;
            const int clsctxInprocServer = 0x1;
            _pGit = NativeMethods.CoCreateInstance(ref clsidStdGlobalInterfaceTable, IntPtr.Zero, clsctxInprocServer,
                ref iidIGlobalInterfaceTable);

            var git = GetGlobalInterfaceTable();
            git.RegisterInterfaceInGlobal(ad7Callback, ref _iidIDebugEventCallback2, out _cookie);
            Marshal.ReleaseComObject(git);
        }

        #region Implementation of IDebugEventCallback2

        /// <summary>
        ///     Sends notification of debugging events to the SDM.
        /// </summary>
        /// <param name="engine">
        ///     An IDebugEngine2 object that represents the debug engine (DE) that is sending this event. A DE is
        ///     required to fill out this parameter.
        /// </param>
        /// <param name="process">
        ///     An IDebugProcess2 object that represents the process in which the event occurs. This parameter is
        ///     filled in by the session debug manager (SDM). A DE always passes a null value for this parameter.
        /// </param>
        /// <param name="program">
        ///     An IDebugProgram2 object that represents the program in which this event occurs. For most events,
        ///     this parameter is not a null value.
        /// </param>
        /// <param name="thread">
        ///     An IDebugThread2 object that represents the thread in which this event occurs. For stopping
        ///     events, this parameter cannot be a null value as the stack frame is obtained from this parameter.
        /// </param>
        /// <param name="event">An IDebugEvent2 object that represents the debug event.</param>
        /// <param name="riidEvent">GUID that identifies which event interface to obtain from the pEvent parameter.</param>
        /// <param name="attribs">A combination of flags from the EVENTATTRIBUTES enumeration.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int Event(IDebugEngine2 engine, IDebugProcess2 process, IDebugProgram2 program,
            IDebugThread2 thread, IDebugEvent2 @event, ref Guid riidEvent, uint attribs)
        {
            var ad7EventCallback = GetAd7EventCallback();
            return ad7EventCallback.Event(engine, process, program, thread, @event, ref riidEvent, attribs);
        }

        #endregion

        ~MonoCallbackWrapper()
        {
            // NOTE: This object does NOT implement the dispose pattern. The reasons are --
            // 1. The underlying thing we are disposing is the SDM's IDebugEventCallback2. We are not going to get
            //    deterministic release of this without both implementing the dispose pattern on this object but also
            //    switching to use Marshal.ReleaseComObject everywhere. Marshal.ReleaseComObject is difficult to get
            //    right and there isn't a large need to deterministically release the SDM's event callback.
            // 2. There is some risk of deadlock if we tried to implement the dispose pattern because of the trickiness
            //    of releasing cross-thread COM interfaces. We could avoid this by doing an async dispose, but then
            //    we losing the primary benefit of dispose which is the deterministic release.

            if (_cookie != 0)
            {
                var git = GetGlobalInterfaceTable();
                git.RevokeInterfaceFromGlobal(_cookie);
                Marshal.ReleaseComObject(git);
            }

            if (_pGit != IntPtr.Zero)
                Marshal.Release(_pGit);
        }

        private IGlobalInterfaceTable GetGlobalInterfaceTable()
        {
            Debug.Assert(_pGit != IntPtr.Zero, "GetGlobalInterfaceTable called before the m_pGIT is initialized");
            // NOTE: We want to use GetUniqueObjectForIUnknown since the GIT will exist in both the STA and the MTA, and we don't want
            // them to be the same rcw
            return (IGlobalInterfaceTable) Marshal.GetUniqueObjectForIUnknown(_pGit);
        }

        private IDebugEventCallback2 GetAd7EventCallback()
        {
            Debug.Assert(_cookie != 0, "GetEventCallback called before m_cookie is initialized");

            // We send esentially all events from the same thread, so lets optimize the common case
            var currentThreadId = Thread.CurrentThread.ManagedThreadId;
            if ((_cacheEventCallback != null) && (_cachedEventCallbackThread == currentThreadId))
                lock (_cacheLock)
                {
                    if ((_cacheEventCallback != null) && (_cachedEventCallbackThread == currentThreadId))
                        return _cacheEventCallback;
                }

            var git = GetGlobalInterfaceTable();

            IntPtr pCallback;
            git.GetInterfaceFromGlobal(_cookie, ref _iidIDebugEventCallback2, out pCallback);

            Marshal.ReleaseComObject(git);

            var eventCallback = (IDebugEventCallback2) Marshal.GetObjectForIUnknown(pCallback);
            Marshal.Release(pCallback);

            lock (_cacheLock)
            {
                _cachedEventCallbackThread = currentThreadId;
                _cacheEventCallback = eventCallback;
            }

            return eventCallback;
        }

        private static class NativeMethods
        {
            [DllImport("ole32.dll", ExactSpelling = true, PreserveSig = false)]
            public static extern IntPtr CoCreateInstance([In] ref Guid clsid, IntPtr punkOuter, int context,
                [In] ref Guid iid);
        }
    }
}