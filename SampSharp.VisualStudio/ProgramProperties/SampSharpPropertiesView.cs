using System.Runtime.InteropServices;
using System.Windows.Forms;
using SampSharp.VisualStudio.PropertyPages;

namespace SampSharp.VisualStudio.ProgramProperties
{
	[Guid("BF4D66B3-1A5A-4E96-AE5D-41F4BAEE8435")]
	public partial class SampSharpPropertiesView : PageView
	{
		private PropertyControlTable _propertyControlTable;

		public SampSharpPropertiesView()
		{
			InitializeComponent();
		}

		public SampSharpPropertiesView(IPageViewSite pageViewSite) : base(pageViewSite)
		{
			InitializeComponent();
		}

		/// <summary>
		///     This property is used to map the control on a PageView object to a property
		///     in PropertyStore object.
		///     This property will be called in the base class's constructor, which means that
		///     the InitializeComponent has not been called and the Controls have not been
		///     initialized.
		/// </summary>
		protected override PropertyControlTable PropertyControlTable
		{
			get
			{
				if (_propertyControlTable == null)
				{
					// This is the list of properties that will be persisted and their
					// assciation to the controls.
					_propertyControlTable = new PropertyControlTable();

					// This means that this CustomPropertyPageView object has not been
					// initialized.
					if (string.IsNullOrEmpty(Name))
						InitializeComponent();

                    // Add two Property Name / Control KeyValuePairs. 
                    _propertyControlTable.Add(SampSharpPropertyPage.MonoDirectory, monoLocationTextBox);
				}
				return _propertyControlTable;
			}
		}
        
	    private void browseRuntimeDirectoryButton_Click(object sender, System.EventArgs e)
	    {
	        var dialog = new FolderBrowserDialog
	        {
	            SelectedPath = monoLocationTextBox.Text
	        };
	        if (dialog.ShowDialog(this) == DialogResult.OK)
	        {
	            monoLocationTextBox.Text = dialog.SelectedPath;
	        }
	    }
	}
}