using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public enum RunScalarTarget
	{
		TicksPerDay = 0
	}

	public class AdjustRunScalarModifier : IOnLevelStartModifier, IModifierUiConfigProvider
	{
		public readonly RunScalarTarget target;
		public readonly int delta;
		public readonly bool revertOnLevelOrRunEnd;
		public string UiConfigId { get; }

		private bool isApplied;
		private Run run;

		public AdjustRunScalarModifier(
			RunScalarTarget target,
			int delta,
			bool revertOnLevelOrRunEnd = true,
			string uiConfigId = null)
		{
			this.target = target;
			this.delta = delta;
			this.revertOnLevelOrRunEnd = revertOnLevelOrRunEnd;
			UiConfigId = uiConfigId;
		}

		public UniTask ModifyValues(DiceModifierContext modifierContext)
		{
			if (modifierContext.Run == null || isApplied)
			{
				return UniTask.CompletedTask;
			}

			run = modifierContext.Run;
			ApplyDelta(run, delta);
			if (revertOnLevelOrRunEnd)
			{
				run.LevelChanged += OnLevelChanged;
				run.RunFinished += OnRunFinished;
			}

			isApplied = true;
			return UniTask.CompletedTask;
		}

		private void OnLevelChanged()
		{
			Reset();
		}

		private void OnRunFinished(bool result)
		{
			Reset();
		}

		private void Reset()
		{
			if (!isApplied || run == null)
			{
				return;
			}

			if (revertOnLevelOrRunEnd)
			{
				run.LevelChanged -= OnLevelChanged;
				run.RunFinished -= OnRunFinished;
			}

			ApplyDelta(run, -delta);
			run = null;
			isApplied = false;
		}

		private void ApplyDelta(Run targetRun, int valueDelta)
		{
			switch (target)
			{
				case RunScalarTarget.TicksPerDay:
					targetRun.SetTicksPerDay(Mathf.Max(1, targetRun.TicksPerDay + valueDelta));
					return;
			}
		}
	}
}
