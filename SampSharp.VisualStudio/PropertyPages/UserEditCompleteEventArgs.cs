using System;
using System.Windows.Forms;

namespace SampSharp.VisualStudio.PropertyPages
{
    public class UserEditCompleteEventArgs : EventArgs
    {
        public UserEditCompleteEventArgs(Control control, string value)
        {
            if (control == null) throw new ArgumentNullException(nameof(control));
            if (value == null) throw new ArgumentNullException(nameof(value));
            Control = control;
            Value = value;
        }

        public Control Control { get; }

        public string Value { get; }
    }
}