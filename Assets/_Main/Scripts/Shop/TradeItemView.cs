using System;
using UnityEngine;

public class TradeItemView : MonoBehaviour
{
    public event Action<TradeItemView> Buyed;

    public void Buy()
    {
        Buyed?.Invoke(this);
    }
}