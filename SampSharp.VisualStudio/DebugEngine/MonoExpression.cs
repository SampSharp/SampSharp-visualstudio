using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Debugger.Interop;
using Mono.Debugging.Client;
using SampSharp.VisualStudio.DebugEngine.Events;
using static Microsoft.VisualStudio.VSConstants;

namespace SampSharp.VisualStudio.DebugEngine
{
    public class MonoExpression : IDebugExpression2
    {
        private readonly MonoEngine _engine;
        private readonly MonoThread _thread;
        private readonly ObjectValue _value;
        private CancellationTokenSource _cancellationToken;

        public MonoExpression(MonoEngine engine, MonoThread thread, string expression, ObjectValue value)
        {
            _engine = engine;
            _thread = thread;
            _value = value;
            Expression = expression;
        }

        public string Expression { get; }

        #region Implementation of IDebugExpression2

        /// <summary>
        ///     Evaluates this expression asynchronously.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <param name="callback">The callback.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int EvaluateAsync(enum_EVALFLAGS flags, IDebugEventCallback2 callback)
        {
            _cancellationToken = new CancellationTokenSource();
            Task.Run(() =>
                {
                    IDebugProperty2 result;
                    EvaluateSync(flags, uint.MaxValue, callback, out result);
                    callback = new MonoCallbackWrapper(callback ?? _engine.Callback);
                    callback.Send(_engine, new MonoExpressionCompleteEvent(_engine, _thread, _value, Expression),
                        MonoExpressionCompleteEvent.Iid, _thread);
                },
                _cancellationToken.Token);
            return S_OK;
        }

        /// <summary>
        ///     Ends asynchronous expression evaluation.
        /// </summary>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int Abort()
        {
            if (_cancellationToken != null)
            {
                _cancellationToken.Cancel();
                _cancellationToken = null;
                return S_OK;
            }
            return S_FALSE;
        }

        /// <summary>
        ///     Evaluates this expression synchronously.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="result">The result.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int EvaluateSync(enum_EVALFLAGS flags, uint timeout, IDebugEventCallback2 callback,
            out IDebugProperty2 result)
        {
            result = new MonoProperty(Expression, _value);
            return S_OK;
        }

        #endregion
    }
}