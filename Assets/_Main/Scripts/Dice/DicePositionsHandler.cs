using UnityEngine;

public class DicePositionsHandler : MonoBehaviour
{
	[SerializeField]
	private Transform[] dicePositions;
	
	public Transform[] DicePositions => dicePositions;
}