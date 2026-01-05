public class QuestNodeUpdateHint : QuestNode
{
    private readonly string hint;

    public QuestNodeUpdateHint(string hint)
    {
        this.hint = hint;
    }

    protected override void StartInternal()
    {
        base.StartInternal();

        Quest.AddComponent<QuestComponentHint>().SetHint(hint);

        Finish();
    }
}