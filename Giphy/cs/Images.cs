using System;
using System.Diagnostics;
using System.Net.Http;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using SQLitePCL;

namespace Giphy
{
    public class GiphyImage
    {
        public string type { get; }
        public string id { get; }
        public string rating { get; }
        public string url { get; }

        public GiphyImage()
        {
            this.type = null;
            this.id = null;
            this.rating = null;
            this.url = null;
        }

        public GiphyImage(string type, string id, string rating, string url)
        {
            this.type = type;
            this.id = id;
            this.rating = rating;
            this.url = url;
        }

        /*
        * Opens context menu on tap or right click (hold)
        */
        public static void ShowContextManu(object sender, TappedRoutedEventArgs e, Image img)
        {
            e.Handled = true;
            var flyout = GiphyImage.GenerateFlyout(img);
            FrameworkElement senderElement = sender as FrameworkElement;
            flyout.ShowAt(senderElement);
        }

        public static void ShowContextManu(object sender, RightTappedRoutedEventArgs e, Image img)
        {
            e.Handled = true;
            var flyout = GiphyImage.GenerateFlyout(img);
            FrameworkElement senderElement = sender as FrameworkElement;
            flyout.ShowAt(senderElement);
        }

        /*
         * Create flyout menu options for specific image
         */
        public static MenuFlyout GenerateFlyout(Image img)
        {
            MenuFlyout flyout = new MenuFlyout();
            MenuFlyoutItem share = new MenuFlyoutItem { Text = "Share" };
            MenuFlyoutItem copyUrl = new MenuFlyoutItem { Text = "Copy URL" };
            flyout.Items.Add(share);
            flyout.Items.Add(copyUrl);
            share.Tapped += (sender, e) => { ShareImage(sender, e, img); };
            copyUrl.Tapped += (sender, e) => { CopyImageUrl(sender, e, img); };

            /*
             * Check if image is already favorited, if so "Unfavorite", else favorite
             */
             //Coding here

            return flyout;
        }

        /*
         * Called after page initializing for image sharing
         */
        public static void RegisterForShare()
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(ShareImageHandler);
        }

        /*
         * Downloads file for sharing
         * Displays Share UI
         */
        private static async void ShareImage(object sender, RoutedEventArgs e, Image img)
        {
            string fileName = img.Name + ".gif";

            if (await ApplicationData.Current.TemporaryFolder.TryGetItemAsync(fileName) == null)
            {
                var httpClient = new HttpClient();
                HttpResponseMessage message = await httpClient.GetAsync(((BitmapImage)img.Source).UriSource.OriginalString);
                StorageFolder myfolder = ApplicationData.Current.TemporaryFolder;
                StorageFile SampleFile = await myfolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                byte[] file = await message.Content.ReadAsByteArrayAsync();
                await FileIO.WriteBytesAsync(SampleFile, file);
                var files = await myfolder.GetFilesAsync();
            }

            Global.shareFileName = fileName;
            DataTransferManager.ShowShareUI();
        }

        /*
         * Event handler to send image after ShowShareUI() is called
         */
        private static async void ShareImageHandler(DataTransferManager sender, DataRequestedEventArgs e)
        {
            DataRequest request = e.Request;
            request.Data.Properties.Title = "Share Image";
            request.Data.Properties.Description = "Shared image from Giphy";

            DataRequestDeferral deferral = request.GetDeferral();

            try
            {
                StorageFile imageFile = await ApplicationData.Current.TemporaryFolder.GetFileAsync(Global.shareFileName);
                request.Data.SetBitmap(RandomAccessStreamReference.CreateFromFile(imageFile));
            }
            finally
            {
                deferral.Complete();
            }
        }

        /*
         * Event handler to copy image URL to clipboard
         */
        private static void CopyImageUrl(object sender, RoutedEventArgs e, Image img)
        {
            DataPackage dataPackage = new DataPackage();
            var url = ((BitmapImage)img.Source).UriSource.OriginalString;
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(url);
            Clipboard.SetContent(dataPackage);
            Clipboard.Flush();
        }
    }
}
