using System;
using System.Collections.Generic;
using _Main.Scripts.Core.Services;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using PlatformCore.Services.Audio;
using TMPro;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public class DiceGameCombinationsDisplayController : IBaseController, IActivatable
	{
		private readonly DiceGameModel diceGameModel;
		private readonly DiceTableView diceTableView;
		private readonly IAsyncAwaiterPool turnFlowAwaiter;
		private readonly IAudioService audioService;
		private readonly Dictionary<string, CardRuntime> cardByKey = new(StringComparer.Ordinal);
		private readonly List<string> keysToRemove = new();
		private readonly Stack<TextMeshProUGUI> flyLabelPool = new();
		private readonly Stack<DiceCombinationCardView> cardViewPool = new();

		private bool isActive;
		private bool poolsPrewarmed;

		public DiceGameCombinationsDisplayController(
			DiceGameModel diceGameModel,
			DiceTableView diceTableView,
			IAsyncAwaiterPool turnFlowAwaiter,
			IAudioService audioService)
		{
			this.diceGameModel = diceGameModel ?? throw new ArgumentNullException(nameof(diceGameModel));
			this.diceTableView = diceTableView ?? throw new ArgumentNullException(nameof(diceTableView));
			this.turnFlowAwaiter = turnFlowAwaiter ?? throw new ArgumentNullException(nameof(turnFlowAwaiter));
			this.audioService = audioService;
		}

		public void Activate()
		{
			ValidateReferences();
			PrewarmPools();

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
			ApplyPreview(DiceCombinationCardsSnapshot.Empty);
		}

		private void OnDiceGameStateChangedHandler()
		{
			if (diceGameModel.DiceGameState != DiceGameState.GAME)
			{
				ApplyPreview(DiceCombinationCardsSnapshot.Empty);
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

			for (int i = 0; i < snapshot.Entries.Length; i++)
			{
				if (!isActive)
				{
					return;
				}

				var entry = snapshot.Entries[i];
				var runtime = GetOrCreateCard(entry);
				runtime.InPreview = true;
				runtime.PendingRemoval = false;
				runtime.View.SetScoreImmediate(entry.Score);
				if (!runtime.View.IsShown)
				{
					audioService?.PlaySound(SoundNames.Whoosh);
					await runtime.View.AnimateShowAsync();
				}

				if (!isActive)
				{
					return;
				}

				await AnimateScoreFlyAsync(entry.Score, runtime.View.FlyOrigin);
			}

			ApplyPreview(DiceCombinationCardsSnapshot.Empty);
		}

		private void ApplyPreview(DiceCombinationCardsSnapshot snapshot)
		{
			foreach (var runtime in cardByKey.Values)
			{
				runtime.InPreview = false;
			}

			if (snapshot.HasEntries)
			{
				for (int i = 0; i < snapshot.Entries.Length; i++)
				{
					var entry = snapshot.Entries[i];
					var runtime = GetOrCreateCard(entry);
					runtime.InPreview = true;
					runtime.PendingRemoval = false;
					runtime.View.SetScoreImmediate(entry.Score);
					if (!runtime.View.IsShown)
					{
						audioService?.PlaySound(SoundNames.Whoosh);
						runtime.View.AnimateShowAsync().RegisterAwaiter(turnFlowAwaiter).Forget();
					}
				}
			}

			keysToRemove.Clear();
			foreach (var pair in cardByKey)
			{
				var runtime = pair.Value;
				if (!runtime.InPreview)
				{
					keysToRemove.Add(pair.Key);
				}
			}

			for (int i = 0; i < keysToRemove.Count; i++)
			{
				ScheduleRemoveCard(keysToRemove[i]);
			}
		}

		private CardRuntime GetOrCreateCard(DiceCombinationCardEntry entry)
		{
			if (!cardByKey.TryGetValue(entry.Key, out var runtime))
			{
				var view = RentCardView();
				view.SetVisibleImmediate(false);
				runtime = new CardRuntime(view);
				cardByKey.Add(entry.Key, runtime);
			}

			runtime.View.SetPresentation(entry.DisplayName, entry.Faces);
			return runtime;
		}

		private void ScheduleRemoveCard(string key)
		{
			if (!cardByKey.TryGetValue(key, out var runtime))
			{
				return;
			}

			if (runtime.PendingRemoval)
			{
				return;
			}

			runtime.PendingRemoval = true;
			RemoveCardAsync(key, runtime).RegisterAwaiter(turnFlowAwaiter).Forget();
		}

		private async UniTask RemoveCardAsync(string key, CardRuntime runtime)
		{
			audioService?.PlaySound(SoundNames.Whoosh);
			await runtime.View.AnimateHideAsync();

			if (!cardByKey.TryGetValue(key, out var current) || !ReferenceEquals(current, runtime))
			{
				return;
			}

			if (runtime.InPreview)
			{
				runtime.PendingRemoval = false;
				if (!runtime.View.IsShown)
				{
					audioService?.PlaySound(SoundNames.Whoosh);
					runtime.View.AnimateShowAsync().RegisterAwaiter(turnFlowAwaiter).Forget();
				}
				return;
			}

			cardByKey.Remove(key);
			if (runtime.View)
			{
				ReleaseCardView(runtime.View);
			}
		}

		private void ClearAllCards()
		{
			foreach (var runtime in cardByKey.Values)
			{
				if (runtime.View)
				{
					ReleaseCardView(runtime.View);
				}
			}

			cardByKey.Clear();
		}

		private DiceCombinationCardView RentCardView()
		{
			while (cardViewPool.Count > 0)
			{
				var pooled = cardViewPool.Pop();
				if (pooled)
				{
					pooled.transform.SetParent(diceTableView.CombinationCardsRoot, false);
					pooled.gameObject.SetActive(true);
					return pooled;
				}
			}

			var created = UnityEngine.Object.Instantiate(
				diceTableView.CombinationCardPrefab,
				diceTableView.CombinationCardsRoot);
			created.PrewarmFaceIcons(diceTableView.CombinationCardFaceIconsPrewarmCount);
			return created;
		}

		private void ReleaseCardView(DiceCombinationCardView view)
		{
			if (!view)
			{
				return;
			}

			view.SetVisibleImmediate(false);
			view.gameObject.SetActive(false);
			view.transform.SetParent(diceTableView.CombinationCardsRoot, false);
			cardViewPool.Push(view);
		}

		private async UniTask AnimateScoreFlyAsync(int scoreDelta, RectTransform flyOrigin)
		{
			if (scoreDelta == 0)
			{
				return;
			}

			var flyLabel = RentFlyLabel();
			if (!flyLabel)
			{
				diceGameModel.NotifyCombinationScoreChunkLanded(scoreDelta);
				return;
			}

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
				var riseTarget = rect.localPosition + diceTableView.CombinationFlyRiseOffset;
				var riseDuration = diceTableView.CombinationFlyRiseDuration;
				if (riseDuration > 0f)
				{
					await rect.DOLocalMove(riseTarget, riseDuration).SetEase(Ease.OutQuad).AsyncWaitForCompletion().AsUniTask();
				}
				else
				{
					rect.localPosition = riseTarget;
				}

				var holdDuration = diceTableView.CombinationFlyHoldDuration;
				if (holdDuration > 0f)
				{
					await UniTask.Delay(TimeSpan.FromSeconds(holdDuration));
				}

				var moveDuration = diceTableView.CombinationFlyToTargetDuration;
				var fadeDuration = diceTableView.CombinationFlyFadeDuration;
				var sequence = DOTween.Sequence()
					.Join(rect.DOMove(diceTableView.TurnScoreFlyTarget.position, moveDuration).SetEase(Ease.InQuad));

				if (fadeDuration > 0f)
				{
					sequence.Join(flyLabel.DOFade(0f, fadeDuration).SetEase(Ease.Linear));
				}

				await sequence.AsyncWaitForCompletion().AsUniTask();
			}
			finally
			{
				ReleaseFlyLabel(flyLabel);
			}

			diceGameModel.NotifyCombinationScoreChunkLanded(scoreDelta);
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

			return null;
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

		private void PrewarmPools()
		{
			if (poolsPrewarmed)
			{
				return;
			}

			var cardPoolSize = diceTableView.CombinationCardsPrewarmCount;
			for (int i = cardViewPool.Count; i < cardPoolSize; i++)
			{
				var cardView = UnityEngine.Object.Instantiate(
					diceTableView.CombinationCardPrefab,
					diceTableView.CombinationCardsRoot);
				cardView.PrewarmFaceIcons(diceTableView.CombinationCardFaceIconsPrewarmCount);
				cardView.SetVisibleImmediate(false);
				cardView.gameObject.SetActive(false);
				cardViewPool.Push(cardView);
			}

			var flyLabelPoolSize = diceTableView.CombinationFlyLabelsPrewarmCount;
			for (int i = flyLabelPool.Count; i < flyLabelPoolSize; i++)
			{
				var flyLabel = UnityEngine.Object.Instantiate(
					diceTableView.CombinationFlyScorePrefab,
					diceTableView.CombinationFlyLayer);
				flyLabel.gameObject.SetActive(false);
				flyLabelPool.Push(flyLabel);
			}

			poolsPrewarmed = true;
		}

		private sealed class CardRuntime
		{
			public DiceCombinationCardView View { get; }
			public bool InPreview { get; set; }
			public bool PendingRemoval { get; set; }

			public CardRuntime(DiceCombinationCardView view)
			{
				View = view;
				InPreview = false;
				PendingRemoval = false;
			}
		}
	}
}
