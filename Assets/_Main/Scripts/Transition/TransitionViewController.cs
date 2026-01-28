using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;

public class TransitionViewController : BaseContextController<UITransitionView>, IGameStateChanger
{
    private float durationStart = 0.1f;
    private float durationEnd = 0.5f;
    private readonly Run run;
    private readonly ConfigService configService;
    private Dictionary<string, StationConfig> stationConfigs;

    public TransitionViewController(IUIService uiService, Run run, ConfigService configService) : base(uiService)
    {
        this.run = run;
        this.configService = configService;
    }

    public IEnumerable<(StateTransitionTask task, GameStateChangeFunc func)> GetStateChangeFuncs()
    {
        yield return (StateTransitionTask.VISUAL_TRANSITION_START, (x) => ShowContext(0.15f));
        yield return (StateTransitionTask.VISUAL_TRANSITION_FINISH, (x) => HideContext(0.15f));
        yield return (StateTransitionTask.CHANGE_LOCATION, async (x) =>
        {
            if (x.Location != Location.MAIN_MENU)
            {
                if (x.Location == Location.STATION)
                {
                    var station = stationConfigs[run.StationId];
                    _context.SetLocationName(station.name);
                }
                else
                {
                    _context.SetLocationName("D6-Express");
                }

                await ShowLocationName();
                await HideLocationName();
            }
        }
        );
    }

    protected override async UniTask OnPreloadAsync()
    {
        stationConfigs = await configService.GetConfigsAsync<StationConfig>(ResourcePaths.Json.stations);
    }

    protected override void OnActivate()
    {
        base.OnActivate();

        _context.Hide();
    }

    protected override void OnDeactivate()
    {
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

    private async UniTask ShowLocationName()
    {
        await _context.ShowLocationName();
        await UniTask.Delay(500);
    }

    private UniTask HideLocationName()
    {
        return _context.HideLocationName();
    }
}