using Microsoft.VisualStudio.Debugger.Interop;

namespace SampSharp.VisualStudio.Debuggers.Events
{
    public class SampSharpCreateEvent : AsynchronousEvent, IDebugProgramCreateEvent2
    {
        public const string Iid = "96CD11EE-ECD4-4E89-957E-B5D496FC4139";

        internal static void Send(MonoEngine engine)
        {
            var eventObject = new SampSharpCreateEvent();
            engine.Send(eventObject, Iid, null);
        }
    }
}