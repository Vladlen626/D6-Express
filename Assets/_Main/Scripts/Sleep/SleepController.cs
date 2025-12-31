using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

public class SleepController : IBaseController, IActivatable
{
	private readonly LevelModel levelModel;
	private readonly SleepView sleepView;
	private readonly Interactor interactor;

	public SleepController(LevelModel levelModel, SleepView sleepView, Interactor interactor)
	{
		this.levelModel = levelModel;
		this.sleepView = sleepView;
		this.interactor = interactor;
	}

    public void Activate()
	{
		interactor.InteractionStarted += OnInteractionStarted;
		interactor.InteractionEnded += OnInteractionEnded;
	}

    public void Deactivate()
	{
		interactor.InteractionEnded -= OnInteractionEnded;
		interactor.InteractionStarted -= OnInteractionStarted;
	}

	private void OnInteractionStarted(InteractionAction action)
	{
		if (action is InteractableActionLay)
		{
			OnLayed();
		}
	}

	private void OnInteractionEnded(InteractionAction action)
	{
		if (action is InteractableActionLay)
		{
			OnStoodUp();
		}
		else if (action is InteractableActionSleep)
		{
			levelModel.IncrementDays();
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
}
