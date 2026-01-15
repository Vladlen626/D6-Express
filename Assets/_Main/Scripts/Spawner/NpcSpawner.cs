using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlatformCore.Services.Factory;
using UnityEngine;

public class NpcSpawner
{
    private const int NPC_SPAWNED_COUNT = 15;

    private readonly IObjectFactory factory;
    private readonly RunModel runModel;
    private readonly IEnumerable<SpawnPoint> trainSpawnPoints;
    private readonly IEnumerable<SpawnPoint> stationSpawnPoints;

    private readonly List<NpcView> npcList = new();

    public NpcSpawner(IObjectFactory factory, RunModel runModel, IEnumerable<SpawnPoint> spawnPoints)
    {
        this.factory = factory;
        this.runModel = runModel;
        trainSpawnPoints = spawnPoints.Where(x => x.levelState == LevelState.TRAIN);
        stationSpawnPoints = spawnPoints.Where(x => x.levelState == LevelState.STATION);
    }

    public async Task Respawn()
    {
        DestroySpawned();
        await Spawn();
    }

    private async Task Spawn()
    {
        var notUsedPoints = runModel.LevelState == LevelState.STATION ? stationSpawnPoints.ToList() : trainSpawnPoints.ToList();

        for (int i = 0; i < NPC_SPAWNED_COUNT; i++)
        {
            if (notUsedPoints.Count == 0)
            {
                break;
            }

            var pointIndex = Random.Range(0, notUsedPoints.Count);
            var point = notUsedPoints[pointIndex];

            notUsedPoints.RemoveAt(pointIndex);

            if (!point.gameObject.activeSelf || point.levelState != runModel.LevelState)
            {
                continue;
            }

            // todo: нужно будет параметризировать
            var npc = await factory.CreateAsync<NpcView>(ResourcePaths.Player.NpcPassenger, point.transform.position, point.transform.rotation);

            if (point.TryGetComponent<Interactable>(out var interactable))
            {
                var interactor = npc.GetComponent<Interactor>();
                interactor.Interact(interactable);
            }

            npcList.Add(npc);
        }
    }

    private void DestroySpawned()
    {
        foreach (NpcView item in npcList)
        {
            var interactor = item.GetComponent<Interactor>();
            interactor.StopAllActions();
            Object.Destroy(item.gameObject);
        }

        npcList.Clear();
    }
}