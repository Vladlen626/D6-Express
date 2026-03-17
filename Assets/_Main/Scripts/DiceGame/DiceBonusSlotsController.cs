using System.Collections.Generic;
using DG.Tweening;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public class DiceBonusSlotsController : IBaseController, IActivatable
	{
		private const int BaseDiceCount = 6;
		private const float OpenAnimationDuration = 0.15f;

		private readonly DiceGameModel diceGameModel;
		private readonly DiceTableView diceTableView;
		private readonly Dictionary<Transform, Vector3> slotDefaultScales = new();
		private readonly Dictionary<Transform, Tween> slotOpenTweens = new();
		private readonly List<Tween> tweenBuffer = new();

		private int visibleGameSlotsCount;
		private int visibleSelectSlotsCount;

		private Transform[] GameSlots => diceTableView.DiceBonusSlots;
		private Transform[] SelectSlots => diceTableView.SelectDiceBonusSlots;

		public DiceBonusSlotsController(DiceGameModel diceGameModel, DiceTableView diceTableView)
		{
			this.diceGameModel = diceGameModel;
			this.diceTableView = diceTableView;
		}

		public void Activate()
		{
			if (diceGameModel == null)
			{
				throw new MissingReferenceException("[DiceBonusSlotsController] DiceGameModel is not assigned.");
			}

			if (!diceTableView)
			{
				throw new MissingReferenceException("[DiceBonusSlotsController] DiceTableView is not assigned.");
			}

			ValidateSlots(GameSlots, nameof(diceTableView.DiceBonusSlots));
			ValidateSlots(SelectSlots, nameof(diceTableView.SelectDiceBonusSlots));

			CacheSlotDefaultScales(GameSlots);
			CacheSlotDefaultScales(SelectSlots);

			diceGameModel.OnMaxDiceCountChanged += OnMaxDiceCountChangedHandler;
			diceGameModel.OnCurrentTurnChanged += OnCurrentTurnChangedHandler;
			diceGameModel.OnDiceGameStateChanged += OnDiceGameStateChangedHandler;

			UpdateVisibleSlots(false);
		}

		public void Deactivate()
		{
			diceGameModel.OnMaxDiceCountChanged -= OnMaxDiceCountChangedHandler;
			diceGameModel.OnCurrentTurnChanged -= OnCurrentTurnChangedHandler;
			diceGameModel.OnDiceGameStateChanged -= OnDiceGameStateChangedHandler;

			SetVisibleSlotsCount(GameSlots, 0, ref visibleGameSlotsCount, false);
			SetVisibleSlotsCount(SelectSlots, 0, ref visibleSelectSlotsCount, false);
			KillAllTweens();
		}

		private void OnMaxDiceCountChangedHandler(int oldValue, int newValue)
		{
			UpdateVisibleSlots(true);
		}

		private void OnCurrentTurnChangedHandler(int oldValue, int newValue)
		{
			UpdateVisibleSlots(true);
		}

		private void OnDiceGameStateChangedHandler()
		{
			UpdateVisibleSlots(true);
		}

		private void UpdateVisibleSlots(bool animateOpen)
		{
			switch (diceGameModel.DiceGameState)
			{
				case DiceGameState.DEFAULT:
					SetVisibleSlotsCount(GameSlots, 0, ref visibleGameSlotsCount, false);
					SetVisibleSlotsCount(SelectSlots, 0, ref visibleSelectSlotsCount, false);
					return;
				case DiceGameState.SELECT_DICE:
					var playerSelectVisible = CalculateBonusSlotsCount(
						diceGameModel.GetMaxDiceCount(true),
						SelectSlots.Length);
					SetVisibleSlotsCount(GameSlots, 0, ref visibleGameSlotsCount, false);
					SetVisibleSlotsCount(SelectSlots, playerSelectVisible, ref visibleSelectSlotsCount, animateOpen);
					return;
				case DiceGameState.BET:
				case DiceGameState.GAME:
					var gameVisible = CalculateBonusSlotsCount(
						diceGameModel.GetMaxDiceCount(diceGameModel.IsPlayerTurn),
						GameSlots.Length);
					SetVisibleSlotsCount(SelectSlots, 0, ref visibleSelectSlotsCount, false);
					SetVisibleSlotsCount(GameSlots, gameVisible, ref visibleGameSlotsCount, animateOpen);
					return;
				default:
					SetVisibleSlotsCount(GameSlots, 0, ref visibleGameSlotsCount, false);
					SetVisibleSlotsCount(SelectSlots, 0, ref visibleSelectSlotsCount, false);
					return;
			}
		}

		private static int CalculateBonusSlotsCount(int maxDiceCount, int totalBonusSlots)
		{
			return Mathf.Clamp(maxDiceCount - BaseDiceCount, 0, totalBonusSlots);
		}

		private void SetVisibleSlotsCount(
			Transform[] slots,
			int targetVisibleCount,
			ref int currentVisibleCount,
			bool animateOpen)
		{
			for (int i = 0; i < slots.Length; i++)
			{
				var slot = slots[i];
				if (!slot)
				{
					continue;
				}

				var shouldBeVisible = i < targetVisibleCount;
				var wasVisible = i < currentVisibleCount;

				if (shouldBeVisible)
				{
					ShowSlot(slot, animateOpen && !wasVisible);
				}
				else
				{
					HideSlot(slot);
				}
			}

			currentVisibleCount = targetVisibleCount;
		}

		private void ShowSlot(Transform slot, bool animate)
		{
			if (!slot)
			{
				return;
			}

			KillSlotTween(slot);
			slot.gameObject.SetActive(true);

			var targetScale = slotDefaultScales[slot];
			if (!animate)
			{
				slot.localScale = targetScale;
				return;
			}

			slot.localScale = Vector3.zero;
			var tween = slot.DOScale(targetScale, OpenAnimationDuration).SetEase(Ease.OutBack);
			tween.OnComplete(() => slotOpenTweens.Remove(slot));
			tween.OnKill(() => slotOpenTweens.Remove(slot));
			slotOpenTweens[slot] = tween;
		}

		private void HideSlot(Transform slot)
		{
			if (!slot)
			{
				return;
			}

			KillSlotTween(slot);
			slot.localScale = slotDefaultScales[slot];
			slot.gameObject.SetActive(false);
		}

		private void CacheSlotDefaultScales(Transform[] slots)
		{
			for (int i = 0; i < slots.Length; i++)
			{
				var slot = slots[i];
				if (!slot)
				{
					continue;
				}

				if (!slotDefaultScales.ContainsKey(slot))
				{
					slotDefaultScales.Add(slot, slot.localScale);
				}
			}
		}

		private static void ValidateSlots(Transform[] slots, string fieldName)
		{
			if (slots == null)
			{
				throw new MissingReferenceException($"[DiceBonusSlotsController] {fieldName} is not assigned.");
			}

			for (int i = 0; i < slots.Length; i++)
			{
				if (!slots[i])
				{
					throw new MissingReferenceException($"[DiceBonusSlotsController] {fieldName}[{i}] is not assigned.");
				}
			}
		}

		private void KillSlotTween(Transform slot)
		{
			if (!slot)
			{
				return;
			}

			if (slotOpenTweens.TryGetValue(slot, out var tween) && tween != null && tween.IsActive())
			{
				tween.Kill();
			}
		}

		private void KillAllTweens()
		{
			tweenBuffer.Clear();
			foreach (var pair in slotOpenTweens)
			{
				tweenBuffer.Add(pair.Value);
			}

			for (int i = 0; i < tweenBuffer.Count; i++)
			{
				var tween = tweenBuffer[i];
				if (tween != null && tween.IsActive())
				{
					tween.Kill();
				}
			}

			slotOpenTweens.Clear();
			tweenBuffer.Clear();
		}
	}
}
