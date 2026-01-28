using System;

public class Run
{
    public int Level { get; private set; }
    public int Day { get; private set; }
    public int Tick { get; private set; }
    public string StationId { get; private set; }
    public int DaysPerLevel { get; private set; }
    public int TicksPerDay { get; private set; }
    public int LevelsCount { get; private set; }
    public int TicketPrice { get; private set; }
    public int NextTicketPrice { get; private set; }

    public event Action<int> TickChangeRequested;
    public event Action<int> DayChangeRequested;
    public event Action LevelChangeRequested;

    public event Action TickChanged;
    public event Action TicksPerDayChanged;
    public event Action DayChanged;
    public event Action DaysPerLevelChanged;
    public event Action LevelChanged;
    public event Action LevelsCountChanged;
    public event Action TicketPriceChanged;
    public event Action NextTicketPriceChanged;
    public event Action RunStarted;
    public event Action<bool> RunFinished;

    public void SetLevelData(string stationId, int daysPerLevel, int tickPerDay, int levels, int ticketPrice, int nextTicketPrice)
    {
        StationId = stationId;
        SetTick(0);
        SetDay(0);
        SetDaysPerLevel(daysPerLevel);
        SetTicksPerDay(tickPerDay);
        SetLevelsCount(levels);
        SetTicketPrice(ticketPrice);
        SetNextTicketPrice(nextTicketPrice);
    }

    public void Start()
    {
        RunStarted?.Invoke();
    }

    public void RequestIncrementTick()
    {
        RequestSetTick(Tick + 1);
    }

    public void RequestSetTick(int value)
    {
        if (value > TicksPerDay)
        {
            RequestSetDay(Day + 1);
        }
        else
        {
            TickChangeRequested?.Invoke(value);
        }
    }

    public void SetTick(int value)
    {
        Tick = value;
        TickChanged?.Invoke();
    }

    public void SetTicksPerDay(int value)
    {
        TicksPerDay = value;
        TicksPerDayChanged?.Invoke();
    }

    public void RequestIncrementDay()
    {
        RequestSetDay(Day + 1);
    }

    public void RequestSetDay(int value)
    {
        DayChangeRequested?.Invoke(value);
    }

    public void SetDay(int value)
    {
        Day = value;
        DayChanged?.Invoke();

        SetTick(0);
    }

    public void SetDaysPerLevel(int value)
    {
        DaysPerLevel = value;
        DaysPerLevelChanged?.Invoke();
    }

    public void SetTicketPrice(int value)
    {
        TicketPrice = value;
        TicketPriceChanged?.Invoke();
    }

    public void SetNextTicketPrice(int value)
    {
        NextTicketPrice = value;
        NextTicketPriceChanged?.Invoke();
    }

    public void SetLevelsCount(int value)
    {
        LevelsCount = value;
        LevelsCountChanged?.Invoke();
    }

    public void RequestChangeLevel()
    {
        LevelChangeRequested?.Invoke();
    }

    public void FinishLevel(bool result)
    {
        if (result)
        {
            Level++;
            LevelChanged?.Invoke();
        }
    }

    public void FinishRun(bool result)
    {
        RunFinished?.Invoke(result);
    }
}