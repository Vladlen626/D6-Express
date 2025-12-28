using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public class DiceController : IBaseController, IActivatable
	{
		private readonly DiceGameModel diceGameModel;
		private readonly DiceModel diceModel;
		private readonly DiceView diceView;

		public DiceController(DiceModel inDiceModel, DiceView inDiceView, DiceGameModel inDiceGameModel)
		{
			diceModel = inDiceModel;
			diceView = inDiceView;
			diceGameModel = inDiceGameModel;
		}

		private void InitializePosition()
		{
			var startPos = diceGameModel.GetFreeActivePosition();
			if (startPos != null)
			{
				diceModel.SetCurrentPosition(startPos);
				diceView.transform.position = startPos.position;
			}
		}
		public void Activate()
		{
			diceModel.OnValueChanged += OnDiceValueChangedHandler;
			diceModel.OnDiceChosenChanged += OnDiceChosenChangedHandler;
			diceModel.OnDiceSavedChanged += OnDiceSavedChangedHandler;

			diceView.OnDiceClicked += OnDiceClickedHandler;

			InitializePosition();
			diceModel.Roll();
		}

		public void Deactivate()
		{
			diceModel.OnValueChanged -= OnDiceValueChangedHandler;
			diceModel.OnDiceChosenChanged -= OnDiceChosenChangedHandler;
			diceModel.OnDiceSavedChanged -= OnDiceSavedChangedHandler;

			if (diceView != null)
			{
				diceView.OnDiceClicked -= OnDiceClickedHandler;
			}

			ReleaseCurrentPosition();
		}

		private void OnDiceClickedHandler()
		{
			if (diceModel.IsSaved)
			{
				return;
			}

			diceModel.SetChosen(!diceModel.IsChosen);
		}

		private void OnDiceChosenChangedHandler()
		{
			diceView.UpdateChosenVisual(diceModel.IsChosen);
		}
		
		private void OnDiceValueChangedHandler()
		{
			diceView.SetSideMesh(diceModel.CurrentValue);
		}

		private void OnDiceSavedChangedHandler()
		{
			ReleaseCurrentPosition();

			var newPos = diceModel.IsSaved
				? diceGameModel.GetFreeBankedPosition()
				: diceGameModel.GetFreeActivePosition();
			
			diceModel.SetCurrentPosition(newPos);
			diceView.MoveToPosition(newPos.position);
		}
		
		private void ReleaseCurrentPosition()
		{
			if (diceModel.CurrentPosition == null)
			{
				return;
			}

			if (diceModel.IsSaved)
			{
				diceGameModel.ReleaseBankedPosition(diceModel.CurrentPosition);
			}
			else
			{
				diceGameModel.ReleaseActivePosition(diceModel.CurrentPosition);
			}

			diceModel.SetCurrentPosition(null);
		}
	}
}

