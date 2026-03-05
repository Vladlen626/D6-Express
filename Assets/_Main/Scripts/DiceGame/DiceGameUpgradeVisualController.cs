using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.UI;
using _Main.Scripts.Core.Services;
using UnityEngine.InputSystem;

namespace _Main.Scripts.Dice
{
	public class DiceGameUpgradeVisualController : BaseContextController<UIDiceUpgradeView>
	{
		private readonly DiceGameUpgradeController upgradeController;
		private readonly IAsyncAwaiterPool upgradeAwaiter;
		private int showVersion;

		public DiceGameUpgradeVisualController(
			IUIService uiService,
			DiceGameUpgradeController upgradeController,
			IAsyncAwaiterPool upgradeAwaiter)
			: base(uiService)
		{
			this.upgradeController = upgradeController;
			this.upgradeAwaiter = upgradeAwaiter;
		}

		protected override void OnActivate()
		{
			if (_context)
			{
				_context.Hide();
			}

			if (upgradeController != null)
			{
				upgradeController.UpgradeApplied += OnUpgradeApplied;
			}
		}

		protected override void OnDeactivate()
		{
			if (upgradeController != null)
			{
				upgradeController.UpgradeApplied -= OnUpgradeApplied;
			}
		}

		private void OnUpgradeApplied(DiceUpgradeVisualData data)
		{
			if (!_context)
			{
				return;
			}

			ShowUpgradeAsync(data).RegisterAwaiter(upgradeAwaiter).Forget();
		}

		private async UniTask ShowUpgradeAsync(DiceUpgradeVisualData data)
		{
			_context.SetData(data);
			_context.Show();

			int current = ++showVersion;
			await UniTask.Delay(500);
			await WaitForContinueClickAsync();

			if (current == showVersion && _context)
			{
				upgradeController?.HideUpgradeDie();
				_context.Hide();
			}
		}

		private async UniTask WaitForContinueClickAsync()
		{
			await UniTask.WaitUntil(() =>
				!_context || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame));
		}
	}
}
