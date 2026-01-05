using System;
using System.Collections.Generic;

public class Quests
{
    private readonly List<Quest> quests = new();

    public event Action<Quest> QuestAdded;
    public event Action<Quest> QuestRemoved;

    public IReadOnlyList<Quest> All => quests;

    public void Add(Quest quest)
    {
        quests.Add(quest);

        quest.Finished += OnQuestFinished;

        QuestAdded?.Invoke(quest);
    }

    public void Remove(Quest quest)
    {
        quest.Finished -= OnQuestFinished;

        quests.Remove(quest);

        QuestRemoved?.Invoke(quest);
    }

    public void Clear()
    {
        for (int i = 0; i < quests.Count; i++)
        {
            Remove(quests[i]);
        }
    }

    private void OnQuestFinished(Quest quest)
    {
        // Remove(quest);
    }
}
