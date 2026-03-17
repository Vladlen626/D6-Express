using System;
using System.Collections.Generic;
using _Main.Scripts.Core.Services;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using TMPro;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public class DiceGameCombinationsDisplayController : IBaseController, IActivatable
	{
		private const float FlyDuration = 0.26f;

		private readonly DiceGameModel diceGameModel;
		private readonly DiceTableView diceTableView;
		private readonly IAsyncAwaiterPool turnFlowAwaiter;
		private readonly Dictionary<string, CardRuntime> cardByKey = new(StringComparer.Ordinal);
		private readonly List<string> keysToRemove = new();
		private readonly Stack<TextMeshProUGUI> flyLabelPool = new();

		private bool isActive;

		public DiceGameCombinationsDisplayController(
			DiceGameModel diceGameModel,
			DiceTableView diceTableView,
			IAsyncAwaiterPool turnFlowAwaiter)
		{
			this.diceGameModel = diceGameModel ?? throw new ArgumentNullException(nameof(diceGameModel));
			this.diceTableView = diceTableView ?? throw new ArgumentNullException(nameof(diceTableView));
			this.turnFlowAwaiter = turnFlowAwaiter ?? throw new ArgumentNullException(nameof(turnFlowAwaiter));
		}

		public void Activate()
		{
			ValidateReferences();

			isActive = true;
			diceGameModel.CombinationPreviewChanged += OnCombinationPreviewChangedHandler;
			diceGameModel.CombinationCommitted += OnCombinationCommittedHandler;
			diceGameModel.OnCurrentTurnChanged += OnCurrentTurnChangedHandler;
			diceGameModel.OnDiceGameStateChanged += OnDiceGameStateChangedHandler;

			ClearAllCards();
		}

		public void Deactivate()
		{
			isActive = false;
			diceGameModel.CombinationPreviewChanged -= OnCombinationPreviewChangedHandler;
			diceGameModel.CombinationCommitted -= OnCombinationCommittedHandler;
			diceGameModel.OnCurrentTurnChanged -= OnCurrentTurnChangedHandler;
			diceGameModel.OnDiceGameStateChanged -= OnDiceGameStateChangedHandler;

			ClearAllCards();
		}

		private void OnCurrentTurnChangedHandler(int oldValue, int newValue)
		{
			ClearAllCards();
		}

		private void OnDiceGameStateChangedHandler()
		{
			if (diceGameModel.DiceGameState != DiceGameState.GAME)
			{
				ClearAllCards();
			}
		}

		private void OnCombinationPreviewChangedHandler(DiceCombinationCardsSnapshot snapshot)
		{
			if (!isActive)
			{
				return;
			}

			if (diceGameModel.DiceGameState != DiceGameState.GAME || !diceGameModel.IsPlayerTurn)
			{
				ApplyPreview(DiceCombinationCardsSnapshot.Empty);
				return;
			}

			ApplyPreview(snapshot);
		}

		private void OnCombinationCommittedHandler(DiceCombinationCardsSnapshot snapshot)
		{
			if (!isActive || !snapshot.HasEntries)
			{
				return;
			}

			HandleCommitAsync(snapshot).RegisterAwaiter(turnFlowAwaiter).Forget();
		}

		private async UniTask HandleCommitAsync(DiceCombinationCardsSnapshot snapshot)
		{
			if (!isActive)
			{
				return;
			}

			ApplyPreview(DiceCombinationCardsSnapshot.Empty);

			for (int i = 0; i < snapshot.Entries.Length; i++)
			{
				if (!isActive)
				{
					return;
				}

				var entry = snapshot.Entries[i];
				var runtime = GetOrCreateCard(entry);
				var fromValue = runtime.CommittedScore;
				var toValue = fromValue + entry.Score;
				runtime.CommittedScore = toValue;

				await runtime.View.AnimateScoreAsync(fromValue, toValue);

				if (!isActive)
				{
					return;
				}

				await AnimateScoreFlyAsync(entry.Score, runtime.View.FlyOrigin);
			}
		}

		private void ApplyPreview(DiceCombinationCardsSnapshot snapshot)
		{
			foreach (var runtime in cardByKey.Values)
			{
				runtime.PreviewScore = 0;
			}

			if (snapshot.HasEntries)
			{
				for (int i = 0; i < snapshot.Entries.Length; i++)
				{
					var entry = snapshot.Entries[i];
					var runtime = GetOrCreateCard(entry);
					runtime.PreviewScore = entry.Score;
				}
			}

			keysToRemove.Clear();
			foreach (var pair in cardByKey)
			{
				var runtime = pair.Value;
				var displayedScore = runtime.CommittedScore + runtime.PreviewScore;
				runtime.View.SetScoreImmediate(displayedScore);

				if (runtime.CommittedScore <= 0 && runtime.PreviewScore <= 0)
				{
					keysToRemove.Add(pair.Key);
				}
			}

			for (int i = 0; i < keysToRemove.Count; i++)
			{
				RemoveCard(keysToRemove[i]);
			}
		}

		private CardRuntime GetOrCreateCard(DiceCombinationCardEntry entry)
		{
			if (!cardByKey.TryGetValue(entry.Key, out var runtime))
			{
				var view = UnityEngine.Object.Instantiate(
					diceTableView.CombinationCardPrefab,
					diceTableView.CombinationCardsRoot);
				runtime = new CardRuntime(view);
				cardByKey.Add(entry.Key, runtime);
			}

			runtime.View.SetPresentation(entry.DisplayName, entry.Faces);
			return runtime;
		}

		private void RemoveCard(string key)
		{
			if (!cardByKey.TryGetValue(key, out var runtime))
			{
				return;
			}

			cardByKey.Remove(key);
			if (runtime.View)
			{
				UnityEngine.Object.Destroy(runtime.View.gameObject);
			}
		}

		private void ClearAllCards()
		{
			foreach (var runtime in cardByKey.Values)
			{
				if (runtime.View)
				{
					UnityEngine.Object.Destroy(runtime.View.gameObject);
				}
			}

			cardByKey.Clear();
		}

		private async UniTask AnimateScoreFlyAsync(int scoreDelta, RectTransform flyOrigin)
		{
			if (scoreDelta == 0)
			{
				return;
			}

			var flyLabel = RentFlyLabel();
			try
			{
				flyLabel.alpha = 1f;
				if (scoreDelta > 0)
				{
					flyLabel.SetText("+{0:0}", scoreDelta);
				}
				else
				{
					flyLabel.SetText("{0:0}", scoreDelta);
				}

				var rect = flyLabel.rectTransform;
				rect.position = flyOrigin.position;
				rect.localScale = Vector3.one;

				var sequence = DOTween.Sequence()
					.Append(rect.DOMove(diceTableView.TurnScoreFlyTarget.position, FlyDuration).SetEase(Ease.OutQuad))
					.Join(flyLabel.DOFade(0f, FlyDuration).SetEase(Ease.InQuad));

				await sequence.AsyncWaitForCompletion().AsUniTask();
			}
			finally
			{
				ReleaseFlyLabel(flyLabel);
			}
		}

		private TextMeshProUGUI RentFlyLabel()
		{
			while (flyLabelPool.Count > 0)
			{
				var pooled = flyLabelPool.Pop();
				if (pooled)
				{
					pooled.gameObject.SetActive(true);
					return pooled;
				}
			}

			return UnityEngine.Object.Instantiate(
				diceTableView.CombinationFlyScorePrefab,
				diceTableView.CombinationFlyLayer);
		}

		private void ReleaseFlyLabel(TextMeshProUGUI flyLabel)
		{
			if (!flyLabel)
			{
				return;
			}

			flyLabel.gameObject.SetActive(false);
			flyLabelPool.Push(flyLabel);
		}

		private void ValidateReferences()
		{
			if (!diceTableView)
			{
				throw new MissingReferenceException("[DiceGameCombinationsDisplayController] DiceTableView is missing.");
			}

			diceTableView.ValidateCombinationCardReferences();
		}

		private sealed class CardRuntime
		{
			public DiceCombinationCardView View { get; }
			public int CommittedScore { get; set; }
			public int PreviewScore { get; set; }

			public CardRuntime(DiceCombinationCardView view)
			{
				View = view;
				CommittedScore = 0;
				PreviewScore = 0;
			}
		}
	}
}
