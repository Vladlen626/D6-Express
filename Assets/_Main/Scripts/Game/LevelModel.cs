using System;

public class LevelModel
{
    public readonly int TicksPerDay;
    public readonly int Days;

    public float TickRatio => Tick / (float)TicksPerDay;
    public int Tick { get; private set; }
    public int Day { get; private set; }

    public event Action TickChanged;
    public event Action DayChanged;
    public event Action LevelFinished;

    public LevelModel(int ticksPerDay, int days)
    {
        this.Days = days;
        this.TicksPerDay = ticksPerDay;
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