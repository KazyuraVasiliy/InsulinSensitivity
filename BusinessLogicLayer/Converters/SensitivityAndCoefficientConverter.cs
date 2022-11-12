using System;
using System.Globalization;
using Xamarin.Forms;

namespace BusinessLogicLayer.Converters
{
    public class SensitivityAndCoefficientConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values is object[] array)
                if (array[0] is decimal sensitivity && array[1] is decimal carbohydrateCoefficient)
                    return $"IC: {sensitivity / carbohydrateCoefficient:N1}";
            return "";
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
