using System;
using UnityEngine;

public class InventoryView : MonoBehaviour
{
	[Header("Slots")] 
	[SerializeField] private CouplePositionsHandler couplePositionsHandler;
	[SerializeField] private Transform[] modifierItemSlots;

	private InventoryModel inventory;
	
	public CouplePositionsHandler CouplePositionsHandler => couplePositionsHandler;
	public Transform[] ModifierItemSlots => modifierItemSlots;

	public void ValidateModifierItemSlots(int requiredCount)
	{
		if (requiredCount == 0)
		{
			return;
		}

		if (modifierItemSlots == null)
		{
			throw new InvalidOperationException("[InventoryView] Modifier item slots are not assigned.");
		}

		if (modifierItemSlots.Length < requiredCount)
		{
			throw new InvalidOperationException(
				$"[InventoryView] Modifier item slots count ({modifierItemSlots.Length}) is less than required items ({requiredCount}).");
		}

		for (int i = 0; i < requiredCount; i++)
		{
			if (!modifierItemSlots[i])
			{
				throw new InvalidOperationException($"[InventoryView] Modifier item slot at index {i} is not assigned.");
			}
		}
	}
}
