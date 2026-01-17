using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.UI;

public class TransitionViewController : BaseContextController<UITransitionView>
{
    private float durationStart = 0.1f;
    private float durationEnd = 0.5f;
    private readonly TransitionService transitionService;

    public TransitionViewController(IUIService uiService, TransitionService transitionService) : base(uiService)
    {
        this.transitionService = transitionService;
    }

    protected override void OnActivate()
    {
        base.OnActivate();
    }

    protected override void OnDeactivate()
    {
        transitionService.TransitionRequested -= OnTransitionRequested;

        base.OnDeactivate();
    }

    public void StartObserving()
    {
        transitionService.TransitionRequested += OnTransitionRequested;
    }

    public UniTask StartTransition(float duration = -1)
    {
        return duration == -1 ? _context.ShowAsync(durationStart) : _context.ShowAsync(duration);
    }

    public UniTask FinishTransition(float duration = -1)
    {
        return duration == -1 ? _context.HideAsync(durationEnd) : _context.HideAsync(duration);
    }

    private void OnTransitionRequested()
    {
        transitionService.CurrentTransition.SetFirstTask(() => StartTransition(durationStart));
        transitionService.CurrentTransition.SetLastTask(() => FinishTransition(durationEnd));
    }
}