using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace _Main.Scripts.Dice
{
	public class ModifiersModel
	{
		public IReadOnlyList<IModifier> AllModifiers => allModifiers;

		private readonly List<IModifier> allModifiers = new();
		private readonly List<IOnLevelStartModifier> onLevelStartActionsHandler = new();
		private readonly List<IOnRoundStartModifier> onRoundStartActionsHandler = new();
		private readonly List<IOnRollModifier> onRollActionsHandler = new();
		private readonly List<IOnPassModifier> onPassActionsHandler = new();
		private readonly List<IOnRoundEndModifier> onRoundEndActionsHandler = new();

		public event Action<IModifier> ModifierAdded;
		public event Action<IModifier> ModifierRemoved;

		public void AddModifier(IModifier modifier)
		{
			allModifiers.Add(modifier);

			if (modifier is IOnLevelStartModifier onLevelStartAction)
			{
				onLevelStartActionsHandler.Add(onLevelStartAction);
			}

			if (modifier is IOnRoundStartModifier onRoundStartAction)
			{
				onRoundStartActionsHandler.Add(onRoundStartAction);
			}

			if (modifier is IOnRollModifier onRollAction)
			{
				onRollActionsHandler.Add(onRollAction);
			}

			if (modifier is IOnPassModifier onPassAction)
			{
				onPassActionsHandler.Add(onPassAction);
			}

			if (modifier is IOnRoundEndModifier onRoundEndAction)
			{
				onRoundEndActionsHandler.Add(onRoundEndAction);
			}

			ModifierAdded?.Invoke(modifier);
		}

		public void RemoveModifier(IModifier modifier)
		{
			allModifiers.Remove(modifier);

			if (modifier is IOnLevelStartModifier onLevelStartAction)
			{
				onLevelStartActionsHandler.Remove(onLevelStartAction);
			}

			if (modifier is IOnRoundStartModifier onRoundStartAction)
			{
				onRoundStartActionsHandler.Remove(onRoundStartAction);
			}

			if (modifier is IOnRollModifier onRollAction)
			{
				onRollActionsHandler.Remove(onRollAction);
			}

			if (modifier is IOnPassModifier onPassAction)
			{
				onPassActionsHandler.Remove(onPassAction);
			}

			if (modifier is IOnRoundEndModifier onRoundEndAction)
			{
				onRoundEndActionsHandler.Remove(onRoundEndAction);
			}

			ModifierRemoved?.Invoke(modifier);
		}

		public async UniTask PlayLevelStartActions(DiceModifierContext modifierContext)
		{
			foreach (var onLevelStartAction in onLevelStartActionsHandler)
			{
				await onLevelStartAction.ModifyValues(modifierContext);
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
			onLevelStartActionsHandler.Clear();
			onRoundStartActionsHandler.Clear();
			onRollActionsHandler.Clear();
			onPassActionsHandler.Clear();
			onRoundEndActionsHandler.Clear();
		}
	}
}
