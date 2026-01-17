using System;
using System.Threading.Tasks;
using UnityEngine;

public class RestockLeverView : MonoBehaviour
{
    private AnimatorSignalBridge bridge;
    private Animator animator;

    public event Action RestockRequested;

    public void RequestRestock()
    {
        RestockRequested?.Invoke();
    }

    private void Awake()
    {
        bridge = GetComponent<AnimatorSignalBridge>();
        animator = GetComponent<Animator>();

        bridge.Reset();
    }

    public async Task Pull()
    {
        animator.SetBool("Pulled", true);

        await bridge.EnterFinished.Task;

        animator.SetBool("Pulled", false);
        bridge.Reset();
    }

    public bool IsPulling()
    {
        // todo плюнуть бы себе за это
        return animator.GetBool("Pulled");
    }
}