using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace _Main.Scripts.Core.Services
{
	/// <summary>
	/// Default implementation that stores named awaiter pools.
	/// </summary>
	public sealed class AsyncAwaiterService : IAsyncAwaiterService
	{
		private readonly Dictionary<string, UniTaskAwaiterPool> pools = new();
		private readonly UniTaskAwaiterPool defaultPool = new();

		/// <summary>Returns (or creates) a pool for the given id. Empty id returns default pool.</summary>
		public IAsyncAwaiterPool GetPool(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				return defaultPool;
			}

			if (!pools.TryGetValue(id, out var pool))
			{
				pool = new UniTaskAwaiterPool();
				pools.Add(id, pool);
			}

			return pool;
		}

		/// <summary>Clears all cached pools.</summary>
		public void Dispose()
		{
			pools.Clear();
		}
	}

	/// <summary>
	/// Thread-safe counter-based awaiter pool. Add increments the counter,
	/// Done decrements it, and WaitForEmptyAsync completes when counter reaches zero.
	/// </summary>
	public sealed class UniTaskAwaiterPool : IAsyncAwaiterPool
	{
		private int pending;
		private UniTaskCompletionSource emptyTasks = new();

		/// <summary>Current number of pending operations.</summary>
		public int PendingCount => Volatile.Read(ref pending);

		/// <summary>Registers a pending operation.</summary>
		public void Add()
		{
			Interlocked.Increment(ref pending);
		}

		/// <summary>Marks a pending operation as completed.</summary>
		public void Done()
		{
			int remaining = Interlocked.Decrement(ref pending);
			if (remaining <= 0)
			{
				Interlocked.Exchange(ref pending, 0);
				var task = emptyTasks;
				emptyTasks = new UniTaskCompletionSource();
				task.TrySetResult();
			}
		}

		/// <summary>Completes when the pool becomes empty.</summary>
		public UniTask WaitForEmptyAsync()
		{
			var task = emptyTasks;
			if (PendingCount == 0)
			{
				return UniTask.CompletedTask;
			}

			return task.Task;
		}
	}
}
