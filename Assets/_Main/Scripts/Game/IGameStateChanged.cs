using System;
using System.Collections.Generic;

public interface IGameStateChanger
{
	public IEnumerable<(StateTransitionTask task, GameStateChangeFunc func)> GetStateChangeFuncs();
}