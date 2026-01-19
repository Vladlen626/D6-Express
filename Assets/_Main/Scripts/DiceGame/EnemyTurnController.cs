using System.Collections.Generic;
using _Main.Scripts.Core;
using _Main.Scripts.Dice;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

public class EnemyTurnController : IBaseController, IActivatable
{
	private readonly DiceGameProcessController processController;
	private readonly DiceGameModel diceGameModel;
	private TableModel tableModel => diceGameModel.tableModel;

	private int delay => GlobalParameters.Delay/2;

	private bool isRunning;

	public EnemyTurnController(
		DiceGameProcessController processController,
		DiceGameModel diceGameModel)
	{
		this.processController = processController;
		this.diceGameModel = diceGameModel;
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

			// первый ролл (как если бы игрок нажал Roll)
			await Roll();

			while (diceGameModel.IsPlayerTurn == false)
			{
				var unbanked = diceGameModel.GetUnbanked();

				if (unbanked.Length == 0)
				{
					processController.EndTurn(true);
					await UniTask.Delay(delay);
					return;
				}

				int[] values = DiceGameUtils.GetDiceValues(unbanked);
				var combinations = DiceGameUtils.GetCombinations(values);
				if (combinations.Combinations.Count == 0)
				{
					await UniTask.Delay(delay);
					return;
				}

				int bestMask = FindBestMask(values);

				for (int i = 0; i < unbanked.Length; i++)
				{
					bool selected = (bestMask & (1 << i)) != 0;
					await UniTask.Delay(delay/2);
					unbanked[i].SetChosen(selected);
				}

				await UniTask.Delay(delay);

				bool hotDice = await processController.TrySaveSelected();

				await UniTask.Delay(delay);

				// если можно уже выиграть — пасуем
				if (tableModel.EnemyBankedPoints + tableModel.TurnPoints >= diceGameModel.TargetPoints)
				{
					processController.EndTurn(true);
					await UniTask.Delay(delay);
					return;
				}

				// если уже набрали нормально — пас
				if (tableModel.TurnPoints >= 400)
				{
					processController.EndTurn(true);
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
					processController.EndTurn(true);
					await UniTask.Delay(delay);
					return;
				}

				await UniTask.Delay(delay);
			}
		}
		finally
		{
			isRunning = false;
		}
	}

	private async UniTask Roll()
	{
		if (processController.IsProcessing)
		{
			return;
		}

		await processController.HandleRollAsync();
	}

	private int FindBestMask(int[] values)
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

			var combinations = DiceGameUtils.GetCombinations(subset.ToArray());
			int score = DiceGameUtils.CalculateScore(combinations);

			if (score > bestScore)
			{
				bestScore = score;
				bestMask = mask;
			}
		}

		return bestMask;
	}
}