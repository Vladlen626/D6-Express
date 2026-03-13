using System.Collections.Generic;
using System.Reflection;
using _Main.Scripts.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Clickable item that temporarily silences every modifier (items and non-items alike).
	/// When activated, it removes all other modifiers from the pipeline until the end of the current round
	/// (RoundEnd stage), then restores them and is consumed.
	/// </summary>
	public class ModifierSilencerItem : ModifierItemBase,
		IOnLevelStartModifier,
		IOnRoundStartModifier,
		IOnRollModifier,
		IOnPassModifier,
		IOnRoundEndModifier,
		IModifierItemViewProvider
	{
		private const string LevelStartField = "onLevelStartActionsHandler";
		private const string RoundStartField = "onRoundStartActionsHandler";
		private const string RollField = "onRollActionsHandler";
		private const string PassField = "onPassActionsHandler";
		private const string RoundEndField = "onRoundEndActionsHandler";

		private readonly ItemView customPrefab;

		private ModifiersModel boundModifiersModel;
		private DiceGameModel boundGameModel;
		private SilencedState savedState;
		private bool isSilencing;
		private bool restoreScheduled;

		public ModifierSilencerItem(string id, int cooldownPasses = 2, ItemView prefabOverride = null)
			: base(id, id, DiceItemActivationType.ClickToActivate)
		{
			_ = cooldownPasses;
			customPrefab = prefabOverride;
		}

		public override string InvalidActivationNotificationKey => GlobalConstants.Localization.ItemActivationOnlyGame;

		public override bool IsActivationAllowed(DiceGameState gameState)
		{
			return gameState == DiceGameState.GAME;
		}

		public override async UniTask ModifyValues(DiceModifierContext modifierContext)
		{
			BindContext(modifierContext);

			if (isSilencing && modifierContext.Stage == ModifierStage.RoundEnd && !restoreScheduled)
			{
				ScheduleRestoreAndConsume();
			}

			await UniTask.CompletedTask;
		}

		protected override bool OnClick()
		{
			if (State != DiceItemState.Ready || isSilencing)
			{
				return false;
			}

			if (!TryActivateSilence())
			{
				return false;
			}

			SetState(DiceItemState.Armed);
			NotifyEffectApplied();
			return true;
		}

		private void BindContext(DiceModifierContext context)
		{
			if (context == null)
			{
				return;
			}

			boundGameModel ??= context.DiceGameModel;
			boundModifiersModel ??= context.DiceGameModel?.ModifiersModel;
		}

		private bool TryActivateSilence()
		{
			var model = boundModifiersModel ?? boundGameModel?.ModifiersModel;
			if (model == null)
			{
				Debug.LogWarning("[ModifierSilencerItem] No ModifiersModel available; activation ignored.");
				return false;
			}

			if (isSilencing)
			{
				return false;
			}

			savedState = CaptureState(model);
			ApplySilence(model);
			isSilencing = true;
			restoreScheduled = false;

			Debug.Log("[ModifierSilencerItem] Modifiers silenced until RoundEnd.");
			return true;
		}

		private void ScheduleRestoreAndConsume()
		{
			restoreScheduled = true;

			UniTask.Void(async () =>
			{
				// Wait one frame so the enumerator in Play*Actions finishes before we touch the lists again.
				await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
				RestoreModifiers();
				Consume();
			});
		}

		private SilencedState CaptureState(ModifiersModel model)
		{
			return new SilencedState
			{
				LevelStart = CloneList<IOnLevelStartModifier>(model, LevelStartField),
				RoundStart = CloneList<IOnRoundStartModifier>(model, RoundStartField),
				Roll = CloneList<IOnRollModifier>(model, RollField),
				Pass = CloneList<IOnPassModifier>(model, PassField),
				RoundEnd = CloneList<IOnRoundEndModifier>(model, RoundEndField)
			};
		}

		private void ApplySilence(ModifiersModel model)
		{
			ReplaceList(model, LevelStartField, new List<IOnLevelStartModifier> { this });
			ReplaceList(model, RoundStartField, new List<IOnRoundStartModifier> { this });
			ReplaceList(model, RollField, new List<IOnRollModifier> { this });
			ReplaceList(model, PassField, new List<IOnPassModifier> { this });
			ReplaceList(model, RoundEndField, new List<IOnRoundEndModifier> { this });
		}

		private void RestoreModifiers()
		{
			if (!isSilencing || boundModifiersModel == null || savedState == null)
			{
				return;
			}

			ReplaceList(boundModifiersModel, LevelStartField, savedState.LevelStart);
			ReplaceList(boundModifiersModel, RoundStartField, savedState.RoundStart);
			ReplaceList(boundModifiersModel, RollField, savedState.Roll);
			ReplaceList(boundModifiersModel, PassField, savedState.Pass);
			ReplaceList(boundModifiersModel, RoundEndField, savedState.RoundEnd);

			isSilencing = false;
			restoreScheduled = false;
			savedState = null;

			Debug.Log("[ModifierSilencerItem] Modifiers restored.");
		}

		public override void ResetItem()
		{
			if (isSilencing)
			{
				RestoreModifiers();
			}

			savedState = null;
			isSilencing = false;
			restoreScheduled = false;

			base.ResetItem();
		}

		public ItemView GetViewPrefab() => customPrefab;

		private static List<T> CloneList<T>(ModifiersModel model, string fieldName)
		{
			var field = typeof(ModifiersModel).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
			if (field?.GetValue(model) is List<T> list)
			{
				return new List<T>(list);
			}

			return new List<T>();
		}

		private static void ReplaceList<T>(ModifiersModel model, string fieldName, List<T> newList)
		{
			var field = typeof(ModifiersModel).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
			if (field == null)
			{
				Debug.LogWarning($"[ModifierSilencerItem] Unable to locate field {fieldName} on ModifiersModel.");
				return;
			}

			field.SetValue(model, newList ?? new List<T>());
		}

		private class SilencedState
		{
			public List<IOnLevelStartModifier> LevelStart;
			public List<IOnRoundStartModifier> RoundStart;
			public List<IOnRollModifier> Roll;
			public List<IOnPassModifier> Pass;
			public List<IOnRoundEndModifier> RoundEnd;
		}
	}
}
