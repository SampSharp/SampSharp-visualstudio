using System;

namespace SampSharp.VisualStudio.Debuggers
{
	public static class DebuggerGuids
	{
		public static readonly Guid CSharpLanguageService = new Guid("{694DD9B6-B865-4C5B-AD85-86356E9C88DC}");
        
		public static Guid GuidFilterLocals { get; } = new Guid("b200f725-e725-4c53-b36a-1ec27aef12ef");

		public static Guid GuidFilterAllLocals { get; } = new Guid("196db21f-5f22-45a9-b5a3-32cddb30db06");

		public static Guid GuidFilterArgs { get; } = new Guid("804bccea-0475-4ae7-8a46-1862688ab863");

		public static Guid GuidFilterLocalsPlusArgs { get; } = new Guid("e74721bb-10c0-40f5-807f-920d37f95419");

		public static Guid GuidFilterAllLocalsPlusArgs { get; } = new Guid("939729a8-4cb0-4647-9831-7ff465240d5f");
	}
}