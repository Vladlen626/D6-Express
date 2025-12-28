using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UILayerOnTop : MonoBehaviour
{
	//TODO: ИЗбавиться от этого говнища. Нужно сделать систему слоев для UI.
	// Enum словев, и чтоб можно было атрибутом к классу выставлять на каком уровне его показывать.
	
	[SerializeField] private int sortingOrder = 5000;
	[SerializeField] private bool screenSpaceOverlay = true;

	void Awake()
	{
		var c = GetComponent<Canvas>();
		if (!c) c = gameObject.AddComponent<Canvas>();

		c.overrideSorting = true;
		c.sortingOrder = sortingOrder;
		if (screenSpaceOverlay) c.renderMode = RenderMode.ScreenSpaceOverlay;

		if (!GetComponent<GraphicRaycaster>())
			gameObject.AddComponent<GraphicRaycaster>();
	}
}