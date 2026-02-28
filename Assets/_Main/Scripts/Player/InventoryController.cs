using System;
using System.Collections.Generic;
using _Main.Scripts.Core;
using _Main.Scripts.Dice;
using Cysharp.Threading.Tasks;
using PlatformCore.Services.Audio;
using PlatformCore.Services.Factory;
using UnityEngine;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

public class InventoryController : IBaseController, IActivatable, IPreloadable
{
	private readonly InventoryModel inventory;
	private readonly IObjectFactory factory;
	private readonly ConfigService configService;
	private readonly IAudioService audioService;

	private List<DiceModel> spawnedDiceModelList = new();
	private Dictionary<string, ItemCatalogEntry> diceConfigs = new();
	private readonly InventoryView inventoryView;
	private readonly DiceGameModel diceGameModel;

	private int dicePosNum;

	public InventoryController(
		InventoryModel inventory,
		DiceGameModel diceGameModel,
		IObjectFactory factory,
		ConfigService configService,
		IAudioService audioService,
		InventoryView inventoryView)
	{
		this.inventory = inventory;
		this.factory = factory;
		this.configService = configService;
		this.audioService = audioService;
		this.inventoryView = inventoryView;
		this.diceGameModel = diceGameModel;
	}

	public async UniTask PreloadAsync()
	{
		diceConfigs = await configService.GetConfigsAsync<ItemCatalogEntry>(ResourcePaths.Json.items_catalog);
	}

	public void Activate()
	{
		inventory.DiceAdded += OnDiceAddedHandler;
		inventory.DiceRemoved += OnDiceRemovedHandler;
	}

	public void Deactivate()
	{
		inventory.DiceAdded -= OnDiceAddedHandler;
		inventory.DiceRemoved -= OnDiceRemovedHandler;

		ClearDice();
	}

	private void OnDiceAddedHandler(string diceId)
	{
		AddDiceAsync(diceId).Forget();
	}

	private void OnDiceRemovedHandler(string diceId)
	{
		foreach (var diceModel in spawnedDiceModelList)
		{
			if (diceModel.ConfigId != diceId)
			{
				continue;
			}

			dicePosNum--;
			factory.Destroy(diceGameModel.ScreenDiceDict[diceModel].gameObject);
			spawnedDiceModelList.Remove(diceModel);
			return;
		}
	}

	private async UniTask AddDiceAsync(string diceId)
	{
		if (diceConfigs == null || diceConfigs.Count == 0)
		{
			diceConfigs = await configService.GetConfigsAsync<ItemCatalogEntry>(ResourcePaths.Json.items_catalog);
		}

		if (!diceConfigs.TryGetValue(diceId, out var config) || config.typeEnum != ItemCatalogType.Dice)
		{
			Debug.LogWarning($"[InventoryController] Dice config '{diceId}' not found.");
			return;
		}

		var slot = inventoryView.CouplePositionsHandler.FirstPosArray[dicePosNum];
		dicePosNum++;

		var diceModel = await DiceFactory.SpawnDiceViewAsync(
			factory,
			config,
			slot.position,
			slot.rotation,
			slot,
			true,
			audioService,
			diceGameModel,
			resetYRotation: true);

		spawnedDiceModelList.Add(diceModel);
	}

	private void ClearDice()
	{
		foreach (var diceModel in spawnedDiceModelList)
		{
			factory.Destroy(diceGameModel.ScreenDiceDict[diceModel].gameObject);
		}

		spawnedDiceModelList.Clear();
	}
}
