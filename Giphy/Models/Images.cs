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
using Gifology.Database;
using SQLite;
using System.Collections.Generic;

namespace Gifology
{
    public class GiphyImage
    {
        public string Name { get; set; }
        public string Url { get; set; }

        public static string ConvertSourceType(string url, string type)
        {
            int lastIndex = url.LastIndexOf('/');
            string fileName = url.Substring(lastIndex + 1);

            switch (type)
            {
                case "gif":
                    url = url.Replace(fileName, "giphy.gif");
                    break;
                case "mp4":
                    url = url.Replace(fileName, "giphy.mp4");
                    break;
                case "fixed_width":
                    url = url.Replace(fileName, "200w.gif");
                    break;
                case "downscale":
                    url = url.Replace(fileName, "200w_d.gif");
                    break;
                case "High":
                    url = url.Replace(fileName, "giphy.gif");
                    break;
                case "Medium":
                    url = url.Replace(fileName, "200w.gif");
                    break;
                case "Low":
                    url = url.Replace(fileName, "200w_d.gif");
                    break;
                default:
                    url = url.Replace(fileName, "giphy.gif");
                    break;
            }

            return url;
        }

        /*
        * Opens context menu on tap or right click (hold)
        */
        public static void ShowContextMenu(object sender, TappedRoutedEventArgs e, Image img)
        {
            e.Handled = true;
            var flyout = GiphyImage.GenerateFlyout(img);
            FrameworkElement senderElement = sender as FrameworkElement;
            flyout.ShowAt(senderElement, e.GetPosition((UIElement)sender));
        }

        public static void ShowContextMenu(object sender, RightTappedRoutedEventArgs e, Image img)
        {
            e.Handled = true;
            var flyout = GiphyImage.GenerateFlyout(img);
            FrameworkElement senderElement = sender as FrameworkElement;
            flyout.ShowAt(senderElement, e.GetPosition((UIElement)sender));
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

            flyout.Items.Add(new MenuFlyoutSeparator());

            /*
             * Check if image is already favorited, if so "Unfavorite", else favorite
             */
            try
            {
                using (var conn = new SQLiteConnection(Global.databaseFile))
                {
                    var item = GifologyDatabase.GetFavorite(img.Name);
                    if (item != null)
                    {
                        MenuFlyoutItem unfavorite = new MenuFlyoutItem { Text = "Remove from Favorites" };
                        flyout.Items.Add(unfavorite);
                        unfavorite.Tapped += (sender, e) => { UnfavoriteImage(sender, e, item); };
                    }
                    else
                    {
                        MenuFlyoutItem favorite = new MenuFlyoutItem { Text = "Add to Favorites" };
                        flyout.Items.Add(favorite);
                        favorite.Tapped += (sender, e) => { FavoriteImage(sender, e, img); };
                    }
                }
            }
            catch (SQLiteException e)
            {
                Debug.WriteLine("DB EXCEPTION: " + e.Message);
            }


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
        public static async void ShareImage(object sender, RoutedEventArgs e, Image img)
        {
            string fileName = img.Name + ".gif";

            if (await ApplicationData.Current.TemporaryFolder.TryGetItemAsync(fileName) == null)
            {
                var httpClient = new HttpClient();

                var OriginalUrl = ((BitmapImage)img.Source).UriSource.OriginalString;
                var RequestUri = Uri.IsWellFormedUriString(GiphyImage.ConvertSourceType(OriginalUrl, SettingsItem.GifQuality), UriKind.Absolute) ?
                    new Uri(GiphyImage.ConvertSourceType(OriginalUrl, SettingsItem.GifQuality)) :
                    new Uri(GiphyImage.ConvertSourceType(OriginalUrl, "High"));

                HttpResponseMessage message = await httpClient.GetAsync(RequestUri);
                StorageFolder myfolder = ApplicationData.Current.TemporaryFolder;
                StorageFile SampleFile = await myfolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                byte[] file = await message.Content.ReadAsByteArrayAsync();
                await FileIO.WriteBytesAsync(SampleFile, file);
                var files = await myfolder.GetFilesAsync();
            }

            Global.shareFileName = fileName;
            DataTransferManager.ShowShareUI();

            //Add to Recents database
            var data = new Gifology.Database.Recents();
            data.Giphy_Id = img.Name;
            data.Url = ((BitmapImage)img.Source).UriSource.OriginalString;
            GifologyDatabase.InsertUpdateRecent(data);
        }

        /*
         * Event handler to send image after ShowShareUI() is called
         */
        private static async void ShareImageHandler(DataTransferManager sender, DataRequestedEventArgs e)
        {
            DataRequest request = e.Request;
            DataRequestDeferral deferral = request.GetDeferral();

            try
            {
                DataPackage requestData = request.Data;
                requestData.Properties.Title = "Share Image";
                requestData.Properties.Description = "Shared image from Gifology";

                List<IStorageItem> imageItems = new List<IStorageItem>();
                imageItems.Add(await ApplicationData.Current.TemporaryFolder.GetFileAsync(Global.shareFileName));
                requestData.SetStorageItems(imageItems);

                RandomAccessStreamReference imageStreamRef = RandomAccessStreamReference.CreateFromFile(await ApplicationData.Current.TemporaryFolder.GetFileAsync(Global.shareFileName));
                requestData.Properties.Thumbnail = imageStreamRef;
                requestData.SetBitmap(imageStreamRef);
            }
            finally
            {
                deferral.Complete();
            }
        }

        /*
         * Event handler to copy image URL to clipboard
         */
        public static void CopyImageUrl(object sender, RoutedEventArgs e, Image img)
        {
            DataPackage dataPackage = new DataPackage();
            var url = GiphyImage.ConvertSourceType(((BitmapImage)img.Source).UriSource.OriginalString, "original");
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(url);
            Clipboard.SetContent(dataPackage);
            Clipboard.Flush();

            //Add to Recents database
            var data = new Gifology.Database.Recents();
            data.Giphy_Id = img.Name;
            data.Url = url;
            GifologyDatabase.InsertUpdateRecent(data);
        }

        /*
         *  Event handler to add image to favorites
         */
        public static void FavoriteImage(object sender, RoutedEventArgs e, Image img)
        {
            var data = new Gifology.Database.Favorites();
            data.Giphy_Id = img.Name;
            data.Url = GiphyImage.ConvertSourceType(((BitmapImage)img.Source).UriSource.OriginalString, "original");
            GifologyDatabase.InsertUpdateFavorite(data);
        }

        /*
         * Event handler to remove image from favorites`
         */
        public static void UnfavoriteImage(object sender, RoutedEventArgs e, Gifology.Database.Favorites favorite)
        {
            GifologyDatabase.DeleteFavorite(favorite);
        }
    }
}
