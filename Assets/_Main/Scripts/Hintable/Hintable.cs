using UnityEngine;

public class Hintable : MonoBehaviour, IHintable
{
	[SerializeField] 
	private string hintText;

	[SerializeField]
	private bool highlighted;

	public string HintText => hintText;
	public bool Highlighted => highlighted;
}