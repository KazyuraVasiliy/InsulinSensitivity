using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

using DataAccessLayer.Contexts;
using BusinessLogicLayer.ViewModel;
using BusinessLogicLayer.Service;
using Models = DataAccessLayer.Models;
using System.Collections.ObjectModel;

namespace InsulinSensitivity.Eating
{
    public class EatingPageViewModel : ObservableBase
    {
        #region Constructors

        public EatingPageViewModel(Models.Eating eating = null)
        {
            // Инициализация приёма пищи
            if (eating != null)
            {
                using (var db = new ApplicationContext(GlobalParameters.DbPath))
                    Eating = db.Eatings
                        .Include(x => x.Exercise)
                            .ThenInclude(x => x.ExerciseType)
                        .Include(x => x.EatingType)
                        .Include(x => x.Injections)
                        .Include(x => x.IntermediateDimensions)
                        .FirstOrDefault(x =>
                            x.Id == eating.Id);
            }
            else Eating = new Models.Eating()
            {
                DateCreated = DateTime.Now
            };

            // Инициализация коллекций
            using (var db = new ApplicationContext(GlobalParameters.DbPath))
            {
                // ... Типы приёмов пищи
                EatingTypes = db.EatingTypes
                    .ToList()
                    .OrderBy(x => 
                        x.TimeStart)
                    .ToList();

                // ... Типы упражнений
                ExerciseTypes = db.ExerciseTypes
                    .Where(x =>
                        x.DateDeleted == null)
                    .OrderBy(x =>
                        x.Name)
                    .ToList();
            }

            // Инициализация времени инъекции
            if (eating == null)
            {
                Eating.InjectionTime = Calculation.TimeSpanWithoutSeconds(DateTime.Now.TimeOfDay);
                Eating.EndEating = Calculation.DateTimeWithoutSeconds(DateTime.Now.AddHours(5));
            }

            // Инициализация инъекций
            if (Eating.Injections != null)
            {
                foreach (var el in Eating.Injections)
                    Injections.Add(el);
            }

            // Инициализация промежуточных измерений
            if (Eating.IntermediateDimensions != null)
            {
                foreach (var el in Eating.IntermediateDimensions)
                    IntermediateDimensions.Add(el);
            }

            // Инициализация нагрузки
            if (Eating.Exercise == null)
                Eating.Exercise = new Models.Exercise()
                {
                    ExerciseType = ExerciseTypes
                        .FirstOrDefault(x =>
                            x.IsDefault),
                    Duration = (int)Math.Round(GlobalParameters.User.BolusType.Duration * 60, 0, MidpointRounding.AwayFromZero),
                    HoursAfterInjection = 0
                };

            // Инициализация данных для расчёта
            InitPrevious();
        }

        #endregion

        #region Properties

        #region --System

        /// <summary>
        /// Видно ли поле для ввода базального инсулина
        /// </summary>
        public bool IsBasalDoseVisibility =>
            !GlobalParameters.User.IsPump
                ? GlobalParameters.User.BasalType.Duration == 12
                    ? true
                    : !Basals.Any(x =>
                        x.DateCreated.Date == DateTime.Now.Date)
                : false;

        /// <summary>
        /// Видно ли поле для ввода начала менструального цикла
        /// </summary>
        public bool IsMenstrualCycleStartVisibility =>
            !GlobalParameters.User.Gender && 
            (LastMenstruationDate == null || LastMenstruationDate.Value.Date.AddDays(15) <= DateTime.Now.Date);

        /// <summary>
        /// Видна ли кнопка удаления
        /// </summary>
        public bool IsRemoveVisibility =>
            Eating.Id != Guid.Empty;

        private bool isModalInjection;
        /// <summary>
        /// Отображается ли модальное окно ввода инъекции
        /// </summary>
        public bool IsModalInjection
        {
            get => isModalInjection;
            set
            {
                isModalInjection = value;
                OnPropertyChanged();
            }
        }

        private bool isModalDimension;
        /// <summary>
        /// Отображается ли модальное окно ввода промежуточного измерения
        /// </summary>
        public bool IsModalDimension
        {
            get => isModalDimension;
            set
            {
                isModalDimension = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region --Previous

        /// <summary>
        /// Количество дней, за которые учитываются средние значения
        /// </summary>
        private DateTime Period { get; set; } =
            DateTime.Now.AddDays(-GlobalParameters.User.PeriodOfCalculation);

        /// <summary>
        /// Предыдущие приёмы пищи
        /// </summary>
        private List<Models.Eating> PreviousEatings { get; set; } =
            new List<Models.Eating>();

        /// <summary>
        /// Предыдущие приёмы пищи в которых есть база
        /// </summary>
        private List<Models.Eating> BasalEatings { get; set; } =
            new List<Models.Eating>();

        /// <summary>
        /// Средний ФЧИ предыдущего типа приёма пищи
        /// </summary>
        private decimal? PreviousAverageEatingTypeSensitivity { get; set; }

        /// <summary>
        /// Средние ФЧИ по нагрузкам за предыдущие приёмы пищи
        /// </summary>
        private List<decimal?> PreviousAverageExerciseTypeSensitivitys { get; set; } =
            new List<decimal?>();

        /// <summary>
        /// Предыдущие приёмы пищи в которых выставлены дозы Базального инсулина
        /// </summary>
        private List<Models.Eating> Basals { get; set; } =
            new List<Models.Eating>();

        /// <summary>
        /// Эквивалентный день предыдущего цикла
        /// </summary>
        private DateTime? EquivalentDay { get; set; }

        /// <summary>
        /// Дата последней менструации
        /// </summary>
        private DateTime? LastMenstruationDate { get; set; }

        /// <summary>
        /// Верхняя граница
        /// </summary>
        private decimal? Extremum { get; set; }

        /// <summary>
        /// Нижняя граница
        /// </summary>
        private decimal? Infinum { get; set; }

        #endregion

        #region --Selected

        private Models.Injection selectedInjection;
        /// <summary>
        /// Выбранная инъекция
        /// </summary>
        public Models.Injection SelectedInjection
        {
            get => selectedInjection;
            set
            {
                selectedInjection = value;
                OnPropertyChanged();
            }
        }

        private Models.IntermediateDimension selectedDimension;
        /// <summary>
        /// Выбранное промежуточное измерение
        /// </summary>
        public Models.IntermediateDimension SelectedDimension
        {
            get => selectedDimension;
            set
            {
                selectedDimension = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region --Exercise

        /// <summary>
        /// Тип нагрузки
        /// </summary>
        public Models.ExerciseType ExerciseType
        {
            get => Eating.Exercise.ExerciseType;
            set
            {
                if (Eating.Exercise.ExerciseType != value)
                {
                    Eating.Exercise.ExerciseType = value;
                    CalculateTotal();
                }
            }
        }

        /// <summary>
        /// Продолжительность нагрузки в десятках минут
        /// </summary>
        public int Duration
        {
            get => Eating.Exercise.Duration;
            set
            {
                if (Eating.Exercise.Duration != value)
                {
                    Eating.Exercise.Duration = value;
                    CalculateTotal();
                }
            }
        }

        /// <summary>
        /// Количество часов после инъекции до начала нагрузки
        /// </summary>
        public int HoursAfterInjection
        {
            get => Eating.Exercise.HoursAfterInjection;
            set
            {
                if (Eating.Exercise.HoursAfterInjection != value)
                {
                    Eating.Exercise.HoursAfterInjection = value;
                    CalculateTotal();
                }
            }
        }

        #endregion

        private decimal? insulinSensitivityAuto;
        /// <summary>
        /// ФЧИ рассчитанный (средний)
        /// </summary>
        public decimal? InsulinSensitivityAuto
        {
            get => insulinSensitivityAuto;
            set
            {
                insulinSensitivityAuto = value;
                OnPropertyChanged();
            }
        }

        #region --Nutritional

        /// <summary>
        /// Белки
        /// </summary>
        public int Protein
        {
            get => Eating.Protein;
            set
            {
                if (Eating.Protein != value)
                {
                    Eating.Protein = value;
                    CalculateTotal();
                }
            }
        }

        /// <summary>
        /// Жиры
        /// </summary>
        public int Fat
        {
            get => Eating.Fat;
            set
            {
                if (Eating.Fat != value)
                {
                    Eating.Fat = value;
                    CalculateTotal();
                }
            }
        }

        /// <summary>
        /// Углеводы
        /// </summary>
        public int Carbohydrate
        {
            get => Eating.Carbohydrate;
            set
            {
                if (Eating.Carbohydrate != value)
                {
                    Eating.Carbohydrate = value;
                    CalculateTotal();
                }
            }
        }

        #endregion

        #region --Basal

        private string activeInformation;
        /// <summary>
        /// Информация об активном инсулине
        /// </summary>
        public string ActiveInformation
        {
            get => activeInformation;
            set
            {
                activeInformation = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Дата инъекции базального инсулина
        /// </summary>
        public DateTime BasalInjectionDate
        {
            get => Eating.BasalInjectionTime ?? DateTime.Now;
            set
            {
                if (Eating.BasalInjectionTime?.Date != value.Date)
                {
                    Eating.BasalInjectionTime = Calculation.DateTimeUnionTimeSpan(value, BasalInjectionTime);
                    CalculateTotal();
                }
            }
        }

        /// <summary>
        /// Время инъекции базального инсулина
        /// </summary>
        public TimeSpan BasalInjectionTime
        {
            get => Eating.BasalInjectionTime?.TimeOfDay ?? DateTime.Now.TimeOfDay;
            set
            {
                if (Eating.BasalInjectionTime == null || Calculation.TimeSpanWithoutSeconds(Eating.BasalInjectionTime.Value.TimeOfDay) != value)
                {
                    Eating.BasalInjectionTime = Calculation.DateTimeUnionTimeSpan(BasalInjectionDate, value);
                    CalculateTotal();
                }
            }
        }

        /// <summary>
        /// Доза базального инсулина
        /// </summary>
        public decimal BasalDose
        {
            get => Eating.BasalDose;
            set
            {
                if (Eating.BasalDose != value)
                {
                    Eating.BasalDose = value;
                    CalculateTotal();
                }
            }
        }

        #endregion

        /// <summary>
        /// Текущий приём пищи
        /// </summary>
        public Models.Eating Eating { get; private set; }

        /// <summary>
        /// Тип приёма пищи
        /// </summary>
        public Models.EatingType EatingType
        {
            get => Eating.EatingType;
            set
            {
                Eating.EatingType = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Время инъекции
        /// </summary>
        public TimeSpan InjectionTime
        {
            get => Eating.InjectionTime;
            set
            {
                if (Calculation.TimeSpanWithoutSeconds(Eating.InjectionTime) != value)
                {
                    Eating.InjectionTime = value;

                    Eating.EndEating = Calculation.DateTimeUnionTimeSpan(Eating.DateCreated, value).AddHours(5);
                    OnPropertyChanged(nameof(EndEatingTime));
                    OnPropertyChanged(nameof(EndEatingDate));

                    CalculateTotal();
                }
            }
        }

        /// <summary>
        /// Дата отработки
        /// </summary>
        public DateTime EndEatingDate
        {
            get => Eating.EndEating ?? DateTime.Now;
            set
            {
                if (Eating.EndEating?.Date != value)
                {
                    Eating.EndEating = Calculation.DateTimeUnionTimeSpan(value, EndEatingTime);
                    CalculateTotal();
                }
            }
        }

        /// <summary>
        /// Время отработки
        /// </summary>
        public TimeSpan EndEatingTime
        {
            get => Eating.EndEating?.TimeOfDay ?? DateTime.Now.TimeOfDay;
            set
            {
                if (Eating.EndEating == null || Calculation.TimeSpanWithoutSeconds(Eating.EndEating.Value.TimeOfDay) != value)
                {
                    Eating.EndEating = Calculation.DateTimeUnionTimeSpan(EndEatingDate, value);
                    CalculateTotal();
                }
            }
        }

        /// <summary>
        /// Пауза
        /// </summary>
        public int Pause
        {
            get => Eating.Pause;
            set => Eating.Pause = value;
        }

        /// <summary>
        /// Исходный сахар
        /// </summary>
        public decimal GlucoseStart
        {
            get => Eating.GlucoseStart;
            set
            {
                if (Eating.GlucoseStart != value)
                {
                    Eating.GlucoseStart = value;
                    CalculateTotal();
                }
            }
        }

        /// <summary>
        /// Сахар на отработке
        /// </summary>
        public decimal? GlucoseEnd
        {
            get => Eating.GlucoseEnd;
            set
            {
                if (Eating.GlucoseEnd != value)
                {
                    Eating.GlucoseEnd = value;

                    Eating.EndEating = Calculation.DateTimeWithoutSeconds(DateTime.Now);
                    OnPropertyChanged(nameof(EndEatingTime));
                    OnPropertyChanged(nameof(EndEatingDate));

                    CalculateTotal();
                }
            }
        }

        /// <summary>
        /// ФЧИ фактический
        /// </summary>
        public decimal? InsulinSensitivityFact
        {
            get => Eating.InsulinSensitivityFact;
            set
            {
                Eating.InsulinSensitivityFact = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// ФЧИ пользователя
        /// </summary>
        public decimal? InsulinSensitivityUser
        {
            get => Eating.InsulinSensitivityUser;
            set
            {
                if (Eating.InsulinSensitivityUser != value)
                {
                    Eating.InsulinSensitivityUser = value;
                    CalculateTotal();
                }
            }
        }

        /// <summary>
        /// Доза болюсного инсулина вычисленная
        /// </summary>
        public decimal? BolusDoseCalculate
        {
            get => Eating.BolusDoseCalculate;
            set
            {
                Eating.BolusDoseCalculate = value;
                OnPropertyChanged();
            }
        }

        private decimal? bolusDoseCarbohydrate;
        /// <summary>
        /// Доза инсулина на углеводы
        /// </summary>
        public decimal? BolusDoseCarbohydrate
        {
            get => bolusDoseCarbohydrate;
            set
            {
                bolusDoseCarbohydrate = value;
                OnPropertyChanged();
            }
        }

        private decimal? bolusDoseFatAndProtein;
        /// <summary>
        /// Доза инсулина на жиры и белки
        /// </summary>
        public decimal? BolusDoseFatAndProtein
        {
            get => bolusDoseFatAndProtein;
            set
            {
                bolusDoseFatAndProtein = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Основная доза
        /// </summary>
        public decimal BolusDoseFact
        {
            get => Eating.BolusDoseFact;
            set
            {
                if (Eating.BolusDoseFact != value)
                {
                    Eating.BolusDoseFact = value;
                    CalculateTotal();
                }
            }
        }

        private decimal bolusDoseTotal;
        /// <summary>
        /// Доза болюсного инсулина итоговая
        /// </summary>
        public decimal BolusDoseTotal
        {
            get => bolusDoseTotal;
            set
            {
                bolusDoseTotal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Начало менструального цикла
        /// </summary>
        public bool IsMenstrualCycleStart { get; set; }

        /// <summary>
        /// Комментарий
        /// </summary>
        public string Comment
        {
            get => Eating.Comment;
            set => Eating.Comment = value;
        }

        /// <summary>
        /// Ожидаемый уровень сахар
        /// </summary>
        public decimal? ExpectedGlucose
        {
            get => Eating.ExpectedGlucose;
            set
            {
                Eating.ExpectedGlucose = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Инициализирует значения для расчёта ФЧИ
        /// </summary>
        private void InitPrevious()
        {
            using (var db = new ApplicationContext(GlobalParameters.DbPath))
            {
                // Предыдущие приёмы пищи
                PreviousEatings = db.Eatings
                    .Where(x =>
                        x.Id != Eating.Id)
                    .OrderByDescending(x =>
                        x.DateCreated)
                    .Include(x => x.Exercise)
                    .Include(x => x.Injections)
                    .Include(x => x.IntermediateDimensions)
                    .Take(3)
                    .ToList();

                // Предыдущие приёмы пищи, в которых есть база
                BasalEatings = db.Eatings
                    .Where(x =>
                        x.Id != Eating.Id &&
                        x.BasalDose != 0 &&
                        x.BasalInjectionTime != null)
                    .Take(3)
                    .ToList();

                // Предыдущий приём пищи
                var previousEating = (PreviousEatings?.Count ?? 0) > 0
                    ? PreviousEatings[0]
                    : null;

                // Исходный сахар
                if (previousEating?.GlucoseEnd != null && Eating.Id == Guid.Empty)
                    GlucoseStart = previousEating.GlucoseEnd.Value;

                // Средний ФЧИ предыдущего типа приёма пищи
                if (previousEating != null)
                {
                    var previousAverageEatingTypeSensitivityQuery = db.Eatings
                        .Where(x =>
                            x.Id != Eating.Id &&
                            x.EatingTypeId == previousEating.EatingTypeId);

                    if (GlobalParameters.User.PeriodOfCalculation > 0)
                        previousAverageEatingTypeSensitivityQuery = previousAverageEatingTypeSensitivityQuery
                            .Where(x =>
                                x.DateCreated.Date >= Period.Date);

                    var previousAverageEatingTypeSensitivityCollection = previousAverageEatingTypeSensitivityQuery
                        .ToList();

                    PreviousAverageEatingTypeSensitivity = (previousAverageEatingTypeSensitivityCollection?.Count ?? 0) > 0
                        ? previousAverageEatingTypeSensitivityCollection
                            .Average(x =>
                                x.InsulinSensitivityFact)
                        : null;
                }

                // Средние ФЧИ по нагрузкам за предыдущие приёмы пищи
                foreach (var previous in PreviousEatings)
                {
                    decimal? value = null;
                    if (previous.Exercise != null)
                    {
                        // ... Приёмы пищи с нагрузкой равной по типу и количеству часов после инъекции
                        var exercisesQuery = db.Eatings
                            .Where(x =>
                                x.Id != Eating.Id &&
                                x.Exercise.ExerciseTypeId == previous.Exercise.ExerciseTypeId &&
                                x.Exercise.HoursAfterInjection == previous.Exercise.HoursAfterInjection);

                        if (GlobalParameters.User.PeriodOfCalculation > 0)
                            exercisesQuery = exercisesQuery
                                .Where(x =>
                                    x.DateCreated.Date >= Period.Date);
                        
                        var exercises = exercisesQuery
                            .Include(x => x.Exercise)
                            .ToList();

                        // ... Приёмы пищи с нагрузкой по продолжительности +- 5
                        var exercisesOne = exercises
                            .Where(x =>
                                x.Exercise.Duration <= (previous.Exercise.Duration + 5) &&
                                x.Exercise.Duration >= (previous.Exercise.Duration - 5));

                        if ((exercisesOne?.Count() ?? 0) > 0)
                            value = exercisesOne
                                .Average(x =>
                                    x.InsulinSensitivityFact);

                        if (value == null)
                        {
                            // ... Приёмы пищи с нагрузкой по продолжительности +- 10
                            var exercisesTwo = exercises
                                .Where(x =>
                                    x.Exercise.Duration <= (previous.Exercise.Duration + 10) &&
                                    x.Exercise.Duration >= (previous.Exercise.Duration - 10));

                            if ((exercisesTwo?.Count() ?? 0) > 0)
                                value = exercisesTwo
                                    .Average(x =>
                                        x.InsulinSensitivityFact);
                        }
                    }
                    PreviousAverageExerciseTypeSensitivitys.Add(value);
                }

                // Предыдущие приёмы пищи в которых выставлены дозы Базального инсулина
                Basals = db.Eatings
                    .Where(x =>
                        x.Id != Eating.Id &&
                        x.BasalDose != 0M)
                    .OrderByDescending(x =>
                        x.DateCreated)
                    .Take(4)
                    .ToList();

                // Эквивалентный день предыдущего цикла
                if (GlobalParameters.User.Gender == false)
                {
                    var menstrualCollection = db.MenstrualCycles
                        .OrderByDescending(x =>
                            x.DateStart)
                        .Take(2)
                        .ToList();

                    if ((menstrualCollection?.Count ?? 0) > 0)
                        LastMenstruationDate = menstrualCollection[0].DateStart;

                    if ((menstrualCollection?.Count ?? 0) > 1)
                    {
                        var day = (DateTime.Now.Date - menstrualCollection[0].DateStart.Date).TotalDays;
                        var equivalentDay = menstrualCollection[1].DateStart.AddDays(day);

                        if (equivalentDay.Date < menstrualCollection[0].DateStart.Date)
                            EquivalentDay = equivalentDay;
                    }
                }
            }
        }

        /// <summary>
        /// Рассчитывает автоматический ФЧИ по первой формуле и дозу болюсного инсулина
        /// </summary>
        private void CalculateInsulinSensitivityOne()
        {
            // Рассчёт ФЧИ по первой формуле
            Eating.InsulinSensitivityAutoOne = null;
            if ((PreviousEatings?.Count ?? 0) > 0 && PreviousEatings[0].InsulinSensitivityFact != null && (PreviousAverageEatingTypeSensitivity ?? 0) != 0 && EatingType != null)
            {
                using (var db = new ApplicationContext(GlobalParameters.DbPath))
                {
                    var averageEatingTypeSensitivityCollectionQuery = db.Eatings
                        .Where(x =>
                            x.Id != Eating.Id &&
                            x.EatingTypeId == EatingType.Id);

                    if (GlobalParameters.User.PeriodOfCalculation > 0)
                        averageEatingTypeSensitivityCollectionQuery = averageEatingTypeSensitivityCollectionQuery
                            .Where(x =>
                                x.DateCreated.Date >= Period.Date);

                    // Средний ФЧИ текущего типа приёма пищи
                    var averageEatingTypeSensitivityCollection = averageEatingTypeSensitivityCollectionQuery
                        .ToList();

                    var averageEatingTypeSensitivity = (averageEatingTypeSensitivityCollection?.Count ?? 0) > 0
                        ? averageEatingTypeSensitivityCollection
                            .Average(x =>
                                x.InsulinSensitivityFact)
                        : null;

                    if (averageEatingTypeSensitivity != null)
                        Eating.InsulinSensitivityAutoOne = PreviousEatings[0].InsulinSensitivityFact * (averageEatingTypeSensitivity / PreviousAverageEatingTypeSensitivity);
                }
            }
        }

        /// <summary>
        /// Рассчитывает автоматический ФЧИ по второй формуле и дозу болюсного инсулина
        /// </summary>
        private void CalculateInsulinSensitivityTwo()
        {
            // Расчёт ФЧИ по второй формуле
            Eating.InsulinSensitivityAutoTwo = null;
            var check = (PreviousEatings?.Count ?? 0) == 3 &&
                PreviousEatings.All(x => x.InsulinSensitivityFact != null) &&
                (PreviousAverageExerciseTypeSensitivitys?.Count ?? 0) == 3 &&
                PreviousAverageExerciseTypeSensitivitys.All(x => (x ?? 0) != 0) &&
                ExerciseType != null;

            if (check)
            {
                using (var db = new ApplicationContext(GlobalParameters.DbPath))
                {
                    var averageExerciseTypeSensitivityCollectionQuery = db.Eatings
                        .Where(x =>
                            x.Id != Eating.Id &&
                            x.Exercise.ExerciseTypeId == ExerciseType.Id &&
                            x.Exercise.HoursAfterInjection == HoursAfterInjection);

                    if (GlobalParameters.User.PeriodOfCalculation > 0)
                        averageExerciseTypeSensitivityCollectionQuery = averageExerciseTypeSensitivityCollectionQuery
                            .Where(x =>
                                x.DateCreated.Date >= Period.Date);

                    // Средний ФЧИ текущего типа нагрузки
                    var averageExerciseTypeSensitivityCollection = averageExerciseTypeSensitivityCollectionQuery
                        .Include(x => x.Exercise)
                        .ToList();

                    decimal? averageExerciseTypeSensitivity = null;
                    if ((averageExerciseTypeSensitivityCollection?.Count ?? 0) > 0)
                    {
                        // ... Приёмы пищи с нагрузкой по продолжительности +- 5
                        var exercisesOne = averageExerciseTypeSensitivityCollection
                            .Where(x =>
                                x.Exercise.Duration <= (Duration + 5) &&
                                x.Exercise.Duration >= (Duration - 5));

                        if ((exercisesOne?.Count() ?? 0) > 0)
                            averageExerciseTypeSensitivity = exercisesOne
                                .Average(x =>
                                    x.InsulinSensitivityFact);

                        if (averageExerciseTypeSensitivity == null)
                        {
                            // ... Приёмы пищи с нагрузкой по продолжительности +- 10
                            var exercisesTwo = averageExerciseTypeSensitivityCollection
                                .Where(x =>
                                    x.Exercise.Duration <= (Duration + 10) &&
                                    x.Exercise.Duration >= (Duration - 10));

                            if ((exercisesTwo?.Count() ?? 0) > 0)
                                averageExerciseTypeSensitivity = exercisesTwo
                                    .Average(x =>
                                        x.InsulinSensitivityFact);
                        }
                    }

                    if (averageExerciseTypeSensitivity != null)
                    {
                        // Средние по предыдущим приёмам пищи
                        decimal average = 0;
                        for (int i = 0; i < 3; i++)
                            average += PreviousEatings[i].InsulinSensitivityFact.Value / PreviousAverageExerciseTypeSensitivitys[i].Value;
                        average /= 3;

                        // Учёт базы
                        decimal basal = 1;
                        if (!GlobalParameters.User.IsPump && !GlobalParameters.Settings.IsActiveBasal)
                        {
                            if (GlobalParameters.User.BasalType.Duration > 12)
                            {
                                if (BasalDose != 0 && (Basals?.Count ?? 0) > 0 && Basals[0] != null)
                                    basal = BasalDose / Basals[0].BasalDose;
                                else if ((Basals?.Count ?? 0) >= 2)
                                    basal = Basals[0].BasalDose / Basals[1].BasalDose;

                                // ... 24 часовые инсулины
                                if (GlobalParameters.User.BasalType.Duration > 12 && GlobalParameters.User.BasalType.Duration <= 24)
                                    basal = (basal + 1) / 2;

                                // ... 48 часовые инсулины
                                if (GlobalParameters.User.BasalType.Duration > 24)
                                    basal = (basal + 2) / 3;
                            }
                            else
                            {
                                List<decimal?> doses = new List<decimal?>();
                                var date = DateTime.Now;

                                for (int i = 0; i < 3; i++)
                                {
                                    date = date.AddDays(-i);
                                    doses.Add(Basals
                                        ?.Where(x =>
                                            x.DateCreated.Day == date.Day &&
                                            x.DateCreated.Month == date.Month &&
                                            x.DateCreated.Year == date.Year)
                                        .OrderBy(x =>
                                            x.InjectionTime)
                                        .Take(1)
                                        .FirstOrDefault()?.BasalDose);
                                }

                                // ... Если сегодня не было инъекций
                                // ... Вчерашняя утренняя доза / Позавчерашняя утренняя доза
                                if (BasalDose == 0 && doses[0] == null && doses[1] != null && doses[2] != null)
                                    basal = doses[1].Value / doses[2].Value;

                                // ... Если сегодня первая инъекция
                                // ... Сегодняшняя утренняя доза / Вчерашняя утренняя доза
                                else if (BasalDose != 0 && doses[0] == null && doses[1] != null)
                                    basal = BasalDose / doses[1].Value;

                                // ... Если сегодня уже была доза, то
                                // ... Сегодняшняя утренняя доза / Вчерашняя утренняя доза
                                else if (doses[0] != null && doses[1] != null)
                                    basal = doses[0].Value / doses[1].Value;
                            }
                        }

                        Eating.InsulinSensitivityAutoTwo = average * averageExerciseTypeSensitivity * basal;
                    }
                }
            }
        }

        /// <summary>
        /// Рассчитывает автоматический ФЧИ по третьей формуле и дозу болюсного инсулина
        /// </summary>
        private void CalculateInsulinSensitivityThree()
        {
            // Рассчёт ФЧИ по третьей формуле
            Eating.InsulinSensitivityAutoThree = null;
            var check = 
                !GlobalParameters.User.Gender &&
                EquivalentDay != null &&
                EatingType != null;

            if (check)
            {
                using (var db = new ApplicationContext(GlobalParameters.DbPath))
                {
                    var previousDay = EquivalentDay.Value.AddDays(-1);
                    var nextDay = EquivalentDay.Value.AddDays(1);

                    var averageEatingTypeSensitivityCollection = db.Eatings
                        .Where(x =>
                            x.Id != Eating.Id &&
                            x.InsulinSensitivityFact != null &&
                            x.EatingTypeId == EatingType.Id &&
                            x.DateCreated.Date >= previousDay.Date &&
                            x.DateCreated.Date <= nextDay.Date)
                        .ToList();

                    if ((averageEatingTypeSensitivityCollection?.Count ?? 0) > 0)
                        Eating.InsulinSensitivityAutoThree = averageEatingTypeSensitivityCollection
                            .Average(x =>
                                x.InsulinSensitivityFact);
                }
            }
        }

        /// <summary>
        /// Рассчитывает автоматический ФЧИ (средний)
        /// </summary>
        private void CalculateInsulinSensitivityAuto()
        {
            var values = new List<decimal?>()
            {
                Eating.InsulinSensitivityAutoOne,
                Eating.InsulinSensitivityAutoTwo,
                Eating.InsulinSensitivityAutoThree
            };

            values = values
                .Where(x =>
                    x != null)
                .ToList();

            if (Infinum != null || Extremum != null)
            {
                for (int i = 0; i < (values?.Count ?? 0); i++)
                {
                    if (Infinum != null)
                        values[i] = values[i] < Infinum ? Infinum : values[i];

                    if (Extremum != null)
                        values[i] = values[i] > Extremum ? Extremum : values[i];
                }
            }

            InsulinSensitivityAuto = (values?.Count ?? 0) == 0
                ? (decimal?)null
                : Math.Round(values.Average().Value, 3, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Расчёт дозы болюсного инсулина
        /// </summary>
        private void CalculateBolusDose()
        {
            var bolusDose = (InsulinSensitivityAuto ?? 0) != 0 || (InsulinSensitivityUser ?? 0) != 0
                ? Calculation.GetBolusDose(GlucoseStart, GlobalParameters.User.TargetGlucose,
                    GlobalParameters.User.CarbohydrateCoefficient, GlobalParameters.User.ProteinCoefficient, GlobalParameters.User.FatCoefficient,
                    Protein, Fat, Carbohydrate,
                    (InsulinSensitivityUser ?? 0) != 0
                        ? InsulinSensitivityUser.Value
                        : InsulinSensitivityAuto.Value) - GlobalMethods.GetActiveInsulin(Eating, Injections,
                            Calculation.DateTimeUnionTimeSpan(Eating.DateCreated, Eating.InjectionTime), Eating.EndEating, Eating.Id, true,
                            Eating.Carbohydrate, Eating.Pause).insulin
                : (decimal?)null;

            if (bolusDose != null && Eating.EndEating != null)
                bolusDose = bolusDose * (decimal)(1 - Calculation.GetActiveInsulinPercent(
                    Calculation.DateTimeUnionTimeSpan(Eating.DateCreated, Eating.InjectionTime), Eating.EndEating.Value, (int)GlobalParameters.User.BolusType.Duration));

            BolusDoseCalculate = bolusDose != null
                ? bolusDose
                : (decimal?)null;

            var proteinAndFat = Protein * GlobalParameters.User.ProteinCoefficient +
                Fat * GlobalParameters.User.FatCoefficient;

            BolusDoseCarbohydrate = BolusDoseCalculate != null
                ? Math.Round(BolusDoseCalculate.Value / (proteinAndFat + Carbohydrate) * Carbohydrate, 2, MidpointRounding.AwayFromZero)
                : (decimal?)null;

            BolusDoseFatAndProtein = BolusDoseCalculate != null
                ? Math.Round(BolusDoseCalculate.Value / (proteinAndFat + Carbohydrate) * proteinAndFat, 2, MidpointRounding.AwayFromZero)
                : (decimal?)null;
        }

        /// <summary>
        /// Расчёт ожидаемого сахара
        /// </summary>
        private void CalculateExpectedGlucose()
        {
            ExpectedGlucose = (InsulinSensitivityAuto != null || InsulinSensitivityUser != null)
                ? Calculation.GetExpectedGlucose(GlucoseStart, BolusDoseTotal,
                    GlobalParameters.User.CarbohydrateCoefficient, GlobalParameters.User.ProteinCoefficient, GlobalParameters.User.FatCoefficient,
                    Protein, Fat, Carbohydrate,
                    InsulinSensitivityUser != null
                        ? InsulinSensitivityUser.Value
                        : InsulinSensitivityAuto.Value)
                : (decimal?)null;
        }

        /// <summary>
        /// Расчёт фактического ФЧИ
        /// </summary>
        private void CalculateInsulinSensitivityFact()
        {
            InsulinSensitivityFact = BolusDoseTotal > 0 && GlucoseEnd != null
                ? Calculation.GetInsulinSensitivityFact(GlucoseStart, GlucoseEnd.Value,
                    GlobalParameters.User.CarbohydrateCoefficient, GlobalParameters.User.ProteinCoefficient, GlobalParameters.User.FatCoefficient,
                    Protein, Fat, Carbohydrate,
                    BolusDoseTotal)
                : (decimal?)null;
        }

        /// <summary>
        /// Расчёт ФЧИ пользователя (если он не введён)
        /// </summary>
        private void CalculateInsulinSensitivityUser()
        {
            Eating.InsulinSensitivityUser = BolusDoseTotal > 0 && GlucoseEnd != null && InsulinSensitivityUser == null
                ? Math.Round(Calculation.GetInsulinSensitivityFact(GlucoseStart, GlobalParameters.User.TargetGlucose,
                    GlobalParameters.User.CarbohydrateCoefficient, GlobalParameters.User.ProteinCoefficient, GlobalParameters.User.FatCoefficient,
                    Protein, Fat, Carbohydrate,
                    BolusDoseTotal), 3, MidpointRounding.AwayFromZero)
                : InsulinSensitivityUser;
            OnPropertyChanged(nameof(InsulinSensitivityUser));
        }

        /// <summary>
        /// Расчёт точности расчётного ФЧИ
        /// </summary>
        private void CalculateAccuracyAuto()
        {
            Eating.AccuracyAuto = (InsulinSensitivityFact ?? 0) != 0 && InsulinSensitivityAuto != null
                ? (int)Math.Round((InsulinSensitivityFact.Value - Math.Abs(InsulinSensitivityFact.Value - InsulinSensitivityAuto.Value)) / InsulinSensitivityFact.Value * 100, 0, MidpointRounding.AwayFromZero)
                : (int?)null;
        }

        /// <summary>
        /// Расчёт точности ФЧИ пользователя
        /// </summary>
        private void CalculateAccuracyUser()
        {
            Eating.AccuracyUser = (InsulinSensitivityFact ?? 0) != 0 && InsulinSensitivityUser != null
                ? (int)Math.Round((InsulinSensitivityFact.Value - Math.Abs(InsulinSensitivityFact.Value - InsulinSensitivityUser.Value)) / InsulinSensitivityFact.Value * 100, 0, MidpointRounding.AwayFromZero)
                : (int?)null;
        }

        /// <summary>
        /// Расчёт минимального и максимального отношения ФЧИ текущего типа приёма пищи к предыдущему
        /// </summary>
        private void CalculateInfinumExtremum()
        {
            Infinum = null;
            Extremum = null;

            bool check =
                EatingType != null &&
                (PreviousEatings?.Count ?? 0) > 0 &&
                PreviousEatings[0].InsulinSensitivityFact != null;

            if (check)
            {
                using (var db = new ApplicationContext(GlobalParameters.DbPath))
                {
                    var yesterday = DateTime.Now.AddDays(-1);
                    var ratioCollectionQuery = db.Eatings
                        .Where(x =>
                            x.DateCreated.Date <= yesterday.Date &&
                            x.InsulinSensitivityFact != null);

                    if (GlobalParameters.User.PeriodOfCalculation > 0)
                        ratioCollectionQuery = ratioCollectionQuery
                            .Where(x =>
                                x.DateCreated >= Period);

                    var ratioCollection = ratioCollectionQuery
                        .ToList()
                        .OrderByDescending(x =>
                            x.DateCreated.Date)
                        .ThenByDescending(x =>
                            x.InjectionTime)
                        .ToList();

                    var ratios = new List<decimal>();
                    for (int i = 0; i < (ratioCollection?.Count ?? 0); i++)
                    {
                        check =
                            ratioCollection[i].EatingTypeId == EatingType.Id &&
                            i != 0 &&
                            ratioCollection[i - 1].InsulinSensitivityFact != 0;

                        if (check)
                            ratios.Add(ratioCollection[i].InsulinSensitivityFact.Value / ratioCollection[i - 1].InsulinSensitivityFact.Value);
                    }

                    if ((ratios?.Count ?? 0) > 0)
                    {
                        var infinum = PreviousEatings[0].InsulinSensitivityFact.Value * ratios.Min();
                        var extremum = PreviousEatings[0].InsulinSensitivityFact.Value * ratios.Max();

                        if (infinum < 0.95M)
                            Infinum = infinum;

                        if (extremum > 1.05M)
                            Extremum = extremum;
                    }
                }
            }
        }

        /// <summary>
        /// Определяет, какую ошибку совершил пользователь
        /// </summary>
        private string GetError()
        {
            if (GlucoseEnd == null)
                return null;

            if (GlucoseEnd >= GlobalParameters.User.Hyperglycemia)
                return "Слишком мало инсулина";
            else if (GlucoseEnd >= GlobalParameters.User.HighSugar)
            {
                if (((Protein * 4 + Fat * 9) / 100) >= 4 && (Injections?.Count ?? 0) == 0)
                    return "Отсутствует дополнительная инъекция на белки и жиры";
                else return "Мало инсулина";
            }
            else if (GlucoseEnd <= GlobalParameters.User.Hypoglycemia)
                return "Слишком много инсулина";
            else if (GlucoseEnd <= GlobalParameters.User.LowSugar)
                return "Много инсулина";
            else
            {
                var dimensions = IntermediateDimensions
                    .OrderByDescending(x =>
                        x.DimensionDate.Date)
                    .ThenByDescending(x =>
                        x.DimensionTime);

                foreach (var dimension in dimensions)
                {
                    if (dimension.Glucose > GlobalParameters.User.LowSugar && dimension.Glucose < GlobalParameters.User.HighSugar)
                        continue;

                    var timeSpan = (Calculation.DateTimeUnionTimeSpan(dimension.DimensionDate, dimension.DimensionTime) - 
                        Calculation.DateTimeUnionTimeSpan(Eating.DateCreated, Eating.InjectionTime)).TotalMinutes;

                    if (timeSpan > 100)
                    {
                        if (dimension.Glucose >= GlobalParameters.User.Hyperglycemia)
                        {
                            if (GlucoseEnd > GlobalParameters.User.TargetGlucose)
                                return "Мало инсулина";
                            else if ((Injections?.Count ?? 0) > 0)
                                return "Дробление инъекции было излишним";
                        }                            
                        else if (dimension.Glucose <= GlobalParameters.User.Hypoglycemia)
                            return "Слишком много инсулина";
                        else if (dimension.Glucose <= GlobalParameters.User.LowSugar && (Injections?.Count ?? 0) > 0 && ((Protein * 4 + Fat * 9) / 100) >= 4)
                            return "Слишком много инсулина на белки и жиры";
                    }
                    else
                    {
                        if (dimension.Glucose >= GlobalParameters.User.HighSugar && (dimension.Glucose - GlucoseStart) > 3)
                        {
                            var fast = (dimension.Glucose - GlucoseStart) >= 5 ? "быстрые " : "";

                            if (Pause == 0)
                                return $"Отсутствует пауза на {fast}углеводы";
                            else return $"Недостаточно паузы на {fast}углеводы";
                        }
                        else if (dimension.Glucose <= GlobalParameters.User.Hypoglycemia)
                            return "Слишком длинная пауза после инъекции";
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Задаёт время отработки пищи
        /// </summary>
        private void SetWorkingTime()
        {
            var workingTime = Math.Round(-30 + ((Carbohydrate / (double)GlobalParameters.User.AbsorptionRateOfCarbohydrates) + 
                (Protein / (double)GlobalParameters.User.AbsorptionRateOfProteins) + 
                (Fat / (double)GlobalParameters.User.AbsorptionRateOfFats)) * 60, 0, MidpointRounding.AwayFromZero);

            if (workingTime < 180)
                workingTime = 180;

            Eating.WorkingTime = InjectionTime.Add(new TimeSpan(0, (int)workingTime, 0));
        }

        /// <summary>
        /// Расчёт всех значений
        /// </summary>
        private void CalculateTotal()
        {
            var startEating = Calculation.DateTimeUnionTimeSpan(Eating.DateCreated, Eating.InjectionTime);

            // Тип приёма пищи
            EatingType = EatingTypes
                .FirstOrDefault(x =>
                    x.TimeStart <= Eating.InjectionTime &&
                    x.TimeEnd >= Eating.InjectionTime);            

            // Минимальное и максимальное отношения ФЧИ текущего типа приёма пищи к предыдущему
            CalculateInfinumExtremum();

            // Расчётное ФЧИ
            CalculateInsulinSensitivityOne();
            CalculateInsulinSensitivityTwo();
            CalculateInsulinSensitivityThree();

            // Расчётное ФЧИ (средний)
            CalculateInsulinSensitivityAuto();

            var active = GlobalMethods.GetActiveInsulin(Eating, Injections,
                startEating, Eating.EndEating, Eating.Id, false,
                Eating.Carbohydrate, Eating.Pause);

            BolusDoseTotal = active.insulin;
            ActiveInformation = string.Join("\n", active.informations);

            // Доза болюсного инсулина
            CalculateBolusDose();

            // Ожидаемый сахар
            CalculateExpectedGlucose();

            // Время отработки пищи
            SetWorkingTime();

            // Фактический ФЧИ
            CalculateInsulinSensitivityFact();

            // ФЧИ пользователя
            CalculateInsulinSensitivityUser();

            // Точность
            CalculateAccuracyUser();
            CalculateAccuracyAuto();
        }

        #endregion

        #region Collections

        private List<Models.EatingType> eatingTypes;
        /// <summary>
        /// Типы приёма пищи
        /// </summary>
        public List<Models.EatingType> EatingTypes
        {
            get => eatingTypes;
            set
            {
                eatingTypes = value;
                OnPropertyChanged(nameof(EatingTypes));
            }
        }

        private List<Models.ExerciseType> exerciseTypes;
        /// <summary>
        /// Типы нагрузки
        /// </summary>
        public List<Models.ExerciseType> ExerciseTypes
        {
            get => exerciseTypes;
            set
            {
                exerciseTypes = value;
                OnPropertyChanged(nameof(ExerciseTypes));
            }
        }

        /// <summary>
        /// Инъекции
        /// </summary>
        public ObservableCollection<Models.Injection> Injections { get; set; } =
            new ObservableCollection<Models.Injection>();

        /// <summary>
        /// Промежуточные измерения
        /// </summary>
        public ObservableCollection<Models.IntermediateDimension> IntermediateDimensions { get; set; } =
            new ObservableCollection<Models.IntermediateDimension>();

        #endregion

        #region Commands

        #region --Ok

        private async void OkExecute()
        {
            if (!OkCanExecute())
            {
                await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Ошибка",
                    "Заполните все поля правильно",
                    "Ok");
                return;
            }

            using (var db = new ApplicationContext(GlobalParameters.DbPath))
            {
                // Нагрузка
                var exercise = Eating.Exercise.Id == Guid.Empty
                    ? new Models.Exercise() { Id = Guid.NewGuid() }
                    : db.Exercises.Find(Eating.Exercise.Id);

                exercise.ExerciseTypeId = Eating.Exercise.ExerciseType.Id;
                exercise.Duration = Eating.Exercise.Duration;
                exercise.HoursAfterInjection = Eating.Exercise.HoursAfterInjection;

                if (Eating.Exercise.Id == Guid.Empty)
                    db.Exercises.Add(exercise);                

                // Инициализация приёма пищи
                var eating = Eating.Id == Guid.Empty
                    ? new Models.Eating() { Id = Guid.NewGuid(), DateCreated = Eating.DateCreated }
                    : db.Eatings.Find(Eating.Id);

                // Инъекции
                var injectionsEntity = db.Injections
                    .Where(x =>
                        x.EatingId == Eating.Id);

                // ... Удаление / изменение
                foreach (var injection in injectionsEntity)
                {
                    var item = Injections
                        .FirstOrDefault(x =>
                            x.Id == injection.Id);

                    if (item == null)
                        db.Injections.Remove(injection);
                    else
                    {
                        injection.InjectionTime = item.InjectionTime;
                        injection.BolusDose = item.BolusDose;
                        injection.InjectionDate = item.InjectionDate;
                    }
                }

                // ... Добавление
                foreach (var injection in Injections)
                {
                    if (!injectionsEntity.Any(x => x.Id == injection.Id))
                        db.Injections.Add(new Models.Injection()
                        {
                            Id = injection.Id,
                            EatingId = eating.Id,
                            InjectionTime = injection.InjectionTime,
                            InjectionDate = injection.InjectionDate,
                            BolusDose = injection.BolusDose
                        });
                }

                // Промежуточные измерения
                var dimensionsEntity = db.IntermediateDimensions
                    .Where(x =>
                        x.EatingId == Eating.Id);

                // ... Удаление / изменение
                foreach (var dimension in dimensionsEntity)
                {
                    var item = IntermediateDimensions
                        .FirstOrDefault(x =>
                            x.Id == dimension.Id);

                    if (item == null)
                        db.IntermediateDimensions.Remove(dimension);
                    else
                    {
                        dimension.DimensionTime = item.DimensionTime;
                        dimension.Glucose = item.Glucose;
                        dimension.DimensionDate = item.DimensionDate;
                    }
                }

                // ... Добавление
                foreach (var dimension in IntermediateDimensions)
                {
                    if (!dimensionsEntity.Any(x => x.Id == dimension.Id))
                        db.IntermediateDimensions.Add(new Models.IntermediateDimension()
                        {
                            Id = dimension.Id,
                            EatingId = eating.Id,
                            DimensionTime = dimension.DimensionTime,
                            DimensionDate = dimension.DimensionDate,
                            Glucose = dimension.Glucose
                        });
                }

                eating.InjectionTime = Eating.InjectionTime;
                eating.GlucoseStart = Eating.GlucoseStart;
                eating.GlucoseEnd = Eating.GlucoseEnd;

                //eating.ActiveInsulinStart = Eating.ActiveInsulinStart;
                //eating.ActiveInsulinEnd = Eating.ActiveInsulinEnd;

                eating.Protein = Eating.Protein;
                eating.Fat = Eating.Fat;
                eating.Carbohydrate = Eating.Carbohydrate;

                eating.BasalDose = Eating.BasalDose;
                eating.BasalInjectionTime = Eating.BasalInjectionTime;

                eating.BolusDoseCalculate = Eating.BolusDoseCalculate;
                eating.BolusDoseFact = Eating.BolusDoseFact;

                eating.InsulinSensitivityAutoOne = Eating.InsulinSensitivityAutoOne;
                eating.InsulinSensitivityAutoTwo = Eating.InsulinSensitivityAutoTwo;
                eating.InsulinSensitivityAutoThree = Eating.InsulinSensitivityAutoThree;

                eating.InsulinSensitivityUser = Eating.InsulinSensitivityUser;
                eating.InsulinSensitivityFact = Eating.InsulinSensitivityFact;

                eating.AccuracyAuto = Eating.AccuracyAuto;
                eating.AccuracyUser = Eating.AccuracyUser;

                // eating.IsMenstrualCycleStart = Eating.IsMenstrualCycleStart;
                eating.Comment = Eating.Comment;

                eating.EatingTypeId = Eating.EatingType.Id;
                eating.UserId = GlobalParameters.User.Id;

                eating.ExpectedGlucose = Eating.ExpectedGlucose;
                eating.Error = GetError();

                // eating.WriteOff = GlobalParameters.User.BasalType.Duration;
                eating.WorkingTime = Eating.WorkingTime;
                eating.Pause = Eating.Pause;
                eating.ExerciseId = exercise.Id;
                eating.EndEating = Eating.EndEating;

                if (Eating.Id == Guid.Empty)
                    db.Eatings.Add(eating);

                if (IsMenstrualCycleStart)
                    db.MenstrualCycles.Add(new Models.MenstrualCycle()
                    {
                        Id = Guid.NewGuid(),
                        DateStart = DateTime.Now,
                        UserId = GlobalParameters.User.Id
                    });

                db.SaveChanges();

                MessagingCenter.Send(this, "Eating");
                await GlobalParameters.Navigation.PopAsync();
            }
        }

        private bool OkCanExecute() =>
            // Cахар
            GlucoseStart > 0 &&
            (GlucoseEnd == null || GlucoseEnd >= 0) &&
            // Нагрузка
            ExerciseType != null &&
            Duration >= 0 &&
            HoursAfterInjection >= 0 &&
            // Приём пищи
            Protein >= 0 &&
            Fat >= 0 &&
            Carbohydrate >= 0 &&
            // Дополнительно
            BasalDose >= 0 &&
            // Расчёты
            (InsulinSensitivityFact == null || InsulinSensitivityFact >= 0) &&
            (InsulinSensitivityUser == null || InsulinSensitivityUser >= 0) &&
            // Пауза
            Pause >= 0;

        public ICommand OkCommand =>
            new Command(OkExecute);

        #endregion

        #region --Remove

        private async void RemoveExecute()
        {
            bool question = await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                "Удалить?",
                "Вы уверены, что хотите удалить запись?",
                "Да",
                "Нет");

            if (!question)
                return;

            using (var db = new ApplicationContext(GlobalParameters.DbPath))
            {
                var exercise = db.Exercises.Find(Eating.Exercise.Id);
                if (exercise != null)
                    db.Exercises.Remove(exercise);

                var injections = db.Injections
                    .Where(x =>
                        x.EatingId == Eating.Id)
                    .ToList();

                foreach (var injection in injections)
                    db.Injections.Remove(injection);

                var dimensions = db.IntermediateDimensions
                    .Where(x =>
                        x.EatingId == Eating.Id)
                    .ToList();

                foreach (var dimension in dimensions)
                    db.IntermediateDimensions.Remove(dimension);

                var eating = db.Eatings.Find(Eating.Id);
                if (eating != null)
                    db.Eatings.Remove(eating);

                db.SaveChanges();

                MessagingCenter.Send(this, "Eating");
                await GlobalParameters.Navigation.PopAsync();
            }
        }

        public ICommand RemoveCommand =>
            new Command(RemoveExecute);

        #endregion

        #region ~--Injection

        public ICommand AddInjectionCommand =>
            new Command(() =>
            {
                SelectedInjection = new Models.Injection()
                {
                    Id = Guid.NewGuid(),
                    InjectionTime = DateTime.Now.TimeOfDay,
                    InjectionDate = DateTime.Now
                };
                IsModalInjection = true;
            });

        public ICommand EditInjectionCommand =>
            new Command((object obj) =>
            {
                SelectedInjection = (Models.Injection)obj;
                IsModalInjection = true;
            });

        public ICommand RemoveInjectionCommand =>
            new Command(async (object obj) =>
            {
                bool question = await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Удалить?",
                    "Вы уверены, что хотите удалить запись?",
                    "Да", "Нет");

                if (question)
                {
                    Injections.Remove((Models.Injection)obj);
                    CalculateTotal();
                }
            });

        #region ----Save Injection

        private async void SaveInjectionExecute()
        {
            try
            {
                var entity = Injections
                    .FirstOrDefault(x =>
                        x.Id == SelectedInjection.Id);

                if (entity != null)
                    Injections.Remove(entity);

                SelectedInjection.InjectionTime = Calculation.TimeSpanWithoutSeconds(SelectedInjection.InjectionTime);
                Injections.Add(SelectedInjection);

                CalculateTotal();
            }
            catch (Exception ex)
            {
                await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Ошибка",
                    ex.Message + ex?.InnerException?.Message,
                    "Ok");
            }

            IsModalInjection = false;
        }

        public ICommand SaveInjectionCommand =>
            new Command(SaveInjectionExecute);

        #endregion

        public ICommand CancelInjectionCommand =>
            new Command(() => IsModalInjection = false);

        #endregion

        #region ~--Dimension

        public ICommand AddDimensionCommand =>
            new Command(() =>
            {
                SelectedDimension = new Models.IntermediateDimension()
                {
                    Id = Guid.NewGuid(),
                    DimensionTime = DateTime.Now.TimeOfDay,
                    DimensionDate = DateTime.Now
                };
                IsModalDimension = true;
            });

        public ICommand EditDimensionCommand =>
            new Command((object obj) =>
            {
                SelectedDimension = (Models.IntermediateDimension)obj;
                IsModalDimension = true;
            });

        public ICommand RemoveDimensionCommand =>
            new Command(async (object obj) =>
            {
                bool question = await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Удалить?",
                    "Вы уверены, что хотите удалить запись?",
                    "Да", "Нет");

                if (question)
                    IntermediateDimensions.Remove((Models.IntermediateDimension)obj);
            });

        #region ----Save Dimension

        private async void SaveDimensionExecute()
        {
            try
            {
                var entity = IntermediateDimensions
                    .FirstOrDefault(x =>
                        x.Id == SelectedDimension.Id);

                if (entity != null)
                    IntermediateDimensions.Remove(entity);

                SelectedDimension.DimensionTime = Calculation.TimeSpanWithoutSeconds(SelectedDimension.DimensionTime);
                IntermediateDimensions.Add(SelectedDimension);
            }
            catch (Exception ex)
            {
                await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Ошибка",
                    ex.Message + ex?.InnerException?.Message,
                    "Ok");
            }

            IsModalDimension = false;
        }

        public ICommand SaveDimensionCommand =>
            new Command(SaveDimensionExecute);

        #endregion

        public ICommand CancelDimensionCommand =>
            new Command(() => IsModalDimension = false);

        #endregion

        #endregion
    }
}
