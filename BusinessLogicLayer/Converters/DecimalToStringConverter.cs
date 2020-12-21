using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace BusinessLogicLayer.Converters
{
    public class DecimalToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value?.ToString()
                .Replace(".", CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator)
                .Replace(",", CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            string stringValue = (value as string)
                ?.Replace(".", CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator)
                ?.Replace(",", CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator);

            if ((stringValue?.Length ?? 0) > 0 && stringValue[stringValue.Length - 1].ToString() == CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator)
                return Binding.DoNothing;

            if (decimal.TryParse(stringValue, NumberStyles.Number, CultureInfo.CurrentUICulture, out decimal result))
                return result;
            return null;
        }
    }
}
