using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Networking.Connectivity;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Gifology
{
    public class Global
    {
        public static string GIPHY_PUBLIC_KEY = "dc6zaTOxFJmzC";
        public static int limit = 20;
        public static string shareFileName = "blank";
        public static string databaseFile = Path.Combine(Windows.Storage.ApplicationData.Current.RoamingFolder.Path, "giphy.db");

        public static async Task<string> CheckInternetConnection()
        {
            bool isInternetConnected = NetworkInterface.GetIsNetworkAvailable();

            if (!isInternetConnected)
            {
                return "None";
            }
            else
            {
                var internet = NetworkInformation.GetInternetConnectionProfile();
                if (internet.IsWwanConnectionProfile) return "Metered";
                else return "Wifi";
            }
            //var messageDialog = new MessageDialog("Something went wrong", "Error");
            //messageDialog.DefaultCommandIndex = 0;
            //messageDialog.CancelCommandIndex = 1;

            ////No Internet Connection
            //if (!isInternetConnected)
            //{
            //    messageDialog.Title = "Oops!";
            //    messageDialog.Content = "Please connect to the internet and try again.";
            //    messageDialog.Commands.Add(new UICommand("Try Again"));
            //    messageDialog.Commands.Add(new UICommand("Close"));
            //    var response = await messageDialog.ShowAsync();
            //    return response.Label;
            //}
            //else
            //{
            //    var internet = NetworkInformation.GetInternetConnectionProfile();

            //    //On Metered Connection
            //    if (internet.IsWwanConnectionProfile)
            //    {
            //        messageDialog.Title = "Whoa There!";
            //        messageDialog.Content = "You're currently on a metered connection.";
            //        messageDialog.Commands.Add(new UICommand("Continue"));
            //        messageDialog.Commands.Add(new UICommand("Close"));
            //        var response = await messageDialog.ShowAsync();
            //        return response.Label;
            //    }
            //    else
            //    {
            //        return "Continue";
            //    }
            //}
        }
    }    
}
