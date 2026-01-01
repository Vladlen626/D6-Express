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

    [Header("UI")]
    [SerializeField]
    private TextMeshProUGUI ticks;

    [SerializeField]
    private TextMeshProUGUI days;
    public Gradient LightColor => lightColor;
    public AnimationCurve LightIntensity => lightIntensity;
    public AnimationCurve RatioModifier => ratioModifier;

    public void SetTicksText(string text)
    {
        ticks.text = text;
    }

    public void SetDaysText(string text)
    {
        days.text = text;
    }
}