using System;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Popups;
using System.Net.NetworkInformation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Giphy
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        /*
         * Runs check for user's internet connection on app load 
         */
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CheckInternetConnection();
        }

        private async void CheckInternetConnection()
        {
            bool isInternetConnected = NetworkInterface.GetIsNetworkAvailable();

            var messageDialog = new MessageDialog("Something went wrong", "Error");
            messageDialog.DefaultCommandIndex = 0;
            messageDialog.CancelCommandIndex = 1;

            //No Internet Connection
            if (!isInternetConnected)
            {
                messageDialog.Title = "Oops!";
                messageDialog.Content = "Please connect to the internet and try again.";
                messageDialog.Commands.Add(new UICommand("Try Again", new UICommandInvokedHandler(this.CommandInvokedHandler)));
                messageDialog.Commands.Add(new UICommand("Close"));
                await messageDialog.ShowAsync();
            }
            else
            {
                var internet = NetworkInformation.GetInternetConnectionProfile();

                //On Metered Connection
                if (internet.IsWwanConnectionProfile)
                {
                    messageDialog.Title = "Whoa There!";
                    messageDialog.Content = "You're currently on a metered connection.";
                    messageDialog.Commands.Add(new UICommand("Continue", new UICommandInvokedHandler(this.CommandInvokedHandler)));
                    messageDialog.Commands.Add(new UICommand("Close"));
                    await messageDialog.ShowAsync();
                }
                else
                {
                    ContentFrame.Navigate(typeof(Search));
                }
            }

        }

        private void CommandInvokedHandler(IUICommand command)
        {
            if (command.Label == "Continue")
            {
                ContentFrame.Navigate(typeof(Search));
                return;
            }
            if (command.Label == "Try Again")
            {
                CheckInternetConnection();
                return;
            }
        }

        private void NavSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchGifs.IsSelected)
            {
                ContentFrame.Navigate(typeof(Search));
            }
            else if (TrendingGifs.IsSelected)
            {
                ContentFrame.Navigate(typeof(Trending));
            }
            else if (FavoriteGifs.IsSelected)
            {
                ContentFrame.Navigate(typeof(Favorites));
            }
            else if (RecentGifs.IsSelected)
            {
                ContentFrame.Navigate(typeof(Recent));
            }
        }

        private void PageSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double width = ((Frame)Window.Current.Content).ActualWidth;
            
            if(width > 1007)
            {
                NavigationPane.IsPaneOpen = true;
                NavigationPane.DisplayMode = SplitViewDisplayMode.Inline;
            }
            else
            {
                NavigationPane.IsPaneOpen = false;
                NavigationPane.DisplayMode = SplitViewDisplayMode.CompactOverlay;
            }
        }
    }
}
