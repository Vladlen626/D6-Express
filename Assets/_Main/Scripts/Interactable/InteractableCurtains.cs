using UnityEngine;

[RequireComponent(typeof(HintableDynamic))]
public class InteractableCurtains : Interactable
{
    [SerializeField]
    private bool isOpened;

    [SerializeField]
    private GameObject opened;

    [SerializeField]
    private GameObject closed;

    [SerializeField]
    private HintableDynamic hintableDynamic;

    public override InteractionType Type => isOpened ? InteractionType.CLOSE : InteractionType.OPEN;

    protected override void Awake()
    {
        base.Awake();

        hintableDynamic = GetComponent<HintableDynamic>();

        ApplyState();
    }

    public override void StartInteract(Interactor interactor)
    {
        isOpened = !isOpened;
        ApplyState();
        StopInteract(interactor);
    }

    private void ApplyState()
    {
        opened.SetActive(isOpened);
        closed.SetActive(!isOpened);
        hintableDynamic.SetText(isOpened ? "close" : "open");
    }

    public override void StopInteract(Interactor interactor)
    {
    }
}