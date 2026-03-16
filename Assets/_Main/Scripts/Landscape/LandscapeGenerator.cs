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

    private readonly List<LandscapeObject> activeObjects = new();
    private readonly Queue<LandscapeObject> pooledObjects = new();

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
        spawnPos.x += Random.Range(-GetWidth(), GetWidth());

        LandscapeObject landscapeObject = GetFromPool();
        Transform objectTransform = landscapeObject.transform;
        objectTransform.SetParent(instantiatedObjectsParent);
        objectTransform.position = spawnPos;
        objectTransform.rotation = Quaternion.identity;
        landscapeObject.gameObject.SetActive(true);
        landscapeObject.Initialize(speed, movable);

        activeObjects.Add(landscapeObject);
    }

    private LandscapeObject GetFromPool()
    {
        if (pooledObjects.Count > 0)
        {
            return pooledObjects.Dequeue();
        }

        GameObject prefab = landScapePrefabs[Random.Range(0, landScapePrefabs.Length)];
        GameObject obj = Instantiate(prefab, instantiatedObjectsParent);
        if (!obj.TryGetComponent(out LandscapeObject landscapeObject))
        {
            landscapeObject = obj.AddComponent<LandscapeObject>();
        }

        return landscapeObject;
    }

    private void ReturnToPool(LandscapeObject landscapeObject)
    {
        landscapeObject.gameObject.SetActive(false);
        pooledObjects.Enqueue(landscapeObject);
    }

    private void InitialSpawnObjects()
    {
        Vector3 spawnPos = GetStartPoint();
        float endZ = GetEndPoint().z;

        for (var i = 0; i < maxSpawnedObjects; i++)
        {
            SpawnLandscapeObject(spawnPos);
            spawnPos.z -= Random.Range(spawnCooldownMin, spawnCooldownMax) * speed;

            if (spawnPos.z < endZ)
            {
                break;
            }
        }
    }

    private void ManageObjects()
    {
        float endZ = GetEndPoint().z;
        for (int i = activeObjects.Count - 1; i >= 0; i--)
        {
            LandscapeObject activeObject = activeObjects[i];
            if (activeObject.transform.position.z < endZ)
            {
                activeObjects.RemoveAt(i);
                ReturnToPool(activeObject);
            }
        }

        TrySpawnLandScapeObject();
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
