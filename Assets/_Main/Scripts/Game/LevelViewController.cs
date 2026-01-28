using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services;

public class LevelViewController : IBaseController, IActivatable
{
    private readonly D6Game game;
	private readonly PlayerView playerView;
	private readonly ICameraService cameraService;

	public LevelViewController(D6Game game, PlayerView playerView, ICameraService cameraService)
	{
        this.game = game;
		this.playerView = playerView;
		this.cameraService = cameraService;
	}

    public void Activate()
    {
		game.LocationChanged += OnLocationChanged;
    }

    public void Deactivate()
    {
		game.LocationChanged -= OnLocationChanged;
    }

	private void OnLocationChanged()
	{
		if (game.Location == Location.STATION || game.Location == Location.TRAIN)
		{
			cameraService.AttachTo(playerView.CameraRoot);
		}
	}
}