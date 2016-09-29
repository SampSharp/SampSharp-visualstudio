using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using Mono.Debugging.Client;
using SampSharp.VisualStudio.Debuggers.Events;

namespace SampSharp.VisualStudio.Debuggers
{
	public class MonoExpression : IDebugExpression2
	{
		private readonly ObjectValue _value;
		private CancellationTokenSource _cancellationToken;

		private readonly MonoEngine _engine;
		private readonly MonoThread _thread;

		public MonoExpression(MonoEngine engine, MonoThread thread, string expression, ObjectValue value)
		{
			_engine = engine;
			_thread = thread;
			_value = value;
			Expression = expression;
		}

		public string Expression { get; }

		public int EvaluateAsync(enum_EVALFLAGS flags, IDebugEventCallback2 callback)
		{
			_cancellationToken = new CancellationTokenSource();
			Task.Run(
				() =>
				{
					IDebugProperty2 result;
					EvaluateSync(flags, uint.MaxValue, callback, out result);
					callback = new MonoCallbackWrapper(callback ?? _engine.Callback);
					callback.Send(_engine, new MonoExpressionCompleteEvent(_engine, _thread, _value, Expression),
						MonoExpressionCompleteEvent.Iid, _thread);
				},
				_cancellationToken.Token);
			return VSConstants.S_OK;
		}

		public int Abort()
		{
			if (_cancellationToken != null)
			{
				_cancellationToken.Cancel();
				_cancellationToken = null;
				return VSConstants.S_OK;
			}
			return VSConstants.S_FALSE;
		}

		public int EvaluateSync(enum_EVALFLAGS flags, uint timeout, IDebugEventCallback2 callback, out IDebugProperty2 result)
		{
			result = new MonoProperty(Expression, _value);
			return VSConstants.S_OK;
		}
	}
}