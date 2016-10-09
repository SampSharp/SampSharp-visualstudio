using System;
using Microsoft.VisualStudio.Debugger.Interop;
using SampSharp.VisualStudio.DebugEngine.Events;

namespace SampSharp.VisualStudio.DebugEngine
{
    public class MonoCallback
    {
        private readonly MonoEngine _engine;

        public MonoCallback(IDebugEventCallback2 callback, MonoEngine engine)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            if (engine == null) throw new ArgumentNullException(nameof(engine));

            Callback = callback;
            _engine = engine;
        }

        public IDebugEventCallback2 Callback { get; }

        public void Send(IDebugEvent2 eventObject, string eventId, IDebugProgram2 program, IDebugThread2 thread)
        {
            uint attributes;
            var @event = new Guid(eventId);
            eventObject.GetAttributes(out attributes);
            Callback.Event(_engine, null, program, thread, eventObject, ref @event, attributes);
        }
        
        public void Send(IDebugEvent2 eventObject, string eventId, IDebugThread2 thread)
        {
            Callback.Send(_engine, eventObject, eventId, _engine, thread);
        }


        public void OnError(string message)
        {
            SendMessage(message, OutputMessage.Severity.Error, isAsync: true);
        }

        /// <summary>
        /// Sends an error to the user, blocking until the user dismisses the error
        /// </summary>
        /// <param name="message">string to display to the user</param>
        public void OnErrorImmediate(string message)
        {
            SendMessage(message, OutputMessage.Severity.Error, isAsync: false);
        }

        public void OnWarning(string message)
        {
            SendMessage(message, OutputMessage.Severity.Warning, isAsync: true);
        }
        
        public void OnOutputString(string outputString)
        {
            var eventObject = new MonoOutputDebugStringEvent(outputString);

            Send(eventObject, MonoOutputDebugStringEvent.Iid, null);
        }

        public void OnOutputMessage(OutputMessage outputMessage)
        {
            try
            {
                if (outputMessage.ErrorCode == 0)
                {
                    var eventObject = new MonoMessageEvent(outputMessage, isAsync: true);
                    Send(eventObject, MonoMessageEvent.Iid, null);
                }
                else
                {
                    var eventObject = new MonoMessageEvent(outputMessage, isAsync: true);
                    Send(eventObject, MonoMessageEvent.Iid, null);
                }
            }
            catch
            {
                // Since we are often trying to report an exception, if something goes wrong we don't want to take down the process,
                // so ignore the failure.
            }
        }
        
        private void SendMessage(string message, OutputMessage.Severity severity, bool isAsync)
        {
            try
            {
                // IDebugErrorEvent2 is used to report error messages to the user when something goes wrong in the debug engine.
                // The sample engine doesn't take advantage of this.

                MonoMessageEvent eventObject = new MonoMessageEvent(new OutputMessage(message, enum_MESSAGETYPE.MT_MESSAGEBOX, severity), isAsync);
                Send(eventObject, MonoMessageEvent.Iid, null);
            }
            catch
            {
                // Since we are often trying to report an exception, if something goes wrong we don't want to take down the process,
                // so ignore the failure.
            }
        }
    }
}