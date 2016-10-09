using System;
using System.Diagnostics;
using Microsoft.VisualStudio.Debugger.Interop;
using Mono.Debugging.Client;
using SampSharp.VisualStudio.DebugEngine.Enumerators;
using SampSharp.VisualStudio.Utils;
using static Microsoft.VisualStudio.VSConstants;

namespace SampSharp.VisualStudio.DebugEngine
{
    public class MonoThread : IDebugThread2, IDebugThread100
    {
        private const string ThreadNameString = "Sample Engine Thread";
        private readonly MonoEngine _engine;
        private ThreadInfo _debuggedThread;
        private int? _lineNumberOverride;

        public MonoThread(MonoEngine engine, ThreadInfo debuggedThread)
        {
            _engine = engine;
            _debuggedThread = debuggedThread;
        }

        private string GetCurrentLocation()
        {
            return _debuggedThread.Location;
        }

        internal ThreadInfo GetDebuggedThread()
        {
            return _debuggedThread;
        }

        internal void SetDebuggedThread(ThreadInfo value)
        {
            _debuggedThread = value;
        }

        #region Implementation of IDebugThread2

        /// <summary>
        ///     Determines whether the next statement can be set to the given stack frame and code context.
        ///     The sample debug engine does not support set next statement, so S_FALSE is returned.
        /// </summary>
        /// <param name="stackFrame">The stack frame.</param>
        /// <param name="codeContext">The code context.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int CanSetNextStatement(IDebugStackFrame2 stackFrame, IDebugCodeContext2 codeContext)
        {
            return S_FALSE;
        }

        /// <summary>
        ///     Retrieves a list of the stack frames for this thread.
        ///     For the sample engine, enumerating the stack frames requires walking the callstack in the debuggee for this thread
        ///     and coverting that to an implementation of IEnumDebugFrameInfo2.
        ///     Real engines will most likely want to cache this information to avoid recomputing it each time it is asked for,
        ///     and or construct it on demand instead of walking the entire stack.
        /// </summary>
        /// <param name="dwFieldSpec">The field spec.</param>
        /// <param name="nRadix">The radix.</param>
        /// <param name="enumObject">The enum object.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int EnumFrameInfo(enum_FRAMEINFO_FLAGS dwFieldSpec, uint nRadix, out IEnumDebugFrameInfo2 enumObject)
        {
            // Ask the lower-level to perform a stack walk on this thread
            enumObject = null;

            try
            {
                var numStackFrames = _debuggedThread.Backtrace.FrameCount;
                FRAMEINFO[] frameInfoArray;

                if (numStackFrames == 0)
                {
                    // failed to walk any frames. Only return the top frame.
                    frameInfoArray = new FRAMEINFO[0];
                    // MonoStackFrame frame = new MonoStackFrame(engine, this, debuggedThread);
                    // frame.SetFrameInfo(dwFieldSpec, out frameInfoArray[0]);
                }
                else
                {
                    frameInfoArray = new FRAMEINFO[numStackFrames];

                    for (var i = 0; i < numStackFrames; i++)
                    {
                        var i1 = i;
                        var frame = new MonoStackFrame(_engine, this, () => _debuggedThread.Backtrace.GetFrame(i1));
                        if (_lineNumberOverride != null)
                            frame.LineNumber = _lineNumberOverride.Value;
                        frame.SetFrameInfo(dwFieldSpec, out frameInfoArray[i]);
                    }
                }

                enumObject = new MonoFrameInfoEnumerator(frameInfoArray);
                return S_OK;
            }
            catch (ComponentException e)
            {
                return e.HResult;
            }
            catch (Exception e)
            {
                return EngineUtils.UnexpectedException(e);
            }
        }

        /// <summary>
        ///     Get the name of the thread. For the sample engine, the name of the thread is always "Sample Engine Thread".
        /// </summary>
        /// <param name="threadName">Name of the thread.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetName(out string threadName)
        {
            threadName = ThreadNameString;
            return S_OK;
        }


        /// <summary>
        ///     Return the program that this thread belongs to.
        /// </summary>
        /// <param name="program">The program.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetProgram(out IDebugProgram2 program)
        {
            program = _engine;
            return S_OK;
        }


        /// <summary>
        ///     Gets the system thread identifier.
        /// </summary>
        /// <param name="threadId">The thread identifier.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetThreadId(out uint threadId)
        {
            threadId = (uint) _debuggedThread.Id;
            return S_OK;
        }


        /// <summary>
        ///     Gets properties that describe a thread..
        /// </summary>
        /// <param name="dwFields">The fields.</param>
        /// <param name="propertiesArray">The properties array.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetThreadProperties(enum_THREADPROPERTY_FIELDS dwFields, THREADPROPERTIES[] propertiesArray)
        {
            try
            {
                var props = new THREADPROPERTIES();

                if ((dwFields & enum_THREADPROPERTY_FIELDS.TPF_ID) != 0)
                {
                    props.dwThreadId = (uint) _debuggedThread.Id;
                    props.dwFields |= enum_THREADPROPERTY_FIELDS.TPF_ID;
                }
                if ((dwFields & enum_THREADPROPERTY_FIELDS.TPF_SUSPENDCOUNT) != 0)
                    props.dwFields |= enum_THREADPROPERTY_FIELDS.TPF_SUSPENDCOUNT;
                if ((dwFields & enum_THREADPROPERTY_FIELDS.TPF_STATE) != 0)
                {
                    props.dwThreadState = (uint) enum_THREADSTATE.THREADSTATE_RUNNING;
                    props.dwFields |= enum_THREADPROPERTY_FIELDS.TPF_STATE;
                }
                if ((dwFields & enum_THREADPROPERTY_FIELDS.TPF_PRIORITY) != 0)
                {
                    props.bstrPriority = "Normal";
                    props.dwFields |= enum_THREADPROPERTY_FIELDS.TPF_PRIORITY;
                }
                if ((dwFields & enum_THREADPROPERTY_FIELDS.TPF_NAME) != 0)
                {
                    props.bstrName = ThreadNameString;
                    props.dwFields |= enum_THREADPROPERTY_FIELDS.TPF_NAME;
                }
                if ((dwFields & enum_THREADPROPERTY_FIELDS.TPF_LOCATION) != 0)
                {
                    props.bstrLocation = GetCurrentLocation();
                    props.dwFields |= enum_THREADPROPERTY_FIELDS.TPF_LOCATION;
                }

                return S_OK;
            }
            catch (ComponentException e)
            {
                return e.HResult;
            }
            catch (Exception e)
            {
                return EngineUtils.UnexpectedException(e);
            }
        }

        /// <summary>
        ///     Resume a thread.
        ///     This is called when the user chooses "Unfreeze" from the threads window when a thread has previously been frozen.
        /// </summary>
        /// <param name="suspendCount">The suspend count.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int Resume(out uint suspendCount)
        {
            // The sample debug engine doesn't support suspending/resuming threads
            suspendCount = 0;
            return E_NOTIMPL;
        }


        /// <summary>
        ///     Sets the next statement to the given stack frame and code context.
        /// </summary>
        /// <param name="stackFrame">The stack frame.</param>
        /// <param name="codeContext">The code context.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int SetNextStatement(IDebugStackFrame2 stackFrame, IDebugCodeContext2 codeContext)
        {
            IDebugDocumentContext2 context;
            codeContext.GetDocumentContext(out context);
            string fileName;
            context.GetName(enum_GETNAME_TYPE.GN_FILENAME, out fileName);
            // fileName = engine.TranslateToBuildServerPath(fileName);

            var startPosition = new TEXT_POSITION[1];
            var endPosition = new TEXT_POSITION[1];
            context.GetStatementRange(startPosition, endPosition);

            EventHandler<TargetEventArgs> stepFinished = null;
            // var waiter = new AutoResetEvent(false);
            stepFinished = (sender, args) =>
            {
                _lineNumberOverride = null;
                // if (true || args.Thread.Backtrace.GetFrame(0).SourceLocation.Line == startPosition[0].dwLine)
                // {
                _engine.Program.Session.TargetStopped -= stepFinished;
                //     waiter.Set();
                //     engine.Send(new MonoBreakpointEvent(new MonoBoundBreakpointsEnumerator(new IDebugBoundBreakpoint2[0])), MonoStepCompleteEvent.Iid, engine.ThreadManager[args.Thread]);
                // }
                // else
                // {
                //     engine.Session.NextInstruction();
                // }
            };
            _engine.Program.Session.TargetStopped += stepFinished;
            _engine.Program.Session.SetNextStatement(fileName, (int) startPosition[0].dwLine,
                (int) startPosition[0].dwColumn + 1);
            _lineNumberOverride = (int) startPosition[0].dwLine;
            // engine.Session.Stop();
            // engine.Session.NextInstruction();
            // waiter.WaitOne();

            return S_OK;
        }

        /// <summary>
        ///     suspend a thread.
        ///     This is called when the user chooses "Freeze" from the threads window
        /// </summary>
        /// <param name="suspendCount">The suspend count.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int Suspend(out uint suspendCount)
        {
            // The sample debug engine doesn't support suspending/resuming threads
            suspendCount = 0;
            return E_NOTIMPL;
        }

        #endregion

        #region Implementation of IDebugThread100

        /// <summary>
        ///     Sets the display name of the thread.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int SetThreadDisplayName(string name)
        {
            // Not necessary to implement in the debug engine. Instead
            // it is implemented in the SDM.
            return E_NOTIMPL;
        }

        /// <summary>
        ///     Gets the display name of the thread.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetThreadDisplayName(out string name)
        {
            // Not necessary to implement in the debug engine. Instead
            // it is implemented in the SDM, which calls GetThreadProperties100()
            name = "";
            return E_NOTIMPL;
        }


        /// <summary>
        ///     Returns whether this thread can be used to do function/property evaluation.
        /// </summary>
        /// <returns>S_OK or S_FALSE.</returns>
        public int CanDoFuncEval()
        {
            return S_FALSE;
        }

        /// <summary>
        ///     Sets the flags.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int SetFlags(uint flags)
        {
            // Not necessary to implement in the debug engine. Instead
            // it is implemented in the SDM.
            return E_NOTIMPL;
        }

        /// <summary>
        ///     Gets the flags.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetFlags(out uint flags)
        {
            // Not necessary to implement in the debug engine. Instead
            // it is implemented in the SDM.
            flags = 0;
            return E_NOTIMPL;
        }

        /// <summary>
        ///     Gets the thread properties.
        /// </summary>
        /// <param name="dwFields">The fields.</param>
        /// <param name="props">The properties.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetThreadProperties100(uint dwFields, THREADPROPERTIES100[] props)
        {
            // Invoke GetThreadProperties to get the VS7/8/9 properties
            var props90 = new THREADPROPERTIES[1];
            var dwFields90 = (enum_THREADPROPERTY_FIELDS) (dwFields & 0x3f);
            var hRes = ((IDebugThread2) this).GetThreadProperties(dwFields90, props90);
            props[0].bstrLocation = props90[0].bstrLocation;
            props[0].bstrName = props90[0].bstrName;
            props[0].bstrPriority = props90[0].bstrPriority;
            props[0].dwFields = (uint) props90[0].dwFields;
            props[0].dwSuspendCount = props90[0].dwSuspendCount;
            props[0].dwThreadId = props90[0].dwThreadId;
            props[0].dwThreadState = props90[0].dwThreadState;

            // Populate the new fields
            if ((hRes == S_OK) && (dwFields != (uint) dwFields90))
            {
                if ((dwFields & (uint) enum_THREADPROPERTY_FIELDS100.TPF100_DISPLAY_NAME) != 0)
                {
                    // Thread display name is being requested
                    props[0].bstrDisplayName = _debuggedThread.Name;
                    props[0].dwFields |= (uint) enum_THREADPROPERTY_FIELDS100.TPF100_DISPLAY_NAME;

                    // Give this display name a higher priority than the default (0)
                    // so that it will actually be displayed
                    props[0].DisplayNamePriority = 10;
                    props[0].dwFields |= (uint) enum_THREADPROPERTY_FIELDS100.TPF100_DISPLAY_NAME_PRIORITY;
                }

                if ((dwFields & (uint) enum_THREADPROPERTY_FIELDS100.TPF100_CATEGORY) != 0)
                {
                    // Thread category is being requested
                    props[0].dwThreadCategory = 0;
                    props[0].dwFields |= (uint) enum_THREADPROPERTY_FIELDS100.TPF100_CATEGORY;
                }

                if ((dwFields & (uint) enum_THREADPROPERTY_FIELDS100.TPF100_AFFINITY) != 0)
                {
                    // Thread cpu affinity is being requested
                    props[0].AffinityMask = 0;
                    props[0].dwFields |= (uint) enum_THREADPROPERTY_FIELDS100.TPF100_AFFINITY;
                }

                if ((dwFields & (uint) enum_THREADPROPERTY_FIELDS100.TPF100_PRIORITY_ID) != 0)
                {
                    // Thread display name is being requested
                    props[0].priorityId = 0;
                    props[0].dwFields |= (uint) enum_THREADPROPERTY_FIELDS100.TPF100_PRIORITY_ID;
                }
            }

            return hRes;
        }

        #endregion

        #region Deprecated interface methods

        // These methods are not currently called by the Visual Studio debugger, so they don't need to be implemented

        public int GetLogicalThread(IDebugStackFrame2 stackFrame, out IDebugLogicalThread2 logicalThread)
        {
            logicalThread = null;

            Debug.Fail("This function is not called by the debugger");

            return E_NOTIMPL;
        }

        public int SetThreadName(string name)
        {
            Debug.Fail("This function is not called by the debugger");

            return E_NOTIMPL;
        }

        #endregion
    }
}