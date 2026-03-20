using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlatformCore.Core;
using PlatformCore.Services.Audio;
using PlatformCore.Services.UI;

public class TransitionViewController : BaseContextController<UITransitionView>, IGameStateChanger
{
    private const float DURATION = .5f;
    private readonly IAudioService audioService;

    public TransitionViewController(IUIService uiService, IAudioService audioService) : base(uiService)
    {
        this.audioService = audioService;
    }

    public IEnumerable<(GameStateTransitionTask task, GameStateChangeFunc func)> GetStateChangeFuncs()
    {
        yield return (GameStateTransitionTask.VISUAL_TRANSITION_START, (x) => ShowContext(DURATION));
        yield return (GameStateTransitionTask.VISUAL_TRANSITION_FINISH, (x) => HideContext(DURATION));
    }

    public UniTask ShowContext(float duration = -1)
    {
        var resolvedDuration = duration == -1 ? DURATION : duration;
        if (resolvedDuration > 0f)
        {
            audioService?.PlaySound(SoundNames.Transition);
        }

        return _context.ShowAsync(resolvedDuration);
    }

    public UniTask HideContext(float duration = -1)
    {
        var resolvedDuration = duration == -1 ? DURATION : duration;
        if (resolvedDuration > 0f)
        {
            audioService?.PlaySound(SoundNames.Transition);
        }

        return _context.HideAsync(resolvedDuration);
    }
}
