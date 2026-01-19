using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace _Main.Scripts.Dice
{
	public class ModifiersModel
	{
		public event Action OnMultiplierChanged;
		public int PointsMultiplier
		{
			get => multiplier;

			private set
			{
				multiplier = value;
				OnMultiplierChanged?.Invoke();
			}
		}
		
		private readonly DiceGameModel diceGameModel;
		private readonly List<IOnRollAction> onRollActionsHandler = new ();
		private readonly List<IOnPassAction> onPassActionsHandler = new ();

		private int multiplier;


		public void AddPointsMultiplierValue(int multiplierValue)
		{
			PointsMultiplier += multiplierValue;
		}

		public void MultiplyPointsMultiplierValue(int multiplierValue)
		{
			PointsMultiplier *= multiplierValue;
		}
		
		public void AddRollAction(IOnRollAction onRollAction)
		{
			onRollActionsHandler.Add(onRollAction);
		}

		public void AddPassAction(IOnPassAction onPassAction)
		{
			onPassActionsHandler.Add(onPassAction);
		}
		
		public async UniTask PlayRollActions()
		{
			foreach (var onRollAction in onRollActionsHandler)
			{
				await onRollAction.OnRoll(diceGameModel);
			}
		}

		public async UniTask PlayPassActions()
		{
			foreach (var onPassAction in onPassActionsHandler)
			{
				await onPassAction.OnPass(diceGameModel);
			}
		}

		public void Reset()
		{
			PointsMultiplier = 0;
			onRollActionsHandler.Clear();
			onPassActionsHandler.Clear();
		}
	}
}