using DG.Tweening;
using TMPro;
using UnityEngine;

public class TradeItemSlotView : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro price;

    [Header("Item")]
    [SerializeField]
    private float delay;

    [SerializeField]
    public Transform ItemTfm;

    public float Delay => delay;

    public void SetPrice(string price)
    {
        this.price.text = price;
    }

}
