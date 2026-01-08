using System.Collections.Generic;
using _Main.Scripts.UI;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public class DiceTooltipsController : BaseContextController<UITooltip>
	{
		private readonly DiceGameModel diceGameModel;
		private readonly ConfigService configService;

		private IReadOnlyDictionary<string, DiceConfig> diceConfigsDict;
		
		private Camera mainCamera;
		
		public DiceTooltipsController(IUIService uiService, DiceGameModel diceGameModel, ConfigService configService,
			Camera mainCamera) 
			: base(uiService)
		{
			this.mainCamera = mainCamera;
			this.diceGameModel = diceGameModel;
			this.configService = configService;
		}

		protected override async UniTask OnPreloadAsync()
		{
			diceConfigsDict = await configService.GetConfigsAsync<DiceConfig>(ResourcePaths.Json.dice_types);
		}

		protected override void OnActivate()
		{
			base.OnActivate();

			_context.Hide();

			diceGameModel.ScreenDiceDictChanged += ScreenDiceDictChangedHandler;
			ScreenDiceDictChangedHandler();
		}

		protected override void OnDeactivate()
		{
			diceGameModel.ScreenDiceDictChanged -= ScreenDiceDictChangedHandler;
			
			base.OnDeactivate();
		}
		
		private void ScreenDiceDictChangedHandler()
		{
			UnsubscribeFromDiceHoverEvents();
			SubscribeOnDiceHoverEvents();
		}
		
		private void SubscribeOnDiceHoverEvents()
		{
			foreach (var keyValuePair in diceGameModel.ScreenDiceDict)
			{
				keyValuePair.Value.OnDiceHoverEnter.AddListener(() => OnDiceHoverEnter(keyValuePair.Key));
				keyValuePair.Value.OnDiceHoverExit.AddListener(() => OnDiceHoverExit(keyValuePair.Key));
			}
		}

		private void UnsubscribeFromDiceHoverEvents()
		{
			foreach (var keyValuePair in diceGameModel.ScreenDiceDict)
			{
				keyValuePair.Value.OnDiceHoverEnter.RemoveAllListeners();
				keyValuePair.Value.OnDiceHoverExit.RemoveAllListeners();
			}
		}

		private void OnDiceHoverEnter(DiceModel diceModel)
		{
			if (!diceConfigsDict.TryGetValue(diceModel.ConfigId, out DiceConfig diceConfig))
			{
				return;
			}

			_context.SetHeaderText(diceConfig.name);
			_context.SetDescriptionText(diceConfig.description);

			_context.SetPositionFromWorld(
				diceModel.CurrentPosition,
				Vector3.zero, 
				mainCamera
			);

			_context.Show();
		}
		
		private void OnDiceHoverExit(DiceModel diceModel)
		{
			_context.Hide();
		}
	}
}