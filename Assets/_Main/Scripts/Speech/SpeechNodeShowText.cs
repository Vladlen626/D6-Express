using _Main.Scripts.Core.Services;
using PlatformCore.Core;
using PlatformCore.Services.UI;
using UnityEngine;

public abstract class SpeechNodeShowText : SpeechNode
{
    protected abstract string Text { get; }

    protected override void StartInternal()
    {
        Locator.Resolve<IUIService>().GetWindow<UISpeechView>().SetSpeech(Text);
        Locator.Resolve<IInputService>().OnSpeechLineSkip += Finish;
    }

    protected override void FinishInternal()
    {
        Locator.Resolve<IInputService>().OnSpeechLineSkip -= Finish;
        Locator.Resolve<IUIService>().GetWindow<UISpeechView>().SetSpeech(string.Empty);
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

    protected override string Text => texts[Random.Range(0, texts.Length)];

    public SpeechNodeShowTextLineRandom(params string[] texts)
    {
        this.texts = texts;
    }
}