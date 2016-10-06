using System.Collections.Generic;
using Mono.Debugging.Client;
using SampSharp.VisualStudio.DebugEngine;

namespace SampSharp.VisualStudio.Debugger
{
    public class MonoThreadManager
    {
        private readonly Dictionary<long, MonoThread> _threads = new Dictionary<long, MonoThread>();

        public MonoThreadManager(MonoEngine engine)
        {
            Engine = engine;
        }

        /// <summary>
        ///     Gets the engine.
        /// </summary>
        public MonoEngine Engine { get; }

        /// <summary>
        ///     Gets the <see cref="MonoThread" /> with the specified thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        public MonoThread this[ThreadInfo thread]
        {
            get
            {
                MonoThread result;
                if (_threads.TryGetValue(thread.Id, out result))
                    result.SetDebuggedThread(thread);
                return result;
            }
        }

        /// <summary>
        ///     Gets all threads.
        /// </summary>
        public IEnumerable<MonoThread> All => _threads.Values;

        /// <summary>
        ///     Adds the specified thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <param name="monoThread">The mono thread.</param>
        public void Add(ThreadInfo thread, MonoThread monoThread)
        {
            _threads[thread.Id] = monoThread;
        }

        /// <summary>
        ///     Removes the specified thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        public void Remove(ThreadInfo thread)
        {
            _threads.Remove(thread.Id);
        }
    }
}