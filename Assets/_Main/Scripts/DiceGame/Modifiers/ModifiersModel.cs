using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace _Main.Scripts.Dice
{
	public class ModifiersModel
	{
		public IReadOnlyList<IModifier> AllModifiers => allModifiers;
		public int LevelStartCount => onLevelStartActionsHandler.Count;
		public int RoundStartCount => onRoundStartActionsHandler.Count;
		public int RollCount => onRollActionsHandler.Count;
		public int PassCount => onPassActionsHandler.Count;
		public int RoundEndCount => onRoundEndActionsHandler.Count;

		private readonly List<IModifier> allModifiers = new();
		private readonly List<IOnLevelStartModifier> onLevelStartActionsHandler = new();
		private readonly List<IOnRoundStartModifier> onRoundStartActionsHandler = new();
		private readonly List<IOnRollModifier> onRollActionsHandler = new();
		private readonly List<IOnPassModifier> onPassActionsHandler = new();
		private readonly List<IOnRoundEndModifier> onRoundEndActionsHandler = new();

		public event Action<IModifier> ModifierAdded;
		public event Action<IModifier> ModifierRemoved;
		public event Action<IModifier, ModifierStage> ModifierApplied;

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

		public void ClearModifiers()
		{
			for (int i = allModifiers.Count - 1; i >= 0; i--)
			{
				RemoveModifier(allModifiers[i]);
				allModifiers.RemoveAt(i);
			}
		}

		public async UniTask PlayLevelStartActions(DiceModifierContext modifierContext)
		{
			foreach (var onLevelStartAction in onLevelStartActionsHandler)
			{
				await onLevelStartAction.ModifyValues(modifierContext);
				ModifierApplied?.Invoke(onLevelStartAction, ModifierStage.LevelStart);
			}
		}

		public async UniTask PlayRoundStartActions(DiceModifierContext modifierContext)
		{
			foreach (var onRoundStartAction in onRoundStartActionsHandler)
			{
				await onRoundStartAction.ModifyValues(modifierContext);
				ModifierApplied?.Invoke(onRoundStartAction, ModifierStage.RoundStart);
			}
		}

		public async UniTask PlayRollActions(DiceModifierContext modifierContext)
		{
			foreach (var onRollAction in onRollActionsHandler)
			{
				await onRollAction.ModifyValues(modifierContext);
				ModifierApplied?.Invoke(onRollAction, ModifierStage.Roll);
			}
		}

		public async UniTask PlayPassActions(DiceModifierContext modifierContext)
		{
			foreach (var onPassAction in onPassActionsHandler)
			{
				await onPassAction.ModifyValues(modifierContext);
				ModifierApplied?.Invoke(onPassAction, ModifierStage.Pass);
			}
		}

		public async UniTask PlayRoundEndActions(DiceModifierContext modifierContext)
		{
			foreach (var onRoundEndAction in onRoundEndActionsHandler)
			{
				await onRoundEndAction.ModifyValues(modifierContext);
				ModifierApplied?.Invoke(onRoundEndAction, ModifierStage.RoundEnd);
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
