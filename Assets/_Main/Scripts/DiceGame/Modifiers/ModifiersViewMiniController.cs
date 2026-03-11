using System.Collections.Generic;
using _Main.Scripts.Dice;
using Cysharp.Threading.Tasks;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Factory;
using UnityEngine;

public class ModifiersViewMiniController : IActivatable, IPreloadable
{
    private readonly ModifiersModel modifiers;
    private readonly IObjectFactory objectFactory;
    private readonly ConfigService configService;
    private readonly Dictionary<IModifier, UIModifierView> modifierViews = new();

    private UIModifiersView uIModifiersView;
    private Dictionary<string, ItemCatalogEntry> configs;

    public ModifiersViewMiniController(ModifiersModel modifiers, IObjectFactory objectFactory, ConfigService configService)
    {
        this.modifiers = modifiers;
        this.objectFactory = objectFactory;
        this.configService = configService;
    }

    public void SetView(UIModifiersView uIModifiersView)
    {
        this.uIModifiersView = uIModifiersView;
    }

    public async UniTask PreloadAsync()
    {
        configs = await configService.GetConfigsAsync<ItemCatalogEntry>(ResourcePaths.Json.items_catalog);
    }

    public bool CanShow()
    {
        return modifiers.AllModifiers.Count > 0;
    }

    public UniTask Show()
    {
        if (!uIModifiersView.gameObject.activeSelf)
        {
            uIModifiersView.gameObject.SetActive(true);
        }
        uIModifiersView.Show();

        uIModifiersView?.Header.SetText("modifiers_header");

        modifiers.ModifierAdded += OnModifierAdded;
        modifiers.ModifierRemoved += OnModifierRemoved;

        foreach (var item in modifiers.AllModifiers)
        {
            OnModifierAdded(item);
        }

        return uIModifiersView.ShowModifiers();
    }

    public async UniTask Hide(bool disable = false)
    {
        modifiers.ModifierRemoved -= OnModifierRemoved;
        modifiers.ModifierAdded -= OnModifierAdded;

        await uIModifiersView.HideModifiers();
        uIModifiersView.Hide();

        if (disable)
        {
            uIModifiersView.gameObject.SetActive(false);
        }

        ClearModifierViews();
    }

    public void Activate()
    {
        uIModifiersView.gameObject.SetActive(false);
    }

    public void Deactivate()
    {
        modifiers.ModifierRemoved -= OnModifierRemoved;
        modifiers.ModifierAdded -= OnModifierAdded;

        if (!uIModifiersView)
        {
            modifierViews.Clear();
            return;
        }

        uIModifiersView.Hide();
        if (uIModifiersView.gameObject.activeSelf)
        {
            uIModifiersView.gameObject.SetActive(false);
        }

        ClearModifierViews();
    }

    private async void OnModifierAdded(IModifier modifier)
    {
        if (modifier is not IModifierItem modifierItem)
        {
            return;
        }

        if (!configs.TryGetValue(modifierItem.Id, out var config) || config.typeEnum != ItemCatalogType.ModifierItem)
        {
            return;
        }

        var view = await objectFactory.CreateAsync<UIModifierView>(ResourcePaths.UI.UIModifierDarkViewVariant, Vector3.zero, Quaternion.identity, uIModifiersView.List);

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

    private void ClearModifierViews()
    {
        var toDelete = new List<IModifier>(modifierViews.Keys);
        foreach (var item in toDelete)
        {
            OnModifierRemoved(item);
        }

        modifierViews.Clear();
    }
}
