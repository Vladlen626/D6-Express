using UnityEngine;

public class Targetable : MonoBehaviour
{
    [SerializeField]
    private Transform cameraTarget;
    
    public Transform CameraTarget => cameraTarget;
}
