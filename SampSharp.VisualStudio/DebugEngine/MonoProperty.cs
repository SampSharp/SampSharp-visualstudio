using System;
using Microsoft.VisualStudio.Debugger.Interop;
using Mono.Debugging.Client;
using SampSharp.VisualStudio.DebugEngine.Enumerators;
using static Microsoft.VisualStudio.VSConstants;

namespace SampSharp.VisualStudio.DebugEngine
{
    public class MonoProperty : IDebugProperty2
    {
        private readonly string _expression;
        private readonly MonoProperty _parent;
        private readonly ObjectValue _value;

        public MonoProperty(string expression, ObjectValue value, MonoProperty parent = null)
        {
            _expression = expression;
            _value = value;
            _parent = parent;
        }

        public DEBUG_PROPERTY_INFO ConstructDebugPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields)
        {
            var propertyInfo = new DEBUG_PROPERTY_INFO();

            if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_FULLNAME) != 0)
            {
                propertyInfo.bstrFullName = _expression;
                propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_FULLNAME;
            }

            if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME) != 0)
            {
                propertyInfo.bstrName = _value.Name;
                propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME;
            }

            if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE) != 0)
            {
                propertyInfo.bstrType = _value.TypeName;
                propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE;
            }

            if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE) != 0)
            {
                propertyInfo.bstrValue = _value.DisplayValue;
                propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE;
            }

            if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ATTRIB) != 0)
            {
                // The sample does not support writing of values displayed in the debugger, so mark them all as read-only.
                propertyInfo.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_READONLY;

                if (_value.HasChildren)
                    propertyInfo.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_OBJ_IS_EXPANDABLE;
            }

            // If the debugger has asked for the property, or the property has children (meaning it is a pointer in the sample)
            // then set the pProperty field so the debugger can call back when the chilren are enumerated.
            if (((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_PROP) != 0) || _value.HasChildren)
            {
                propertyInfo.pProperty = this;
                propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_PROP;
            }

            return propertyInfo;
        }

        #region Implementation of IDebugProperty2

        /// <summary>
        ///     Fills in a DEBUG_PROPERTY_INFO structure that describes a property.
        /// </summary>
        /// <param name="dwFields">The fields.</param>
        /// <param name="dwRadix">The radix.</param>
        /// <param name="dwTimeout">The timeout.</param>
        /// <param name="rgpArgs">The  arguments.</param>
        /// <param name="dwArgCount">The argument count.</param>
        /// <param name="pPropertyInfo">The property information.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, uint dwTimeout,
            IDebugReference2[] rgpArgs, uint dwArgCount, DEBUG_PROPERTY_INFO[] pPropertyInfo)
        {
            pPropertyInfo[0] = new DEBUG_PROPERTY_INFO();
            pPropertyInfo[0] = ConstructDebugPropertyInfo(dwFields);
            return S_OK;
        }

        /// <summary>
        ///     Sets the value of a property from a string.
        /// </summary>
        /// <param name="pszValue">The value.</param>
        /// <param name="dwRadix">The radix.</param>
        /// <param name="dwTimeout">The timeout.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int SetValueAsString(string pszValue, uint dwRadix, uint dwTimeout)
        {
            _value.SetValue(pszValue);
            return S_OK;
        }

        /// <summary>
        ///     Sets the value of the property from the value of a given reference.
        /// </summary>
        /// <param name="rgpArgs">The RGP arguments.</param>
        /// <param name="dwArgCount">The dw argument count.</param>
        /// <param name="pValue">The p value.</param>
        /// <param name="dwTimeout">The dw timeout.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int SetValueAsReference(IDebugReference2[] rgpArgs, uint dwArgCount, IDebugReference2 pValue,
            uint dwTimeout)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     Enumerates the children of a property.
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="radix">The radix.</param>
        /// <param name="guidFilter">The unique identifier filter.</param>
        /// <param name="attributeFilter">The attribute filter.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="enumerator">The enumerator.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int EnumChildren(enum_DEBUGPROP_INFO_FLAGS fields, uint radix, ref Guid guidFilter,
            enum_DBG_ATTRIB_FLAGS attributeFilter, string filter, uint timeout, out IEnumDebugPropertyInfo2 enumerator)
        {
            enumerator = null;

            if (_value.HasChildren)
            {
                var children = _value.GetAllChildren();
                var properties = new DEBUG_PROPERTY_INFO[children.Length];
                for (var i = 0; i < children.Length; i++)
                {
                    var child = children[i];
                    properties[i] = new MonoProperty(_expression, child, this).ConstructDebugPropertyInfo(fields);
                }
                enumerator = new MonoPropertyEnumerator(properties);
                return S_OK;
            }

            return S_FALSE;
        }

        /// <summary>
        ///     Returns the parent of a property.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetParent(out IDebugProperty2 parent)
        {
            parent = _parent;
            return parent == null ? S_FALSE : S_OK;
        }

        /// <summary>
        ///     Returns the property that describes the most-derived property of a property.
        /// </summary>
        /// <param name="ppDerivedMost">The derived most.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetDerivedMostProperty(out IDebugProperty2 ppDerivedMost)
        {
            ppDerivedMost = null;
            return E_NOTIMPL;
        }

        /// <summary>
        ///     Returns the memory bytes that compose the value of a property.
        /// </summary>
        /// <param name="ppMemoryBytes">The memory bytes.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes)
        {
            ppMemoryBytes = null;
            return E_NOTIMPL;
        }

        /// <summary>
        ///     Returns the memory context for a property value.
        /// </summary>
        /// <param name="ppMemory">The memory.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetMemoryContext(out IDebugMemoryContext2 ppMemory)
        {
            ppMemory = null;
            return E_NOTIMPL;
        }

        /// <summary>
        ///     Returns the size, in bytes, of the property value.
        /// </summary>
        /// <param name="pdwSize">The size.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetSize(out uint pdwSize)
        {
            pdwSize = 0;
            return E_NOTIMPL;
        }

        /// <summary>
        ///     Returns a reference to this property's value.
        /// </summary>
        /// <param name="ppReference">The reference.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetReference(out IDebugReference2 ppReference)
        {
            ppReference = null;
            return E_NOTIMPL;
        }

        /// <summary>
        ///     Returns the extended information of a property.
        /// </summary>
        /// <param name="guidExtendedInfo">The extended information GUID.</param>
        /// <param name="pExtendedInfo">The extended information.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetExtendedInfo(ref Guid guidExtendedInfo, out object pExtendedInfo)
        {
            pExtendedInfo = null;
            return E_NOTIMPL;
        }

        #endregion
    }
}