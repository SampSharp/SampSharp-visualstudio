using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using SampSharp.VisualStudio.Utils;

namespace SampSharp.VisualStudio.Debuggers
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

		public int GetDocument(out IDebugDocument2 document)
		{
			document = null;
			return VSConstants.E_FAIL;
		}

		public int GetName(enum_GETNAME_TYPE gnType, out string pbstrFileName)
		{
			pbstrFileName = _fileName;
			return VSConstants.S_OK;
		}

		public int EnumCodeContexts(out IEnumDebugCodeContexts2 ppEnumCodeCxts)
		{
			ppEnumCodeCxts = null;
			try
			{
				var codeContexts = new IDebugCodeContext2[1];
				codeContexts[0] = _address;
				ppEnumCodeCxts = new MonoCodeContextEnum(codeContexts);
				return VSConstants.S_OK;
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

		public int GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage)
		{
			pbstrLanguage = "C#";
			pguidLanguage = new Guid("{694DD9B6-B865-4C5B-AD85-86356E9C88DC}");
			return VSConstants.S_OK;
		}

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

			return VSConstants.S_OK;
		}

		public int GetSourceRange(TEXT_POSITION[] pBegPosition, TEXT_POSITION[] pEndPosition)
		{
			throw new NotImplementedException("This method is not implemented");
		}

		public int Compare(enum_DOCCONTEXT_COMPARE compare, IDebugDocumentContext2[] rgpDocContextSet, uint dwDocContextSetLen,
			out uint pdwDocContext)
		{
			pdwDocContext = 0;
			return VSConstants.E_NOTIMPL;
		}

		public int Seek(int nCount, out IDebugDocumentContext2 ppDocContext)
		{
			ppDocContext = null;
			return VSConstants.E_NOTIMPL;
		}
	}
}