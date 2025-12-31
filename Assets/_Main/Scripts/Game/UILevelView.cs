using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;

public class UILevelView : UIBaseElement
{
    [Header("DaynightCycle")]
    [SerializeField]
    private Light sun;

    [SerializeField]
    private Gradient lightColor;

    [SerializeField]
    private AnimationCurve lightIntensity;

    [Header("UI")]
    [SerializeField]
    private TextMeshProUGUI ticks;

    [SerializeField]
    private TextMeshProUGUI days;
    public Gradient LightColor => lightColor;
    public AnimationCurve LightIntensity => lightIntensity;

    public void SetTicksText(string text)
    {
        ticks.text = text;
    }

    public void SetDaysText(string text)
    {
        days.text = text;
    }
}