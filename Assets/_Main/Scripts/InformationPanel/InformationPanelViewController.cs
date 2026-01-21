using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

public class InformationPanelViewController : IBaseController, IActivatable
{
    private readonly RunModel runModel;
    private readonly InformationPanelView informationPanelView;

    public InformationPanelViewController(RunModel RunModel, InformationPanelView informationPanelView)
    {
        runModel = RunModel;
        this.informationPanelView = informationPanelView;
    }

    public void Activate()
    {
        runModel.LevelIndexChanged += OnLevelChanged;
        OnLevelChanged();
    }

    public void Deactivate()
    {
        runModel.LevelIndexChanged -= OnLevelChanged;
    }

    private void OnLevelChanged()
    {
        informationPanelView.Anchor.position = informationPanelView.Stations[runModel.LevelIndex].position;
    }
}