using System;

public class SpeechNodeConditional : SpeechNode
{
    private readonly Func<bool> condition;

    public event Action True;
    public event Action False;

    public SpeechNodeConditional(Func<bool> condition)
    {
        this.condition = condition;
    }

    protected override void StartInternal()
    {
        if (condition())
        {
            True?.Invoke();
        }
        else
        {
            False?.Invoke();
        }
        
        Finish();
    }

    public SpeechNodeConditional OnTrue(SpeechNode node)
    {
        True += () =>
        {
            Speech.SetNextNode(node);
        };

        return this;
    }

    public SpeechNodeConditional OnFalse(SpeechNode node)
    {
        False += () =>
        {
            Speech.SetNextNode(node);
        };

        return this;
    }
}