using PlatformCore.Services.UI;

public static class QuestFactory
{
    public static Quest GenerateRandomQuest(PlayerView playerView)
    {
        var quest = new Quest(0);

        var updateHint = new QuestNodeUpdateHint("пообщайся с кем-нибудь")
        .Init(quest);

        var interactWithSpeakable = new QuestNodeInteractWith(playerView.GetComponent<Interactor>(), typeof(InteractableSpeakable))
        .Init(quest)
        .ExecuteOnFinished(updateHint);

        var updateHint2 = new QuestNodeUpdateHint("ты очень устал, посиди чуть-чуть")
        .Init(quest)
        .ExecuteOnFinished(interactWithSpeakable);

        var interactWithSitable = new QuestNodeInteractWith(playerView.GetComponent<Interactor>(), typeof(InteractableActionSit))
        .Init(quest)
        .ExecuteOnFinished(updateHint2);
        
        quest.SetNextNode(updateHint);

        return quest;
    }

    public static QuestsController GetController(IUIService uIService, Quests quests)
    {
        return new QuestsController(uIService, quests);
    } 
}