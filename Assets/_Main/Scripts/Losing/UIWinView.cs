using System;
using PlatformCore.Services.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIWinView : UIBaseElement
{
    [SerializeField]
    private Button exitButton;

    public event Action ExitButtonClicked;

    private void OnEnable()
    {
        exitButton.onClick.AddListener(() => ExitButtonClicked?.Invoke());
    }

    private void OnDisable()
    {
        exitButton.onClick.RemoveAllListeners();
    }
}
