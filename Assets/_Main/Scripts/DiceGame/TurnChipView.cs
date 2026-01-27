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
	
	[SerializeField]
	private float jumpPower = 0.05f;

	public void SwitchTurn(bool isPlayerTurn)
	{
		turnNameText.text = isPlayerTurn ? "Player turn" : "Enemy turn";
		
		enemyModel.SetActive(!isPlayerTurn);
		playerModel.SetActive(isPlayerTurn);
		
		var chipPos = isPlayerTurn ? playerTurnPos.position : enemyTurnPos.position;
		transform.DOJump(chipPos, jumpPower, 2, animDuration);
	}
	
}