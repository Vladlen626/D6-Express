using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.UI;

public class TransitionViewController : BaseContextController<UITransitionView>
{
    private float durationStart = 0.1f;
    private float durationEnd = 0.5f;

    public TransitionViewController(IUIService uiService) : base(uiService) { }

    protected override void OnActivate()
    {
        base.OnActivate();
    }

    protected override void OnDeactivate()
    {
        Locator.Resolve<TransitionService>().TransitionRequested -= OnTransitionRequested;

        base.OnDeactivate();
    }

    public void StartObserving()
    {
        Locator.Resolve<TransitionService>().TransitionRequested += OnTransitionRequested;
    }

    public Task StartTransition(float duration = -1)
    {
        return duration == -1 ? _context.ShowAsync(durationStart) : _context.ShowAsync(duration);
    }

    public Task FinishTransition(float duration = -1)
    {
        return duration == -1 ? _context.HideAsync(durationEnd) : _context.HideAsync(duration);
    }

    private void OnTransitionRequested()
    {
        Locator.Resolve<TransitionService>().CurrentTransition.SetFirstTask(() => StartTransition(durationStart));
        Locator.Resolve<TransitionService>().CurrentTransition.SetLastTask(() => FinishTransition(durationEnd));
    }
}