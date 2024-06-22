using BusinessLogicLayer.Service;
using BusinessLogicLayer.ViewModel;
using DataAccessLayer.Contexts;
using Microcharts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace InsulinSensitivity.Statistic
{
    public class StatisticPageViewModel : ObservableBase
    {
        #region Constructors

        public StatisticPageViewModel(Statistic.StatisticPage page)
        {
            Page = page;

            // Инициализация коллекций
            _ = InitStatistic();
        }

        #endregion

        #region Properties

        public Statistic.StatisticPage Page { get; private set; }

        /// <summary>
        /// Видна ли статистика по циклам
        /// </summary>
        public bool IsCycleChartVisibility =>
            GlobalParameters.IsCycleSettingsAccess &&
            !GlobalParameters.User.IsPregnancy &&
            !string.IsNullOrWhiteSpace(Cycle);

        /// <summary>
        /// Видна ли статистика по беременности
        /// </summary>
        public bool IsPregnancyChartVisibility =>
            GlobalParameters.IsCycleSettingsAccess &&
            GlobalParameters.User.IsPregnancy;

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

        private double widthRequestWeeks;
        /// <summary>
        /// Ширина недельного графика
        /// </summary>
        public double WidthRequestWeeks
        {
            get => widthRequestWeeks;
            set
            {
                widthRequestWeeks = value;
                OnPropertyChanged();
            }
        }

        private double widthRequestHours;
        /// <summary>
        /// Ширина часового графика
        /// </summary>
        public double WidthRequestHours
        {
            get => widthRequestHours;
            set
            {
                widthRequestHours = value;
                OnPropertyChanged();
            }
        }

        private double cycleWidthRequest;
        /// <summary>
        /// Ширина графика ФЧИ по циклу
        /// </summary>
        public double CycleWidthRequest
        {
            get => cycleWidthRequest;
            set
            {
                cycleWidthRequest = value;
                OnPropertyChanged();
            }
        }

        private double pregnancyWidthRequest;
        /// <summary>
        /// Ширина графика ФЧИ по беременности
        /// </summary>
        public double PregnancyWidthRequest
        {
            get => pregnancyWidthRequest;
            set
            {
                pregnancyWidthRequest = value;
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

        private string carbohydrateCoefficient;
        /// <summary>
        /// Соотношение углеводов к инсулину
        /// </summary>
        public string CarbohydrateCoefficient
        {
            get => carbohydrateCoefficient;
            set
            {
                carbohydrateCoefficient = value;
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

                OnPropertyChanged(nameof(IsCycleChartVisibility));
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

        private int pregnancyWeek;
        /// <summary>
        /// Неделя беременности
        /// </summary>
        public int PregnancyWeek
        {
            get => pregnancyWeek;
            set
            {
                pregnancyWeek = value;
                OnPropertyChanged(nameof(IncPregnancyWeek));
            }
        }

        private DateTime lastMenstruationDate;
        /// <summary>
        /// Неделя беременности
        /// </summary>
        public DateTime LastMenstruationDate
        {
            get => lastMenstruationDate;
            set
            {
                lastMenstruationDate = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// День цикла в расчёте от 1
        /// </summary>
        public int IncCycleDay =>
            CycleDay + 1;

        /// <summary>
        /// Неделя беременности в расчёте от 1
        /// </summary>
        public int IncPregnancyWeek =>
            PregnancyWeek + 1;

        private Dictionary<string, string> exercise = new Dictionary<string, string>();
        /// <summary>
        /// Упражнения
        /// </summary>
        public Dictionary<string, string> Exercise
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

        public LineChart chartWeeks;
        /// <summary>
        /// График недельный
        /// </summary>
        public LineChart ChartWeeks
        {
            get => chartWeeks;
            set
            {
                chartWeeks = value;
                OnPropertyChanged();
            }
        }

        public LineChart chartHours;
        /// <summary>
        /// График часовой
        /// </summary>
        public LineChart ChartHours
        {
            get => chartHours;
            set
            {
                chartHours = value;
                OnPropertyChanged();
            }
        }

        public LineChart chartICPerHours;
        /// <summary>
        /// График IC часовой
        /// </summary>
        public LineChart ChartICPerHours
        {
            get => chartICPerHours;
            set
            {
                chartICPerHours = value;
                OnPropertyChanged();
            }
        }

        public LineChart cycleChart;
        /// <summary>
        /// График ФЧИ по дням цикла
        /// </summary>
        public LineChart CycleChart
        {
            get => cycleChart;
            set
            {
                cycleChart = value;
                OnPropertyChanged();
            }
        }

        public LineChart pregnancyChart;
        /// <summary>
        /// График ФЧИ по беременности
        /// </summary>
        public LineChart PregnancyChart
        {
            get => pregnancyChart;
            set
            {
                pregnancyChart = value;
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
        private async Task InitStatistic() =>
            await AsyncBase.NewTask(async () =>
            {
                using (var db = new ApplicationContext(GlobalParameters.DbPath))
                {
                    if (!(await db.Eatings.AnyAsync(x => x.InsulinSensitivityFact != null && !x.IsIgnored)))
                        return;

                    // Менструальные циклы
                    var cycles = await db.MenstrualCycles
                        .AsNoTracking()
                        .OrderBy(x => x.DateStart)
                        .ToListAsync();

                    // Дни овуляции
                    var ovulations = cycles
                        .Select(x => x.DateStart.AddDays(-14))
                        .ToList();

                    if ((cycles?.Count ?? 0) > 0)
                        ovulations.Add(cycles.Last().DateStart.AddDays(14));

                    // Периоды усвоения
                    var eatings = await db.Eatings
                        .AsNoTracking()
                        .Where(x => x.InsulinSensitivityFact != null)
                        .Include(x => x.EatingType)
                        .Include(x => x.Exercise)
                            .ThenInclude(x => x.ExerciseType)
                        .ToListAsync();

                    // Кол-во базального инсулина по дням
                    var basals = eatings
                        .GroupBy(x => x.DateCreated.Date)
                        .ToList();

                    var basalsPerDay = new List<BasalDose>();
                    foreach (var el in basals)
                    {
                        var value = el.Sum(x => x.BasalDose * (x.BasalType?.Concentration ?? 1));

                        var rateCollection = el.Where(x =>
                            x.BasalRate != 0);

                        if (rateCollection.Count() != 0)
                            value += Math.Round(rateCollection.Average(x => x.BasalRate) * 24, 1, MidpointRounding.AwayFromZero);

                        basalsPerDay.Add(new BasalDose()
                        {
                            Date = el.Key,
                            Dose = value
                        });
                    }

                    // Исключаем игнорируемые периоды усвоения
                    eatings = eatings
                        .Where(x => !x.IsIgnored)
                        .ToList();

                    // Инициализация графика ФЧИ по дням
                    InitInsuinSensitivityChartPerDay(eatings, basalsPerDay, cycles, ovulations);

                    // Инициализация графика ФЧИ по неделям
                    InitInsuinSensitivityChartPerWeek(eatings, basalsPerDay);

                    // Инициализация графика ФЧИ по часам
                    InitInsuinSensitivityChartPerHour(eatings);

                    // Инициализация графика IC по часам
                    InitICChartPerHour(ChartHours);

                    // Цикл
                    if (GlobalParameters.IsCycleSettingsAccess && !GlobalParameters.User.IsPregnancy && (cycles?.Count ?? 0) > 0)
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
                                new
                                {
                                    x.EatingType.Name,
                                    x.EatingType.TimeStart
                                })
                            .OrderBy(x =>
                                x.Key.TimeStart)
                            .Select(x =>
                                $"{x.Key.Name}: {Math.Round(x.Average(y => y.InsulinSensitivityFact.Value), 3, MidpointRounding.AwayFromZero)}");

                        Cycle = string.Join("\n", eatingTypeCycleAverages);
                    }

                    // График ФЧИ по циклу
                    if (GlobalParameters.IsCycleSettingsAccess && !GlobalParameters.User.IsPregnancy && (cycles?.Count ?? 0) > 0)
                    {
                        var values = new List<(int day, decimal value, decimal baseDose)>();

                        for (int i = -10; i < GlobalParameters.User.LengthGraph; i++)
                        {
                            List<DateTime> dates = new List<DateTime>();

                            for (int j = 0; j < cycles.Count; j++)
                            {
                                var equivalentDay = cycles[j].DateStart.AddDays(i);
                                if (i >= 0)
                                {
                                    if ((j != (cycles.Count - 1)) && equivalentDay.Date < cycles[j + 1].DateStart.Date)
                                        dates.Add(equivalentDay);

                                    if (j == (cycles.Count - 1))
                                        dates.Add(equivalentDay);
                                }
                                else
                                {
                                    if (j == 0)
                                        dates.Add(equivalentDay);

                                    if (j != 0 && equivalentDay.Date >= cycles[j - 1].DateStart.Date)
                                        dates.Add(equivalentDay);
                                }
                            }

                            var data = eatings
                                .Where(x =>
                                    dates.Any(y => y.Date == x.DateCreated.Date));

                            var basalsCycle = basalsPerDay
                                .Where(x =>
                                    dates.Any(y => y.Date == x.Date));

                            if (data.Count() > 0)
                                values.Add((
                                    i >= 0 ? i + 1 : i,
                                    Math.Round(data
                                        .Average(x =>
                                            x.InsulinSensitivityFact.Value), 3, MidpointRounding.AwayFromZero),
                                    basalsCycle.Count() > 0
                                        ? Math.Round(basalsCycle
                                            .Average(x =>
                                                x.Dose), 1, MidpointRounding.AwayFromZero)
                                        : 0));
                        }

                        if (values.Count > 0)
                        {
                            CycleWidthRequest = (values?.Count() ?? 0) * 15;

                            CycleChart = new LineChart()
                            {
                                LineMode = LineMode.Spline,
                                LabelTextSize = 24,
                                Entries = values
                                    .Select(x =>
                                        new ChartEntry((float)x.value)
                                        {
                                            Label = x.day.ToString("D2"),
                                            ValueLabel = $"{x.value} " +
                                                $"({x.baseDose})",
                                            ValueLabelColor = App.Current.RequestedTheme == OSAppTheme.Dark
                                                ? SkiaSharp.SKColors.White
                                                : SkiaSharp.SKColors.Black,

                                            Color = x.day == CycleDay + 1
                                                ? SkiaSharp.SKColors.Green
                                                : x.day >= 1 && x.day <= 3
                                                    ? SkiaSharp.SKColors.Red
                                                    : x.day == 15
                                                        ? SkiaSharp.SKColors.Pink
                                                        : App.Current.RequestedTheme == OSAppTheme.Dark
                                                            ? SkiaSharp.SKColors.LightSkyBlue
                                                            : SkiaSharp.SKColors.Blue
                                        }),

                                LabelColor = App.Current.RequestedTheme == OSAppTheme.Dark
                                    ? SkiaSharp.SKColors.White
                                    : SkiaSharp.SKColors.Black,
                                BackgroundColor = App.Current.RequestedTheme == OSAppTheme.Dark
                                    ? new SkiaSharp.SKColor(29, 29, 29)
                                    : SkiaSharp.SKColors.White,
                                MinValue = (float)values.Min(x => x.value) - 0.5f
                            };
                        }
                    }

                    // График ФЧИ по беременности
                    if (GlobalParameters.IsCycleSettingsAccess && GlobalParameters.User.IsPregnancy && (cycles?.Count ?? 0) > 0)
                    {
                        // Дата последней менструации
                        LastMenstruationDate = cycles.Last().DateStart.Date;

                        var pregnancyBasalWeeks = basalsPerDay
                            .Where(x => x.Date >= LastMenstruationDate)
                            .GroupBy(x => (int)(x.Date - LastMenstruationDate).TotalDays / 7)
                            .Select(x => (
                                x.Key,
                                x.Count() > 0 ? Math.Round(x.Average(y => y.Dose), 1, MidpointRounding.AwayFromZero) : 0));

                        PregnancyWeek = (int)(DateTime.Now.Date - LastMenstruationDate).TotalDays / 7;
                        var pregnancyWeeks = eatings
                            .Where(x => x.DateCreated.Date >= LastMenstruationDate)
                            .GroupBy(x => (int)(x.DateCreated.Date - LastMenstruationDate).TotalDays / 7)
                            .OrderBy(x => x.Key)
                            .Select(x =>
                                new ChartEntry((float)x.Average(y => y.InsulinSensitivityFact))
                                {
                                    Label = (x.Key + 1).ToString(),
                                    ValueLabel = $"{Methods.Round(x.Average(y => y.InsulinSensitivityFact.Value), 2)} " +
                                        $"({pregnancyBasalWeeks.FirstOrDefault(y => y.Key == x.Key).Item2})",
                                    ValueLabelColor = App.Current.RequestedTheme == OSAppTheme.Dark
                                        ? SkiaSharp.SKColors.White
                                        : SkiaSharp.SKColors.Black,

                                    Color = PregnancyWeek == x.Key
                                        ? SkiaSharp.SKColors.Green
                                        : App.Current.RequestedTheme == OSAppTheme.Dark
                                            ? SkiaSharp.SKColors.LightSkyBlue
                                            : SkiaSharp.SKColors.Blue
                                })
                            .ToList();

                        PregnancyWidthRequest = (pregnancyWeeks?.Count() ?? 0) * 15;

                        PregnancyChart = new LineChart()
                        {
                            LineMode = LineMode.Spline,
                            LabelTextSize = 24,
                            Entries = pregnancyWeeks,

                            LabelColor = App.Current.RequestedTheme == OSAppTheme.Dark
                                ? SkiaSharp.SKColors.White
                                : SkiaSharp.SKColors.Black,
                            BackgroundColor = App.Current.RequestedTheme == OSAppTheme.Dark
                                ? new SkiaSharp.SKColor(29, 29, 29)
                                : SkiaSharp.SKColors.White,
                        };
                    }

                    // Активность
                    var exercises = eatings
                        .GroupBy(x =>
                            new
                            {
                                x.Exercise.ExerciseType.Id,
                                x.Exercise.ExerciseType.Name,
                                x.Exercise.HoursAfterInjection
                            })
                        .Select(x =>
                            new
                            {
                                Count = x.Count(),
                                LastDate = x.Max(y => y.DateCreated.Date),
                                ExerciseType = $"{x.Key.Name} (через {x.Key.HoursAfterInjection} ч.)",
                                InsulinSensitivityFact = x.Average(y => y.InsulinSensitivityFact)
                            })
                        .Where(x =>
                            x.Count > 2 &&
                            x.LastDate > DateTime.Now.AddMonths(-3).Date)
                        .OrderBy(x =>
                            x.InsulinSensitivityFact)
                        .ToList();

                    var maxElement = (exercises?.Count ?? 0) > 0
                        ? exercises
                            .FirstOrDefault(x =>
                                x.Count == exercises.Max(y => y.Count))
                        : null;

                    for (int i = 0; i < (exercises?.Count ?? 0); i++)
                    {
                        if (exercises[i] != maxElement)
                        {
                            var increase = (int)Math.Round((exercises[i].InsulinSensitivityFact.Value / maxElement.InsulinSensitivityFact.Value - 1) * 100, 0, MidpointRounding.AwayFromZero);
                            Exercise.Add(exercises[i].ExerciseType, $"{Math.Round(exercises[i].InsulinSensitivityFact.Value, 3, MidpointRounding.AwayFromZero)} ({GetSign(increase)}{increase}%)");
                        }
                        else Exercise.Add(exercises[i].ExerciseType, $"{Math.Round(exercises[i].InsulinSensitivityFact.Value, 3, MidpointRounding.AwayFromZero)}");
                    }

                    Exercise = new Dictionary<string, string>(Exercise);

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

                Device.BeginInvokeOnMainThread(async () =>
                    await Page.sv.ScrollToAsync(WidthRequest, 0, true));                

            }, "Инициализация\nПожалуйста, подождите");

        /// <summary>
        /// Инициализация графика ФЧИ по дням
        /// </summary>
        /// <param name="eatings"></param>
        /// <param name="basalsPerDay"></param>
        /// <param name="cycles"></param>
        /// <param name="ovulations"></param>
        /// <returns></returns>
        private void InitInsuinSensitivityChartPerDay(List<DataAccessLayer.Models.Eating> eatings, List<BasalDose> basalsPerDay, List<DataAccessLayer.Models.MenstrualCycle> cycles, List<DateTime> ovulations)
        {
            var lastYear = DateTime.Now.AddYears(-1).Date;
            var entries = eatings
                .Where(x => x.DateCreated.Date >= lastYear)
                .OrderBy(x => x.DateCreated.Date)
                .GroupBy(x => x.DateCreated.Date)
                .Select(x =>
                    new ChartEntry((float)x.Average(y => y.InsulinSensitivityFact))
                    {
                        Label = x.Key.ToString("dd.MM.yy"),
                        ValueLabel = $"{Math.Round(x.Average(y => y.InsulinSensitivityFact.Value), 2, MidpointRounding.AwayFromZero)} " +
                            $"({basalsPerDay.FirstOrDefault(y => y.Date == x.Key).Dose})",
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
                LabelTextSize = 24,
                Entries = entries,

                LabelColor = App.Current.RequestedTheme == OSAppTheme.Dark
                    ? SkiaSharp.SKColors.White
                    : SkiaSharp.SKColors.Black,
                BackgroundColor = App.Current.RequestedTheme == OSAppTheme.Dark
                    ? new SkiaSharp.SKColor(29, 29, 29)
                    : SkiaSharp.SKColors.White,
            };

            // Периоды усвоения за последний месяц
            var lastMonth = DateTime.Now.AddMonths(-1).Date;
            var eatingsLastMonth = eatings
                .Where(x => x.DateCreated.Date >= lastMonth);

            // ФЧИ
            if (eatingsLastMonth.Count() > 0)
            {
                var min = eatingsLastMonth.Min(x => x.InsulinSensitivityFact.Value);
                string minInformation = $"\nМинимальный ФЧИ: {Math.Round(min, 3, MidpointRounding.AwayFromZero)} от {eatingsLastMonth.Last(x => x.InsulinSensitivityFact == min).DateCreated:dd.MM.yy}";

                var max = eatingsLastMonth.Max(x => x.InsulinSensitivityFact.Value);
                string maxInformation = $"\nМаксимальный ФЧИ: {Math.Round(max, 3, MidpointRounding.AwayFromZero)} от {eatingsLastMonth.Last(x => x.InsulinSensitivityFact == max).DateCreated:dd.MM.yy}";

                InsulinSensitivity = minInformation + maxInformation;
            }
        }

        /// <summary>
        /// Инициализация графика ФЧИ по неделям
        /// </summary>
        /// <param name="eatings"></param>
        /// <param name="basalsPerDay"></param>
        /// <returns></returns>
        private void InitInsuinSensitivityChartPerWeek(List<DataAccessLayer.Models.Eating> eatings, List<BasalDose> basalsPerDay)
        {
            var basalsCalculateWeeks = basalsPerDay
                .GroupBy(x => (x.Date.DayOfYear - 1) / 7)
                .Select(x => (
                    x.Key,
                    x.Count() > 0 ? Math.Round(x.Average(y => y.Dose), 1, MidpointRounding.AwayFromZero) : 0));

            var currentWeek = (DateTime.Now.DayOfYear - 1) / 7;
            var entriesWeeks = eatings
                .GroupBy(x => (x.DateCreated.Date.DayOfYear - 1) / 7)
                .OrderBy(x => x.Key)
                .Select(x =>
                    new ChartEntry((float)x.Average(y => y.InsulinSensitivityFact))
                    {
                        Label = x.Key.ToString(),
                        ValueLabel = $"{Math.Round(x.Average(y => y.InsulinSensitivityFact.Value), 2, MidpointRounding.AwayFromZero)} " +
                            $"({basalsCalculateWeeks.FirstOrDefault(y => y.Key == x.Key).Item2})",
                        ValueLabelColor = App.Current.RequestedTheme == OSAppTheme.Dark
                            ? SkiaSharp.SKColors.White
                            : SkiaSharp.SKColors.Black,

                        Color = currentWeek == x.Key
                            ? SkiaSharp.SKColors.Green
                            : App.Current.RequestedTheme == OSAppTheme.Dark
                                ? SkiaSharp.SKColors.LightSkyBlue
                                : SkiaSharp.SKColors.Blue
                    })
                .ToList();

            WidthRequestWeeks = (entriesWeeks?.Count() ?? 0) * 15;

            ChartWeeks = new LineChart()
            {
                LineMode = LineMode.Spline,
                LabelTextSize = 24,
                Entries = entriesWeeks,

                LabelColor = App.Current.RequestedTheme == OSAppTheme.Dark
                    ? SkiaSharp.SKColors.White
                    : SkiaSharp.SKColors.Black,
                BackgroundColor = App.Current.RequestedTheme == OSAppTheme.Dark
                    ? new SkiaSharp.SKColor(29, 29, 29)
                    : SkiaSharp.SKColors.White,
            };
        }

        /// <summary>
        /// Инициализация графика ФЧИ по часам
        /// </summary>
        /// <param name="eatings"></param>
        /// <returns></returns>
        private void InitInsuinSensitivityChartPerHour(List<DataAccessLayer.Models.Eating> eatings)
        {
            var dataForHours = new Dictionary<int, List<decimal>>();
            for (int i = 0; i < 24; i++)
                dataForHours.Add(i, new List<decimal>());

            var lastFiveDays = DateTime.Now.AddDays(-5);

            var eatingsForHours = eatings
                .Where(x =>
                    x.InsulinSensitivityFact != null &&
                    x.DateCreated.Date >= lastFiveDays.Date)
                .OrderBy(x => x.DateCreated)
                    .ThenBy(x => x.InjectionTime)
                .ToList();

            for (int i = 0; i < eatingsForHours.Count; i++)
            {
                if (i + 1 > eatingsForHours.Count - 1)
                    continue;

                var begin = Calculation.DateTimeUnionTimeSpanWithoutMinutes(eatingsForHours[i].DateCreated, eatingsForHours[i].InjectionTime);
                var end = Calculation.DateTimeUnionTimeSpanWithoutMinutes(eatingsForHours[i + 1].DateCreated, eatingsForHours[i + 1].InjectionTime);

                do
                {
                    dataForHours[begin.Hour].Add(eatingsForHours[i].InsulinSensitivityFact.Value);
                    begin = begin.AddHours(1);

                } while (begin < end);
            }

            WidthRequestHours = 24 * 15;

            ChartHours = new LineChart()
            {
                LineMode = LineMode.Spline,
                LabelTextSize = 24,
                Entries = dataForHours
                    .Where(x => x.Value.Count > 0)
                    .Select(x =>
                        new ChartEntry((float)x.Value.Average())
                        {
                            Label = x.Key.ToString(),
                            ValueLabel = $"{Methods.Round(x.Value.Average(), 2)}",
                            ValueLabelColor = App.Current.RequestedTheme == OSAppTheme.Dark
                                ? SkiaSharp.SKColors.White
                                : SkiaSharp.SKColors.Black,

                            Color = App.Current.RequestedTheme == OSAppTheme.Dark
                                ? SkiaSharp.SKColors.LightSkyBlue
                                : SkiaSharp.SKColors.Blue
                        }),

                LabelColor = App.Current.RequestedTheme == OSAppTheme.Dark
                    ? SkiaSharp.SKColors.White
                    : SkiaSharp.SKColors.Black,
                BackgroundColor = App.Current.RequestedTheme == OSAppTheme.Dark
                    ? new SkiaSharp.SKColor(29, 29, 29)
                    : SkiaSharp.SKColors.White,
            };
        }

        /// <summary>
        /// Инициализация графика IC по часам
        /// </summary>
        /// <param name="chartHours"></param>
        /// <returns></returns>
        private void InitICChartPerHour(LineChart chartHours)
        {
            ChartICPerHours = new LineChart()
            {
                LineMode = chartHours.LineMode,
                LabelTextSize = chartHours.LabelTextSize,
                LabelColor = chartHours.LabelColor,
                BackgroundColor = chartHours.BackgroundColor,
                Entries = chartHours.Entries
                    .Select(x =>
                        new ChartEntry(x.Value / (float)GlobalParameters.User.CarbohydrateCoefficient)
                        {
                            Label = x.Label,
                            ValueLabel = $"{Methods.Round(x.Value / (float)GlobalParameters.User.CarbohydrateCoefficient, 2)}",
                            ValueLabelColor = x.ValueLabelColor,
                            Color = x.Color
                        })
            };
        }

        #endregion
    }
}
