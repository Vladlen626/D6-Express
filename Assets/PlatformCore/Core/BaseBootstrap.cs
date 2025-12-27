using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PlatformCore.Core
{
	[RequireComponent(typeof(PersistentSceneContext))]
	public abstract class BaseBootstrap : MonoBehaviour
	{
#if UNITY_EDITOR
		public BaseGameRoot GameRoot => _gameRoot;
#endif
		private PersistentSceneContext _persistentSceneContext;
		private BaseGameRoot _gameRoot;
		private bool _initialized;

		private void Awake()
		{
			if (_initialized)
			{
				return;
			}

			_persistentSceneContext = GetComponent<PersistentSceneContext>();

			_gameRoot = CreateGameRoot();
			_gameRoot.LaunchAsync(_persistentSceneContext).Forget();

			_initialized = true;
		}

		protected abstract BaseGameRoot CreateGameRoot();

		private void Update()
		{
			if (_gameRoot != null)
			{
				_gameRoot.OnUpdate(Time.deltaTime);
			}
		}

		private void FixedUpdate()
		{
			if (_gameRoot != null)
			{
				_gameRoot.OnFixedUpdate(Time.fixedDeltaTime);
			}
		}

		private void LateUpdate()
		{
			if (_gameRoot != null)
			{
				_gameRoot.OnLateUpdate(Time.deltaTime);
			}
		}

		private void OnDestroy()
		{
			_gameRoot?.Dispose();
		}
	}
}