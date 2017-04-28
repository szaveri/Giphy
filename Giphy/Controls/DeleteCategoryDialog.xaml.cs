using System;
using System.Collections.Generic;
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
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Gifology.Controls
{
    public enum Results
    {
        Delete,
        Keep,
        Cancel,
        None
    }

    public sealed partial class DeleteCategoryDialog : ContentDialog
    {
        public Results Result { get; set; }

        public DeleteCategoryDialog()
        {
            this.InitializeComponent();
            this.Result = Results.None;
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Result = Results.Delete;
            Dialog.Hide();
        }

        private void KeepBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Result = Results.Keep;
            Dialog.Hide();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Result = Results.Cancel;
            Dialog.Hide();
        }
    }
}
