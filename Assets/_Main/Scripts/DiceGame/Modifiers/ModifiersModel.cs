using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace _Main.Scripts.Dice
{
	public class ModifiersModel
	{
		private readonly List<IOnRoundStartModifier> onRoundStartActionsHandler = new ();
		private readonly List<IOnRollModifier> onRollActionsHandler = new ();
		private readonly List<IOnPassModifier> onPassActionsHandler = new ();
		private readonly List<IOnRoundEndModifier> onRoundEndActionsHandler = new ();

		private int multiplier;
		public void AddModifier(IModifier modifier)
		{
			switch (modifier)
			{
				case IOnRoundStartModifier onRoundStartAction:
					onRoundStartActionsHandler.Add(onRoundStartAction);
					break;
				case IOnRollModifier onRollAction:
					onRollActionsHandler.Add(onRollAction);
					break;
				case IOnPassModifier onPassAction:
					onPassActionsHandler.Add(onPassAction);
					break;
				case IOnRoundEndModifier onRoundEndAction:
					onRoundEndActionsHandler.Add(onRoundEndAction);
					break;
			}
		}

		public async UniTask PlayRoundStartActions(DiceModifierContext modifierContext)
		{
			foreach (var onRoundStartAction in onRoundStartActionsHandler)
			{
				await onRoundStartAction.ModifyValues(modifierContext);
			}
		}

		public async UniTask PlayRollActions(DiceModifierContext modifierContext)
		{
			foreach (var onRollAction in onRollActionsHandler)
			{
				await onRollAction.ModifyValues(modifierContext);
			}
		}

		public async UniTask PlayPassActions(DiceModifierContext modifierContext)
		{
			foreach (var onPassAction in onPassActionsHandler)
			{
				await onPassAction.ModifyValues(modifierContext);
			}
		}

		public async UniTask PlayRoundEndActions(DiceModifierContext modifierContext)
		{
			foreach (var onRoundEndAction in onRoundEndActionsHandler)
			{
				await onRoundEndAction.ModifyValues(modifierContext);
			}
		}

		public void Reset()
		{
			onRoundStartActionsHandler.Clear();
			onRollActionsHandler.Clear();
			onPassActionsHandler.Clear();
			onRoundEndActionsHandler.Clear();
		}
	}
}
