using System;
using System.Collections;
using System.Collections.Generic;

public class QuestComponentObjectives : QuestComponent, IEnumerable<QuestObjective>
{
    private int lastGivenId;

    private readonly Dictionary<int, QuestObjective> objectives = new();

    public event Action<QuestObjective> Added;
    public event Action<QuestObjective> Removed;

    public QuestObjective Add()
    {
        var questObjective = new QuestObjective(lastGivenId);
        objectives[lastGivenId] = questObjective;
        lastGivenId++;
        Added?.Invoke(questObjective);
        return questObjective;
    }

    public void Remove(QuestObjective questObjective)
    {
        Remove(questObjective.id);
    }

    public void Remove(int id)
    {
        if (!objectives.TryGetValue(id, out var questObjective))
        {
            return;
        }

        objectives.Remove(id);
        Removed?.Invoke(questObjective);
    }

    public QuestObjective Get(int id)
    {
        return objectives[id];
    }

    public IEnumerator<QuestObjective> GetEnumerator()
    {
        return objectives.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return objectives.Values.GetEnumerator();
    }
}