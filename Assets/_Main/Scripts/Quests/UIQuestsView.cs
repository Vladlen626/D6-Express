using PlatformCore.Services.UI;
using UnityEngine;

public class UIQuestsView : UIBaseElement
{
    [SerializeField]
    private Transform questsContainer;

    public void AddQuest(UIQuestView uIQuestView)
    {
        uIQuestView.transform.SetParent(questsContainer);
    }

    public void RemoveQuest(UIQuestView uIQuestView)
    {
        uIQuestView.transform.SetParent(null);
    }
}