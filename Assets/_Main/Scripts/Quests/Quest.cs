using System;
using System.Collections.Generic;

public class Quest
{
    private readonly Dictionary<Type, QuestComponent> components = new();

    private QuestNode current;
    private QuestNode next;


    public int Id { get; private set; }
    public bool IsFinished { get; private set; }

    public event Action<Quest> Started;
    public event Action<Quest> Finished;

    public event Action<QuestNode> NodeStarted;
    public event Action<QuestNode> NodeFinished;

    public event Action<QuestComponent> ComponentAdded;
    public event Action<QuestComponent> ComponentRemoved;

    public Quest(int id)
    {
        this.Id = id;
    }

    public void RequestStart()
    {
        Started?.Invoke(this);

        ProcessNextNode();
    }

    public void RequestFinish()
    {
        current.Finish();
    }

    public void SetNextNode(QuestNode next)
    {
        this.next = next;
    }

    public T AddComponent<T>() where T : QuestComponent, new()
    {
        if (!components.TryGetValue(typeof(T), out QuestComponent component))
        {
            component = new T();
            components.Add(typeof(T), component);
            component.Init(this);
            component.Activate();

            ComponentAdded?.Invoke(component);
        }

        return component as T;
    }

    public void RemoveComponent<T>() where T : QuestComponent
    {
        var component = components[typeof(T)];
        component.Deactivate();
        components.Remove(typeof(T));

        ComponentAdded?.Invoke(component);
    }

    public T GetComponent<T>() where T : QuestComponent
    {
        var component = components.GetValueOrDefault(typeof(T), null);
        return component as T;
    }

    private void ProcessNextNode()
    {
        if (current != null)
        {
            current.Started -= OnNodeStarted;
            current.Finished -= OnNodeFinished;

            current = null;
        }

        if (next == null)
        {
            FinishQuest();
            return;
        }

        current = next;
        next = null;
        current.Started += OnNodeStarted;
        current.Finished += OnNodeFinished;

        current.Start();
    }

    private void OnNodeStarted()
    {
        NodeStarted?.Invoke(current);
    }

    private void OnNodeFinished()
    {
        NodeFinished?.Invoke(current);
        ProcessNextNode();
    }

    private void FinishQuest()
    {
        IsFinished = true;

        foreach (var item in components.Values)
        {
            item.Deactivate();
        }

        Finished?.Invoke(this);
    }
}