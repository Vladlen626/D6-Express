using System;
using System.Collections.Generic;

public class Quest
{
    public enum State
    {
        INACTIVE,
        IN_PROGRESS,
        COMPLETED,
        FAILED,
        FINISHED
    }

    private readonly Dictionary<Type, QuestComponent> components = new();

    public string Title { get; private set; }
    public State CurrentState { get; private set; } = State.INACTIVE;
    public IEnumerable<QuestComponent> Components => components.Values;

    public event Action<Quest, State> StateChanged;
    public event Action<QuestComponent> ComponentAdded;
    public event Action<QuestComponent> ComponentRemoved;

    public Quest(string title)
    {
        Title = title;
    }

    public void RequestInProgress()
    {
        CurrentState = State.IN_PROGRESS;
        StateChanged?.Invoke(this, CurrentState);
    }

    public void RequestComplete()
    {
        CurrentState = State.COMPLETED;
        StateChanged?.Invoke(this, CurrentState);
    }

    public void RequestFail()
    {
        CurrentState = State.FAILED;
        StateChanged?.Invoke(this, CurrentState);
    }

    public void RequestFinish()
    {
        CurrentState = State.FINISHED;
        StateChanged?.Invoke(this, CurrentState);
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
        if (!components.TryGetValue(typeof(T), out var component))
        {
            return;
        }

        component.Deactivate();
        components.Remove(typeof(T));

        ComponentRemoved?.Invoke(component);
    }

    public T GetComponent<T>() where T : QuestComponent
    {
        var component = components.GetValueOrDefault(typeof(T), null);
        return component as T;
    }
}