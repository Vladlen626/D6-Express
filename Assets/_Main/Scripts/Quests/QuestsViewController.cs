using System.Collections.Generic;
using PlatformCore.Core;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;
using UnityEngine;

public class QuestsViewController : BaseContextController<UIQuestsView>
{
    private readonly Quests quests;
    private readonly IObjectFactory objectFactory;
    private readonly Dictionary<Quest, UIQuestView> questViews = new();

    public QuestsViewController(IUIService uiService, Quests quests, IObjectFactory objectFactory) : base(uiService)
    {
        this.quests = quests;
        this.objectFactory = objectFactory;
    }

    protected override void OnActivate()
    {
        base.OnActivate();

        quests.QuestAdded += OnQuestAdded;
        quests.QuestRemoved += OnQuestRemoved;

        foreach (var item in quests)
        {
            OnQuestAdded(item);
        }
    }

    protected override void OnDeactivate()
    {
        quests.QuestRemoved -= OnQuestRemoved;
        quests.QuestAdded -= OnQuestAdded;

        base.OnDeactivate();
    }

    private async void OnQuestAdded(Quest quest)
    {
        var questView = await objectFactory.CreateAsync<UIQuestView>(ResourcePaths.UI.UIQuestView, Vector3.zero, Quaternion.identity);
        questViews[quest] = questView;

        _context.AddQuest(questView);

        questView.SetTitleText(quest.Title);
        questView.Show();

        quest.ComponentAdded += (c) => OnQuestComponentAdded(quest, c);

        foreach (var item in quest.Components)
        {
            OnQuestComponentAdded(quest, item);
        }
    }

    private void OnQuestRemoved(Quest quest)
    {
        var view = questViews[quest];
        view.Hide();
        _context.RemoveQuest(view);
        questViews.Remove(quest);
        Object.Destroy(view.gameObject);
    }

    private void OnQuestComponentAdded(Quest quest, QuestComponent component)
    {
        if (component is QuestComponentHint componentHint)
        {
            componentHint.HintUpdated += () => OnHintUpdated(quest);
            OnHintUpdated(quest);
        }
    }

    private void OnHintUpdated(Quest quest)
    {
        var view = questViews[quest];
        var questComponentHint = quest.GetComponent<QuestComponentHint>();
        view.SetGoalText(questComponentHint.Hint);
        view.SetMarked(questComponentHint.Marked);
    }
}
