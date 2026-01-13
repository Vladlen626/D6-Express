using UnityEngine;

public class SpeechNodePlayVoice : SpeechNode
{
    protected override void StartInternal()
    {
        base.StartInternal();

        var target = Speech.Target;
        target.GetComponent<VoiceUser>().Play();

        Finish();
    }
}