using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class RestockLeverView : MonoBehaviour
{
	private static readonly int PulledParam = Animator.StringToHash("Pulled");

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private AnimatorSignalBridge bridge;

	private bool isPulling;

	public event Action RestockRequested;

	public bool IsPulling => isPulling;

	private void Awake()
	{
		bridge.Reset();
	}

	public void RequestRestock()
	{
		RestockRequested?.Invoke();
	}

	public async UniTask Pull()
	{
		if (isPulling)
		{
			return;
		}

		isPulling = true;
		animator.SetBool(PulledParam, true);
		await bridge.EnterFinished.Task;
		animator.SetBool(PulledParam, false);
		bridge.Reset();
		isPulling = false;
	}
}
