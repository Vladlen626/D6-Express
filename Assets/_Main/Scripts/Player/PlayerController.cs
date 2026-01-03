using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
// TODO: временный. сюда скину все, на что пока нет отдельного контроллера
public class PlayerController : IBaseController, IActivatable
{
	private readonly PlayerModel playerModel;
	private readonly PlayerView playerView;

    public PlayerController(PlayerModel playerModel, PlayerView playerView)
    {
        this.playerModel = playerModel;
        this.playerView = playerView;
    }

    public void Activate()
	{
		playerModel.OnCharacterStateChanged += OnCharacterStateChanged;
	}

	public void Deactivate()
	{
		playerModel.OnCharacterStateChanged -= OnCharacterStateChanged;
	}

	private void OnCharacterStateChanged(CharacterState oldCharacterState, CharacterState newCharacterState)
	{
		if (newCharacterState == CharacterState.LOCATION_TRANSITIONING)
		{
			playerView.GetComponent<CharacterStateController>().TryAddState(CharacterState.LOCATION_TRANSITIONING);
		}
	}
}
