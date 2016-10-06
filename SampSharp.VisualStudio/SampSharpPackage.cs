using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using SampSharp.VisualStudio.DebugEngine;
using SampSharp.VisualStudio.ProgramProperties;
using SampSharp.VisualStudio.Projects;
using SampSharp.VisualStudio.Utils;

namespace SampSharp.VisualStudio
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(PackageGuidString)]
    [ProvideObject(typeof(SampSharpPropertyPage), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideProjectFactory(typeof(SampSharpProjectFactory), "SampSharp", null, null, null, @"..\Templates\Projects")]
    [ProvideDebugEngine("SampSharp Debug Engine", typeof(SampSharpDebugProvider), typeof(MonoEngine),
         "{"+ Guids.EngineIdGuidString + "}", true, true,
         false)]
    public sealed class SampSharpPackage : Package
    {
        /// <summary>
        ///     SampSharp package GUID string.
        /// </summary>
        public const string PackageGuidString = "72e5e379-d9c1-442f-a350-f93845fd0cbb";

        /// <summary>
        ///     Initialization of the package; this method is called right after the package is sited, so this is the place
        ///     where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            RegisterProjectFactory(new SampSharpProjectFactory(this));
        }

        public T GetGlobalService<T>()
        {
            return (T) GetService(typeof(T));
        }
    }
}