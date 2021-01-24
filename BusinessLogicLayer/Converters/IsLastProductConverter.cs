using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace BusinessLogicLayer.Converters
{
    public class IsLastProductConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values is object[] array)
                if (array[0] is Guid current && array[1] is Guid last)
                    return array.Length > 2 && array[2] is bool invert && invert
                        ? current != last
                        : current == last;
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
