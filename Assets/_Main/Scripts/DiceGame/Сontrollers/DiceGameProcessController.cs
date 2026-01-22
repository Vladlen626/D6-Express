using System.Collections.Generic;
using _Main.Scripts.Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services;
using PlatformCore.Services.Audio;

namespace _Main.Scripts.Dice
{
	public class DiceGameProcessController : IBaseController, IActivatable
	{
		private readonly ILoggerService logger;
		private readonly IAudioService audioService;
		private readonly ICameraShakeService cameraShakeService;
		private readonly DiceGameModel diceGameModel;
		private TableModel tableModel => diceGameModel.tableModel;

		public bool IsProcessing { get; private set; }

		public DiceGameProcessController(
			ILoggerService logger,
			DiceGameModel diceGameModel,
			ICameraShakeService cameraShakeService,
			IAudioService audioService)
		{
			this.logger = logger;
			this.diceGameModel = diceGameModel;
			this.cameraShakeService = cameraShakeService;
			this.audioService = audioService;
		}

		public void Activate()
		{
			logger?.Log("[DiceGameController] Activating...");

			diceGameModel.OnRollClicked += HandleRoll;
			diceGameModel.OnPassClicked += HandlePass;

			foreach (var diceModel in diceGameModel.CurrentDiceModelList)
			{
				diceModel.OnDiceChosenChanged += UpdateUI;
			}

			UpdateUI();
		}

		public void Deactivate()
		{
			logger?.Log("[DiceGameController] Deactivating...");

			diceGameModel.OnRollClicked -= HandleRoll;
			diceGameModel.OnPassClicked -= HandlePass;

			foreach (var diceModel in diceGameModel.CurrentDiceModelList)
			{
				diceModel.OnDiceChosenChanged -= UpdateUI;
			}
		}

		// === ОБРАБОТЧИКИ КНОПОК ===

		public void HandleRoll()
		{
			if (IsProcessing)
			{
				return;
			}

			_ = HandleRollAsync();
		}

		// ReSharper disable Unity.PerformanceAnalysis
		public async UniTask HandleRollAsync()
		{
			IsProcessing = true;

			try
			{
				logger?.Log("[DiceGameController] Handle roll");

				diceGameModel.tableModel.DisableButtons();

				if (tableModel.isFirstRoll)
				{
					var roundStartContext = new DiceModifierContext(
						new DiceCombinationResult { Combinations = new List<DiceCombinationEntry>() },
						diceGameModel.GetUnbanked(),
						tableModel,
						diceGameModel,
						ModifierStage.RoundStart);
					await diceGameModel.ModifiersModel.PlayRoundStartActions(roundStartContext);

					tableModel.isFirstRoll = false;
					diceGameModel.ShowAllDiceGameModels();
				}
				
	
				bool isHotDice = await TrySaveSelected(diceGameModel.GetSelected(), DiceGameUtils.GetCombinations(GetValues(diceGameModel.GetSelected())));
				tableModel.SetPreviewPoints(0);

				// Если все кубы забанкированы после сохранения, сбросить пул
				if (isHotDice)
				{
					await ResetAllDiceToActiveAsync();
					diceGameModel.ResetAllDices();
				}

				// Роллим актуальные кубы
				var tasks = new List<UniTask>();
				var diceToRoll = diceGameModel.GetUnbanked();
				foreach (var dice in diceToRoll)
				{
					dice.Roll();
					var view = diceGameModel.ScreenDiceDict[dice];
					tasks.Add(view.PlayRollAnimationAsync());
				}

				await UniTask.WhenAll(tasks);
				audioService.PlaySound(SoundNames.DiceDrop);

				await UniTask.Delay(GlobalParameters.Delay / 2);
				
				var diceCombinationResult = DiceGameUtils.GetCombinations(GetValues(diceToRoll));
				var rollModifierContext = new DiceModifierContext(
					diceCombinationResult,
					diceToRoll,
					tableModel,
					diceGameModel,
					ModifierStage.Roll);
				
				await diceGameModel.ModifiersModel.PlayRollActions(rollModifierContext);

				if (diceCombinationResult.Combinations.Count == 0)
				{
					audioService.PlaySound(SoundNames.Fail);
					await UniTask.Delay(GlobalParameters.Delay);
					var roundEndContext = new DiceModifierContext(
						diceCombinationResult,
						diceToRoll,
						tableModel,
						diceGameModel,
						ModifierStage.RoundEnd);
					await diceGameModel.ModifiersModel.PlayRoundEndActions(roundEndContext);
					EndTurn(false);
				}
			}
			finally
			{
				diceGameModel.RollEnded();
				IsProcessing = false;
			}
		}

		private int[] GetValues(DiceModel[] dice)
		{
			var values = new int[dice.Length];
			for (int i = 0; i < dice.Length; i++) values[i] = dice[i].CurrentValue;
			return values;
		}

		private void HandlePass()
		{
			if (IsProcessing)
			{
				return;
			}

			_ = HandlePassAsync();
		}

		private async UniTask HandlePassAsync()
		{
			IsProcessing = true;

			try
			{
				diceGameModel.tableModel.DisableButtons();
				
				var selected = diceGameModel.GetSelected();
				var combo = DiceGameUtils.GetCombinations(GetValues(selected));
				var passModifierContext = new DiceModifierContext(
					combo,
					selected,
					tableModel,
					diceGameModel,
					ModifierStage.Pass);
				await diceGameModel.ModifiersModel.PlayPassActions(passModifierContext);
				await TrySaveSelected(selected, passModifierContext.CombinationResult);
				var roundEndContext = new DiceModifierContext(
					passModifierContext.CombinationResult,
					selected,
					tableModel,
					diceGameModel,
					ModifierStage.RoundEnd);
				await diceGameModel.ModifiersModel.PlayRoundEndActions(roundEndContext);
				EndTurn(true);
			}
			finally
			{
				diceGameModel.PassEnded();
				IsProcessing = false;
			}
		}

		// ReSharper disable Unity.PerformanceAnalysis
		public void EndTurn(bool success)
		{
			diceGameModel.EndTurn(success);
			audioService.PlaySound(SoundNames.TurnChange);
			UpdateUI();
		}

		public async UniTask<bool> TrySaveSelected(DiceModel[] selected, DiceCombinationResult combinationResult)
		{
			int points = DiceGameUtils.CalculateTotalScore(combinationResult);
			if (points <= 0)
			{
				return false;
			}

			tableModel.AddTurnPoints(points);
			var tweenList = new List<Tween>();
			foreach (var diceModel in selected)
			{
				diceModel.SetSaved(true);
				diceModel.SetChosen(false);

				var position = tableModel.GetFreeBankedPosition();
				diceModel.SetCurrentPosition(position);
				var view = diceGameModel.ScreenDiceDict[diceModel];
				view.ResetYRotation();
				tweenList.Add(view.MoveToPosition(position.position));
			}

			await UniTaskUtils.WaitAllTweens(tweenList.ToArray());

			return diceGameModel.AllBanked();
		}

		private async UniTask ResetAllDiceToActiveAsync()
		{
			var tweens = new List<Tween>();
			
			foreach (var diceModel in diceGameModel.CurrentDiceModelList)
			{
				var pos = tableModel.GetFreeActivePosition();
				diceModel.SetSaved(false);
				diceModel.SetCurrentPosition(pos);

				var view = diceGameModel.ScreenDiceDict[diceModel];
				tweens.Add(view.MoveToPosition(pos.position));
			}

			await UniTaskUtils.WaitAllTweens(tweens.ToArray());
		}

		private void UpdateUI()
		{
			if (tableModel.isFirstRoll)
			{
				diceGameModel.HideAllDiceGameModels();
			}
			else
			{
				diceGameModel.ShowAllDiceGameModels();
			}

			var selectedDice = diceGameModel.GetSelected();
			var selectedValues = new int[selectedDice.Length];
			for (int i = 0; i < selectedDice.Length; i++)
			{
				selectedValues[i] = selectedDice[i].CurrentValue;
			}


			if (DiceGameUtils.HasTrashInSelected(selectedValues))
			{
				tableModel.SetPreviewPoints(0);
			}
			else
			{
				var combo = DiceGameUtils.GetCombinations(selectedValues);
				tableModel.SetPreviewPoints(DiceGameUtils.CalculateTotalScore(combo));
			}


			diceGameModel.tableModel.SendUpdateUI();
		}
	}
}
