using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;

public class UIStatsView : UIBaseElement
{
	[SerializeField] private TextMeshProUGUI locationName;

	[SerializeField] private UIModifiersView uIModifiersView;

	public UIModifiersView UIModifiersView => uIModifiersView;

	public void SetMessage(string name)
	{
		locationName.text = name;
	}
}