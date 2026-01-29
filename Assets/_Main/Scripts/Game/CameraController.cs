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

		playerStateModel.StateAdded += OnStateAddedHandler;
		playerStateModel.StateRemoved += OnStateRemovedHandler;
	}

	public void Deactivate()
	{
		inputService.OnDiceGameNext -= OnNextHandler;
		inputService.OnDiceGamePrevious -= OnPreviousHandler;

		playerStateModel.StateAdded -= OnStateAddedHandler;
		playerStateModel.StateRemoved -= OnStateRemovedHandler;
	}

	
	//TODO: Возможно это абсолютно не правильно и стейт машина должна сама камеру менять,
	// а то если еще где-то начнем это делать все сломается... но пока так...
	private void OnStateAddedHandler(CharacterState state)
	{
		if (state == CharacterState.DICE_GAME)
		{
			cameraService.SetActiveCamera(CameraStateEnum.DiceGame);
		}
	}
	
	private void OnStateRemovedHandler(CharacterState state)
	{
		if (state == CharacterState.DICE_GAME)
		{
			cameraService.SetActiveCamera(CameraStateEnum.FirstPerson);
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