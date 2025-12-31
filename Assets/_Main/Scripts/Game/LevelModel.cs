using System;

public class LevelModel
{
    private readonly int ticksPerDay;
    private readonly int days;

    public float TickRatio => Tick / (float)ticksPerDay;
    public int Tick { get; private set; }
    public int Day { get; private set; }

    public event Action TickChanged;
    public event Action DayChanged;
    public event Action LevelFinished;

    public LevelModel(int ticksPerDay, int days)
    {
        this.days = days;
        this.ticksPerDay = ticksPerDay;
    }

    public void IncrementDays()
    {
        if (Day + 1 == days)
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
        if (Tick + 1 < ticksPerDay)
        {
            Tick++;
            TickChanged?.Invoke();
        }
        else
        {
            Tick = 0;
            TickChanged?.Invoke();
            IncrementDays();
        }
    }
}