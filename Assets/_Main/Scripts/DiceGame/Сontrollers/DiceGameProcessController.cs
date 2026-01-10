using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services;

namespace _Main.Scripts.Dice
{
	public class DiceGameProcessController : IBaseController, IActivatable
	{
		private readonly ILoggerService logger;
		private readonly ICameraShakeService cameraShakeService;
		private readonly DiceGameModel diceGameModel;
		private readonly TableModel tableModel;

		private readonly DiceTableView tableView;
		private readonly DicePoolLogic dicePool;

		private bool isProcessing;
		public DiceGameProcessController(
			TableModel tableModel,
			DiceTableView tableView,
			ILoggerService logger,
			DiceGameModel diceGameModel,
			ICameraShakeService cameraShakeService)
		{
			this.tableModel = tableModel;
			this.tableView = tableView;
			this.logger = logger;
			this.diceGameModel = diceGameModel;
			this.cameraShakeService = cameraShakeService;

			dicePool = new DicePoolLogic(diceGameModel);
		}

		public void Activate()
		{
			logger?.Log("[DiceGameController] Activating...");

			tableView.OnRollClicked += HandleRoll;
			tableView.OnPassClicked += HandlePass;

			foreach (var diceModel in diceGameModel.CurrentDiceModelList)
			{
				diceModel.OnDiceChosenChanged += UpdateUI;
			}
			
			UpdateUI();
		}

		public void Deactivate()
		{
			logger?.Log("[DiceGameController] Deactivating...");

			tableView.OnRollClicked -= HandleRoll;
			tableView.OnPassClicked -= HandlePass;

			foreach (var diceModel in diceGameModel.CurrentDiceModelList)
			{
				diceModel.OnDiceChosenChanged -= UpdateUI;
			}
		}

		// === ОБРАБОТЧИКИ КНОПОК ===

		private void HandleRoll()
		{
			if (isProcessing)
			{
				return;
			}

			_ = HandleRollAsync();
		}

		// ReSharper disable Unity.PerformanceAnalysis
		private async UniTask HandleRollAsync()
		{
			isProcessing = true;

			try
			{
				logger?.Log("[DiceGameController] Handle roll");

				DisableButtons();

				await cameraShakeService.ShakeAsync(tableView.TableCamera,0.75f, 0.075f);
				if (tableModel.isFirstRoll)
				{
					tableModel.isFirstRoll = false;
					diceGameModel.ShowAllDiceGameModels();
				}

				// Сохраняем выбранные кубы, если есть
				bool isHotDice = await TrySaveSelected();
				tableModel.SetPreviewPoints(0);

				// Если все кубы забанкированы после сохранения, сбросить пул
				if (isHotDice)
				{
					await ResetAllDiceToActiveAsync();
					dicePool.ResetAll();
				}

				// Роллим актуальные кубы
				var tasks = new List<UniTask>();
				var diceToRoll = dicePool.GetUnbanked();
				foreach (var dice in diceToRoll)
				{
					dice.Roll();
					var view = diceGameModel.ScreenDiceDict[dice];
					tasks.Add(view.PlayRollAnimationAsync());
				}

				await UniTask.WhenAll(tasks);
				
				await cameraShakeService.ShakeAsync(tableView.TableCamera,1.5f, 0.1f);

				CheckBust();
				UpdateUI();
			}
			finally
			{
				isProcessing = false;
			}
		}

		private void CheckBust()
		{
			var diceToRoll = dicePool.GetUnbanked();
			if (DiceGameUtils.RollHasAnyScore(GetValues(diceToRoll)))
			{
				return;
			}

			logger?.Log("[DiceGameController] BUST!");
			EndTurn(false);
		}

		private int[] GetValues(DiceModel[] dice)
		{
			var values = new int[dice.Length];
			for (int i = 0; i < dice.Length; i++) values[i] = dice[i].CurrentValue;
			return values;
		}

		private void HandlePass()
		{
			if (isProcessing)
			{
				return;
			}

			_ = HandlePassAsync();
		}

		private async UniTask HandlePassAsync()
		{
			isProcessing = true;

			try
			{
				DisableButtons();
				await TrySaveSelected();
				EndTurn(true);
			}
			finally
			{
				isProcessing = false;
			}
		}

		private void EndTurn(bool success)
		{
			if (success)
			{
				if (diceGameModel.IsPlayerTurn)
				{
					tableModel.AddBankedPointsForPlayer(tableModel.TurnPoints);
				}
				else
				{
					tableModel.AddBankedPointsForEnemy(tableModel.TurnPoints);
				}
			}

			
			diceGameModel.IncreaseCurrentTurn();
			tableModel.ResetTurn();
			dicePool.ResetAll();
			foreach (var diceModel in diceGameModel.CurrentDiceModelList)
			{
				diceGameModel.ScreenDiceDict[diceModel].MoveToPosition(tableModel.GetFreeActivePosition().position);
			}
			UpdateUI();
		}

		private async UniTask<bool> TrySaveSelected()
		{
			var selected = dicePool.GetSelected();
			if (selected.Length == 0)
			{
				return false;
			}

			var values = new int[selected.Length];
			for (var i = 0; i < selected.Length; i++)
			{
				values[i] = selected[i].CurrentValue;
			}

			int points = DiceGameUtils.CalculateScore(values);
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
				tweenList.Add(view.MoveToPosition(position.position));
			}

			await UniTaskUtils.WaitAllTweens(tweenList.ToArray());

			return dicePool.AllBanked();
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

			var selectedDice = dicePool.GetSelected();
			var selectedValues = new int[selectedDice.Length];
			for (int i = 0; i < selectedDice.Length; i++)
			{
				selectedValues[i] = selectedDice[i].CurrentValue;
			}

			int scorePreview = DiceGameUtils.CalculateScore(selectedValues);
			bool hasValidComboSelected = scorePreview > 0;
			bool canPass = hasValidComboSelected || (tableModel.TurnPoints > 0 && selectedDice.Length == 0);
			bool canRoll = tableModel.isFirstRoll || hasValidComboSelected;

			int previewPoints = hasValidComboSelected ? scorePreview : 0;
			tableModel.SetPreviewPoints(previewPoints);

			tableView.SetButtonInteractable("Roll", canRoll);
			tableView.SetButtonInteractable("Pass", canPass);
		}

		public void DisableButtons()
		{
			tableView.SetButtonInteractable("Roll", false);
			tableView.SetButtonInteractable("Pass", false);
		}
	}
}