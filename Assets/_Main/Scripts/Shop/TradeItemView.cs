using System;
using UnityEngine;

public class TradeItemView : MonoBehaviour
{
    public int Index { get; private set; }

    public void Init(int index)
    {
        this.Index = index;
    }

    public event Action<TradeItemView> Buyed;

    public void Buy()
    {
        Buyed?.Invoke(this);
    }
}