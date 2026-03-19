using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Main.Scripts.Dice
{
	public class DiceItemTargetingVisualController : IBaseController, IActivatable
	{
		private readonly DiceGameModel diceGameModel;
		private readonly DiceItemViewRegistry itemViewRegistry;
		private readonly DiceItemTargetingView targetingView;
		private readonly DiceItemTargetingController targetingController;
		private readonly int lockedArrowPoolSize;
		private readonly List<DiceItemTargetingView> lockedArrowViews = new();
		private readonly List<DiceView> selectedDiceBuffer = new();
		private Camera mainCamera;
		private CancellationTokenSource watchLoopCts;

		public DiceItemTargetingVisualController(
			DiceGameModel diceGameModel,
			DiceItemViewRegistry itemViewRegistry,
			DiceTableView diceTableView,
			DiceItemTargetingController targetingController)
		{
			this.diceGameModel = diceGameModel ?? throw new ArgumentNullException(nameof(diceGameModel));
			this.itemViewRegistry = itemViewRegistry ?? throw new ArgumentNullException(nameof(itemViewRegistry));
			if (!diceTableView)
			{
				throw new ArgumentNullException(nameof(diceTableView));
			}

			targetingView = diceTableView.ItemTargetingView;
			lockedArrowPoolSize = diceTableView.ItemTargetingLockedArrowsPoolSize;
			if (lockedArrowPoolSize < 0)
			{
				throw new InvalidOperationException("[DiceItemTargetingVisualController] ItemTargetingLockedArrowsPoolSize must be >= 0.");
			}

			this.targetingController = targetingController ?? throw new ArgumentNullException(nameof(targetingController));
		}

		public void Activate()
		{
			if (!targetingView)
			{
				throw new InvalidOperationException("[DiceItemTargetingVisualController] DiceTableView.ItemTargetingView is not assigned.");
			}

			mainCamera = Camera.main;
			if (!mainCamera)
			{
				throw new InvalidOperationException("[DiceItemTargetingVisualController] Camera.main is not available.");
			}

			if (Mouse.current == null)
			{
				throw new InvalidOperationException("[DiceItemTargetingVisualController] Mouse input device is not available.");
			}

			targetingView.EnsureConfiguredOrThrow();
			targetingView.SetVisible(false);
			InitializeLockedArrows();

			diceGameModel.OnItemTargetingChanged += OnItemTargetingChanged;

			watchLoopCts = new CancellationTokenSource();
			WatchVisualLoopAsync(watchLoopCts.Token).Forget();
		}

		public void Deactivate()
		{
			diceGameModel.OnItemTargetingChanged -= OnItemTargetingChanged;

			if (watchLoopCts != null)
			{
				watchLoopCts.Cancel();
				watchLoopCts.Dispose();
				watchLoopCts = null;
			}

			if (targetingView)
			{
				targetingView.SetVisible(false);
			}

			DestroyLockedArrows();
			mainCamera = null;
		}

		private void OnItemTargetingChanged(bool oldValue, bool newValue)
		{
			if (!newValue)
			{
				HideAllArrows();
			}
		}

		private async UniTaskVoid WatchVisualLoopAsync(CancellationToken cancellationToken)
		{
			try
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
					UpdateTargetingVisual();
				}
			}
			catch (OperationCanceledException)
			{
			}
		}

		private void UpdateTargetingVisual()
		{
			if (!diceGameModel.IsItemTargetingActive ||
			    diceGameModel.DiceGameState != DiceGameState.GAME ||
			    !diceGameModel.IsPlayerTurn)
			{
				HideAllArrows();
				return;
			}

			var activeItem = diceGameModel.ActiveTargetingItem;
			if (activeItem == null || !itemViewRegistry.TryGetItemView(activeItem, out var itemView) || !itemView)
			{
				HideAllArrows();
				return;
			}

			var source = itemView.transform.position;
			UpdateLockedArrows(source);

			var pointerTarget = ResolvePointerWorldPoint(source);
			targetingView.SetPoints(source, pointerTarget);
			targetingView.SetVisible(true);
		}

		private void UpdateLockedArrows(Vector3 source)
		{
			selectedDiceBuffer.Clear();
			foreach (var selectedView in targetingController.SelectedDiceViews)
			{
				if (selectedView)
				{
					selectedDiceBuffer.Add(selectedView);
				}
			}

			for (var i = 0; i < lockedArrowViews.Count; i++)
			{
				var lockedArrow = lockedArrowViews[i];
				if (!lockedArrow)
				{
					continue;
				}

				if (i < selectedDiceBuffer.Count)
				{
					lockedArrow.SetPoints(source, selectedDiceBuffer[i].transform.position);
					lockedArrow.SetVisible(true);
				}
				else
				{
					lockedArrow.SetVisible(false);
				}
			}
		}

		private void InitializeLockedArrows()
		{
			DestroyLockedArrows();

			if (lockedArrowPoolSize <= 0)
			{
				return;
			}

			var sourceTransform = targetingView.transform;
			for (var i = 0; i < lockedArrowPoolSize; i++)
			{
				var clone = UnityEngine.Object.Instantiate(sourceTransform.gameObject, sourceTransform.parent);
				clone.name = $"{sourceTransform.gameObject.name}_LockedArrow_{i + 1}";
				if (!clone.TryGetComponent<DiceItemTargetingView>(out var cloneView))
				{
					throw new InvalidOperationException("[DiceItemTargetingVisualController] Locked arrow clone has no DiceItemTargetingView.");
				}

				cloneView.EnsureConfiguredOrThrow();
				cloneView.SetVisible(false);
				lockedArrowViews.Add(cloneView);
			}
		}

		private void HideAllArrows()
		{
			targetingView.SetVisible(false);
			for (var i = 0; i < lockedArrowViews.Count; i++)
			{
				var lockedArrow = lockedArrowViews[i];
				if (lockedArrow)
				{
					lockedArrow.SetVisible(false);
				}
			}
		}

		private void DestroyLockedArrows()
		{
			for (var i = 0; i < lockedArrowViews.Count; i++)
			{
				var lockedArrow = lockedArrowViews[i];
				if (lockedArrow)
				{
					UnityEngine.Object.Destroy(lockedArrow.gameObject);
				}
			}

			lockedArrowViews.Clear();
			selectedDiceBuffer.Clear();
		}

		private Vector3 ResolvePointerWorldPoint(Vector3 sourcePoint)
		{
			var mouse = Mouse.current;
			if (!mainCamera)
			{
				throw new InvalidOperationException("[DiceItemTargetingVisualController] Camera.main reference is lost.");
			}

			if (mouse == null)
			{
				throw new InvalidOperationException("[DiceItemTargetingVisualController] Mouse input device reference is lost.");
			}

			var mousePosition = mouse.position.ReadValue();
			var ray = mainCamera.ScreenPointToRay(mousePosition);
			var tablePlane = new Plane(Vector3.up, new Vector3(0f, sourcePoint.y, 0f));
			if (!tablePlane.Raycast(ray, out var enter))
			{
				throw new InvalidOperationException("[DiceItemTargetingVisualController] Pointer ray does not intersect table plane.");
			}

			var targetPoint = ray.GetPoint(enter);

			if (float.IsNaN(targetPoint.x) || float.IsNaN(targetPoint.y) || float.IsNaN(targetPoint.z))
			{
				throw new InvalidOperationException("[DiceItemTargetingVisualController] Failed to resolve targeting world point.");
			}

			return targetPoint;
		}
	}
}
