using System.Threading.Tasks;
using _Main.Scripts.Core;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services;
using PlatformCore.Services.UI;

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

public class DbgMenuItemIncrementSleep : DebugMenuItem
{
    public override string Path => "Sleep";

    public override void Execute()
    {
        Locator.Resolve<IUIService>().GetWindow<UISleepView>().CloseEyes();
    }
}

public class DbgMenuItemIncrementWakeUp : DebugMenuItem
{
    public override string Path => "Wake Up";

    public override void Execute()
    {
        Locator.Resolve<IUIService>().GetWindow<UISleepView>().OpenEyes();
    }
}

public class DbgMenuItemSwitchToStation : DebugMenuItem
{
    private readonly Run run;

    public override string Path => "Switch to Station";

    public DbgMenuItemSwitchToStation(Run run)
    {
        this.run = run;
    }

    public override void Execute()
    {
        run.RequestSetLocation(Location.STATION);
    }
}


public class DbgMenuItemSwitchToTrain : DebugMenuItem
{
    private readonly Run run;

    public override string Path => "Switch to Train";

    public DbgMenuItemSwitchToTrain(Run run)
    {
        this.run = run;
    }

    public override void Execute()
    {
        run.RequestSetLocation(Location.TRAIN);
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