using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using PlatformCore.Services.UI;

public class SpeechNodeShowText : SpeechNode
{
    private readonly string text;

    public SpeechNodeShowText(string text)
    {
        this.text = text;
    }

    protected override void StartInternal()
    {
        Locator.Resolve<IUIService>().GetWindow<UISpeechView>().SetSpeech(text);
        Locator.Resolve<IInputService>().OnSpeechLineSkip += Finish;
    }

    protected override void FinishInternal()
    {
        Locator.Resolve<IInputService>().OnSpeechLineSkip -= Finish;
        Locator.Resolve<IUIService>().GetWindow<UISpeechView>().SetSpeech(string.Empty);
    }
}