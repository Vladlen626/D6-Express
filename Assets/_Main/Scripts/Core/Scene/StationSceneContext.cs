using UnityEngine;

public class StationSceneContext : MonoBehaviour, ISceneContext
{
    [SerializeField]
    private Transform playerSpawnPosition;

    public Transform PlayerSpawnPosition => playerSpawnPosition;
}