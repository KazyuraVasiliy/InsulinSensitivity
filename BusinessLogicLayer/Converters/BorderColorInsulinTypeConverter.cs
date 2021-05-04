using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace BusinessLogicLayer.Converters
{
    public class BorderColorInsulinTypeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values is object[] array)
                if (array[0] is Guid current && array[1] is Guid equalOne && array[2] is Guid equalTwo)                    
                    if (current == equalOne || current == equalTwo)
                        return Color.Orange;
            return Color.SkyBlue;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
