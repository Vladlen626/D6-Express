using PlatformCore.Core;
using TMPro;
using UnityEngine;

public class LocalizedText : MonoBehaviour
{
    [SerializeField]
    private TMP_Text text;

    public TMP_Text Tmp => text;

    public void SetText(string id)
    {
        text.text = Locator.Resolve<ILocalizationService>().GetLocalized(id);
    }

    public void SetRawText(string text)
    {
        this.text.text = text;
    }

    public void SetText(string id, params string[] agrs)
    {
        var localized = Locator.Resolve<ILocalizationService>().GetLocalized(id);
        text.text = string.Format(localized, agrs);
    }
}