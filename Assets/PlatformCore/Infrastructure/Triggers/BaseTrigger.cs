using System;
using UnityEngine;

namespace PlatformCore.Infrastructure.Triggers
{
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
}
