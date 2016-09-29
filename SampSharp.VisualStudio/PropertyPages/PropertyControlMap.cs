using System.Windows.Forms;

namespace SampSharp.VisualStudio.PropertyPages
{
	public class PropertyControlMap
	{
		// The IPageViewSite Interface is implemented by the PropertyPage class.
		private readonly IPageViewSite _pageViewSite;

		// The PropertyControlTable class stores the Control / Property Name KeyValuePairs. 
		// A KeyValuePair contains a Control of a PageView object, and a Property Name of
		// PropertyPage object.
		private readonly PropertyControlTable _propertyControlTable;

		// The IPropertyPageUi Interface is implemented by the PageView Class.
		private readonly IPropertyPageUi _propertyPageUi;

		public PropertyControlMap(IPageViewSite pageViewSite, IPropertyPageUi propertyPageUi,
			PropertyControlTable propertyControlTable)
		{
			_propertyControlTable = propertyControlTable;
			_pageViewSite = pageViewSite;
			_propertyPageUi = propertyPageUi;
		}

		/// <summary>
		///     Initialize the Controls on a PageView Object using the Properties of a PropertyPage object.
		/// </summary>
		public void InitializeControls()
		{
			_propertyPageUi.UserEditComplete -= propertyPageUI_UserEditComplete;
			foreach (var str in _propertyControlTable.GetPropertyNames())
			{
				var valueForProperty = _pageViewSite.GetValueForProperty(str);
				var controlFromPropertyName = _propertyControlTable.GetControlFromPropertyName(str);

				_propertyPageUi.SetControlValue(controlFromPropertyName, valueForProperty);
			}
			_propertyPageUi.UserEditComplete += propertyPageUI_UserEditComplete;
		}

		/// <summary>
		///     Notify the PropertyPage object that a Control value is changed.
		/// </summary>
		private void propertyPageUI_UserEditComplete(Control control, string value)
		{
			var propertyNameFromControl = _propertyControlTable.GetPropertyNameFromControl(control);
			_pageViewSite.PropertyChanged(propertyNameFromControl, value);
		}
	}
}