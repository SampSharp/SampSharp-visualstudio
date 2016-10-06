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
        /// Get the Control which is mapped to a the specified property name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public Control GetControlFromPropertyName(string propertyName)
        {
            Control control;
            _propertyNameIndex.TryGetValue(propertyName, out control);
            return control;
        }

        /// <summary>
        /// Get all Controls.
        /// </summary>
        /// <returns></returns>
        public List<Control> GetControls()
        {
            var controlArray = new Control[_controlNameIndex.Count];
            _controlNameIndex.Keys.CopyTo(controlArray, 0);
            return new List<Control>(controlArray);
        }

        /// <summary>
        /// Get the Property Name which is mapped to a Control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public string GetPropertyNameFromControl(Control control)
        {
            string str;
            _controlNameIndex.TryGetValue(control, out str);
            return str;
        }

        /// <summary>
        /// Get all property names.
        /// </summary>
        /// <returns></returns>
        public List<string> GetPropertyNames()
        {
            var strArray = new string[_propertyNameIndex.Count];
            _propertyNameIndex.Keys.CopyTo(strArray, 0);
            return new List<string>(strArray);
        }

        /// <summary>
        /// Remove a Key Value Pair from the dictionaries.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="control">The control.</param>
        public void Remove(string propertyName, Control control)
        {
            _controlNameIndex.Remove(control);
            _propertyNameIndex.Remove(propertyName);
        }
    }
}