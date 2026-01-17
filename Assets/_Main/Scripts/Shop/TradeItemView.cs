using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(AnimatorSignalBridge))]
public class TradeItemView : MonoBehaviour
{
    [SerializeField]
    private float rotationDuration = 2f;

    [SerializeField]
    private float amplitude = 1f;

    [SerializeField]
    private float transitionDuration = 1f;

    [SerializeField]
    private Ease ease = Ease.OutBack;

    private Animator animator;

    private AnimatorSignalBridge bridge;

    public int Index { get; private set; }

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

    public async Task ShowAsync()
    {
        animator.SetBool("Active", true);

        await bridge.EnterFinished.Task;
    }

    public UniTask Showing()
    {
        return bridge.EnterFinished.Task;
    }

    public UniTask Hiding()
    {
        return bridge.ExitFinished.Task;
    }

    public async Task HideAsync()
    {
        animator.SetBool("Active", false);

        await bridge.ExitFinished.Task;
    }
}