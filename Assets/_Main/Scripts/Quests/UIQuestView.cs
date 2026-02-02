using System.Collections.Generic;
using System.Text;
using PlatformCore.Services.UI;
using Unity.VisualScripting;
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

    public void UpdateObjectives(IEnumerable<QuestObjective> objectives)
    {
        StringBuilder sb = new();
        foreach (var item in objectives)
        {
            if (item.Completed)
            {
                sb.AppendLine($"<color=#{completedColor.ToHexString()}><s>{item.Title}</s></color>");
            }
            else
            {
                sb.AppendLine($"<color=#{activeColor.ToHexString()}>{item.Title}</color>");
            }
        }

        goal.SetRawText(sb.ToString());
    }
}