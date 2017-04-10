using System;
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

        public SearchPage()
        {
            this.InitializeComponent();
            GiphyImage.RegisterForShare();
        }

        /*
        *  Event triggered to search for GIFs based on search box
        */
        private void GifSearch(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs arg)
        {
            Offset = 0;
            this.ColumnOne.Children.Clear();
            this.ColumnTwo.Children.Clear();

            SearchValue = arg.QueryText;

            if(SearchValue == "")
            {
                NoSearchText.Visibility = Visibility.Visible;
                NoSearchText.Text = "Start searching for that perfect GIF!";
                this.PreviousAppButton.IsEnabled = this.PreviousButton.IsEnabled = false;
                this.NextAppButton.IsEnabled = this.NextButton.IsEnabled = false;
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

            this.PreviousAppButton.IsEnabled = this.PreviousButton.IsEnabled = Offset - response.pagination.count > 0;
            this.NextAppButton.IsEnabled = this.NextButton.IsEnabled = Offset < Total;

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
                    Image img = new Image();
                    img.Name = list[i].id;
                    img.Source = new BitmapImage(new Uri(list[i].images.fixed_width.url, UriKind.Absolute));
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
            //Draws list on scroll view
            
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

            GetGifs();
        }

        /*
         * Load next set of GIFs
         */
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            this.ColumnOne.Children.Clear();
            this.ColumnTwo.Children.Clear();

            GetGifs();
        }
    }
}
