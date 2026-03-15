using PlatformCore.Core;
using PlatformCore.Services.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class UIButtonClickSound : MonoBehaviour, IPointerClickHandler, ISubmitHandler
{
	private IAudioService audioService;
	private Button button;

	private void Awake()
	{
		button = GetComponent<Button>();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData != null && eventData.button != PointerEventData.InputButton.Left)
		{
			return;
		}

		PlayClickSoundIfAllowed();
	}

	public void OnSubmit(BaseEventData eventData)
	{
		PlayClickSoundIfAllowed();
	}

	private void PlayClickSoundIfAllowed()
	{
		if (!Application.isPlaying)
		{
			return;
		}

		if (!button || !button.IsInteractable())
		{
			return;
		}

		audioService ??= Locator.Resolve<IAudioService>();
		audioService?.PlaySound(SoundNames.Button);
	}
}
