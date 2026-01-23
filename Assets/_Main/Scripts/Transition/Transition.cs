using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;


public class Transition
{
    public readonly Data data;

    private readonly List<Func<UniTask>> tasks = new();
    private Func<UniTask> first;
    private Func<UniTask> last;

    public Transition(Data transitionData)
    {
        this.data = transitionData;
    }

    public void SetFirstTask(Func<UniTask> first)
    {
        this.first = first;
    }

    public void SetLastTask(Func<UniTask> last)
    {
        this.last = last;
    }

    public void AddTask(Func<UniTask> task)
    {
        tasks.Add(task);
    }

    public void AddTasks(IEnumerable<Func<UniTask>> tasks)
    {
        this.tasks.AddRange(tasks);
    }

    public async Task Start()
    {
        if (first != null)
        {
            await first.Invoke();
        }
        Debug.Log("TRANSITION: first task completed");

        for (int i = 0; i < tasks.Count; i++)
        {
            Func<UniTask> item = tasks[i];
            await item();
            Debug.Log($"TRANSITION: task {i} completed");
        }

        if (last != null)
        {
            await last.Invoke();
        }
        Debug.Log("TRANSITION: last task completed");
    }

    public struct Data
    {
        public TaskType[] tasks;
    }

    public enum TaskType
    {
        CHANGE_LOCATION,
        WAKE_UP,
        WIN,
        LOSE,
        NPC_RESPAWN,
        SHOP_RESTOCK,
    }
}
