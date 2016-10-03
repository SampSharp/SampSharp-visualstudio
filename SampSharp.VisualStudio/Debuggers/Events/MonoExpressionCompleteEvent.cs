using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using Mono.Debugging.Client;

namespace SampSharp.VisualStudio.Debuggers.Events
{
    public class MonoExpressionCompleteEvent : AsynchronousEvent, IDebugExpressionEvaluationCompleteEvent2
    {
        public const string Iid = "C0E13A85-238A-4800-8315-D947C960A843";

        private readonly MonoEngine _engine;
        private readonly string _expression;
        private readonly IDebugProperty2 _property;
        private readonly MonoThread _thread;
        private readonly ObjectValue _value;

        public MonoExpressionCompleteEvent(MonoEngine engine, MonoThread thread, ObjectValue value, string expression,
            IDebugProperty2 property = null)
        {
            _engine = engine;
            _thread = thread;
            _value = value;
            _expression = expression;
            _property = property;
        }

        #region Implementation of IDebugExpressionEvaluationCompleteEvent2

        /// <summary>
        ///     Gets the original expression.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetExpression(out IDebugExpression2 expr)
        {
            expr = new MonoExpression(_engine, _thread, _expression, _value);
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Gets the result of expression evaluation.
        /// </summary>
        /// <param name="prop"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetResult(out IDebugProperty2 prop)
        {
            prop = _property ?? new MonoProperty(_expression, _value);
            return VSConstants.S_OK;
        }

        #endregion
    }
}