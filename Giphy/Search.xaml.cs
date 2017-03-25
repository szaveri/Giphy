using System;
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
    public sealed partial class Search : Page
    {
        private static int total = int.MaxValue;
        private static int offset = 0;
        private static string searchValue = "";

        public Search()
        {
            this.InitializeComponent();
            GiphyImage.RegisterForShare();
        }

        /*
        *  Event triggered to search for GIFs based on search box
        */
        private void GifSearch(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs arg)
        {
            offset = 0;
            this.ColumnOne.Children.Clear();
            this.ColumnTwo.Children.Clear();
            searchValue = arg.QueryText;
            GetGifs();
        }

        /*
         * Loads GIFs based on search text
         */
        private async void GetGifs()
        {
            this.ProgressBar.Visibility = Visibility.Visible;

            Uri uri = HttpRequest.GenerateURL("search", offset, Uri.EscapeDataString(searchValue));
            var response = await HttpRequest.GetQuery(uri);
            var list = response.data;

            total = response.pagination.total_count;
            offset += response.pagination.count;

            //Draws list on scroll view
            for (int i = 0; i < list.Count; i++)
            {
                Image img = new Image();
                img.Name = list[i].id;
                img.Source = new BitmapImage(new Uri(list[i].images.fixed_width.url, UriKind.Absolute));
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
            if (offset >= total)
                return;

            var verticalOffset = sv.VerticalOffset;
            var maxVerticalOffset = sv.ScrollableHeight; //sv.ExtentHeight - sv.ViewportHeight;

            if (maxVerticalOffset < 0 ||
                verticalOffset == maxVerticalOffset)
            {
                // Scrolled to bottom
                GetGifs();
            }
        }
    }
}
