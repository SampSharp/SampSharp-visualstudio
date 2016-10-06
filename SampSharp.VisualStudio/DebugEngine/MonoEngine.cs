using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Mono.Debugging.Client;
using Mono.Debugging.Soft;
using SampSharp.VisualStudio.DebugEngine.Enumerators;
using SampSharp.VisualStudio.DebugEngine.Events;
using SampSharp.VisualStudio.Debugger;
using SampSharp.VisualStudio.Utils;
using Task = System.Threading.Tasks.Task;

namespace SampSharp.VisualStudio.DebugEngine
{
    [Guid("D78CF801-CE2A-499B-BF1F-C81742877A34")]
    public class MonoEngine : IDebugEngine2, IDebugProgram3, IDebugEngineLaunch2, IDebugSymbolSettings100
    {
        private readonly MonoBreakpointManager _breakpointManager;
        private IVsOutputWindowPane _outputWindow;
        private AD_PROCESS_ID _processId;

        private Guid _programId;
        private AutoResetEvent _waiter;

        public MonoEngine()
        {
            _breakpointManager = new MonoBreakpointManager(this);
            ThreadManager = new MonoThreadManager(this);
        }

        public MonoThreadManager ThreadManager { get; }
        public SoftDebuggerSession Session { get; private set; }
        public IDebugEventCallback2 Callback { get; private set; }

        #region Implementation of IDebugSymbolSettings100

        /// <summary>
        ///     The SDM will call this method on the debug engine when it is created, to notify it of the user's
        ///     symbol settings in Tools->Options->Debugging->Symbols.
        /// </summary>
        /// <param name="isManual">true if 'Automatically load symbols: Only for specified modules' is checked.</param>
        /// <param name="loadAdjacentSymbols">true if 'Specify modules'->'Always load symbols next to the modules' is checked.</param>
        /// <param name="includeList">semicolon-delimited list of modules when automatically loading 'Only specified modules'</param>
        /// <param name="excludeList">
        ///     semicolon-delimited list of modules when automatically loading 'All modules, unless
        ///     excluded'.
        /// </param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int SetSymbolLoadState(int isManual, int loadAdjacentSymbols, string includeList, string excludeList)
        {
            return VSConstants.S_OK;
        }

        #endregion

        #region Methods of MonoEngine

        /// <summary>
        ///     Gets the document name for the document at the pointer.
        /// </summary>
        /// <param name="locationPtr"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public string GetDocumentName(IntPtr locationPtr)
        {
            TEXT_POSITION[] startPosition;
            TEXT_POSITION[] endPosition;
            return GetLocationInfo(locationPtr, out startPosition, out endPosition);
        }

        /// <summary>
        ///     Gets the location information for the specified document position.
        /// </summary>
        /// <param name="locationPtr"></param>
        /// <param name="startPosition"></param>
        /// <param name="endPosition"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public string GetLocationInfo(IntPtr locationPtr, out TEXT_POSITION[] startPosition,
            out TEXT_POSITION[] endPosition)
        {
            var docPosition = (IDebugDocumentPosition2) Marshal.GetObjectForIUnknown(locationPtr);
            var result = GetLocationInfo(docPosition, out startPosition, out endPosition);
            Marshal.ReleaseComObject(docPosition);
            return result;
        }

        /// <summary>
        ///     Gets the location information for the specifed document position.
        /// </summary>
        /// <param name="docPosition"></param>
        /// <param name="startPosition"></param>
        /// <param name="endPosition"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public string GetLocationInfo(IDebugDocumentPosition2 docPosition, out TEXT_POSITION[] startPosition,
            out TEXT_POSITION[] endPosition)
        {
            string documentName;
            EngineUtils.CheckOk(docPosition.GetFileName(out documentName));

            startPosition = new TEXT_POSITION[1];
            endPosition = new TEXT_POSITION[1];
            EngineUtils.CheckOk(docPosition.GetRange(startPosition, endPosition));

            return documentName;
        }

        /// <summary>
        ///     Logs a message to the debugger, console and output window.
        /// </summary>
        /// <param name="message"></param>
        public void Log(string message)
        {
            Debug.WriteLine(message);
            Console.WriteLine(message);
            _outputWindow.OutputString(message + "\r\n");
        }

        /// <summary>
        ///     Sends an event to the callback.
        /// </summary>
        /// <param name="event"></param>
        /// <param name="eventId"></param>
        /// <param name="program"></param>
        /// <param name="thread"></param>
        public void Send(IDebugEvent2 @event, string eventId, IDebugProgram2 program, IDebugThread2 thread)
        {
            Callback.Send(this, @event, eventId, program, thread);
        }

        /// <summary>
        ///     Sends an event to the callback.
        /// </summary>
        /// <param name="event"></param>
        /// <param name="eventId"></param>
        /// <param name="thread"></param>
        public void Send(IDebugEvent2 @event, string eventId, IDebugThread2 thread)
        {
            Send(@event, eventId, this, thread);
        }

        #endregion

        #region Implementation of IDebugEngine2

        /// <summary>
        ///     Creates a pending breakpoint in the engine. A pending breakpoint is contains all the information needed to bind a
        ///     breakpoint to
        ///     a location in the debuggee.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="pendingBreakpoint"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int CreatePendingBreakpoint(IDebugBreakpointRequest2 request,
            out IDebugPendingBreakpoint2 pendingBreakpoint)
        {
            pendingBreakpoint = new MonoPendingBreakpoint(_breakpointManager, request);

            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Specifies how the DE should handle a given exception.
        ///     The sample engine does not support exceptions in the debuggee so this method is not actually implemented.
        /// </summary>
        /// <param name="pException"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int SetException(EXCEPTION_INFO[] pException)
        {
            if (pException[0].dwState.HasFlag(enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE))
            {
                var catchpoint = Session.Breakpoints.AddCatchpoint(pException[0].bstrExceptionName);
                _breakpointManager.Add(catchpoint);
                return VSConstants.S_OK;
            }
            return RemoveSetException(pException);
        }

        /// <summary>
        /// </summary>
        /// <param name="pException"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int RemoveSetException(EXCEPTION_INFO[] pException)
        {
            if (_breakpointManager.ContainsCatchpoint(pException[0].bstrExceptionName))
            {
                _breakpointManager.Remove(_breakpointManager[pException[0].bstrExceptionName]);
                Session.Breakpoints.RemoveCatchpoint(pException[0].bstrExceptionName);
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Removes the list of exceptions the IDE has set for a particular run-time architecture or language.
        /// </summary>
        /// <param name="guidType"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int RemoveAllSetExceptions(ref Guid guidType)
        {
            foreach (var catchpoint in _breakpointManager.Catchpoints)
                Session.Breakpoints.RemoveCatchpoint(catchpoint.ExceptionName);
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Gets the GUID of the DE.
        /// </summary>
        /// <param name="engineGuid">The unique identifier of the DE.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetEngineId(out Guid engineGuid)
        {
            engineGuid = Guids.EngineIdGuid;
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Informs a DE that the program specified has been atypically terminated and that the DE should
        ///     clean up all references to the program and send a program destroy event.
        /// </summary>
        /// <param name="pProgram">The p program.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int DestroyProgram(IDebugProgram2 pProgram)
        {
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        ///     Called by the SDM to indicate that a synchronous debug event, previously sent by the DE to the SDM,
        ///     was received and processed. The only event the sample engine sends in this fashion is Program Destroy.
        ///     It responds to that event by shutting down the engine.
        /// </summary>
        /// <param name="event">The event.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int ContinueFromSynchronousEvent(IDebugEvent2 @event)
        {
            if (@event is SampSharpDestroyEvent)
                Session.Dispose();
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Sets the locale of the DE.
        ///     This method is called by the session debug manager (SDM) to propagate the locale settings of the IDE so that
        ///     strings returned by the DE are properly localized. The sample engine is not localized so this is not implemented.
        /// </summary>
        /// <param name="languageId">The language identifier.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int SetLocale(ushort languageId)
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Sets the registry root currently in use by the DE. Different installations of Visual Studio can change where their
        ///     registry information is stored
        ///     This allows the debugger to tell the engine where that location is.
        /// </summary>
        /// <param name="registryRoot">The registry root.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int SetRegistryRoot(string registryRoot)
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     A metric is a registry value used to change a debug engine's behavior or to advertise supported functionality.
        ///     This method can forward the call to the appropriate form of the Debugging SDK Helpers function, SetMetric.
        /// </summary>
        /// <param name="metric">The metric.</param>
        /// <param name="value">The value.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int SetMetric(string metric, object value)
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     The debugger calls CauseBreak when the user clicks on the pause button in VS. The debugger should respond by
        ///     entering
        ///     breakmode.
        /// </summary>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int CauseBreak()
        {
            EventHandler<TargetEventArgs> stepFinished = null;
            stepFinished = (sender, args) =>
            {
                Session.TargetStopped -= stepFinished;

                var thread = ThreadManager[args.Thread] ?? ThreadManager.All.First();

                Send(new MonoBreakpointEvent(new MonoBoundBreakpointsEnumerator(new IDebugBoundBreakpoint2[0])),
                    MonoStepCompleteEvent.Iid, thread);
            };
            Session.TargetStopped += stepFinished;

            Session.Stop();
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Attach the debug engine to a program.
        /// </summary>
        /// <param name="programs"></param>
        /// <param name="rgpProgramNodes">.</param>
        /// <param name="celtPrograms"></param>
        /// <param name="pCallback"></param>
        /// <param name="dwReason"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int Attach(IDebugProgram2[] programs, IDebugProgramNode2[] rgpProgramNodes, uint celtPrograms,
            IDebugEventCallback2 pCallback, enum_ATTACH_REASON dwReason)
        {
            var program = programs[0];
            IDebugProcess2 process;
            program.GetProcess(out process);
            Guid processId;
            process.GetProcessId(out processId);
            if (processId != _processId.guidProcessId)
                return VSConstants.S_FALSE;

            EngineUtils.RequireOk(program.GetProgramId(out _programId));

            Task.Run(() =>
            {
                _waiter.WaitOne();

                var ipAddress = HostUtils.ResolveHostOrIpAddress("127.0.0.1");
                Session.Run(new SoftDebuggerStartInfo(new SoftDebuggerConnectArgs("", ipAddress, 6438)),
                    new DebuggerSessionOptions
                    {
                        EvaluationOptions = EvaluationOptions.DefaultOptions,
                        ProjectAssembliesOnly = false
                    });
            });

            MonoEngineCreateEvent.Send(this);
            SampSharpCreateEvent.Send(this);

            return VSConstants.S_OK;
        }

        #endregion

        #region Implementation of IDebugProgram2

        /// <summary>
        ///     EnumThreads is called by the debugger when it needs to enumerate the threads in the program.
        /// </summary>
        /// <param name="ppEnum"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int EnumThreads(out IEnumDebugThreads2 ppEnum)
        {
            var threads = ThreadManager.All.ToArray();

            var threadObjects = new IDebugThread2[threads.Length];
            for (var i = 0; i < threads.Length; i++)
                threadObjects[i] = threads[i];

            ppEnum = new MonoThreadEnumerator(threadObjects);

            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Gets the name of the program.
        ///     The name returned by this method is always a friendly, user-displayable name that describes the program.
        /// </summary>
        /// <param name="programName">Name of the program.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetName(out string programName)
        {
            programName = null;
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Terminates the program.
        /// </summary>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int Terminate()
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Determines if a debug engine (DE) can detach from the program.
        /// </summary>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int CanDetach()
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Detach is called when debugging is stopped and the process was attached to (as opposed to launched)
        ///     or when one of the Detach commands are executed in the UI.
        /// </summary>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int Detach()
        {
            if (!Session.IsRunning)
                Session.Continue();

            Session.Dispose();
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Gets a GUID for this program. A debug engine (DE) must return the program identifier originally passed to the
        ///     IDebugProgramNodeAttach2::OnAttach
        ///     or IDebugEngine2::Attach methods. This allows identification of the program across debugger components.
        /// </summary>
        /// <param name="programId">The program identifier.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetProgramId(out Guid programId)
        {
            programId = _programId;
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     The properties returned by this method are specific to the program. If the program needs to return more than one
        ///     property,
        ///     then the IDebugProperty2 object returned by this method is a container of additional properties and calling the
        ///     IDebugProperty2::EnumChildren method returns a list of all properties.
        ///     A program may expose any number and type of additional properties that can be described through the IDebugProperty2
        ///     interface.
        ///     An IDE might display the additional program properties through a generic property browser user interface.
        ///     The sample engine does not support this
        /// </summary>
        /// <param name="ppProperty"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetDebugProperty(out IDebugProperty2 ppProperty)
        {
            ppProperty = null;

            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        ///     Gets the name and identifier of the debug engine (DE) running this program.
        /// </summary>
        /// <param name="pbstrEngine"></param>
        /// <param name="pguidEngine"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetEngineInfo(out string pbstrEngine, out Guid pguidEngine)
        {
            // TODO: Implement
            pbstrEngine = null;
            pguidEngine = Guid.Empty;

            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        ///     The memory bytes as represented by the IDebugMemoryBytes2 object is for the program's image in memory and not any
        ///     memory
        ///     that was allocated when the program was executed.
        /// </summary>
        /// <param name="ppMemoryBytes"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes)
        {
            ppMemoryBytes = null;

            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        ///     The debugger calls this when it needs to obtain the IDebugDisassemblyStream2 for a particular code-context.
        ///     The sample engine does not support dissassembly so it returns E_NOTIMPL
        ///     In order for this to be called, the Disassembly capability must be set in the registry for this Engine
        /// </summary>
        /// <param name="dwScope"></param>
        /// <param name="pCodeContext"></param>
        /// <param name="ppDisassemblyStream"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetDisassemblyStream(enum_DISASSEMBLY_STREAM_SCOPE dwScope, IDebugCodeContext2 pCodeContext,
            out IDebugDisassemblyStream2 ppDisassemblyStream)
        {
            ppDisassemblyStream = null;
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        ///     EnumModules is called by the debugger when it needs to enumerate the modules in the program.
        /// </summary>
        /// <param name="ppEnum"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int EnumModules(out IEnumDebugModules2 ppEnum)
        {
            ppEnum = new MonoModuleEnumerator(new IDebugModule2[] { new MonoModule(this) });
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     This method gets the Edit and Continue (ENC) update for this program. A custom debug engine always returns
        ///     E_NOTIMPL
        /// </summary>
        /// <param name="ppUpdate"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetENCUpdate(out object ppUpdate)
        {
            ppUpdate = null;
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        ///     EnumCodePaths is used for the step-into specific feature -- right click on the current statment and decide which
        ///     function to step into. This is not something that the SampleEngine supports.
        /// </summary>
        /// <param name="hint">The hint.</param>
        /// <param name="start">The start.</param>
        /// <param name="frame">The frame.</param>
        /// <param name="fSource">The f source.</param>
        /// <param name="pathEnum">The path enum.</param>
        /// <param name="safetyContext">The safety context.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int EnumCodePaths(string hint, IDebugCodeContext2 start, IDebugStackFrame2 frame, int fSource,
            out IEnumCodePaths2 pathEnum, out IDebugCodeContext2 safetyContext)
        {
            pathEnum = null;
            safetyContext = null;
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        ///     Writes a dump to a file.
        /// </summary>
        /// <param name="dumptype"></param>
        /// <param name="pszDumpUrl"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int WriteDump(enum_DUMPTYPE dumptype, string pszDumpUrl)
        {
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region Implementation of IDebugProgram3

        /// <summary>
        ///     Steps to the next statement.
        /// </summary>
        /// <param name="thread"></param>
        /// <param name="kind"></param>
        /// <param name="unit"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int Step(IDebugThread2 thread, enum_STEPKIND kind, enum_STEPUNIT unit)
        {
            switch (kind)
            {
                case enum_STEPKIND.STEP_BACKWARDS:
                    return VSConstants.E_NOTIMPL;
            }

            EventHandler<TargetEventArgs> stepFinished = null;
            stepFinished = (sender, args) =>
            {
                Session.TargetStopped -= stepFinished;
                Send(new MonoStepCompleteEvent(), MonoStepCompleteEvent.Iid, ThreadManager[args.Thread]);
            };
            Session.TargetStopped += stepFinished;

            switch (kind)
            {
                case enum_STEPKIND.STEP_OVER:
                    switch (unit)
                    {
                        case enum_STEPUNIT.STEP_INSTRUCTION:
                            Session.NextInstruction();
                            break;
                        default:
                            Session.NextLine();
                            break;
                    }
                    break;
                case enum_STEPKIND.STEP_INTO:
                    switch (unit)
                    {
                        case enum_STEPUNIT.STEP_INSTRUCTION:
                            Session.StepInstruction();
                            break;
                        default:
                            Session.StepLine();
                            break;
                    }
                    break;
                case enum_STEPKIND.STEP_OUT:
                    Session.Finish();
                    break;
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Enumerates the code contexts for a given position in a source file.
        /// </summary>
        /// <param name="pDocPos"></param>
        /// <param name="ppEnum"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int EnumCodeContexts(IDebugDocumentPosition2 pDocPos, out IEnumDebugCodeContexts2 ppEnum)
        {
            TEXT_POSITION[] startPosition;
            TEXT_POSITION[] endPosition;
            var documentName = _breakpointManager.Engine.GetLocationInfo(pDocPos, out startPosition, out endPosition);

            var textPosition = new TEXT_POSITION { dwLine = startPosition[0].dwLine + 1 };
            var documentContext = new MonoDocumentContext(documentName, textPosition, textPosition, null);
            ppEnum =
                new MonoCodeContextEnumerator(new IDebugCodeContext2[]
                    { new MonoMemoryAddress(this, 0, documentContext) });
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Continue is called from the SDM when it wants execution to continue in the debugee
        ///     but have stepping state remain. An example is when a tracepoint is executed,
        ///     and the debugger does not want to actually enter break mode.
        /// </summary>
        /// <param name="pThread"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int Continue(IDebugThread2 pThread)
        {
            Session.Continue();
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     ExecuteOnThread is called when the SDM wants execution to continue and have
        ///     stepping state cleared.
        /// </summary>
        /// <param name="pThread"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int ExecuteOnThread(IDebugThread2 pThread)
        {
            var monoThread = (MonoThread) pThread;
            var thread = monoThread.GetDebuggedThread();
            if (Session.ActiveThread?.Id != thread.Id)
                thread.SetActive();
            Session.Continue();
            return VSConstants.S_OK;
        }

        #endregion

        #region Implementation of IDebugEngineLaunch2

        /// <summary>
        ///     Launches a process by means of the debug engine.
        ///     Normally, Visual Studio launches a program using the IDebugPortEx2::LaunchSuspended method and then attaches the
        ///     debugger
        ///     to the suspended program. However, there are circumstances in which the debug engine may need to launch a program
        ///     (for example, if the debug engine is part of an interpreter and the program being debugged is an interpreted
        ///     language),
        ///     in which case Visual Studio uses the IDebugEngineLaunch2::LaunchSuspended method
        ///     The IDebugEngineLaunch2::ResumeProcess method is called to start the process after the process has been
        ///     successfully launched in a suspended state.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="port"></param>
        /// <param name="exe"></param>
        /// <param name="args"></param>
        /// <param name="directory"></param>
        /// <param name="environment"></param>
        /// <param name="options"></param>
        /// <param name="launchFlags"></param>
        /// <param name="standardInput"></param>
        /// <param name="standardOutput"></param>
        /// <param name="standardError"></param>
        /// <param name="callback"></param>
        /// <param name="process"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int LaunchSuspended(string server, IDebugPort2 port, string exe, string args, string directory,
            string environment, string options, enum_LAUNCH_FLAGS launchFlags, uint standardInput, uint standardOutput,
            uint standardError, IDebugEventCallback2 callback, out IDebugProcess2 process)
        {
            _waiter = new AutoResetEvent(false);


            var outputWindow = (IVsOutputWindow) Package.GetGlobalService(typeof(SVsOutputWindow));
            var generalPaneGuid = VSConstants.GUID_OutWindowDebugPane;
            outputWindow.GetPane(ref generalPaneGuid, out _outputWindow);

            var serverDir = directory;
            string serverPath = null;
            while (!string.IsNullOrWhiteSpace(serverDir))
            {
                serverPath = Path.Combine(serverDir, "samp-server.exe");

                if (File.Exists(serverPath))
                    break;
                serverPath = null;
                serverDir = Directory.GetParent(serverDir)?.FullName;
            }

            if (serverPath == null)
            {
                // TODO: Error not appearing...
                _outputWindow.Log(VsLogSeverity.Error, "", "",
                    "Could not locate samp-server.exe. Are you sure you are building into the gamemode directory?");

                _processId = new AD_PROCESS_ID
                {
                    ProcessIdType = (uint) enum_AD_PROCESS_ID.AD_PROCESS_ID_GUID,
                    guidProcessId = Guid.NewGuid()
                };

                EngineUtils.CheckOk(port.GetProcess(_processId, out process));
                Callback = callback;

                return VSConstants.S_FALSE;
            }

            Task.Run(() =>
            {
                var proc = Process.Start(new ProcessStartInfo
                {
                    FileName = serverPath,
                    WorkingDirectory = serverDir,
                    UseShellExecute = false
                });

                Debug.WriteLine(proc);
                // Trigger that the app is now running for whomever might be waiting for that signal
                _waiter.Set();
            });

            Session = new SoftDebuggerSession();
            Session.TargetReady += (sender, eventArgs) =>
            {
                Log($"TargetReady {eventArgs}");
                var activeThread = Session.ActiveThread;
                ThreadManager.Add(activeThread, new MonoThread(this, activeThread));
            };
            Session.ExceptionHandler = exception => true;
            Session.TargetExited += (sender, x) =>
            {
                Log($"TargetExited {x}");
                Send(new SampSharpDestroyEvent((uint?) x.ExitCode ?? 0), SampSharpDestroyEvent.Iid, null);
            };
            Session.TargetUnhandledException += (sender, x) => Log("TargetUnhandledException" + x.Backtrace.ToString());
            Session.LogWriter = (stderr, text) => Log($"LogWriter {text}");
            Session.OutputWriter = (stderr, text) => Log($"OutputWriter {text}");
            Session.TargetThreadStarted += (sender, x) =>
            {
                Log($"TargetThreadStarted {x}");
                ThreadManager.Add(x.Thread, new MonoThread(this, x.Thread));
            };
            Session.TargetThreadStopped += (sender, x) =>
            {
                Log($"TargetThreadStopped {x}");
                ThreadManager.Remove(x.Thread);
            };
            Session.TargetStopped += (sender, x) => Log($"TargetStopped {x}");
            Session.TargetStarted += (sender, x) => Log($"TargetStarted {x}");
            Session.TargetSignaled += (sender, x) => Log($"TargetSignaled {x}");
            Session.TargetInterrupted += (sender, x) => Log($"TargetInterrupted {x}");
            Session.TargetExceptionThrown += (sender, x) =>
            {
                Log($"TargetStopped {x}");
                Send(new MonoBreakpointEvent(new MonoBoundBreakpointsEnumerator(new IDebugBoundBreakpoint2[0])),
                    MonoStepCompleteEvent.Iid,
                    ThreadManager[x.Thread]);
            };
            Session.TargetHitBreakpoint += (sender, x) =>
            {
                Log($"TargetStopped {x}");

                var breakpoint = x.BreakEvent as Breakpoint;
                var pendingBreakpoint = _breakpointManager[breakpoint];
                if (pendingBreakpoint != null)
                    Send(
                        new MonoBreakpointEvent(new MonoBoundBreakpointsEnumerator(pendingBreakpoint.BoundBreakpoints)),
                        MonoBreakpointEvent.Iid, ThreadManager[x.Thread]);
            };

            _processId = new AD_PROCESS_ID
            {
                ProcessIdType = (uint) enum_AD_PROCESS_ID.AD_PROCESS_ID_GUID,
                guidProcessId = Guid.NewGuid()
            };

            EngineUtils.CheckOk(port.GetProcess(_processId, out process));
            Callback = callback;

            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Resume a process launched by IDebugEngineLaunch2.LaunchSuspended
        /// </summary>
        /// <param name="process"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int ResumeProcess(IDebugProcess2 process)
        {
            IDebugPort2 port;
            EngineUtils.RequireOk(process.GetPort(out port));

            var defaultPort = (IDebugDefaultPort2) port;
            IDebugPortNotify2 portNotify;
            EngineUtils.RequireOk(defaultPort.GetPortNotify(out portNotify));

            EngineUtils.RequireOk(portNotify.AddProgramNode(new MonoProgramNode(_processId)));

            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Determines if a process can be terminated.
        /// </summary>
        /// <param name="pProcess"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int CanTerminateProcess(IDebugProcess2 pProcess)
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     This function is used to terminate a process that the SampleEngine launched
        ///     The debugger will call IDebugEngineLaunch2::CanTerminateProcess before calling this method
        /// </summary>
        /// <param name="pProcess"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int TerminateProcess(IDebugProcess2 pProcess)
        {
            Log("TerminateProcess");

            pProcess.Terminate();
            Send(new SampSharpDestroyEvent(0), SampSharpDestroyEvent.Iid, null);
            return VSConstants.S_OK;
        }

        #endregion

        #region Deprecated interface methods

        public int EnumPrograms(out IEnumDebugPrograms2 programs)
        {
            programs = null;

            Debug.Fail("This function is not called by the debugger");

            return VSConstants.E_NOTIMPL;
        }

        public int Attach(IDebugEventCallback2 pCallback)
        {
            Debug.Fail("This function is not called by the debugger");

            return VSConstants.E_NOTIMPL;
        }

        public int GetProcess(out IDebugProcess2 process)
        {
            process = null;

            Debug.Fail("This function is not called by the debugger");

            return VSConstants.E_NOTIMPL;
        }

        public int Execute()
        {
            Debug.Fail("This function is not called by the debugger");

            return VSConstants.E_NOTIMPL;
        }

        #endregion
    }
}