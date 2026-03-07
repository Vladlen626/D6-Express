using _Main.Scripts.Core.Services;
using _Main.Scripts.Dice;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services;

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
	private readonly ICameraService cameraService;
	private readonly PlayerStateModel playerStateModel;

	private int currentCameraIndex;

	public CameraController(IInputService inputService, ICameraService cameraService, PlayerStateModel playerStateModel)
	{
		this.inputService = inputService;
		this.cameraService = cameraService;
		this.playerStateModel = playerStateModel;
	}

	public void Activate()
	{
		inputService.OnDiceGameNext += OnNextHandler;
		inputService.OnDiceGamePrevious += OnPreviousHandler;

		playerStateModel.StateAdded += OnStateChanged;
		playerStateModel.StateRemoved += OnStateChanged;
	}

	public void Deactivate()
	{
		inputService.OnDiceGameNext -= OnNextHandler;
		inputService.OnDiceGamePrevious -= OnPreviousHandler;

		playerStateModel.StateAdded -= OnStateChanged;
		playerStateModel.StateRemoved -= OnStateChanged;
	}


	//TODO: Возможно это абсолютно не правильно и стейт машина должна сама камеру менять,
	// а то если еще где-то начнем это делать все сломается... но пока так...
	private void OnStateChanged(CharacterState state)
	{
		foreach (var (characterState, cameraState) in stateToCamera)
		{
			if (playerStateModel.HasState(characterState))
			{
				cameraService.SetActiveCamera(cameraState);
				break;
			}
		}
	}

	private async void OnNextHandler()
	{
		currentCameraIndex++;
		if (currentCameraIndex >= diceGameCameraStates.Length)
		{
			currentCameraIndex = 0;
		}

		await cameraService.SetActiveCameraAsync(diceGameCameraStates[currentCameraIndex]);
	}

	private async void OnPreviousHandler()
	{
		currentCameraIndex--;
		if (currentCameraIndex < 0)
		{
			currentCameraIndex = diceGameCameraStates.Length - 1;
		}

		await cameraService.SetActiveCameraAsync(diceGameCameraStates[currentCameraIndex]);
	}
}