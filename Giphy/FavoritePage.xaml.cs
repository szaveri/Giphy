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
using Giphy.Database;
using SQLite;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Giphy
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FavoritesPage : Page
    {

        private static List<Giphy.Database.Favorites> FavoriteList = new List<Giphy.Database.Favorites>();

        public FavoritesPage()
        {
            this.InitializeComponent();
            GiphyImage.RegisterForShare();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.ColumnOne.Children.Clear();
            this.ColumnTwo.Children.Clear();
            GetFavorites();
        }

        /*
         * Get new set from Favorites database
         */
        private void GetFavorites()
        {
            using (var conn = new SQLiteConnection(Global.databaseFile))
            {
                FavoriteList = GiphyDatabase.GetFavorites(conn);
            }
                
            DrawList(FavoriteList);
        }

        private void DrawList(List<Giphy.Database.Favorites> list)
        {
            this.ProgressBar.Visibility = Visibility.Visible;

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
            this.ProgressBar.Visibility = Visibility.Collapsed;
        }
    }
}
