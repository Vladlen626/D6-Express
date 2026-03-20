using _Main.Scripts.Core.Services;
using _Main.Scripts.Dice;
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
    private readonly DiceGameModel diceGameModel;

    private int currentCameraIndex;

    public DiceGameUIController(
        IUIService uiService,
        IInputService inputService,
        IAudioService audioService,
        PlayerStateModel playerStateModel,
        DiceGameModel diceGameModel) : base(uiService)
    {
        this.inputService = inputService;
        this.audioService = audioService;
        this.playerStateModel = playerStateModel;
        this.diceGameModel = diceGameModel;
    }

    protected override void OnActivate()
    {
        base.OnActivate();

        _context.Hide();

        inputService.OnDiceGameNext += OnNextHandler;
        inputService.OnDiceGamePrevious += OnPreviousHandler;

        playerStateModel.StateAdded += OnStateChanged;
        playerStateModel.StateRemoved += OnStateChanged;

        diceGameModel.OnDiceGameStateChanged += OnDiceGameStateChanged;
        diceGameModel.OnCurrentTurnChanged += OnCurrentTurnChanged;
        diceGameModel.OnDiceAnimationInProgressChanged += OnDiceAnimationInProgressChanged;

        RefreshContextState();
    }

    protected override void OnDeactivate()
    {
        diceGameModel.OnDiceAnimationInProgressChanged -= OnDiceAnimationInProgressChanged;
        diceGameModel.OnCurrentTurnChanged -= OnCurrentTurnChanged;
        diceGameModel.OnDiceGameStateChanged -= OnDiceGameStateChanged;

        playerStateModel.StateRemoved -= OnStateChanged;
        playerStateModel.StateAdded -= OnStateChanged;

        inputService.OnDiceGamePrevious -= OnPreviousHandler;
        inputService.OnDiceGameNext -= OnNextHandler;

        base.OnDeactivate();
    }

    private void OnStateChanged(CharacterState state)
    {
        RefreshContextState();
    }

    private void OnDiceGameStateChanged()
    {
        RefreshHintsState();
    }

    private void OnCurrentTurnChanged(int oldValue, int newValue)
    {
        RefreshHintsState();
    }

    private void OnDiceAnimationInProgressChanged(bool oldValue, bool newValue)
    {
        RefreshHintsState();
    }

    private void OnNextHandler()
    {
        if (!CanSwitchCamera())
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
        if (!CanSwitchCamera())
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

    private void RefreshContextState()
    {
        if (!playerStateModel.HasState(CharacterState.DICE_GAME))
        {
            _context.Hide();
            return;
        }

        _context.Show();
        RefreshHintsState();
    }

    private void RefreshHintsState()
    {
        if (!_context || !playerStateModel.HasState(CharacterState.DICE_GAME))
        {
            return;
        }
        
        if (CanSwitchCamera())
        {
            _context.Show();
        }
        else
        {
            _context.Hide();
            return;
        }

        UpdateHints();
    }

    private bool CanSwitchCamera()
    {
        if (!playerStateModel.HasState(CharacterState.DICE_GAME))
        {
            return false;
        }

        if (playerStateModel.HasState(CharacterState.SPEAKING))
        {
            return false;
        }

        return diceGameModel.IsPlayerActionPhase;
    }
}
