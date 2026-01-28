using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

public delegate UniTask GameStateChangeFunc(GameStateChange data);

public class GameStateController : IBaseController, IActivatable
{
	private readonly List<GameStateChangeFunc>[] funcs = new List<GameStateChangeFunc>[Enum.GetValues(typeof(StateTransitionTask)).Length];

	private readonly D6Game game;
	private readonly Run run;

	private static readonly StateTransitionTask[] RunStartRecipe =
	{
		StateTransitionTask.LOCK_CURSOR,
		StateTransitionTask.VISUAL_TRANSITION_START,
		StateTransitionTask.CHANGE_LOCATION,
		StateTransitionTask.NPC_RESPAWN,
		StateTransitionTask.SHOP_RESTOCK,
		StateTransitionTask.VISUAL_TRANSITION_FINISH
	};

	private static readonly StateTransitionTask[] TickRecipe =
	{
		StateTransitionTask.LOCK_CURSOR,
		StateTransitionTask.VISUAL_TRANSITION_START,
		StateTransitionTask.NPC_RESPAWN,
		StateTransitionTask.SHOP_RESTOCK,
		StateTransitionTask.VISUAL_TRANSITION_FINISH,
		StateTransitionTask.UNLOCK_CURSOR,
	};

	private static readonly StateTransitionTask[] DayRecipe =
	{
		StateTransitionTask.LOCK_CURSOR,
		StateTransitionTask.VISUAL_TRANSITION_START,
		StateTransitionTask.NPC_RESPAWN,
		StateTransitionTask.SHOP_RESTOCK,
		StateTransitionTask.SHOW_WAKE_UP,
		StateTransitionTask.VISUAL_TRANSITION_FINISH
	};

	private static readonly StateTransitionTask[] LocationRecipe =
	{
		StateTransitionTask.LOCK_CURSOR,
		StateTransitionTask.VISUAL_TRANSITION_START,
		StateTransitionTask.CHANGE_LOCATION,
		StateTransitionTask.NPC_RESPAWN,
		StateTransitionTask.SHOP_RESTOCK,
		StateTransitionTask.VISUAL_TRANSITION_FINISH
	};

	private static readonly StateTransitionTask[] LocationMainMenuRecipe =
	{
		StateTransitionTask.LOCK_CURSOR,
		StateTransitionTask.VISUAL_TRANSITION_START,
		StateTransitionTask.CHANGE_LOCATION,
		StateTransitionTask.NPC_RESPAWN,
		StateTransitionTask.SHOP_RESTOCK,
		StateTransitionTask.VISUAL_TRANSITION_FINISH,
		StateTransitionTask.UNLOCK_CURSOR
	};

	private static readonly StateTransitionTask[] RunFinishedWinRecipe =
	{
		StateTransitionTask.LOCK_CURSOR,
		StateTransitionTask.VISUAL_TRANSITION_START,
		StateTransitionTask.SHOW_WIN,
		StateTransitionTask.UNLOCK_CURSOR,
		StateTransitionTask.VISUAL_TRANSITION_FINISH,
	};

	private static readonly StateTransitionTask[] RunFinishedLoseRecipe =
	{
		StateTransitionTask.LOCK_CURSOR,
		StateTransitionTask.VISUAL_TRANSITION_START,
		StateTransitionTask.SHOW_LOSE,
		StateTransitionTask.UNLOCK_CURSOR,
		StateTransitionTask.VISUAL_TRANSITION_FINISH,
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

	public void AddTask(GameStateChangeFunc func, StateTransitionTask task = StateTransitionTask.OTHER)
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

		RequestChange(new GameStateChange(RunStartRecipe, Location.STATION));
	}

	private void OnTickChanged()
	{
		RequestChange(new GameStateChange(TickRecipe));
	}

	private void OnDayChanged()
	{
		RequestChange(new GameStateChange(DayRecipe));
	}

	private void OnLocationChangeRequested(Location location)
	{
		if (location == Location.MAIN_MENU)
		{
			RequestChange(new GameStateChange(LocationMainMenuRecipe, location));
		}
		else
		{
			RequestChange(new GameStateChange(LocationRecipe, location));
		}
	}

	private void OnRunFinished(bool finished)
	{
		run.RunFinished -= OnRunFinished;
		game.DayChanged -= OnDayChanged;
		game.TickChanged -= OnTickChanged;

		var recipe = finished ? RunFinishedWinRecipe : RunFinishedLoseRecipe;
		RequestChange(new GameStateChange(recipe));
	}

	private async void RequestChange(GameStateChange data)
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