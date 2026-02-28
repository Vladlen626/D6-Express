using System;
using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using PlatformCore.Services.UI;

// todo: ноды не должны дергать UI напрямую, контроллер должен следить за изменениями в диалоге
public class SpeechNodeChoice : SpeechNodeShowTextLine
{
    public event Action Accepted;
    public event Action Declined;

    public SpeechNodeChoice(string text) : base(text)
    {
    }

    protected override void StartInternal()
    {
        base.StartInternal();

        var uISpeechView = Locator.Resolve<IUIService>().GetWindow<UISpeechView>();
        uISpeechView.SetSpeakerName(Speech.Target.GetComponent<CharacterView>().CharacterName);
        uISpeechView.ShowChoiceOptions();

        Locator.Resolve<IInputService>().OnSpeechAccept += AcceptSelected;
        Locator.Resolve<IInputService>().OnSpeechDecline += DeclineSelected;
    }

    protected override void FinishInternal()
    {
        Locator.Resolve<IInputService>().OnSpeechDecline -= DeclineSelected;
        Locator.Resolve<IInputService>().OnSpeechAccept -= AcceptSelected;

        var uISpeechView = Locator.Resolve<IUIService>().GetWindow<UISpeechView>();
        uISpeechView.HideChoiceOptions();

        base.FinishInternal();
    }

    private void AcceptSelected()
    {
        Accepted?.Invoke();
        Finish();

    }

    private void DeclineSelected()
    {
        Declined?.Invoke();
        Finish();
    }

    public SpeechNodeChoice OnAccepted(SpeechNode node)
    {
        Accepted += () =>
        {
            Speech.SetNextNode(node);
        };

        return this;
    }

    public SpeechNodeChoice OnDeclined(SpeechNode node)
    {
        Declined += () =>
        {
            Speech.SetNextNode(node);
        };

        return this;
    }
}
