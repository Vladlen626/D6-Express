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
		GameStateTransitionTask.LOCK_CURSOR,
		GameStateTransitionTask.VISUAL_TRANSITION_START,
		GameStateTransitionTask.CHANGE_LOCATION,
		GameStateTransitionTask.NPC_RESPAWN,
		GameStateTransitionTask.SHOP_RESTOCK,
		GameStateTransitionTask.VISUAL_TRANSITION_FINISH
	};

	private static readonly GameStateTransitionTask[] TickRecipe =
	{
		GameStateTransitionTask.LOCK_CURSOR,
		GameStateTransitionTask.VISUAL_TRANSITION_START,
		GameStateTransitionTask.NPC_RESPAWN,
		GameStateTransitionTask.SHOP_RESTOCK,
		GameStateTransitionTask.VISUAL_TRANSITION_FINISH,
		GameStateTransitionTask.UNLOCK_CURSOR,
	};

	private static readonly GameStateTransitionTask[] DayRecipe =
	{
		GameStateTransitionTask.LOCK_CURSOR,
		GameStateTransitionTask.VISUAL_TRANSITION_START,
		GameStateTransitionTask.NPC_RESPAWN,
		GameStateTransitionTask.SHOP_RESTOCK,
		GameStateTransitionTask.SHOW_WAKE_UP,
		GameStateTransitionTask.VISUAL_TRANSITION_FINISH
	};

	private static readonly GameStateTransitionTask[] LocationRecipe =
	{
		GameStateTransitionTask.LOCK_CURSOR,
		GameStateTransitionTask.VISUAL_TRANSITION_START,
		GameStateTransitionTask.CHANGE_LOCATION,
		GameStateTransitionTask.NPC_RESPAWN,
		GameStateTransitionTask.SHOP_RESTOCK,
		GameStateTransitionTask.VISUAL_TRANSITION_FINISH
	};

	private static readonly GameStateTransitionTask[] LocationMainMenuRecipe =
	{
		GameStateTransitionTask.LOCK_CURSOR,
		GameStateTransitionTask.VISUAL_TRANSITION_START,
		GameStateTransitionTask.CHANGE_LOCATION,
		GameStateTransitionTask.NPC_RESPAWN,
		GameStateTransitionTask.SHOP_RESTOCK,
		GameStateTransitionTask.VISUAL_TRANSITION_FINISH,
		GameStateTransitionTask.UNLOCK_CURSOR
	};

	private static readonly GameStateTransitionTask[] RunFinishedWinRecipe =
	{
		GameStateTransitionTask.LOCK_CURSOR,
		GameStateTransitionTask.VISUAL_TRANSITION_START,
		GameStateTransitionTask.SHOW_WIN,
		GameStateTransitionTask.UNLOCK_CURSOR,
		GameStateTransitionTask.VISUAL_TRANSITION_FINISH,
	};

	private static readonly GameStateTransitionTask[] RunFinishedLoseRecipe =
	{
		GameStateTransitionTask.LOCK_CURSOR,
		GameStateTransitionTask.VISUAL_TRANSITION_START,
		GameStateTransitionTask.SHOW_LOSE,
		GameStateTransitionTask.UNLOCK_CURSOR,
		GameStateTransitionTask.VISUAL_TRANSITION_FINISH,
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
			RequestChange(new GameStateTransition(LocationMainMenuRecipe, location));
		}
		else
		{
			RequestChange(new GameStateTransition(LocationRecipe, location));
		}
	}

	private void OnRunFinished(bool finished)
	{
		run.RunFinished -= OnRunFinished;
		game.DayChanged -= OnDayChanged;
		game.TickChanged -= OnTickChanged;

		var recipe = finished ? RunFinishedWinRecipe : RunFinishedLoseRecipe;
		RequestChange(new GameStateTransition(recipe));
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