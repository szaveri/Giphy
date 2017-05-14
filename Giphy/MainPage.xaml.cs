using System;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Networking.Connectivity;
using Windows.UI.Popups;
using Gifology.Database;
using Gifology.Controls;
using Windows.UI.Core;

namespace Gifology
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        #region Variables
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _isImageSelected;
        public bool IsImageSelected
        {
            get { return _isImageSelected; }
            set { _isImageSelected = value;
                OnPropertyChanged("IsImageSelected");
            }
        }

        private bool _isInfiniteScroll;
        public bool IsInfiniteScroll
        {
            get { return _isInfiniteScroll; }
            set
            {
                _isInfiniteScroll = value;
                OnPropertyChanged("IsInfiniteScroll");
            }
        }

        private bool ConnectionDismiss = false;
        private NotificationControl NoInternetConnection = null;
        private NotificationControl MeteredNotification = null;

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
        private static ObservableCollection<CategoryListItem> CategoryList = new ObservableCollection<CategoryListItem>();
        private static List<Categories> Categories { get; set; } = new List<Categories>();
        #endregion

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            GiphyImage.RegisterForShare();
        }

        /*
         * Runs check for user's internet connection on app load 
         */
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Trending.NextEnabled = true;
            IsInfiniteScroll = SettingsItem.InfiniteScroll == 1 ? true : false;
            ImageListControl.NextButton_Clicked += new RoutedEventHandler(NextButton_Click);
            ImageListControl.PrevButton_Clicked += new RoutedEventHandler(PreviousButton_Click);
            ImageListControl.ShowSingleImageIcons += ShowSingleImageIcons;
            ImageListControl.ShowFullListIcons += ShowFullListIcons;

            CategorySelectionControl.LoadCategoryList += () =>
            {
                if(NavDictionary[PivotNavigation.SelectedIndex] == "MyGifs" && ((ComboBoxItem)ViewBox.SelectedItem).Tag.ToString() == "Category")
                    LoadCategoryList(0);
            };

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                Frame.CanGoBack ?
                AppViewBackButtonVisibility.Visible :
                AppViewBackButtonVisibility.Collapsed;

            PivotNavigation.SelectedIndex = NavDictionary.FirstOrDefault(x => x.Value == SettingsItem.StartPage).Key;
        }

        #region Navigation Functions
        private async void Pivot_NavSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EditCategoryAppButton.Visibility = Visibility.Collapsed;
            DeleteCategoryAppButton.Visibility = Visibility.Collapsed;

            if (!InternetStatus()) return;

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
                    Categories = await GifologyDatabase.GetCategories();
                    CategoryBox.ItemsSource = Categories;

                    if (ViewBox.SelectedValue == null)
                        ViewBox.SelectedValue = "Favorite";
                    else if (((ComboBoxItem)ViewBox.SelectedItem).Tag.ToString() == "Favorite")
                    {
                        MyGifs.Offset = MyGifs.PreviousOffset;
                        GetFavorites();
                    }
                    else if (((ComboBoxItem)ViewBox.SelectedItem).Tag.ToString() == "Recent")
                    {
                        MyGifs.Offset = MyGifs.PreviousOffset;
                        GetRecents();
                    }
                    else if (((ComboBoxItem)ViewBox.SelectedItem).Tag.ToString() == "Category")
                    {
                        CategoryBox.SelectedIndex = 0;
                        if (((Categories)CategoryBox.SelectedValue).Id != 1)
                        {
                            EditCategoryAppButton.Visibility = Visibility.Visible;
                            DeleteCategoryAppButton.Visibility = Visibility.Visible;
                        }
                    }

                    this.PreviousAppButton.IsEnabled = MyGifs.PreviousEnabled;
                    this.NextAppButton.IsEnabled = MyGifs.NextEnabled;
                    break;
            }

            if (GetSelectedImage() == null)
                ShowFullListIcons();
            else
                ShowSingleImageIcons();
        }
        #endregion

        #region Page Functions
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
        #endregion

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

            if(SettingsItem.InfiniteScroll == 0)
            {
                Search.ColumnOneList.Clear();
                Search.ColumnTwoList.Clear();
            }

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

            if(SettingsItem.InfiniteScroll == 0)
            {
                Trending.ColumnOneList.Clear();
                Trending.ColumnTwoList.Clear();
            }

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

            if(SettingsItem.InfiniteScroll == 0)
            {
                MyGifs.ColumnOneList.Clear();
                MyGifs.ColumnTwoList.Clear();
            }
            
            for (int i = 0; i < list.Count; i++)
            {
                if (i % 2 == 0)
                    MyGifs.ColumnOneList.Add(new GiphyImage
                    {
                        Name = list[i].Giphy_Id,
                        Url = GetListUri(list[i].Url)
                    });
                else
                    MyGifs.ColumnTwoList.Add(new GiphyImage
                    {
                        Name = list[i].Giphy_Id,
                        Url = GetListUri(list[i].Url)
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

            if (SettingsItem.InfiniteScroll == 0)
            {
                MyGifs.ColumnOneList.Clear();
                MyGifs.ColumnTwoList.Clear();
            }
            
            for (int i = 0; i < list.Count; i++)
            {
                if (i % 2 == 0)
                    MyGifs.ColumnOneList.Add(new GiphyImage
                    {
                        Name = list[i].Giphy_Id,
                        Url = GetListUri(list[i].Url)
                    });
                else
                    MyGifs.ColumnTwoList.Add(new GiphyImage
                    {
                        Name = list[i].Giphy_Id,
                        Url = GetListUri(list[i].Url)
                    });
            }

            this.PreviousAppButton.IsEnabled = MyGifs.Offset - Global.limit > 0;
        }

        /* ==================================
         * START CATEGORY FUNCTIONS
         * ==================================
         */
        private async void GetCategory(int Category_Id)
        {
            MyGifs.PreviousOffset = MyGifs.Offset;
            var list = await GifologyDatabase.GetFavoritesInCategory(Category_Id, MyGifs.Offset);
            MyGifs.Offset += Global.limit;

            this.NextAppButton.IsEnabled = MyGifs.NextEnabled = list.Count > Global.limit;
            
            if (SettingsItem.InfiniteScroll == 0)
            {
                MyGifs.ColumnOneList.Clear();
                MyGifs.ColumnTwoList.Clear();
            }
            for (int i = 0; i < list.Count; i++)
            {
                if (i % 2 == 0)
                    MyGifs.ColumnOneList.Add(new GiphyImage
                    {
                        Name = list[i].Giphy_Id,
                        Url = GetListUri(list[i].Url)
                    });
                else
                    MyGifs.ColumnTwoList.Add(new GiphyImage
                    {
                        Name = list[i].Giphy_Id,
                        Url = GetListUri(list[i].Url)
                    });
            }

            this.PreviousAppButton.IsEnabled = MyGifs.Offset - Global.limit > 0;
        }


        private void ViewBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = ((ComboBoxItem)ViewBox.SelectedItem).Tag.ToString();
            MyGifs.Offset = 0;
            MyGifs.ColumnOneList.Clear();
            MyGifs.ColumnTwoList.Clear();

            switch (selected)
            {
                case "Category":
                    if (CategoryBox.SelectedIndex == -1 || CategoryBox.SelectedValue == null)
                        CategoryBox.SelectedIndex = 0;
                    CategoryBox.Visibility = Visibility.Visible;
                    break;
                case "Favorite":
                    CategoryBox.Visibility = Visibility.Collapsed;
                    CategoryBox.SelectedIndex = -1;
                    GetFavorites();
                    break;
                case "Recent":
                    CategoryBox.Visibility = Visibility.Collapsed;
                    CategoryBox.SelectedIndex = -1;
                    GetRecents();
                    break;
            }
        }

        private async void CategoryBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MyGifs.Offset = 0;
            MyGifs.ColumnOneList.Clear();
            MyGifs.ColumnTwoList.Clear();

            if (CategoryBox.SelectedIndex != -1 && ((Categories)CategoryBox.SelectedValue).Id != 1)
            {
                EditCategoryAppButton.Visibility = Visibility.Visible;
                DeleteCategoryAppButton.Visibility = Visibility.Visible;
            }
            else
            {
                EditCategoryAppButton.Visibility = Visibility.Collapsed;
                DeleteCategoryAppButton.Visibility = Visibility.Collapsed;
            }

            if (CategoryBox.SelectedValue == null)
            {
                return;
            }
            int? Id = ((Categories)CategoryBox.SelectedValue).Id;
            GetCategory(Id ?? default(int));
        }
        #endregion

        #region Button Functions

        private async void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (!InternetStatus()) return;

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
                        case "Category":
                            string Selected = ((ComboBoxItem)CategoryBox.SelectedItem).Tag.ToString();
                            GetCategory(Int32.Parse(Selected));
                            break;
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

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (!InternetStatus()) return;

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
            {
                GiphyImage.CopyImageUrl(sender, e, img);
                var SuccessNotification = Notifications.CreateNotification("SuccessNotification", "URL Copied", "Success", false);
                SuccessNotification.ShowHideNotification(true);
            }
                
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

                    var SuccessNotification = Notifications.CreateNotification("SuccessNotification", "Image Favorited", "Success", false);
                    SuccessNotification.ShowHideNotification(true);
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

                    var SuccessNotification = Notifications.CreateNotification("SuccessNotification", "Image Unfavorited", "Success", false);
                    SuccessNotification.ShowHideNotification(true);
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

        private async void CategoryAppButton_Click(object sender, RoutedEventArgs e)
        {
            var img = GetSelectedImage();
            if (img != null)
            {
                CategoryList.Clear();
                var giphy_id = img.Name;

                var SelectedCategories = await GifologyDatabase.GetImageCategory(giphy_id);
                var AllCategories = await GifologyDatabase.GetCategoryList();

                for (int i = 0; i < AllCategories.Count; i++)
                {
                    CategoryList.Add(new CategoryListItem
                    {
                        Id = (int)AllCategories[i].Id,
                        Name = AllCategories[i].Name,
                        IsChecked = SelectedCategories.IndexOf((int)AllCategories[i].Id) != -1
                    });
                }

                CategoryListControl.SelectedCategories = SelectedCategories;
                CategoryListControl.SelectedImage = img;
                CategoryListControl.CategoryList = CategoryList;
                CategoryListControl.Visibility = Visibility.Visible;
            }
        }

        private async void EditCategoryAppButton_Click(object sender, RoutedEventArgs e)
        {
            var CurrentCategory = (Categories)CategoryBox.SelectedValue;

            TextBox inputTextBox = new TextBox();
            inputTextBox.Text = CurrentCategory.Name;
            inputTextBox.AcceptsReturn = false;
            inputTextBox.Height = 32;
            ContentDialog dialog = new ContentDialog();
            dialog.Content = inputTextBox;
            dialog.Title = "Edit Category";
            dialog.IsSecondaryButtonEnabled = true;
            dialog.PrimaryButtonText = "Update";
            dialog.SecondaryButtonText = "Cancel";
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                if (inputTextBox.Text.Trim() != "")
                {
                    CurrentCategory.Name = inputTextBox.Text.Trim();
                    GifologyDatabase.InsertUpdateCategory(CurrentCategory);

                    LoadCategoryList(0);
                }
            }
        }

        private async void DeleteCategoryAppButton_Click(object sender, RoutedEventArgs e)
        {
            var CurrentCategory = (Categories)CategoryBox.SelectedValue;
            NotificationControl SuccessNotification;

            DeleteCategoryDialog dialog = new DeleteCategoryDialog();
            await dialog.ShowAsync();

            switch (dialog.Result)
            {
                case Results.Delete:
                    //Remove all favorites in category
                    GifologyDatabase.DeleteImageInCategory(CurrentCategory);
                    GifologyDatabase.DeleteCategories(CurrentCategory);
                    SuccessNotification = Notifications.CreateNotification("SuccessNotification", "Category Deleted", "Success", false);
                    SuccessNotification.ShowHideNotification(true);
                    break;
                case Results.Keep:
                    //Don't delete favorites
                    GifologyDatabase.MoveImageToUncategorized(CurrentCategory);
                    GifologyDatabase.DeleteCategories(CurrentCategory);
                    SuccessNotification = Notifications.CreateNotification("SuccessNotification", "Category Deleted", "Success", false);
                    SuccessNotification.ShowHideNotification(true);
                    break;
                case Results.Cancel:
                    return;
                    break;
                default:
                    break;
            }

            LoadCategoryList(0);
        }

        private void SettingAppButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage), PivotNavigation.SelectedIndex); 
        }
        #endregion

        private bool InternetStatus()
        {
            switch (Global.CheckInternetConnection())
            {
                case "None":
                    if (MeteredNotification != null && MeteredNotification.GetHeight() > 0)
                        MeteredNotification.DestroyNotification();
                    if (NoInternetConnection == null)
                        NoInternetConnection = this.Notifications.CreateNotification("NoInternetNotification", "No internet connection", "Error", false);
                    if (NoInternetConnection != null && NoInternetConnection.GetHeight() == 0)
                        NoInternetConnection.ShowNotification();
                    return false;
                    break;
                case "Metered":
                    if (NoInternetConnection != null && NoInternetConnection.GetHeight() > 0)
                        NoInternetConnection.DestroyNotification();
                    if (ConnectionDismiss == false)
                    {
                        if (MeteredNotification == null)
                            MeteredNotification = this.Notifications.CreateNotification("MeteredNotification", "On metered connection", "Warning");
                        if (MeteredNotification != null && MeteredNotification.GetHeight() == 0)
                            MeteredNotification.ShowNotification();
                    }
                    break;
                case "Wifi":
                    ConnectionDismiss = false;
                    if (NoInternetConnection != null && NoInternetConnection.GetHeight() > 0)
                        NoInternetConnection.DestroyNotification();
                    if (MeteredNotification != null && MeteredNotification.GetHeight() > 0)
                        MeteredNotification.DestroyNotification();
                    break;
                default:
                    break;
            }

            return true;
        }

        private void ShowSingleImageIcons()
        {
            var img = GetSelectedImage();
            if (img == null)
                return;

            if (GifologyDatabase.GetFavorite(img.Name) != null)
            {
                UnfavoriteAppButton.Visibility = Visibility.Visible;
                FavoriteAppButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                UnfavoriteAppButton.Visibility = Visibility.Collapsed;
                FavoriteAppButton.Visibility = Visibility.Visible;
            }

            IsImageSelected = true;
        }

        private void ShowFullListIcons()
        {
            IsImageSelected = false;
        }

        private async void LoadCategoryList(int SelectedIndex)
        {
            Categories = await GifologyDatabase.GetCategories();
            CategoryBox.ItemsSource = Categories;
            CategoryBox.SelectedIndex = SelectedIndex;
        }

        private string GetListUri(string url)
        {
            return Uri.IsWellFormedUriString(GiphyImage.ConvertSourceType(url, "downscale"), UriKind.Absolute) ?
                        GiphyImage.ConvertSourceType(url, "downscale") :
                        GiphyImage.ConvertSourceType(url, "fixed_width");
        }
    }
}
