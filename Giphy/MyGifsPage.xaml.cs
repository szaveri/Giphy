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
using System.Collections.ObjectModel;
using System.Diagnostics;

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
        private ObservableCollection<GiphyImage> ColumnOneList = new ObservableCollection<GiphyImage>();
        private ObservableCollection<GiphyImage> ColumnTwoList = new ObservableCollection<GiphyImage>();

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
                    ImageListControl.NextButton_Clicked += new RoutedEventHandler(NextButton_Click);
                    ImageListControl.PrevButton_Clicked += new RoutedEventHandler(PreviousButton_Click);
                    ImageListControl.ShowSingleImageIcons += ShowSingleImageIcons;
                    ImageListControl.ShowFullListIcons += ShowFullListIcons;
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
            this.PreviousAppButton.IsEnabled = Offset - Global.limit > 0;

            this.ProgressBar.Visibility = Visibility.Collapsed;
        }

        private void DrawList(List<Gifology.Database.Favorites> list)
        {
            this.NextAppButton.IsEnabled = list.Count > Global.limit;
            ColumnOneList.Clear();
            ColumnTwoList.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                if (i % 2 == 0)
                    ColumnOneList.Add(new GiphyImage
                    {
                        Name = list[i].Giphy_Id,
                        Url = list[i].Url
                    });
                else
                    ColumnTwoList.Add(new GiphyImage
                    {
                        Name = list[i].Giphy_Id,
                        Url = list[i].Url
                    });
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
            this.PreviousAppButton.IsEnabled = Offset - Global.limit > 0;

            this.ProgressBar.Visibility = Visibility.Collapsed;
        }

        private void DrawList(List<Gifology.Database.Recents> list)
        {
            this.NextAppButton.IsEnabled = list.Count > Global.limit;
            ColumnOneList.Clear();
            ColumnTwoList.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                if (i % 2 == 0)
                    ColumnOneList.Add(new GiphyImage
                    {
                        Name = list[i].Giphy_Id,
                        Url = list[i].Url
                    });
                else
                    ColumnTwoList.Add(new GiphyImage
                    {
                        Name = list[i].Giphy_Id,
                        Url = list[i].Url
                    });
            }
        }

        private void ViewBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
            if (CurrentView == "Favorite")
                GetFavorites();
            else if (CurrentView == "Recent")
                GetRecents();
        }

        private void CopyUrlAppButton_Click(object sender, RoutedEventArgs e)
        {
            if (ImageListControl.SelectedImage != null)
                GiphyImage.CopyImageUrl(sender, e, ImageListControl.SelectedImage);
        }

        private void FavoriteAppButton_Click(object sender, RoutedEventArgs e)
        {
            if (ImageListControl.SelectedImage != null)
            {
                try
                {
                    var data = new Gifology.Database.Favorites();
                    data.Giphy_Id = ImageListControl.SelectedImage.Name;
                    data.Url = ((BitmapImage)ImageListControl.SelectedImage.Source).UriSource.OriginalString;
                    GifologyDatabase.InsertUpdateFavorite(data);
                    FavoriteAppButton.Visibility = Visibility.Collapsed;
                    UnfavoriteAppButton.Visibility = Visibility.Visible;
                }
                catch (SQLite.SQLiteException ex)
                {
                    Debug.WriteLine("DB EXCEPTION: " + ex.Message);
                }

            }
        }

        private void UnfavoriteAppButton_Click(object sender, RoutedEventArgs e)
        {
            if (ImageListControl.SelectedImage != null)
            {
                try
                {
                    var item = GifologyDatabase.GetFavorite(ImageListControl.SelectedImage.Name);
                    GiphyImage.UnfavoriteImage(sender, e, item);
                    FavoriteAppButton.Visibility = Visibility.Visible;
                    UnfavoriteAppButton.Visibility = Visibility.Collapsed;
                }
                catch (SQLite.SQLiteException ex)
                {
                    Debug.WriteLine("DB EXCEPTION: " + ex.Message);
                }
            }
        }

        private void ShareAppButton_Click(object sender, RoutedEventArgs e)
        {
            if (ImageListControl.SelectedImage != null)
                GiphyImage.ShareImage(sender, e, ImageListControl.SelectedImage);
        }

        private void ShowSingleImageIcons()
        {
            PreviousAppButton.Visibility =
                NextAppButton.Visibility = Visibility.Collapsed;

            if (GifologyDatabase.GetFavorite(ImageListControl.SelectedImage.Name) != null)
            {
                FavoriteAppButton.Visibility = Visibility.Collapsed;
                UnfavoriteAppButton.Visibility = Visibility.Visible;
            }
            else
            {
                FavoriteAppButton.Visibility = Visibility.Visible;
                UnfavoriteAppButton.Visibility = Visibility.Collapsed;
            }

            CopyUrlAppButton.Visibility = ShareAppButton.Visibility = Visibility.Visible;
        }

        private void ShowFullListIcons()
        {
            PreviousAppButton.Visibility =
                NextAppButton.Visibility = Visibility.Visible;

            FavoriteAppButton.Visibility =
                UnfavoriteAppButton.Visibility =
                ShareAppButton.Visibility =
                CopyUrlAppButton.Visibility = Visibility.Collapsed;
        }
    }
}
