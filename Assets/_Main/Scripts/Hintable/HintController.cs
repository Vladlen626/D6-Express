using PlatformCore.Core;
using PlatformCore.Services.UI;

public class HintController : BaseContextController<UIHintView>
{
    private readonly PlayerView playerView;

    public HintController(IUIService uiService, PlayerView playerView) : base(uiService)
    {
        this.playerView = playerView;
    }

    protected override void OnActivate()
    {
		playerView.Interactor.Noticed += OnNoticed;
		playerView.Interactor.Missed += OnMissed;
    }

    protected override void OnDeactivate()
    {
		playerView.Interactor.Missed -= OnMissed;
		playerView.Interactor.Noticed -= OnNoticed;
    }

    private void OnNoticed(Interactable interactable)
    {
        if (interactable.TryGetComponent<IHintable>(out var hintable))
        {
            _context.SetHintText(hintable.HintText);
        }
    }

    private void OnMissed(Interactable interactable)
    {
        _context.SetHintText(string.Empty);
    }
}