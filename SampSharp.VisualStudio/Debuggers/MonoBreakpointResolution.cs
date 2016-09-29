using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace SampSharp.VisualStudio.Debuggers
{
	public class MonoBreakpointResolution : IDebugBreakpointResolution2
	{
		private readonly uint _address;
		private readonly MonoDocumentContext _documentContext;
		private readonly MonoEngine _engine;

		public MonoBreakpointResolution(MonoEngine engine, uint address, MonoDocumentContext documentContext)
		{
			_engine = engine;
			_address = address;
			_documentContext = documentContext;
		}

		public int GetBreakpointType(enum_BP_TYPE[] type)
		{
			type[0] = enum_BP_TYPE.BPT_CODE;
			return VSConstants.S_OK;
		}

		public int GetResolutionInfo(enum_BPRESI_FIELDS fields, BP_RESOLUTION_INFO[] resolutionInfo)
		{
			if ((fields & enum_BPRESI_FIELDS.BPRESI_BPRESLOCATION) != 0)
			{
				// The sample engine only supports code breakpoints.
			    var location = new BP_RESOLUTION_LOCATION { bpType = (uint) enum_BP_TYPE.BPT_CODE };

			    // The debugger will not QI the IDebugCodeContex2 interface returned here. We must pass the pointer
				// to IDebugCodeContex2 and not IUnknown.
				var codeContext = new MonoMemoryAddress(_engine, _address, _documentContext);
				location.unionmember1 = Marshal.GetComInterfaceForObject(codeContext, typeof(IDebugCodeContext2));
				resolutionInfo[0].bpResLocation = location;
				resolutionInfo[0].dwFields |= enum_BPRESI_FIELDS.BPRESI_BPRESLOCATION;
			}


			return VSConstants.S_OK;
		}
	}
}