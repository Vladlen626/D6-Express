using System;

public class QuestComponentHint : QuestComponent
{
    private string hint;
    private bool marked;

    public string Hint => hint;
    public bool Marked => marked;

    public event Action HintUpdated;

    public void SetHint(string hint)
    {
        this.hint = hint;
        HintUpdated?.Invoke();
    }

    public void Mark(bool completed)
    {
        this.marked = completed;
        HintUpdated?.Invoke();
    }
}