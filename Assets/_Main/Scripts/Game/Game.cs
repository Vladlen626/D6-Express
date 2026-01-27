using System;

public class D6Game
{
	public Location Location { get; private set; }

	public event Action<Location> LocationChangeRequested;
	public event Action LocationChanged;
	public event Action TickChanged;
	public event Action DayChanged;

	public void NotifyTickChanged()
	{
		TickChanged?.Invoke();
	}

	public void NotifyDayChanged()
	{
		DayChanged?.Invoke();
	}

	public void RequestSetLocation(Location location)
	{
		LocationChangeRequested?.Invoke(location);
	}

	public void SetLocation(Location location)
	{
		Location = location;
		LocationChanged?.Invoke();
	}
}