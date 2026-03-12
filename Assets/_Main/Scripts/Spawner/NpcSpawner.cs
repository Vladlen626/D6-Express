using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using PlatformCore.Services.Factory;
using UnityEngine;

public class NpcSpawner
{
    private const int NPC_SPAWNED_COUNT = 8;

    private readonly IObjectFactory factory;
    private readonly D6Game d6Game;
    private readonly Run run;
    private readonly IEnumerable<SpawnPoint> trainSpawnPoints;
    private readonly IEnumerable<SpawnPoint> stationSpawnPoints;

    private readonly List<NpcView> npcList = new();

    public NpcSpawner(IObjectFactory factory, D6Game d6Game, Run run, IEnumerable<SpawnPoint> spawnPoints)
    {
        this.factory = factory;
        this.d6Game = d6Game;
        this.run = run;
        trainSpawnPoints = spawnPoints.Where(x => x.levelState == Location.TRAIN);
        stationSpawnPoints = spawnPoints.Where(x => x.levelState == Location.STATION);
    }

    public async UniTask Respawn()
    {
        DestroySpawned();
        await Spawn();
    }

    private async UniTask Spawn()
    {
        var notUsedPoints = d6Game.Location == Location.STATION ? stationSpawnPoints.ToList() : trainSpawnPoints.ToList();

        for (int i = 0; i < NPC_SPAWNED_COUNT; i++)
        {
            if (notUsedPoints.Count == 0)
            {
                break;
            }

            var pointIndex = Random.Range(0, notUsedPoints.Count);
            var point = notUsedPoints[pointIndex];

            notUsedPoints.RemoveAt(pointIndex);

            if (!point.isActiveAndEnabled)
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
            interactor.StopAllActions(true);
            Object.Destroy(item.gameObject);
        }

        npcList.Clear();
    }
}