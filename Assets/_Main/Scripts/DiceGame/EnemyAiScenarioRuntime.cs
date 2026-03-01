using System;

namespace _Main.Scripts.Dice
{
	public class EnemyAiScenarioRuntime
	{
		public EnemyAiScenarioConfig Scenario { get; }
		public int CurrentTurnIndex { get; private set; }
		public int ExecutedEnemyTurns { get; private set; }
		public bool IsCompleted => CurrentTurnIndex >= Scenario.turns.Count;
		public bool IsFailed { get; private set; }
		public string FailureReason { get; private set; }

		public EnemyAiScenarioRuntime(EnemyAiScenarioConfig scenario)
		{
			Scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
		}

		public EnemyAiTurnConfig GetCurrentTurnOrNull()
		{
			if (IsCompleted)
			{
				return null;
			}

			return Scenario.turns[CurrentTurnIndex];
		}

		public void MarkTurnCompleted()
		{
			ExecutedEnemyTurns++;
			CurrentTurnIndex++;
		}

		public void MarkFailed(string reason)
		{
			IsFailed = true;
			FailureReason = reason;
		}
	}
}
