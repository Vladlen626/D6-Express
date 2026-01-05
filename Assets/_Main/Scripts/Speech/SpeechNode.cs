using System;


public abstract class SpeechNode
{
    protected Speech Speech { get; private set; }

    public event Action Started;
    public event Action Finished;

    public SpeechNode Init(Speech speech)
    {
        this.Speech = speech;

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

    public SpeechNode After(SpeechNode right)
    {
        right.Finished += () =>
        {
            Speech.SetNextNode(this);
        };

        return this;
    }
}