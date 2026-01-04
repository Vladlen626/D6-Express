using UnityEngine;

public class HintableDynamic : MonoBehaviour, IHintable
{
    private string text;

    public void SetText(string text)
    {
        this.text = text;
    }

    public string HintText => text;
}