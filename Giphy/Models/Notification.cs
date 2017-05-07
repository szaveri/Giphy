using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gifology.Controls;
using Windows.UI.Xaml.Controls;

namespace Gifology
{
    public static class Notification
    {
        public static NotificationControl CreateNotification(this Grid Grid, string Name, string Text, string Type = "Success", bool Dismissable = true)
        {
            NotificationControl notification = new NotificationControl();

            notification.Name = Name;
            notification.Text = Text;
            notification.Type = Type;
            notification.Dismissable = Dismissable;

            Grid.Children.Add(notification);

            return notification;
        }

        public static void DestroyNotification(this NotificationControl Notification)
        {
            Notification.DestroyNotification(null, null);
        }
    }
}
