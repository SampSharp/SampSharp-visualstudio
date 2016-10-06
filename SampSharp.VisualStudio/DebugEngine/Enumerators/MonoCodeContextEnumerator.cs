using Microsoft.VisualStudio.Debugger.Interop;

namespace SampSharp.VisualStudio.DebugEngine.Enumerators
{
    public class MonoCodeContextEnumerator : Enumerator<IDebugCodeContext2, IEnumDebugCodeContexts2>,
        IEnumDebugCodeContexts2
    {
        public MonoCodeContextEnumerator(IDebugCodeContext2[] codeContexts) : base(codeContexts)
        {
        }
    }
}