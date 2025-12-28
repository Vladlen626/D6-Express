using UnityEngine;

public class PlayerView : MonoBehaviour
{
	[SerializeField] private Transform cameraRoot;
	public Transform CameraRoot => cameraRoot;
}