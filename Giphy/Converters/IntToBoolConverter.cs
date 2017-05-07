using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Gifology.Converters
{
    public class IntToBoolConverter : IValueConverter

    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value is int && (int)value == 1) ? true : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (value is bool && (bool)value) ? 1 : 0;
        }
    }
}
