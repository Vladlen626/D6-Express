using System;
using System.Collections.Generic;

public class Notifications
{
    public event Action<Notification> Added;
    public event Action<Notification> Removed;

    private readonly List<Notification> notifications = new();

    public IReadOnlyList<Notification> All => notifications;

    public void Add(Notification notification)
    {
        notifications.Add(notification);
        Added?.Invoke(notification);
    }

    public void Remove(Notification notification)
    {
        notifications.Remove(notification);
        Removed?.Invoke(notification);
    }

    public class Notification
    {
        public string message;
    }
}
