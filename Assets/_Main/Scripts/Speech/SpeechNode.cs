using System;


public abstract class SpeechNode
{
    protected Speech Speech { get; private set; }

    public event Action<SpeechNode> Started;
    public event Action<SpeechNode> Finished;

    public SpeechNode Init(Speech speech)
    {
        this.Speech = speech;

        return this;
    }

    public void Start()
    {
        Started?.Invoke(this);

        StartInternal();
    }

    public void Finish()
    {
        FinishInternal();

        Finished?.Invoke(this);
    }

    protected virtual void StartInternal() { }
    protected virtual void FinishInternal() { }

    public SpeechNode After(SpeechNode right)
    {
        right.Finished += (node) =>
        {
            Speech.SetNextNode(this);
        };

        return this;
    }
}