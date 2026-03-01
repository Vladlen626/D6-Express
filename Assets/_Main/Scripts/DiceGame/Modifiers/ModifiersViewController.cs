using System.Collections.Generic;
using _Main.Scripts.UI;
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
    private readonly ConfigService configService;
    private readonly PauseState pauseState;
    private readonly Dictionary<IModifier, UIModifierView> modifierViews = new();

    private Dictionary<string, ItemCatalogEntry> configs;

    public ModifiersViewController(IUIService uiService, ModifiersModel modifiers, IObjectFactory objectFactory, ConfigService configService, PauseState pauseState) : base(uiService)
    {
        this.modifiers = modifiers;
        this.objectFactory = objectFactory;
        this.configService = configService;
        this.pauseState = pauseState;
    }

    protected override async UniTask OnPreloadAsync()
    {
        configs = await configService.GetConfigsAsync<ItemCatalogEntry>(ResourcePaths.Json.items_catalog);

        await base.OnPreloadAsync();
    }

    protected override void OnActivate()
    {
        base.OnActivate();

        _context.Hide();

        _context.Header.SetText("modifiers_header");

        modifiers.ModifierAdded += OnModifierAdded;
        modifiers.ModifierRemoved += OnModifierRemoved;

        pauseState.Changed += OnPauseStateChanged;

        foreach (var item in modifiers.AllModifiers)
        {
            OnModifierAdded(item);
        }

        if (pauseState.IsPaused)
        {
            _context.Show();
        }
    }

    protected override void OnDeactivate()
    {
        pauseState.Changed -= OnPauseStateChanged;

        modifiers.ModifierRemoved -= OnModifierRemoved;
        modifiers.ModifierAdded -= OnModifierAdded;

        var toDelete = new List<IModifier>(modifierViews.Keys);
        foreach (var item in toDelete)
        {
            OnModifierRemoved(item);
        }
        modifierViews.Clear();

        _context.Hide();

        base.OnDeactivate();
    }

    private async void OnModifierAdded(IModifier modifier)
    {
        if (modifier is not IModifierItem modifierItem)
        {
            return;
        }

        if (!configs.TryGetValue(modifierItem.Id, out var config) || config.typeEnum != ItemCatalogType.Modifier)
        {
            return;
        }

        var view = await objectFactory.CreateAsync<UIModifierView>(ResourcePaths.UI.UIModifierView, Vector3.zero, Quaternion.identity, _context.List);

        modifierViews[modifier] = view;

        view.SetTitle(config.nameKey);
        view.SetDescription(config.descriptionKey);

        view.Show();
    }

    private void OnModifierRemoved(IModifier modifier)
    {
        if (!modifierViews.TryGetValue(modifier, out var view))
        {
            return;
        }

        modifierViews.Remove(modifier);

        view.Hide();

        Object.Destroy(view.gameObject);
    }

    private void OnPauseStateChanged(bool isPaused)
    {
        if (isPaused)
        {
            _context.Show();
        }
        else
        {
            _context.Hide();
        }
    }
}

