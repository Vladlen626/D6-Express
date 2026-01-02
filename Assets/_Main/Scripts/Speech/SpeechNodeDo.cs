using System;

public class SpeechNodeDo : SpeechNode
{
    private readonly Action action;

    public SpeechNodeDo(Action action)
    {
        this.action = action;
    }

    protected override void StartInternal()
    {
        action?.Invoke();
        Finish();
    }
}  