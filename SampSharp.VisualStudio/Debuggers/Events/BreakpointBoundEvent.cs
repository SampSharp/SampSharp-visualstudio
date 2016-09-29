using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace SampSharp.VisualStudio.Debuggers.Events
{
	public class BreakpointBoundEvent : AsynchronousEvent, IDebugBreakpointBoundEvent2
	{
		public const string Iid = "1dddb704-cf99-4b8a-b746-dabb01dd13a0";
		private readonly MonoBoundBreakpoint _boundBreakpoint;

		private readonly MonoPendingBreakpoint _pendingBreakpoint;

		public BreakpointBoundEvent(MonoPendingBreakpoint pendingBreakpoint, MonoBoundBreakpoint boundBreakpoint)
		{
			_pendingBreakpoint = pendingBreakpoint;
			_boundBreakpoint = boundBreakpoint;
		}

		int IDebugBreakpointBoundEvent2.EnumBoundBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum)
		{
			var boundBreakpoints = new IDebugBoundBreakpoint2[1];
			boundBreakpoints[0] = _boundBreakpoint;
			ppEnum = new MonoBoundBreakpointsEnum(boundBreakpoints);
			return VSConstants.S_OK;
		}

		int IDebugBreakpointBoundEvent2.GetPendingBreakpoint(out IDebugPendingBreakpoint2 ppPendingBp)
		{
			ppPendingBp = _pendingBreakpoint;
			return VSConstants.S_OK;
		}
	}
}