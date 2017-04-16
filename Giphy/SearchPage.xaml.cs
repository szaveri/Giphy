using Gifology.Database;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Gifology
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        private static int Total = 0;
        private static int Offset = 0;
        private static int PreviousOffset = 0;
        private static string SearchValue = "";
        private ObservableCollection<GiphyImage> ColumnOneList = new ObservableCollection<GiphyImage>();
        private ObservableCollection<GiphyImage> ColumnTwoList = new ObservableCollection<GiphyImage>();

        public SearchPage()
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
                    break;
                case "Try Again":
                    Page_Loaded(sender, e);
                    break;
                case "Close":
                    break;
            }

        }

        /*
        *  Event triggered to search for GIFs based on search box
        */
        private void GifSearch(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs arg)
        {
            Offset = 0;
            ColumnOneList.Clear();
            ColumnTwoList.Clear();

            SearchValue = arg.QueryText;

            if(SearchValue == "")
            {
                NoSearchText.Visibility = Visibility.Visible;
                NoSearchText.Text = "Start searching for that perfect GIF!";
                this.PreviousAppButton.IsEnabled = false;
                this.NextAppButton.IsEnabled = false;
                return;
            }

            GetGifs();
        }

        /*
         * Loads GIFs based on search text
         */
        private async void GetGifs()
        {
            PreviousOffset = Offset;
            this.ProgressBar.Visibility = Visibility.Visible;

            Uri uri = HttpRequest.GenerateURL("search", Offset, Uri.EscapeDataString(SearchValue));
            var response = await HttpRequest.GetQuery(uri);
            var list = response.data;

            Total = response.pagination.total_count;
            Offset += response.pagination.count;

            this.PreviousAppButton.IsEnabled = Offset - response.pagination.count > 0;
            this.NextAppButton.IsEnabled = Offset < Total;

            ColumnOneList.Clear();
            ColumnTwoList.Clear();

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
            }
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

            GetGifs();
        }

        /*
         * Load next set of GIFs
         */
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            GetGifs();
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
