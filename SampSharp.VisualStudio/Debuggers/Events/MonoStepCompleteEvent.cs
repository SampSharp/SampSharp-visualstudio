using Microsoft.VisualStudio.Debugger.Interop;

namespace SampSharp.VisualStudio.Debuggers.Events
{
    public class MonoStepCompleteEvent : MonoStoppingEvent, IDebugStepCompleteEvent2
    {
        public const string Iid = "0f7f24c1-74d9-4ea6-a3ea-7edb2d81441d";
    }
}