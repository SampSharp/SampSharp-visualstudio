using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace SampSharp.VisualStudio.Debuggers.Events
{
    public class SynchronousEvent : IDebugEvent2
    {
        public const uint Attributes = (uint) enum_EVENTATTRIBUTES.EVENT_SYNCHRONOUS;

        #region Implementation of IDebugEvent2

        /// <summary>
        ///     Gets the attributes for this debug event.
        /// </summary>
        /// <param name="eventAttributes"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetAttributes(out uint eventAttributes)
        {
            eventAttributes = Attributes;
            return VSConstants.S_OK;
        }

        #endregion
    }
}