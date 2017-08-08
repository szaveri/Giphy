using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Gifology.Controls
{
    public partial class ImageListControl : UserControl, INotifyPropertyChanged
    {
        #region Properties
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public static event RoutedEventHandler NextButton_Clicked;
        public static event RoutedEventHandler PrevButton_Clicked;
        public static Action ShowSingleImageIcons = () => { };
        public static Action ShowFullListIcons = () => { };

        public int AutoPlayEnabled = SettingsItem.AutoPlay;

        public static readonly DependencyProperty ColumnOneListProperty =
            DependencyProperty.Register("ColumnOneList", typeof(ObservableCollection<GiphyImage>), typeof(ImageListControl), null);

        public ObservableCollection<GiphyImage> ColumnOneList
        {
            get { return GetValue(ColumnOneListProperty) as ObservableCollection<GiphyImage>; }
            set { ColumnOneListView.ItemsSource = value; }
        }

        public static readonly DependencyProperty ColumnTwoListProperty =
            DependencyProperty.Register("ColumnTwoList", typeof(ObservableCollection<GiphyImage>), typeof(ImageListControl), null);

        public ObservableCollection<GiphyImage> ColumnTwoList
        {
            get { return GetValue(ColumnTwoListProperty) as ObservableCollection<GiphyImage>; }
            set { ColumnTwoListView.ItemsSource = value; }
        }

        public static readonly DependencyProperty SelectedImageProperty =
            DependencyProperty.Register("SelectedImage", typeof(Image), typeof(ImageListControl), null);

        public Image SelectedImage
        {
            get { return GetValue(SelectedImageProperty) as Image; }
            set { SetValue(SelectedImageProperty, value); }
        }

        private bool SingleTap;
        #endregion

        public ImageListControl()
        {
            this.InitializeComponent();
        }

        #region Functions
        private void OnScrollViewerViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (SettingsItem.InfiniteScroll == 0)
                return;

            var verticalOffset = sv.VerticalOffset;
            var maxVerticalOffset = sv.ScrollableHeight;

            if (maxVerticalOffset < 0 ||
                verticalOffset == maxVerticalOffset)
            {
                // Scrolled to bottom
                NextButton_Click(null, null);
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (NextButton_Clicked != null)
                NextButton_Clicked(this, e);
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (PrevButton_Clicked != null)
                PrevButton_Clicked(this, e);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedImage = null;
            SingleImage.Source = null;
            SingleImageWrapper.Visibility = Visibility.Collapsed;
            ContentWrapper.Visibility = Visibility.Visible;

            if (ShowFullListIcons != null)
                ShowFullListIcons();

            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested -= OnCloseRequest;
        }

        private async void ImageList_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.SingleTap = true;
            await Task.Delay(200);
            if (this.SingleTap)
            {
                Image img = sender as Image;
                SelectedImage = img;

                var OriginalUrl = ((BitmapImage)img.Source).UriSource.OriginalString;
                SingleImage.Source = Uri.IsWellFormedUriString(GiphyImage.ConvertSourceType(OriginalUrl, SettingsItem.GifQuality), UriKind.Absolute) ?
                    new BitmapImage(new Uri(GiphyImage.ConvertSourceType(OriginalUrl, SettingsItem.GifQuality))) :
                    new BitmapImage(new Uri(GiphyImage.ConvertSourceType(OriginalUrl, "High")));
                SingleImageWrapper.Visibility = Visibility.Visible;
                ContentWrapper.Visibility = Visibility.Collapsed;

                if (ShowSingleImageIcons != null)
                    ShowSingleImageIcons();

                SystemNavigationManager.GetForCurrentView().BackRequested += OnCloseRequest;
                Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            }
        }

        private void Image_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (SettingsItem.AutoPlay == 1)
                return;

            this.SingleTap = false;
            var Img = sender as Image;
            var Source = Img.Source as BitmapImage;

            if (Source.IsPlaying)
                Source.Stop();
            else
                Source.Play();
        }

        private void OnCloseRequest(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            CloseButton_Click(null, null);
        }
        #endregion
    }
}
