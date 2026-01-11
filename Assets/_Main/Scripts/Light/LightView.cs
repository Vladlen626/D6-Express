using UnityEngine;

public class LightView : MonoBehaviour
{
    [SerializeField]
    private Light[] lights;

    public void SetState(bool state)
    {
        foreach (var item in lights)
        {
            item.enabled = state;
        }
    }
}