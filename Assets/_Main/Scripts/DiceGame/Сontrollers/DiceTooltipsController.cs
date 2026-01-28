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
		private readonly DiceTableView tableView;

		private TextsConfig textsConfig;
		private IReadOnlyDictionary<string, DiceConfig> diceConfigsDict;

		private DiceModel currentDiceModel;
		private Camera mainCamera;
		public DiceTooltipsController(IUIService uiService, DiceGameModel diceGameModel, ConfigService configService,
			Camera mainCamera, DiceTableView tableView) 
			: base(uiService)
		{
			this.mainCamera = mainCamera;
			this.diceGameModel = diceGameModel;
			this.configService = configService;
			this.tableView = tableView;
		}

		protected override async UniTask OnPreloadAsync()
		{
			diceConfigsDict = await configService.GetConfigsAsync<DiceConfig>(ResourcePaths.Json.dice_types);
			textsConfig = await configService.GetFirstOrDefaultAsync<TextsConfig>(ResourcePaths.Json.texts_eng);
		}

		protected override void OnActivate()
		{
			base.OnActivate();

			_context.Hide();
			_context.HideTooltip();
			diceGameModel.OnDiceGameStateChanged += OnDiceGameStateChangedHandler;
			diceGameModel.ScreenDiceDictChanged += ScreenDiceDictChangedHandler;
			ScreenDiceDictChangedHandler();
			OnDiceGameStateChangedHandler();
		}

		private void OnDiceGameStateChangedHandler()
		{
			if (diceGameModel.DiceGameState is DiceGameState.BET)
			{
				_context.Hide();
				_context.HideTooltip();
			}
			else
			{
				_context.Show();
			}
		}

		protected override void OnDeactivate()
		{
			diceGameModel.OnDiceGameStateChanged -= OnDiceGameStateChangedHandler;
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
			currentDiceModel = diceModel;

			var header = textsConfig.texts[diceConfig.name];
			var description = textsConfig.texts[diceConfig.description];
			_context.SetHeaderText(header);
			_context.SetDescriptionText(description);
			_context.SetRarity(diceConfig.rarityEnum);


			var pos = diceGameModel.DiceGameState != DiceGameState.GAME
				? diceModel.CurrentPosition
				: tableView.TooltipPos;

			_context.SetPositionFromWorld(
				pos,
				Vector3.zero, 
				mainCamera
			);

			_context.ShowTooltip();
		}
		
		private void OnDiceHoverExit(DiceModel diceModel)
		{
			if (diceModel == currentDiceModel)
			{
				_context.HideTooltip();
			}
		}
	}
}