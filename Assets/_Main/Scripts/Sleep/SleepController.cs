using System;
using System.Collections.Generic;
using _Main.Scripts.Core.Services;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.UI;

public class SleepController : BaseContextController<UISleepView>, IGameStateChanger
{
	private readonly Run run;
	private readonly SleepView sleepView;
	private readonly Interactor interactor;
	private readonly IInputService inputService;

	public SleepController(IUIService uIService, Run run, SleepView sleepView, Interactor interactor, IInputService inputService) : base(uIService)
	{
		this.run = run;
		this.sleepView = sleepView;
		this.interactor = interactor;
		this.inputService = inputService;
	}

	protected override void OnActivate()
	{
		base.OnActivate();

		interactor.InteractionStarted += OnInteractionStarted;
		interactor.InteractionEnded += OnInteractionEnded;
	}

	protected override void OnDeactivate()
	{
		interactor.InteractionEnded -= OnInteractionEnded;
		interactor.InteractionStarted -= OnInteractionStarted;

		base.OnDeactivate();
	}

	public IEnumerable<(GameStateTransitionTask task, GameStateChangeFunc func)> GetStateChangeFuncs()
	{
		yield return (GameStateTransitionTask.SHOW_WAKE_UP, async (x) =>
		{
			await ShowWakeUp();
			await WaitAndHideWakeUp();
		}
		);
	}

	private void OnInteractionStarted(InteractionAction action)
	{
		if (action is InteractableActionLay)
		{
			OnLayed();
		}
		else if (action is InteractableActionSleep)
		{
			run.RequestSetDay(run.Day + 1);
		}
	}

	private void OnInteractionEnded(InteractionAction action)
	{
		if (action is InteractableActionLay)
		{
			OnStoodUp();
		}
	}

	private void OnLayed()
	{
		sleepView.SleepObject.SetActive(true);
	}

	private void OnStoodUp()
	{
		sleepView.SleepObject.SetActive(false);
	}

	private UniTask ShowWakeUp()
	{
		return _context.ShowWakeUp();
	}

	private async UniTask WaitAndHideWakeUp()
	{
		var source = new UniTaskCompletionSource();

		void OnInteracted()
		{
			inputService.OnSpeechLineSkip -= OnInteracted;
			source.TrySetResult();
		}

		inputService.OnSpeechLineSkip += OnInteracted;

		await source.Task;

		await _context.HideWakeUp();
	}

}