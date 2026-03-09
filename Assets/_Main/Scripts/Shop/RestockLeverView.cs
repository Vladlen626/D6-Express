using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class RestockLeverView : MonoBehaviour
{
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private AnimatorSignalBridge bridge;

	private UniTask? currentPullTask;

	public event Action RestockRequested;

	public bool IsPulling => currentPullTask.HasValue;

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
		if (IsPulling) return; // Already pulling

		animator.SetBool("Pulled", true);

		currentPullTask = bridge.EnterFinished.Task;
		await currentPullTask.Value;

		animator.SetBool("Pulled", false);
		bridge.Reset();

		currentPullTask = null;
	}
}