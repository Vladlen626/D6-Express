using System;
using PlatformCore.Services.UI;
using UnityEngine;
using UnityEngine.UI;

public class UILoseView : UIBaseElement
{
    [SerializeField]
    private Button exitButton;

    [SerializeField]
    private Button continueButton;

    public event Action ExitButtonClicked;
    public event Action ContinueButtonClicked;

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