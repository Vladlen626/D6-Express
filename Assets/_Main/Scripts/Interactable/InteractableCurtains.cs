using UnityEngine;

[RequireComponent(typeof(HintableDynamic))]
[ExecuteInEditMode]
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

    private void OnValidate()
    {
        ApplyState();
    }

    public override void StartInteract(Interactor interactor)
    {
        if (interactor is InteractorPlayer)
        {
            PlayInteractionSound(SoundNames.CurtainsInteract);
        }

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
