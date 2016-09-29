using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace SampSharp.VisualStudio.Debuggers
{
	public class MonoProgramNode : IDebugProgramNode2
	{
		private readonly AD_PROCESS_ID _processId;

		public MonoProgramNode(AD_PROCESS_ID processId)
		{
			_processId = processId;
		}

		public int GetProgramName(out string programName)
		{
			programName = null;
			return VSConstants.S_OK;
		}

		public int GetHostName(enum_GETHOSTNAME_TYPE hostNameType, out string hostName)
		{
			hostName = null;
			return VSConstants.S_OK;
		}

		public int GetHostPid(AD_PROCESS_ID[] hostProcessIds)
		{
			hostProcessIds[0] = _processId;
			return VSConstants.S_OK;
		}

		public int GetHostMachineName_V7(out string hostMachineName)
		{
			hostMachineName = null;
			return VSConstants.E_NOTIMPL;
		}

		public int Attach_V7(IDebugProgram2 pMdmProgram, IDebugEventCallback2 pCallback, uint dwReason)
		{
			return VSConstants.E_NOTIMPL;
		}

		public int GetEngineInfo(out string engineName, out Guid engineGuid)
		{
			engineName = "Mono Debug Engine";
			engineGuid = new Guid(Guids.EngineId);
			return VSConstants.S_OK;
		}

		public int DetachDebugger_V7()
		{
			return VSConstants.E_NOTIMPL;
		}
	}
}