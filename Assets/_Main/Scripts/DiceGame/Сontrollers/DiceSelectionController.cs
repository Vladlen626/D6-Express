using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using _Main.Scripts.Core;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Audio;
using PlatformCore.Services.Factory;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public class DiceSelectionController : IBaseController, IActivatable
	{
		private readonly DiceGameModel _diceGameModel;
		private readonly InventoryModel _inventory;
		private readonly DiceTableView _view;
		private readonly IObjectFactory _factory;
		private readonly IAudioService _audioService;
		private readonly ConfigService _configService;
		
		private List<DiceModel> SelectedModel => _diceGameModel.PlayerDiceModelList;

		private CouplePositionsHandler _posHandler;
		private bool _isFinished;

		private int SelectionLimit => GetSelectionLimit();

		public DiceSelectionController(InventoryModel inventory, DiceTableView view, IObjectFactory factory, 
			ConfigService configService, DiceGameModel diceGameModel, IAudioService audioService)
		{
			_inventory = inventory;
			_view = view;
			_factory = factory;
			_configService = configService;
			_diceGameModel = diceGameModel;
			_audioService = audioService;
			_posHandler = view.SelectionStatePosHandler;
		}

		public void Activate()
		{
			_view.OnPlayClicked += OnPlayClickedHandler;
			_diceGameModel.OnMaxDiceCountChanged += OnMaxDiceCountChangedHandler;
			_view.SetButtonInteractable("Play", false);
			SetupSelectionStage().Forget();
		}

		public void Deactivate()
		{
			_diceGameModel.OnMaxDiceCountChanged -= OnMaxDiceCountChangedHandler;

			foreach (var pair in _diceGameModel.ScreenDiceDict)
			{
				pair.Value.OnDiceClicked.RemoveAllListeners();
			}

			CleanupUnselectedDices();
			_view.OnPlayClicked -= OnPlayClickedHandler;
		}

		private async UniTaskVoid SetupSelectionStage()
		{
			var configs = await _configService.GetConfigsAsync<ItemCatalogEntry>(ResourcePaths.Json.items_catalog);

			var diceIds = _inventory.DiceIdList;
			var gameSlotLimit = _diceGameModel.tableModel?.ActiveSlotsCount ?? int.MaxValue;
			var spawnLimit = Mathf.Min(diceIds.Count, _posHandler.FirstPosArray.Length, gameSlotLimit);

			for (int i = 0; i < spawnLimit; i++)
			{
				string id = diceIds[i];

				if (configs.TryGetValue(id, out var config) && config.typeEnum == ItemCatalogType.Dice)
				{
					await CreateDice(config, i);
				}
			}

			UpdateVisualPositions();
		}

		private async UniTask CreateDice(ItemCatalogEntry config, int index)
		{
			Transform startPos = _posHandler.FirstPosArray[index];

			DiceModel model = await DiceFactory.SpawnDiceViewAsync(
				_factory,
				config,
				startPos.position,
				Quaternion.identity,
				startPos,
				true,
				_audioService,
				_diceGameModel);

			_diceGameModel.SelectionDiceModelList.Add(model);
			_diceGameModel.ScreenDiceDict[model].OnDiceClicked.AddListener(() => OnDiceClickedHandler(model));
		}

		private void OnDiceClickedHandler(DiceModel model)
		{
			if (SelectedModel.Contains(model))
			{
				SelectedModel.Remove(model);
			}
			else if (SelectedModel.Count < SelectionLimit)
			{
				SelectedModel.Add(model);
			}

			_view.SetButtonInteractable("Play", SelectedModel.Count == SelectionLimit);

			UpdateVisualPositions();
		}

		private void UpdateVisualPositions()
		{
			// 1. Расставляем выбранные кубы в забанкированные слоты
			for (int i = 0; i < SelectedModel.Count; i++)
			{
				if (i < _posHandler.SecondPosArray.Length)
				{
					MoveToSlot(SelectedModel[i], _posHandler.SecondPosArray[i]);
				}
			}

			// 2. Все остальные кубы расставляем по порядку в основные слоты стола
			var unselected = _diceGameModel.SelectionDiceModelList.Where(m => !SelectedModel.Contains(m)).ToList();
			for (int i = 0; i < unselected.Count; i++)
			{
				if (i < _posHandler.FirstPosArray.Length)
				{
					MoveToSlot(unselected[i], _posHandler.FirstPosArray[i]);
				}
			}
		}

		private void MoveToSlot(DiceModel model, Transform slot)
		{
			model.SetCurrentPosition(slot);
			if (_diceGameModel.ScreenDiceDict.TryGetValue(model, out var view) && view)
			{
				view.transform.SetParent(slot);
				view.MoveToPosition(slot.position);
			}
			else
			{
				Debug.LogWarning($"[DiceSelectionController] Missing dice view for model {model?.ConfigId}");
			}
		}

		private int GetSelectionLimit()
		{
			var bankSlots = _posHandler.SecondPosArray?.Length ?? int.MaxValue;
			var gameActiveSlots = _diceGameModel.tableModel?.ActiveSlotsCount ?? int.MaxValue;
			var gameBankSlots = _diceGameModel.tableModel?.BankedSlotsCount ?? int.MaxValue;
			return Mathf.Min(_diceGameModel.MaxDiceCount, bankSlots, gameActiveSlots, gameBankSlots, _diceGameModel.SelectionDiceModelList.Count);
		}

		private void OnPlayClickedHandler()
		{
			if (SelectedModel.Count == SelectionLimit)
			{
				_isFinished = true;
			}
		}

		private void OnMaxDiceCountChangedHandler(int oldValue, int newValue)
		{
			_view.SetButtonInteractable("Play", SelectedModel.Count == SelectionLimit);
		}

		public async UniTask WaitSelection() => await UniTask.WaitUntil(() => _isFinished);

		public void CleanupUnselectedDices()
		{
			foreach (var model in _diceGameModel.SelectionDiceModelList)
			{
				if (!SelectedModel.Contains(model))
				{
					if (_diceGameModel.ScreenDiceDict.TryGetValue(model, out var dice))
					{
						_diceGameModel.RemoveDiceOnScreen(model);
						if (dice)
						{
							_factory.Destroy(dice.gameObject);
						}
					}
				}
			}

			_diceGameModel.SelectionDiceModelList.Clear();
		}
	}
}
