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

namespace Gifology
{
    public sealed partial class SettingsPage : Page, INotifyPropertyChanged
    {
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

        public SettingsPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }
    }
}
