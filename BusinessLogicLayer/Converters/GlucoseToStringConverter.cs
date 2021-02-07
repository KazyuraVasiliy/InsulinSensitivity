using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Xamarin.Forms;

using Models = DataAccessLayer.Models;

namespace BusinessLogicLayer.Converters
{
    public class GlucoseToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.Eating eating)
            {
                var dimensions = eating.IntermediateDimensions?
                    .OrderBy(x =>
                        x.DimensionDate.Date)
                    .ThenBy(x =>
                        x.DimensionTime)
                    .Select(x =>
                        x.Glucose)
                    .ToList() ?? new List<decimal>();

                var dimensionToString = dimensions.Count > 0 ? $" - {string.Join(" - ", dimensions)}" : "";
                return $"Гликемия: {eating.GlucoseStart}{dimensionToString} - {eating.GlucoseEnd}";
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
