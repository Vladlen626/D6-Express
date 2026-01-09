using System;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.UI;

[Serializable]
public class InteractableActionSleep : InteractionAction
{
	public override bool CanInteract(IInteractable interactable)
	{
		return interactable.Type == InteractionType.SLEEP && !StateModel.HasState(CharacterState.SLEEPING) && base.CanInteract(interactable);
	}

	protected override async void StartInteractInternal()
	{
		StateModel.TryAddState(CharacterState.SLEEPING);

		// todo господь прости поправлю позже
		// ПИЗДЕЦ НАСРАЛ ЖОСКА, работает -> терплю
		await Locator.Resolve<IUIService>().PreloadAsync<UISleepView>();
		await Locator.Resolve<IUIService>().GetWindow<UISleepView>().CloseEyes();
		await UniTask.WaitForSeconds(1);

		StopInteract();
	}

	protected override async void StopInteractInternal()
	{
		await Locator.Resolve<IUIService>().GetWindow<UISleepView>().OpenEyes();

		StateModel.TryRemoveState(CharacterState.SLEEPING);
	}
}