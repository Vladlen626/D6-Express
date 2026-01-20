using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;

public class UILevelView : UIBaseElement
{
    [Header("DaynightCycle")]
    [SerializeField]
    private Gradient lightColor;

    [SerializeField]
    private AnimationCurve lightIntensity;

    [SerializeField]
    private AnimationCurve ratioModifier;

    [SerializeField]
    private LocalizedText ticks;

    [SerializeField]
    private LocalizedText days;

    [SerializeField]
    private LocalizedText cashProgress;

    public Gradient LightColor => lightColor;
    public AnimationCurve LightIntensity => lightIntensity;
    public AnimationCurve RatioModifier => ratioModifier;

    public void SetTicksText(string id, params string[] args)
    {
        ticks.SetText(id, args);
    }

    public void SetDaysText(string id, params string[] args)
    {
        days.SetText(id, args);
    }

    public void SetCashProgress(string id, params string[] args)
    {
        cashProgress.SetText(id, args);
    }
}