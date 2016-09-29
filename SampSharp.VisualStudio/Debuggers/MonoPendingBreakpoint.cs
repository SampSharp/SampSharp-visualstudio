using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using Mono.Debugging.Client;
using SampSharp.VisualStudio.Debuggers.Events;
using SampSharp.VisualStudio.Utils;

namespace SampSharp.VisualStudio.Debuggers
{
	public class MonoPendingBreakpoint : IDebugPendingBreakpoint2
	{
		private readonly List<MonoBoundBreakpoint> _boundBreakpoints = new List<MonoBoundBreakpoint>();

		private readonly MonoBreakpointManager _breakpointManager;
		private readonly IDebugBreakpointRequest2 _request;
		private readonly BP_REQUEST_INFO _requestInfo;
		private Breakpoint _breakpoint;
		private bool _isDeleted;
		private bool _isEnabled;

		public MonoPendingBreakpoint(MonoBreakpointManager breakpointManager, IDebugBreakpointRequest2 request)
		{
			_breakpointManager = breakpointManager;
			_request = request;

			var requestInfo = new BP_REQUEST_INFO[1];
			EngineUtils.CheckOk(request.GetRequestInfo(enum_BPREQI_FIELDS.BPREQI_ALLFIELDS, requestInfo));
			_requestInfo = requestInfo[0];
		}

		public MonoBoundBreakpoint[] BoundBreakpoints => _boundBreakpoints.ToArray();

		public int CanBind(out IEnumDebugErrorBreakpoints2 error)
		{
			error = null;
			if (_isDeleted || (_requestInfo.bpLocation.bpLocationType != (uint)enum_BP_LOCATION_TYPE.BPLT_CODE_FILE_LINE))
				return VSConstants.S_FALSE;

			return VSConstants.S_OK;
		}

		public int Bind()
		{
			TEXT_POSITION[] startPosition;
			TEXT_POSITION[] endPosition;
			var engine = _breakpointManager.Engine;
			var documentName = engine.GetLocationInfo(_requestInfo.bpLocation.unionmember2, out startPosition, out endPosition);
			// documentName = engine.TranslateToBuildServerPath(documentName);

			_breakpoint = engine.Session.Breakpoints.Add(documentName, (int)startPosition[0].dwLine + 1,
				(int)startPosition[0].dwColumn + 1);
			_breakpointManager.Add(_breakpoint, this);
			SetCondition(_requestInfo.bpCondition);
			SetPassCount(_requestInfo.bpPassCount);

			// Enable(...) would have already been called before Bind
			if (!_isEnabled)
				_breakpoint.Enabled = false;

			lock (_boundBreakpoints)
			{
				uint address = 0;
				var breakpointResolution = new MonoBreakpointResolution(engine, address, GetDocumentContext(address));
				var boundBreakpoint = new MonoBoundBreakpoint(this, breakpointResolution);
				_boundBreakpoints.Add(boundBreakpoint);

				engine.Send(new MonoBreakpointBoundEvent(this, boundBreakpoint), MonoBreakpointBoundEvent.Iid, null);
			}

			return VSConstants.S_OK;
		}

		public int GetState(PENDING_BP_STATE_INFO[] state)
		{
			if (_isDeleted)
				state[0].state = (enum_PENDING_BP_STATE)enum_BP_STATE.BPS_DELETED;
			else if (_isEnabled)
				state[0].state = (enum_PENDING_BP_STATE)enum_BP_STATE.BPS_ENABLED;
			else
				state[0].state = (enum_PENDING_BP_STATE)enum_BP_STATE.BPS_DISABLED;

			return VSConstants.S_OK;
		}

		public int GetBreakpointRequest(out IDebugBreakpointRequest2 request)
		{
			request = _request;
			return VSConstants.S_OK;
		}

		public int Virtualize(int fVirtualize)
		{
			return VSConstants.S_OK;
		}

		public int Enable(int enable)
		{
			lock (_boundBreakpoints)
			{
				_isEnabled = enable != 0;

				var breakpoint = _breakpoint;
				if (breakpoint != null)
					breakpoint.Enabled = _isEnabled;

				foreach (var boundBreakpoint in _boundBreakpoints)
					boundBreakpoint.Enable(enable);
			}

			return VSConstants.S_OK;
		}

		public int SetCondition(BP_CONDITION condition)
		{
			_breakpoint.ConditionExpression = condition.bstrCondition;
			_breakpoint.BreakIfConditionChanges = condition.styleCondition == enum_BP_COND_STYLE.BP_COND_WHEN_CHANGED;

			return VSConstants.S_OK;
		}

		public int SetPassCount(BP_PASSCOUNT passCount)
		{
			_breakpoint.HitCount = (int)passCount.dwPassCount;
			switch (passCount.stylePassCount)
			{
				case enum_BP_PASSCOUNT_STYLE.BP_PASSCOUNT_EQUAL:
					_breakpoint.HitCountMode = HitCountMode.EqualTo;
					break;
				case enum_BP_PASSCOUNT_STYLE.BP_PASSCOUNT_EQUAL_OR_GREATER:
					_breakpoint.HitCountMode = HitCountMode.GreaterThanOrEqualTo;
					break;
				case enum_BP_PASSCOUNT_STYLE.BP_PASSCOUNT_MOD:
					_breakpoint.HitCountMode = HitCountMode.MultipleOf;
					break;
				default:
					_breakpoint.HitCountMode = HitCountMode.None;
					break;
			}

			return VSConstants.S_OK;
		}

		public int EnumBoundBreakpoints(out IEnumDebugBoundBreakpoints2 enumerator)
		{
			lock (_boundBreakpoints)
			{
				enumerator = new MonoBoundBreakpointsEnum(_boundBreakpoints.OfType<IDebugBoundBreakpoint2>().ToArray());
			}
			return VSConstants.S_OK;
		}

		public int EnumErrorBreakpoints(enum_BP_ERROR_TYPE errorType, out IEnumDebugErrorBreakpoints2 enumerator)
		{
			enumerator = null;
			return VSConstants.S_OK;
		}

		public int Delete()
		{
			if (!_isDeleted)
			{
				_isDeleted = true;
				if (_breakpoint != null)
					_breakpointManager.Remove(_breakpoint);

				lock (_boundBreakpoints)
				{
					for (var i = _boundBreakpoints.Count - 1; i >= 0; i--)
						_boundBreakpoints[i].Delete();
				}
			}
			return VSConstants.S_OK;
		}

		public MonoDocumentContext GetDocumentContext(uint address)
		{
			TEXT_POSITION[] startPosition;
			TEXT_POSITION[] endPosition;
			var documentName = _breakpointManager.Engine.GetLocationInfo(_requestInfo.bpLocation.unionmember2, out startPosition,
				out endPosition);
			var codeContext = new MonoMemoryAddress(_breakpointManager.Engine, address, null);

			return new MonoDocumentContext(documentName, startPosition[0], endPosition[0], codeContext);
		}
	}
}