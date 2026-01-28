using System;
using System.Collections.Generic;

public class GameStateTransition
{
    public IReadOnlyList<GameStateTransitionTask> Tasks { get; private set; }
    public Location? Location { get; private set; }

    public GameStateTransition(IReadOnlyList<GameStateTransitionTask> tasks, Location? location = null)
    {
        Tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));
        Location = location;
    }
}