using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

namespace _Main.Scripts.Dice
{
	public class DiceController : IBaseController, IActivatable
	{
		private readonly TableModel _tableModel;
		private readonly DiceModel diceModel;
		private readonly DiceView diceView;

		public DiceController(DiceModel inDiceModel, DiceView inDiceView, TableModel inTableModel)
		{
			diceModel = inDiceModel;
			diceView = inDiceView;
			_tableModel = inTableModel;
		}

		public void Activate()
		{
			diceModel.OnValueChanged += OnDiceValueChangedHandler;
			diceModel.OnDiceChosenChanged += OnDiceChosenChangedHandler;
			diceModel.OnDiceSavedChanged += OnDiceSavedChangedHandler;
			diceModel.OnDiceHiddenChanged += OnDiceHiddenChangedHandler;

			diceView.OnDiceClicked.AddListener(OnDiceClickedHandler);
		}

		public void Deactivate()
		{
			diceModel.OnValueChanged -= OnDiceValueChangedHandler;
			diceModel.OnDiceChosenChanged -= OnDiceChosenChangedHandler;
			diceModel.OnDiceSavedChanged -= OnDiceSavedChangedHandler;
			diceModel.OnDiceHiddenChanged -= OnDiceHiddenChangedHandler;

			if (diceView)
			{
				diceView.OnDiceClicked.RemoveListener(OnDiceClickedHandler);
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
			diceView.SetRotation(diceModel.CurrentValue);
		}

		private void OnDiceSavedChangedHandler()
		{
			ReleaseCurrentPosition();

			var newPos = diceModel.IsSaved
				? _tableModel.GetFreeBankedPosition()
				: _tableModel.GetFreeActivePosition();
			
			diceModel.SetCurrentPosition(newPos);
			diceView.MoveToPosition(newPos.position);
		}

		private void OnDiceHiddenChangedHandler()
		{
			diceView.gameObject.SetActive(!diceModel.IsHide);
		}
		
		private void ReleaseCurrentPosition()
		{
			if (!diceModel.CurrentPosition)
			{
				return;
			}

			if (diceModel.IsSaved)
			{
				_tableModel.ReleaseBankedPosition(diceModel.CurrentPosition);
			}
			else
			{
				_tableModel.ReleaseActivePosition(diceModel.CurrentPosition);
			}

			diceModel.SetCurrentPosition(null);
		}
	}
}

