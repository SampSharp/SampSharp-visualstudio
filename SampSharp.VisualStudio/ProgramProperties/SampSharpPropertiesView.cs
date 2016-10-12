using System;
using System.Diagnostics;
using System.Drawing;
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
        }

        public SampSharpPropertiesView(IPageViewSite pageViewSite) : base(pageViewSite)
        {
        }

        public override void Initialize(Control parentControl, Rectangle rectangle)
        {
            InitializeComponent();
            
            base.Initialize(parentControl, rectangle);
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
                    _propertyControlTable.Add(SampSharpPropertyPage.GameMode, gameModeTextBox);
                }
                return _propertyControlTable;
            }
        }

        private void browseRuntimeDirectoryButton_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog
            {
                SelectedPath = monoLocationTextBox.Text,
                Description = "Please select the location of your mono runtime.",
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                Debug.WriteLine($"Value to be set: {dialog.SelectedPath}; value to be replaced {monoLocationTextBox.Text}");
                monoLocationTextBox.Text = dialog.SelectedPath;
            }
        }
    }
}