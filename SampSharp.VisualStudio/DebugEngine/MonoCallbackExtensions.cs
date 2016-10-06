using System;
using Microsoft.VisualStudio.Debugger.Interop;

namespace SampSharp.VisualStudio.DebugEngine
{
    /// <summary>
    ///     Contains helper methods for sending callbacks.
    /// </summary>
    public static class MonoCallbackExtensions
    {
        /// <summary>
        ///     Sends this callback to the specified engine.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="engine">The engine.</param>
        /// <param name="eventObject">The event object.</param>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="program">The program.</param>
        /// <param name="thread">The thread.</param>
        public static void Send(this IDebugEventCallback2 callback, MonoEngine engine, IDebugEvent2 eventObject,
            string eventId, IDebugProgram2 program, IDebugThread2 thread)
        {
            uint attributes;
            var @event = new Guid(eventId);
            eventObject.GetAttributes(out attributes);
            callback.Event(engine, null, program, thread, eventObject, ref @event, attributes);
        }

        /// <summary>
        ///     Sends this callback to the specified engine.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="engine">The engine.</param>
        /// <param name="eventObject">The event object.</param>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="thread">The thread.</param>
        public static void Send(this IDebugEventCallback2 callback, MonoEngine engine, IDebugEvent2 eventObject,
            string eventId, IDebugThread2 thread)
        {
            callback.Send(engine, eventObject, eventId, engine, thread);
        }
    }
}