using System;
using System.Collections.Generic;
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
using SQLite;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Gifology
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MyGifsPage : Page
    {
        private int Offset = 0;
        private int PreviousOffset = 0;
        private string CurrentView = "Favorite";

        public MyGifsPage()
        {
            this.InitializeComponent();
            GiphyImage.RegisterForShare();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            switch (await Global.CheckInternetConnection())
            {
                case "Continue":
                    ViewBox.SelectedValue = "Favorite";
                    break;
                case "Try Again":
                    Page_Loaded(sender, e);
                    break;
                case "Close":
                    break;
            }
        }

        /* ==================================
         * START FAVORITE FUNCTIONS
         * ==================================
         */
        private async void GetFavorites()
        {
            this.ProgressBar.Visibility = Visibility.Visible;

            PreviousOffset = Offset;
            DrawList(await GifologyDatabase.GetFavorites(Offset));
            Offset += Global.limit;
            this.PreviousAppButton.IsEnabled = this.PreviousButton.IsEnabled = Offset - Global.limit > 0;

            this.ProgressBar.Visibility = Visibility.Collapsed;
        }

        private void DrawList(List<Gifology.Database.Favorites> list)
        {
            this.NextAppButton.IsEnabled = this.NextButton.IsEnabled = list.Count > Global.limit;
            for (int i = 0; i < list.Count; i++)
            {
                Image img = new Image();
                img.Name = list[i].Giphy_Id;
                img.Source = new BitmapImage(new Uri(list[i].Url, UriKind.Absolute));
                img.Margin = new Thickness(0, 0, 10, 10);
                img.Stretch = Stretch.UniformToFill;
                img.MaxWidth = 400;
                img.Tapped += (sender, e) => { GiphyImage.ShowContextMenu(sender, e, img); };
                img.RightTapped += (sender, e) => { GiphyImage.ShowContextMenu(sender, e, img); };

                if (i % 2 == 0)
                    this.ColumnOne.Children.Add(img);
                else
                    this.ColumnTwo.Children.Add(img);
            }
            
        }

        /* ==================================
         * START RECENT FUNCTIONS
         * ==================================
         */
        private async void GetRecents()
        {
            this.ProgressBar.Visibility = Visibility.Visible;

            PreviousOffset = Offset;
            DrawList(await GifologyDatabase.GetRecents(Offset));
            Offset += Global.limit;
            this.PreviousAppButton.IsEnabled = this.PreviousButton.IsEnabled = Offset - Global.limit > 0;

            this.ProgressBar.Visibility = Visibility.Collapsed;
        }

        private void DrawList(List<Gifology.Database.Recents> list)
        {
            this.NextAppButton.IsEnabled = this.NextButton.IsEnabled = list.Count > Global.limit;
            for (int i = 0; i < list.Count; i++)
            {
                Image img = new Image();
                img.Name = list[i].Giphy_Id;
                img.Source = new BitmapImage(new Uri(list[i].Url, UriKind.Absolute));
                img.Margin = new Thickness(0, 0, 10, 10);
                img.Stretch = Stretch.UniformToFill;
                img.MaxWidth = 400;
                img.Tapped += (sender, e) => { GiphyImage.ShowContextMenu(sender, e, img); };
                img.RightTapped += (sender, e) => { GiphyImage.ShowContextMenu(sender, e, img); };

                if (i % 2 == 0)
                    this.ColumnOne.Children.Add(img);
                else
                    this.ColumnTwo.Children.Add(img);
            }
        }

        private void ViewBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ColumnOne.Children.Clear();
            this.ColumnTwo.Children.Clear();
            var selected = ((ComboBoxItem)ViewBox.SelectedItem).Tag.ToString();
            Offset = 0;
            CurrentView = selected;

            switch (selected)
            {
                case "Favorite":
                    GetFavorites();
                    break;
                case "Recent":
                    GetRecents();
                    break;
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            this.ColumnOne.Children.Clear();
            this.ColumnTwo.Children.Clear();
            Offset = PreviousOffset;
            if (Offset - Global.limit >= 0) Offset -= Global.limit;
            else Offset -= Offset;

            if (CurrentView == "Favorite")
                GetFavorites();
            else if (CurrentView == "Recent")
                GetRecents();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            this.ColumnOne.Children.Clear();
            this.ColumnTwo.Children.Clear();
            if (CurrentView == "Favorite")
                GetFavorites();
            else if (CurrentView == "Recent")
                GetRecents();
        }
    }
}
