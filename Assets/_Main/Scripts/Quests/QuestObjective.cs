using System;

public class QuestObjective
{
    public readonly int id;

    private string title;
    private bool completed;

    public string Title
    {
        get => title;
        set
        {
            title = value;
            TitleChanged?.Invoke(title);
        }
    }

    public bool Completed
    {
        get => completed;
        set
        {
            completed = value;
            CompletedChanged?.Invoke(completed);
        }
    }

    public event Action<bool> CompletedChanged;
    public event Action<string> TitleChanged;

    public QuestObjective(int id)
    {
        this.id = id;
    }
}