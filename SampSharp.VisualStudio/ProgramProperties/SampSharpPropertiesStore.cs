using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Shell.Interop;
using SampSharp.VisualStudio.Projects;
using SampSharp.VisualStudio.PropertyPages;

namespace SampSharp.VisualStudio.ProgramProperties
{
	public class SampSharpPropertiesStore : IDisposable, IPropertyStore
	{
		private readonly List<SampSharpFlavorConfig> _configs = new List<SampSharpFlavorConfig>();
		private bool _disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public event Action StoreChanged;

		/// <summary>
		///     Use the data passed in to initialize the Properties.
		/// </summary>
		/// <param name="dataObjects">
		///     This is normally only one our configuration object, which means that there will be only one elements in configs.
		/// </param>
		public void Initialize(object[] dataObjects)
		{
			// If we are editing multiple configuration at once, we may get multiple objects.
			foreach (var dataObject in dataObjects)
			{
			    var vsConfig = dataObject as IVsCfg;
			    if (vsConfig != null)
			    {
			        // This should be our configuration object, so retrive the specific
			        // class so we can access its properties.
			        var config = SampSharpFlavorConfig.GetSampSharpFlavorCfgFromVsCfg(vsConfig);

			        if (!_configs.Contains(config))
			            _configs.Add(config);
			    }
			}
		}

		/// <summary>
		///     Set the value of the specified property in storage.
		/// </summary>
		/// <param name="propertyName">Name of the property to set.</param>
		/// <param name="propertyValue">Value to set the property to.</param>
		public void Persist(string propertyName, string propertyValue)
		{
			// If the value is null, make it empty.
			if (propertyValue == null)
				propertyValue = string.Empty;

			foreach (var config in _configs)
				config[propertyName] = propertyValue;

			StoreChanged?.Invoke();
		}

		/// <summary>
		///     Retrieve the value of the specified property from storage
		/// </summary>
		/// <param name="propertyName">Name of the property to retrieve</param>
		public string PropertyValue(string propertyName)
		{
			string value = null;
			if (_configs.Count > 0)
				value = _configs[0][propertyName];
			if (_configs.Any(config => config[propertyName] != value))
			{
				value = string.Empty;
			}

			return value;
		}

		protected virtual void Dispose(bool disposing)
		{
			// Protect from being called multiple times.
			if (_disposed)
				return;

			if (disposing)
				_configs.Clear();

			_disposed = true;
		}
	}
}