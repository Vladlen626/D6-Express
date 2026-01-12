using System.Collections.Generic;
using UnityEngine;

public class ShopView : MonoBehaviour
{
    [SerializeField]
    private TradeItemSlotView[] slots;

    public IReadOnlyList<TradeItemSlotView> Slots => slots;
}