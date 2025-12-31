using UnityEngine;

public class LevelView : MonoBehaviour
{
    [Header("DaynightCycle")]
    [SerializeField]
    private Light sun;

    [SerializeField]
    private Gradient lightColor;

    [SerializeField]
    private AnimationCurve lightIntensity;

    public Light Sun => sun;
    public Gradient LightColor => lightColor;
    public AnimationCurve LightIntensity => lightIntensity;
}