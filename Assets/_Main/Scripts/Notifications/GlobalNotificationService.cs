using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;

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

	public void ShowBanner(string id, float holdSeconds = 0.9f)
	{
		ShowBannerAsync(id, holdSeconds).Forget();
	}

	public UniTask ShowBannerAsync(string id, float holdSeconds = 0.9f)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			return UniTask.CompletedTask;
		}

		var message = localizationService != null ? localizationService.GetLocalized(id) : id;
		return ShowBannerRawAsync(message, holdSeconds);
	}

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

	public void ShowBannerRaw(string message, float holdSeconds = 0.9f)
	{
		ShowBannerRawAsync(message, holdSeconds).Forget();
	}

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

	public void EnqueueToast(string id)
	{
		EnqueueToastAsync(id).Forget();
	}

	public UniTask EnqueueToastAsync(string id)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			return UniTask.CompletedTask;
		}

		var message = localizationService != null ? localizationService.GetLocalized(id) : id;
		return EnqueueToastRawAsync(message);
	}

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

	public void EnqueueToastRaw(string message)
	{
		EnqueueToastRawAsync(message).Forget();
	}

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

		var view = await objectFactory.CreateAsync<UINotificationView>(
			ResourcePaths.UI.UINotificationView,
			UnityEngine.Vector3.zero,
			UnityEngine.Quaternion.identity,
			notificationsView.List);

		if (!view)
		{
			return;
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
