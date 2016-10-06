using Microsoft.VisualStudio.Debugger.Interop;
using static Microsoft.VisualStudio.VSConstants;

namespace SampSharp.VisualStudio.DebugEngine
{
    public class MonoBoundBreakpoint : IDebugBoundBreakpoint2
    {
        private readonly MonoBreakpointResolution _breakpointResolution;
        private readonly MonoPendingBreakpoint _pendingBreakpoint;

        public MonoBoundBreakpoint(MonoPendingBreakpoint pendingBreakpoint,
            MonoBreakpointResolution breakpointResolution)
        {
            _pendingBreakpoint = pendingBreakpoint;
            _breakpointResolution = breakpointResolution;
        }

        #region Implementation of IDebugBoundBreakpoint2

        /// <summary>
        ///     Gets the pending breakpoint from which the specified bound breakpoint was created.
        /// </summary>
        /// <param name="pendingBreakpoint">The pending breakpoint.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetPendingBreakpoint(out IDebugPendingBreakpoint2 pendingBreakpoint)
        {
            pendingBreakpoint = _pendingBreakpoint;
            return S_OK;
        }

        /// <summary>
        ///     Gets the state of this bound breakpoint.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetState(enum_BP_STATE[] state)
        {
            state[0] = enum_BP_STATE.BPS_ENABLED;
            return S_OK;
        }

        /// <summary>
        ///     Gets the current hit count for this bound breakpoint.
        /// </summary>
        /// <param name="hitCount">The hit count.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetHitCount(out uint hitCount)
        {
            hitCount = 0;
            return S_OK;
        }

        /// <summary>
        ///     Gets the breakpoint resolution that describes this breakpoint.
        /// </summary>
        /// <param name="breakpointResolution">The breakpoint resolution.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetBreakpointResolution(out IDebugBreakpointResolution2 breakpointResolution)
        {
            breakpointResolution = _breakpointResolution;
            return S_OK;
        }

        /// <summary>
        ///     Enables or disables the breakpoint.
        /// </summary>
        /// <param name="enable"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int Enable(int enable)
        {
            return S_OK;
        }

        /// <summary>
        ///     Sets the hit count for this bound breakpoint.
        /// </summary>
        /// <param name="hitCount">The hit count.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int SetHitCount(uint hitCount)
        {
            return S_OK;
        }

        /// <summary>
        ///     Sets or changes the condition associated with this bound breakpoint.
        /// </summary>
        /// <param name="bpCondition">The breakpoint condition.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int SetCondition(BP_CONDITION bpCondition)
        {
            return S_OK;
        }

        /// <summary>
        ///     Sets or change the pass count associated with this bound breakpoint.
        /// </summary>
        /// <param name="bpPassCount">The bp pass count.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int SetPassCount(BP_PASSCOUNT bpPassCount)
        {
            return S_OK;
        }

        /// <summary>
        ///     Deletes the breakpoint.
        /// </summary>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int Delete()
        {
            return S_OK;
        }

        #endregion
    }
}