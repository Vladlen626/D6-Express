using System;
using System.Collections.Generic;
using _Main.Scripts.Core;
using _Main.Scripts.Dice;
using _Main.Scripts.UI;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;
using UnityEngine;

public class UpgradeCombinationsViewController : BaseContextController<UIUpgradeCombinationsView>
{
    private const string CardPrefabPath = "UI/UIUpgradeCombinationCardView";
    private const string StraightComboId = "straight";
    private const string OfAKindComboId = "ofakind";

    private readonly DiceGameModel diceGameModel;
    private readonly IObjectFactory objectFactory;
    private readonly ILocalizationService localizationService;
    private readonly PauseState pauseState;
    private readonly Dictionary<string, UIUpgradeCombinationCardView> cardViews = new();

    private int activationVersion;

    public UpgradeCombinationsViewController(
        IUIService uiService,
        DiceGameModel diceGameModel,
        IObjectFactory objectFactory,
        ILocalizationService localizationService,
        PauseState pauseState) : base(uiService)
    {
        this.diceGameModel = diceGameModel;
        this.objectFactory = objectFactory;
        this.localizationService = localizationService;
        this.pauseState = pauseState;
    }

    protected override void OnActivate()
    {
        base.OnActivate();
        activationVersion++;

        _context.Hide();
        _context.Header.SetText(GlobalConstants.Localization.DiceUpgradePauseHeader);

        pauseState.Changed += OnPauseStateChanged;

        if (pauseState.IsPaused)
        {
            EnsureCardsAndRefreshAsync(activationVersion);
        }

        UpdateContextVisibility();
    }

    protected override void OnDeactivate()
    {
        activationVersion++;
        pauseState.Changed -= OnPauseStateChanged;

        ClearCards();
        _context.Hide();

        base.OnDeactivate();
    }

    private void OnPauseStateChanged(bool isPaused)
    {
        if (isPaused)
        {
            EnsureCardsAndRefreshAsync(activationVersion);
            return;
        }

        UpdateContextVisibility();
    }

    private async void EnsureCardsAndRefreshAsync(int version)
    {
        if (!IsActiveVersion(version) || !pauseState.IsPaused)
        {
            return;
        }

        await EnsureCardAsync(
            StraightComboId,
            GlobalConstants.Localization.DiceUpgradeComboStraight,
            version);
        await EnsureCardAsync(
            OfAKindComboId,
            GlobalConstants.Localization.DiceUpgradeComboOfAKind,
            version);

        if (!IsActiveVersion(version) || !pauseState.IsPaused)
        {
            return;
        }

        RefreshCardStats();
        UpdateContextVisibility();
    }

    private async UniTask EnsureCardAsync(string comboId, string titleLocalizationId, int version)
    {
        if (cardViews.ContainsKey(comboId))
        {
            return;
        }

        var cardView = await objectFactory.CreateAsync<UIUpgradeCombinationCardView>(
            CardPrefabPath,
            Vector3.zero,
            Quaternion.identity,
            _context.List);

        if (!cardView)
        {
            return;
        }

        if (!IsActiveVersion(version))
        {
            objectFactory.Destroy(cardView.gameObject);
            return;
        }

        cardViews[comboId] = cardView;
        cardView.SetTitle(titleLocalizationId);
        cardView.Show();
    }

    private void RefreshCardStats()
    {
        var scoringService = diceGameModel.PlayerScoringService;
        if (scoringService == null)
        {
            return;
        }

        var minLabel = localizationService.GetLocalized(GlobalConstants.Localization.DiceUpgradeLabelMinLen);
        var maxLabel = localizationService.GetLocalized(GlobalConstants.Localization.DiceUpgradeLabelMaxLen);
        var bonusLabel = localizationService.GetLocalized(GlobalConstants.Localization.DiceUpgradeLabelBonus);

        if (cardViews.TryGetValue(StraightComboId, out var straightCard) && straightCard)
        {
            var straightState = scoringService.GetStraightState();
            if (straightState == null)
            {
                straightState = new StraightRuntimeState(scoringService.GetStraightDefaults());
            }

            straightCard.SetStats(
                minLabel,
                maxLabel,
                bonusLabel,
                straightState.MinLen,
                straightState.MaxLen,
                straightState.ScoreBonus);
        }

        if (cardViews.TryGetValue(OfAKindComboId, out var ofAKindCard) && ofAKindCard)
        {
            var ofAKindState = scoringService.GetComboUpgradeState(OfAKindComboId);
            if (ofAKindState == null)
            {
                ofAKindState = ResolveOfAKindDefaultState(scoringService);
            }

            ofAKindCard.SetStats(
                minLabel,
                maxLabel,
                bonusLabel,
                ofAKindState.Min,
                ofAKindState.Max,
                ofAKindState.ScoreBonus);
        }
    }

    private static ComboUpgradeState ResolveOfAKindDefaultState(DiceScoringService scoringService)
    {
        var min = 3;
        var max = 6;
        var rules = scoringService.GetActiveRules();
        if (rules == null)
        {
            return new ComboUpgradeState
            {
                Min = min,
                Max = max,
                ScoreBonus = 0
            };
        }

        for (int i = 0; i < rules.Count; i++)
        {
            var rule = rules[i];
            if (rule == null)
            {
                continue;
            }

            if (rule.RuleType != ComboRuleType.OfAKind)
            {
                continue;
            }

            if (!string.Equals(rule.Id, OfAKindComboId, StringComparison.Ordinal))
            {
                continue;
            }

            if (rule.MinCount > 0)
            {
                min = rule.MinCount;
            }

            if (rule.MaxCount > 0)
            {
                max = rule.MaxCount;
            }

            if (max < min)
            {
                max = min;
            }

            break;
        }

        return new ComboUpgradeState
        {
            Min = min,
            Max = max,
            ScoreBonus = 0
        };
    }

    private void UpdateContextVisibility()
    {
        if (pauseState.IsPaused && cardViews.Count > 0)
        {
            _context.Show();
        }
        else
        {
            _context.Hide();
        }
    }

    private void ClearCards()
    {
        var cards = new List<UIUpgradeCombinationCardView>(cardViews.Values);
        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            if (!card)
            {
                continue;
            }

            card.Hide();
            objectFactory.Destroy(card.gameObject);
        }

        cardViews.Clear();
    }

    private bool IsActiveVersion(int version)
    {
        return version == activationVersion && _context;
    }
}
