using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Flavor;

namespace SampSharp.VisualStudio.Projects
{
	[Guid("629CB73E-1FBE-4FA1-81F4-F7C15FCA9590")]
	public class SampSharpProjectFactory : FlavoredProjectFactoryBase
	{
		private readonly SampSharpPackage _package;

		public SampSharpProjectFactory(SampSharpPackage package)
		{
			_package = package;
		}

		/// <summary>
		///     Create an instance of SampSharpProjectFlavor.  The initialization will be done later when Visual Studio calls
		///     InitalizeForOuter on it.
		/// </summary>
		/// <param name="outerProjectIUnknown">
		///     This value points to the outer project. It is useful if there is a
		///     Project SubType of this Project SubType.
		/// </param>
		/// <returns>A SampSharpProjectFactory instance that has not been initialized.</returns>
		protected override object PreCreateForOuter(IntPtr outerProjectIUnknown)
		{
			return new SampSharpProjectFlavor(_package);
		}
	}
}