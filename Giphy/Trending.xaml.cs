using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Giphy
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Trending : Page
    {

        private static int offset = 0;
        private static List<Datum> trendingList = new List<Datum>();

        public Trending()
        {
            this.InitializeComponent();
            GiphyImage.RegisterForShare();
        }

        /*
         * Gets new set of Trending GIFs if not previously queried on page load
         */
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //Only load a default view of trending images if page contains no content
            if (offset == 0)
                GetTrending();
            else
            {
                this.ColumnOne.Children.Clear();
                this.ColumnTwo.Children.Clear();
                DrawList(trendingList);
            }
                
        }

        /*
         *  Search for latest trending GIFs
         *  Store GIFs in global variable to prevent unnecessary query on search clear
         */
        private async void GetTrending()
        {
            ProgressBar.Visibility = Visibility.Visible;
            Uri uri = HttpRequest.GenerateURL("trending", offset, null);
            var response = await HttpRequest.GetQuery(uri);
            var list = response.data;
            trendingList.AddRange(list);
            offset += response.pagination.count;

            DrawList(list);
            
            ProgressBar.Visibility = Visibility.Collapsed;
        }

        /*
         * Draws list of trending GIFs
         */
        private void DrawList(List<Datum> trendingList)
        {
            this.ProgressBar.Visibility = Visibility.Visible;

            for (int i = 0; i < trendingList.Count; i++)
            {
                Image img = new Image();
                img.Name = trendingList[i].id;
                img.Source = new BitmapImage(new Uri(trendingList[i].images.fixed_width.url, UriKind.Absolute));
                img.Margin = new Thickness(0, 0, 10, 10);
                img.Stretch = Stretch.UniformToFill;
                img.MaxWidth = 400;
                img.Tapped += (sender, e) => { GiphyImage.ShowContextManu(sender, e, img); };
                img.RightTapped += (sender, e) => { GiphyImage.ShowContextManu(sender, e, img); };

                if (i % 2 == 0)
                    this.ColumnOne.Children.Add(img);
                else
                    this.ColumnTwo.Children.Add(img);
            }
            this.ProgressBar.Visibility = Visibility.Collapsed;
        }

        /*
         * Load additional GIFs when scrollbar reaches bottom
         */
        private void OnScrollViewerViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var verticalOffset = sv.VerticalOffset;
            var maxVerticalOffset = sv.ScrollableHeight; //sv.ExtentHeight - sv.ViewportHeight;

            if (maxVerticalOffset < 0 ||
                verticalOffset == maxVerticalOffset)
            {
                // Scrolled to bottom
                GetTrending();
            }
        }
    }
}
