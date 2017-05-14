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
            set { this.NotificationText.Text = value;  }
        }

        public static readonly DependencyProperty DismissableProperty =
             DependencyProperty.Register("Dismissable", typeof(bool?), typeof(NotificationControl), null);

        public bool? Dismissable
        {
            get { return GetValue(DismissableProperty) as bool?; }
            set {
                if (value != null && value == true) NotificationDismissButton.Visibility = Visibility.Visible;
                else NotificationDismissButton.Visibility = Visibility.Collapsed;
            }
        }

        public static readonly DependencyProperty TypeProperty =
             DependencyProperty.Register("Type", typeof(string), typeof(NotificationControl), null);

        public string Type
        {
            get { return GetValue(TypeProperty) as string; }
            set
            {
                if (value == "Success")
                    this.NotificationMessageGrid.BorderBrush = new SolidColorBrush(Color.FromArgb(100, 51, 204, 0));
                else if (value == "Error")
                    this.NotificationMessageGrid.BorderBrush = new SolidColorBrush(Color.FromArgb(100, 162, 54, 54));
                else if (value == "Warning")
                    this.NotificationMessageGrid.BorderBrush = new SolidColorBrush(Color.FromArgb(100, 185, 185, 104));
                else
                    this.NotificationMessageGrid.BorderBrush = this.NotificationMessageGrid.Background;
            }
        }

        public static readonly DependencyProperty ParentTypeProperty =
             DependencyProperty.Register("ParentType", typeof(string), typeof(NotificationControl), null);

        public string ParentType
        {
            get { return GetValue(ParentTypeProperty) as string; }
            set { SetValue(ParentTypeProperty, value); }
            }


        public NotificationControl()
        {
            this.InitializeComponent();
        }

        public void ShowNotification()
        {
            this.NotificationShow.Begin();
        }

        public void HideNotification(bool destroy = false)
        {
            this.NotificationHide.Begin();
            if (destroy)
                this.NotificationShowHide.Completed += (s, e) =>
                {
                    this.DestroyNotification(null, null);
                };
        }

        public void ShowHideNotification(bool destroy = false)
        {
            this.NotificationShowHide.Begin();
            if (destroy)
                this.NotificationShowHide.Completed += (s, e) =>
                {
                    this.DestroyNotification(null, null);
                };
        }

        public void DestroyNotification(object sender, RoutedEventArgs e)
        {
            if(this.NotificationMessageGrid.Height > 0)
            {
                this.NotificationHide.Begin();
                this.NotificationHide.Completed += (s, args) =>
                {
                    if(this.ParentType == "StackPanel")
                        ((StackPanel)this.Parent).Children.Remove(this);
                    else
                        ((Grid)this.Parent).Children.Remove(this);
                };
            }
            else
            {
                if (this.ParentType == "StackPanel")
                    ((StackPanel)this.Parent).Children.Remove(this);
                else
                    ((Grid)this.Parent).Children.Remove(this);
            }
           
        }

        public double GetHeight()
        {
            return this.NotificationMessageGrid.Height;
        }
    }
}
