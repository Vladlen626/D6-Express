using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

// todo: большая вероятность что следить за интерактором - плохая идея
public class SceneTransitionController : IBaseController, IActivatable
{
    private readonly LevelModel levelModel;
    private readonly Interactor interactor;

    public SceneTransitionController(Interactor interactor, LevelModel levelModel)
    {
        this.interactor = interactor;
        this.levelModel = levelModel;
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

    }

    private void OnInteractionEnded(InteractionAction action)
    {
        if (action is InteractableActionBuyTicket)
        {
            levelModel.SetLevelState(LevelState.TRAIN);
        }
    }
}
