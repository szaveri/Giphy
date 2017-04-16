using Gifology.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Gifology
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TrendingPage : Page
    {

        private int Offset = 0;
        private int PreviousOffset = 0;
        private ObservableCollection<GiphyImage> ColumnOneList = new ObservableCollection<GiphyImage>();
        private ObservableCollection<GiphyImage> ColumnTwoList = new ObservableCollection<GiphyImage>();
        private bool IsPrevButtonEnabled = false;

        public TrendingPage()
        {
            this.InitializeComponent();
            GiphyImage.RegisterForShare();
        }

        /*
         * Gets new set of Trending GIFs if not previously queried on page load
         */
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            switch (await Global.CheckInternetConnection())
            {
                case "Continue":
                    ImageListControl.NextButton_Clicked += new RoutedEventHandler(NextButton_Click);
                    ImageListControl.PrevButton_Clicked += new RoutedEventHandler(PreviousButton_Click);
                    ImageListControl.ShowSingleImageIcons += ShowSingleImageIcons;
                    ImageListControl.ShowFullListIcons += ShowFullListIcons;
                    Offset = PreviousOffset;
                    GetTrending();
                    break;
                case "Try Again":
                    Page_Loaded(sender, e);
                    break;
                case "Close":
                    break;
            }
            
        }

        /*
         *  Search for latest trending GIFs
         *  Store GIFs in global variable to prevent unnecessary query on search clear
         */
        private async void GetTrending()
        {
            this.ProgressBar.Visibility = Visibility.Visible;
            
            PreviousOffset = Offset;
            Uri uri = HttpRequest.GenerateURL("trending", Offset, null);
            var response = await HttpRequest.GetQuery(uri);
            var list = response.data;
            Offset += response.pagination.count;

            ColumnOneList.Clear();
            ColumnTwoList.Clear();

            for (int i = 0; i < list.Count; i++)
            {
                if (i % 2 == 0)
                    ColumnOneList.Add(new GiphyImage
                    {
                        Name = list[i].id,
                        Url = list[i].images.fixed_width_downsampled != null ? list[i].images.fixed_width_downsampled.url : list[i].images.fixed_width.url
                    });
                else
                    ColumnTwoList.Add(new GiphyImage
                    {
                        Name = list[i].id,
                        Url = list[i].images.fixed_width_downsampled != null ? list[i].images.fixed_width_downsampled.url : list[i].images.fixed_width.url
                    });
            }

            this.PreviousAppButton.IsEnabled = IsPrevButtonEnabled = Offset - response.pagination.count > 0;

            this.ProgressBar.Visibility = Visibility.Collapsed;
        }

        /*
         * Load previous set of GIFs
         */
        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            Offset = PreviousOffset;
            if (Offset - Global.limit >= 0) Offset -= Global.limit;
            else Offset -= Offset;

            GetTrending();
        }

        /*
         * Load next set of GIFs
         */
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            GetTrending();
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
                catch(SQLite.SQLiteException ex)
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
                catch(SQLite.SQLiteException ex)
                {
                    Debug.WriteLine("DB EXCEPTION: " + ex.Message);
                }
            }
        }

        private void ShareAppButton_Click(object sender, RoutedEventArgs e)
        {
            if(ImageListControl.SelectedImage != null)
                GiphyImage.ShareImage(sender, e, ImageListControl.SelectedImage);
        }

        private void ShowSingleImageIcons()
        {
            PreviousAppButton.Visibility = 
                NextAppButton.Visibility = Visibility.Collapsed;

            if(GifologyDatabase.GetFavorite(ImageListControl.SelectedImage.Name) != null)
            {
                FavoriteAppButton.Visibility = Visibility.Collapsed;
                UnfavoriteAppButton.Visibility = Visibility.Visible;
            }
            else {
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
