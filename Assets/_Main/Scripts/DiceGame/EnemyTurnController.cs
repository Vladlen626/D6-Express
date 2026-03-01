using System;
using System.Collections.Generic;
using _Main.Scripts.Core;
using _Main.Scripts.Dice;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using UnityEngine;

public class EnemyTurnController : IBaseController, IActivatable
{
	private readonly DiceGameProcessController processController;
	private readonly DiceGameModel diceGameModel;
	private readonly EnemyAiScenarioRuntime scenarioRuntime;
	private TableModel tableModel => diceGameModel.tableModel;

	private int delay => Mathf.Max(1, Mathf.RoundToInt(GlobalParameters.Delay * GlobalParameters.EnemyTurnDelayMultiplier));

	private bool isRunning;

	public EnemyTurnController(
		DiceGameProcessController processController,
		DiceGameModel diceGameModel,
		EnemyAiScenarioRuntime scenarioRuntime = null)
	{
		this.processController = processController;
		this.diceGameModel = diceGameModel;
		this.scenarioRuntime = scenarioRuntime;
	}

	public void Activate()
	{
		diceGameModel.OnCurrentTurnChanged += OnCurrentTurnChangedHandler;
	}
	
	public void Deactivate()
	{
		diceGameModel.OnCurrentTurnChanged -= OnCurrentTurnChangedHandler;
	}

	private void OnCurrentTurnChangedHandler(int oldValue, int newValue)
	{
		if (diceGameModel.DiceGameState != DiceGameState.GAME)
		{
			return;
		}

		if (diceGameModel.IsPlayerTurn)
		{
			return;
		}

		TakeTurn().Forget();
	}

	public async UniTask TakeTurn()
	{
		if (isRunning)
		{
			return;
		}

		isRunning = true;

		try
		{
			diceGameModel.tableModel.DisableButtons();
			await UniTask.Delay(delay);
			if (scenarioRuntime != null && !scenarioRuntime.IsFailed)
			{
				await TakeScriptedTurn();
			}
			else
			{
				await TakeHeuristicTurn();
			}
		}
		catch (EnemyAiScenarioValidationException ex)
		{
			scenarioRuntime?.MarkFailed(ex.Message);
			Debug.LogError($"[EnemyAI][Scripted] Validation failed: {ex.Message}");
			diceGameModel.SetConditionFailed();
		}
		catch (Exception ex)
		{
			Debug.LogError($"[EnemyAI] Turn failed: {ex}");
			diceGameModel.SetConditionFailed();
		}
		finally
		{
			isRunning = false;
		}
	}

	private async UniTask TakeHeuristicTurn()
	{
		// первый ролл (как если бы игрок нажал Roll)
		await Roll();

		while (diceGameModel.IsPlayerTurn == false)
		{
			var unbanked = diceGameModel.GetUnbanked();

			if (unbanked.Length == 0)
			{
				await Pass();
				await UniTask.Delay(delay);
				return;
			}

			int[] values = DiceGameUtils.GetDiceValues(unbanked);
			var activeScoringService = diceGameModel.GetCurrentScoringService();
			var combinations = activeScoringService.Evaluate(values);
			if (combinations.Combinations.Count == 0)
			{
				await UniTask.Delay(delay);
				return;
			}

			int bestMask = FindBestMask(values, activeScoringService);

			for (int i = 0; i < unbanked.Length; i++)
			{
				bool selected = (bestMask & (1 << i)) != 0;
				await UniTask.Delay(delay / 2);
				unbanked[i].SetChosen(selected);
			}

			await UniTask.Delay(delay);

			bool hotDice = await processController.TrySaveSelected(diceGameModel.GetSelected(), combinations);

			await UniTask.Delay(delay);

			// если можно уже выиграть — пасуем
			if (tableModel.EnemyBankedPoints + tableModel.TurnPoints >= diceGameModel.TargetPoints)
			{
				await Pass();
				await UniTask.Delay(delay);
				return;
			}

			// если уже набрали нормально — пас
			if (tableModel.TurnPoints >= 400)
			{
				await Pass();
				await UniTask.Delay(delay);
				return;
			}

			// если хот дайс — ролл снова
			if (hotDice || diceGameModel.GetUnbanked().Length > 0)
			{
				await Roll();
				await UniTask.Delay(delay);
			}
			else
			{
				await Pass();
				await UniTask.Delay(delay);
				return;
			}

			await UniTask.Delay(delay);
		}
	}

	private async UniTask TakeScriptedTurn()
	{
		if (scenarioRuntime.IsCompleted)
		{
			ThrowScriptValidation("Enemy received a turn, but scenario has no turns left.");
		}

		var scriptTurn = scenarioRuntime.GetCurrentTurnOrNull();
		if (scriptTurn == null)
		{
			ThrowScriptValidation("Current scripted turn is null.");
		}

		var expectedTurnNumber = scenarioRuntime.ExecutedEnemyTurns + 1;
		if (scriptTurn.enemy_turn > 0 && scriptTurn.enemy_turn != expectedTurnNumber)
		{
			ThrowScriptValidation($"Unexpected enemy_turn value. Expected {expectedTurnNumber}, got {scriptTurn.enemy_turn}.");
		}

		for (int stepIndex = 0; stepIndex < scriptTurn.steps.Count; stepIndex++)
		{
			var step = scriptTurn.steps[stepIndex];
			switch (step.action_type)
			{
				case EnemyAiStepAction.Roll:
					await ExecuteScriptedRollStep(scriptTurn, step, stepIndex);
					break;
				case EnemyAiStepAction.Pass:
					await Pass();
					await UniTask.Delay(delay);
					break;
				default:
					ThrowScriptValidation($"Unknown action in turn {expectedTurnNumber}, step {stepIndex + 1}.");
					break;
			}

			if (stepIndex < scriptTurn.steps.Count - 1 && diceGameModel.IsPlayerTurn)
			{
				ThrowScriptValidation($"Enemy turn ended too early at turn {expectedTurnNumber}, step {stepIndex + 1}.");
			}
		}

		if (!diceGameModel.IsPlayerTurn)
		{
			ThrowScriptValidation($"Enemy turn {expectedTurnNumber} did not end after scripted steps.");
		}

		scenarioRuntime.MarkTurnCompleted();
		ValidateScriptedProgress();
	}

	private async UniTask ExecuteScriptedRollStep(EnemyAiTurnConfig turn, EnemyAiStepConfig step, int stepIndex)
	{
		var unbanked = diceGameModel.GetUnbanked();
		if (unbanked.Length == 0)
		{
			ThrowScriptValidation($"Turn {turn.enemy_turn}, step {stepIndex + 1}: no unbanked dice before roll.");
		}

		if (step.forced_values == null || step.forced_values.Length != unbanked.Length)
		{
			ThrowScriptValidation(
				$"Turn {turn.enemy_turn}, step {stepIndex + 1}: forced_values length ({step.forced_values?.Length ?? 0}) must match unbanked dice count ({unbanked.Length}).");
		}

		for (int i = 0; i < unbanked.Length; i++)
		{
			var forcedValue = step.forced_values[i];
			if (forcedValue < 1 || forcedValue > 6)
			{
				ThrowScriptValidation(
					$"Turn {turn.enemy_turn}, step {stepIndex + 1}: forced value at index {i} is out of range ({forcedValue}).");
			}

			unbanked[i].EnqueueForcedRollValue(forcedValue);
		}

		await Roll();
		await UniTask.Delay(delay);

		if (diceGameModel.IsPlayerTurn)
		{
			ThrowScriptValidation($"Turn {turn.enemy_turn}, step {stepIndex + 1}: roll ended the turn unexpectedly.");
		}

		var unbankedAfterRoll = diceGameModel.GetUnbanked();
		var selectedIndexes = new HashSet<int>(step.save_unbanked_indexes ?? Array.Empty<int>());

		foreach (var index in selectedIndexes)
		{
			if (index < 0 || index >= unbankedAfterRoll.Length)
			{
				ThrowScriptValidation(
					$"Turn {turn.enemy_turn}, step {stepIndex + 1}: save_unbanked_indexes contains invalid index {index} for {unbankedAfterRoll.Length} dice.");
			}
		}

		for (int i = 0; i < unbankedAfterRoll.Length; i++)
		{
			await UniTask.Delay(delay / 2);
			unbankedAfterRoll[i].SetChosen(selectedIndexes.Contains(i));
		}

		var selected = diceGameModel.GetSelected();
		if (selected.Length == 0)
		{
			ThrowScriptValidation($"Turn {turn.enemy_turn}, step {stepIndex + 1}: selected dice set is empty.");
		}

		var activeScoringService = diceGameModel.GetCurrentScoringService();
		var selectedCombinations = activeScoringService.Evaluate(DiceGameUtils.GetDiceValues(selected));
		var turnPointsBeforeSave = tableModel.TurnPoints;
		await processController.TrySaveSelected(selected, selectedCombinations);
		var savedPoints = tableModel.TurnPoints - turnPointsBeforeSave;

		if (step.expected_saved_score.HasValue && savedPoints != step.expected_saved_score.Value)
		{
			ThrowScriptValidation(
				$"Turn {turn.enemy_turn}, step {stepIndex + 1}: expected_saved_score={step.expected_saved_score.Value}, actual={savedPoints}.");
		}

		await UniTask.Delay(delay);
	}

	private void ValidateScriptedProgress()
	{
		var expected = scenarioRuntime.Scenario.expected_result;
		var actualBanked = tableModel.EnemyBankedPoints;
		var reachedTarget = actualBanked >= diceGameModel.TargetPoints;

		if (scenarioRuntime.ExecutedEnemyTurns < expected.completed_in_enemy_turns && reachedTarget)
		{
			ThrowScriptValidation(
				$"Enemy reached target score too early: expected after {expected.completed_in_enemy_turns} turns, actual after {scenarioRuntime.ExecutedEnemyTurns}.");
		}

		if (scenarioRuntime.ExecutedEnemyTurns != expected.completed_in_enemy_turns)
		{
			return;
		}

		if (!scenarioRuntime.IsCompleted)
		{
			ThrowScriptValidation(
				$"Scenario expected_result.completed_in_enemy_turns={expected.completed_in_enemy_turns}, but turns data is not exhausted.");
		}

		if (actualBanked != expected.enemy_final_banked_score)
		{
			ThrowScriptValidation(
				$"Scenario final enemy score mismatch: expected {expected.enemy_final_banked_score}, actual {actualBanked}.");
		}

		if (reachedTarget != expected.enemy_reaches_target)
		{
			ThrowScriptValidation(
				$"Scenario target reach mismatch: expected {expected.enemy_reaches_target}, actual {reachedTarget}.");
		}
	}

	private static void ThrowScriptValidation(string message)
	{
		throw new EnemyAiScenarioValidationException(message);
	}

	private async UniTask Roll()
	{
		if (processController.IsProcessing)
		{
			return;
		}

		await processController.HandleRollAsync();
	}

	private async UniTask Pass()
	{
		if (processController.IsProcessing)
		{
			return;
		}

		await processController.HandlePassForCurrentTurnAsync();
	}

	private int FindBestMask(int[] values, DiceScoringService scoringService)
	{
		int bestScore = -1;
		int bestMask = 0;

		int maxMask = 1 << values.Length;

		for (int mask = 1; mask < maxMask; mask++)
		{
			List<int> subset = new List<int>();

			for (int i = 0; i < values.Length; i++)
			{
				if ((mask & (1 << i)) != 0)
				{
					subset.Add(values[i]);
				}
			}

			var combinations = scoringService.Evaluate(subset.ToArray());
			int score = scoringService.CalculateTotalScore(combinations);

			if (score > bestScore)
			{
				bestScore = score;
				bestMask = mask;
			}
		}

		return bestMask;
	}

	private sealed class EnemyAiScenarioValidationException : Exception
	{
		public EnemyAiScenarioValidationException(string message)
			: base(message)
		{
		}
	}
}
