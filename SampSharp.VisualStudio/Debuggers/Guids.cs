using System;

namespace SampSharp.VisualStudio.Debuggers
{
	public class Guids
	{
		public const string EngineId = "{4C46E2E6-9222-44F3-ABA0-E1A15BF03CD0}";
		public const string AttachCommandGroupGuidString = "727E4A66-FA6B-44AE-8639-9489FC65347E";
		public const uint AttachCommandId = 0x100;
		public static readonly Guid AttachCommandGroupGuid = new Guid(AttachCommandGroupGuidString);
	}
}