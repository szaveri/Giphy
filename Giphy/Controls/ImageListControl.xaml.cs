using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

namespace Gifology
{
    public partial class ImageListControl : UserControl
    {
        #region Properties
        public static event RoutedEventHandler NextButton_Clicked;
        public static event RoutedEventHandler PrevButton_Clicked;
        public static Action ShowSingleImageIcons = () => { };
        public static Action ShowFullListIcons = () => { };
        
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
            DependencyProperty.Register("SelectedImage", typeof(ObservableCollection<GiphyImage>), typeof(ImageListControl), null);

        public Image SelectedImage
        {
            get { return GetValue(SelectedImageProperty) as Image; }
            set { SetValue(SelectedImageProperty, value); }
        }

        #endregion

        public ImageListControl()
        {
            this.InitializeComponent();
        }

        #region Functions
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
            SystemNavigationManager.GetForCurrentView().BackRequested += null;
        }

        private void ImageList_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Image img = sender as Image;
            SelectedImage = img;
            SingleImage.Source = new BitmapImage(new Uri(GiphyImage.ConvertSourceType(((BitmapImage)img.Source).UriSource.OriginalString, "fixed_width")));
            SingleImageWrapper.Visibility = Visibility.Visible;
            ContentWrapper.Visibility = Visibility.Collapsed;

            if (ShowSingleImageIcons != null)
                ShowSingleImageIcons();

            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            CloseButton_Click(null, null);
        }
        #endregion
    }
}
