using System.Collections.Generic;
using PlatformCore.Core;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;
using UnityEngine;

public class QuestsViewController : BaseContextController<UIQuestsView>
{
    private readonly Quests quests;
    private readonly IObjectFactory objectFactory;
    private readonly D6Game game;
    private readonly Dictionary<Quest, UIQuestView> questViews = new();

    public QuestsViewController(IUIService uiService, Quests quests, IObjectFactory objectFactory, D6Game game) : base(uiService)
    {
        this.quests = quests;
        this.objectFactory = objectFactory;
        this.game = game;
    }

    protected override void OnActivate()
    {
        base.OnActivate();

        game.LocationChanged += OnLocationChanged;

        quests.QuestAdded += OnQuestAdded;
        quests.QuestRemoved += OnQuestRemoved;

        foreach (var item in quests)
        {
            OnQuestAdded(item);
        }
    }

    protected override void OnDeactivate()
    {
        game.LocationChanged -= OnLocationChanged;

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
        if (component is QuestComponentObjectives objectives)
        {
            objectives.Added += (o) => ObjectiveAdded(quest, o);
            foreach (var item in objectives)
            {
                ObjectiveAdded(quest, item);
            }
        }
    }

    private void ObjectiveAdded(Quest quest, QuestObjective questObjective)
    {
        var view = questViews[quest];
        var objectives = quest.GetComponent<QuestComponentObjectives>();

        questObjective.TitleChanged += (t) => view.UpdateObjectives(objectives);
        questObjective.CompletedChanged += (t) => view.UpdateObjectives(objectives);

        view.UpdateObjectives(objectives);
    }

    private void ObjectiveRemoved(Quest quest, QuestObjective questObjective)
    {
        var view = questViews[quest];
        var objectives = quest.GetComponent<QuestComponentObjectives>();
        view.UpdateObjectives(objectives);
    }

    private void OnLocationChanged()
    {
        if (game.Location == Location.MAIN_MENU)
        {
            _context.Hide();
        }
        else
        {
            _context.Show();
        }
    }
}
