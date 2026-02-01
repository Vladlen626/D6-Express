using System;
using System.Collections;
using System.Collections.Generic;

public class Quests : IEnumerable<Quest>
{
    private readonly List<Quest> quests = new();

    public event Action<Quest> QuestAdded;
    public event Action<Quest> QuestRemoved;

    public void Add(Quest quest)
    {
        quests.Add(quest);

        QuestAdded?.Invoke(quest);
    }

    public void Remove(Quest quest)
    {
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

    public IEnumerator<Quest> GetEnumerator()
    {
        return quests.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return quests.GetEnumerator();
    }
}
