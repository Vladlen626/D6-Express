using PlatformCore.Services.UI;
using UnityEngine;

public class UIQuestView : UIBaseElement
{
    [SerializeField]
    LocalizedText title;

    [SerializeField]
    LocalizedText goal;

    [SerializeField]
    private Color activeColor;

    [SerializeField]
    private Color completedColor;

    public void SetTitleText(string text)
    {
        title.SetRawText(text);
    }

    public void SetGoalText(string text)
    {
        goal.SetRawText(text);
    }

    // todo: подумать мб есть умнее способ
    public void SetMarked(bool marked)
    {
        if (marked)
        {
            goal.Tmp.color = completedColor;
            goal.Tmp.text = $"<s>{goal.Tmp.text}</s>";
        }
        else
        {
            goal.Tmp.color = activeColor;
            goal.Tmp.text = goal.Tmp.text
                .Replace("<s>", "")
                .Replace("</s>", "")
                .Replace("<strikethrough>", "")
                .Replace("</strikethrough>", "");
        }
    }
}