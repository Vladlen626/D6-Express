using System;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.UI;

[Serializable]
public class InteractableActionSleep : InteractionAction
{
    private CharacterStateController stateController;

    public override void Init(Interactor interactor)
    {
        base.Init(interactor);

        stateController = interactor.GetComponent<CharacterStateController>();
    }

    public override bool CanInteract(IInteractable interactable)
    {
        return interactable is InteractableSleepable && !stateController.HasState(CharacterState.SLEEPING);
    }

    protected override async void StartInteractInternal(IInteractable interactable)
    {
        stateController.TryAddState(CharacterState.SLEEPING);

        // todo господь прости поправлю позже
        // ПИЗДЕЦ НАСРАЛ ЖОСКА, работает -> терплю
        await Locator.Resolve<IUIService>().PreloadAsync<UISleepView>();
        await Locator.Resolve<IUIService>().GetWindow<UISleepView>().CloseEyes();
        await UniTask.WaitForSeconds(1);

        StopInteract(interactable);
    }

    protected override async void StopInteractInternal(IInteractable interactable)
    {
        await Locator.Resolve<IUIService>().GetWindow<UISleepView>().OpenEyes();

        stateController.TryRemoveState(CharacterState.SLEEPING);
    }
}