using System;

public class LevelModel
{
    private LevelState levelState = LevelState.TRAIN;

    public readonly int CashGoal;
    public readonly int TicksPerDay;
    public readonly int Days;

    public LevelState LevelState => levelState;
    public float TickRatio => Tick / (float)TicksPerDay;
    public int Tick { get; private set; }
    public int Day { get; private set; }

    public event Action TickChanged;
    public event Action DayChanged;
    public event Action LevelFinished;
    public event Action LevelStateChanged;

    public LevelModel(int ticksPerDay, int days, int cashGoal)
    {
        Days = days;
        TicksPerDay = ticksPerDay;
        CashGoal = cashGoal;
    }

    public void SetLevelState(LevelState levelState)
    {
        this.levelState = levelState;
        LevelStateChanged?.Invoke();
    }

    public void IncrementDays()
    {
        Tick = 0;
        TickChanged?.Invoke();

        if (Day + 1 == Days)
        {
            LevelFinished?.Invoke();
        }
        else
        {
            Day++;
            DayChanged?.Invoke();
        }
    }

    public void IncrementTicks()
    {
        if (Tick + 1 < TicksPerDay)
        {
            Tick++;
            TickChanged?.Invoke();
        }
        else
        {
            IncrementDays();
        }
    }
}