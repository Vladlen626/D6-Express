using System.Collections.Generic;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

public class LightController : IBaseController, IActivatable
{
    private readonly LevelModel levelModel;
    private readonly List<LightView> lightViews;

    public LightController(IEnumerable<LightView> lights, LevelModel levelModel)
    {
        this.levelModel = levelModel;
        this.lightViews = new(lights);
    }

    public void Activate()
    {
        levelModel.TickChanged += OnTickChanged;
        OnTickChanged();
    }

    public void Deactivate()
    {
        levelModel.TickChanged -= OnTickChanged;
    }

    private void OnTickChanged()
    {
        // todo: в конфиг 
        var lightNeeded = levelModel.TickRatio >= 0.5f;

        foreach (var item in lightViews)
        {
            item.SetState(lightNeeded);
        }
    }
}