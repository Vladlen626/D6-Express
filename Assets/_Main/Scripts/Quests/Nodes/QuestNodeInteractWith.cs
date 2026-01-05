using System;

public class QuestNodeInteractWith : QuestNode
{
    private readonly Interactor interactor;
    private readonly Type interactableType;

    public QuestNodeInteractWith(Interactor interactor, Type interactableType)
    {
        this.interactor = interactor;
        this.interactableType = interactableType;
    }

    protected override void StartInternal()
    {
        base.StartInternal();

        interactor.InteractionStarted += OnInteractionStarted;
    }

    protected override void FinishInternal()
    {
        interactor.InteractionStarted -= OnInteractionStarted;

        base.FinishInternal();
    }

    private void OnInteractionStarted(InteractionAction action)
    {
        Finish();
    }
}