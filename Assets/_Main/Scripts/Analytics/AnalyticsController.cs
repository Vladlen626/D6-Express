using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

public class AnalyticsController : IBaseController, IActivatable
{
	private readonly D6Game game;
	private readonly Run run;
	private readonly IAnalyticsService analytics;

	public AnalyticsController(D6Game game, Run run, IAnalyticsService analytics)
	{
		this.game = game;
		this.run = run;
		this.analytics = analytics;
	}

	public void Activate()
	{
		game.LocationChanged += OnLocationChanged;
		run.RunStarted += OnRunStarted;
		run.RunFinished += OnRunFinished;
	}

	public void Deactivate()
	{
		run.RunFinished -= OnRunFinished;
		run.RunStarted -= OnRunStarted;
		game.LocationChanged -= OnLocationChanged;
	}

	private void OnRunStarted()
	{
		analytics.TrackRunStarted(run);
	}

	private void OnRunFinished(Run.FinishType result)
	{
		analytics.TrackRunFinished(run, result);
	}

	private void OnLocationChanged()
	{
		analytics.TrackLocationChanged(game.Location);
	}
}
