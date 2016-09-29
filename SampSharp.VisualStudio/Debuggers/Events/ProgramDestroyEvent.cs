using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace SampSharp.VisualStudio.Debuggers.Events
{
	// This interface is sent by the debug engine (DE) to the session debug manager (SDM) when a program has run to completion
	// or is otherwise destroyed.
	public class ProgramDestroyEvent : SynchronousEvent, IDebugProgramDestroyEvent2
	{
		public const string Iid = "E147E9E3-6440-4073-A7B7-A65592C714B5";

		private readonly uint _mExitCode;

		public ProgramDestroyEvent(uint exitCode)
		{
			_mExitCode = exitCode;
		}

		int IDebugProgramDestroyEvent2.GetExitCode(out uint exitCode)
		{
			exitCode = _mExitCode;

			return VSConstants.S_OK;
		}
	}
}