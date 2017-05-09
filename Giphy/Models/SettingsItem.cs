using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gifology
{
    public class SettingsItem
    {
        public static int InfiniteScroll { get; set; } = 0;
        public static string GifQuality { get; set; } = "Medium";
        public static string StartPage { get; set; } = "Search";
    }
}
