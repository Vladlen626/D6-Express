using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopView : MonoBehaviour
{
    [SerializeField]
    private TradeItemSlotView[] slots;

    [Header("Restock")]
    [SerializeField]
    private RestockLeverView restockLever;

    [SerializeField]
    private TextMeshPro restockPrice;

    public IReadOnlyList<TradeItemSlotView> Slots => slots;
    public RestockLeverView RestockLever => restockLever;

    public void SetRestockPrice(string price)
    {
        restockPrice.text = price;
    }
}