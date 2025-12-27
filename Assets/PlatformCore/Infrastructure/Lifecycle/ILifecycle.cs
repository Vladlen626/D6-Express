using Cysharp.Threading.Tasks;

namespace PlatformCore.Infrastructure.Lifecycle
{
	public interface IPreloadable : IDeactivatable
	{
		UniTask PreloadAsync();
	}
	
	public interface IActivatable : IDeactivatable
	{
		void Activate();
	}
	
	public interface IDeactivatable
	{
		void Deactivate();
	}
	
	public interface IUpdatable
	{
		void OnUpdate(float deltaTime);
	}

	public interface ILateUpdatable
	{
		void OnLateUpdate(float deltaTime);
	}

	public interface IFixedUpdatable
	{
		void OnFixedUpdate(float fixedDeltaTime);
	}
}