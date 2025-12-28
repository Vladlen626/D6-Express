using UnityEngine;

public class DicePositionsHandler : MonoBehaviour
{
	[Header("Active Dice Positions")]
	[SerializeField] private Transform[] dicePositions;
    
	[Header("Banked Dice Positions")]
	[SerializeField] private Transform[] bankedPositions;
    
	public Transform[] DicePositions => dicePositions;
	public Transform[] BankedPositions => bankedPositions;
}