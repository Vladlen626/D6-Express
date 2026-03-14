using System;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

namespace _Main.Scripts.Dice
{
	public class DicePreGameController : IBaseController, IActivatable
	{
		private readonly SceneContext sceneContext;
		private readonly PlayerView playerView;
		private readonly PlayerModel playerModel;
		private readonly Run run;

		public DicePreGameController(SceneContext sceneContext, PlayerView playerView, PlayerModel playerModel, Run run)
		{
			this.sceneContext = sceneContext;
			this.playerView = playerView;
			this.playerModel = playerModel;
			this.run = run;
		}

		public void Activate()
		{
			sceneContext.DiceGameTableView.OnPlayRequested += OnPlayRequested;
		}

		public void Deactivate()
		{
			sceneContext.DiceGameTableView.OnPlayRequested -= OnPlayRequested;
		}

		private void OnPlayRequested()
		{
			if (playerModel.InventoryModel.CashCount <= 0)
			{
				// todo: убрать такой способ
				var interactable = sceneContext.DiceGameOpponent.GetComponent<InteractableSpeakable>();
				interactable.SetId(97);
				playerView.Interactor.Interact(interactable);
				interactable.ResetId();
			}
			else if (run.TicksPerDay - run.Tick == 0)
			{
				// todo: убрать такой способ
				var interactable = sceneContext.DiceGameOpponent.GetComponent<InteractableSpeakable>();
				interactable.SetId(95);
				playerView.Interactor.Interact(interactable);
				interactable.ResetId();
			}
			else
			{
				sceneContext.DiceGameTableView.AllowPlay();
			}
		}
	}
}
