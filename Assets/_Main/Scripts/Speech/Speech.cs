using System;
using System.Collections.Generic;
using UnityEngine;

public class Speech
{
    private readonly Dictionary<string, object> blackboard = new();

    private SpeechNode root;
    private SpeechNode current;
    private SpeechNode next;

    public int Id { get; private set; }

    public event Action Started;
    public event Action Finished;

    public event Action<SpeechNode> NodeStarted;
    public event Action<SpeechNode> NodeFinished;

    public GameObject Speaker => Blackboard[SpeechBlackboardBaseKeys.USER] as GameObject;
    public GameObject Target => Blackboard[SpeechBlackboardBaseKeys.TARGET] as GameObject;

    public Dictionary<string, object> Blackboard => blackboard;

    public Speech(int id)
    {
        this.Id = id;
    }

    public void SetRootNode(SpeechNode node)
    {
        this.root = node;
    }

    public void RequestStart()
    {
        Started?.Invoke();

        SetNextNode(root);
        ProcessNextNode();
    }

    public void RequestFinish()
    {
        current.Finish();
    }

    public void SetNextNode(SpeechNode next)
    {
        this.next = next;
    }

    private void ProcessNextNode()
    {
        if (current != null)
        {
            current.Started -= OnNodeStarted;
            current.Finished -= OnNodeFinished;

            current = null;
        }

        if (next == null)
        {
            blackboard.Clear();

            Finished?.Invoke();
            return;
        }

        current = next;
        next = null;
        current.Started += OnNodeStarted;
        current.Finished += OnNodeFinished;

        current.Start();
    }

    private void OnNodeStarted()
    {
        NodeStarted?.Invoke(current);
    }

    private void OnNodeFinished()
    {
        NodeFinished?.Invoke(current);
        ProcessNextNode();
    }
}