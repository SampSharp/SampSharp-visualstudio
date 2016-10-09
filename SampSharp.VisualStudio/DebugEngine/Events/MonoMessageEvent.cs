using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace SampSharp.VisualStudio.DebugEngine.Events
{
    public class OutputMessage
    {
        public enum Severity
        {
            Error,
            Warning
        };

        public string Message { get; }
        public enum_MESSAGETYPE MessageType { get; }
        public Severity SeverityValue { get; }

        /// <summary>
        /// Error HRESULT to send to the debug package. 0 (S_OK) if there is no associated error code.
        /// </summary>
        public uint ErrorCode { get; }

        public OutputMessage(string message, enum_MESSAGETYPE messageType, Severity severity, uint errorCode = 0)
        {
            Message = message;
            MessageType = messageType;
            SeverityValue = severity;
            ErrorCode = errorCode;
        }
    }

    public class MonoMessageEvent : IDebugEvent2, IDebugMessageEvent2
    {
        public const string Iid = "3BDB28CF-DBD2-4D24-AF03-01072B67EB9E";

        private readonly OutputMessage _outputMessage;
        private readonly bool _isAsync;

        public MonoMessageEvent(OutputMessage outputMessage, bool isAsync)
        {
            _outputMessage = outputMessage;
            _isAsync = isAsync;
        }

        public int GetAttributes(out uint eventAttributes)
        {
            if (_isAsync)
                eventAttributes = (uint)enum_EVENTATTRIBUTES.EVENT_ASYNCHRONOUS;
            else
                eventAttributes = (uint)enum_EVENTATTRIBUTES.EVENT_IMMEDIATE;

            return VSConstants.S_OK;
        }

        public int GetMessage(enum_MESSAGETYPE[] pMessageType, out string pbstrMessage, out uint pdwType, out string pbstrHelpFileName, out uint pdwHelpId)
        {
            return ConvertMessageToAD7(_outputMessage, pMessageType, out pbstrMessage, out pdwType, out pbstrHelpFileName, out pdwHelpId);
        }

        internal static int ConvertMessageToAD7(OutputMessage outputMessage, enum_MESSAGETYPE[] pMessageType, out string pbstrMessage, out uint pdwType, out string pbstrHelpFileName, out uint pdwHelpId)
        {
            const uint MB_ICONERROR = 0x00000010;
            const uint MB_ICONWARNING = 0x00000030;

            pMessageType[0] = outputMessage.MessageType;
            pbstrMessage = outputMessage.Message;
            pdwType = 0;
            if ((outputMessage.MessageType & enum_MESSAGETYPE.MT_TYPE_MASK) == enum_MESSAGETYPE.MT_MESSAGEBOX)
            {
                switch (outputMessage.SeverityValue)
                {
                    case OutputMessage.Severity.Error:
                        pdwType |= MB_ICONERROR;
                        break;

                    case OutputMessage.Severity.Warning:
                        pdwType |= MB_ICONWARNING;
                        break;
                }
            }

            pbstrHelpFileName = null;
            pdwHelpId = 0;

            return VSConstants.S_OK;
        }

        public int SetResponse(uint dwResponse)
        {
            return VSConstants.S_OK;
        }
    }
}
