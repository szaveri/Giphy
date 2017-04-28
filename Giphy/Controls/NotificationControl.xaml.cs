using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Gifology.Controls
{
    public sealed partial class NotificationControl : UserControl
    {
        private static NotificationControl _this;
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(NotificationControl), null);

        public string Name
        {
            get { return GetValue(NameProperty) as string; }
            set { SetValue(NameProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(NotificationControl), null);

        public string Text
        {
            get { return GetValue(TextProperty) as string; }
            set { _this.NotificationText.Text = value;  }
        }

        public static readonly DependencyProperty DismissableProperty =
             DependencyProperty.Register("Dismissable", typeof(bool?), typeof(NotificationControl), null);

        public bool? Dismissable
        {
            get { return GetValue(DismissableProperty) as bool?; }
            set { SetValue(DismissableProperty, value); }
        }

        public static readonly DependencyProperty TypeProperty =
             DependencyProperty.Register("Type", typeof(string), typeof(NotificationControl), null);

        public string Type
        {
            get { return GetValue(DismissableProperty) as string; }
            set
            {
                if (value == "Notification")
                    _this.NotificationMessageGrid.BorderBrush = new SolidColorBrush(Color.FromArgb(100, 185, 185, 104));
                else if (value == "Error")
                    _this.NotificationMessageGrid.BorderBrush = new SolidColorBrush(Color.FromArgb(100, 162, 54, 54));
                else
                    _this.NotificationMessageGrid.BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 110, 10));
            }
        }

        public NotificationControl()
        {
            this.InitializeComponent();
            _this = this;
        }

        public void ShowNotification()
        {
            _this.NotificationShow.Begin();
        }

        public void HideNotification(bool destroy = false)
        {
            _this.NotificationHide.Begin();
            if (destroy)
                _this.NotificationShowHide.Completed += (s, e) =>
                {
                    _this.DestroyNotification(null, null);
                };
        }

        public void ShowHideNotification(bool destroy = false)
        {
            _this.NotificationShowHide.Begin();
            if (destroy)
                _this.NotificationShowHide.Completed += (s, e) =>
                {
                    _this.DestroyNotification(null, null);
                };
        }

        public void DestroyNotification(object sender, RoutedEventArgs e)
        {
            if(_this.NotificationMessageGrid.Height > 0)
            {
                _this.NotificationHide.Begin();
                _this.NotificationHide.Completed += (s, args) =>
                {
                    ((Grid)_this.Parent).Children.Remove(_this);
                };
            }
            else
            {
                ((Grid)_this.Parent).Children.Remove(_this);
            }
           
        }
    }
}
