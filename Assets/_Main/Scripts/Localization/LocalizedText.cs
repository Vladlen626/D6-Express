using PlatformCore.Core;
using TMPro;
using UnityEngine;

public class LocalizedText : MonoBehaviour
{
    [SerializeField]
    private TMP_Text text;

    public void SetText(string id)
    {
        text.text = Locator.Resolve<ILocalizationService>().GetLocalized(id);
    }
}