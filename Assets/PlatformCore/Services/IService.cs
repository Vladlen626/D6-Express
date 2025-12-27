using System;
using System.Threading;
using Cysharp.Threading.Tasks;

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

public interface IPlayerLocalService : IDisposable
{
	UniTask InitAsync(CancellationToken ct);
}