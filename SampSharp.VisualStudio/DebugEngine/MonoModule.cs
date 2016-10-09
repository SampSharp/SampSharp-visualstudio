using System;
using System.IO;
using Microsoft.VisualStudio.Debugger.Interop;
using SampSharp.VisualStudio.Debugger;
using static Microsoft.VisualStudio.VSConstants;

namespace SampSharp.VisualStudio.DebugEngine
{
    public class MonoModule : IDebugModule3
    {
        private readonly DebuggedProgram _program;

        public MonoModule(DebuggedProgram program)
        {
            _program = program;
        }

        #region Implementation of IDebugModule2

        /// <summary>
        ///     Gets the MODULE_INFO that describes this module.
        /// </summary>
        /// <param name="dwFields">The dw fields.</param>
        /// <param name="pinfo">The pinfo.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetInfo(enum_MODULE_INFO_FIELDS dwFields, MODULE_INFO[] pinfo)
        {
            var info = new MODULE_INFO();

            if ((dwFields & enum_MODULE_INFO_FIELDS.MIF_NAME) != 0)
            {
                // todo: get path to entry dll
                info.m_bstrName = Path.GetFileName(_program.Session.VirtualMachine.RootDomain.GetEntryAssembly().Location);
                info.dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_NAME;
            }
            if ((dwFields & enum_MODULE_INFO_FIELDS.MIF_URL) != 0)
            {
                // todo: get path to entry dll
                info.m_bstrUrl = _program.Session.VirtualMachine.RootDomain.GetEntryAssembly().Location;
                info.dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_URL;
            }
            if ((dwFields & enum_MODULE_INFO_FIELDS.MIF_LOADADDRESS) != 0)
            {
                info.m_addrLoadAddress = 0;
                info.dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_LOADADDRESS;
            }
            if ((dwFields & enum_MODULE_INFO_FIELDS.MIF_PREFFEREDADDRESS) != 0)
            {
                info.m_addrPreferredLoadAddress = 0;
                info.dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_PREFFEREDADDRESS;
            }
            if ((dwFields & enum_MODULE_INFO_FIELDS.MIF_SIZE) != 0)
            {
                info.m_dwSize = 0;
                info.dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_SIZE;
            }
            if ((dwFields & enum_MODULE_INFO_FIELDS.MIF_LOADORDER) != 0)
            {
                info.m_dwLoadOrder = 0;
                info.dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_LOADORDER;
            }
            if ((dwFields & enum_MODULE_INFO_FIELDS.MIF_URLSYMBOLLOCATION) != 0)
            {
                // if (this.DebuggedModule.SymbolsLoaded)
                // {
                //     info.m_bstrUrlSymbolLocation = this.DebuggedModule.SymbolPath;
                //     info.dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_URLSYMBOLLOCATION;
                // }
            }
            if ((dwFields & enum_MODULE_INFO_FIELDS.MIF_FLAGS) != 0)
            {
                info.m_dwModuleFlags = 0;
                // if (this.DebuggedModule.SymbolsLoaded)
                // {
                info.m_dwModuleFlags |= enum_MODULE_FLAGS.MODULE_FLAG_SYMBOLS;
                // }

                // if (this.Process.Is64BitArch)
                // {
                //     info.m_dwModuleFlags |= enum_MODULE_FLAGS.MODULE_FLAG_64BIT;
                // }

                info.dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_FLAGS;
            }

            pinfo[0] = info;

            return S_OK;
        }

        /// <summary>
        ///     OBSOLETE. DO NOT USE. Reloads the symbols for this module.
        /// </summary>
        /// <param name="pszUrlToSymbols"></param>
        /// <param name="pbstrDebugMessage"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        [Obsolete]
        public int ReloadSymbols_Deprecated(string pszUrlToSymbols, out string pbstrDebugMessage)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Implementation of IDebugModule3

        /// <summary>
        ///     Returns a list of paths searched for symbols and the results of searching each path.
        /// </summary>
        /// <param name="dwFields">The fields.</param>
        /// <param name="pinfo">The pinfo.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetSymbolInfo(enum_SYMBOL_SEARCH_INFO_FIELDS dwFields, MODULE_SYMBOL_SEARCH_INFO[] pinfo)
        {
            return S_OK;
        }

        /// <summary>
        ///     Loads and initializes symbols for the current module.
        /// </summary>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int LoadSymbols()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Returns flag specifying whether the module represents user code.
        /// </summary>
        /// <param name="pfUser">The flag.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int IsUserCode(out int pfUser)
        {
            pfUser = 1;
            return S_OK;
        }

        /// <summary>
        ///     Specifies whether the module should be considered user code or not.
        /// </summary>
        /// <param name="fIsUserCode">The flag.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int SetJustMyCodeState(int fIsUserCode)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}