using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Xamarin.Forms;

using Models = DataAccessLayer.Models;
using BusinessLogicLayer.Service.Models;

namespace BusinessLogicLayer.Converters
{
    public class InjectionsToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.Eating eating)
            {
                var dateTime = Service.Calculation.DateTimeUnionTimeSpan(eating.DateCreated, eating.InjectionTime);
                var injections = new List<InjectionToString>();

                injections.Add(new InjectionToString()
                {
                    Dose = eating.BolusDoseFact,
                    Offset = 0,
                    Name = eating.BolusType?.Name ?? "Инъекции"
                });

                foreach (var injection in eating.Injections ?? new List<Models.Injection>())
                {
                    injections.Add(new InjectionToString()
                    {
                        Dose = injection.BolusDose,
                        Offset = (int)Math.Round((Service.Calculation.DateTimeUnionTimeSpan(injection.InjectionDate, injection.InjectionTime) - dateTime).TotalMinutes, 0, MidpointRounding.AwayFromZero),
                        Name = injection.BolusType?.Name ?? "Инъекции"
                    });
                }

                return string.Join("\n", injections
                    .GroupBy(x =>
                        x.Name)
                    .OrderBy(x =>
                        x.Key)
                    .Select(x =>
                        $"{x.Key}: {string.Join(" + ", x.OrderBy(y => y.Offset).Select(y => y.ToString()))}"));
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
