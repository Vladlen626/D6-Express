using System;
using _Main.Scripts.SceneOrchestra;
using UnityEngine;

public class BaseTrigger : MonoBehaviour, ITrigger
{
	public event Action<string> OnTriggered;

	[SerializeField] private string _triggerId;
	public string triggerId => _triggerId;
	public void Trigger()
	{
		OnTriggered?.Invoke(triggerId);
	}
}