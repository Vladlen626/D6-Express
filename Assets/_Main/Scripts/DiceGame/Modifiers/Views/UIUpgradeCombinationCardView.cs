using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;

public class UIUpgradeCombinationCardView : UIBaseElement
{
    [SerializeField]
    private LocalizedText title;

    [SerializeField]
    private TextMeshProUGUI minValue;

    [SerializeField]
    private TextMeshProUGUI maxValue;

    [SerializeField]
    private TextMeshProUGUI bonusValue;

    public void SetTitle(string id)
    {
        title.SetText(id);
    }

    public void SetStats(
        string minLabel,
        string maxLabel,
        string bonusLabel,
        int minStatValue,
        int maxStatValue,
        int bonusStatValue)
    {
        if (minValue)
        {
            minValue.text = $"{minLabel} {minStatValue}";
        }

        if (maxValue)
        {
            maxValue.text = $"{maxLabel} {maxStatValue}";
        }

        if (bonusValue)
        {
            bonusValue.text = $"{bonusLabel} {bonusStatValue}";
        }
    }
}
