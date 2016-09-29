using System;

namespace SampSharp.VisualStudio.PropertyPages
{
	public interface IPropertyStore
	{
		event Action StoreChanged;
		void Dispose();
		void Initialize(object[] dataObject);
		void Persist(string propertyName, string propertyValue);
		string PropertyValue(string propertyName);
	}
}