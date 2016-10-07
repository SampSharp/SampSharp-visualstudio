using System.Collections.Generic;
using Mono.Debugging.Client;
using SampSharp.VisualStudio.DebugEngine;

namespace SampSharp.VisualStudio.Debugger
{
    public class MonoBreakpointManager
    {
        private readonly Dictionary<BreakEvent, MonoPendingBreakpoint> _breakpoints =
            new Dictionary<BreakEvent, MonoPendingBreakpoint>();

        private readonly Dictionary<string, Catchpoint> _catchpoints = new Dictionary<string, Catchpoint>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="MonoBreakpointManager" /> class.
        /// </summary>
        /// <param name="engine">The engine.</param>
        public MonoBreakpointManager(MonoEngine engine)
        {
            Engine = engine;
        }

        /// <summary>
        ///     Gets the engine.
        /// </summary>
        public MonoEngine Engine { get; }

        /// <summary>
        ///     Gets the <see cref="MonoPendingBreakpoint" /> with the specified break event.
        /// </summary>
        /// <param name="breakEvent">The break event.</param>
        public MonoPendingBreakpoint this[BreakEvent breakEvent]
        {
            get
            {
                MonoPendingBreakpoint breakpoint;
                _breakpoints.TryGetValue(breakEvent, out breakpoint);
                return breakpoint;
            }
        }

        /// <summary>
        ///     Gets the <see cref="Catchpoint" /> with the specified exception name.
        /// </summary>
        /// <param name="exceptionName">Name of the exception.</param>
        public Catchpoint this[string exceptionName] => _catchpoints[exceptionName];

        public IEnumerable<Catchpoint> Catchpoints => _catchpoints.Values;

        /// <summary>
        ///     Determines whether the this instance contains the specified catchpoint.
        /// </summary>
        /// <param name="exceptionName">Name of the catchpoint.</param>
        /// <returns>
        ///     <c>true</c> if this instacne contains the specified catchpoint; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsCatchpoint(string exceptionName) => _catchpoints.ContainsKey(exceptionName);

        /// <summary>
        ///     Adds the specified break event.
        /// </summary>
        /// <param name="breakEvent">The break event.</param>
        /// <param name="pendingBreakpoint">The pending breakpoint.</param>
        public void Add(BreakEvent breakEvent, MonoPendingBreakpoint pendingBreakpoint)
        {
            _breakpoints[breakEvent] = pendingBreakpoint;
        }

        /// <summary>
        ///     Removes the specified break event.
        /// </summary>
        /// <param name="breakEvent">The break event.</param>
        public void Remove(BreakEvent breakEvent)
        {
            Engine.Program.Session.Breakpoints.Remove(breakEvent);
            _breakpoints.Remove(breakEvent);
        }

        /// <summary>
        ///     Adds the specified catchpoint.
        /// </summary>
        /// <param name="catchpoint">The catchpoint.</param>
        public void Add(Catchpoint catchpoint)
        {
            _catchpoints[catchpoint.ExceptionName] = catchpoint;
        }

        /// <summary>
        ///     Removes the specified catchpoint.
        /// </summary>
        /// <param name="catchpoint">The catchpoint.</param>
        public void Remove(Catchpoint catchpoint)
        {
            _catchpoints.Remove(catchpoint.ExceptionName);
        }
    }
}