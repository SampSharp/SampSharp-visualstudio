using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace SampSharp.VisualStudio.Debuggers.Events
{
    public class MonoEngineCreateEvent : AsynchronousEvent, IDebugEngineCreateEvent2
    {
        public const string Iid = "FE5B734C-759D-4E59-AB04-F103343BDD06";

        private readonly IDebugEngine2 _engine;

        public MonoEngineCreateEvent(MonoEngine engine)
        {
            _engine = engine;
        }

        #region Implementation of IDebugEngineCreateEvent2

        /// <summary>
        ///     Retrieves the object that represents the newly created debug engine (DE).
        /// </summary>
        /// <param name="engine"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetEngine(out IDebugEngine2 engine)
        {
            engine = _engine;

            return VSConstants.S_OK;
        }

        #endregion

        public static void Send(MonoEngine engine)
        {
            var eventObject = new MonoEngineCreateEvent(engine);
            engine.Send(eventObject, Iid, null, null);
        }
    }
}