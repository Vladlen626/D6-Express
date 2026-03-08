using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using PlatformCore.Services.UI;

// todo: ноды не должны дергать UI напрямую, контроллер должен следить за изменениями в диалоге
public abstract class SpeechNodeShowText : SpeechNode
{
    protected abstract string Text { get; }
    protected virtual bool FinishOnSkip => true; 
    protected override void StartInternal()
    {
        var uISpeechView = Locator.Resolve<IUIService>().GetWindow<UISpeechView>();
        uISpeechView.SetSpeakerName(Speech.Target.GetComponent<CharacterView>().CharacterName);
        uISpeechView.SetSpeech(Text);
        Locator.Resolve<IInputService>().OnSpeechLineSkip += SpeechLineSkipHandler;
    }


    protected override void FinishInternal()
    {
        var uISpeechView = Locator.Resolve<IUIService>().GetWindow<UISpeechView>();

        Locator.Resolve<IInputService>().OnSpeechLineSkip -= SpeechLineSkipHandler;
        uISpeechView.SetSpeakerName(string.Empty);
        uISpeechView.SetSpeech(string.Empty);
    }

    private void SpeechLineSkipHandler()
    {
        var uISpeechView = Locator.Resolve<IUIService>().GetWindow<UISpeechView>();

        if (uISpeechView.IsWriting())
        {
            uISpeechView.SkipWriter();
        }
        else if (FinishOnSkip)
        {
            Finish();
        }
    }
}

public class SpeechNodeShowTextLine : SpeechNodeShowText
{
    private readonly string text;

    protected override string Text => text;

    public SpeechNodeShowTextLine(string text)
    {
        this.text = text;
    }
}

public class SpeechNodeShowTextLineRandom : SpeechNodeShowText
{
    private readonly string[] texts;

    protected override string Text => texts[UnityEngine.Random.Range(0, texts.Length)];

    public SpeechNodeShowTextLineRandom(params string[] texts)
    {
        this.texts = texts;
    }
}