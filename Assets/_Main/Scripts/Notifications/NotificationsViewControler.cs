using System.Collections.Generic;
using PlatformCore.Core;
using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;
using UnityEngine;

public class NotificationsViewControler : BaseContextController<UINotificationsView>
{
    private readonly Notifications notifications;
    private readonly IObjectFactory objectFactory;

    private readonly List<(Notifications.Notification notification, UINotificationView view)> notificationList = new();

    public NotificationsViewControler(IUIService uiService, Notifications notifications, IObjectFactory objectFactory) : base(uiService)
    {
        this.notifications = notifications;
        this.objectFactory = objectFactory;
    }

    protected override void OnActivate()
    {
        base.OnActivate();

        notifications.Added += OnNotificationAdded;
        notifications.Removed += OnNotificationRemoved;
    }

    protected override void OnDeactivate()
    {
        notifications.Removed -= OnNotificationRemoved;
        notifications.Added -= OnNotificationAdded;

        base.OnDeactivate();
    }

    private async void OnNotificationAdded(Notifications.Notification notification)
    {
        var notificationView = await objectFactory.CreateAsync<UINotificationView>(ResourcePaths.UI.UINotificationView, Vector3.zero, Quaternion.identity, _context.List);
        if (_context.List && notificationView && notificationView.transform is RectTransform rect)
        {
            rect.SetParent(_context.List, false);
        }
        notificationView.Showed += OnShowed;

        notificationList.Add((notification, notificationView));
        notificationView.SetText(notification.message);
        notificationView.Show();
    }

    private void OnNotificationRemoved(Notifications.Notification notification)
    {
        foreach (var item in notificationList)
        {
            if (item.notification == notification)
            {
                item.view.Hide();
                Object.Destroy(item.view.gameObject);
                break;
            }
        }
    }

    private void OnShowed(UINotificationView notificationView)
    {
        foreach (var (notification, view) in notificationList)
        {
            if (view == notificationView)
            {
                notifications.Remove(notification);
                break;
            }
        }
    }
}
