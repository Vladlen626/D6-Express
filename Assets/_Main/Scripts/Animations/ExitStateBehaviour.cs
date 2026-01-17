using UnityEngine;

public class ExitStateBehaviour : StateMachineBehaviour
{
    public override void OnStateEnter(
        Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<AnimatorSignalBridge>()
            ?.ExitStarted?.TrySetResult();
    }

    public override void OnStateExit(
        Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<AnimatorSignalBridge>()
            ?.ExitFinished?.TrySetResult();
    }
}