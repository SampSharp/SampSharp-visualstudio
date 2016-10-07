using static Microsoft.VisualStudio.VSConstants;

namespace SampSharp.VisualStudio.DebugEngine
{
    public static class VSConstantHelper
    {
        public static int ToVS(this bool value)
        {
            return value ? S_OK : S_FALSE;
        }
    }
}