using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Example clickable item: when armed, the next Pass gains a score multiplier.
	/// Shows how to reuse the modifier pipeline while driving behaviour from a 3D item.
	/// </summary>
	public class PassMultiplierItem : DiceItemBase, IOnPassModifier, IOnRoundStartModifier
	{
		private readonly float scoreMultiplier;
		private readonly int activationsPerDay;
		private Run run;
		private int lastDay = -1;
		private int remaining;

		public PassMultiplierItem(float scoreMultiplier = 1.5f, int activationsPerDay = 1)
			: base("pass_multiplier_item", "Pass Multiplier", DiceItemActivationType.ClickToActivate)
		{
			this.scoreMultiplier = Mathf.Max(1f, scoreMultiplier);
			this.activationsPerDay = Mathf.Max(1, activationsPerDay);
			remaining = activationsPerDay;
		}

		public override UniTask ModifyValues(DiceModifierContext modifierContext)
		{
			switch (modifierContext.Stage)
			{
				case ModifierStage.RoundStart:
					AttachRun(modifierContext.Run);
					RefreshDailyAllowance();
					break;

				case ModifierStage.Pass:
					AttachRun(modifierContext.Run);
					RefreshDailyAllowance();
					ApplyIfArmed(modifierContext.CombinationResult);
					break;
			}

			return UniTask.CompletedTask;
		}

		protected override bool OnClick()
		{
			if (remaining <= 0)
			{
				return false;
			}

			if (State == DiceItemState.Ready)
			{
				SetState(DiceItemState.Armed);
				return true;
			}

			return false;
		}

		private void ApplyIfArmed(DiceCombinationResult combinationResult)
		{
			if (State != DiceItemState.Armed || remaining <= 0 || combinationResult.Combinations == null)
			{
				return;
			}

			foreach (var entry in combinationResult.Combinations)
			{
				entry.BaseScore = Mathf.RoundToInt(entry.BaseScore * scoreMultiplier);
			}

			remaining = Mathf.Max(0, remaining - 1);
			SetState(remaining > 0 ? DiceItemState.Ready : DiceItemState.Cooldown);
		}

		private void RefreshDailyAllowance()
		{
			if (run == null)
			{
				return;
			}

			if (run.Day != lastDay)
			{
				lastDay = run.Day;
				remaining = activationsPerDay;
				SetState(DiceItemState.Ready);
			}
		}

		private void AttachRun(Run newRun)
		{
			if (newRun == null || ReferenceEquals(run, newRun))
			{
				return;
			}

			if (run != null)
			{
				run.DayChanged -= OnRunDayChanged;
				run.LevelChanged -= OnLevelChanged;
				run.RunFinished -= OnRunFinished;
			}

			run = newRun;
			run.DayChanged += OnRunDayChanged;
			run.LevelChanged += OnLevelChanged;
			run.RunFinished += OnRunFinished;
		}

		private void OnRunDayChanged() => RefreshDailyAllowance();

		private void OnLevelChanged()
		{
			lastDay = -1;
			RefreshDailyAllowance();
		}

		private void OnRunFinished()
		{
			remaining = activationsPerDay;
			SetState(DiceItemState.Ready);
			if (run != null)
			{
				run.DayChanged -= OnRunDayChanged;
				run.LevelChanged -= OnLevelChanged;
				run.RunFinished -= OnRunFinished;
				run = null;
			}
		}

		public override void ResetItem()
		{
			base.ResetItem();
			remaining = activationsPerDay;
			lastDay = -1;
		}
	}
}
