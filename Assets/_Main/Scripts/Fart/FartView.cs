using UnityEngine;

public class FartView : MonoBehaviour
{
	[SerializeField]
	private Transform fartTfm;

	[SerializeField]
	private float radius;

	[SerializeField]
	private InteractableFart interactableFart;

	[SerializeField]
	private LayerMask fartLayerMask;

	public float Radius => radius;
	public InteractableFart InteractableFart => interactableFart;
	public Transform FartTfm => fartTfm;
	public LayerMask FartLayerMask => fartLayerMask;
}
