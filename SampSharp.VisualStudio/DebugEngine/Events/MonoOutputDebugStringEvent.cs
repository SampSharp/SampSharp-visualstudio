using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace SampSharp.VisualStudio.DebugEngine.Events
{
    internal sealed class MonoOutputDebugStringEvent : AsynchronousEvent, IDebugOutputStringEvent2
    {
        public const string Iid = "569c4bb1-7b82-46fc-ae28-4536ddad753e";

        private readonly string _str;
        public MonoOutputDebugStringEvent(string str)
        {
            _str = str;
        }

        #region IDebugOutputStringEvent2 Members

        int IDebugOutputStringEvent2.GetString(out string pbstrString)
        {
            pbstrString = _str;
            return VSConstants.S_OK;
        }

        #endregion
    }
}
