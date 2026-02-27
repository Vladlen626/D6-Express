using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace PlatformCore.Services
{
	public interface IService : IDisposable
	{
	}

	public interface IAsyncInitializable : IService
	{
		UniTask PreInitializeAsync(CancellationToken ct);
		UniTask PostInitializeAsync(CancellationToken ct);
	}

	public interface ISyncInitializable : IService
	{
		void Initialize();
	}
}
