using System.Text;
using PlatformCore.Core;
using PlatformCore.Services.UI;

public class QuestsController : BaseContextController<UIQuestsView>
{
    private readonly Quests quests;

    public QuestsController(IUIService uiService, Quests quests) : base(uiService)
    {
        this.quests = quests;
    }

    protected override void OnActivate()
    {
        base.OnActivate();

        quests.QuestAdded += OnQuestAdded;
        quests.QuestRemoved += OnQuestRemoved;
    }

    override protected void OnDeactivate()
    {
        quests.QuestRemoved -= OnQuestRemoved;
        quests.QuestAdded -= OnQuestAdded;

        base.OnDeactivate();
    }

    private void OnQuestAdded(Quest quest)
    {
        quest.Finished += OnQuestFinished;
        quest.ComponentAdded += OnQuestComponentAdded;
        quest.ComponentRemoved += OnQuestComponentRemoved;

        var questComponentHint = quest.GetComponent<QuestComponentHint>();
        if (questComponentHint != null)
        {
            questComponentHint.HintUpdated += OnQuestsChanged;
        }

        OnQuestsChanged();
    }

    private void OnQuestRemoved(Quest quest)
    {
        quest.ComponentRemoved -= OnQuestComponentRemoved;
        quest.ComponentAdded -= OnQuestComponentAdded;
        quest.Finished -= OnQuestFinished;

        var questComponentHint = quest.GetComponent<QuestComponentHint>();
        if (questComponentHint != null)
        {
            questComponentHint.HintUpdated -= OnQuestsChanged;
        }

        OnQuestsChanged();
    }

    private void OnQuestComponentAdded(QuestComponent questComponent)
    {
        if (questComponent is QuestComponentHint questComponentHint)
        {
            questComponentHint.HintUpdated += OnQuestsChanged;
        }
    }

    private void OnQuestComponentRemoved(QuestComponent questComponent)
    {
        if (questComponent is QuestComponentHint questComponentHint)
        {
            questComponentHint.HintUpdated -= OnQuestsChanged;
        }
    }

    private void OnQuestFinished(Quest quest)
    {
        OnQuestsChanged();
    }

    private void OnQuestsChanged()
    {
        _context.SetHints(GetHints());
    }

    private string GetHints()
    {
        StringBuilder stringBuilder = new();

        foreach (var item in quests.All)
        {
            var componentHint = item.GetComponent<QuestComponentHint>();
            if (componentHint != null)
            {
                stringBuilder.Append($"- {componentHint.Hint}");
                if (item.IsFinished)
                {
                    stringBuilder.Append($" (завершен)");
                }
                stringBuilder.AppendLine();
            }
        }

        return stringBuilder.ToString();
    }
}