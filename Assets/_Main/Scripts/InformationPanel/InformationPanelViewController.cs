using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

public class InformationPanelViewController : IBaseController, IActivatable
{
    private readonly Run run;
    private readonly InformationPanelView informationPanelView;

    public InformationPanelViewController(Run Run, InformationPanelView informationPanelView)
    {
        run = Run;
        this.informationPanelView = informationPanelView;
    }

    public void Activate()
    {
        run.LevelChanged += OnLevelChanged;
        OnLevelChanged();
    }

    public void Deactivate()
    {
        run.LevelChanged -= OnLevelChanged;
    }

    private void OnLevelChanged()
    {
        informationPanelView.Anchor.position = informationPanelView.Stations[run.Level].position;
    }
}