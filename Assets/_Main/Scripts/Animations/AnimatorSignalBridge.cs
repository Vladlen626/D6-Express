using Cysharp.Threading.Tasks;
using UnityEngine;

public class AnimatorSignalBridge : MonoBehaviour
{
    public UniTaskCompletionSource EnterStarted;
    public UniTaskCompletionSource EnterFinished;
    public UniTaskCompletionSource ExitStarted;
    public UniTaskCompletionSource ExitFinished;

    public void Reset()
    {
        EnterStarted  = new UniTaskCompletionSource();
        EnterFinished = new UniTaskCompletionSource();
        ExitStarted   = new UniTaskCompletionSource();
        ExitFinished  = new UniTaskCompletionSource();
    }
}
