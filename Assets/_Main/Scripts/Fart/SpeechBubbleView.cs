using System;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services;
using UnityEngine;

public class SpeechBubbleView : MonoBehaviour
{
	[SerializeField]
	private Transform bubbleTransform;

	[SerializeField]
	private LocalizedText textLine;

	[SerializeField]
	private float duration;

	private ICameraService cameraService;

	bool showing;

	public async UniTask ShowLine(string line)
	{
		textLine.SetText(line);

		showing = true;
		textLine.gameObject.SetActive(true);

		await UniTask.Delay(TimeSpan.FromSeconds(duration));

		showing = false;
		textLine.gameObject.SetActive(false);
	}

	private void LateUpdate()
	{
		if (showing)
		{
			if (cameraService == null)
			{
				cameraService = Locator.Resolve<ICameraService>();
			}

			var cameraTfm = cameraService.GetCameraTransform();
			bubbleTransform.rotation = Quaternion.LookRotation(
				cameraTfm.position - bubbleTransform.position,
				Vector3.up
			);
		}
	}
}