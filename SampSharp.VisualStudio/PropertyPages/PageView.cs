using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualStudio;

namespace SampSharp.VisualStudio.PropertyPages
{
    public class PageView : UserControl, IPageView, IPropertyPageUi
    {
        private readonly IPageViewSite _pageViewSite;
        private PropertyControlMap _propertyControlMap;

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected PageView()
        {
        }

        protected PageView(IPageViewSite pageViewSite)
        {
            _pageViewSite = pageViewSite;
        }

        /// <summary>
        ///     This property is used to map the control on a PageView object to a property
        ///     in PropertyStore object.
        /// </summary>
        protected virtual PropertyControlTable PropertyControlTable { get; } = null;

        /// <summary>
        ///     Occurs if the value of a control changed.
        /// </summary>
        public event EventHandler<UserEditCompleteEventArgs> UserEditComplete;
        
        protected virtual void OnInitialize()
        {
        }

        #region Implementation of IPageView

        /// <summary>
        ///     Initialize this PageView object.
        /// </summary>
        /// <param name="parentControl">The parent control of this PageView object.</param>
        /// <param name="rectangle">The position of this PageView object.</param>
        public virtual void Initialize(Control parentControl, Rectangle rectangle)
        {
            SetBounds(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
            Parent = parentControl;

            // Initialize the value of the Controls on this PageView object. 
            _propertyControlMap = new PropertyControlMap(_pageViewSite, this, PropertyControlTable);
            _propertyControlMap.InitializeControls();

            // Register the event when the value of a Control changed.
            foreach (var control in PropertyControlTable.GetControls())
            {
                var textBox = control as TextBox;
                var checkBox = control as CheckBox;
                if (textBox != null)
                    textBox.TextChanged +=
                        (sender, args) => OnUserEditComplete(new UserEditCompleteEventArgs(textBox, textBox.Text));
                else if (checkBox != null)
                    checkBox.CheckedChanged +=
                        (sender, args) =>
                                OnUserEditComplete(new UserEditCompleteEventArgs(checkBox, checkBox.Checked.ToString()));
            }

            OnInitialize();
        }

        /// <summary>
        ///     Move to new position.
        /// </summary>
        public void MoveView(Rectangle rectangle)
        {
            Location = new Point(rectangle.X, rectangle.Y);
            Size = new Size(rectangle.Width, rectangle.Height);
        }

        /// <summary>
        ///     Pass a keystroke to the property page for processing.
        /// </summary>
        public int ProcessAccelerator(ref Message keyboardMessage) =>
            FromHandle(keyboardMessage.HWnd).PreProcessMessage(ref keyboardMessage)
                ? VSConstants.S_OK
                : VSConstants.S_FALSE;

        /// <summary>
        ///     Refresh the UI.
        /// </summary>
        public void RefreshPropertyValues() => _propertyControlMap?.InitializeControls();

        #endregion

        #region Implementation of IPropertyPageUI

        /// <summary>
        ///     Get the value of a Control on this PageView object.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public virtual string GetControlValue(Control control)
        {
            var chk = control as CheckBox;
            if (chk != null)
                return chk.Checked.ToString();

            var tb = control as TextBox;
            if (tb == null)
                throw new ArgumentOutOfRangeException();
            return tb.Text;
        }

        /// <summary>
        ///     Set the value of a Control on this PageView object.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="value">The value.</param>
        public virtual void SetControlValue(Control control, string value)
        {
            var chk = control as CheckBox;
            if (chk != null)
            {
                bool flag;
                if (!bool.TryParse(value, out flag))
                    flag = false;
                chk.Checked = flag;
            }
            else
            {
                var tb = control as TextBox;
                if (tb != null)
                    tb.Text = value;
            }
        }

        #endregion

        protected virtual void OnUserEditComplete(UserEditCompleteEventArgs e)
        {
            UserEditComplete?.Invoke(this, e);
        }
    }
}