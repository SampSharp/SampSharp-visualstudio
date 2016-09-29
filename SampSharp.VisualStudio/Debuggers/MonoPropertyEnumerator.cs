using Microsoft.VisualStudio.Debugger.Interop;

namespace SampSharp.VisualStudio.Debuggers
{
	public class MonoPropertyEnumerator : Enumerator<DEBUG_PROPERTY_INFO, IEnumDebugPropertyInfo2>, IEnumDebugPropertyInfo2
	{
		public MonoPropertyEnumerator(DEBUG_PROPERTY_INFO[] properties) : base(properties)
		{
		}
	}
}