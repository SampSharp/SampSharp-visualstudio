using Microsoft.VisualStudio.Debugger.Interop;

namespace SampSharp.VisualStudio.DebugEngine.Enumerators
{
    public class MonoPropertyInfoEnumerator : Enumerator<DEBUG_PROPERTY_INFO, IEnumDebugPropertyInfo2>,
        IEnumDebugPropertyInfo2
    {
        public MonoPropertyInfoEnumerator(DEBUG_PROPERTY_INFO[] data) : base(data)
        {
        }

        public int Next(uint celt, DEBUG_PROPERTY_INFO[] rgelt, out uint celtFetched)
        {
            return NextOut(celt, rgelt, out celtFetched);
        }
    }
}