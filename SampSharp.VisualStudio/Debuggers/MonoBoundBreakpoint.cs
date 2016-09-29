using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace SampSharp.VisualStudio.Debuggers
{
	public class MonoBoundBreakpoint : IDebugBoundBreakpoint2
	{
		private readonly MonoBreakpointResolution _breakpointResolution;
		private readonly MonoPendingBreakpoint _pendingBreakpoint;

		public MonoBoundBreakpoint(MonoPendingBreakpoint pendingBreakpoint, MonoBreakpointResolution breakpointResolution)
		{
			_pendingBreakpoint = pendingBreakpoint;
			_breakpointResolution = breakpointResolution;
		}

		public int GetPendingBreakpoint(out IDebugPendingBreakpoint2 pendingBreakpoint)
		{
			pendingBreakpoint = _pendingBreakpoint;
			return VSConstants.S_OK;
		}

		public int GetState(enum_BP_STATE[] state)
		{
			state[0] = enum_BP_STATE.BPS_ENABLED;
			return VSConstants.S_OK;
		}

		public int GetHitCount(out uint hitCount)
		{
			hitCount = 0;
			return VSConstants.S_OK;
		}

		public int GetBreakpointResolution(out IDebugBreakpointResolution2 breakpointResolution)
		{
			breakpointResolution = _breakpointResolution;
			return VSConstants.S_OK;
		}

		public int Enable(int enable)
		{
			return VSConstants.S_OK;
		}

		public int SetHitCount(uint hitCount)
		{
			return VSConstants.S_OK;
		}

		public int SetCondition(BP_CONDITION bpCondition)
		{
			return VSConstants.S_OK;
		}

		public int SetPassCount(BP_PASSCOUNT bpPassCount)
		{
			return VSConstants.S_OK;
		}

		public int Delete()
		{
			return VSConstants.S_OK;
		}
	}
}