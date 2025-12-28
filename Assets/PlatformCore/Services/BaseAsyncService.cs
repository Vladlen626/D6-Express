using System.Threading;
using Cysharp.Threading.Tasks;

public abstract class BaseAsyncService : IAsyncInitializable
{
	protected virtual UniTask OnPreInitializeAsync(CancellationToken ct) => UniTask.CompletedTask;
	protected virtual UniTask OnPostInitializeAsync(CancellationToken ct) => UniTask.CompletedTask;

	public async UniTask PreInitializeAsync(CancellationToken ct) => await OnPreInitializeAsync(ct);
	public async UniTask PostInitializeAsync(CancellationToken ct) => await OnPostInitializeAsync(ct);

	public virtual void Dispose() { }
}