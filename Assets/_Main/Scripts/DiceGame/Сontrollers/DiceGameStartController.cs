using _Main.Scripts.Core;
using _Main.Scripts.Core.Services;
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
			playerModel.OnCharacterStateChanged += OnCharacterStateChangedHandler;
		}

		public void Deactivate()
		{
			playerModel.OnCharacterStateChanged -= OnCharacterStateChangedHandler;
		}

		private void OnCharacterStateChangedHandler(CharacterState oldCharacterState, CharacterState newCharacterState)
		{
			if (newCharacterState == CharacterState.DICE_GAME)
			{
				StartDiceGame().Forget();
			}
			else
			{
				StopDiceGame();
			}
		}

		private async UniTask StartDiceGame()
		{
			// TODO: Перенести в конфиги.
			int targetScore = 4000;
			int betSize = 200;
			int maxTurnCount = 10;

			diceGameModel.Reset();
			diceGameModel.SetTargetScore(targetScore);
			diceGameModel.SetBetSize(betSize);
			diceGameModel.SetMaxTurnCount(maxTurnCount);

			var objectFactory = serviceLocator.Get<IObjectFactory>();
			var loggerService = serviceLocator.Get<ILoggerService>();
			
			gameControllers = await DiceFactory.GetDiceGameControllers(sceneContext, objectFactory, loggerService,
				diceGameModel);

			foreach (var controller in gameControllers)
			{
				await lifecycleService.RegisterAsync(controller);
			}
		}

		private void StopDiceGame()
		{
			if (gameControllers == null)
			{
				return;
			}

			foreach (var controller in gameControllers)
			{
				lifecycleService.Unregister(controller);
			}

			gameControllers = null;
		}
	}
}