using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

public delegate UniTask GameStateChangeFunc(GameStateTransition data);

public class GameStateController : IBaseController, IActivatable
{
	private readonly List<GameStateChangeFunc>[] funcs = new List<GameStateChangeFunc>[Enum.GetValues(typeof(GameStateTransitionTask)).Length];

	private readonly D6Game game;
	private readonly Run run;

	private static readonly GameStateTransitionTask[] RunStartRecipe =
	{
		GameStateTransitionTask.LOCK_PLAYER_INPUT,
		GameStateTransitionTask.VISUAL_TRANSITION_START,
		GameStateTransitionTask.CHARACTER_TRANSITION_START,
		GameStateTransitionTask.SHOW_STATS,
		GameStateTransitionTask.VISUAL_TRANSITION_FINISH,
		GameStateTransitionTask.UNLOCK_CURSOR,
		GameStateTransitionTask.AWAIT_STATS,
		GameStateTransitionTask.LOCK_CURSOR,
		GameStateTransitionTask.VISUAL_TRANSITION_START,
		GameStateTransitionTask.HIDE_STATS,
		GameStateTransitionTask.CHANGE_LOCATION,
		GameStateTransitionTask.NPC_RESPAWN,
		GameStateTransitionTask.SHOP_RESTOCK,
		GameStateTransitionTask.CHARACTER_TRANSITION_FINISH,
		GameStateTransitionTask.ENABLE_CHARACTER,
		GameStateTransitionTask.VISUAL_TRANSITION_FINISH,
		GameStateTransitionTask.UNLOCK_PLAYER_INPUT,
	};

	private static readonly GameStateTransitionTask[] TickRecipe =
	{
		GameStateTransitionTask.LOCK_PLAYER_INPUT,
		GameStateTransitionTask.VISUAL_TRANSITION_START,
		GameStateTransitionTask.CHARACTER_TRANSITION_START,
		GameStateTransitionTask.NPC_RESPAWN,
		GameStateTransitionTask.SHOP_RESTOCK,
		GameStateTransitionTask.CHARACTER_TRANSITION_FINISH,
		GameStateTransitionTask.VISUAL_TRANSITION_FINISH,
		GameStateTransitionTask.UNLOCK_PLAYER_INPUT,
	};

	private static readonly GameStateTransitionTask[] DayRecipe =
	{
		GameStateTransitionTask.LOCK_PLAYER_INPUT,
		GameStateTransitionTask.VISUAL_TRANSITION_START,
		GameStateTransitionTask.CHARACTER_TRANSITION_START,
		GameStateTransitionTask.SHOW_STATS,
		GameStateTransitionTask.VISUAL_TRANSITION_FINISH,
		GameStateTransitionTask.UNLOCK_CURSOR,
		GameStateTransitionTask.AWAIT_STATS,
		GameStateTransitionTask.LOCK_CURSOR,
		GameStateTransitionTask.VISUAL_TRANSITION_START,
		GameStateTransitionTask.HIDE_STATS,
		GameStateTransitionTask.NPC_RESPAWN,
		GameStateTransitionTask.SHOP_RESTOCK,
		GameStateTransitionTask.CHARACTER_TRANSITION_FINISH,
		GameStateTransitionTask.VISUAL_TRANSITION_FINISH,
		GameStateTransitionTask.UNLOCK_PLAYER_INPUT,
	};

	private static readonly GameStateTransitionTask[] LocationRecipe =
	{
		GameStateTransitionTask.LOCK_PLAYER_INPUT,
		GameStateTransitionTask.VISUAL_TRANSITION_START,
		GameStateTransitionTask.CHARACTER_TRANSITION_START,
		GameStateTransitionTask.SHOW_STATS,
		GameStateTransitionTask.VISUAL_TRANSITION_FINISH,
		GameStateTransitionTask.UNLOCK_CURSOR,
		GameStateTransitionTask.AWAIT_STATS,
		GameStateTransitionTask.LOCK_CURSOR,
		GameStateTransitionTask.VISUAL_TRANSITION_START,
		GameStateTransitionTask.HIDE_STATS,
		GameStateTransitionTask.CHANGE_LOCATION,
		GameStateTransitionTask.NPC_RESPAWN,
		GameStateTransitionTask.SHOP_RESTOCK,
		GameStateTransitionTask.CHARACTER_TRANSITION_FINISH,
		GameStateTransitionTask.VISUAL_TRANSITION_FINISH,
		GameStateTransitionTask.UNLOCK_PLAYER_INPUT,
	};

	private static readonly GameStateTransitionTask[] LocationMainMenuRecipe =
	{
		GameStateTransitionTask.LOCK_PLAYER_INPUT,
		GameStateTransitionTask.VISUAL_TRANSITION_START,
		GameStateTransitionTask.CHARACTER_TRANSITION_START,
		GameStateTransitionTask.LOCK_CURSOR,
		GameStateTransitionTask.HIDE_LOSE,
		GameStateTransitionTask.HIDE_WIN,
		GameStateTransitionTask.CHANGE_LOCATION,
		GameStateTransitionTask.NPC_RESPAWN,
		GameStateTransitionTask.UNLOCK_CURSOR,
		GameStateTransitionTask.CHARACTER_TRANSITION_FINISH,
		GameStateTransitionTask.VISUAL_TRANSITION_FINISH,
		GameStateTransitionTask.UNLOCK_PLAYER_INPUT,
	};

	private static readonly GameStateTransitionTask[] LocationStationToTrainRecipe =
	{
		GameStateTransitionTask.LOCK_PLAYER_INPUT,
		GameStateTransitionTask.VISUAL_TRANSITION_START,
		GameStateTransitionTask.CHARACTER_TRANSITION_START,
		GameStateTransitionTask.LOCK_CURSOR,
		GameStateTransitionTask.CHANGE_LOCATION,
		GameStateTransitionTask.NPC_RESPAWN,
		GameStateTransitionTask.SHOP_RESTOCK,
		GameStateTransitionTask.UNLOCK_CURSOR,
		GameStateTransitionTask.CHARACTER_TRANSITION_FINISH,
		GameStateTransitionTask.VISUAL_TRANSITION_FINISH,
		GameStateTransitionTask.UNLOCK_PLAYER_INPUT,
	};

	private static readonly GameStateTransitionTask[] RunFinishedWinRecipe =
	{
		GameStateTransitionTask.LOCK_PLAYER_INPUT,
		GameStateTransitionTask.VISUAL_TRANSITION_START,
		GameStateTransitionTask.CHARACTER_TRANSITION_START,
		GameStateTransitionTask.UNLOCK_CURSOR,
		GameStateTransitionTask.SHOW_WIN,
		GameStateTransitionTask.CHARACTER_TRANSITION_FINISH,
		GameStateTransitionTask.VISUAL_TRANSITION_FINISH,
		GameStateTransitionTask.DISABLE_CHARACTER,
		GameStateTransitionTask.UNLOCK_PLAYER_INPUT,
	};

	private static readonly GameStateTransitionTask[] RunFinishedLoseRecipe =
	{
		GameStateTransitionTask.LOCK_PLAYER_INPUT,
		GameStateTransitionTask.CHARACTER_TRANSITION_START,
		GameStateTransitionTask.VISUAL_TRANSITION_START,
		GameStateTransitionTask.UNLOCK_CURSOR,
		GameStateTransitionTask.SHOW_LOSE,
		GameStateTransitionTask.VISUAL_TRANSITION_FINISH,
		GameStateTransitionTask.CHARACTER_TRANSITION_FINISH,
		GameStateTransitionTask.DISABLE_CHARACTER,
		GameStateTransitionTask.UNLOCK_PLAYER_INPUT,
	};

	private static readonly GameStateTransitionTask[] RunFinishedAbortRecipe =
{
		GameStateTransitionTask.LOCK_PLAYER_INPUT,
		GameStateTransitionTask.CHARACTER_TRANSITION_START,
		GameStateTransitionTask.VISUAL_TRANSITION_START,
		GameStateTransitionTask.DISABLE_CHARACTER,
		GameStateTransitionTask.HIDE_LOSE,
		GameStateTransitionTask.HIDE_WIN,
		GameStateTransitionTask.CHANGE_LOCATION,
		GameStateTransitionTask.UNLOCK_CURSOR,
		GameStateTransitionTask.CHARACTER_TRANSITION_FINISH,
		GameStateTransitionTask.VISUAL_TRANSITION_FINISH,
		GameStateTransitionTask.UNLOCK_PLAYER_INPUT,
	};

	public GameStateController(D6Game game, Run run)
	{
		this.game = game;
		this.run = run;
	}

	public void Activate()
	{
		game.LocationChangeRequested += OnLocationChangeRequested;
		run.RunStarted += OnRunStarted;
	}

	public void Deactivate()
	{
		run.RunStarted -= OnRunStarted;
		game.LocationChangeRequested -= OnLocationChangeRequested;
	}

	public void AddChanger(IGameStateChanger gameStateChanger)
	{
		foreach (var (task, func) in gameStateChanger.GetStateChangeFuncs())
		{
			AddTask(func, task);
		}
	}

	public void AddTask(GameStateChangeFunc func, GameStateTransitionTask task = GameStateTransitionTask.OTHER)
	{
		int index = (int)task;
		if (funcs[index] == null)
		{
			funcs[index] = new();
		}
		funcs[index].Add(func);
	}

	private void OnRunStarted()
	{
		game.TickChanged += OnTickChanged;
		game.DayChanged += OnDayChanged;
		run.RunFinished += OnRunFinished;

		RequestChange(new GameStateTransition(RunStartRecipe, Location.STATION));
	}

	private void OnTickChanged()
	{
		RequestChange(new GameStateTransition(TickRecipe));
	}

	private void OnDayChanged()
	{
		RequestChange(new GameStateTransition(DayRecipe));
	}

	private void OnLocationChangeRequested(Location location)
	{
		if (location == Location.MAIN_MENU)
		{
			RequestChange(new GameStateTransition(LocationMainMenuRecipe, location, false));
		}
		else if (location == Location.TRAIN && game.Location == Location.STATION)
		{
			RequestChange(new GameStateTransition(LocationStationToTrainRecipe, location, false));
		}
		else
		{
			RequestChange(new GameStateTransition(LocationRecipe, location));
		}
	}

	private void OnRunFinished(Run.FinishType finished)
	{
		run.RunFinished -= OnRunFinished;
		game.DayChanged -= OnDayChanged;
		game.TickChanged -= OnTickChanged;

		GameStateTransitionTask[] recipe = null;
		switch (finished)
		{
			case Run.FinishType.WIN:
				recipe = RunFinishedWinRecipe;
				break;
			case Run.FinishType.LOSE:
				recipe = RunFinishedLoseRecipe;
				break;
			case Run.FinishType.ABORT:
				recipe = RunFinishedAbortRecipe;
				break;
		}
		RequestChange(new GameStateTransition(recipe, Location.MAIN_MENU));
	}

	private async void RequestChange(GameStateTransition data)
	{
		foreach (var task in data.Tasks)
		{
			foreach (var func in funcs[(int)task])
			{
				await func(data);
			}
		}
	}
}
