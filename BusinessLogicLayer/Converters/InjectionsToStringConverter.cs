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
    public class InjectionsToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.Eating eating)
            {
                var dateTime = Service.Calculation.DateTimeUnionTimeSpan(eating.DateCreated, eating.InjectionTime);
                var injections = eating.Injections?
                    .OrderBy(x =>
                        x.InjectionDate.Date)
                    .ThenBy(x =>
                        x.InjectionTime)
                    .Select(x =>
                        $"{x.BolusDose} ({Math.Round((Service.Calculation.DateTimeUnionTimeSpan(x.InjectionDate, x.InjectionTime) - dateTime).TotalMinutes, 0, MidpointRounding.AwayFromZero)} мин)")
                    .ToList() ?? new List<string>();

                var injectionsToString = injections.Count > 0 ? $" + {string.Join(" + ", injections)}" : "";
                return $"Инъекции: {eating.BolusDoseFact}{injectionsToString}";
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
