using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

public class TransitionService : IService
{
    public Transition CurrentTransition { get; private set; }

    public event Action TransitionRequested;
    public event Action TransitionStarted;
    public event Action TransitionFinished;

    public async Task Request(Transition.Data data, params Func<UniTask>[] tasks)
    {
        CurrentTransition = new Transition(data);

        CurrentTransition.AddTasks(tasks);

        TransitionRequested?.Invoke();

        TransitionStarted?.Invoke();

        await CurrentTransition.Start();
        TransitionFinished?.Invoke();

        CurrentTransition = null;
    }

    public bool IsInTransition()
    {
        return CurrentTransition != null;
    }

    public void Dispose() { }
}
