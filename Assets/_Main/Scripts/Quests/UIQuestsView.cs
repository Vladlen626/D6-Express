using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;

public class UIQuestsView : UIBaseElement
{
    [SerializeField]
    private TextMeshProUGUI hints;

    public void SetHints(string hint)
    {
        hints.text = hint;
    }
}