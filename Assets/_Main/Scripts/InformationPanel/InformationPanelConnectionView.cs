using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class InformationPanelConnectionView : MonoBehaviour
{
    [SerializeField]
    private int index;

    [SerializeField]
    private List<Image> segments = new();

    private Sequence waveSequence;

    public int Index => index;

    public void SetIndex(int value)
    {
        index = value;
    }

    public void SetSegments(List<Image> value)
    {
        segments = value ?? new List<Image>();
    }

    public void SetColor(Color color)
    {
        if (segments == null)
        {
            return;
        }

        for (int i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            if (!segment)
            {
                continue;
            }

            segment.color = color;
        }
    }

    public void PlayWave(float scale, float duration, float stagger)
    {
        StopWave();

        if (segments == null || segments.Count == 0)
        {
            return;
        }

        if (scale <= 0f || duration <= 0f)
        {
            return;
        }

        var delayStep = Mathf.Max(0f, stagger);
        waveSequence = DOTween.Sequence();

        for (int i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            if (!segment)
            {
                continue;
            }

            var rect = segment.rectTransform;
            rect.localScale = Vector3.one;

            var tween = rect.DOScale(scale, duration * 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(2, LoopType.Yoyo);

            waveSequence.Insert(i * delayStep, tween);
        }

        waveSequence.SetLoops(-1, LoopType.Restart);
    }

    public void StopWave()
    {
        if (waveSequence != null)
        {
            waveSequence.Kill();
            waveSequence = null;
        }

        if (segments == null)
        {
            return;
        }

        for (int i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            if (!segment)
            {
                continue;
            }

            segment.rectTransform.localScale = Vector3.one;
        }
    }

    private void OnDisable()
    {
        StopWave();
    }
}
