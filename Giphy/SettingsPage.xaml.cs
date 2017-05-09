using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Gifology.Database;
using Windows.System;
using Windows.UI.Core;

namespace Gifology
{
    public sealed partial class SettingsPage : Page, INotifyPropertyChanged
    {
        #region Variables
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public List<string> Qualities { get; set; } = new List<string>{
            "Low",
            "Medium",
            "High"
        };

        public List<string> Pages { get; set; } = new List<string>{
            "Search",
            "Trending",
            "MyGifs"
        };

        public int InfiniteScrollEnabled
        {
            get { return SettingsItem.InfiniteScroll; }
            set
            {
                SettingsItem.InfiniteScroll = value;
                GifologyDatabase.InsertUpdateSettings();
                OnPropertyChanged("InfiniteScrollEnabled");
            }
        }

        public string SelectedQuality {
            get { return SettingsItem.GifQuality; }
            set
            {
                SettingsItem.GifQuality = value;
                GifologyDatabase.InsertUpdateSettings();
                OnPropertyChanged("SelectedQuality");
            }
        }

        public string SelectedPageStart
        {
            get { return SettingsItem.StartPage; }
            set
            {
                SettingsItem.StartPage = value;
                GifologyDatabase.InsertUpdateSettings();
                OnPropertyChanged("SelectedPageStart");
            }
        }
        #endregion

        public SettingsPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.IsSupported())
            {
                this.GiveFeedBack.Visibility = Visibility.Visible;
            }
            
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                Frame.CanGoBack ?
                AppViewBackButtonVisibility.Visible :
                AppViewBackButtonVisibility.Collapsed;

            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
        }

        private async void GiveFeedBack_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
            await launcher.LaunchAsync();
        }

        private async void RateAndReview_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(string.Format("ms-windows-store:REVIEW?PFN={0}", Windows.ApplicationModel.Package.Current.Id.FamilyName)));
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
                e.Handled = true;
                SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;
            }
        }
       
    }
}
