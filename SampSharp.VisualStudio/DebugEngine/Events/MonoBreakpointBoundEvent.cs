using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using SampSharp.VisualStudio.DebugEngine.Enumerators;

namespace SampSharp.VisualStudio.DebugEngine.Events
{
    public class MonoBreakpointBoundEvent : AsynchronousEvent, IDebugBreakpointBoundEvent2
    {
        public const string Iid = "1dddb704-cf99-4b8a-b746-dabb01dd13a0";

        private readonly MonoBoundBreakpoint _boundBreakpoint;
        private readonly MonoPendingBreakpoint _pendingBreakpoint;

        public MonoBreakpointBoundEvent(MonoPendingBreakpoint pendingBreakpoint, MonoBoundBreakpoint boundBreakpoint)
        {
            _pendingBreakpoint = pendingBreakpoint;
            _boundBreakpoint = boundBreakpoint;
        }

        #region Implementation of IDebugBreakpointBoundEvent2

        /// <summary>
        ///     Creates an enumerator of breakpoints that were bound on this event.
        /// </summary>
        /// <param name="ppEnum">
        ///     Returns an IEnumDebugBoundBreakpoints2 object that enumerates all the breakpoints bound from this
        ///     event.
        /// </param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int EnumBoundBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum)
        {
            var boundBreakpoints = new IDebugBoundBreakpoint2[1];
            boundBreakpoints[0] = _boundBreakpoint;
            ppEnum = new MonoBoundBreakpointsEnumerator(boundBreakpoints);
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Gets the pending breakpoint that is being bound.
        /// </summary>
        /// <param name="ppPendingBp">
        ///     Returns the IDebugPendingBreakpoint2 object that represents the pending breakpoint being
        ///     bound.
        /// </param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetPendingBreakpoint(out IDebugPendingBreakpoint2 ppPendingBp)
        {
            ppPendingBp = _pendingBreakpoint;
            return VSConstants.S_OK;
        }

        #endregion
    }
}