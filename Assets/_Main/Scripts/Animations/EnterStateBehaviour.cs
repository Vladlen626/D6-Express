using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;

public class EnterStateBehaviour : StateMachineBehaviour
{
    public override void OnStateEnter(
        Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<AnimatorSignalBridge>()
            ?.EnterStarted?.TrySetResult();
    }

    public override void OnStateExit(
        Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<AnimatorSignalBridge>()
            ?.EnterFinished?.TrySetResult();
    }
}
