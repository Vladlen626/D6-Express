using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
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
		private readonly ConfigService _configService;

		private readonly List<DiceModel> _allModels = new();
		private List<DiceModel> _selectedModels => _diceGameModel.GameSelectedDiceModelsList;

		private DicePositionsHandler _posHandler;
		private bool _isFinished;

		public DiceSelectionController(InventoryModel inventory, DiceTableView view, IObjectFactory factory, 
			ConfigService configService, DiceGameModel diceGameModel)
		{
			_inventory = inventory;
			_view = view;
			_factory = factory;
			_configService = configService;
			_diceGameModel = diceGameModel;
			_posHandler = view.SelectionStatePosHandler;
		}

		public void Activate()
		{
			_view.OnPlayClicked += OnPlayClickedHandler;
			SetupSelectionStage().Forget();
		}

		public void Deactivate()
		{
			CleanupUnselectedDices();
			_view.OnPlayClicked -= OnPlayClickedHandler;

			foreach (var pair in _diceGameModel.ScreenDiceDict)
			{
				pair.Value.OnDiceClicked.RemoveAllListeners();
			}
		}

		private async UniTaskVoid SetupSelectionStage()
		{
			var configs = await _configService.GetConfigsAsync<DiceConfig>(ResourcePaths.Json.dice_types);

			for (int i = 0; i < _inventory.DiceIdList.Count; i++)
			{
				string id = _inventory.DiceIdList[i];

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

			view.Initialize(config.id);
			view.transform.SetParent(startPos);

			DiceModel model = new DiceModel(config);
			model.SetCurrentPosition(startPos);

			_allModels.Add(model);
			_diceGameModel.AddDiceOnScreen(model, view);

			view.OnDiceClicked.AddListener(() => OnDiceClickedHandler(model));
		}

		private void OnDiceClickedHandler(DiceModel model)
		{
			if (_selectedModels.Contains(model))
			{
				_selectedModels.Remove(model);
			}
			else if (_selectedModels.Count < 6)
			{
				_selectedModels.Add(model);
			}

			_view.SetButtonInteractable("Play", _selectedModels.Count == 6);

			UpdateVisualPositions();
		}

		private void UpdateVisualPositions()
		{
			// 1. Расставляем выбранные кубы в забанкированные слоты
			for (int i = 0; i < _selectedModels.Count; i++)
			{
				MoveToSlot(_selectedModels[i], _posHandler.BankedPositions[i]);
			}

			// 2. Все остальные кубы расставляем по порядку в основные слоты стола
			var unselected = _allModels.Where(m => !_selectedModels.Contains(m)).ToList();
			for (int i = 0; i < unselected.Count; i++)
			{
				MoveToSlot(unselected[i], _posHandler.DicePositions[i]);
			}
		}

		private void MoveToSlot(DiceModel model, Transform slot)
		{
			model.SetCurrentPosition(slot);
			_diceGameModel.ScreenDiceDict[model].MoveToPosition(slot.position);
		}

		private void OnPlayClickedHandler()
		{
			if (_selectedModels.Count == 6)
			{
				_isFinished = true;
			}
		}

		public async UniTask WaitSelection() => await UniTask.WaitUntil(() => _isFinished);

		public void CleanupUnselectedDices()
		{
			foreach (var model in _allModels)
			{
				if (!_selectedModels.Contains(model))
				{
					var dice = _diceGameModel.ScreenDiceDict[model];
					_diceGameModel.RemoveDiceOnScreen(model);
					_factory.Destroy(dice.gameObject);
				}
			}
		}
	}
}