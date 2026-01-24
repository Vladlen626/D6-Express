using System.Collections.Generic;
using PlatformCore.Services.Factory;

public static class NpcFactory
{
    public static NpcSpawner CreateNpcSpawner(IObjectFactory factory, Run run, IEnumerable<SpawnPoint> spawnPoints)
    {
        return new NpcSpawner(factory, run, spawnPoints);
    }
}
