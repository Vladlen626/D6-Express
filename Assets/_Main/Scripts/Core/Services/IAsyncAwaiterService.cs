using Cysharp.Threading.Tasks;
using PlatformCore.Services;

namespace _Main.Scripts.Core.Services
{
	/// <summary>
	/// Lightweight counter-based awaiter pool. Use Add/Done to track async work
	/// and WaitForEmptyAsync to await completion of all pending items.
	/// </summary>
	public interface IAsyncAwaiterPool
	{
		/// <summary>Current number of pending operations.</summary>
		int PendingCount { get; }
		/// <summary>Registers a pending operation.</summary>
		void Add();
		/// <summary>Marks a pending operation as completed.</summary>
		void Done();
		/// <summary>Completes when the pool becomes empty.</summary>
		UniTask WaitForEmptyAsync();
	}

	/// <summary>
	/// Provides named awaiter pools for coordinating async flows across systems.
	/// </summary>
	public interface IAsyncAwaiterService : IService
	{
		/// <summary>Returns (or creates) a pool for the given id. Empty id returns default pool.</summary>
		IAsyncAwaiterPool GetPool(string id);
	}
}
