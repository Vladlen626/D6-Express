using System.Collections.Generic;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

public class LightController : IBaseController, IActivatable
{
    private readonly Run run;
    private readonly List<LightView> lightViews;

    public LightController(IEnumerable<LightView> lights, Run levelModel)
    {
        this.run = levelModel;
        this.lightViews = new(lights);
    }

    public void Activate()
    {
        run.TickChanged += OnSessionsChanged;
        OnSessionsChanged();
    }

    public void Deactivate()
    {
        run.TickChanged -= OnSessionsChanged;
    }

    private void OnSessionsChanged()
    {
        // todo: в конфиг 
        var ratio = run.Tick > 0 ? (run.Tick / run.TicksPerDay) : 0;
        var lightNeeded = ratio >= 0.5f;

        foreach (var item in lightViews)
        {
            item.SetState(lightNeeded);
        }
    }
}