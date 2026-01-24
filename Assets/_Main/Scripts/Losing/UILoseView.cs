using System;
using PlatformCore.Services.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILoseView : UIBaseElement
{
    [SerializeField]
    private TextMeshProUGUI loseText;

    [Header("Exit")]
    [SerializeField]
    private Button exitButton;

    [SerializeField]
    private TextMeshProUGUI exitButtonText;

    public event Action ExitButtonClicked;
    public event Action ContinueButtonClicked;

    public void SetLoseText(string text)
    {
        loseText.text = text;
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
