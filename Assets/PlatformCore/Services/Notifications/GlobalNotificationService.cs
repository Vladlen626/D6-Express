using Cysharp.Threading.Tasks;
using PlatformCore.Services.Factory;
using PlatformCore.Services.Audio;
using PlatformCore.Services.UI;
using PlatformCore.Services;

public class GlobalNotificationService : BaseAsyncService
{
	private readonly IUIService uiService;
	private readonly IObjectFactory objectFactory;
	private readonly ILocalizationService localizationService;
	private readonly IAudioService audioService;
	private readonly string positiveNotificationSound;
	private readonly string negativeNotificationSound;

	private UIGlobalNotificationView bannerView;
	private UINotificationsView notificationsView;

	public GlobalNotificationService(
		IUIService uiService,
		IObjectFactory objectFactory,
		ILocalizationService localizationService,
		IAudioService audioService,
		string positiveNotificationSound,
		string negativeNotificationSound)
	{
		this.uiService = uiService;
		this.objectFactory = objectFactory;
		this.localizationService = localizationService;
		this.audioService = audioService;
		this.positiveNotificationSound = positiveNotificationSound;
		this.negativeNotificationSound = negativeNotificationSound;
	}

	/// <summary>
	/// Shows a localized banner immediately. Duration is controlled by <paramref name="holdSeconds"/>
	/// plus the animation timings inside <c>UIGlobalNotificationView</c>.
	/// </summary>
	public void ShowBanner(string id, float holdSeconds = 0.9f, bool isNegative = false, bool playSound = true)
	{
		ShowBannerAsync(id, holdSeconds, isNegative, playSound).Forget();
	}

	/// <summary>
	/// Shows a localized banner and awaits its completion. Duration is controlled by <paramref name="holdSeconds"/>
	/// plus the animation timings inside <c>UIGlobalNotificationView</c>.
	/// </summary>
	public UniTask ShowBannerAsync(string id, float holdSeconds = 0.9f, bool isNegative = false, bool playSound = true)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			return UniTask.CompletedTask;
		}

		var message = localizationService != null ? localizationService.GetLocalized(id) : id;
		return ShowBannerRawAsync(message, holdSeconds, isNegative, playSound);
	}

	/// <summary>
	/// Shows a formatted localized banner (string.Format) and awaits its completion.
	/// Duration is controlled by <paramref name="holdSeconds"/> plus banner animation timings.
	/// </summary>
	public UniTask ShowBannerAsync(string id, string[] args, float holdSeconds = 0.9f, bool isNegative = false, bool playSound = true)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			return UniTask.CompletedTask;
		}

		var template = localizationService != null ? localizationService.GetLocalized(id) : id;
		var message = args != null && args.Length > 0 ? string.Format(template, args) : template;
		return ShowBannerRawAsync(message, holdSeconds, isNegative, playSound);
	}

	/// <summary>
	/// Shows a raw banner message immediately (no localization).
	/// Duration is controlled by <paramref name="holdSeconds"/> plus banner animation timings.
	/// </summary>
	public void ShowBannerRaw(string message, float holdSeconds = 0.9f, bool isNegative = false, bool playSound = true)
	{
		ShowBannerRawAsync(message, holdSeconds, isNegative, playSound).Forget();
	}

	/// <summary>
	/// Shows a raw banner message (no localization) and awaits its completion.
	/// Duration is controlled by <paramref name="holdSeconds"/> plus banner animation timings.
	/// </summary>
	public async UniTask ShowBannerRawAsync(string message, float holdSeconds = 0.9f, bool isNegative = false, bool playSound = true)
	{
		if (string.IsNullOrWhiteSpace(message))
		{
			return;
		}

		await EnsureBannerViewAsync();
		if (!bannerView)
		{
			return;
		}

		bannerView.Interrupt();
		if (playSound)
		{
			PlayNotificationSound(isNegative);
		}
		await bannerView.PlayAsync(message, holdSeconds, isNegative);
	}

	/// <summary>
	/// Shows a localized toast immediately (parallel with other toasts).
	/// Toast duration is defined by <c>UINotificationView</c> (show delay and animations).
	/// </summary>
	public void EnqueueToast(string id, bool isNegative = false)
	{
		EnqueueToastAsync(id, isNegative).Forget();
	}

	/// <summary>
	/// Shows a localized toast immediately and returns a task that completes when it finishes.
	/// Toast duration is defined by <c>UINotificationView</c> (show delay and animations).
	/// </summary>
	public UniTask EnqueueToastAsync(string id, bool isNegative = false)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			return UniTask.CompletedTask;
		}

		var message = localizationService != null ? localizationService.GetLocalized(id) : id;
		return EnqueueToastRawAsync(message, isNegative);
	}

	/// <summary>
	/// Shows a formatted localized toast immediately (string.Format) and returns a task that completes when it finishes.
	/// Toast duration is defined by <c>UINotificationView</c> (show delay and animations).
	/// </summary>
	public UniTask EnqueueToastAsync(string id, string[] args, bool isNegative = false)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			return UniTask.CompletedTask;
		}

		var template = localizationService != null ? localizationService.GetLocalized(id) : id;
		var message = args != null && args.Length > 0 ? string.Format(template, args) : template;
		return EnqueueToastRawAsync(message, isNegative);
	}

	/// <summary>
	/// Shows a raw toast message immediately (no localization).
	/// Toast duration is defined by <c>UINotificationView</c> (show delay and animations).
	/// </summary>
	public void EnqueueToastRaw(string message, bool isNegative = false)
	{
		EnqueueToastRawAsync(message, isNegative).Forget();
	}

	/// <summary>
	/// Shows a raw toast message immediately (no localization) and returns a task that completes when it finishes.
	/// Toast duration is defined by <c>UINotificationView</c> (show delay and animations).
	/// </summary>
	public UniTask EnqueueToastRawAsync(string message, bool isNegative = false)
	{
		if (string.IsNullOrWhiteSpace(message))
		{
			return UniTask.CompletedTask;
		}

		return ShowToastInternal(message, isNegative);
	}

	/// <summary>
	/// Shows a localized toast immediately (no queue). Multiple calls will display in parallel.
	/// </summary>
	public void ShowToastImmediate(string id, bool isNegative = false)
	{
		ShowToastImmediateAsync(id, isNegative).Forget();
	}

	/// <summary>
	/// Shows a localized toast immediately and returns a task that completes when it finishes.
	/// </summary>
	public UniTask ShowToastImmediateAsync(string id, bool isNegative = false)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			return UniTask.CompletedTask;
		}

		var message = localizationService != null ? localizationService.GetLocalized(id) : id;
		return ShowToastRawImmediateAsync(message, isNegative);
	}

	/// <summary>
	/// Shows a formatted localized toast immediately (no queue).
	/// </summary>
	public UniTask ShowToastImmediateAsync(string id, string[] args, bool isNegative = false)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			return UniTask.CompletedTask;
		}

		var template = localizationService != null ? localizationService.GetLocalized(id) : id;
		var message = args != null && args.Length > 0 ? string.Format(template, args) : template;
		return ShowToastRawImmediateAsync(message, isNegative);
	}

	/// <summary>
	/// Shows a raw toast immediately (no queue). Multiple calls will display in parallel.
	/// </summary>
	public void ShowToastRawImmediate(string message, bool isNegative = false)
	{
		ShowToastRawImmediateAsync(message, isNegative).Forget();
	}

	/// <summary>
	/// Shows a raw toast immediately (no queue) and returns a task that completes when it finishes.
	/// </summary>
	public UniTask ShowToastRawImmediateAsync(string message, bool isNegative = false)
	{
		if (string.IsNullOrWhiteSpace(message))
		{
			return UniTask.CompletedTask;
		}

		return ShowToastInternal(message, isNegative);
	}

	private async UniTask EnsureBannerViewAsync()
	{
		if (bannerView)
		{
			return;
		}

		if (uiService == null)
		{
			return;
		}

		await uiService.PreloadAsync<UIGlobalNotificationView>();
		bannerView = uiService.GetWindow<UIGlobalNotificationView>();
		if (bannerView)
		{
			bannerView.Hide();
		}
	}

	private async UniTask EnsureNotificationsViewAsync()
	{
		if (notificationsView)
		{
			return;
		}

		if (uiService == null)
		{
			return;
		}

		await uiService.PreloadAsync<UINotificationsView>();
		notificationsView = uiService.GetWindow<UINotificationsView>();
		if (notificationsView)
		{
			notificationsView.Show();
		}
	}

	private async UniTask ShowToastInternal(string message, bool isNegative)
	{
		await EnsureNotificationsViewAsync();
		if (!notificationsView || objectFactory == null)
		{
			return;
		}

		var parent = notificationsView.List ? notificationsView.List : notificationsView.transform;
		var view = await objectFactory.CreateAsync<UINotificationView>(
			ResourcePaths.UI.UINotificationView,
			UnityEngine.Vector3.zero,
			UnityEngine.Quaternion.identity,
			parent);

		if (!view)
		{
			return;
		}

		if (parent && view.transform is UnityEngine.RectTransform rect)
		{
			rect.SetParent(parent, false);
		}

		var tcs = new UniTaskCompletionSource();
		void OnShowed(UINotificationView v)
		{
			view.Showed -= OnShowed;
			tcs.TrySetResult();
		}

		view.Showed += OnShowed;
		view.SetText(message, isNegative);
		PlayNotificationSound(isNegative);
		view.Show();

		await tcs.Task;

		UnityEngine.Object.Destroy(view.gameObject);
	}

	private void PlayNotificationSound(bool isNegative)
	{
		if (audioService == null)
		{
			return;
		}

		audioService.PlaySound(isNegative
			? negativeNotificationSound
			: positiveNotificationSound);
	}
}
