using System;
using DG.Tweening;
using PlatformCore.Core;
using PlatformCore.Services.Audio;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Main.Scripts.Game.Views
{
	public class ButtonView : MonoBehaviour
	{
		[SerializeField] private Transform buttonModel;
		[SerializeField] private Collider buttonCollider;
		[SerializeField] private Transform greenButton;
		[SerializeField] private Transform redButton;
		public event Action OnClicked;

		private bool isInteractable = true;
		private Camera mainCamera;
		private IAudioService audioService;

		private void Start()
		{
			mainCamera = Camera.main;
			audioService = Locator.Resolve<IAudioService>();
			SetInteractable(true);
		}

		private void Update()
		{
			if (!isInteractable)
			{
				return;
			}

			if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
			{
				CheckClick();
			}
		}

		private void CheckClick()
		{
			Vector2 mousePos = Mouse.current.position.ReadValue();
			Ray ray = mainCamera.ScreenPointToRay(mousePos);

			if (Physics.Raycast(ray, out RaycastHit hit))
			{
				if (hit.collider == buttonCollider)
				{
					OnClicked?.Invoke();
					PlayButtonSound();
					PlayClickAnimation();
				}
			}
		}

		private void PlayClickAnimation()
		{
			buttonModel.DOScale(0.9f, 0.1f).OnComplete(() =>
				buttonModel.DOScale(1f, 0.1f));
		}

		private void PlayButtonSound()
		{
			audioService ??= Locator.Resolve<IAudioService>();
			audioService?.PlaySound(SoundNames.Button);
		}
		
		public void SetInteractable(bool interactable)
		{
			isInteractable = interactable;
			greenButton.gameObject.SetActive(interactable);
			redButton.gameObject.SetActive(!interactable);
		}
	}
}
