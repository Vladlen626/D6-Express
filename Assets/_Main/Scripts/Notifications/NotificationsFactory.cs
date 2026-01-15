using PlatformCore.Services.Factory;
using PlatformCore.Services.UI;

public static class NotificationsFactory
{
    public static Notifications CreateNotifications()
    {
        return new Notifications();
    }

    public static NotificationsViewControler GetNotificationsViewControler(IUIService uiService, Notifications notifications, IObjectFactory objectFactory)
    {
        return new NotificationsViewControler(uiService, notifications, objectFactory);
    }

    public static NotificationsController GetNotificationsControler(Notifications notifications, InventoryModel inventory, ConfigService configService)
    {
        return new NotificationsController(notifications, inventory, configService);
    }
}
