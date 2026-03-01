using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;
using PlatformCore.Services;

public class GlobalNotificationService : BaseAsyncService
{
	private readonly IUIService uiService;
	private readonly IObjectFactory objectFactory;
	private readonly ILocalizationService localizationService;

	private UIGlobalNotificationView bannerView;
	private UINotificationsView notificationsView;

	private readonly Queue<ToastRequest> toastQueue = new();
	private bool isToastPlaying;

	private class ToastRequest
	{
		public string Message;
		public UniTaskCompletionSource Completion;
	}

	public GlobalNotificationService(IUIService uiService, IObjectFactory objectFactory, ILocalizationService localizationService)
	{
		this.uiService = uiService;
		this.objectFactory = objectFactory;
		this.localizationService = localizationService;
	}

	/// <summary>
	/// Shows a localized banner immediately. Duration is controlled by <paramref name="holdSeconds"/>
	/// plus the animation timings inside <c>UIGlobalNotificationView</c>.
	/// </summary>
	public void ShowBanner(string id, float holdSeconds = 0.9f)
	{
		ShowBannerAsync(id, holdSeconds).Forget();
	}

	/// <summary>
	/// Shows a localized banner and awaits its completion. Duration is controlled by <paramref name="holdSeconds"/>
	/// plus the animation timings inside <c>UIGlobalNotificationView</c>.
	/// </summary>
	public UniTask ShowBannerAsync(string id, float holdSeconds = 0.9f)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			return UniTask.CompletedTask;
		}

		var message = localizationService != null ? localizationService.GetLocalized(id) : id;
		return ShowBannerRawAsync(message, holdSeconds);
	}

	/// <summary>
	/// Shows a formatted localized banner (string.Format) and awaits its completion.
	/// Duration is controlled by <paramref name="holdSeconds"/> plus banner animation timings.
	/// </summary>
	public UniTask ShowBannerAsync(string id, string[] args, float holdSeconds = 0.9f)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			return UniTask.CompletedTask;
		}

		var template = localizationService != null ? localizationService.GetLocalized(id) : id;
		var message = args != null && args.Length > 0 ? string.Format(template, args) : template;
		return ShowBannerRawAsync(message, holdSeconds);
	}

	/// <summary>
	/// Shows a raw banner message immediately (no localization).
	/// Duration is controlled by <paramref name="holdSeconds"/> plus banner animation timings.
	/// </summary>
	public void ShowBannerRaw(string message, float holdSeconds = 0.9f)
	{
		ShowBannerRawAsync(message, holdSeconds).Forget();
	}

	/// <summary>
	/// Shows a raw banner message (no localization) and awaits its completion.
	/// Duration is controlled by <paramref name="holdSeconds"/> plus banner animation timings.
	/// </summary>
	public async UniTask ShowBannerRawAsync(string message, float holdSeconds = 0.9f)
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
		await bannerView.PlayAsync(message, holdSeconds);
	}

	/// <summary>
	/// Enqueues a localized toast for sequential display (queue).
	/// Toast duration is defined by <c>UINotificationView</c> (show delay and animations).
	/// </summary>
	public void EnqueueToast(string id)
	{
		EnqueueToastAsync(id).Forget();
	}

	/// <summary>
	/// Enqueues a localized toast and returns a task that completes when the toast finishes.
	/// Toast duration is defined by <c>UINotificationView</c> (show delay and animations).
	/// </summary>
	public UniTask EnqueueToastAsync(string id)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			return UniTask.CompletedTask;
		}

		var message = localizationService != null ? localizationService.GetLocalized(id) : id;
		return EnqueueToastRawAsync(message);
	}

	/// <summary>
	/// Enqueues a formatted localized toast (string.Format) and returns a task that completes when it finishes.
	/// Toast duration is defined by <c>UINotificationView</c> (show delay and animations).
	/// </summary>
	public UniTask EnqueueToastAsync(string id, string[] args)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			return UniTask.CompletedTask;
		}

		var template = localizationService != null ? localizationService.GetLocalized(id) : id;
		var message = args != null && args.Length > 0 ? string.Format(template, args) : template;
		return EnqueueToastRawAsync(message);
	}

	/// <summary>
	/// Enqueues a raw toast message immediately (no localization).
	/// Toast duration is defined by <c>UINotificationView</c> (show delay and animations).
	/// </summary>
	public void EnqueueToastRaw(string message)
	{
		EnqueueToastRawAsync(message).Forget();
	}

	/// <summary>
	/// Enqueues a raw toast message (no localization) and returns a task that completes when it finishes.
	/// Toast duration is defined by <c>UINotificationView</c> (show delay and animations).
	/// </summary>
	public UniTask EnqueueToastRawAsync(string message)
	{
		if (string.IsNullOrWhiteSpace(message))
		{
			return UniTask.CompletedTask;
		}

		var request = new ToastRequest
		{
			Message = message,
			Completion = new UniTaskCompletionSource()
		};

		toastQueue.Enqueue(request);
		if (!isToastPlaying)
		{
			PlayToastQueue().Forget();
		}

		return request.Completion.Task;
	}

	/// <summary>
	/// Shows a localized toast immediately (no queue). Multiple calls will display in parallel.
	/// </summary>
	public void ShowToastImmediate(string id)
	{
		ShowToastImmediateAsync(id).Forget();
	}

	/// <summary>
	/// Shows a localized toast immediately and returns a task that completes when it finishes.
	/// </summary>
	public UniTask ShowToastImmediateAsync(string id)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			return UniTask.CompletedTask;
		}

		var message = localizationService != null ? localizationService.GetLocalized(id) : id;
		return ShowToastRawImmediateAsync(message);
	}

	/// <summary>
	/// Shows a formatted localized toast immediately (no queue).
	/// </summary>
	public UniTask ShowToastImmediateAsync(string id, string[] args)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			return UniTask.CompletedTask;
		}

		var template = localizationService != null ? localizationService.GetLocalized(id) : id;
		var message = args != null && args.Length > 0 ? string.Format(template, args) : template;
		return ShowToastRawImmediateAsync(message);
	}

	/// <summary>
	/// Shows a raw toast immediately (no queue). Multiple calls will display in parallel.
	/// </summary>
	public void ShowToastRawImmediate(string message)
	{
		ShowToastRawImmediateAsync(message).Forget();
	}

	/// <summary>
	/// Shows a raw toast immediately (no queue) and returns a task that completes when it finishes.
	/// </summary>
	public UniTask ShowToastRawImmediateAsync(string message)
	{
		if (string.IsNullOrWhiteSpace(message))
		{
			return UniTask.CompletedTask;
		}

		return ShowToastInternal(message);
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

	private async UniTaskVoid PlayToastQueue()
	{
		isToastPlaying = true;

		while (toastQueue.Count > 0)
		{
			var request = toastQueue.Dequeue();
			await ShowToastInternal(request.Message);
			request.Completion.TrySetResult();
		}

		isToastPlaying = false;
	}

	private async UniTask ShowToastInternal(string message)
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
		view.SetText(message);
		view.Show();

		await tcs.Task;

		UnityEngine.Object.Destroy(view.gameObject);
	}
}
