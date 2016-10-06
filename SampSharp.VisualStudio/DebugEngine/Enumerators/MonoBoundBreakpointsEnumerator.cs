using Microsoft.VisualStudio.Debugger.Interop;

namespace SampSharp.VisualStudio.DebugEngine.Enumerators
{
    public class MonoBoundBreakpointsEnumerator : Enumerator<IDebugBoundBreakpoint2, IEnumDebugBoundBreakpoints2>,
        IEnumDebugBoundBreakpoints2
    {
        public MonoBoundBreakpointsEnumerator(IDebugBoundBreakpoint2[] breakpoints) : base(breakpoints)
        {
        }
    }
}