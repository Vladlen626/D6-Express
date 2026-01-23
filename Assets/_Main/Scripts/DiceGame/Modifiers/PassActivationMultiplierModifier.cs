using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Active modifier: when armed via the on-screen button, the next Pass gains a 1.5x score multiplier.
	/// Only one activation is available per in-game day.
	/// </summary>
	public class PassActivationMultiplierModifier : IOnPassModifier
	{
		private const float ScoreMultiplier = 1.5f;
		private const int ActivationsPerDay = 1;

		private Run run;
		private int lastKnownDay = -1;
		private int remainingActivations = ActivationsPerDay;
		private bool isArmed;

		public PassActivationMultiplierModifier()
		{
			PassActivationMultiplierOverlay.RegisterActivateCallback(OnActivationRequested);
			PassActivationMultiplierOverlay.UpdateState(remainingActivations, isArmed, false);
		}

		public UniTask ModifyValues(DiceModifierContext modifierContext)
		{
			if (modifierContext.Stage != ModifierStage.Pass)
			{
				return UniTask.CompletedTask;
			}

			AttachRun(modifierContext.Run);
			RefreshDailyAllowance();

			if (!isArmed)
			{
				PassActivationMultiplierOverlay.UpdateState(remainingActivations, isArmed, true);
				return UniTask.CompletedTask;
			}

			ApplyMultiplier(modifierContext.CombinationResult);

			isArmed = false;
			remainingActivations = Mathf.Max(0, remainingActivations - 1);
			PassActivationMultiplierOverlay.UpdateState(remainingActivations, isArmed, true);

			return UniTask.CompletedTask;
		}

		private static void ApplyMultiplier(DiceCombinationResult combinationResult)
		{
			if (combinationResult.Combinations == null)
			{
				return;
			}

			foreach (var entry in combinationResult.Combinations)
			{
				entry.BaseScore = Mathf.RoundToInt(entry.BaseScore * ScoreMultiplier);
			}
		}

		private void OnActivationRequested()
		{
			RefreshDailyAllowance();

			if (remainingActivations <= 0)
			{
				return;
			}

			isArmed = true;
			PassActivationMultiplierOverlay.UpdateState(remainingActivations, isArmed, true);
		}

		private void RefreshDailyAllowance()
		{
			if (run == null)
			{
				return;
			}

			if (run.Day != lastKnownDay)
			{
				lastKnownDay = run.Day;
				remainingActivations = ActivationsPerDay;
				isArmed = false;
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
				run.RunFinished -= OnRunFinished;
				run.LevelChanged -= OnLevelChanged;
			}

			run = newRun;
			lastKnownDay = -1;
			RefreshDailyAllowance();

			run.DayChanged += OnRunDayChanged;
			run.RunFinished += OnRunFinished;
			run.LevelChanged += OnLevelChanged;

			PassActivationMultiplierOverlay.UpdateState(remainingActivations, isArmed, true);
		}

		private void OnRunDayChanged()
		{
			RefreshDailyAllowance();
			PassActivationMultiplierOverlay.UpdateState(remainingActivations, isArmed, true);
		}

		private void OnLevelChanged()
		{
			lastKnownDay = -1;
			RefreshDailyAllowance();
			PassActivationMultiplierOverlay.UpdateState(remainingActivations, isArmed, true);
		}

		private void OnRunFinished()
		{
			remainingActivations = ActivationsPerDay;
			isArmed = false;
			lastKnownDay = -1;

			if (run != null)
			{
				run.DayChanged -= OnRunDayChanged;
				run.RunFinished -= OnRunFinished;
				run.LevelChanged -= OnLevelChanged;
				run = null;
			}

			PassActivationMultiplierOverlay.UpdateState(remainingActivations, isArmed, false);
		}
	}
}
