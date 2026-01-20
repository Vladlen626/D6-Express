using System;
using PlatformCore.Core;

public class LevelModel
{
	public int Days { get; private set; }
	public int Ticks { get; private set; }
	public int CashGoal { get; private set; }
	public int TicketPrice { get; private set; }
	public int Day { get; private set; }
	public int Tick { get; private set; }
	public bool IsLevelFinished { get; private set; }
	public float TickRatio => Tick / (float)Ticks;
	public bool IsFinalDay => Day + 1 >= Days;
	public bool IsTicksLeft => Tick + 1 < Ticks;

	public event Action TickChanged;
	public event Action DayChanged;
	public event Action OnFinalDay;
	public event Action<bool> LevelFinished;

	public void UpdateLevel(LevelData levelData, int ticketPrice)
	{
		Tick = 0;
		Day = 0;

		Days = levelData.days;
		Ticks = levelData.ticks;
		CashGoal = levelData.cashGoal;

		TicketPrice = ticketPrice;

		IsLevelFinished = false;
	}

	public void IncrementDays()
	{
		Tick = 0;
		TickChanged?.Invoke();

		if (Day + 1 == Days)
		{
			Day = 0;
			DayChanged?.Invoke();
			OnFinalDay?.Invoke();
		}
		else
		{
			Day++;
			DayChanged?.Invoke();
		}
	}

	public void IncrementTicks()
	{
		if (Tick + 1 < Ticks)
		{
			Tick++;
			TickChanged?.Invoke();
		}
		else
		{
			IncrementDays();
		}
	}


	// ReSharper disable Unity.PerformanceAnalysis
	public void SetLevelFinished(bool success)
	{
		IsLevelFinished = true;
		LevelFinished?.Invoke(success);
	}
}