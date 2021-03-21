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

        /// <summary>
        /// Видна ли статистика по циклам
        /// </summary>
        public bool IsCycleVisibility =>
            !GlobalParameters.User.Gender &&
            !string.IsNullOrWhiteSpace(Cycle);

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

        private string insulinSensitivity;
        /// <summary>
        /// ФЧИ
        /// </summary>
        public string InsulinSensitivity
        {
            get => insulinSensitivity;
            set
            {
                insulinSensitivity = value;
                OnPropertyChanged();
            }
        }

        private string cycle;
        /// <summary>
        /// Циклы
        /// </summary>
        public string Cycle
        {
            get => cycle;
            set
            {
                cycle = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(IsCycleVisibility));
            }
        }

        private int cycleDay;
        /// <summary>
        /// День цикла
        /// </summary>
        public int CycleDay
        {
            get => cycleDay;
            set
            {
                cycleDay = value;
                OnPropertyChanged(nameof(IncCycleDay));
            }
        }

        /// <summary>
        /// День цикла в расчёте от 1
        /// </summary>
        public int IncCycleDay =>
            CycleDay + 1;

        private string exercise;
        /// <summary>
        /// Упражнения
        /// </summary>
        public string Exercise
        {
            get => exercise;
            set
            {
                exercise = value;
                OnPropertyChanged();
            }
        }

        private string accuracy;
        /// <summary>
        /// Точность
        /// </summary>
        public string Accuracy
        {
            get => accuracy;
            set
            {
                accuracy = value;
                //OnPropertyChanged();
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

        #endregion

        #region Methods

        /// <summary>
        /// Возвращает знак числа в виде строки
        /// </summary>
        /// <param name="value">Число</param>
        /// <returns></returns>
        private string GetSign(int value) =>
            value > 0 ? "+" : "";

        /// <summary>
        /// Инициализирует статистику
        /// </summary>
        private void InitStatistic()
        {
            using (var db = new ApplicationContext(GlobalParameters.DbPath))
            {
                if (!db.Eatings.Any(x => x.InsulinSensitivityFact != null && !x.IsIgnored))
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
                        !x.IsIgnored &&
                        x.InsulinSensitivityFact != null)
                    .Include(x => x.EatingType)
                    .Include(x => x.Exercise)
                        .ThenInclude(x => x.ExerciseType)
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

                // ФЧИ
                var eatingTypeAverages = eatings
                    .GroupBy(x =>
                        x.EatingType)
                    .OrderBy(x =>
                        x.Key.TimeStart)
                    .Select(x =>
                        $"{x.Key.Name}: {Math.Round(x.Average(y => y.InsulinSensitivityFact.Value), 3, MidpointRounding.AwayFromZero)}");

                var min = eatings.Min(x => x.InsulinSensitivityFact.Value);
                string minInformation = $"\nМинимальный ФЧИ: {Math.Round(min, 3, MidpointRounding.AwayFromZero)} от {eatings.Last(x => x.InsulinSensitivityFact == min).DateCreated:dd.MM.yy}";

                var max = eatings.Max(x => x.InsulinSensitivityFact.Value);
                string maxInformation = $"\nМаксимальный ФЧИ: {Math.Round(max, 3, MidpointRounding.AwayFromZero)} от {eatings.Last(x => x.InsulinSensitivityFact == max).DateCreated:dd.MM.yy}";

                InsulinSensitivity = string.Join("\n", eatingTypeAverages) + "\n" + minInformation + maxInformation;
                    
                // Цикл
                if (!GlobalParameters.User.Gender && (cycles?.Count ?? 0) > 0)
                {
                    CycleDay = (int)Math.Round((DateTime.Now.Date - cycles.Last().DateStart.Date).TotalDays, 0, MidpointRounding.AwayFromZero);
                    List<DateTime> dates = new List<DateTime>();

                    for (int i = 0; i < cycles.Count; i++)
                    {
                        var equivalentDay = cycles[i].DateStart.AddDays(CycleDay);
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

                    Cycle = string.Join("\n", eatingTypeCycleAverages);
                }                

                // Активность
                var exercises = eatings
                    .GroupBy(x =>
                        new
                        {
                            x.Exercise.ExerciseType,
                            x.Exercise.HoursAfterInjection
                        })
                    .Select(x =>
                        new
                        {
                            Count = x.Count(),
                            ExerciseType = $"{x.Key.ExerciseType.Name} (через {x.Key.HoursAfterInjection} ч.)",
                            InsulinSensitivityFact = x.Average(y => y.InsulinSensitivityFact)
                        })
                    .OrderBy(x =>
                        x.InsulinSensitivityFact)
                    .ToList();

                var maxElement = (exercises?.Count ?? 0) > 0
                    ? exercises
                        .FirstOrDefault(x =>
                            x.Count == exercises.Max(y => y.Count))
                    : null;

                StringBuilder exercisesInformation = new StringBuilder(exercises.Count);
                for (int i = 0; i < (exercises?.Count ?? 0); i++)
                {
                    if (exercises[i] != maxElement)
                    {
                        var increase = (int)Math.Round((exercises[i].InsulinSensitivityFact.Value / maxElement.InsulinSensitivityFact.Value - 1) * 100, 0, MidpointRounding.AwayFromZero);
                        exercisesInformation.AppendLine($"{exercises[i].ExerciseType} - {Math.Round(exercises[i].InsulinSensitivityFact.Value, 3, MidpointRounding.AwayFromZero)} ({GetSign(increase)}{increase}%)");
                    }
                    else exercisesInformation.AppendLine($"{exercises[i].ExerciseType} - {Math.Round(exercises[i].InsulinSensitivityFact.Value, 3, MidpointRounding.AwayFromZero)}");
                }

                Exercise = exercisesInformation.ToString();

                // Точность
                StringBuilder accuracyInformation = new StringBuilder(9);
                if (eatings.Any(x => x.AccuracyAuto != null || x.AccuracyUser != null))
                {
                    accuracyInformation.AppendLine("Всё время:");

                    if (eatings.Any(x => x.AccuracyAuto != null))
                        accuracyInformation.AppendLine($"Средняя точность программы: {Math.Round(eatings.Where(x => x.AccuracyAuto != null).Average(x => x.AccuracyAuto.Value), 2, MidpointRounding.AwayFromZero)}%");

                    if (eatings.Any(x => x.AccuracyUser != null))
                        accuracyInformation.AppendLine($"Средняя точность пользователя: {Math.Round(eatings.Where(x => x.AccuracyUser != null).Average(x => x.AccuracyUser.Value), 2, MidpointRounding.AwayFromZero)}%");
                        
                    accuracyInformation.AppendLine("\nМесяц:");

                    var month = DateTime.Now.AddMonths(-1).Date;
                    var monthCollection = eatings
                        .Where(x =>
                            x.DateCreated.Date >= month);

                    if (monthCollection.Any(x => x.AccuracyAuto != null))
                        accuracyInformation.AppendLine($"Средняя точность программы: {Math.Round(monthCollection.Where(x => x.AccuracyAuto != null).Average(x => x.AccuracyAuto.Value), 2, MidpointRounding.AwayFromZero)}%");

                    if (monthCollection.Any(x => x.AccuracyUser != null))
                        accuracyInformation.AppendLine($"Средняя точность пользователя: {Math.Round(monthCollection.Where(x => x.AccuracyUser != null).Average(x => x.AccuracyUser.Value), 2, MidpointRounding.AwayFromZero)}%");

                    accuracyInformation.AppendLine("\nНеделя:");

                    var week = DateTime.Now.AddDays(-7).Date;
                    var weekCollection = monthCollection
                        .Where(x =>
                            x.DateCreated.Date >= week);

                    if (weekCollection.Any(x => x.AccuracyAuto != null))
                        accuracyInformation.AppendLine($"Средняя точность программы: {Math.Round(weekCollection.Where(x => x.AccuracyAuto != null).Average(x => x.AccuracyAuto.Value), 2, MidpointRounding.AwayFromZero)}%");

                    if (weekCollection.Any(x => x.AccuracyUser != null))
                        accuracyInformation.AppendLine($"Средняя точность пользователя: {Math.Round(weekCollection.Where(x => x.AccuracyUser != null).Average(x => x.AccuracyUser.Value), 2, MidpointRounding.AwayFromZero)}%");
                }

                Accuracy = accuracyInformation.ToString();
            }
        }

        #endregion
    }
}
