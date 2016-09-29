using System.Collections.Generic;
using Mono.Debugging.Client;

namespace SampSharp.VisualStudio.Debuggers
{
	public class MonoThreadManager
	{
		private readonly Dictionary<long, MonoThread> _threads = new Dictionary<long, MonoThread>();

		public MonoThreadManager(MonoEngine engine)
		{
			Engine = engine;
		}

		public MonoEngine Engine { get; }

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

		public IEnumerable<MonoThread> All => _threads.Values;

		public void Add(ThreadInfo thread, MonoThread monoThread)
		{
			_threads[thread.Id] = monoThread;
		}

		public void Remove(ThreadInfo thread)
		{
			_threads.Remove(thread.Id);
		}
	}
}