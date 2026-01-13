using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Audio;

namespace _Main.Scripts.Dice
{
	public class DiceController : IBaseController, IActivatable
	{
		private readonly TableModel tableModel;
		private readonly DiceModel diceModel;
		private readonly DiceView diceView;
		private readonly IAudioService audioService;

		public DiceController(DiceModel diceModel, DiceView diceView, TableModel tableModel, IAudioService audioService)
		{
			this.diceModel = diceModel;
			this.diceView = diceView;
			this.tableModel = tableModel;
			this.audioService = audioService;
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
			audioService.PlaySound(SoundNames.DiceClick);
			
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

		private void OnDiceSavedChangedHandler(bool oldValue, bool newValue)
		{
			if (oldValue == newValue)
			{
				return;
			}

			ReleaseCurrentPosition();
		}

		private void OnDiceHiddenChangedHandler()
		{
			if (diceModel.IsHide)
			{
				diceView.Hide();
			}
			else
			{
				diceView.Show();
			}
		}
		
		private void ReleaseCurrentPosition()
		{
			if (!diceModel.CurrentPosition)
			{
				return;
			}

			if (diceModel.IsSaved)
			{
				tableModel.ReleaseBankedPosition(diceModel.CurrentPosition);
			}
			else
			{
				tableModel.ReleaseActivePosition(diceModel.CurrentPosition);
			}

			diceModel.SetCurrentPosition(null);
		}
	}
}

