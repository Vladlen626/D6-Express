using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Audio;
using UnityEngine;

public class FartController : IBaseController, IActivatable
{
	private readonly FartView fartView;
	private readonly IInputService inputService;
	private readonly IAudioService audioService;

	public FartController(FartView fartView, IInputService inputService, IAudioService audioService)
	{
		this.fartView = fartView;
		this.inputService = inputService;
		this.audioService = audioService;
	}

	public void Activate()
	{
		inputService.OnFarted += Fart;
	}

	public void Deactivate()
	{
		inputService.OnFarted -= Fart;
	}

	public void Fart()
	{
		audioService.PlaySound("event:/Fart");

		var colliders = Physics.OverlapSphere(fartView.FartTfm.position, fartView.Radius, fartView.FartLayerMask);
		foreach (var item in colliders)
		{
			if (item.TryGetComponent<Interactor>(out var interactor))
			{
				interactor.Interact(fartView.InteractableFart);
			}
		}
	}
}