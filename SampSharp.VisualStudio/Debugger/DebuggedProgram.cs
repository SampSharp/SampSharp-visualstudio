using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Mono.Debugging.Client;
using Mono.Debugging.Soft;
using SampSharp.VisualStudio.DebugEngine;
using SampSharp.VisualStudio.DebugEngine.Enumerators;
using SampSharp.VisualStudio.DebugEngine.Events;
using SampSharp.VisualStudio.Utils;
using Task = System.Threading.Tasks.Task;

namespace SampSharp.VisualStudio.Debugger
{
    public class DebuggedProgram : IDisposable
    {
        private readonly MonoEngine _engine;
        private readonly MonoThreadManager _threadManager;
        private readonly MonoBreakpointManager _breakpointManager;

        public DebuggedProgram(MonoEngine engine, MonoBreakpointManager breakpointManager, MonoThreadManager threadManager)
        {
            if (engine == null) throw new ArgumentNullException(nameof(engine));
            if (breakpointManager == null) throw new ArgumentNullException(nameof(breakpointManager));
            if (threadManager == null) throw new ArgumentNullException(nameof(threadManager));

            _engine = engine;
            _breakpointManager = breakpointManager;
            _threadManager = threadManager;
        }

        public Guid Id => _programId;
        public SoftDebuggerSession Session { get; private set; }
        public IDebugModule2 Module => new MonoModule(this);
        public IDebugProgramNode2 Node => new MonoProgramNode(_processId);

        public static bool IsPortAvailable(int port)
        {
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            return tcpConnInfoArray.All(tcpi => tcpi.LocalEndPoint.Port != port);
        }

        public IDebugPendingBreakpoint2 CreatePendingBreakpoint(IDebugBreakpointRequest2 request)
        {
            return new MonoPendingBreakpoint(_breakpointManager, request);
        }

        public bool SetException(EXCEPTION_INFO exception)
        {
            if (exception.dwState.HasFlag(enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE))
            {
                var catchpoint = Session.Breakpoints.AddCatchpoint(exception.bstrExceptionName);
                _breakpointManager.Add(catchpoint);
                return true;
            }

            return RemoveSetException(exception);
        }

        public bool RemoveSetException(EXCEPTION_INFO exception)
        {
            if (_breakpointManager.ContainsCatchpoint(exception.bstrExceptionName))
            {
                _breakpointManager.Remove(_breakpointManager[exception.bstrExceptionName]);
                Session.Breakpoints.RemoveCatchpoint(exception.bstrExceptionName);
            }

            return true;
        }

        public bool RemoveAllSetExceptions()
        {
            foreach (var catchpoint in _breakpointManager.Catchpoints)
                Session.Breakpoints.RemoveCatchpoint(catchpoint.ExceptionName);
            return true;
        }

        public void Dispose()
        {
            // todo properly implement
            Session.Dispose();
        }

        public bool Break()
        {
            EventHandler<TargetEventArgs> stepFinished = null;
            stepFinished = (sender, args) =>
            {
                Session.TargetStopped -= stepFinished;

                var thread = _threadManager[args.Thread] ?? _threadManager.All.First();

                _engine.Send(new MonoBreakpointEvent(
                        new MonoBoundBreakpointsEnumerator(new IDebugBoundBreakpoint2[0])),
                    MonoStepCompleteEvent.Iid, thread);
            };
            Session.TargetStopped += stepFinished;

            Session.Stop();

            return true;
        }

        private Guid _programId;
        private AutoResetEvent _waiter;
        private AD_PROCESS_ID _processId;

        public bool Attach(IDebugProgram2 program)
        {
            IDebugProcess2 process;
            program.GetProcess(out process);
            Guid processId;
            process.GetProcessId(out processId);
            if (processId != _processId.guidProcessId)
                return false;

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

            MonoEngineCreateEvent.Send(_engine);
            SampSharpCreateEvent.Send(_engine);

            return true;

        }

        public bool Detach()
        {
            if (!Session.IsRunning)
                Session.Continue();

            Session.Dispose();

            return true;
        }

        public bool Step(enum_STEPKIND kind, enum_STEPUNIT unit)
        {
            if (kind == enum_STEPKIND.STEP_BACKWARDS)
            {
                return false;
            }

            EventHandler<TargetEventArgs> stepFinished = null;
            stepFinished = (sender, args) =>
            {
                Session.TargetStopped -= stepFinished;
                _engine.Send(new MonoStepCompleteEvent(), MonoStepCompleteEvent.Iid, _threadManager[args.Thread]);
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
            return true;
        }

        public bool Continue()
        {
            Session.Continue();
            return true;
        }

        public bool ExecuteOnThread(IDebugThread2 thread)
        {
            var threadInfo = ((MonoThread)thread).GetDebuggedThread();

            if (Session.ActiveThread?.Id != threadInfo.Id)
                threadInfo.SetActive();

            Session.Continue();

            return true;
        }

        public bool LaunchSuspended(IDebugPort2 port, string args, string directory, IDebugEventCallback2 callback, out IDebugProcess2 process)
        {
            _waiter = new AutoResetEvent(false);
            
            var serverDir = directory;
            string serverPath = null;
            while (!string.IsNullOrWhiteSpace(serverDir)) {
                serverPath = Path.Combine(serverDir, "samp-server.exe");

                if (File.Exists(serverPath))
                    break;
                serverPath = null;
                serverDir = Directory.GetParent(serverDir)?.FullName;
            }

            if (serverPath == null) {
                // TODO: Error not appearing...
                _engine.Log(VsLogSeverity.Error, "", "", "Could not locate samp-server.exe. Are you sure you are building into the gamemode directory?");

                _processId = new AD_PROCESS_ID {
                    ProcessIdType = (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_GUID,
                    guidProcessId = Guid.NewGuid()
                };

                EngineUtils.CheckOk(port.GetProcess(_processId, out process));
     
                return false;
            }

            Task.Run(() => {
                var proc = Process.Start(new ProcessStartInfo {
                    FileName = serverPath,
                    WorkingDirectory = serverDir,
                    UseShellExecute = false
                });

                Debug.WriteLine(proc);
                // Trigger that the app is now running for whomever might be waiting for that signal
                _waiter.Set();
            });

            Session = new SoftDebuggerSession();
            Session.TargetReady += (sender, eventArgs) => {
                var activeThread = Session.ActiveThread;
                _threadManager.Add(activeThread, new MonoThread(_engine, activeThread));
            };
            Session.ExceptionHandler = exception => true;
            Session.TargetExited += (sender, x) => {
                _engine.Send(new SampSharpDestroyEvent((uint?)x.ExitCode ?? 0), SampSharpDestroyEvent.Iid, null);
            };
            Session.TargetUnhandledException += (sender, x) => { }; // todo
            Session.LogWriter = (stderr, text) => { };
            Session.OutputWriter = (stderr, text) => { };
            Session.TargetThreadStarted += (sender, x) => {
                _threadManager.Add(x.Thread, new MonoThread(_engine, x.Thread));
            };
            Session.TargetThreadStopped += (sender, x) => {
                _threadManager.Remove(x.Thread);
            };
            Session.TargetStopped += (sender, x) => { };
            Session.TargetStarted += (sender, x) => { };
            Session.TargetSignaled += (sender, x) => { };
            Session.TargetInterrupted += (sender, x) => { };
            Session.TargetExceptionThrown += (sender, x) => {
                _engine.Send(new MonoBreakpointEvent(new MonoBoundBreakpointsEnumerator(new IDebugBoundBreakpoint2[0])),
                    MonoStepCompleteEvent.Iid,
                    _threadManager[x.Thread]);
            };
            Session.TargetHitBreakpoint += (sender, x) => {
                var breakpoint = x.BreakEvent as Breakpoint;
                var pendingBreakpoint = _breakpointManager[breakpoint];
                if (pendingBreakpoint != null)
                    _engine.Send(
                        new MonoBreakpointEvent(new MonoBoundBreakpointsEnumerator(pendingBreakpoint.BoundBreakpoints)),
                        MonoBreakpointEvent.Iid, _threadManager[x.Thread]);
            };

            _processId = new AD_PROCESS_ID {
                ProcessIdType = (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_GUID,
                guidProcessId = Guid.NewGuid()
            };

            EngineUtils.CheckOk(port.GetProcess(_processId, out process));
       
            return true;
        }
    }
}