using DG.Tweening;
using TMPro;
using UnityEngine;

public class TurnChipView : MonoBehaviour
{
	[SerializeField]
	private GameObject playerModel;
	
	[SerializeField]
	private GameObject enemyModel;
	
	[SerializeField]
	private Transform enemyTurnPos;
	
	[SerializeField]
	private Transform playerTurnPos;
	
	[SerializeField]
	private TextMeshPro turnNameText;
	
	[SerializeField]
	private float animDuration = 0.15f;

	public void SwitchTurn(bool isPlayerTurn)
	{
		turnNameText.text = isPlayerTurn ? "Player turn" : "Enemy turn";
		
		enemyModel.SetActive(!isPlayerTurn);
		playerModel.SetActive(isPlayerTurn);
		
		var chipPos = isPlayerTurn ? playerTurnPos.position : enemyTurnPos.position;
		transform.DOJump(chipPos, 0.25f, 2, animDuration);
	}
	
}