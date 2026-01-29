using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
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

		private List<DiceModel> _allModels => _diceGameModel.ScreenDiceDict.Keys.ToList();
		private List<DiceModel> SelectedModel => _diceGameModel.PlayerDiceModelList;

		private DicePositionsHandler _posHandler;
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
			SetupSelectionStage().Forget();
		}

		public void Deactivate()
		{
			foreach (var pair in _diceGameModel.ScreenDiceDict)
			{
				pair.Value.OnDiceClicked.RemoveAllListeners();
			}

			CleanupUnselectedDices();
			_view.OnPlayClicked -= OnPlayClickedHandler;
		}

		private async UniTaskVoid SetupSelectionStage()
		{
			var configs = await _configService.GetConfigsAsync<DiceConfig>(ResourcePaths.Json.dice_types);

			var diceIds = _inventory.DiceIdList;
			var gameSlotLimit = _diceGameModel.tableModel?.ActiveSlotsCount ?? int.MaxValue;
			var spawnLimit = Mathf.Min(diceIds.Count, _posHandler.DicePositions.Length, gameSlotLimit);

			for (int i = 0; i < spawnLimit; i++)
			{
				string id = diceIds[i];

				if (configs.TryGetValue(id, out var config))
				{
					await CreateDice(config, i);
				}
			}

			UpdateVisualPositions();
		}

		private async UniTask CreateDice(DiceConfig config, int index)
		{
			Transform startPos = _posHandler.DicePositions[index];

			DiceView view = await _factory.CreateAsync<DiceView>(
				ResourcePaths.Items.DicePrefab, startPos.position, Quaternion.identity);

			view.Initialize(config.id, true, _audioService);
			view.transform.SetParent(startPos);

			DiceModel model = new DiceModel(config);
			model.SetCurrentPosition(startPos);
			
			_diceGameModel.AddDiceOnScreen(model, view);

			view.OnDiceClicked.AddListener(() => OnDiceClickedHandler(model));
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
				if (i < _posHandler.BankedPositions.Length)
				{
					MoveToSlot(SelectedModel[i], _posHandler.BankedPositions[i]);
				}
			}

			// 2. Все остальные кубы расставляем по порядку в основные слоты стола
			var unselected = _allModels.Where(m => !SelectedModel.Contains(m)).ToList();
			for (int i = 0; i < unselected.Count; i++)
			{
				if (i < _posHandler.DicePositions.Length)
				{
					MoveToSlot(unselected[i], _posHandler.DicePositions[i]);
				}
			}
		}

		private void MoveToSlot(DiceModel model, Transform slot)
		{
			model.SetCurrentPosition(slot);
			_diceGameModel.ScreenDiceDict[model].MoveToPosition(slot.position);
		}

		private int GetSelectionLimit()
		{
			var bankSlots = _posHandler.BankedPositions?.Length ?? int.MaxValue;
			var gameActiveSlots = _diceGameModel.tableModel?.ActiveSlotsCount ?? int.MaxValue;
			var gameBankSlots = _diceGameModel.tableModel?.BankedSlotsCount ?? int.MaxValue;
			return Mathf.Min(_diceGameModel.MaxDiceCount, bankSlots, gameActiveSlots, gameBankSlots, _allModels.Count);
		}

		private void OnPlayClickedHandler()
		{
			if (SelectedModel.Count == SelectionLimit)
			{
				_isFinished = true;
			}
		}

		public async UniTask WaitSelection() => await UniTask.WaitUntil(() => _isFinished);

		public void CleanupUnselectedDices()
		{
			foreach (var model in _allModels)
			{
				if (!SelectedModel.Contains(model))
				{
					var dice = _diceGameModel.ScreenDiceDict[model];
					_diceGameModel.RemoveDiceOnScreen(model);
					_factory.Destroy(dice.gameObject);
				}
			}
		}
	}
}
