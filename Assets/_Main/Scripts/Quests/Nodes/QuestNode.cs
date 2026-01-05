using System;

public abstract class QuestNode
{
    protected Quest Quest { get; private set; }

    public event Action Started;
    public event Action Finished;

    public QuestNode Init(Quest quest)
    {
        this.Quest = quest;

        return this;
    }

    public void Start()
    {
        Started?.Invoke();

        StartInternal();
    }

    public void Finish()
    {
        FinishInternal();

        Finished?.Invoke();
    }

    protected virtual void StartInternal() { }
    protected virtual void FinishInternal() { }

    public QuestNode ExecuteOnFinished(QuestNode right)
    {
        right.Finished += () =>
        {
            Quest.SetNextNode(this);
        };

        return this;
    }
}