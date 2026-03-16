using System;
using System.Collections.Generic;
using PlatformCore.Core;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;
using UnityEngine;
using Object = UnityEngine.Object;

public class QuestsViewController : BaseContextController<UIQuestsView>
{
    private readonly Quests quests;
    private readonly IObjectFactory objectFactory;
    private readonly D6Game game;
    private readonly Dictionary<Quest, UIQuestView> questViews = new();
    private readonly Dictionary<Quest, Action<QuestComponent>> componentAddedHandlers = new();
    private readonly Dictionary<Quest, Action<QuestComponent>> componentRemovedHandlers = new();
    private readonly Dictionary<QuestComponentObjectives, ObjectivesHandlers> objectivesHandlers = new();
    private readonly Dictionary<QuestObjective, ObjectiveHandlers> objectiveHandlers = new();

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

        var questsToCleanup = new List<Quest>(questViews.Keys);
        for (var i = 0; i < questsToCleanup.Count; i++)
        {
            CleanupQuest(questsToCleanup[i], removeView: true);
        }

        base.OnDeactivate();
    }

    private async void OnQuestAdded(Quest quest)
    {
        var questView = await objectFactory.CreateAsync<UIQuestView>(ResourcePaths.UI.UIQuestView, Vector3.zero, Quaternion.identity);

        if (!_context || questViews.ContainsKey(quest))
        {
            Object.Destroy(questView.gameObject);
            return;
        }

        questViews[quest] = questView;

        _context.AddQuest(questView);

        questView.SetTitleText(quest.Title);
        questView.Show();

        Action<QuestComponent> onComponentAdded = component => OnQuestComponentAdded(quest, component);
        Action<QuestComponent> onComponentRemoved = component => OnQuestComponentRemoved(quest, component);
        componentAddedHandlers[quest] = onComponentAdded;
        componentRemovedHandlers[quest] = onComponentRemoved;
        quest.ComponentAdded += onComponentAdded;
        quest.ComponentRemoved += onComponentRemoved;

        foreach (var item in quest.Components)
        {
            OnQuestComponentAdded(quest, item);
        }
    }

    private void OnQuestRemoved(Quest quest)
    {
        CleanupQuest(quest, removeView: true);
    }

    private void CleanupQuest(Quest quest, bool removeView)
    {
        if (componentAddedHandlers.TryGetValue(quest, out var onComponentAdded))
        {
            quest.ComponentAdded -= onComponentAdded;
            componentAddedHandlers.Remove(quest);
        }

        if (componentRemovedHandlers.TryGetValue(quest, out var onComponentRemoved))
        {
            quest.ComponentRemoved -= onComponentRemoved;
            componentRemovedHandlers.Remove(quest);
        }

        foreach (var component in quest.Components)
        {
            if (component is QuestComponentObjectives objectives)
            {
                DetachObjectivesHandlers(objectives);
            }
        }

        if (!removeView)
        {
            return;
        }

        if (questViews.TryGetValue(quest, out var view))
        {
            if (view)
            {
                view.Hide();

                if (_context)
                {
                    _context.RemoveQuest(view);
                }

                Object.Destroy(view.gameObject);
            }

            questViews.Remove(quest);
        }
    }

    private void OnQuestComponentAdded(Quest quest, QuestComponent component)
    {
        if (component is QuestComponentObjectives objectives)
        {
            AttachObjectivesHandlers(quest, objectives);
        }
    }

    private void OnQuestComponentRemoved(Quest quest, QuestComponent component)
    {
        if (component is QuestComponentObjectives objectives)
        {
            DetachObjectivesHandlers(objectives);
            UpdateObjectivesView(quest, objectives);
        }
    }

    private void AttachObjectivesHandlers(Quest quest, QuestComponentObjectives objectives)
    {
        if (objectivesHandlers.ContainsKey(objectives))
        {
            return;
        }

        Action<QuestObjective> onAdded = objective => ObjectiveAdded(quest, objectives, objective);
        Action<QuestObjective> onRemoved = objective => ObjectiveRemoved(quest, objectives, objective);
        objectives.Added += onAdded;
        objectives.Removed += onRemoved;
        objectivesHandlers[objectives] = new ObjectivesHandlers(onAdded, onRemoved);

        foreach (var objective in objectives)
        {
            ObjectiveAdded(quest, objectives, objective);
        }
    }

    private void DetachObjectivesHandlers(QuestComponentObjectives objectives)
    {
        if (objectivesHandlers.TryGetValue(objectives, out var handlers))
        {
            objectives.Added -= handlers.OnAdded;
            objectives.Removed -= handlers.OnRemoved;
            objectivesHandlers.Remove(objectives);
        }

        foreach (var objective in objectives)
        {
            DetachObjectiveHandlers(objective);
        }
    }

    private void ObjectiveAdded(Quest quest, QuestComponentObjectives objectives, QuestObjective questObjective)
    {
        if (objectiveHandlers.ContainsKey(questObjective))
        {
            return;
        }

        Action<string> onTitleChanged = _ => UpdateObjectivesView(quest, objectives);
        Action<bool> onCompletedChanged = _ => UpdateObjectivesView(quest, objectives);
        questObjective.TitleChanged += onTitleChanged;
        questObjective.CompletedChanged += onCompletedChanged;
        objectiveHandlers[questObjective] = new ObjectiveHandlers(onTitleChanged, onCompletedChanged);

        UpdateObjectivesView(quest, objectives);
    }

    private void ObjectiveRemoved(Quest quest, QuestComponentObjectives objectives, QuestObjective questObjective)
    {
        DetachObjectiveHandlers(questObjective);
        UpdateObjectivesView(quest, objectives);
    }

    private void DetachObjectiveHandlers(QuestObjective questObjective)
    {
        if (!objectiveHandlers.TryGetValue(questObjective, out var handlers))
        {
            return;
        }

        questObjective.TitleChanged -= handlers.OnTitleChanged;
        questObjective.CompletedChanged -= handlers.OnCompletedChanged;
        objectiveHandlers.Remove(questObjective);
    }

    private void UpdateObjectivesView(Quest quest, QuestComponentObjectives objectives)
    {
        if (!questViews.TryGetValue(quest, out var view) || !view)
        {
            return;
        }

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

    private readonly struct ObjectivesHandlers
    {
        public readonly Action<QuestObjective> OnAdded;
        public readonly Action<QuestObjective> OnRemoved;

        public ObjectivesHandlers(Action<QuestObjective> onAdded, Action<QuestObjective> onRemoved)
        {
            OnAdded = onAdded;
            OnRemoved = onRemoved;
        }
    }

    private readonly struct ObjectiveHandlers
    {
        public readonly Action<string> OnTitleChanged;
        public readonly Action<bool> OnCompletedChanged;

        public ObjectiveHandlers(Action<string> onTitleChanged, Action<bool> onCompletedChanged)
        {
            OnTitleChanged = onTitleChanged;
            OnCompletedChanged = onCompletedChanged;
        }
    }
}
