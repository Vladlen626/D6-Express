using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Main.Scripts.Core.Services;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;

public class TransitionViewController : BaseContextController<UITransitionView>
{
    private float durationStart = 0.1f;
    private float durationEnd = 0.5f;
    private readonly TransitionService transitionService;
    private readonly Run run;
    private readonly ConfigService configService;
    private readonly IInputService inputService;
    private Dictionary<string, StationConfig> stationConfigs;

    public TransitionViewController(IUIService uiService, TransitionService transitionService, Run run, ConfigService configService, IInputService inputService) : base(uiService)
    {
        this.transitionService = transitionService;
        this.run = run;
        this.configService = configService;
        this.inputService = inputService;
    }

    protected override async UniTask OnPreloadAsync()
    {
        stationConfigs = await configService.GetConfigsAsync<StationConfig>(ResourcePaths.Json.stations);
    }

    protected override void OnActivate()
    {
        base.OnActivate();

        transitionService.TransitionRequested += OnTransitionRequested;
    }

    protected override void OnDeactivate()
    {
        transitionService.TransitionRequested -= OnTransitionRequested;

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
        await UniTask.WhenAll(_context.ShowAsync(durationStart), _context.ShowLocationName());
        await UniTask.Delay(500);
    }

    private UniTask HideLocationName()
    {
        return UniTask.WhenAll(_context.HideAsync(durationEnd), _context.HideLocationName());
    }

    private UniTask ShowWakeUp()
    {
        return UniTask.WhenAll(_context.ShowAsync(durationStart), _context.ShowWakeUp());
    }

    private async UniTask WaitAndHideWakeUp()
    {
        var source = new UniTaskCompletionSource();

        void OnInteracted()
        {
            inputService.OnInteractPressed -= OnInteracted;
            source.TrySetResult();
        }

        inputService.OnInteractPressed += OnInteracted;

        await source.Task;

        await _context.HideWakeUp();
    }

    private void OnTransitionRequested()
    {
        transitionService.CurrentTransition.SetFirstTask(() => ShowContext());
        transitionService.CurrentTransition.AddTasks(GetTasks(transitionService.CurrentTransition.data));
        transitionService.CurrentTransition.SetLastTask(() => HideContext());
    }

    private IEnumerable<Func<UniTask>> GetTasks(Transition.Data data)
    {
        foreach (var item in data.tasks)
        {
            if (item == Transition.TaskType.CHANGE_LOCATION)
            {
                yield return async () =>
                {
                    if (run.Location == Location.STATION)
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
                };
            }
            else if (item == Transition.TaskType.WAKE_UP)
            {
                yield return () => ShowWakeUp();
                yield return () => WaitAndHideWakeUp();
            }
        }
    }
}