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

    private Dictionary<string, ItemCatalogEntry> configs;

    public ModifiersViewController(IUIService uiService, ModifiersModel modifiers, IObjectFactory objectFactory, IInputService inputService, ConfigService configService) : base(uiService)
    {
        this.modifiers = modifiers;
        this.objectFactory = objectFactory;
        this.inputService = inputService;
        this.configService = configService;
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

