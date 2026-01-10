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
    private readonly RunModel runModel;

    public override string Path => "Increment Day";

    public DbgMenuItemIncrementDays(RunModel runModel)
    {
        this.runModel = runModel;
    }

    public override void Execute()
    {
        runModel.LevelModel.IncrementDays();
    }
}

public class DbgMenuItemIncrementTicks : DebugMenuItem
{
    private readonly RunModel runModel;

    public override string Path => "Increment Tick";

    public DbgMenuItemIncrementTicks(RunModel runModel)
    {
        this.runModel = runModel;
    }

    public override void Execute()
    {
        runModel.LevelModel.IncrementTicks();
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
    private readonly RunModel runModel;

    public override string Path => "Switch to Station";

    public DbgMenuItemSwitchToStation(RunModel runModel)
    {
        this.runModel = runModel;
    }

    public override void Execute()
    {
        runModel.SetLevelState(LevelState.STATION);
    }
}


public class DbgMenuItemSwitchToTrain : DebugMenuItem
{
    private readonly RunModel runModel;

    public override string Path => "Switch to Train";

    public DbgMenuItemSwitchToTrain(RunModel runModel)
    {
        this.runModel = runModel;
    }

    public override void Execute()
    {
        runModel.SetLevelState(LevelState.TRAIN);
    }
}

public class DbgMenuItemOpenPlayerWindow : DebugMenuItem
{
    private readonly DebugWindowPlayer debugWindowPlayer;

    public override string Path => "Open Player Window";

    public DbgMenuItemOpenPlayerWindow(PlayerModel playerModel, PlayerView playerView, ConfigService configService)
    {
        debugWindowPlayer = new DebugWindowPlayer(playerModel, playerView, configService);
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