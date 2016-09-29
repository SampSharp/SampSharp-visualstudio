﻿using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace SampSharp.VisualStudio.Debuggers.Events
{
	public class MonoStoppingEvent : IDebugEvent2
	{
		public const uint Attributes = (uint)enum_EVENTATTRIBUTES.EVENT_ASYNC_STOP;

		int IDebugEvent2.GetAttributes(out uint eventAttributes)
		{
			eventAttributes = Attributes;
			return VSConstants.S_OK;
		}
	}
}