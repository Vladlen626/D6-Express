using System;

public abstract class SpeechNode
{
    protected Speech speech;

    public event Action Started;
    public event Action Finished;

    public SpeechNode Init(Speech speech)
    {
        this.speech = speech;

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
            speech.SetNextNode(this);
        };

        return this;
    }
}