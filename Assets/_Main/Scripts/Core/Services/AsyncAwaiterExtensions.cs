using Cysharp.Threading.Tasks;

namespace _Main.Scripts.Core.Services
{
	public static class AsyncAwaiterExtensions
	{
		public static UniTask RegisterAwaiter(this UniTask task, IAsyncAwaiterPool pool)
		{
			if (pool == null)
			{
				return task;
			}

			pool.Add();
			return AwaitAndRelease(task, pool);
		}

		public static UniTask<T> RegisterAwaiter<T>(this UniTask<T> task, IAsyncAwaiterPool pool)
		{
			if (pool == null)
			{
				return task;
			}

			pool.Add();
			return AwaitAndRelease(task, pool);
		}

		private static async UniTask AwaitAndRelease(UniTask task, IAsyncAwaiterPool pool)
		{
			try
			{
				await task;
			}
			finally
			{
				pool.Done();
			}
		}

		private static async UniTask<T> AwaitAndRelease<T>(UniTask<T> task, IAsyncAwaiterPool pool)
		{
			try
			{
				return await task;
			}
			finally
			{
				pool.Done();
			}
		}
	}
}
