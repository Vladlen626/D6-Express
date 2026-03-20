using System;
using System.Threading;
using _Main.Scripts.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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

    [SerializeField]
    private TextMeshProUGUI moneyText;

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
    private RectTransform animatedRoot;

    [SerializeField]
    public Color colorLose;

    [SerializeField]
    public Color colorWin;

    public event Action ExitButtonClicked;

    private CancellationTokenSource cashAnimationCts;
    private Tween rootScaleTween;
    private Tween rootShakeTween;
    private Tween rootSettleScaleTween;
    private Tween rootSettlePositionTween;
    private Vector3 animatedRootBaseScale = Vector3.one;
    private Vector2 animatedRootBaseAnchoredPosition = Vector2.zero;

    protected override void OnAwake()
    {
        CacheAnimatedRootBaseline();
    }

    private void OnEnable()
    {
        exitButton.onClick.AddListener(OnExitButtonClicked);
    }

    private void OnDisable()
    {
        exitButton.onClick.RemoveListener(OnExitButtonClicked);
        StopCashAnimation();
    }

    protected override void OnHide()
    {
        StopCashAnimation();
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

    public void PlayWinCashAnimation(int finalCash)
    {
        StopCashAnimation();
        CacheAnimatedRootBaseline();
        int clampedFinalCash = Mathf.Max(0, finalCash);

        if (clampedFinalCash <= 0)
        {
            UIUtils.SetCashText(moneyText, 0);
            return;
        }

        StartWinRootAnimation();

        cashAnimationCts = new CancellationTokenSource();
        AnimateWinCashAsync(clampedFinalCash, cashAnimationCts.Token).Forget();
    }

    public void PlayLoseCashAnimation(int missingCash, int requiredCash)
    {
        StopCashAnimation();
        CacheAnimatedRootBaseline();

        int clampedMissingCash = Mathf.Max(0, missingCash);
        int clampedRequiredCash = Mathf.Max(0, requiredCash);
        if (moneyText)
        {
            moneyText.SetText("$: {0:0}/{1:0}", clampedMissingCash, clampedRequiredCash);
        }

        StartLoseRootAnimation();
    }

    public void StopCashAnimation()
    {
        if (cashAnimationCts != null)
        {
            cashAnimationCts.Cancel();
            cashAnimationCts.Dispose();
            cashAnimationCts = null;
        }

        KillRootTweens();
        ResetAnimatedRootTransform();
    }

    private async UniTask AnimateWinCashAsync(int finalCash, CancellationToken cancellationToken)
    {
        UIUtils.SetCashText(moneyText, 0);

        float targetDurationSeconds = Mathf.Clamp(0.35f + finalCash * 0.0025f, 1.2f, 4f);
        float minDelaySeconds = Mathf.Max(0.001f, targetDurationSeconds / (finalCash * 2f));
        float maxDelaySeconds = Mathf.Max(minDelaySeconds, (targetDurationSeconds * 2f) / finalCash);

        int currentValue = 0;

        try
        {
            while (currentValue < finalCash)
            {
                cancellationToken.ThrowIfCancellationRequested();

                currentValue++;
                UIUtils.SetCashText(moneyText, currentValue);

                float progress = (float)currentValue / finalCash;
                float delaySeconds = Mathf.Lerp(maxDelaySeconds, minDelaySeconds, progress * progress);
                int delayMs = Mathf.Max(1, Mathf.RoundToInt(delaySeconds * 1000f));

                await UniTask.Delay(delayMs, DelayType.UnscaledDeltaTime, PlayerLoopTiming.Update, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }

        FinishWinRootAnimation();
    }

    private void StartWinRootAnimation()
    {
        if (!animatedRoot)
        {
            return;
        }

        ResetAnimatedRootTransform();

        rootScaleTween = animatedRoot
            .DOScale(animatedRootBaseScale * 1.035f, 0.16f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);

        rootShakeTween = animatedRoot
            .DOShakeAnchorPos(0.24f, new Vector2(8f, 4f), 16, 90f, false, true)
            .SetLoops(-1, LoopType.Restart)
            .SetUpdate(true);
    }

    private void StartLoseRootAnimation()
    {
        if (!animatedRoot)
        {
            return;
        }

        ResetAnimatedRootTransform();

        rootScaleTween = animatedRoot
            .DOScale(animatedRootBaseScale * 1.01f, 0.85f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);
    }

    private void FinishWinRootAnimation()
    {
        KillRootTweens();

        if (!animatedRoot)
        {
            return;
        }

        rootSettleScaleTween = animatedRoot
            .DOScale(animatedRootBaseScale, 0.18f)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);

        rootSettlePositionTween = animatedRoot
            .DOAnchorPos(animatedRootBaseAnchoredPosition, 0.18f)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
    }

    private void KillRootTweens()
    {
        if (rootScaleTween != null)
        {
            rootScaleTween.Kill();
            rootScaleTween = null;
        }

        if (rootShakeTween != null)
        {
            rootShakeTween.Kill();
            rootShakeTween = null;
        }

        if (rootSettleScaleTween != null)
        {
            rootSettleScaleTween.Kill();
            rootSettleScaleTween = null;
        }

        if (rootSettlePositionTween != null)
        {
            rootSettlePositionTween.Kill();
            rootSettlePositionTween = null;
        }
    }

    private void CacheAnimatedRootBaseline()
    {
        if (!animatedRoot)
        {
            return;
        }

        animatedRootBaseScale = animatedRoot.localScale;
        animatedRootBaseAnchoredPosition = animatedRoot.anchoredPosition;
    }

    private void ResetAnimatedRootTransform()
    {
        if (!animatedRoot)
        {
            return;
        }

        animatedRoot.localScale = animatedRootBaseScale;
        animatedRoot.anchoredPosition = animatedRootBaseAnchoredPosition;
    }

    private void OnExitButtonClicked()
    {
        ExitButtonClicked?.Invoke();
    }
}
