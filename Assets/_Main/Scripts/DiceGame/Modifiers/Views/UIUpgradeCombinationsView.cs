using PlatformCore.Services.UI;
using UnityEngine;

public class UIUpgradeCombinationsView : UIBaseElement
{
    [SerializeField]
    private Transform list;

    [SerializeField]
    private LocalizedText header;

    public Transform List => list;
    public LocalizedText Header => header;
}
