using Microsoft.VisualStudio.Debugger.Interop;

namespace SampSharp.VisualStudio.Debuggers
{
	public class MonoPropertyInfoEnum : Enumerator<DEBUG_PROPERTY_INFO, IEnumDebugPropertyInfo2>, IEnumDebugPropertyInfo2
	{
		public MonoPropertyInfoEnum(DEBUG_PROPERTY_INFO[] data) : base(data)
		{
		}
	}
}