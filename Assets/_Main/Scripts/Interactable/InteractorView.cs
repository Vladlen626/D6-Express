using TMPro;
using UnityEngine;

public class InteractorView : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI hintText;

	[SerializeField]
	private Interactor interactor;

	public void Initialize(Interactor inInteractor)
	{
		interactor = inInteractor;
		interactor.Noticed += OnNoticed;
		interactor.Missed += OnMissed;
	}

	public void Deactivate()
	{
		interactor.Missed -= OnMissed;
		interactor.Noticed -= OnNoticed;
	}

	private void OnDestroy()
	{
		Deactivate();
	}

	private void OnNoticed(Interactable interactable)
	{
		if (interactable.TryGetComponent<Hintable>(out var hintable))
		{
			hintText.text = hintable.HintText;
		}
	}

	private void OnMissed(Interactable interactable)
	{
		hintText.text = string.Empty;
	}
}