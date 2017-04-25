using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Gifology.Database;

namespace Gifology
{
    public sealed partial class CategorySelectionControl : UserControl
    {
        private CategorySelectionControl _this;
        public static readonly DependencyProperty SelectedImageProperty =
            DependencyProperty.Register("SelectedImage", typeof(Image), typeof(CategorySelectionControl), null);

        public Image SelectedImage
        {
            get { return GetValue(SelectedImageProperty) as Image; }
            set { SetValue(SelectedImageProperty, value); }
        }

        public static readonly DependencyProperty CategoryListProperty =
            DependencyProperty.Register("CategoryList", typeof(ObservableCollection<CategoryListItem>), typeof(CategorySelectionControl), null);

        public ObservableCollection<CategoryListItem> CategoryList
        {
            get { return GetValue(CategoryListProperty) as ObservableCollection<CategoryListItem>; }
            set { CategoryCheckList.ItemsSource = value; }
        }

        public List<int> SelectedCategories = new List<int>();

        public CategorySelectionControl()
        {
            this.InitializeComponent();
            _this = this;
        }

        private async void ReloadCategories()
        {
            var List  = await GifologyDatabase.GetCategoryList();
            var NewList = new ObservableCollection<CategoryListItem>();

            for (int i = 0; i < List.Count; i++)
            {
                NewList.Add(new CategoryListItem
                {
                    Id = (int)List[i].Id,
                    Name = List[i].Name,
                    IsChecked = SelectedCategories.IndexOf((int)List[i].Id) != -1
                });
            }

            CategoryList = NewList;
        }

        private void Category_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            int x = 0;
            if (cb.IsChecked == true)
            {
                if (int.TryParse(cb.Name, out x))
                {
                    SelectedCategories.Add(x);
                }
            }
            else
            {
                if (int.TryParse(cb.Name, out x))
                {
                    SelectedCategories.Remove(x);
                }
            }
        }

        private async void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            TextBox inputTextBox = new TextBox();
            inputTextBox.AcceptsReturn = false;
            inputTextBox.Height = 32;
            ContentDialog dialog = new ContentDialog();
            dialog.Content = inputTextBox;
            dialog.Title = "Add Category";
            dialog.IsSecondaryButtonEnabled = true;
            dialog.PrimaryButtonText = "Add";
            dialog.SecondaryButtonText = "Cancel";
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                if(inputTextBox.Text.Trim() != "")
                {
                    GifologyDatabase.InsertUpdateCategory(new Categories
                    {
                        Name = inputTextBox.Text.Trim()
                    });

                    ReloadCategories();
                }
            }
        }

        private void SaveCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GifologyDatabase.DeleteFavoriteId(SelectedImage.Name);
                if (SelectedCategories.Count == 0)
                {
                    if (GifologyDatabase.GetFavorite(SelectedImage.Name) != null)
                    {
                        Favorites data = new Favorites
                        {
                            Giphy_Id = SelectedImage.Name,
                            Url = GiphyImage.ConvertSourceType(((BitmapImage)SelectedImage.Source).UriSource.OriginalString, "original"),
                            Category = 1
                        };

                        GifologyDatabase.InsertUpdateFavorite(data);
                    }
                }
                else
                {
                    for (int i = 0; i < SelectedCategories.Count; i++)
                    {
                        Favorites data = new Favorites
                        {
                            Giphy_Id = SelectedImage.Name,
                            Url = GiphyImage.ConvertSourceType(((BitmapImage)SelectedImage.Source).UriSource.OriginalString, "original"),
                            Category = SelectedCategories[i]
                        };
                        GifologyDatabase.InsertUpdateFavorite(data);
                    }
                }
            }
            catch (SQLite.SQLiteException ex)
            {
                SelectedCategories.Clear();
                Debug.WriteLine("DB EXCEPTION: " + ex.Message);
            }

            _this.Visibility = Visibility.Collapsed;
        }

        private void CancelCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            _this.Visibility = Visibility.Collapsed;
        }
    }
}
