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

        #region Implementation of IDebugBreakpointEvent2

        /// <summary>
        ///     Creates an enumerator for all the breakpoints that fired at the current code location.
        /// </summary>
        /// <param name="ppEnum"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int EnumBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum)
        {
            ppEnum = _boundBreakpoints;
            return VSConstants.S_OK;
        }

        #endregion
    }
}