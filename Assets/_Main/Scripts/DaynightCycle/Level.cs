using System;
using UnityEngine;

[Serializable]
public class Level
{
    public const int MAX_TICKS = 3;

    [SerializeField]
    private LevelSegment[] segments;

    public int CurrentSegmentIndex { get; private set; }
    public int CurrentTick { get; private set; }

    public bool CanIncrementPhase()
    {
        return CurrentTick + 1 < MAX_TICKS;
    }

    public void IncrementPhase()
    {
        CurrentTick++;
        if (CurrentTick >= MAX_TICKS)
        {
            CurrentTick = 0;
        }
    }

    public bool CanIncrementSegment()
    {
        return CurrentSegmentIndex < segments.Length;
    }

    public void IncrementSegment()
    {
        CurrentSegmentIndex++;
        if (CurrentSegmentIndex >= segments.Length)
        {
            // TODO: победа?
        }
    }
}