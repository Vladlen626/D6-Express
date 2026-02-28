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
		base.OnActivate();
		playerView.Interactor.Noticed += OnNoticed;
		playerView.Interactor.Missed += OnMissed;
		_context.Hide();
	}

	protected override void OnDeactivate()
	{
		playerView.Interactor.Missed -= OnMissed;
		playerView.Interactor.Noticed -= OnNoticed;
		base.OnDeactivate();
	}

	private void OnNoticed(Interactable interactable)
	{
		_context.Show();
		if (interactable.TryGetComponent<IHintable>(out var hintable))
		{
			_context.SetHintText(hintable.HintText);
		}
	} 

	private void OnMissed(Interactable interactable)
	{
		_context.SetHintText(string.Empty);
		_context.Hide();
	}
}