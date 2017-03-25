using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Windows.Foundation;
using Windows.Graphics.Display;

namespace Giphy
{
    public class Global
    {
        public static string GIPHY_PUBLIC_KEY = "dc6zaTOxFJmzC";
        public static int limit = 20;
        public static string shareFileName = "blank";
        public static string databaseFile = Path.Combine(Windows.Storage.ApplicationData.Current.RoamingFolder.Path, "giphy.db");
    }
}
