using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Factory;
using UnityEngine;

public class InformationPanelStationController : IBaseController, IActivatable, IPreloadable
{
    private readonly Run run;
    private readonly InformationPanelView informationPanelView;
    private readonly ConfigService configService;

    private InformationPanelStationView activeStation;
    private Dictionary<string, StationConfig> stationConfigs;
    private RunConfig runConfig;

    public InformationPanelStationController(Run run, InformationPanelView informationPanelView, ConfigService configService)
    {
        this.run = run;
        this.informationPanelView = informationPanelView;
        this.configService = configService;
    }

    public void Activate()
    {
        run.LevelChanged += OnLevelChanged;
        run.RunStarted += OnRunStarted;

        OnLevelChanged();
    }

    public void Deactivate()
    {
        run.RunStarted -= OnRunStarted;
        run.LevelChanged -= OnLevelChanged;
    }

    public async UniTask PreloadAsync()
    {
        stationConfigs = await configService.GetConfigsAsync<StationConfig>(ResourcePaths.Json.stations);
        runConfig = await configService.GetFirstOrDefaultAsync<RunConfig>(ResourcePaths.Json.run_rules);
    }

    private void OnRunStarted()
    {
        if (runConfig == null || runConfig.levels == null || stationConfigs == null)
        {
            return;
        }

        var stationsView = informationPanelView.Stations;
        var levelCount = runConfig.levels.Length;
        var viewCount = stationsView.Count;
        var count = Mathf.Min(viewCount, levelCount);

        for (int i = 0; i < viewCount; i++)
        {
            var view = stationsView[i];
            if (!view)
            {
                continue;
            }

            var isActive = i < levelCount;
            view.gameObject.SetActive(isActive);

            if (!isActive)
            {
                continue;
            }

            var stationId = runConfig.levels[i].station_id;
            if (!string.IsNullOrEmpty(stationId) && stationConfigs.TryGetValue(stationId, out var data))
            {
                view.SetStationName(data.name);
            }
            else
            {
                view.SetStationName(stationId);
            }
        }

        informationPanelView.RefreshConnections();
        informationPanelView.SetProgress(run.Level);
    }

    private void OnLevelChanged()
    {
        activeStation?.SetActiveStation(false);

        if (run.Level < 0 || run.Level >= informationPanelView.Stations.Count)
        {
            activeStation = null;
            informationPanelView.SetProgress(run.Level);
            return;
        }

        activeStation = informationPanelView.Stations[run.Level];
        if (activeStation)
        {
            activeStation.SetActiveStation(true);
        }

        informationPanelView.SetProgress(run.Level);
    }
}
