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

    [Header("Continue")]
    [SerializeField]
    private Button continueButton;

    [SerializeField]
    private TextMeshProUGUI continueButtonText;


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

    public void SetContinueButtonText(string text)
    {
        continueButtonText.text = text;
    }

    private void OnEnable()
    {
        exitButton.onClick.AddListener(() => ExitButtonClicked?.Invoke());
        continueButton.onClick.AddListener(() => ContinueButtonClicked?.Invoke());
    }

    private void OnDisable()
    {
        exitButton.onClick.RemoveAllListeners();
        continueButton.onClick.RemoveAllListeners();
    }
}
