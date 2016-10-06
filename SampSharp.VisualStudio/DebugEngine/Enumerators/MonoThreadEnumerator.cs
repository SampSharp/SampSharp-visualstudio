using Microsoft.VisualStudio.Debugger.Interop;

namespace SampSharp.VisualStudio.DebugEngine.Enumerators
{
    public class MonoThreadEnumerator : Enumerator<IDebugThread2, IEnumDebugThreads2>, IEnumDebugThreads2
    {
        public MonoThreadEnumerator(IDebugThread2[] threads) : base(threads)
        {
        }

        public int Next(uint celt, IDebugThread2[] rgelt, out uint celtFetched)
        {
            return NextOut(celt, rgelt, out celtFetched);
        }
    }
}