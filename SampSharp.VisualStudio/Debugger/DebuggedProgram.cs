using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Mono.Debugging.Client;
using Mono.Debugging.Soft;
using SampSharp.VisualStudio.DebugEngine;
using SampSharp.VisualStudio.DebugEngine.Enumerators;
using SampSharp.VisualStudio.DebugEngine.Events;
using SampSharp.VisualStudio.ProgramProperties;
using SampSharp.VisualStudio.Projects;
using SampSharp.VisualStudio.Utils;
using Task = System.Threading.Tasks.Task;

namespace SampSharp.VisualStudio.Debugger
{
    public class DebuggedProgram : IDisposable
    {
        private readonly MonoBreakpointManager _breakpointManager;
        private readonly MonoEngine _engine;
        private readonly MonoThreadManager _threadManager;
        private AD_PROCESS_ID _processId;
        private Guid _programId;
        private AutoResetEvent _waiter;
        private IVsOutputWindowPane _serverPane;

        public DebuggedProgram(MonoEngine engine, MonoBreakpointManager breakpointManager,
            MonoThreadManager threadManager)
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

        public string ServerPath { get; private set; }

        private DebuggerAddress DebuggerAddress { get; set; }

        public void Dispose()
        {
            // todo properly implement
            Session.Dispose();
        }

        public IDebugPendingBreakpoint2 CreatePendingBreakpoint(IDebugBreakpointRequest2 request)
        {
            return new MonoPendingBreakpoint(_breakpointManager, request);
        }

        public void SetException(EXCEPTION_INFO exception)
        {
            if (exception.dwState.HasFlag(enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE))
            {
                var catchpoint = Session.Breakpoints.AddCatchpoint(exception.bstrExceptionName);
                _breakpointManager.Add(catchpoint);
            }
            else
             RemoveSetException(exception);
        }

        public void RemoveSetException(EXCEPTION_INFO exception)
        {
            if (_breakpointManager.ContainsCatchpoint(exception.bstrExceptionName))
            {
                _breakpointManager.Remove(_breakpointManager[exception.bstrExceptionName]);
                Session.Breakpoints.RemoveCatchpoint(exception.bstrExceptionName);
            }
        }

        public void RemoveAllSetExceptions()
        {
            foreach (var catchpoint in _breakpointManager.Catchpoints)
                Session.Breakpoints.RemoveCatchpoint(catchpoint.ExceptionName);
        }

        public void Break()
        {
            EventHandler<TargetEventArgs> stepFinished = null;
            stepFinished = (sender, args) =>
            {
                Session.TargetStopped -= stepFinished;

                var thread = _threadManager[args.Thread] ?? _threadManager.All.First();

                _engine.Callback.Send(new MonoBreakpointEvent(
                        new MonoBoundBreakpointsEnumerator(new IDebugBoundBreakpoint2[0])),
                    MonoStepCompleteEvent.Iid, thread);
            };
            Session.TargetStopped += stepFinished;

            Session.Stop();
        }

        private void Log(string message)
        {
            _serverPane?.Log(message);
        }

        public void LaunchSuspended(IDebugPort2 port, string gameMode, bool noWindow, string exe, string directory, out IDebugProcess2 process)
        {
            _waiter = new AutoResetEvent(false);
            
            CreateProcessId();
            process = GetProcess(port);

            ComputeAvailableDebuggerAddress();
            ComputeServerDirectory(directory);
            

            // Run the server.
            Task.Run(() =>
            {
                if (noWindow)
                {
                    IVsOutputWindow outWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;

                    Guid wndGuid = Guids.ServerWindow;
                    outWindow.CreatePane(ref wndGuid, "SA-MP Server", 1, 1);

                    outWindow.GetPane(ref wndGuid, out _serverPane);
                    _serverPane.Clear();
                    Log("Starting SA-MP server...");
                    _serverPane.Activate();


                    var startInfo = new ProcessStartInfo
                    {
                        FileName = Path.Combine(ServerPath, "samp-server.exe"),
                        WorkingDirectory = ServerPath,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        EnvironmentVariables =
                        {
                            ["debugger_address"] = DebuggerAddress.ToString(),
                            ["gamemode"] = $"{Path.GetFileNameWithoutExtension(exe)}:{gameMode}"
                        }
                    };

                    var server = new Process { StartInfo = startInfo };
                    server.OutputDataReceived += (sender, args) =>
                    {
                        if (args.Data != null)
                            Log($"{args.Data.Replace("\r", "")}");
                    };
                    server.ErrorDataReceived += (sender, args) =>
                    {
                        if (!string.IsNullOrWhiteSpace(args.Data))
                            Log($"ERROR: {args.Data}");
                    };
                    server.Start();
                    server.BeginOutputReadLine();
                    server.BeginErrorReadLine();
                }
                else
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = Path.Combine(ServerPath, "samp-server.exe"),
                        WorkingDirectory = ServerPath,
                        UseShellExecute = false,
                        EnvironmentVariables =
                        {
                            ["debugger_address"] = DebuggerAddress.ToString(),
                            ["gamemode"] = $"{Path.GetFileNameWithoutExtension(exe)}:{gameMode}"
                        }
                    });
                }
                // Trigger that the app is now running for whomever might be waiting for that signal
                _waiter.Set();
            });

            CreateDebuggerSession();
        }

        public void Attach(IDebugProgram2 program)
        {
            IDebugProcess2 process;
            program.GetProcess(out process);
            Guid processId;
            process.GetProcessId(out processId);


            if (processId != _processId.guidProcessId)
                throw new DebuggerInitializeException("Cannot attach to specified program.");

            EngineUtils.RequireOk(program.GetProgramId(out _programId));

            Task.Run(() =>
            {
                _waiter.WaitOne();

                var ipAddress = new IPAddress(new byte[] { 0x7f, 0x00, 0x00, 0x01 });
                Session.Run(new SoftDebuggerStartInfo(new SoftDebuggerConnectArgs("samp-server", ipAddress, DebuggerAddress.Port)),
                    new DebuggerSessionOptions
                    {
                        EvaluationOptions = EvaluationOptions.DefaultOptions,
                        ProjectAssembliesOnly = false
                    });
            });

            MonoEngineCreateEvent.Send(_engine);
            SampSharpCreateEvent.Send(_engine);
        }

        public void Detach()
        {
            if (!Session.IsRunning)
                Session.Continue();

            Session.Dispose();
        }

        public bool Step(enum_STEPKIND kind, enum_STEPUNIT unit)
        {
            if (kind == enum_STEPKIND.STEP_BACKWARDS)
                return false;

            EventHandler<TargetEventArgs> stepFinished = null;
            stepFinished = (sender, args) =>
            {
                Session.TargetStopped -= stepFinished;
                _engine.Callback.Send(new MonoStepCompleteEvent(), MonoStepCompleteEvent.Iid, _threadManager[args.Thread]);
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

        public void Continue()
        {
            Session.Continue();
        }

        public void ExecuteOnThread(IDebugThread2 thread)
        {
            var threadInfo = ((MonoThread) thread).GetDebuggedThread();

            if (Session.ActiveThread?.Id != threadInfo.Id)
                threadInfo.SetActive();

            Session.Continue();
        }

        private void CreateProcessId()
        {
            _processId = new AD_PROCESS_ID
            {
                ProcessIdType = (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_GUID,
                guidProcessId = Guid.NewGuid()
            };
        }

        private IDebugProcess2 GetProcess(IDebugPort2 port)
        {
            IDebugProcess2 process;
            EngineUtils.CheckOk(port.GetProcess(_processId, out process));

            return process;
        }

        private void ComputeServerDirectory(string path)
        {
            while (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
            {
                var executablePath = Path.Combine(path, "samp-server.exe");

                if (File.Exists(executablePath))
                    break;

                path = Directory.GetParent(path)?.FullName;
            }

            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                throw new DebuggerInitializeException("Could not locate samp-server.exe. Are you sure you are building into the gamemode directory?");
            }

            ServerPath = path;
        }

        private void ComputeAvailableDebuggerAddress()
        {
            var address = DebuggerAddress.GetAvailable();

            if (address == null)
                throw new DebuggerInitializeException("Debugger port is unavailable.");

            DebuggerAddress = address;
        }

        private void CreateDebuggerSession()
        {
            Session = new SoftDebuggerSession();
            Session.TargetReady += (sender, eventArgs) =>
            {
                var activeThread = Session.ActiveThread;
                _threadManager.Add(activeThread, new MonoThread(_engine, activeThread));
            };

            Session.ExceptionHandler = e =>
            {
                Log($"DEBUGGER ERROR: {e}");
                return true;
            };
            Session.TargetThreadStarted +=
                (sender, x) => { _threadManager.Add(x.Thread, new MonoThread(_engine, x.Thread)); };
            Session.TargetThreadStopped += (sender, x) => { _threadManager.Remove(x.Thread); };
            Session.TargetExited += (sender, x) => _engine.Callback.Send(new SampSharpDestroyEvent((uint?)x.ExitCode ?? 0), SampSharpDestroyEvent.Iid, null);
            Session.TargetExceptionThrown += (sender, args) =>
            {
                _engine.Callback.Send(
                    new MonoBreakpointEvent(new MonoBoundBreakpointsEnumerator(new IDebugBoundBreakpoint2[0])),
                    MonoStepCompleteEvent.Iid, _threadManager[args.Thread]);
            };
            Session.TargetHitBreakpoint += (sender, x) =>
            {
                var breakpoint = x.BreakEvent as Breakpoint;
                var pendingBreakpoint = _breakpointManager[breakpoint];
                if (pendingBreakpoint != null)
                    _engine.Callback.Send(
                        new MonoBreakpointEvent(new MonoBoundBreakpointsEnumerator(pendingBreakpoint.BoundBreakpoints)),
                        MonoBreakpointEvent.Iid, _threadManager[x.Thread]);
            };
        }

    }
}