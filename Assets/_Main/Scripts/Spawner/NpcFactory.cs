using System.Collections.Generic;
using PlatformCore.Services.Factory;

public static class NpcFactory
{
    public static NpcSpawner CreateNpcSpawner(IObjectFactory factory, D6Game d6Game, Run run, IEnumerable<SpawnPoint> spawnPoints)
    {
        return new NpcSpawner(factory, d6Game, run, spawnPoints);
    }
}
