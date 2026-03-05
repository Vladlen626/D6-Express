using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.UI;

public class TransitionViewController : BaseContextController<UITransitionView>, IGameStateChanger
{
    private const float DURATION = .5f;

    public TransitionViewController(IUIService uiService) : base(uiService) { }

    public IEnumerable<(GameStateTransitionTask task, GameStateChangeFunc func)> GetStateChangeFuncs()
    {
        yield return (GameStateTransitionTask.VISUAL_TRANSITION_START, (x) => ShowContext(DURATION));
        yield return (GameStateTransitionTask.VISUAL_TRANSITION_FINISH, (x) => HideContext(DURATION));
    }

    public UniTask ShowContext(float duration = -1)
    {
        return duration == -1 ? _context.ShowAsync(DURATION) : _context.ShowAsync(duration);
    }

    public UniTask HideContext(float duration = -1)
    {
        return duration == -1 ? _context.HideAsync(DURATION) : _context.HideAsync(duration);
    }
}