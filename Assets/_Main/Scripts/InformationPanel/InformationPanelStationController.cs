using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Factory;

public class InformationPanelStationController : IBaseController, IActivatable, IPreloadable
{
    private readonly Run run;
    private readonly InformationPanelView informationPanelView;
    private readonly ConfigService configService;

    private InformationPanelStationView activeStation;
    private List<StationConfig> stationConfigs;

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
        var stationConfigsDict = await configService.GetConfigsAsync<StationConfig>(ResourcePaths.Json.stations);
        stationConfigs = new();
        foreach (var item in stationConfigsDict)
        {
            stationConfigs.Add(item.Value);
        }
    }

    private void OnRunStarted()
    {
        // todo: сейчас ожидается что их равное количество
        // точно хочется настраивать количество
        for (int i = 0; i < stationConfigs.Count; i++)
        {
            var view = informationPanelView.Stations[i];
            var data = stationConfigs[i];
            view.SetStationName(data.name);
        }
    }

    private void OnLevelChanged()
    {
        activeStation?.SetActiveStation(false);
        activeStation = informationPanelView.Stations[run.Level];
        activeStation.SetActiveStation(true);
    }
}