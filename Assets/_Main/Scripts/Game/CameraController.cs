using _Main.Scripts.Core.Services;
using _Main.Scripts.Dice;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services;
using PlatformCore.Services.Audio;

public class CameraController : IBaseController, IActivatable
{
	private static readonly CameraStateEnum[] diceGameCameraStates =
	{
		CameraStateEnum.DiceGame,
		CameraStateEnum.DiceGameCombinations,
		CameraStateEnum.Inventory,
		CameraStateEnum.TrainWatch,
	};

	private static readonly (CharacterState characterState, CameraStateEnum cameraState)[] stateToCamera = {
		(CharacterState.SPEAKING, CameraStateEnum.FirstPerson),
		(CharacterState.DICE_GAME, CameraStateEnum.DiceGame),
		(CharacterState.DEFAULT, CameraStateEnum.FirstPerson),
	};

	private readonly IInputService inputService;
	private readonly IAudioService audioService;
	private readonly ICameraService cameraService;
	private readonly PlayerStateModel playerStateModel;
	private readonly D6Game game;

	private int currentCameraIndex;

	public CameraController(
		IInputService inputService,
		ICameraService cameraService,
		PlayerStateModel playerStateModel,
		D6Game game,
		IAudioService audioService)
	{
		this.inputService = inputService;
		this.cameraService = cameraService;
		this.playerStateModel = playerStateModel;
		this.audioService = audioService;
		this.game = game;
	}

	public void Activate()
	{
		inputService.OnDiceGameNext += OnNextHandler;
		inputService.OnDiceGamePrevious += OnPreviousHandler;

		playerStateModel.StateAdded += OnStateChanged;
		playerStateModel.StateRemoved += OnStateChanged;
		game.LocationChanged += OnLocationChanged;

		ApplyCamera();
	}

	public void Deactivate()
	{
		inputService.OnDiceGameNext -= OnNextHandler;
		inputService.OnDiceGamePrevious -= OnPreviousHandler;

		playerStateModel.StateAdded -= OnStateChanged;
		playerStateModel.StateRemoved -= OnStateChanged;
		game.LocationChanged -= OnLocationChanged;
	}


	//TODO: Возможно это абсолютно не правильно и стейт машина должна сама камеру менять,
	// а то если еще где-то начнем это делать все сломается... но пока так...
	private void OnStateChanged(CharacterState state)
	{
		ApplyCamera();
	}

	private void OnLocationChanged()
	{
		ApplyCamera();
	}

	private void ApplyCamera()
	{
		if (game.Location == Location.MAIN_MENU)
		{
			cameraService.SetActiveCamera(CameraStateEnum.MainMenu);
			return;
		}

		foreach (var (characterState, cameraState) in stateToCamera)
		{
			if (playerStateModel.HasState(characterState))
			{
				cameraService.SetActiveCamera(cameraState);
				return;
			}
		}

		cameraService.SetActiveCamera(CameraStateEnum.FirstPerson);
	}

	private async void OnNextHandler()
	{
		currentCameraIndex++;
		if (currentCameraIndex >= diceGameCameraStates.Length)
		{
			currentCameraIndex = 0;
		}

		audioService.PlaySound(SoundNames.CameraMove);
		await cameraService.SetActiveCameraAsync(diceGameCameraStates[currentCameraIndex]);
	}

	private async void OnPreviousHandler()
	{
		currentCameraIndex--;
		if (currentCameraIndex < 0)
		{
			currentCameraIndex = diceGameCameraStates.Length - 1;
		}

		audioService.PlaySound(SoundNames.CameraMove);
		await cameraService.SetActiveCameraAsync(diceGameCameraStates[currentCameraIndex]);
	}
}
