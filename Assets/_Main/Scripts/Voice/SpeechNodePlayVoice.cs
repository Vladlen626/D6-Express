using UnityEngine;

public class SpeechNodePlayVoice : SpeechNode
{
    protected override void StartInternal()
    {
        base.StartInternal();

        var target = Speech.Blackboard[SpeechBlackboardBaseKeys.TARGET] as GameObject;
        target.GetComponent<VoiceUser>().Play();

        Finish();
    }
}