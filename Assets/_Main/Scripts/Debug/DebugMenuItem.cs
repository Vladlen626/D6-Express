public abstract class DebugMenuItem
{
    public virtual string Path { get; }

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