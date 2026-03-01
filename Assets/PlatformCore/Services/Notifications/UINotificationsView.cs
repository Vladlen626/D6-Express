using PlatformCore.Services.UI;
using UnityEngine;

public class UINotificationsView : UIBaseElement
{
	[SerializeField] 
	private Transform list;

	public Transform List => list;

	protected override void OnAwake()
	{
		base.OnAwake();
		var rect = transform as RectTransform;
		if (rect && rect.parent is RectTransform)
		{
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;
			rect.offsetMin = Vector2.zero;
			rect.offsetMax = Vector2.zero;
			rect.pivot = new Vector2(0.5f, 0.5f);
		}

		if (!list)
		{
			list = transform.Find("PanelNotifications");
		}
	}
}
