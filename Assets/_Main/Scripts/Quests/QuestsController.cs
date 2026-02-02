using System.Collections.Generic;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using UnityEngine;

public class QuestsController : IBaseController, IActivatable
{
    private const int MIN_QUESTS_PER_LEVEL = 2;
    private const int MAX_QUESTS_PER_LEVEL = 2;

    private readonly List<IQuestGenerator> generators = new();

    private readonly Run run;
    private readonly Quests quests;

    public QuestsController(Run run, Quests quests, IEnumerable<IQuestGenerator> generators)
    {
        this.run = run;
        this.quests = quests;
        this.generators.AddRange(generators);
    }

    public void Activate()
    {
        run.LevelChanged += OnLevelChanged;
        GenerateQuests();
    }

    public void Deactivate()
    {
        run.LevelChanged -= OnLevelChanged;
    }

    private void OnLevelChanged()
    {
        quests.Clear();
        GenerateQuests();
    }

    private void GenerateQuests()
    {
        var toGenerate = Random.Range(MIN_QUESTS_PER_LEVEL, MAX_QUESTS_PER_LEVEL);
        var notUsedGenerators = new List<IQuestGenerator>(generators);
        for (var i = 0; i < toGenerate; i++)
        {
            if (notUsedGenerators.Count == 0)
            {
                break;
            }
            
            var generatorIdx = Random.Range(0, notUsedGenerators.Count);
            var generator = notUsedGenerators[i];
            notUsedGenerators.RemoveAt(generatorIdx);

            var quest = generator.Generate();
            quests.Add(quest);
            quest.RequestInProgress();
        }
    }
}
