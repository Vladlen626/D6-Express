using System;
using System.Collections.Generic;

public sealed class GameStateChange
{
    public IReadOnlyList<StateTransitionTask> Tasks { get; private set; }
    public Location? Location { get; private set; }

    public GameStateChange(IReadOnlyList<StateTransitionTask> tasks, Location? location = null)
    {
        Tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));
        Location = location;
    }
}