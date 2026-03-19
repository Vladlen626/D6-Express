using System;
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
		private Camera mainCamera;
		private CancellationTokenSource watchLoopCts;

		public DiceItemTargetingVisualController(
			DiceGameModel diceGameModel,
			DiceItemViewRegistry itemViewRegistry,
			DiceTableView diceTableView)
		{
			this.diceGameModel = diceGameModel ?? throw new ArgumentNullException(nameof(diceGameModel));
			this.itemViewRegistry = itemViewRegistry ?? throw new ArgumentNullException(nameof(itemViewRegistry));
			targetingView = diceTableView ? diceTableView.ItemTargetingView : throw new ArgumentNullException(nameof(diceTableView));
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

			mainCamera = null;
		}

		private void OnItemTargetingChanged(bool oldValue, bool newValue)
		{
			if (!newValue)
			{
				targetingView.SetVisible(false);
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
				targetingView.SetVisible(false);
				return;
			}

			var activeItem = diceGameModel.ActiveTargetingItem;
			if (activeItem == null || !itemViewRegistry.TryGetItemView(activeItem, out var itemView) || !itemView)
			{
				targetingView.SetVisible(false);
				return;
			}

			var source = itemView.transform.position;
			var target = ResolvePointerWorldPoint(source);

			targetingView.SetPoints(source, target);
			targetingView.SetVisible(true);
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
