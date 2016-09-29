﻿using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace SampSharp.VisualStudio.Debuggers
{
	public class SampSharpDebugProvider : IDebugProgramProvider2
	{
		public int GetProviderProcessData(enum_PROVIDER_FLAGS flags, IDebugDefaultPort2 port, AD_PROCESS_ID processId,
			CONST_GUID_ARRAY engineFilter, PROVIDER_PROCESS_DATA[] process)
		{
			return VSConstants.S_FALSE;
		}

		public int GetProviderProgramNode(enum_PROVIDER_FLAGS flags, IDebugDefaultPort2 port, AD_PROCESS_ID processId,
			ref Guid guidEngine, ulong programId, out IDebugProgramNode2 programNode)
		{
			programNode = null;
			return VSConstants.S_FALSE;
		}

		public int WatchForProviderEvents(enum_PROVIDER_FLAGS flags, IDebugDefaultPort2 port, AD_PROCESS_ID processId,
			CONST_GUID_ARRAY engineFilter, ref Guid guidLaunchingEngine, IDebugPortNotify2 eventCallback)
		{
			return VSConstants.S_OK;
		}

		public int SetLocale(ushort locale)
		{
			return VSConstants.S_OK;
		}
	}
}