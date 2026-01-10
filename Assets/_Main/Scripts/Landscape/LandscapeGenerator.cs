using System.Collections.Generic;
using UnityEngine;

public class LandscapeGenerator : MonoBehaviour
{
    [SerializeField]
    private Transform instantiatedObjectsParent;

    [SerializeField]
    private GameObject[] landScapePrefabs;

    [SerializeField]
    private int maxSpawnedObjects;

    [SerializeField]
    private float spawnCooldownMin;

    [SerializeField]
    private float spawnCooldownMax;

    [SerializeField]
    private float speed;

    [SerializeField]
    private bool movable;

    private readonly List<GameObject> activeObjects = new();

    private float lastSpawnTime;

    private void Start()
    {
        lastSpawnTime = -spawnCooldownMax;
        InitialSpawnObjects();
    }

    private void Update()
    {
        ManageObjects();
    }

    private Vector3 GetStartPoint()
    {
        return new Vector3(transform.position.x, transform.position.y, transform.position.z + transform.localScale.z);
    }

    private Vector3 GetEndPoint()
    {
        return new Vector3(transform.position.x, transform.position.y, transform.position.z - transform.localScale.z);
    }

    private float GetWidth()
    {
        return transform.localScale.x;
    }

    private void TrySpawnLandScapeObject()
    {
        if (lastSpawnTime + Random.Range(spawnCooldownMin, spawnCooldownMax) < Time.time && activeObjects.Count < maxSpawnedObjects)
        {
            lastSpawnTime = Time.time;
            SpawnLandscapeObject(GetStartPoint());
        }
    }

    private void SpawnLandscapeObject(Vector3 spawnPos)
    {
        GameObject prefab = landScapePrefabs[Random.Range(0, landScapePrefabs.Length)];

        spawnPos.x += Random.Range(-GetWidth(), GetWidth());

        GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity, instantiatedObjectsParent);
        obj.AddComponent<LandscapeObject>().Initialize(speed, movable);
        activeObjects.Add(obj);
    }

    private void InitialSpawnObjects()
    {
        Vector3 spawnPos = GetStartPoint();

        for (var i = 0; i < maxSpawnedObjects; i++)
        {
            Debug.Log(spawnPos);
            SpawnLandscapeObject(spawnPos);
            spawnPos.z -= Random.Range(spawnCooldownMin, spawnCooldownMax) * speed;

            if (OutOfBounds(gameObject))
            {
                break;
            }
        }
    }

    private void ManageObjects()
    {
        for (int i = activeObjects.Count - 1; i >= 0; i--)
        {
            var activeObject = activeObjects[i];
            if (OutOfBounds(activeObject))
            {
                Destroy(activeObjects[i]);
                activeObjects.RemoveAt(i);
            }
        }

        TrySpawnLandScapeObject();
    }

    private bool OutOfBounds(GameObject activeObject)
    {
        return activeObject.transform.position.z < GetEndPoint().z;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);

        Gizmos.DrawWireCube(Vector3.zero, Vector3.one * 2);

        Gizmos.matrix = oldMatrix;
    }
}
