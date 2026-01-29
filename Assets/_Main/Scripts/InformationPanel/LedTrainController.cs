using System.Collections.Generic;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

public class LedTrainController : IBaseController, IActivatable
{
    private readonly Run run;
    private readonly IEnumerable<LedTrainView> views;

    public LedTrainController(Run run, IEnumerable<LedTrainView> views)
    {
        this.run = run;
        this.views = views;
    }

    public void Activate()
    {
        run.DayChanged += OnDayChanged;
        run.DaysPerLevelChanged += OnDayChanged;

        OnDayChanged();
    }

    public void Deactivate()
    {
        run.DaysPerLevelChanged -= OnDayChanged;
        run.DayChanged -= OnDayChanged;
    }

    private void OnDayChanged()
    {
        foreach (var item in views)
        {
            item.SetText("days_progress", (run.DaysPerLevel - run.Day).ToString());
        }
    }
}
