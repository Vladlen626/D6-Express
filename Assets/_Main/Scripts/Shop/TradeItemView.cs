using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(AnimatorSignalBridge))]
public class TradeItemView : MonoBehaviour
{
    private Animator animator;
    private AnimatorSignalBridge bridge;
    private UniTaskCompletionSource currentTransition;
    
    public int Index { get; private set; }
    public bool IsTransitioning => currentTransition != null;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        bridge = GetComponent<AnimatorSignalBridge>();
        bridge.Reset();
    }
    
    public void Init(int index)
    {
        this.Index = index;
    }
    
    public event Action<TradeItemView> Buyed;
    
    public void Buy()
    {
        Buyed?.Invoke(this);
    }
    
    public async UniTask ShowAsync()
    {
        if (currentTransition != null)
            await currentTransition.Task;
        
        currentTransition = new UniTaskCompletionSource();
        
        try
        {
            animator.SetBool("Active", true);
            await bridge.EnterFinished.Task;
        }
        finally
        {
            currentTransition.TrySetResult();
            currentTransition = null;
        }
    }
    
    public async UniTask HideAsync()
    {
        if (currentTransition != null)
            await currentTransition.Task;
        
        currentTransition = new UniTaskCompletionSource();
        
        try
        {
            animator.SetBool("Active", false);
            await bridge.ExitFinished.Task;
        }
        finally
        {
            currentTransition.TrySetResult();
            currentTransition = null;
        }
    }
    
    public UniTask WaitForTransition()
    {
        return currentTransition?.Task ?? UniTask.CompletedTask;
    }
}
