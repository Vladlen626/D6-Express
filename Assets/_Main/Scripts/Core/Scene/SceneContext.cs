using UnityEngine;

public class SceneContext : MonoBehaviour, ISceneContext
{
	[SerializeField]
	private DiceTableView diceGameTableView;
	
	public DiceTableView DiceGameTableView => diceGameTableView;
}
