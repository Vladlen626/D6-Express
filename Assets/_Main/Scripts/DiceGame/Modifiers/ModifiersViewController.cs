using System.Collections.Generic;
using System.Threading.Tasks;
using _Main.Scripts.Core.Services;
using _Main.Scripts.Dice;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;
using UnityEngine;

public class ModifiersViewController : BaseContextController<UIModifiersView>
{
    private readonly ModifiersModel modifiers;
    private readonly IObjectFactory objectFactory;
    private readonly IInputService inputService;
    private readonly ConfigService configService;
    private readonly Dictionary<IModifier, UIModifierView> modifierViews = new();

    private Dictionary<string, ModifierUIConfig> configs;

    public ModifiersViewController(IUIService uiService, ModifiersModel modifiers, IObjectFactory objectFactory, IInputService inputService, ConfigService configService) : base(uiService)
    {
        this.modifiers = modifiers;
        this.objectFactory = objectFactory;
        this.inputService = inputService;
        this.configService = configService;
    }

    protected override async UniTask OnPreloadAsync()
    {
        configs = await configService.GetConfigsAsync<ModifierUIConfig>(ResourcePaths.Json.modifiers_ui);

        await base.OnPreloadAsync();
    }

    protected override void OnActivate()
    {
        base.OnActivate();

        _context.Hide();

        _context.Header.SetText("modifiers_header");

        modifiers.ModifierAdded += OnModifierAdded;
        modifiers.ModifierRemoved += OnModifierRemoved;

        inputService.OnPausePressed += OnPausePressed;

        foreach (var item in modifiers.AllModifiers)
        {
            OnModifierAdded(item);
        }
    }

    protected override void OnDeactivate()
    {
        inputService.OnPausePressed -= OnPausePressed;

        modifiers.ModifierRemoved -= OnModifierRemoved;
        modifiers.ModifierAdded -= OnModifierAdded;

        foreach (var item in modifierViews.Keys)
        {
            OnModifierRemoved(item);
        }
        modifierViews.Clear();

        _context.Hide();

        base.OnDeactivate();
    }

    private async void OnModifierAdded(IModifier modifier)
    {
        // todo максимально осуждаю. у модификаторов должны быть айдишники
        if (!configs.TryGetValue(modifier.GetType().Name, out var config))
        {
            return;
        }

        var view = await objectFactory.CreateAsync<UIModifierView>(ResourcePaths.UI.UIModifierView, Vector3.zero, Quaternion.identity, _context.List);

        modifierViews[modifier] = view;

        view.SetTitle(config.title);
        view.SetDescription(config.description);

        if (modifier is ShakeRerollModifier shakeRerollModifier)
        {
            view.SetValue(config.value, shakeRerollModifier.shakeChance.ToString());
        }
        else if (modifier is MultiplyComboModifier multiplyComboModifier)
        {
            view.SetValue(config.value, multiplyComboModifier.combination.ToString());
        }
        else if (modifier is MultiplyKindOfModifiers multiplyKindOfModifiers)
        {
            view.SetValue(config.value, multiplyKindOfModifiers.combination.ToString());
        }
        else if (modifier is PassActivationMultiplierModifier passActivationMultiplierModifier)
        {
            view.SetValue(config.value, PassActivationMultiplierModifier.ScoreMultiplier.ToString());
        }
        else if (modifier is AdjustTicksPerDayModifier adjustTicksPerDayModifier)
        {
            view.SetValue(config.value, adjustTicksPerDayModifier.delta.ToString());
        }

        view.Show();
    }

    private void OnModifierRemoved(IModifier modifier)
    {
        var view = modifierViews[modifier];

        view.Hide();

        Object.Destroy(view.gameObject);
    }

    private void OnPausePressed()
    {
        if (_context.IsShown())
        {
            _context.Hide();
        }
        else
        {
            _context.Show();
        }
    }
}