using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using Mono.Debugging.Client;
using SampSharp.VisualStudio.Utils;
using StackFrame = Mono.Debugging.Client.StackFrame;

namespace SampSharp.VisualStudio.Debuggers
{
	public class MonoStackFrame : IDebugStackFrame2, IDebugExpressionContext2
	{
		private readonly string _documentName;
		private readonly MonoEngine _engine;
		private readonly Func<StackFrame> _frame;
		private readonly string _functionName;
		private readonly bool _hasSource;

		// An array of this frame's locals
		private readonly ObjectValue[] _locals;

		// An array of this frame's parameters
		private readonly ObjectValue[] _parameters;
		private readonly MonoThread _thread;
		private int? _lineNumberOverride;

		public MonoStackFrame(MonoEngine engine, MonoThread thread, Func<StackFrame> frame)
		{
			_engine = engine;
			_thread = thread;
			_frame = frame;

			var allLocals = frame().GetAllLocals(EvaluationOptions.DefaultOptions);
			_parameters = frame().GetParameters(EvaluationOptions.DefaultOptions);
			_locals = allLocals.Where(x => _parameters.All(y => y.Name != x.Name)).ToArray();
			_hasSource = frame().HasDebugInfo;
			_functionName = frame().SourceLocation.MethodName;
			_documentName = frame().SourceLocation.FileName;
		}

		public int LineNumber
		{
			get { return _lineNumberOverride ?? _frame().SourceLocation.Line; }
			set { _lineNumberOverride = value; }
		}

		// Retrieves the name of the evaluation context. 
		// The name is the description of this evaluation context. It is typically something that can be parsed by an expression evaluator 
		// that refers to this exact evaluation context. For example, in C++ the name is as follows: 
		// "{ function-name, source-file-name, module-file-name }"
		int IDebugExpressionContext2.GetName(out string pbstrName)
		{
			throw new NotImplementedException();
		}

		// Parses a text-based expression for evaluation.
		// The engine sample only supports locals and parameters so the only task here is to check the names in those collections.
		int IDebugExpressionContext2.ParseText(string code,
			enum_PARSEFLAGS dwFlags,
			uint nRadix,
			out IDebugExpression2 expression,
			out string error,
			out uint pichError)
		{
			var frame = _frame();
			error = null;
			pichError = 0;
			try
			{
				if (frame.ValidateExpression(code))
				{
					var value = frame.GetExpressionValue(code, EvaluationOptions.DefaultOptions);
					expression = new MonoExpression(_engine, _thread, code, value);
					return VSConstants.S_OK;
				}
				expression = null;
				return VSConstants.S_FALSE;
			}
			catch (Exception e)
			{
				expression = null;
				Debug.WriteLine("Unexpected exception during Attach: \r\n" + e);
				return VSConstants.S_FALSE;
			}
		}

        /// <summary>
        /// Creates an enumerator for properties associated with the stack frame, such as local variables.
        /// The sample engine only supports returning locals and parameters. Other possible values include
        /// class fields (this pointer), registers, exceptions...
        /// </summary>
        /// <param name="dwFields"></param>
        /// <param name="nRadix"></param>
        /// <param name="guidFilter"></param>
        /// <param name="dwTimeout"></param>
        /// <param name="elementsReturned"></param>
        /// <param name="enumObject"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        int IDebugStackFrame2.EnumProperties(enum_DEBUGPROP_INFO_FLAGS dwFields, uint nRadix, ref Guid guidFilter,
			uint dwTimeout, out uint elementsReturned, out IEnumDebugPropertyInfo2 enumObject)
		{
			int hr;

			elementsReturned = 0;
			enumObject = null;

			try
			{
				if ((guidFilter == DebuggerGuids.GuidFilterLocalsPlusArgs) ||
				    (guidFilter == DebuggerGuids.GuidFilterAllLocalsPlusArgs) ||
				    (guidFilter == DebuggerGuids.GuidFilterAllLocals))
				{
					CreateLocalsPlusArgsProperties(out elementsReturned, out enumObject);
					hr = VSConstants.S_OK;
				}
				else if (guidFilter == DebuggerGuids.GuidFilterLocals)
				{
					CreateLocalProperties(out elementsReturned, out enumObject);
					hr = VSConstants.S_OK;
				}
				else if (guidFilter == DebuggerGuids.GuidFilterArgs)
				{
					CreateParameterProperties(out elementsReturned, out enumObject);
					hr = VSConstants.S_OK;
				}
				else
				{
					hr = VSConstants.E_NOTIMPL;
				}
			}
			catch (ComponentException e)
			{
				return e.HResult;
			}
			catch (Exception e)
			{
				return EngineUtils.UnexpectedException(e);
			}

			return hr;
		}

		// Gets the code context for this stack frame. The code context represents the current instruction pointer in this stack frame.
		int IDebugStackFrame2.GetCodeContext(out IDebugCodeContext2 memoryAddress)
		{
			memoryAddress = null;

			try
			{
				memoryAddress = new MonoMemoryAddress(_engine, (uint)_frame().Address, null);
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

		// Gets a description of the properties of a stack frame.
		// Calling the IDebugProperty2::EnumChildren method with appropriate filters can retrieve the local variables, method parameters, registers, and "this" 
		// pointer associated with the stack frame. The debugger calls EnumProperties to obtain these values in the sample.
		int IDebugStackFrame2.GetDebugProperty(out IDebugProperty2 property)
		{
			throw new NotImplementedException();
		}

		// Gets the document context for this stack frame. The debugger will call this when the current stack frame is changed
		// and will use it to open the correct source document for this stack frame.
		int IDebugStackFrame2.GetDocumentContext(out IDebugDocumentContext2 docContext)
		{
			docContext = null;
			try
			{
				if (_hasSource)
				{
					// Assume all lines begin and end at the beginning of the line.
					// TODO: Accurate line endings
					var lineNumber = (uint)LineNumber;
				    var begTp = new TEXT_POSITION
				    {
				        dwColumn = 0,
				        dwLine = lineNumber - 1
				    };
				    var endTp = new TEXT_POSITION
				    {
				        dwColumn = 0,
				        dwLine = lineNumber - 1
				    };

				    docContext = new MonoDocumentContext(_documentName, begTp, endTp, null);
					return VSConstants.S_OK;
				}
			}
			catch (ComponentException e)
			{
				return e.HResult;
			}
			catch (Exception e)
			{
				return EngineUtils.UnexpectedException(e);
			}

			return VSConstants.S_FALSE;
		}

		// Gets an evaluation context for expression evaluation within the current context of a stack frame and thread.
		// Generally, an expression evaluation context can be thought of as a scope for performing expression evaluation. 
		// Call the IDebugExpressionContext2::ParseText method to parse an expression and then call the resulting IDebugExpression2::EvaluateSync 
		// or IDebugExpression2::EvaluateAsync methods to evaluate the parsed expression.
		int IDebugStackFrame2.GetExpressionContext(out IDebugExpressionContext2 ppExprCxt)
		{
			ppExprCxt = this;
			return VSConstants.S_OK;
		}

		// Gets a description of the stack frame.
		int IDebugStackFrame2.GetInfo(enum_FRAMEINFO_FLAGS dwFieldSpec, uint nRadix, FRAMEINFO[] pFrameInfo)
		{
			try
			{
				SetFrameInfo(dwFieldSpec, out pFrameInfo[0]);

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

		// Gets the language associated with this stack frame. 
		// In this sample, all the supported stack frames are C++
		int IDebugStackFrame2.GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage)
		{
			pbstrLanguage = "C#";
			pguidLanguage = DebuggerGuids.CSharpLanguageService;
			return VSConstants.S_OK;
		}

		// Gets the name of the stack frame.
		// The name of a stack frame is typically the name of the method being executed.
		int IDebugStackFrame2.GetName(out string name)
		{
			name = _frame().SourceLocation.MethodName;
			return VSConstants.S_OK;
		}

		// Gets a machine-dependent representation of the range of physical addresses associated with a stack frame.
		int IDebugStackFrame2.GetPhysicalStackRange(out ulong addrMin, out ulong addrMax)
		{
			addrMin = 0;
			addrMax = 0;
			return VSConstants.S_OK;
		}

		// Gets the thread associated with a stack frame.
		int IDebugStackFrame2.GetThread(out IDebugThread2 thread)
		{
			thread = _thread;
			return VSConstants.S_OK;
		}

		// Construct a FRAMEINFO for this stack frame with the requested information.
		public void SetFrameInfo(enum_FRAMEINFO_FLAGS dwFieldSpec, out FRAMEINFO frameInfo)
		{
			frameInfo = new FRAMEINFO();
			var frame = _frame();

			// The debugger is asking for the formatted name of the function which is displayed in the callstack window.
			// There are several optional parts to this name including the module, argument types and values, and line numbers.
			// The optional information is requested by setting flags in the dwFieldSpec parameter.
			if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_FUNCNAME) != 0)
			{
				// If there is source information, construct a string that contains the module name, function name, and optionally argument names and values.
				if (_hasSource)
				{
					frameInfo.m_bstrFuncName = "";

					if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_MODULE) != 0)
						frameInfo.m_bstrFuncName = Path.GetFileName(frame.FullModuleName) + "!";

					frameInfo.m_bstrFuncName += _functionName;

					if (((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_ARGS) != 0) && (_parameters.Length > 0))
					{
						frameInfo.m_bstrFuncName += "(";
						for (var i = 0; i < _parameters.Length; i++)
						{
							if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_ARGS_TYPES) != 0)
								frameInfo.m_bstrFuncName += _parameters[i].TypeName + " ";

							if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_ARGS_NAMES) != 0)
								frameInfo.m_bstrFuncName += _parameters[i].Name;

							if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_ARGS_VALUES) != 0)
								frameInfo.m_bstrFuncName += "=" + _parameters[i].Value;

							if (i < _parameters.Length - 1)
								frameInfo.m_bstrFuncName += ", ";
						}
						frameInfo.m_bstrFuncName += ")";
					}

					if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_LINES) != 0)
						frameInfo.m_bstrFuncName += " Line:" + (uint)LineNumber;
				}
				else
				{
					// No source information, so only return the module name and the instruction pointer.
					frameInfo.m_bstrFuncName = frame.AddressSpace;
				}
				frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_FUNCNAME;
			}

			// The debugger is requesting the name of the module for this stack frame.
			if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_MODULE) != 0)
			{
				frameInfo.m_bstrModule = frame.FullModuleName;
				frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_MODULE;
			}

			// The debugger is requesting the range of memory addresses for this frame.
			// For the sample engine, this is the contents of the frame pointer.
			if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_STACKRANGE) != 0)
			{
				frameInfo.m_addrMin = (ulong)frame.Address;
				frameInfo.m_addrMax = (ulong)frame.Address;
				frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_STACKRANGE;
			}

			// The debugger is requesting the IDebugStackFrame2 value for this frame info.
			if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_FRAME) != 0)
			{
				frameInfo.m_pFrame = this;
				frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_FRAME;
			}

			// Does this stack frame of symbols loaded?
			if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_DEBUGINFO) != 0)
			{
				frameInfo.m_fHasDebugInfo = _hasSource ? 1 : 0;
				frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_DEBUGINFO;
			}

			// Is this frame stale?
			if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_STALECODE) != 0)
			{
				frameInfo.m_fStaleCode = 0;
				frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_STALECODE;
			}

			// The debugger would like a pointer to the IDebugModule2 that contains this stack frame.
			if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_DEBUG_MODULEP) != 0)
			{
/*
												                if (module != null)
												                {
												                    AD7Module ad7Module = (AD7Module)module.Client;
												                    Debug.Assert(ad7Module != null);
												                    frameInfo.m_pModule = ad7Module;
												                    frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_DEBUG_MODULEP;
												                }
												*/
			}
		}

		// Construct an instance of IEnumDebugPropertyInfo2 for the combined locals and parameters.
		private void CreateLocalsPlusArgsProperties(out uint elementsReturned, out IEnumDebugPropertyInfo2 enumObject)
		{
			elementsReturned = 0;

			var localsLength = 0;

			if (_locals != null)
			{
				localsLength = _locals.Length;
				elementsReturned += (uint)localsLength;
			}

			if (_parameters != null)
				elementsReturned += (uint)_parameters.Length;
			var propInfo = new DEBUG_PROPERTY_INFO[elementsReturned];

			if (_locals != null)
				for (var i = 0; i < _locals.Length; i++)
				{
					var property = new MonoProperty(_locals[i].Name, _locals[i]);
					propInfo[i] = property.ConstructDebugPropertyInfo(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_STANDARD);
				}

			if (_parameters != null)
				for (var i = 0; i < _parameters.Length; i++)
				{
					var property = new MonoProperty(_parameters[i].Name, _parameters[i]);
					propInfo[localsLength + i] = property.ConstructDebugPropertyInfo(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_STANDARD);
				}

			enumObject = new MonoPropertyInfoEnum(propInfo);
		}

		// Construct an instance of IEnumDebugPropertyInfo2 for the locals collection only.
		private void CreateLocalProperties(out uint elementsReturned, out IEnumDebugPropertyInfo2 enumObject)
		{
			elementsReturned = (uint)_locals.Length;
			var propInfo = new DEBUG_PROPERTY_INFO[_locals.Length];

			for (var i = 0; i < propInfo.Length; i++)
			{
				var property = new MonoProperty(_locals[i].Name, _locals[i]);
				propInfo[i] = property.ConstructDebugPropertyInfo(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_STANDARD);
			}

			enumObject = new MonoPropertyInfoEnum(propInfo);
		}

		// Construct an instance of IEnumDebugPropertyInfo2 for the parameters collection only.
		private void CreateParameterProperties(out uint elementsReturned, out IEnumDebugPropertyInfo2 enumObject)
		{
			elementsReturned = (uint)_parameters.Length;
			var propInfo = new DEBUG_PROPERTY_INFO[_parameters.Length];

			for (var i = 0; i < propInfo.Length; i++)
			{
				var property = new MonoProperty(_parameters[i].Name, _parameters[i]);
				propInfo[i] = property.ConstructDebugPropertyInfo(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_STANDARD);
			}

			enumObject = new MonoPropertyInfoEnum(propInfo);
		}
	}
}