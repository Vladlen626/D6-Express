using UnityEngine;

public class PositionsHandler : MonoBehaviour
{
	[SerializeField] private Transform[] positions;
	public Transform[] Positions => positions;
}