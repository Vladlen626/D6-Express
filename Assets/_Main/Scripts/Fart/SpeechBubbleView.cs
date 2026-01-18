using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class SpeechBubbleView : MonoBehaviour
{
	[SerializeField]
	private LocalizedText textLine;

	[SerializeField]
	private float duration;

	public async UniTask ShowLine(string line)
	{
		textLine.SetText(line);

		textLine.gameObject.SetActive(true);
		
		await UniTask.Delay(TimeSpan.FromSeconds(duration));

		textLine.gameObject.SetActive(false);
	}
}