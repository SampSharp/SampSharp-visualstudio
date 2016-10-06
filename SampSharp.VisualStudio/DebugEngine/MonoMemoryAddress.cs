using System;
using Microsoft.VisualStudio.Debugger.Interop;
using SampSharp.VisualStudio.Utils;
using static Microsoft.VisualStudio.VSConstants;

namespace SampSharp.VisualStudio.DebugEngine
{
    public class MonoMemoryAddress : IDebugCodeContext2, IDebugCodeContext100
    {
        private readonly uint _address;
        private readonly MonoDocumentContext _documentContext;
        private readonly MonoEngine _engine;

        public MonoMemoryAddress(MonoEngine engine, uint address, MonoDocumentContext documentContext)
        {
            _engine = engine;
            _address = 0;
            _documentContext = documentContext;
        }

        #region Implementation of IDebugCodeContext100

        /// <summary>
        ///     Gets the program.
        /// </summary>
        /// <param name="program">The program.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetProgram(out IDebugProgram2 program)
        {
            program = _engine;
            return S_OK;
        }

        #endregion

        #region Implementation of IDebugMemoryContext2 

        /// <summary>
        ///     Gets the user-displayable name for this context.
        /// </summary>
        /// <param name="pbstrName">The name.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetName(out string pbstrName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Gets information that describes this context.
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="info">The information.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetInfo(enum_CONTEXT_INFO_FIELDS fields, CONTEXT_INFO[] info)
        {
            info[0].dwFields = 0;

            if ((fields & enum_CONTEXT_INFO_FIELDS.CIF_ADDRESS) != 0)
            {
                info[0].bstrAddress = _address.ToString();
                info[0].dwFields |= enum_CONTEXT_INFO_FIELDS.CIF_ADDRESS;
            }

            // Fields not supported by the sample
            if ((fields & enum_CONTEXT_INFO_FIELDS.CIF_ADDRESSOFFSET) != 0)
            {
            }
            if ((fields & enum_CONTEXT_INFO_FIELDS.CIF_ADDRESSABSOLUTE) != 0)
            {
            }
            if ((fields & enum_CONTEXT_INFO_FIELDS.CIF_MODULEURL) != 0)
            {
            }
            if ((fields & enum_CONTEXT_INFO_FIELDS.CIF_FUNCTION) != 0)
            {
            }
            if ((fields & enum_CONTEXT_INFO_FIELDS.CIF_FUNCTIONOFFSET) != 0)
            {
            }

            return S_OK;
        }

        /// <summary>
        ///     Adds a specified value to the current context's address to create a new context.
        /// </summary>
        /// <param name="dwCount">The count.</param>
        /// <param name="newAddress">The new address.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int Add(ulong dwCount, out IDebugMemoryContext2 newAddress)
        {
            newAddress = new MonoMemoryAddress(_engine, (uint) dwCount + _address, _documentContext);
            return S_OK;
        }

        /// <summary>
        ///     Subtracts a specified value from the current context's address to create a new context.
        /// </summary>
        /// <param name="dwCount">The count.</param>
        /// <param name="ppMemCxt">The memory context.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int Subtract(ulong dwCount, out IDebugMemoryContext2 ppMemCxt)
        {
            ppMemCxt = new MonoMemoryAddress(_engine, (uint) dwCount - _address, _documentContext);
            return S_OK;
        }

        /// <summary>
        ///     Compares two contexts in the manner indicated by compare flags.
        /// </summary>
        /// <param name="compare">The compare flags.</param>
        /// <param name="compareToItems">The compare to items.</param>
        /// <param name="compareToLength">Length of the compare to.</param>
        /// <param name="foundIndex">Index of the found context.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int Compare(enum_CONTEXT_COMPARE compare, IDebugMemoryContext2[] compareToItems, uint compareToLength,
            out uint foundIndex)
        {
            foundIndex = uint.MaxValue;

            try
            {
                var contextCompare = compare;

                for (uint c = 0; c < compareToLength; c++)
                {
                    var compareTo = compareToItems[c] as MonoMemoryAddress;
                    if (compareTo == null)
                        continue;

                    if (!ReferenceEquals(_engine, compareTo._engine))
                        continue;

                    bool result;

                    switch (contextCompare)
                    {
                        case enum_CONTEXT_COMPARE.CONTEXT_EQUAL:
                            result = _address == compareTo._address;
                            break;

                        case enum_CONTEXT_COMPARE.CONTEXT_LESS_THAN:
                            result = _address < compareTo._address;
                            break;

                        case enum_CONTEXT_COMPARE.CONTEXT_GREATER_THAN:
                            result = _address > compareTo._address;
                            break;

                        case enum_CONTEXT_COMPARE.CONTEXT_LESS_THAN_OR_EQUAL:
                            result = _address <= compareTo._address;
                            break;

                        case enum_CONTEXT_COMPARE.CONTEXT_GREATER_THAN_OR_EQUAL:
                            result = _address >= compareTo._address;
                            break;

                        // The sample debug engine doesn't understand scopes or functions
                        case enum_CONTEXT_COMPARE.CONTEXT_SAME_SCOPE:
                        case enum_CONTEXT_COMPARE.CONTEXT_SAME_FUNCTION:
                            result = _address == compareTo._address;
                            break;

                        case enum_CONTEXT_COMPARE.CONTEXT_SAME_MODULE:
                            result = _address == compareTo._address;
                            // if (!result)
                            // {
                            //     DebuggedModule module = engine.DebuggedProcess.ResolveAddress(address);
                            //     if (module != null)
                            //     {
                            //         result = (compareTo.address >= module.BaseAddress) &&
                            //             (compareTo.address < module.BaseAddress + module.Size);
                            //     }
                            // }
                            break;

                        case enum_CONTEXT_COMPARE.CONTEXT_SAME_PROCESS:
                            result = true;
                            break;

                        default:
                            // A new comparison was invented that we don't support
                            return E_NOTIMPL;
                    }

                    if (result)
                    {
                        foundIndex = c;
                        return S_OK;
                    }
                }

                return S_FALSE;
            }
            catch (ComponentException e)
            {
                return e.HResult;
            }
            catch (Exception e)
            {
                return EngineUtils.UnexpectedException(e);
            }
        }

        #endregion

        #region Implementation of IDebugCodeContext2

        /// <summary>
        ///     Gets the document context that corresponds to the active code context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetDocumentContext(out IDebugDocumentContext2 context)
        {
            context = _documentContext;
            return S_OK;
        }

        /// <summary>
        ///     Gets the language information for this code context.
        /// </summary>
        /// <param name="pbstrLanguage">The language.</param>
        /// <param name="pguidLanguage">The language GUID.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage)
        {
            if (_documentContext != null)
            {
                _documentContext.GetLanguageInfo(ref pbstrLanguage, ref pguidLanguage);
                return S_OK;
            }
            return S_FALSE;
        }

        #endregion
    }
}