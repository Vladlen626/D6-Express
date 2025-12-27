using UnityEngine;

public class ZoneTrigger : BaseTrigger
{ 
	private void OnTriggerEnter(Collider other)
	{
		Trigger();
	}
	
}