using System;
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
using System.Threading.Tasks;
using Gifology.Database;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel;

namespace Gifology
{
    public sealed partial class MainPage : Page
    {
        private Paging Search = new Paging();
        private Paging Trending = new Paging();
        private Paging MyGifs = new Paging();

        private static string SearchValue = "";
        public Dictionary<int, string> NavDictionary = new Dictionary<int, string>()
        {
            {0, "Search" },
            {1, "Trending" },
            {2, "MyGifs" }
        };

        public MainPage()
        {
            this.InitializeComponent();
            GiphyImage.RegisterForShare();
        }

        /*
         * Runs check for user's internet connection on app load 
         */
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Trending.NextEnabled = true;

            switch (await Global.CheckInternetConnection())
            {
                case "Continue":
                    ImageListControl.NextButton_Clicked += new RoutedEventHandler(NextButton_Click);
                    ImageListControl.PrevButton_Clicked += new RoutedEventHandler(PreviousButton_Click);
                    ImageListControl.ShowSingleImageIcons += ShowSingleImageIcons;
                    ImageListControl.ShowFullListIcons += ShowFullListIcons;
                    PivotNavigation.SelectedIndex = 0;
                    break;
                case "Try Again":
                    Page_Loaded(sender, e);
                    break;
                case "Close":
                    break;
            }
        }

        private async void Pivot_NavSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (NavDictionary[PivotNavigation.SelectedIndex])
            {
                case "Search":
                    this.PreviousAppButton.IsEnabled = Search.PreviousEnabled;
                    this.NextAppButton.IsEnabled = Search.NextEnabled;
                    break;
                case "Trending":
                    if(Trending.ColumnOneList.Count == 0 && Trending.ColumnTwoList.Count == 0)
                        GetTrending();
                    this.PreviousAppButton.IsEnabled = Trending.PreviousEnabled;
                    this.NextAppButton.IsEnabled = Trending.NextEnabled;
                    break;
                case "MyGifs":
                    if (ViewBox.SelectedValue == null)
                        ViewBox.SelectedValue = "Favorite";
                    this.PreviousAppButton.IsEnabled = MyGifs.PreviousEnabled;
                    this.NextAppButton.IsEnabled = MyGifs.NextEnabled;
                    break;
            }

            if (GetSelectedImage() == null)
                ShowFullListIcons();
            else
                ShowSingleImageIcons();
        }

        private Image GetSelectedImage()
        {
            switch (NavDictionary[PivotNavigation.SelectedIndex])
            {
                case "Search":
                    return SearchILC.SelectedImage;
                    break;
                case "Trending":
                    return TrendingILC.SelectedImage;
                    break;
                case "MyGifs":
                    return MyGifILC.SelectedImage;
                    break;
                default:
                    return null;
                    break;
            }
        }

        #region Search Functions
        private void GifSearch(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs arg)
        {
            Search.Offset = 0;
            Search.ColumnOneList.Clear();
            Search.ColumnTwoList.Clear();

            SearchValue = arg.QueryText;

            if (SearchValue == "")
            {
                NoSearchText.Visibility = Visibility.Visible;
                NoSearchText.Text = "Start searching for that perfect GIF!";
                this.PreviousAppButton.IsEnabled = Search.PreviousEnabled = false;
                this.NextAppButton.IsEnabled = Search.NextEnabled = false;
                return;
            }

            GetSearchGifs();
        }

        /*
         * Loads GIFs based on search text
         */
        private async void GetSearchGifs()
        {
            Search.PreviousOffset = Search.Offset;

            Uri uri = HttpRequest.GenerateURL("search", Search.Offset, Uri.EscapeDataString(SearchValue));
            var response = await HttpRequest.GetQuery(uri);
            var list = response.data;

            Search.Total = response.pagination.total_count;
            Search.Offset += response.pagination.count;

            this.PreviousAppButton.IsEnabled = Search.PreviousEnabled = Search.Offset - response.pagination.count > 0;
            this.NextAppButton.IsEnabled = Search.NextEnabled = Search.Offset < Search.Total;

            Search.ColumnOneList.Clear();
            Search.ColumnTwoList.Clear();

            if (list.Count == 0)
            {
                NoSearchText.Visibility = Visibility.Visible;
                NoSearchText.Text = "No GIFs Found";
            }
            else
            {
                NoSearchText.Visibility = Visibility.Collapsed;

                for (int i = 0; i < list.Count; i++)
                {
                    if (i % 2 == 0)
                        Search.ColumnOneList.Add(new GiphyImage
                        {
                            Name = list[i].id,
                            Url = list[i].images.fixed_width_downsampled != null ? list[i].images.fixed_width_downsampled.url : list[i].images.fixed_width.url
                        });
                    else
                        Search.ColumnTwoList.Add(new GiphyImage
                        {
                            Name = list[i].id,
                            Url = list[i].images.fixed_width_downsampled != null ? list[i].images.fixed_width_downsampled.url : list[i].images.fixed_width.url
                        });
                }
            }
        }
        #endregion

        #region Trending Functions
        private async void GetTrending()
        {
            Trending.PreviousOffset = Trending.Offset;
            Uri uri = HttpRequest.GenerateURL("trending", Trending.Offset, null);
            var response = await HttpRequest.GetQuery(uri);
            var list = response.data;
            Trending.Offset += response.pagination.count;

            Trending.ColumnOneList.Clear();
            Trending.ColumnTwoList.Clear();

            for (int i = 0; i < list.Count; i++)
            {
                if (i % 2 == 0)
                    Trending.ColumnOneList.Add(new GiphyImage
                    {
                        Name = list[i].id,
                        Url = list[i].images.fixed_width_downsampled != null ? list[i].images.fixed_width_downsampled.url : list[i].images.fixed_width.url
                    });
                else
                    Trending.ColumnTwoList.Add(new GiphyImage
                    {
                        Name = list[i].id,
                        Url = list[i].images.fixed_width_downsampled != null ? list[i].images.fixed_width_downsampled.url : list[i].images.fixed_width.url
                    });
            }

            this.PreviousAppButton.IsEnabled = Trending.PreviousEnabled = Trending.Offset - response.pagination.count > 0;
        }
        #endregion

        #region MyGifs Functions
        /* ==================================
         * START FAVORITE FUNCTIONS
         * ==================================
         */
        private async void GetFavorites()
        {
            MyGifs.PreviousOffset = MyGifs.Offset;
            var list = await GifologyDatabase.GetFavorites(MyGifs.Offset);
            MyGifs.Offset += Global.limit;

            this.NextAppButton.IsEnabled = list.Count > Global.limit;
            MyGifs.ColumnOneList.Clear();
            MyGifs.ColumnTwoList.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                if (i % 2 == 0)
                    MyGifs.ColumnOneList.Add(new GiphyImage
                    {
                        Name = list[i].Giphy_Id,
                        Url = Uri.IsWellFormedUriString(GiphyImage.ConvertSourceType(list[i].Url, "downscale"), UriKind.Absolute) ? GiphyImage.ConvertSourceType(list[i].Url, "downscale") : GiphyImage.ConvertSourceType(list[i].Url, "fixed_width")
                    });
                else
                    MyGifs.ColumnTwoList.Add(new GiphyImage
                    {
                        Name = list[i].Giphy_Id,
                        Url = Uri.IsWellFormedUriString(GiphyImage.ConvertSourceType(list[i].Url, "downscale"), UriKind.Absolute) ? GiphyImage.ConvertSourceType(list[i].Url, "downscale") : GiphyImage.ConvertSourceType(list[i].Url, "fixed_width")
                    });
            }

            this.PreviousAppButton.IsEnabled = MyGifs.PreviousEnabled = MyGifs.Offset - Global.limit > 0;
        }

        /* ==================================
         * START RECENT FUNCTIONS
         * ==================================
         */
        private async void GetRecents()
        {
            MyGifs.PreviousOffset = MyGifs.Offset;
            var list = await GifologyDatabase.GetRecents(MyGifs.Offset);
            MyGifs.Offset += Global.limit;

            this.NextAppButton.IsEnabled = MyGifs.NextEnabled = list.Count > Global.limit;
            MyGifs.ColumnOneList.Clear();
            MyGifs.ColumnTwoList.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                if (i % 2 == 0)
                    MyGifs.ColumnOneList.Add(new GiphyImage
                    {
                        Name = list[i].Giphy_Id,
                        Url = Uri.IsWellFormedUriString(GiphyImage.ConvertSourceType(list[i].Url, "downscale"), UriKind.Absolute) ? GiphyImage.ConvertSourceType(list[i].Url, "downscale") : GiphyImage.ConvertSourceType(list[i].Url, "fixed_width")
                    });
                else
                    MyGifs.ColumnTwoList.Add(new GiphyImage
                    {
                        Name = list[i].Giphy_Id,
                        Url = Uri.IsWellFormedUriString(GiphyImage.ConvertSourceType(list[i].Url, "downscale"), UriKind.Absolute) ? GiphyImage.ConvertSourceType(list[i].Url, "downscale") : GiphyImage.ConvertSourceType(list[i].Url, "fixed_width")
                    });
            }

            this.PreviousAppButton.IsEnabled = MyGifs.Offset - Global.limit > 0;
        }

        private void ViewBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = ((ComboBoxItem)ViewBox.SelectedItem).Tag.ToString();
            MyGifs.Offset = 0;

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
        #endregion

        #region Button Functions

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            this.ProgressBar.Visibility = Visibility.Visible;

            switch (NavDictionary[PivotNavigation.SelectedIndex])
            {
                case "Search":
                    Search.Offset = Search.PreviousOffset;
                    if (Search.Offset - Global.limit >= 0) Search.Offset -= Global.limit;
                    else Search.Offset -= Search.Offset;
                    GetSearchGifs();
                    break;
                case "Trending":
                    Trending.Offset = Trending.PreviousOffset;
                    if (Trending.Offset - Global.limit >= 0) Trending.Offset -= Global.limit;
                    else Trending.Offset -= Trending.Offset;
                    GetTrending();
                    break;
                case "MyGifs":
                    MyGifs.Offset = MyGifs.PreviousOffset;
                    if (MyGifs.Offset - Global.limit >= 0) MyGifs.Offset -= Global.limit;
                    else MyGifs.Offset -= MyGifs.Offset;
                    switch (((ComboBoxItem)ViewBox.SelectedItem).Tag.ToString())
                    {
                        case "Favorite":
                            GetFavorites();
                            break;
                        case "Recent":
                            GetRecents();
                            break;
                    }
                    break;
            }

            this.ProgressBar.Visibility = Visibility.Collapsed;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            this.ProgressBar.Visibility = Visibility.Visible;

            switch (NavDictionary[PivotNavigation.SelectedIndex])
            {
                case "Search":
                    Search.PreviousOffset = Search.Offset;
                    GetSearchGifs();
                    break;
                case "Trending":
                    Trending.PreviousOffset = Trending.Offset;
                    GetTrending();
                    break;
                case "MyGifs":
                    MyGifs.PreviousOffset = MyGifs.Offset;

                    switch (((ComboBoxItem)ViewBox.SelectedItem).Tag.ToString())
                    {
                        case "Favorite":
                            GetFavorites();
                            break;
                        case "Recent":
                            GetRecents();
                            break;
                    }
                    break;
            }
            
            this.ProgressBar.Visibility = Visibility.Collapsed;
        }

        private void CopyUrlAppButton_Click(object sender, RoutedEventArgs e)
        {
            var img = GetSelectedImage();
            if(img != null)
                GiphyImage.CopyImageUrl(sender, e, img);
        }

        private void FavoriteAppButton_Click(object sender, RoutedEventArgs e)
        {
            this.ProgressRing.Visibility = Visibility.Visible;

            var img = GetSelectedImage();
            if (img != null)
            {
                try
                {
                    GiphyImage.FavoriteImage(sender, e, img);
                    FavoriteAppButton.Visibility = Visibility.Collapsed;
                    UnfavoriteAppButton.Visibility = Visibility.Visible;
                }
                catch (SQLite.SQLiteException ex)
                {
                    Debug.WriteLine("DB EXCEPTION: " + ex.Message);
                }

            }

            this.ProgressRing.Visibility = Visibility.Collapsed;
        }

        private void UnfavoriteAppButton_Click(object sender, RoutedEventArgs e)
        {
            this.ProgressRing.Visibility = Visibility.Visible;

            var img = GetSelectedImage();
            if (img != null)
            {
                try
                {
                    var item = GifologyDatabase.GetFavorite(img.Name);
                    GiphyImage.UnfavoriteImage(sender, e, item);
                    FavoriteAppButton.Visibility = Visibility.Visible;
                    UnfavoriteAppButton.Visibility = Visibility.Collapsed;
                }
                catch (SQLite.SQLiteException ex)
                {
                    Debug.WriteLine("DB EXCEPTION: " + ex.Message);
                }
            }

            this.ProgressRing.Visibility = Visibility.Collapsed;
        }

        private void ShareAppButton_Click(object sender, RoutedEventArgs e)
        {
            this.ProgressRing.Visibility = Visibility.Visible;

            var img = GetSelectedImage();
            if (img != null)
                GiphyImage.ShareImage(sender, e, img);

            this.ProgressRing.Visibility = Visibility.Collapsed;
        }

        private void ShowSingleImageIcons()
        {
            var img = GetSelectedImage();
            if (img == null)
                return;

            PreviousAppButton.Visibility =
            NextAppButton.Visibility = Visibility.Collapsed;

            if (GifologyDatabase.GetFavorite(img.Name) != null)
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

        #endregion
    }
}
