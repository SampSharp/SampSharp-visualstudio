using System.Collections.Generic;
using System.Windows.Forms;

namespace SampSharp.VisualStudio.PropertyPages
{
    public class PropertyControlTable
    {
        // With these two dictionaries, it is quicker to find a Control or Property Name. 
        private readonly Dictionary<Control, string> _controlNameIndex = new Dictionary<Control, string>();
        private readonly Dictionary<string, Control> _propertyNameIndex = new Dictionary<string, Control>();

        /// <summary>
        ///     Add a Key Value Pair to the dictionaries.
        /// </summary>
        public void Add(string propertyName, Control control)
        {
            _controlNameIndex.Add(control, propertyName);
            _propertyNameIndex.Add(propertyName, control);
        }

        /// <summary>
        ///     Get the Control which is mapped to a Property.
        /// </summary>
        public Control GetControlFromPropertyName(string propertyName)
        {
            Control control;
            if (_propertyNameIndex.TryGetValue(propertyName, out control))
                return control;
            return null;
        }

        /// <summary>
        ///     Get all Controls.
        /// </summary>
        public List<Control> GetControls()
        {
            var controlArray = new Control[_controlNameIndex.Count];
            _controlNameIndex.Keys.CopyTo(controlArray, 0);
            return new List<Control>(controlArray);
        }

        /// <summary>
        ///     Get the Property Name which is mapped to a Control.
        /// </summary>
        public string GetPropertyNameFromControl(Control control)
        {
            string str;
            if (_controlNameIndex.TryGetValue(control, out str))
                return str;
            return null;
        }

        /// <summary>
        ///     Get all Property Names.
        /// </summary>
        public List<string> GetPropertyNames()
        {
            var strArray = new string[_propertyNameIndex.Count];
            _propertyNameIndex.Keys.CopyTo(strArray, 0);
            return new List<string>(strArray);
        }

        /// <summary>
        ///     Remove a Key Value Pair from the dictionaries.
        /// </summary>
        public void Remove(string propertyName, Control control)
        {
            _controlNameIndex.Remove(control);
            _propertyNameIndex.Remove(propertyName);
        }
    }
}