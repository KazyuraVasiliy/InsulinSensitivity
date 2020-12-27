using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace BusinessLogicLayer.Converters
{
    public class ExpectedGlucoseBoolConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values is object[] array)
                if (array[0] is decimal target && array[1] is decimal current)
                    return target + 1.5M >= current && target - 1.5M <= current;
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
