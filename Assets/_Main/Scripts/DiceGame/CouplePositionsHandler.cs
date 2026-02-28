using UnityEngine;

public class CouplePositionsHandler : MonoBehaviour
{
	[Header("Active Dice Positions")] [SerializeField]
	private Transform[] firstPosArray;

	[Header("Banked Dice Positions")] [SerializeField]
	private Transform[] secondPosArray;

	public Transform[] FirstPosArray => firstPosArray;
	public Transform[] SecondPosArray => secondPosArray;
}