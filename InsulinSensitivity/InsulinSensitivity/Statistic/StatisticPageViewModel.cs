using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using Microcharts;

using DataAccessLayer.Contexts;
using BusinessLogicLayer.ViewModel;
using BusinessLogicLayer.Service;
using Models = DataAccessLayer.Models;

namespace InsulinSensitivity.Statistic
{
    public class StatisticPageViewModel : ObservableBase
    {
        #region Constructors

        public StatisticPageViewModel()
        {
            // Инициализация коллекций
            InitStatistic();
        }

        #endregion

        #region Properties

        private bool isRefreshing;
        /// <summary>
        /// Указывает на то, что обновление завершено
        /// </summary>
        public bool IsRefreshing
        {
            get => isRefreshing;
            set
            {
                isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }

        private double widthRequest;
        /// <summary>
        /// Ширина
        /// </summary>
        public double WidthRequest
        {
            get => widthRequest;
            set
            {
                widthRequest = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Collections

        public LineChart chart;
        /// <summary>
        /// График
        /// </summary>
        public LineChart Chart
        {
            get => chart;
            set
            {
                chart = value;
                OnPropertyChanged();
            }
        }

        private string information;
        /// <summary>
        /// Дополнительная информация
        /// </summary>
        public string Information
        {
            get => information;
            set
            {
                information = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Инициализирует статистику
        /// </summary>
        private async void InitStatistic()
        {
            AsyncBase.Open("Рассчёт статистики");
            await Task.Run(() =>
            {
                using (var db = new ApplicationContext(GlobalParameters.DbPath))
                {
                    if (!db.Eatings.Any(x => x.GlucoseEnd != null))
                        return;

                    var cycles = db.MenstrualCycles
                        .OrderBy(x =>
                            x.DateStart)
                        .ToList();

                    var ovulations = cycles
                        .Select(x =>
                            x.DateStart.AddDays(-14))
                        .ToList();
                    if ((cycles?.Count ?? 0) > 0)
                        ovulations.Add(cycles.Last().DateStart.AddDays(14));

                    var eatings = db.Eatings
                        .Where(x =>
                            x.InsulinSensitivityFact != null)
                        .Include(x => x.EatingType)
                        .ToList();

                    var basals = eatings
                        .GroupBy(x =>
                            x.DateCreated.Date);

                    var entries = eatings
                        .OrderBy(x =>
                            x.DateCreated.Date)
                        .GroupBy(x =>
                            x.DateCreated.Date)
                        .Select(x =>
                            new ChartEntry((float)x.Average(y => y.InsulinSensitivityFact))
                            {
                                Label = x.Key.ToString("dd.MM.yy"),
                                ValueLabel = $"{Math.Round(x.Average(y => y.InsulinSensitivityFact.Value), 2, MidpointRounding.AwayFromZero)} " +
                                    $"({basals.FirstOrDefault(y => y.Key == x.Key).Sum(y => y.BasalDose)})",
                                ValueLabelColor = App.Current.RequestedTheme == OSAppTheme.Dark
                                    ? SkiaSharp.SKColors.White
                                    : SkiaSharp.SKColors.Black,

                                Color = cycles.Any(y => (x.Key.Date - y.DateStart.Date).TotalDays <= 2 && (x.Key.Date - y.DateStart.Date).TotalDays >= 0)
                                    ? SkiaSharp.SKColors.Red
                                    : ovulations.Any(y => y.Date == x.Key.Date)
                                        ? SkiaSharp.SKColors.Pink
                                        : App.Current.RequestedTheme == OSAppTheme.Dark
                                            ? SkiaSharp.SKColors.LightSkyBlue
                                            : SkiaSharp.SKColors.Blue
                            })
                        .ToList();

                    WidthRequest = (entries?.Count() ?? 0) * 15;

                    App.Current.Dispatcher.BeginInvokeOnMainThread(() =>
                    {
                        Chart = new LineChart()
                        {
                            LineMode = LineMode.Spline,
                            LabelTextSize = 40,
                            Entries = entries,

                            LabelColor = App.Current.RequestedTheme == OSAppTheme.Dark
                                ? SkiaSharp.SKColors.White
                                : SkiaSharp.SKColors.Black,
                            BackgroundColor = App.Current.RequestedTheme == OSAppTheme.Dark
                                ? new SkiaSharp.SKColor(29, 29, 29)
                                : SkiaSharp.SKColors.White,
                        };
                    });

                    List<string> informations = new List<string>();
                    informations.Add("Средние ФЧИ по приёмам пищи");

                    var eatingTypeAverages = eatings
                        .GroupBy(x =>
                            x.EatingType)
                        .OrderBy(x =>
                            x.Key.TimeStart)
                        .Select(x =>
                            $"{x.Key.Name}: {Math.Round(x.Average(y => y.InsulinSensitivityFact.Value), 3, MidpointRounding.AwayFromZero)}");
                    informations.AddRange(eatingTypeAverages);

                    if (!GlobalParameters.User.Gender && (cycles?.Count ?? 0) > 0)
                    {
                        informations.Add("\nСредние ФЧИ по текущему дню цикла");

                        var day = (DateTime.Now - cycles.Last().DateStart).TotalDays;
                        List<DateTime> dates = new List<DateTime>();

                        for (int i = 0; i < cycles.Count; i++)
                        {
                            var equivalentDay = cycles[i].DateStart.AddDays(day);
                            if ((i != (cycles.Count - 1)) && equivalentDay.Date < cycles[i + 1].DateStart.Date)
                                dates.Add(equivalentDay);

                            if (i == (cycles.Count - 1))
                                dates.Add(equivalentDay);
                        }

                        var eatingTypeCycleAverages = eatings
                            .Where(x =>
                                dates.Any(y => y.Date == x.DateCreated.Date))
                            .GroupBy(x =>
                                x.EatingType)
                            .OrderBy(x =>
                                x.Key.TimeStart)
                            .Select(x =>
                                $"{x.Key.Name}: {Math.Round(x.Average(y => y.InsulinSensitivityFact.Value), 3, MidpointRounding.AwayFromZero)}");
                        informations.AddRange(eatingTypeCycleAverages);
                    }

                    if (eatings.Any(x => x.AccuracyAuto != null || x.AccuracyUser != null))
                    {
                        informations.Add("\n");
                        if (eatings.Any(x => x.AccuracyAuto != null))
                            informations.Add($"Средняя точность программы: {Math.Round(eatings.Where(x => x.AccuracyAuto != null).Average(x => x.AccuracyAuto.Value), 2, MidpointRounding.AwayFromZero)}%");

                        if (eatings.Any(x => x.AccuracyUser != null))
                            informations.Add($"Средняя точность пользователя: {Math.Round(eatings.Where(x => x.AccuracyUser != null).Average(x => x.AccuracyUser.Value), 2, MidpointRounding.AwayFromZero)}%");
                    }

                    var min = eatings.Min(x => x.InsulinSensitivityFact.Value);
                    informations.Add($"\nМинимальный ФЧИ: {Math.Round(min, 3, MidpointRounding.AwayFromZero)} от {eatings.Last(x => x.InsulinSensitivityFact == min).DateCreated:dd.MM.yy}");

                    var max = eatings.Max(x => x.InsulinSensitivityFact.Value);
                    informations.Add($"Максимальный ФЧИ: {Math.Round(max, 3, MidpointRounding.AwayFromZero)} от {eatings.Last(x => x.InsulinSensitivityFact == max).DateCreated:dd.MM.yy}");

                    Information = string.Join("\n", informations);
                }
            });
            AsyncBase.Close();
        }

        #endregion
    }
}
