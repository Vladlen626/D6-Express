using System;
using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIEndView : UIBaseElement
{
    [SerializeField]
    private TextMeshProUGUI titleText;

    [SerializeField]
    private TextMeshProUGUI messageText;

    [Header("Exit")]
    [SerializeField]
    private Button exitButton;

    [SerializeField]
    private TextMeshProUGUI exitButtonText;

    public event Action ExitButtonClicked;

    private void OnEnable()
    {
        exitButton.onClick.AddListener(() =>
        {
            ExitButtonClicked?.Invoke();
        });
    }

    private void OnDisable()
    {
        exitButton.onClick.RemoveAllListeners();
    }

    public void SetTitle(string text)
    {
        titleText.text = text;
    }

    public void SetMessage(string text)
    {
        messageText.text = text;
    }

    public void SetExitButtonText(string text)
    {
        exitButtonText.text = text;
    }
}