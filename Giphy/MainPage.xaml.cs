﻿using System;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Popups;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using Windows.UI.Xaml.Input;
using System.Diagnostics;
using Windows.UI.Xaml.Media;
using Windows.UI;

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
                    ContentFrame.Navigate(typeof(SearchPage));
                }
            }

        }

        private void CommandInvokedHandler(IUICommand command)
        {
            if (command.Label == "Continue")
            {
                ContentFrame.Navigate(typeof(SearchPage));
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
                PivotNavigation.SelectedIndex = 0;
            }
            else if (TrendingGifs.IsSelected)
            {
                PivotNavigation.SelectedIndex = 1;
            }
            else if (FavoriteGifs.IsSelected)
            {
                PivotNavigation.SelectedIndex = 2;
            }
            else if (RecentGifs.IsSelected)
            {
                PivotNavigation.SelectedIndex = 3;
            }
        }

        private void Pivot_NavSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (PivotNavigation.SelectedIndex)
            {
                case 0:
                    ContentFrame.Navigate(typeof(SearchPage));
                    break;
                case 1:
                    ContentFrame.Navigate(typeof(TrendingPage));
                    break;
                case 2:
                    ContentFrame.Navigate(typeof(FavoritesPage));
                    break;
                case 3:
                    ContentFrame.Navigate(typeof(RecentPage));
                    break;
                default:
                    ContentFrame.Navigate(typeof(SearchPage));
                    break;
            }
        }
    }
}
