using System;
using System.Collections.Generic;
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

        private static int Offset = 0;
        private static int PreviousOffset = 0;

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
            PreviousOffset = Offset;
            this.ProgressBar.Visibility = Visibility.Visible;
            Uri uri = HttpRequest.GenerateURL("trending", Offset, null);
            var response = await HttpRequest.GetQuery(uri);
            var list = response.data;
            Offset += response.pagination.count;

            this.PreviousAppButton.IsEnabled = this.PreviousButton.IsEnabled = Offset - response.pagination.count > 0;

            DrawList(list);
            
            this.ProgressBar.Visibility = Visibility.Collapsed;
        }

        /*
         * Draws list of trending GIFs
         */
        private void DrawList(List<Datum> list)
        {
            this.ProgressBar.Visibility = Visibility.Visible;

            for (int i = 0; i < list.Count; i++)
            {
                Image img = new Image();
                img.Name = list[i].id;
                img.Source = new BitmapImage(new Uri(list[i].images.fixed_width_downsampled.url, UriKind.Absolute));
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

        /*
         * Load previous set of GIFs
         */
        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            this.ColumnOne.Children.Clear();
            this.ColumnTwo.Children.Clear();

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
            this.ColumnOne.Children.Clear();
            this.ColumnTwo.Children.Clear();

            GetTrending();
        }
    }
}
