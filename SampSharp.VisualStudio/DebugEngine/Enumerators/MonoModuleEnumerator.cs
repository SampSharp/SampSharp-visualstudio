using Microsoft.VisualStudio.Debugger.Interop;

namespace SampSharp.VisualStudio.DebugEngine.Enumerators
{
    public class MonoModuleEnumerator : Enumerator<IDebugModule2, IEnumDebugModules2>, IEnumDebugModules2
    {
        public MonoModuleEnumerator(IDebugModule2[] modules) : base(modules)
        {
        }
    }
}