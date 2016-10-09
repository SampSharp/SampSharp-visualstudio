using System;
using System.Diagnostics;
using Microsoft.VisualStudio;

namespace SampSharp.VisualStudio.Utils
{
    public static class EngineUtils
    {
        public static void CheckOk(int hr)
        {
            if (hr != 0)
                throw new ComponentException(hr);
        }

        public static void RequireOk(int hr)
        {
            if (hr != 0)
                throw new InvalidOperationException();
        }

        public static int UnexpectedException(Exception e)
        {
            Debug.Fail("Unexpected exception during Attach");
            return VSConstants.RPC_E_SERVERFAULT;
        }
    }
}