﻿using System;
using System.Diagnostics;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

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

		public static int GetProcessId(IDebugProcess2 process)
		{
			var pid = new AD_PROCESS_ID[1];
			RequireOk(process.GetPhysicalProcessId(pid));

			if (pid[0].ProcessIdType != (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_SYSTEM)
				return 0;

			return (int)pid[0].dwProcessId;
		}

		public static int GetProcessId(IDebugProgram2 program)
		{
			IDebugProcess2 process;
			RequireOk(program.GetProcess(out process));

			return GetProcessId(process);
		}

		public static int UnexpectedException(Exception e)
		{
			Debug.Fail("Unexpected exception during Attach");
			return VSConstants.RPC_E_SERVERFAULT;
		}
	}
}