using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.UI;
using UnityEngine;

public class LevelViewController : BaseContextController<UILevelView>
{
    private readonly LevelModel levelModel;
    private readonly Light sun;

    public LevelViewController(IUIService uiService, LevelModel levelModel, Light sun) :  base(uiService)
    {
        this.levelModel = levelModel;
        this.sun = sun;
    }

    protected override void OnActivate()
    {
        levelModel.TickChanged += OnTickChanged;
        levelModel.DayChanged += OnDaysChanged;
        OnTickChanged();
        OnDaysChanged();
    }

    protected override void OnDeactivate()
    {
        levelModel.DayChanged -= OnDaysChanged;
        levelModel.TickChanged -= OnTickChanged;
    }

    private void OnTickChanged()
    {
        sun.transform.rotation = Quaternion.Euler(levelModel.TickRatio * 360f - 90f, 170f, 0f);
        sun.color = _context.LightColor.Evaluate(levelModel.TickRatio);
        sun.intensity = _context.LightIntensity.Evaluate(levelModel.TickRatio);

        _context.SetTicksText($"Ticks: {levelModel.Tick} / {levelModel.TicksPerDay}");
    }

    private void OnDaysChanged()
    {
        _context.SetDaysText($"Days: {levelModel.Day} / {levelModel.Days}");
    }
}