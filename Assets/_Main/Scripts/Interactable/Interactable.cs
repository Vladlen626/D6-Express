using PlatformCore.Core;
using PlatformCore.Services.Audio;
using UnityEngine;

public abstract class Interactable : MonoBehaviour, IInteractable
{
    [SerializeField]
    private bool blockInteract;
    private IAudioService audioService;

    protected virtual void Awake()
    {
        // TODO: не юзать стринг
        gameObject.layer = LayerMask.NameToLayer("Interactable");

        if (!Application.isPlaying)
        {
            return;
        }

        audioService = Locator.Resolve<IAudioService>();
    }

    public abstract InteractionType Type { get; }

    public virtual bool CanInteract(Interactor interactor)
    {
        return !blockInteract;
    }

    public void PlayInteractionSound(string eventPath)
    {
        if (!Application.isPlaying || string.IsNullOrWhiteSpace(eventPath))
        {
            return;
        }

        audioService ??= Locator.Resolve<IAudioService>();
        audioService?.PlaySound(eventPath);
    }

    public abstract void StartInteract(Interactor interactor);
    public abstract void StopInteract(Interactor interactor);
}
