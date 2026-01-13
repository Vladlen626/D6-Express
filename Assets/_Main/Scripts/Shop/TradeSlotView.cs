using DG.Tweening;
using TMPro;
using UnityEngine;

public class TradeItemSlotView : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro price;

    [Header("Item")]
    [SerializeField]
    private float rotationDuration = 2f;

    [SerializeField]
    private float amplitude = 1f;

    [SerializeField]
    private float delay;

    [SerializeField]
    public Transform ItemTfm;
    
    public void SetPrice(string price)
    {
        this.price.text = price;
    }

    private void Start()
    {
        ItemTfm
            .DORotate(new Vector3(0f, 360f, 0f), rotationDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1);

        ItemTfm.DOMoveY(ItemTfm.position.y + amplitude, rotationDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetDelay(delay);
    }
}
