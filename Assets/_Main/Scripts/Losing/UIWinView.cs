using System;
using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIWinView : UIBaseElement
{

    [SerializeField]
    private TextMeshProUGUI winText;

    [Header("Exit")]
    [SerializeField]
    private Button exitButton;

    [SerializeField]
    private TextMeshProUGUI exitButtonText;

    public event Action ExitButtonClicked;

    public void SetWinText(string text)
    {
        winText.text = text;
    }

    public void SetExitButtonText(string text)
    {
        exitButtonText.text = text;
    }

    private void OnEnable()
    {
        exitButton.onClick.AddListener(() => ExitButtonClicked?.Invoke());
    }

    private void OnDisable()
    {
        exitButton.onClick.RemoveAllListeners();
    }
}
