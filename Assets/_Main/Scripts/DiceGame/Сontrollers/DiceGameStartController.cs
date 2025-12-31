using _Main.Scripts.Core;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services;
using PlatformCore.Services.Factory;

namespace _Main.Scripts.Dice
{
	public class DiceGameStartController : IBaseController, IActivatable
	{
		private readonly DiceGameModel diceGameModel;
		private readonly PlayerModel playerModel;
		private readonly SceneContext sceneContext;
		private readonly ServiceLocator serviceLocator;
		private readonly LifecycleService lifecycleService;

		private IBaseController[] gameControllers;

		public DiceGameStartController(DiceGameModel diceGameModel, PlayerModel playerModel, SceneContext sceneContext,
			ServiceLocator serviceLocator)
		{
			this.diceGameModel = diceGameModel;
			this.playerModel = playerModel;
			this.sceneContext = sceneContext;
			this.serviceLocator =  serviceLocator;
			lifecycleService = serviceLocator.Get<LifecycleService>();
		}

		public void Activate()
		{
			throw new System.NotImplementedException();
		}
		public void Deactivate()
		{
			throw new System.NotImplementedException();
		}


		private async UniTask StartDiceGame()
		{
			gameControllers = await DiceFactory.GetDiceGameControllers(sceneContext, serviceLocator.Get<IObjectFactory>(), 
				serviceLocator.Get<ILoggerService>(), diceGameModel);
			
			foreach (var controller in gameControllers)
			{
				await lifecycleService.RegisterAsync(controller);
			}
		}

		private void StopDiceGame()
		{
			foreach (var controller in gameControllers)
			{
				lifecycleService.Unregister(controller);
			}

			gameControllers = null;
		}
	}
}