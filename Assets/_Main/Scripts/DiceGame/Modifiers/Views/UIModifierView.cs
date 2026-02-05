using PlatformCore.Services.UI;
using UnityEngine;

public class UIModifierView : UIBaseElement
{
    [SerializeField]
    private LocalizedText title;

    [SerializeField]
    private LocalizedText description;

    [SerializeField]
    private LocalizedText value;

    public void SetTitle(string id)
    {
        title.SetText(id);
    }

    public void SetDescription(string id)
    {
        description.SetText(id);
    }

    public void SetValue(string text, params string[] values)
    {
        value.SetText(text, values);
    }
}