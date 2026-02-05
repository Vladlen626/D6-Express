using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Active modifier: when armed via the on-screen button, the next Pass gains a 1.5x score multiplier.
	/// Only one activation is available per in-game day.
	/// </summary>
	public class PassActivationMultiplierModifier : IOnPassModifier, IOnRoundStartModifier
	{
		public const float ScoreMultiplier = 1.5f;
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
			switch (modifierContext.Stage)
			{
				case ModifierStage.RoundStart:
					AttachRun(modifierContext.Run);
					RefreshDailyAllowance();
					Debug.Log($"[PassMultiplier] RoundStart | day={lastKnownDay} remaining={remainingActivations} armed={isArmed}");
					PassActivationMultiplierOverlay.UpdateState(remainingActivations, isArmed, true);
					return UniTask.CompletedTask;

				case ModifierStage.Pass:
					AttachRun(modifierContext.Run);
					RefreshDailyAllowance();
					Debug.Log($"[PassMultiplier] Pass stage | day={lastKnownDay} remaining={remainingActivations} armed={isArmed}");
					break;

				default:
					return UniTask.CompletedTask;
			}

			if (!isArmed)
			{
				PassActivationMultiplierOverlay.UpdateState(remainingActivations, isArmed, true);
				return UniTask.CompletedTask;
			}

			ApplyMultiplier(modifierContext.CombinationResult);
			Debug.Log($"[PassMultiplier] Applied x{ScoreMultiplier} | combos={modifierContext.CombinationResult.Combinations?.Count ?? 0}");

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
				Debug.Log("[PassMultiplier] Activation click ignored: no charges left");
				return;
			}

			isArmed = true;
			Debug.Log($"[PassMultiplier] Armed for next pass | day={lastKnownDay} remaining={remainingActivations}");
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
				Debug.Log($"[PassMultiplier] New day detected -> charges reset to {remainingActivations}");
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

			Debug.Log($"[PassMultiplier] Attached to run | day={run.Day} level={run.Level}");
			PassActivationMultiplierOverlay.UpdateState(remainingActivations, isArmed, true);
		}

		private void OnRunDayChanged()
		{
			RefreshDailyAllowance();
			Debug.Log($"[PassMultiplier] Run day changed -> remaining={remainingActivations} armed={isArmed}");
			PassActivationMultiplierOverlay.UpdateState(remainingActivations, isArmed, true);
		}

		private void OnLevelChanged()
		{
			lastKnownDay = -1;
			RefreshDailyAllowance();
			Debug.Log($"[PassMultiplier] Level changed -> day reset, remaining={remainingActivations} armed={isArmed}");
			PassActivationMultiplierOverlay.UpdateState(remainingActivations, isArmed, true);
		}

		private void OnRunFinished(bool result)
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

			Debug.Log("[PassMultiplier] Run finished -> overlay hidden and state reset");
			PassActivationMultiplierOverlay.UpdateState(remainingActivations, isArmed, false);
		}
	}
}
