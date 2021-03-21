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
    public class GlucoseToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.Eating eating)
            {
                var dateTime = Service.Calculation.DateTimeUnionTimeSpan(eating.DateCreated, eating.InjectionTime);
                var glucose = new List<GlucoseToString>();

                glucose.Add(new GlucoseToString()
                {
                    Glucose = eating.GlucoseStart,
                    Offset = 0
                });

                foreach (var dimension in eating.IntermediateDimensions ?? new List<Models.IntermediateDimension>())
                {
                    glucose.Add(new GlucoseToString()
                    {
                        Glucose = dimension.Glucose,
                        Offset = (int)Math.Round((Service.Calculation.DateTimeUnionTimeSpan(dimension.DimensionDate, dimension.DimensionTime) - dateTime).TotalMinutes, 0, MidpointRounding.AwayFromZero)
                    });
                }

                if (eating.GlucoseEnd != null)
                    glucose.Add(new GlucoseToString()
                    {
                        Glucose = eating.GlucoseEnd.Value,
                        Offset = eating.EndEating != null
                            ? (int)Math.Round((eating.EndEating.Value - dateTime).TotalMinutes, 0, MidpointRounding.AwayFromZero)
                            : 0
                    });

                return "Гликемия: " + string.Join(" - ", glucose
                    .Select(x =>
                        x.ToString()));
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
