using System.Collections.Generic;
using _Main.Scripts.Core.Services;
using _Main.Scripts.Dice;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;

public class TransitionViewController : BaseContextController<UITransitionView>, IGameStateChanger
{
    // todo: в конфиги
    private readonly float durationStart = 0.1f;
    // todo: в конфиги
    private readonly float durationEnd = 0.5f;
    private readonly Run run;
    private readonly ConfigService configService;
    private Dictionary<string, StationConfig> stationConfigs;
    private readonly IInputService inputService;

    private readonly ModifiersViewMiniController modifiersController;

    public TransitionViewController(IUIService uiService, Run run, ConfigService configService, IInputService inputService, ModifiersModel modifiersModel, IObjectFactory objectFactory) : base(uiService)
    {
        this.run = run;
        this.configService = configService;
        this.inputService = inputService;
        this.modifiersController = new ModifiersViewMiniController(modifiersModel, objectFactory, configService);
    }

    public IEnumerable<(GameStateTransitionTask task, GameStateChangeFunc func)> GetStateChangeFuncs()
    {
        yield return (GameStateTransitionTask.VISUAL_TRANSITION_START, async (x) =>
        {
            await ShowContext(0.15f);

            if (x.Location != Location.MAIN_MENU)
            {
                if (x.Location == Location.STATION)
                {
                    var station = stationConfigs[run.StationId];
                    _context.SetMessage(station.name);
                }
                else
                {
                    _context.SetMessage("D6-Express");
                }
            }

            if (x.AwaitUserInput)
            {
                await UniTask.WhenAll(ShowMessage(), _context.ShowHint(), modifiersController.Show());
            }
            else
            {
                await modifiersController.Hide(true);
                await ShowMessage();
                await HideMessage();
                return;
            }

            var source = new UniTaskCompletionSource();

            void OnInteracted()
            {
                inputService.OnInteractPressed -= OnInteracted;
                source.TrySetResult();
            }

            inputService.OnInteractPressed += OnInteracted;

            await source.Task;
            await UniTask.WhenAll(HideMessage(), _context.HideHint(), modifiersController.Hide(true));
        }
        );
        yield return (GameStateTransitionTask.VISUAL_TRANSITION_FINISH, (x) => HideContext(0.15f));
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

    public UniTask ShowContext(float duration = -1)
    {
        return duration == -1 ? _context.ShowAsync(durationStart) : _context.ShowAsync(duration);
    }

    public UniTask HideContext(float duration = -1)
    {
        return duration == -1 ? _context.HideAsync(durationEnd) : _context.HideAsync(duration);
    }

    private async UniTask ShowMessage()
    {
        await _context.ShowLocationName();
        await UniTask.Delay(500);
    }

    private UniTask HideMessage()
    {
        return _context.HideLocationName();
    }
}