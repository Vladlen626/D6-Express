using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;
using _Main.Scripts.Core.Services;
using UnityEngine.InputSystem;

namespace _Main.Scripts.Dice
{
	public class DiceGameUpgradeVisualController : BaseContextController<UIDiceUpgradeView>
	{
		private readonly DiceGameUpgradeController upgradeController;
		private readonly IAsyncAwaiterPool upgradeAwaiter;
		private readonly IResourceService resourceService;
		private readonly ILoggerService loggerService;
		private UIDiceUpgradeVariantView rouletteVariantPrefab;
		private bool warnedMissingVariantPrefab;
		private int showVersion;

		public DiceGameUpgradeVisualController(
			IUIService uiService,
			DiceGameUpgradeController upgradeController,
			IAsyncAwaiterPool upgradeAwaiter,
			IResourceService resourceService,
			ILoggerService loggerService)
			: base(uiService)
		{
			this.upgradeController = upgradeController;
			this.upgradeAwaiter = upgradeAwaiter;
			this.resourceService = resourceService;
			this.loggerService = loggerService;
		}

		protected override async UniTask OnPreloadAsync()
		{
			rouletteVariantPrefab = await resourceService.LoadAsync<UIDiceUpgradeVariantView>(ResourcePaths.UI.UIDiceUpgradeVariantView);
			await base.OnPreloadAsync();
		}

		protected override void OnActivate()
		{
			if (_context)
			{
				_context.SetRouletteVariantPrefab(rouletteVariantPrefab);
				_context.Hide();
			}

			if (!rouletteVariantPrefab && !warnedMissingVariantPrefab)
			{
				loggerService?.LogWarning(
					$"[DiceGameUpgradeVisualController] Failed to load prefab at '{ResourcePaths.UI.UIDiceUpgradeVariantView}'.");
				warnedMissingVariantPrefab = true;
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
				upgradeController.HideUpgradeDie();
				upgradeController.RestoreGameplayDiceAfterUpgrade();
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
			upgradeController?.HideGameplayDiceForUpgrade();
			try
			{
				_context.SetData(data);
				_context.Show();

				int current = ++showVersion;
				await UniTask.Delay(300);
				await WaitForClickAsync();

				if (current != showVersion || !_context)
				{
					return;
				}

				if (upgradeController != null)
				{
					await upgradeController.StopUpgradeRollAsync(data.RolledFace);
				}
				_context.ApplyRollResult();

				await UniTask.Delay(120);
				await WaitForClickAsync();

				if (current == showVersion && _context)
				{
					upgradeController?.HideUpgradeDie();
					_context.Hide();
				}
			}
			finally
			{
				upgradeController?.RestoreGameplayDiceAfterUpgrade();
			}
		}

		private async UniTask WaitForClickAsync()
		{
			await UniTask.WaitUntil(() =>
				!_context || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame));
		}
	}
}
