using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Flavor;
using Microsoft.VisualStudio.Shell.Interop;
using SampSharp.VisualStudio.ProgramProperties;

namespace SampSharp.VisualStudio.Projects
{
	public class SampSharpProjectFlavor : FlavoredProjectBase, IVsProjectFlavorCfgProvider
	{
		private IVsProjectFlavorCfgProvider _innerCfgProvider;

		public SampSharpProjectFlavor(SampSharpPackage package)
		{
			Package = package;
		}

		public SampSharpPackage Package { get; }

		public int CreateProjectFlavorCfg(IVsCfg baseProjectCfg, out IVsProjectFlavorCfg flavoredProjectCfg)
		{
			IVsProjectFlavorCfg config;
			_innerCfgProvider.CreateProjectFlavorCfg(baseProjectCfg, out config);

			flavoredProjectCfg = new SampSharpFlavorConfig(this, baseProjectCfg, config);
			return VSConstants.S_OK;
		}

		protected override void SetInnerProject(IntPtr innerIUnknown)
		{
			// This line has to be called before the base invocation or you'll get an error complaining that the serviceProvider
			// must have been set first.
			serviceProvider = Package;

			base.SetInnerProject(innerIUnknown);

			var objectForIUnknown = Marshal.GetObjectForIUnknown(innerIUnknown);
			_innerCfgProvider = (IVsProjectFlavorCfgProvider)objectForIUnknown;
		}

		/// <summary>
		///     Release the innerVsProjectFlavorCfgProvider when closed.
		/// </summary>
		protected override void Close()
		{
			base.Close();

			if (_innerCfgProvider != null)
			{
				Marshal.ReleaseComObject(_innerCfgProvider);
				_innerCfgProvider = null;
			}
		}

		/// <summary>
		///     By overriding GetProperty method and using propId parameter containing one of
		///     the values of the __VSHPROPID2 enumeration, we can filter, add or remove project
		///     properties.
		///     For example, to add a page to the configuration-dependent property pages, we
		///     need to filter configuration-dependent property pages and then add a new page
		///     to the existing list.
		/// </summary>
		protected override int GetProperty(uint itemId, int propId, out object property)
		{
			if (propId == (int)__VSHPROPID2.VSHPROPID_CfgPropertyPagesCLSIDList)
			{
				// Get a semicolon-delimited list of clsids of the configuration-dependent
				// property pages.
				ErrorHandler.ThrowOnFailure(base.GetProperty(itemId, propId, out property));

				// Add the CustomPropertyPage property page.
				property += ';' + typeof(SampSharpPropertyPage).GUID.ToString("B");

				return VSConstants.S_OK;
			}

			return base.GetProperty(itemId, propId, out property);
		}
	}
}