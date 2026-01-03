using _Main.Scripts.Core;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services;
using PlatformCore.Services.UI;

public abstract class DebugMenuItem
{
    public abstract string Path { get; }

    public abstract void Execute();
}

public class DbgMenuItemIncrementDays : DebugMenuItem
{
    private readonly LevelModel levelModel;

    public override string Path => "Increment Day";

    public DbgMenuItemIncrementDays(LevelModel levelModel)
    {
        this.levelModel = levelModel;
    }

    public override void Execute()
    {
        levelModel.IncrementDays();
    }
}

public class DbgMenuItemIncrementTicks : DebugMenuItem
{
    private readonly LevelModel levelModel;

    public override string Path => "Increment Tick";

    public DbgMenuItemIncrementTicks(LevelModel levelModel)
    {
        this.levelModel = levelModel;
    }

    public override void Execute()
    {
        levelModel.IncrementTicks();
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
    private readonly LevelModel levelModel;

    public override string Path => "Switch to Station";

    public DbgMenuItemSwitchToStation(LevelModel levelModel)
    {
        this.levelModel = levelModel;
    }

    public override void Execute()
    {
        levelModel.SetLevelState(LevelState.STATION);
    }
}


public class DbgMenuItemSwitchToTrain : DebugMenuItem
{
    private readonly LevelModel levelModel;

    public override string Path => "Switch to Train";

    public DbgMenuItemSwitchToTrain(LevelModel levelModel)
    {
        this.levelModel = levelModel;
    }

    public override void Execute()
    {
        levelModel.SetLevelState(LevelState.TRAIN);
    }
}

public class DbgMenuItemOpenPlayerWindow : DebugMenuItem
{
    private readonly DebugWindowPlayer debugWindowPlayer;

    public override string Path => "Open Player Window";

    public DbgMenuItemOpenPlayerWindow(PlayerModel playerModel, PlayerView playerView)
    {
        debugWindowPlayer = new DebugWindowPlayer(playerModel, playerView);
    }

    public override void Execute()
    {
        debugWindowPlayer.Open();
    }
}

public class DbgMenuItemOpenDebugVariablesWindow : DebugMenuItem
{
    private readonly DebugWindowVariables debugWindowVariables;

    public override string Path => "Open Debug Variables Window";

    public DbgMenuItemOpenDebugVariablesWindow(PlayerModel playerModel, PlayerView playerView)
    {
        debugWindowVariables = new DebugWindowVariables(playerModel, playerView);
    }

    public override void Execute()
    {
        debugWindowVariables.Open();
    }
}