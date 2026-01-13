using NUnit.Framework;
using PlatformCore.Core;
using PlatformCore.Services.Audio;
using UnityEngine;

// todo: так тупо делать не надо
public class VoiceUser : MonoBehaviour
{
    [SerializeField]
    private VoiceType type;

    [SerializeField]
    private Transform voiceTfm;

    public void Play()
    {
        Locator.Resolve<IAudioService>().PlaySoundAt(GetVoice(), voiceTfm.position);
    }

    private string GetVoice()
    {
        return GetVoiceType() + Random.Range(0, 3).ToString();
    }

    private string GetVoiceType()
    {
        return type switch
        {
            VoiceType.LOW => "event:/NpcTalk/GibberishLow",
            VoiceType.MEDIUM => "event:/NpcTalk/GibberishMedium",
            VoiceType.HIGH => "event:/NpcTalk/GibberishHigh",
            _ => string.Empty,
        };
    }
}
