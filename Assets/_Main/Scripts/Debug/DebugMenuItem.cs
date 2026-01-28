using System.Threading.Tasks;

public abstract class DebugMenuItem
{
    public abstract string Path { get; }

    public virtual async Task Preload() { }

    public abstract void Execute();
}

public class DbgMenuItemIncrementDays : DebugMenuItem
{
    private readonly Run run;

    public override string Path => "Increment Day";

    public DbgMenuItemIncrementDays(Run run)
    {
        this.run = run;
    }

    public override void Execute()
    {
        run.RequestIncrementDay();
    }
}

public class DbgMenuItemIncrementTicks : DebugMenuItem
{
    private readonly Run run;

    public override string Path => "Increment Tick";

    public DbgMenuItemIncrementTicks(Run run)
    {
        this.run = run;
    }

    public override void Execute()
    {
        run.RequestIncrementTick();
    }
}

public class DbgMenuItemSwitchToStation : DebugMenuItem
{
    private readonly D6Game game;

    public override string Path => "Switch to Station";

    public DbgMenuItemSwitchToStation(D6Game game)
    {
        this.game = game;
    }

    public override void Execute()
    {
        game.RequestSetLocation(Location.STATION);
    }
}


public class DbgMenuItemSwitchToTrain : DebugMenuItem
{
    private readonly D6Game game;

    public override string Path => "Switch to Train";

    public DbgMenuItemSwitchToTrain(D6Game game)
    {
        this.game = game;
    }

    public override void Execute()
    {
        game.RequestSetLocation(Location.TRAIN);
    }
}

public class DbgMenuItemOpenPlayerWindow : DebugMenuItem
{
    private readonly DebugWindowPlayer debugWindowPlayer;

    public override string Path => "Open Player Window";

    public DbgMenuItemOpenPlayerWindow(Run run, PlayerModel playerModel, PlayerView playerView, ConfigService configService, Notifications notifications)
    {
        debugWindowPlayer = new DebugWindowPlayer(run, playerModel, playerView, configService, notifications);
    }

    public override async Task Preload()
    {
        await base.Preload();

        await debugWindowPlayer.Preload();
    }

    public override void Execute()
    {
        debugWindowPlayer.Open();
    }
}