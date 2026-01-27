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

    public Gradient LightColor => lightColor;
    public AnimationCurve LightIntensity => lightIntensity;
    public AnimationCurve RatioModifier => ratioModifier;
}