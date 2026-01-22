using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace _Main.Scripts.Dice
{
	public class ModifiersModel
	{
		private readonly List<IOnRollModifier> onRollActionsHandler = new ();
		private readonly List<IOnPassModifier> onPassActionsHandler = new ();

		private int multiplier;
		public void AddModifier(IModifier modifier)
		{
			switch (modifier)
			{
				case IOnRollModifier onRollAction:
					onRollActionsHandler.Add(onRollAction);
					break;
				case IOnPassModifier onPassAction:
					onPassActionsHandler.Add(onPassAction);
					break;
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

		public void Reset()
		{
			onRollActionsHandler.Clear();
			onPassActionsHandler.Clear();
		}
	}
}
