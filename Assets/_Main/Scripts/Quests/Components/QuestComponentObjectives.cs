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
        return questObjective;
    }

    public void Remove(QuestObjective questObjective)
    {
        Remove(questObjective.id);
    }

    public void Remove(int id)
    {
        objectives.Remove(id);
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