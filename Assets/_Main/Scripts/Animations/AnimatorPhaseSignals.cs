using Cysharp.Threading.Tasks;

public class AnimatorPhaseSignals
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
