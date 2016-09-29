namespace SampSharp.VisualStudio.Debuggers
{
	public class MonoSourceMapping
	{
		public MonoSourceMapping(string sourceRoot, string buildRoot)
		{
			SourceRoot = sourceRoot;
			BuildRoot = buildRoot;
		}

		/// <summary>
		///     The path to the directory that contains the .csproj file.
		/// </summary>
		public string SourceRoot { get; }

		/// <summary>
		///     The path to the directory on the build server that contains the .csproj file.
		/// </summary>
		public string BuildRoot { get; }
	}
}