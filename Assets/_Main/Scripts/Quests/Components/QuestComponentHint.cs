using System;

public class QuestComponentHint : QuestComponent
{
    private string hint;

    public string Hint => hint;

    public event Action HintUpdated;

    public void SetHint(string hint)
    {
        this.hint = hint;
        HintUpdated?.Invoke();
    }
}