using System;
using Microsoft.VisualStudio.Debugger.Interop;
using SampSharp.VisualStudio.DebugEngine.Enumerators;
using SampSharp.VisualStudio.Utils;
using static Microsoft.VisualStudio.VSConstants;

namespace SampSharp.VisualStudio.DebugEngine
{
    public class MonoDocumentContext : IDebugDocumentContext2
    {
        private readonly MonoMemoryAddress _address;
        private readonly TEXT_POSITION _end;
        private readonly string _fileName;
        private readonly TEXT_POSITION _start;

        public MonoDocumentContext(string fileName, TEXT_POSITION start, TEXT_POSITION end, MonoMemoryAddress address)
        {
            _fileName = fileName;
            _start = start;
            _end = end;
            _address = address;
        }

        #region Implementation of IDebugDocumentContext2

        /// <summary>
        ///     Gets the document that contains this document context.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetDocument(out IDebugDocument2 document)
        {
            document = null;
            return E_FAIL;
        }

        /// <summary>
        ///     Gets the displayable name of the document that contains this document context.
        /// </summary>
        /// <param name="gnType">Type of the name.</param>
        /// <param name="pbstrFileName">The name.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetName(enum_GETNAME_TYPE gnType, out string pbstrFileName)
        {
            pbstrFileName = _fileName;
            return S_OK;
        }

        /// <summary>
        ///     Retrieves a list of all code contexts associated with this document context.
        /// </summary>
        /// <param name="ppEnumCodeCxts">The enumerator.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int EnumCodeContexts(out IEnumDebugCodeContexts2 ppEnumCodeCxts)
        {
            ppEnumCodeCxts = null;
            try
            {
                var codeContexts = new IDebugCodeContext2[1];
                codeContexts[0] = _address;
                ppEnumCodeCxts = new MonoCodeContextEnumerator(codeContexts);
                return S_OK;
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

        /// <summary>
        ///     Gets the language associated with this document context.
        /// </summary>
        /// <param name="pbstrLanguage">The languag name.</param>
        /// <param name="pguidLanguage">The language GUID.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage)
        {
            pbstrLanguage = "C#";
            pguidLanguage = new Guid("{694DD9B6-B865-4C5B-AD85-86356E9C88DC}");
            return S_OK;
        }

        /// <summary>
        ///     Gets the file statement range of this document context.
        /// </summary>
        /// <param name="pBegPosition">The begin position.</param>
        /// <param name="pEndPosition">The end position.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetStatementRange(TEXT_POSITION[] pBegPosition, TEXT_POSITION[] pEndPosition)
        {
            try
            {
                pBegPosition[0].dwColumn = _start.dwColumn;
                pBegPosition[0].dwLine = _start.dwLine;

                pEndPosition[0].dwColumn = _end.dwColumn;
                pEndPosition[0].dwLine = _end.dwLine;
            }
            catch (ComponentException e)
            {
                return e.HResult;
            }
            catch (Exception e)
            {
                return EngineUtils.UnexpectedException(e);
            }

            return S_OK;
        }

        /// <summary>
        ///     Gets the file source range of this document context..
        /// </summary>
        /// <param name="pBegPosition">The begin position.</param>
        /// <param name="pEndPosition">The end position.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetSourceRange(TEXT_POSITION[] pBegPosition, TEXT_POSITION[] pEndPosition)
        {
            throw new NotImplementedException("This method is not implemented");
        }

        /// <summary>
        ///     Compares this document context to a given array of document contexts.
        /// </summary>
        /// <param name="compare">The compare.</param>
        /// <param name="rgpDocContextSet">The document context.</param>
        /// <param name="dwDocContextSetLen">Length of the dw document context.</param>
        /// <param name="pdwDocContext">The document context.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int Compare(enum_DOCCONTEXT_COMPARE compare, IDebugDocumentContext2[] rgpDocContextSet,
            uint dwDocContextSetLen,
            out uint pdwDocContext)
        {
            pdwDocContext = 0;
            return E_NOTIMPL;
        }

        /// <summary>
        ///     Moves the document context by a given number of statements or lines.
        /// </summary>
        /// <param name="nCount">The count.</param>
        /// <param name="ppDocContext">The document context.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int Seek(int nCount, out IDebugDocumentContext2 ppDocContext)
        {
            ppDocContext = null;
            return E_NOTIMPL;
        }

        #endregion
    }
}