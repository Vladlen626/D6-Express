using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace _Main.Scripts.Dice
{
	public class DiceItemTargetingController : IBaseController, IActivatable
	{
		private readonly DiceGameModel diceGameModel;
		private readonly HashSet<DiceView> selectedDiceViews = new();
		private readonly Dictionary<DiceView, UnityAction> diceClickHandlers = new();
		private Camera mainCamera;
		private CancellationTokenSource watchLoopCts;
		public IReadOnlyCollection<DiceView> SelectedDiceViews => selectedDiceViews;

		public DiceItemTargetingController(DiceGameModel diceGameModel)
		{
			this.diceGameModel = diceGameModel ?? throw new ArgumentNullException(nameof(diceGameModel));
		}

		public void Activate()
		{
			mainCamera = Camera.main;
			if (!mainCamera)
			{
				throw new InvalidOperationException("[DiceItemTargetingController] Camera.main is not available.");
			}

			if (Mouse.current == null)
			{
				throw new InvalidOperationException("[DiceItemTargetingController] Mouse input device is not available.");
			}

			diceGameModel.OnItemTargetingChanged += OnItemTargetingChanged;
			diceGameModel.ScreenDiceDictChanged += OnScreenDiceDictChanged;
			RebindDiceClickHandlers();

			watchLoopCts = new CancellationTokenSource();
			WatchPointerAsync(watchLoopCts.Token).Forget();
		}

		public void Deactivate()
		{
			diceGameModel.ScreenDiceDictChanged -= OnScreenDiceDictChanged;
			diceGameModel.OnItemTargetingChanged -= OnItemTargetingChanged;
			ClearDiceClickHandlers();
			ClearSelectedDiceVisuals();

			if (watchLoopCts == null)
			{
				return;
			}

			watchLoopCts.Cancel();
			watchLoopCts.Dispose();
			watchLoopCts = null;
			mainCamera = null;
		}

		private async UniTaskVoid WatchPointerAsync(CancellationToken cancellationToken)
		{
			try
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);

					if (!diceGameModel.IsItemTargetingActive)
					{
						continue;
					}

					if (diceGameModel.DiceGameState != DiceGameState.GAME ||
					    !diceGameModel.IsPlayerTurn ||
					    diceGameModel.IsDiceAnimationInProgress)
					{
						diceGameModel.CancelItemTargeting();
						continue;
					}

					var mouse = Mouse.current;
					if (mouse == null)
					{
						throw new InvalidOperationException("[DiceItemTargetingController] Mouse input device reference is lost.");
					}

					if (!mouse.leftButton.wasPressedThisFrame)
					{
						continue;
					}

					var mousePosition = mouse.position.ReadValue();
					if (TryGetDiceViewUnderPointer(mousePosition, out _))
					{
						continue;
					}

					if (IsPointerOverActiveItem(mousePosition))
					{
						continue;
					}

					diceGameModel.CancelItemTargeting();
				}
			}
			catch (OperationCanceledException)
			{
			}
		}

		private void OnItemTargetingChanged(bool oldValue, bool newValue)
		{
			ClearSelectedDiceVisuals();
		}

		private void OnScreenDiceDictChanged()
		{
			RebindDiceClickHandlers();
		}

		private void RebindDiceClickHandlers()
		{
			ClearDiceClickHandlers();

			if (diceGameModel.ScreenDiceDict == null)
			{
				return;
			}

			foreach (var kv in diceGameModel.ScreenDiceDict)
			{
				var view = kv.Value;
				if (!view)
				{
					continue;
				}

				var capturedView = view;
				UnityAction handler = () => OnDiceClicked(capturedView);
				capturedView.OnDiceClicked.AddListener(handler);
				diceClickHandlers[capturedView] = handler;
			}
		}

		private void ClearDiceClickHandlers()
		{
			foreach (var kv in diceClickHandlers)
			{
				if (kv.Key)
				{
					kv.Key.OnDiceClicked.RemoveListener(kv.Value);
				}
			}

			diceClickHandlers.Clear();
		}

		private void OnDiceClicked(DiceView diceView)
		{
			if (!diceGameModel.IsItemTargetingActive ||
			    diceGameModel.DiceGameState != DiceGameState.GAME ||
			    !diceGameModel.IsPlayerTurn ||
			    diceGameModel.IsDiceAnimationInProgress)
			{
				return;
			}

			if (!CanBeItemSelected(diceView))
			{
				return;
			}

			ToggleDiceItemSelection(diceView);
		}

		private bool TryGetDiceViewUnderPointer(Vector2 screenPosition, out DiceView diceView)
		{
			diceView = null;

			if (!mainCamera)
			{
				throw new InvalidOperationException("[DiceItemTargetingController] Camera.main reference is lost.");
			}

			var ray = mainCamera.ScreenPointToRay(screenPosition);
			if (!Physics.Raycast(ray, out var hit))
			{
				return false;
			}

			if (hit.collider == null)
			{
				return false;
			}

			diceView = hit.collider.GetComponentInParent<DiceView>();
			if (!diceView)
			{
				return false;
			}

			return true;
		}

		private bool IsPointerOverActiveItem(Vector2 screenPosition)
		{
			if (!mainCamera)
			{
				throw new InvalidOperationException("[DiceItemTargetingController] Camera.main reference is lost.");
			}

			var ray = mainCamera.ScreenPointToRay(screenPosition);
			if (!Physics.Raycast(ray, out var hit) || hit.collider == null)
			{
				return false;
			}

			if (hit.collider.GetComponentInParent<ItemView>() is not { } itemView)
			{
				return false;
			}

			return itemView.BoundItem != null &&
			       ReferenceEquals(itemView.BoundItem, diceGameModel.ActiveTargetingItem);
		}

		private bool CanBeItemSelected(DiceView diceView)
		{
			if (!diceView || diceGameModel.ScreenDiceDict == null)
			{
				return false;
			}

			if (!TryGetDiceModelByView(diceView, out var diceModel))
			{
				return false;
			}

			if (!diceGameModel.CurrentDiceModelList.Contains(diceModel))
			{
				return false;
			}

			if (diceModel.IsSaved || DiceGameUtils.IsDiceBanked(diceModel, diceGameModel.tableModel))
			{
				return false;
			}

			return true;
		}

		private bool TryGetDiceModelByView(DiceView diceView, out DiceModel diceModel)
		{
			foreach (var kv in diceGameModel.ScreenDiceDict)
			{
				if (ReferenceEquals(kv.Value, diceView))
				{
					diceModel = kv.Key;
					return true;
				}
			}

			diceModel = null;
			return false;
		}

		private void ToggleDiceItemSelection(DiceView diceView)
		{
			if (!diceView)
			{
				return;
			}

			if (selectedDiceViews.Remove(diceView))
			{
				diceView.SetItemTargetSelectedVisual(false);
				return;
			}

			selectedDiceViews.Add(diceView);
			diceView.SetItemTargetSelectedVisual(true);
		}

		private void ClearSelectedDiceVisuals()
		{
			foreach (var view in selectedDiceViews)
			{
				if (view)
				{
					view.SetItemTargetSelectedVisual(false);
				}
			}

			selectedDiceViews.Clear();
		}
	}
}
