using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace SampSharp.VisualStudio.Debuggers.Events
{
	public class MonoBreakpointEvent : MonoStoppingEvent, IDebugBreakpointEvent2
	{
		public const string Iid = "501C1E21-C557-48B8-BA30-A1EAB0BC4A74";

		private readonly IEnumDebugBoundBreakpoints2 _boundBreakpoints;

		public MonoBreakpointEvent(IEnumDebugBoundBreakpoints2 boundBreakpoints)
		{
			_boundBreakpoints = boundBreakpoints;
		}

		int IDebugBreakpointEvent2.EnumBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum)
		{
			ppEnum = _boundBreakpoints;
			return VSConstants.S_OK;
		}
	}
}