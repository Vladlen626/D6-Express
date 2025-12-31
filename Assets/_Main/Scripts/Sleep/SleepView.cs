using UnityEngine;

public class SleepView : MonoBehaviour
{
	[Header("Sleep")]
	[SerializeField] private GameObject sleepObject;

	public GameObject SleepObject => sleepObject;
}