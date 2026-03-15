using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using PlatformCore.Services.Audio;
using PlatformCore.Services.UI;

public class DiceGameUIController : BaseContextController<DiceGameUIView>
{
    private readonly string[] DiceGameCameraStates = new[]
    {
        "dice_ui_to_board",
        "dice_ui_to_combinations",
        "dice_ui_to_inventory",
        "dice_ui_to_watches",
    };

    private readonly IInputService inputService;
    private readonly IAudioService audioService;
    private readonly PlayerStateModel playerStateModel;

    private int currentCameraIndex;

    public DiceGameUIController(
        IUIService uiService,
        IInputService inputService,
        IAudioService audioService,
        PlayerStateModel playerStateModel) : base(uiService)
    {
        this.inputService = inputService;
        this.audioService = audioService;
        this.playerStateModel = playerStateModel;
    }

    protected override void OnActivate()
    {
        base.OnActivate();

        _context.Hide();

        inputService.OnDiceGameNext += OnNextHandler;
        inputService.OnDiceGamePrevious += OnPreviousHandler;

        playerStateModel.StateAdded += OnStateChanged;
        playerStateModel.StateRemoved += OnStateChanged;
    }

    protected override void OnDeactivate()
    {
        playerStateModel.StateRemoved -= OnStateChanged;
        playerStateModel.StateAdded -= OnStateChanged;

        inputService.OnDiceGamePrevious -= OnPreviousHandler;
        inputService.OnDiceGameNext -= OnNextHandler;

        base.OnDeactivate();
    }

    private void OnStateChanged(CharacterState state)
    {
        if (playerStateModel.HasState(CharacterState.DICE_GAME))
        {
            _context.Show();
            UpdateHints();
        }
        else
        {
            _context.Hide();
        }
    }

    private void OnNextHandler()
    {
        if (!playerStateModel.HasState(CharacterState.DICE_GAME))
        {
            return;
        }

        audioService.PlaySound(SoundNames.Button);

        currentCameraIndex++;
        if (currentCameraIndex >= DiceGameCameraStates.Length)
        {
            currentCameraIndex = 0;
        }

        UpdateHints();
    }

    private void OnPreviousHandler()
    {
        if (!playerStateModel.HasState(CharacterState.DICE_GAME))
        {
            return;
        }

        audioService.PlaySound(SoundNames.Button);

        currentCameraIndex--;
        if (currentCameraIndex < 0)
        {
            currentCameraIndex = DiceGameCameraStates.Length - 1;
        }

        UpdateHints();
    }

    private void UpdateHints()
    {
        _context.SetLeftHint(currentCameraIndex - 1 >= 0 ? DiceGameCameraStates[currentCameraIndex - 1] : DiceGameCameraStates[^1]);
        _context.SetRightHint(currentCameraIndex + 1 < DiceGameCameraStates.Length ? DiceGameCameraStates[currentCameraIndex + 1] : DiceGameCameraStates[0]);
    }
}
