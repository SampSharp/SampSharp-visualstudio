using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using EnvDTE;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using SampSharp.VisualStudio.DebugEngine;
using SampSharp.VisualStudio.ProgramProperties;
using SampSharp.VisualStudio.Utils;
using static Microsoft.VisualStudio.VSConstants;
using Process = System.Diagnostics.Process;

namespace SampSharp.VisualStudio.Projects
{
    public class SampSharpFlavorConfig : IVsProjectFlavorCfg, IPersistXMLFragment, IVsDebuggableProjectCfg,
        IVsBuildableProjectCfg
    {
        /// <summary>
        ///     This allows the property page to map a IVsCfg object (the baseConfiguration) to an actual instance of
        ///     CustomPropertyPageProjectFlavorCfg.
        /// </summary>
        private static readonly Dictionary<IVsCfg, SampSharpFlavorConfig> Configs =
            new Dictionary<IVsCfg, SampSharpFlavorConfig>();

        private readonly IVsDebuggableProjectCfg _baseDebugConfiguration;
        private readonly IVsCfg _baseProjectConfig;

        private readonly Dictionary<uint, IVsBuildStatusCallback> _callbacks =
            new Dictionary<uint, IVsBuildStatusCallback>();

        private readonly IVsProjectFlavorCfg _innerProjectFlavorConfig;

        private readonly SampSharpProjectFlavor _project;
        private readonly IVsProjectCfg _projectConfig;
        private readonly Dictionary<string, string> _propertiesList = new Dictionary<string, string>();

        private uint _callbackCookieCounter;
        private bool _isClosed;
        private bool _isDirty;

        public SampSharpFlavorConfig(SampSharpProjectFlavor project, IVsCfg baseProjectConfig,
            IVsProjectFlavorCfg innerProjectFlavorConfig)
        {
            _project = project;
            _baseProjectConfig = baseProjectConfig;
            _innerProjectFlavorConfig = innerProjectFlavorConfig;

            _projectConfig = (IVsProjectCfg2) baseProjectConfig;
            string configurationName;
            _projectConfig.get_CanonicalName(out configurationName);

            Configs.Add(baseProjectConfig, this);

            var debugGuid = typeof(IVsDebuggableProjectCfg).GUID;
            IntPtr baseDebugConfigurationPtr;
            innerProjectFlavorConfig.get_CfgType(ref debugGuid, out baseDebugConfigurationPtr);
            _baseDebugConfiguration = (IVsDebuggableProjectCfg) Marshal.GetObjectForIUnknown(baseDebugConfigurationPtr);
        }

        /// <summary>
        ///     Get or set a Property.
        /// </summary>
        public string this[string propertyName]
        {
            get
            {
                var value = _propertiesList.ContainsKey(propertyName) ? _propertiesList[propertyName] : "";

                if (string.IsNullOrEmpty(value))
                    switch (propertyName)
                    {
                        case SampSharpPropertyPage.MonoDirectory:
                            return @"..\..\env\mono";
                    }
                return value;
            }
            set
            {
                // Don't do anything if there isn't any real change
                if (this[propertyName] == value)
                    return;

                _isDirty = true;
                if (_propertiesList.ContainsKey(propertyName))
                    _propertiesList.Remove(propertyName);
                _propertiesList.Add(propertyName, value);
            }
        }

        /// <summary>
        ///     Implement the InitNew method to initialize the project extension properties and other build-independent data. This
        ///     method is called if there is no XML configuration data present in the project file.
        /// </summary>
        /// <param name="guidFlavor">GUID of the project subtype.</param>
        /// <param name="storage">
        ///     Specifies the storage type used for persisting files. Values are taken from the
        ///     _PersistStorageType enumeration. The file type is either project file (.csproj or .vbproj) or user file
        ///     (.csproj.user or .vbproj.user).
        /// </param>
        public int InitNew(ref Guid guidFlavor, uint storage)
        {
            // Return, if it is our guid.
            if (IsMyFlavorGuid(ref guidFlavor))
                return S_OK;

            // Forward the call to inner flavor(s).
            var fragment = _innerProjectFlavorConfig as IPersistXMLFragment;
            return fragment?.InitNew(ref guidFlavor, storage) ?? S_OK;
        }

        /// <summary>
        ///     Implement the IsFragmentDirty method to determine whether an XML fragment has changed since it was last saved to
        ///     its current file.
        /// </summary>
        /// <param name="storage">
        ///     Storage type of the file in which the XML is persisted. Values are taken from
        ///     _PersistStorageType enumeration.
        /// </param>
        /// <param name="pfDirty">Set to 1 if dirty, 0 if not</param>
        public int IsFragmentDirty(uint storage, out int pfDirty)
        {
            pfDirty = 0;
            switch (storage)
            {
                // Specifies storage file type to project file.
                case (uint) _PersistStorageType.PST_PROJECT_FILE:
                    if (_isDirty)
                        pfDirty |= 1;
                    break;
                // Specifies storage file type to user file.
                case (uint) _PersistStorageType.PST_USER_FILE:
                    // Do not store anything in the user file.
                    break;
            }

            // Forward the call to inner flavor(s) 
            if ((pfDirty == 0) && _innerProjectFlavorConfig is IPersistXMLFragment)
                return ((IPersistXMLFragment) _innerProjectFlavorConfig).IsFragmentDirty(storage, out pfDirty);

            return S_OK;
        }

        /// <summary>
        ///     Implement the Load method to load the XML data from the project file.
        /// </summary>
        /// <param name="guidFlavor">GUID of the project subtype.</param>
        /// <param name="storage">
        ///     Storage type of the file in which the XML is persisted. Values are taken from _PersistStorageType
        ///     enumeration.
        /// </param>
        /// <param name="pszXmlFragment">String containing the XML fragment.</param>
        public int Load(ref Guid guidFlavor, uint storage, string pszXmlFragment)
        {
            if (IsMyFlavorGuid(ref guidFlavor))
                switch (storage)
                {
                    case (uint) _PersistStorageType.PST_PROJECT_FILE:
                        break;
                    case (uint) _PersistStorageType.PST_USER_FILE:
                        var doc = new XmlDocument();
                        var node = doc.CreateElement(GetType().Name);
                        node.InnerXml = pszXmlFragment;
                        if (node.FirstChild != null)
                            foreach (XmlNode child in node.FirstChild.ChildNodes)
                                _propertiesList.Add(child.Name, child.InnerText);
                        break;
                }

            // Forward the call to inner flavor(s)
            var cfg = _innerProjectFlavorConfig as IPersistXMLFragment;
            return cfg?.Load(ref guidFlavor, storage, pszXmlFragment) ?? S_OK;
        }

        /// <summary>
        ///     Implement the Save method to save the XML data in the project file.
        /// </summary>
        /// <param name="guidFlavor">GUID of the project subtype.</param>
        /// <param name="storage">
        ///     Storage type of the file in which the XML is persisted. Values are taken from
        ///     _PersistStorageType enumeration.
        /// </param>
        /// <param name="pbstrXmlFragment">String containing the XML fragment.</param>
        /// <param name="fClearDirty">
        ///     Indicates whether to clear the dirty flag after the save is complete. If true, the flag
        ///     should be cleared. If false, the flag should be left unchanged.
        /// </param>
        public int Save(ref Guid guidFlavor, uint storage, out string pbstrXmlFragment, int fClearDirty)
        {
            pbstrXmlFragment = null;

            if (IsMyFlavorGuid(ref guidFlavor))
                switch (storage)
                {
                    case (uint) _PersistStorageType.PST_PROJECT_FILE:
                        break;
                    case (uint) _PersistStorageType.PST_USER_FILE:
                        var doc = new XmlDocument();
                        var root = doc.CreateElement(GetType().Name);

                        foreach (var property in _propertiesList)
                        {
                            XmlNode node = doc.CreateElement(property.Key);
                            node.AppendChild(doc.CreateTextNode(property.Value));
                            root.AppendChild(node);
                        }

                        doc.AppendChild(root);

                        // Get XML fragment representing our data
                        pbstrXmlFragment = doc.InnerXml;

                        if (fClearDirty != 0)
                            _isDirty = false;
                        break;
                }

            // Forward the call to inner flavor(s)
            var fragment = _innerProjectFlavorConfig as IPersistXMLFragment;
            return fragment != null
                ? fragment.Save(ref guidFlavor, storage, out pbstrXmlFragment, fClearDirty)
                : S_OK;
        }

        public int get_ProjectCfg(out IVsProjectCfg ppIVsProjectCfg)
        {
            ppIVsProjectCfg = this;
            return S_OK;
        }

        public int AdviseBuildStatusCallback(IVsBuildStatusCallback callback, out uint pdwCookie)
        {
            pdwCookie = ++_callbackCookieCounter;
            _callbacks[_callbackCookieCounter] = callback;
            return S_OK;
        }

        public int UnadviseBuildStatusCallback(uint dwCookie)
        {
            _callbacks.Remove(dwCookie);
            return S_OK;
        }

        public int StartBuild(IVsOutputWindowPane outputPane, uint dwOptions)
        {
            var dteProject = GetDteProject(_project);
            var monoDirectory = this[SampSharpPropertyPage.MonoDirectory];
            outputPane.Log($"Mono runtime path: {monoDirectory}.");

            var projectFolder = Path.GetDirectoryName(dteProject.FullName);

            if (projectFolder == null)
                throw new Exception("projectFolder is null");

            var projectConfiguration = dteProject.ConfigurationManager.ActiveConfiguration;
            var dir =
                Path.GetDirectoryName(Path.Combine(projectFolder,
                    projectConfiguration.Properties.Item("OutputPath").Value.ToString()));

            if (dir == null)
                throw new Exception("dir is null");

            var fileName = dteProject.Properties.Item("OutputFileName").Value.ToString();
            var outputFile = Path.Combine(dir, fileName);

            if (string.IsNullOrWhiteSpace(monoDirectory) || !Directory.Exists(monoDirectory))
            {
                outputPane.Log(VsLogSeverity.Error, dteProject.UniqueName, dteProject.FullName,
                    "Error: You must set up the mono runtime directory on the 'SampSharp' project property page.");
                UpdateBuildStatus(0);
                return S_FALSE;
            }

            // xbuild
            var xbuidPath = Path.Combine(monoDirectory, @"lib\mono\4.5\xbuild.exe");
            var monoPath = Path.Combine(monoDirectory, @"bin\mono.exe");
            var pdb2mdbPath = Path.Combine(monoDirectory, @"lib\mono\4.5\pdb2mdb.exe");

            if (!File.Exists(xbuidPath))
            {
                outputPane.Log(VsLogSeverity.Error, dteProject.UniqueName, dteProject.FullName,
                    "Error: XBuild is missing from the specified mono runtime directory. Are you sure you selected the right directory?");
                UpdateBuildStatus(0);
                return S_FALSE;
            }

            if (!File.Exists(monoPath))
            {
                outputPane.Log(VsLogSeverity.Error, dteProject.UniqueName, dteProject.FullName,
                    "Error: mono is missing from the specified mono runtime directory. Are you sure you selected the right directory?");
                UpdateBuildStatus(0);
                return S_FALSE;
            }

            if (!File.Exists(pdb2mdbPath))
            {
                outputPane.Log(VsLogSeverity.Error, dteProject.UniqueName, dteProject.FullName,
                    "Error: pdb2mdb is missing from the specified mono runtime directory. Are you sure you selected the right directory?");
                UpdateBuildStatus(0);
                return S_FALSE;
            }

            var projectFile = dteProject.FullName;
            var projectDirectory = Path.GetDirectoryName(projectFile);

            if (projectDirectory == null)
                throw new Exception("projectDirectory is null");

            // xbuild
            var startInfo = new ProcessStartInfo
            {
                Arguments = "\"" + Path.GetFileName(projectFile) + "\"",
                WorkingDirectory = projectDirectory,
                CreateNoWindow = true,
                FileName = xbuidPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            // XBuild the project.
            outputPane.Log("Starting XBuild with arguments: " + startInfo.Arguments);

            var process = new Process { StartInfo = startInfo };
            process.OutputDataReceived += (sender, args) => { outputPane.Log("O:" + args.Data); };
            process.ErrorDataReceived += (sender, args) => { outputPane.Log("E:" + args.Data); };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            
            // Convert symbol files from .pdb to .mdb.
            startInfo = new ProcessStartInfo
            {
                Arguments = "\"" + pdb2mdbPath + "\" \"" + outputFile + "\"",
                WorkingDirectory = projectDirectory,
                CreateNoWindow = true,
                FileName = monoPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            outputPane.Log("Starting pdb2mdb with arguments: " + startInfo.Arguments);

            process = new Process { StartInfo = startInfo };
            process.OutputDataReceived += (sender, args) => { outputPane.Log("O:" + args.Data); };
            process.ErrorDataReceived += (sender, args) => { outputPane.Log("E:" + args.Data); };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            outputPane.Log("XBuild finished.");
            
            UpdateBuildStatus(1);
            return S_OK;
        }

        public int StartClean(IVsOutputWindowPane outputPane, uint dwOptions)
        {
//            var dteProject = GetDteProject(_project);
//            var projectFolder = Path.GetDirectoryName(dteProject.FullName);
//
//            outputPane.Log($"Starting cleaning of {projectFolder}...");
            outputPane.Log("Cleaning is not implemented by the SampSharp Visual Studio extension."); //todo

            return S_OK;
        }

        public int StartUpToDateCheck(IVsOutputWindowPane outputPane, uint dwOptions)
        {
            // Always rebuild
            return S_FALSE;
        }

        public int QueryStatus(out int pfBuildDone)
        {
            throw new NotImplementedException();
        }

        public int Stop(int fSync)
        {
            return S_OK;
        }

        [Obsolete]
        public int Wait(uint dwMilliseconds, int fTickWhenMessageQNotEmpty)
        {
            return S_OK;
        }

        public int QueryStartBuild(uint dwOptions, int[] pfSupported, int[] pfReady)
        {
            pfSupported[0] = 1;
            pfReady[0] = 1;
            return S_OK;
        }

        public int QueryStartClean(uint dwOptions, int[] pfSupported, int[] pfReady)
        {
            pfSupported[0] = 1;
            return S_OK;
        }

        public int QueryStartUpToDateCheck(uint dwOptions, int[] pfSupported, int[] pfReady)
        {
            pfSupported[0] = 0;
            pfReady[0] = 1;
            return S_OK;
        }

        public int get_DisplayName(out string pbstrDisplayName)
        {
            return _baseDebugConfiguration.get_DisplayName(out pbstrDisplayName);
        }

        [Obsolete]
        public int get_IsDebugOnly(out int pfIsDebugOnly)
        {
            return _baseDebugConfiguration.get_IsDebugOnly(out pfIsDebugOnly);
        }

        [Obsolete]
        public int get_IsReleaseOnly(out int pfIsReleaseOnly)
        {
            return _baseDebugConfiguration.get_IsReleaseOnly(out pfIsReleaseOnly);
        }

        [Obsolete]
        public int EnumOutputs(out IVsEnumOutputs ppIVsEnumOutputs)
        {
            return _baseDebugConfiguration.EnumOutputs(out ppIVsEnumOutputs);
        }

        [Obsolete]
        public int OpenOutput(string szOutputCanonicalName, out IVsOutput ppIVsOutput)
        {
            return _baseDebugConfiguration.OpenOutput(szOutputCanonicalName, out ppIVsOutput);
        }

        [Obsolete]
        public int get_ProjectCfgProvider(out IVsProjectCfgProvider ppIVsProjectCfgProvider)
        {
            return _baseDebugConfiguration.get_ProjectCfgProvider(out ppIVsProjectCfgProvider);
        }

        public int get_BuildableProjectCfg(out IVsBuildableProjectCfg ppIVsBuildableProjectCfg)
        {
            return _baseDebugConfiguration.get_BuildableProjectCfg(out ppIVsBuildableProjectCfg);
        }

        public int get_CanonicalName(out string pbstrCanonicalName)
        {
            return _baseDebugConfiguration.get_CanonicalName(out pbstrCanonicalName);
        }

        [Obsolete]
        public int get_Platform(out Guid pguidPlatform)
        {
            return _baseDebugConfiguration.get_Platform(out pguidPlatform);
        }

        [Obsolete]
        public int get_IsPackaged(out int pfIsPackaged)
        {
            return _baseDebugConfiguration.get_IsPackaged(out pfIsPackaged);
        }

        [Obsolete]
        public int get_IsSpecifyingOutputSupported(out int pfIsSpecifyingOutputSupported)
        {
            return _baseDebugConfiguration.get_IsSpecifyingOutputSupported(out pfIsSpecifyingOutputSupported);
        }

        [Obsolete]
        public int get_TargetCodePage(out uint puiTargetCodePage)
        {
            return _baseDebugConfiguration.get_TargetCodePage(out puiTargetCodePage);
        }

        [Obsolete]
        public int get_UpdateSequenceNumber(ULARGE_INTEGER[] puliUsn)
        {
            return _baseDebugConfiguration.get_UpdateSequenceNumber(puliUsn);
        }

        public int get_RootURL(out string pbstrRootUrl)
        {
            return _baseDebugConfiguration.get_RootURL(out pbstrRootUrl);
        }

        public int DebugLaunch(uint grfLaunch)
        {
            var dteProject = GetDteProject(_project);
            var projectFolder = Path.GetDirectoryName(dteProject.FullName);

            if (projectFolder == null)
                throw new Exception("projectFolder is null");

            var projectConfiguration = dteProject.ConfigurationManager.ActiveConfiguration;
            var dir =
                Path.GetDirectoryName(Path.Combine(projectFolder,
                    projectConfiguration.Properties.Item("OutputPath").Value.ToString()));

            if (dir == null)
                throw new Exception("dir is null");

            var fileName = dteProject.Properties.Item("OutputFileName").Value.ToString();

            var outputFile = Path.Combine(dir, fileName);

            // ReSharper disable once SuspiciousTypeConversion.Global
            var debugger = (IVsDebugger4) _project.Package.GetGlobalService<IVsDebugger>();
            var debugTargets = new VsDebugTargetInfo4[1];
            debugTargets[0].LaunchFlags = grfLaunch;
            debugTargets[0].dlo = (uint) DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;
            debugTargets[0].bstrExe = outputFile;
            debugTargets[0].guidLaunchDebugEngine = Guids.EngineIdGuid;

            var processInfo = new VsDebugTargetProcessInfo[debugTargets.Length];
            debugger.LaunchDebugTargets4(1, debugTargets, processInfo);

            return S_OK;
        }

        public int QueryDebugLaunch(uint grfLaunch, out int pfCanLaunch)
        {
            pfCanLaunch = 1;
            return S_OK;
        }

        /// <summary>
        ///     Provides access to a configuration interfaces such as IVsBuildableProjectCfg2 or IVsDebuggableProjectCfg.
        /// </summary>
        /// <param name="iidCfg">IID of the interface that is being asked</param>
        /// <param name="ppCfg">Object that implement the interface</param>
        /// <returns>HRESULT</returns>
        public int get_CfgType(ref Guid iidCfg, out IntPtr ppCfg)
        {
            ppCfg = IntPtr.Zero;
            if (iidCfg == typeof(IVsDebuggableProjectCfg).GUID)
            {
                ppCfg = Marshal.GetComInterfaceForObject(this, typeof(IVsDebuggableProjectCfg));
                return S_OK;
            }
            if (iidCfg == typeof(IVsBuildableProjectCfg2).GUID)
            {
                ppCfg = Marshal.GetComInterfaceForObject(this, typeof(IVsBuildableProjectCfg2));
                return S_OK;
            }
            if (iidCfg == typeof(IVsBuildableProjectCfg).GUID)
            {
                ppCfg = Marshal.GetComInterfaceForObject(this, typeof(IVsBuildableProjectCfg));
                return S_OK;
            }
            if (_innerProjectFlavorConfig != null)
                return _innerProjectFlavorConfig.get_CfgType(ref iidCfg, out ppCfg);
            return S_OK;
        }

        /// <summary>
        ///     Closes the IVsProjectFlavorCfg object.
        /// </summary>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int Close()
        {
            if (_isClosed)
                return E_FAIL;

            _isClosed = true;
            Configs.Remove(_baseProjectConfig);

            string configurationName;
            _projectConfig.get_CanonicalName(out configurationName);

            var hr = _innerProjectFlavorConfig.Close();

            Marshal.ReleaseComObject(_baseProjectConfig);
            Marshal.ReleaseComObject(_innerProjectFlavorConfig);
            Marshal.ReleaseComObject(_baseDebugConfiguration);

            return hr;
        }

        internal static SampSharpFlavorConfig GetSampSharpFlavorCfgFromVsCfg(IVsCfg configuration)
        {
            if (Configs.ContainsKey(configuration))
                return Configs[configuration];
            throw new ArgumentOutOfRangeException(nameof(configuration),
                $"Cannot find configuration in {nameof(Configs)}.");
        }

        private bool IsMyFlavorGuid(ref Guid guidFlavor)
        {
            return guidFlavor.Equals(typeof(SampSharpProjectFactory).GUID);
        }

        private void UpdateBuildStatus(int status)
        {
            foreach (var callback in _callbacks.Values.ToArray())
                callback.BuildEnd(status);
        }

        public static Project GetDteProject(IVsHierarchy hierarchy)
        {
            if (hierarchy == null)
                throw new ArgumentNullException(nameof(hierarchy));

            object obj;
            hierarchy.GetProperty(VSITEMID_ROOT, (int) __VSHPROPID.VSHPROPID_ExtObject, out obj);
            return obj as Project;
        }
    }
}