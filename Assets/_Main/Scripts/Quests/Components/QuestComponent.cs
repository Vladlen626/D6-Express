using System;

public abstract class QuestComponent
{
    public event Action Activated;
    public event Action Deactivated;

    protected Quest Quest { get; private set; }

    public void Init(Quest quest)
    {
        Quest = quest;

        InitInternal();
    }

    public void Activate()
    {
        ActivateInternal();

        Activated?.Invoke();
    }

    public void Deactivate()
    {
        DeactivateInternal();

        Deactivated?.Invoke();
    }

    protected virtual void InitInternal() { }
    protected virtual void ActivateInternal() { }
    protected virtual void DeactivateInternal() { }
}
