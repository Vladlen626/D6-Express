using System;
using System.Collections.Generic;

public interface IGameStateChanger
{
	public IEnumerable<(GameStateTransitionTask task, GameStateChangeFunc func)> GetStateChangeFuncs();
}