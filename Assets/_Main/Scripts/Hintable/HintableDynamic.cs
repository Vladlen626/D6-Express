using UnityEngine;

public class HintableDynamic : MonoBehaviour, IHintable
{
	private string text;
	private bool highlighted;

	public string HintText => text;
	public bool Highlighted => highlighted;

	public void SetText(string text, bool highlighted = false)
	{
		this.text = text;
		this.highlighted = highlighted;
	}
}