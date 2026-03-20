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

    [SerializeField]
    private Image imageLose;

    [SerializeField]
    private Image imageWin;

    [SerializeField]
    private Image postcardBackground;

    [SerializeField]
    private Image postStamp;

    [SerializeField]
    public Color colorLose;

    [SerializeField]
    public Color colorWin;

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

    public void SetWinImage(bool enable)
    {
        // imageWin.gameObject.SetActive(enable);
    }

    public void SetLoseImage(bool enable)
    {
        // imageLose.gameObject.SetActive(enable);
    }

    public void SetPostcardColor(Color color)
    {
        postcardBackground.color = color;
    }

    public void SetPoststampColor(Color color)
    {
        postStamp.color = color;
    }
}