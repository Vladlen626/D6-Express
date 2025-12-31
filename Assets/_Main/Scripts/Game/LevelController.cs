using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using UnityEngine;

public class LevelController : IBaseController, IActivatable
{
    private readonly LevelModel levelModel;
    private readonly LevelView levelView;

    public LevelController(LevelModel levelModel, LevelView levelView)
    {
        this.levelModel = levelModel;
        this.levelView = levelView;
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
        levelView.Sun.transform.rotation = Quaternion.Euler(levelModel.TickRatio * 360f - 90f, 170f, 0f);
        levelView.Sun.color = levelView.LightColor.Evaluate(levelModel.TickRatio);
        levelView.Sun.intensity = levelView.LightIntensity.Evaluate(levelModel.TickRatio);
    }
}