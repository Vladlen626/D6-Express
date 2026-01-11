using System.Collections.Generic;
using PlatformCore.Services.Factory;

public static class NpcFactory
{
    public static NpcSpawner CreateNpcSpawner(IObjectFactory factory, RunModel runModel, IEnumerable<SpawnPoint> spawnPoints)
    {
        return new NpcSpawner(factory, runModel, spawnPoints);
    }
}
