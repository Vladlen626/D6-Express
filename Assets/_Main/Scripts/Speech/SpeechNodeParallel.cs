using System.Collections.Generic;


public class SpeechNodeParallel : SpeechNode
{
    private readonly List<SpeechNode> nodes = new();

    private int completedCount;

    public void Add(params SpeechNode[] nodes)
    {
        this.nodes.AddRange(nodes);
    }

    protected override void StartInternal()
    {
        base.StartInternal();

        foreach (var item in nodes)
        {
            item.Finished += OnFinished;
        }

        foreach (var item in nodes)
        {
            item.Start();
        }
    }

    private void OnFinished(SpeechNode speechNode)
    {
        completedCount++;

        if (completedCount == nodes.Count)
        {
            foreach (var item in nodes)
            {
                item.Finished -= OnFinished;
            }
            completedCount = 0;

            Finish();
        }
    }
}