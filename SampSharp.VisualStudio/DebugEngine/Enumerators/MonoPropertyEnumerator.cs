using Microsoft.VisualStudio.Debugger.Interop;

namespace SampSharp.VisualStudio.DebugEngine.Enumerators
{
    public class MonoPropertyEnumerator : Enumerator<DEBUG_PROPERTY_INFO, IEnumDebugPropertyInfo2>,
        IEnumDebugPropertyInfo2
    {
        public MonoPropertyEnumerator(DEBUG_PROPERTY_INFO[] properties) : base(properties)
        {
        }

        public int Next(uint celt, DEBUG_PROPERTY_INFO[] rgelt, out uint celtFetched)
        {
            return NextOut(celt, rgelt, out celtFetched);
        }
    }
}