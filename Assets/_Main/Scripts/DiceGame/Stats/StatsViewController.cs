using System.Collections.Generic;
using _Main.Scripts.Core.Services;
using _Main.Scripts.Dice;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.Audio;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;

public class StatsViewController : BaseContextController<UIStatsView>, IGameStateChanger
{
    private readonly Run run;
    private readonly ConfigService configService;
    private Dictionary<string, StationConfig> stationConfigs;
    private readonly IInputService inputService;
    private readonly IAudioService audioService;

    private readonly ModifiersViewMiniController modifiersController;

    public StatsViewController(
        IUIService uiService,
        Run run,
        ConfigService configService,
        IInputService inputService,
        IAudioService audioService,
        ModifiersModel modifiersModel,
        IObjectFactory objectFactory) : base(uiService)
    {
        this.run = run;
        this.configService = configService;
        this.inputService = inputService;
        this.audioService = audioService;
        this.modifiersController = new ModifiersViewMiniController(modifiersModel, objectFactory, configService);
    }

    public IEnumerable<(GameStateTransitionTask task, GameStateChangeFunc func)> GetStateChangeFuncs()
    {
        yield return (GameStateTransitionTask.SHOW_STATS, async (x) =>
        {
            _context.Show();

            if (modifiersController.CanShow())
            {
                await modifiersController.Show();
            }

            if (x.Location != Location.MAIN_MENU)
            {
                if (x.Location == Location.STATION)
                {
                    var station = stationConfigs[run.StationId];
                    _context.SetMessage(station.name);
                    _context.SetDaysRemaningText("days_progress", (run.DaysPerLevel - run.Day).ToString());
                }
                else
                {
                    _context.SetMessage("D6-Express");
                }
            }
        }
        );

        yield return (GameStateTransitionTask.HIDE_STATS, async (x) =>
        {
            await modifiersController.Hide();
            _context.Hide();
        }
        );

        yield return (GameStateTransitionTask.AWAIT_STATS, async (x) =>
        {
            if (!x.AwaitUserInput)
            {
                return;
            }

            var source = new UniTaskCompletionSource();

            void OnInteracted()
            {
                audioService.PlaySound(SoundNames.Button);
                _context.StartButtonClicked -= OnButtonClicked;
                inputService.OnUISubmit -= OnInteracted;
                source.TrySetResult();
            }

            void OnButtonClicked()
            {
                _context.StartButtonClicked -= OnButtonClicked;
                inputService.OnUISubmit -= OnInteracted;
                source.TrySetResult();
            }

            inputService.OnUISubmit += OnInteracted;
            _context.StartButtonClicked += OnButtonClicked;

            await source.Task;
        }
        );
    }

    protected override async UniTask OnPreloadAsync()
    {
        await modifiersController.PreloadAsync();
        stationConfigs = await configService.GetConfigsAsync<StationConfig>(ResourcePaths.Json.stations);
    }

    protected override void OnActivate()
    {
        base.OnActivate();

        modifiersController.SetView(_context.UIModifiersView);
        modifiersController.Activate();

        _context.Hide();
    }

    protected override void OnDeactivate()
    {
        modifiersController.Deactivate();

        base.OnDeactivate();
    }
}
