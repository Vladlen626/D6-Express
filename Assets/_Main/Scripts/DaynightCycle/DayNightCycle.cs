using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [SerializeField]
    private Light sun;

    [SerializeField]
    private float dayLengthInMinutes = 2f;

    [SerializeField]
    private Gradient lightColor;

    [SerializeField]
    private AnimationCurve lightIntensity;

    private float time; // 0–1

    private void Update()
    {
        time += Time.deltaTime / (dayLengthInMinutes * 60f);
        time %= 1f;

        sun.transform.rotation = Quaternion.Euler(time * 360f - 90f, 170f, 0f);

        sun.color = lightColor.Evaluate(time);
        sun.intensity = lightIntensity.Evaluate(time);
    }
}