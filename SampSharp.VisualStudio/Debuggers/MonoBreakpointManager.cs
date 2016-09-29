using System.Collections.Generic;
using Mono.Debugging.Client;

namespace SampSharp.VisualStudio.Debuggers
{
	public class MonoBreakpointManager
	{
		private readonly Dictionary<BreakEvent, MonoPendingBreakpoint> _breakpoints =
			new Dictionary<BreakEvent, MonoPendingBreakpoint>();

		private readonly Dictionary<string, Catchpoint> _catchpoints = new Dictionary<string, Catchpoint>();

		public MonoBreakpointManager(MonoEngine engine)
		{
			Engine = engine;
		}

		public MonoEngine Engine { get; }
		public MonoPendingBreakpoint this[BreakEvent breakEvent]
		{
		    get
		    {
		        MonoPendingBreakpoint breakpoint;
                _breakpoints.TryGetValue(breakEvent, out breakpoint);
		        return breakpoint;
		    }
		}

	    public Catchpoint this[string exceptionName] => _catchpoints[exceptionName];
		public IEnumerable<Catchpoint> Catchpoints => _catchpoints.Values;
		public bool ContainsCatchpoint(string exceptionName) => _catchpoints.ContainsKey(exceptionName);

		public void Add(BreakEvent breakEvent, MonoPendingBreakpoint pendingBreakpoint)
		{
			_breakpoints[breakEvent] = pendingBreakpoint;
		}

		public void Remove(BreakEvent breakEvent)
		{
			Engine.Session.Breakpoints.Remove(breakEvent);
			_breakpoints.Remove(breakEvent);
		}

		public void Add(Catchpoint catchpoint)
		{
			_catchpoints[catchpoint.ExceptionName] = catchpoint;
		}

		public void Remove(Catchpoint catchpoint)
		{
			_catchpoints.Remove(catchpoint.ExceptionName);
		}
	}
}