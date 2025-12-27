using TMPro;
using UnityEngine;

public class InteractorView : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI hintText;

	[SerializeField]
	private Interactor interactor;

	private void OnEnable()
	{
		interactor.Noticed += OnNoticed;
		interactor.Missed += OnMissed;
	}

	private void OnDisable()
	{
		interactor.Missed -= OnMissed;
		interactor.Noticed -= OnNoticed;
	}

	private void OnNoticed(GameObject interactable)
	{
		hintText.text = interactable.name;
	}

	private void OnMissed(GameObject interactable)
	{
		hintText.text = string.Empty;
	}
}
