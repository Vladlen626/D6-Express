using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace _Main.Scripts.Dice
{
	public class ModifiersModel
	{
		private readonly DiceGameModel diceGameModel;

		private List<IOnRollAction> onRollActionsHandler;
		private List<IOnPassAction> onPassActionsHandler;

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
			onRollActionsHandler.Clear();
			onPassActionsHandler.Clear();
		}
	}
}