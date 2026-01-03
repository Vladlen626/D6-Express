using System;
using UnityEngine;

public class TradeItem : MonoBehaviour
{
    [SerializeField]
    private int price;

    public int Price => price;

    public event Action<TradeItem, GameObject> Buyed;

    public void Buy(GameObject buyer)
    {
        Buyed?.Invoke(this, buyer);
    }
}