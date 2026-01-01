using System.Collections.Generic;
using _Main.Scripts.Core;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services;
using PlatformCore.Services.Factory;

namespace _Main.Scripts.Dice
{
	public class DiceGameGlobalController : IBaseController, IActivatable
	{
		private readonly DiceGameModel diceGameModel;
		private readonly PlayerModel playerModel;

		private readonly IObjectFactory objectFactory;
		private readonly ILoggerService loggerService;
		private readonly LifecycleService lifecycleService;
		
		private readonly SceneContext sceneContext;
		private readonly DicePositionsHandler dicePositionsHandler;

		private readonly List<DiceModel> diceModelsList = new ();
		private DiceView[] diceViewsArray;
		private TableModel tableModel;

		private List<IBaseController> gameControllers = new ();

		public DiceGameGlobalController(DiceGameModel diceGameModel, PlayerModel playerModel, SceneContext sceneContext,
			ServiceLocator serviceLocator)
		{
			this.diceGameModel = diceGameModel;
			this.playerModel = playerModel;
			this.sceneContext = sceneContext;
			dicePositionsHandler = sceneContext.DiceGameTableView.DicePositionsHandler;
			lifecycleService = serviceLocator.Get<LifecycleService>();
			objectFactory = serviceLocator.Get<IObjectFactory>();
			loggerService = serviceLocator.Get<ILoggerService>();
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
			else if (oldCharacterState == CharacterState.DICE_GAME)
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

			diceGameModel.SetTargetScore(targetScore);
			diceGameModel.SetBetSize(betSize);
			diceGameModel.SetMaxTurnCount(maxTurnCount);

			tableModel = new TableModel(dicePositionsHandler.DicePositions, dicePositionsHandler.BankedPositions);

			await SetupDiceForGame(tableModel);

			gameControllers.AddRange(DiceFactory.GetDiceGameControllers(sceneContext, loggerService,
				diceGameModel, tableModel ,diceModelsList));

			foreach (var controller in gameControllers)
			{
				await lifecycleService.RegisterAsync(controller);
			}
		}

		private async UniTask SetupDiceForGame(TableModel tableModel)
		{
			diceViewsArray =
				await DiceFactory.SpawnDiceArrayAsync(objectFactory, dicePositionsHandler.DicePositions);
			
			foreach (var diceView in diceViewsArray)
			{
				var model = new DiceModel(new LoadedDiceProfileConfig()); 
				var controller = new DiceController(model, diceView, tableModel);
				diceModelsList.Add(model);
				gameControllers.Add(controller);
			}
		}

		private void StopDiceGame()
		{
			CleanUp();
		}

		private void CleanUp()
		{
			if (diceViewsArray != null)
			{
				foreach (var dice in diceViewsArray)
				{
					objectFactory.Destroy(dice.gameObject);
				}
				
				diceViewsArray = null;
			}

			foreach (var controller in gameControllers)
			{
				lifecycleService.Unregister(controller);
			}

			gameControllers.Clear();
			diceModelsList.Clear();
			diceGameModel.Reset();
			tableModel.Reset();
		}
	}
}