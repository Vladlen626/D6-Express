using System;
using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using UnityEngine;

[Serializable]
public class InteractableActionSitPlayer : InteractableActionSit
{
	protected override async void StartInteractInternal(bool immediate = false)
	{
		base.StartInteractInternal(immediate);

		Locator.Resolve<IInputService>().OnMoved += OnMoved;
	}

	protected override async void StopInteractInternal(bool immediate = false)
	{
		Locator.Resolve<IInputService>().OnMoved -= OnMoved;

		base.StopInteractInternal(immediate);
	}

	private async void OnMoved(Vector2 dir)
	{
		StopInteract();
	}
}