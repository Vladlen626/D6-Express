using UnityEngine;

namespace PlatformCore.Infrastructure.Triggers
{
	public class ZoneTrigger : BaseTrigger
	{
		private void OnTriggerEnter(Collider other)
		{
			Trigger();
		}
	}
}
