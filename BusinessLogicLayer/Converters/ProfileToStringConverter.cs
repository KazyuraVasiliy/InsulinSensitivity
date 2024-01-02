using System;
using System.Globalization;
using Xamarin.Forms;

namespace BusinessLogicLayer.Converters
{
    public class ProfileToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is int profile
                ? profile == 0
                    ? "Быстрый"
                    : "Сверхбыстрый"
                : "";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
