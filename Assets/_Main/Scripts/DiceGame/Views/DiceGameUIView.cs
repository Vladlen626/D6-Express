using System;
using PlatformCore.Services.UI;
using UnityEngine;
using UnityEngine.UI;

public class DiceGameUIView : UIBaseElement
{
    [SerializeField]
    private LocalizedText hintLeft;

    [SerializeField]
    private LocalizedText hintRight;

    public void SetLeftHint(string id)
    {
        hintLeft.SetText(id);
    }

    public void SetRightHint(string id)
    {
        hintRight.SetText(id);
    }
}
