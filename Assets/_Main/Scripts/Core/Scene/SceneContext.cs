using UnityEngine;

public class SceneContext : MonoBehaviour, ISceneContext
{
	[SerializeField]
	private DicePositionsHandler dicePositionsHandler;
	
	public DicePositionsHandler DicePositionsHandler => dicePositionsHandler;
}
