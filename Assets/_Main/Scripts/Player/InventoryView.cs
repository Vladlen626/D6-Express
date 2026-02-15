using UnityEngine;

public class InventoryView : MonoBehaviour
{
	[Header("Slots")] 
	[SerializeField] private CouplePositionsHandler couplePositionsHandler;

	private InventoryModel inventory;
	
	public CouplePositionsHandler CouplePositionsHandler => couplePositionsHandler;
}