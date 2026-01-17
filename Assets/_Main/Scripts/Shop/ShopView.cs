using System.Collections.Generic;
using UnityEngine;

public class ShopView : MonoBehaviour
{
    [SerializeField]
    private TradeItemSlotView[] slots;

    [SerializeField]
    private RestockLeverView lever;

    public IReadOnlyList<TradeItemSlotView> Slots => slots;
    public RestockLeverView Lever => lever;
}