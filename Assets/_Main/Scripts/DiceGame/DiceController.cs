using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public class DiceController : IBaseController, IActivatable
	{
		private readonly DiceModel diceModel;
		private readonly DiceView diceView;

		public DiceController(DiceModel inDiceModel, DiceView inDiceView)
		{
			diceModel = inDiceModel;
			diceView = inDiceView;
		}
		public void Activate()
		{
			diceModel.OnValueChanged += OnDiceValueChangedHandler;
			diceModel.OnDiceChosenChanged += OnDiceChosenChangedHandler;
			diceModel.OnDiceSavedChanged += OnDiceSavedChangedHandler;

			diceView.OnDiceClicked += OnDiceClickedHandler;


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
			var pos = Vector3.zero;
			diceView.MoveToPosition(pos);
		}
	}
}

