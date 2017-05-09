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
        public static string settingsFile = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "settings.db");

        public static string CheckInternetConnection()
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
        }

        public static T FindParent<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(dependencyObject);

            if (parent == null) return null;

            var parentT = parent as T;
            return parentT ?? FindParent<T>(parent);
        }
    }    
}
