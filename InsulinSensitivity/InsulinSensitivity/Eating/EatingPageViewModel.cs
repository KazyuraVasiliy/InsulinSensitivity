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
using System.Runtime.CompilerServices;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Security.Cryptography;
using DataAccessLayer.Migrations;

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
                            .ThenInclude(x => x.BolusType)
                        .Include(x => x.IntermediateDimensions)
                        .Include(x => x.BasalType)
                        .Include(x => x.BolusType)
                        .AsNoTracking()
                        .FirstOrDefault(x =>
                            x.Id == eating.Id);
            }
            else Eating = new Models.Eating()
            {
                DateCreated = DateTime.Now,
                BasalRateCoefficient = GlobalParameters.User.DefaultBasalRateCoefficient
            };

            // Инициализация коллекций
            using (var db = new ApplicationContext(GlobalParameters.DbPath))
            {
                // ... Типы приёмов пищи
                EatingTypes = db.EatingTypes
                    .AsNoTracking()
                    .ToList()
                    .OrderBy(x => 
                        x.TimeStart)
                    .ToList();

                // ... Типы упражнений
                ExerciseTypes = db.ExerciseTypes
                    .AsNoTracking()
                    .Where(x =>
                        x.DateDeleted == null)
                    .OrderBy(x =>
                        x.Name)
                    .ToList();

                // ... Типы инсулинов
                InsulinTypes = db.InsulinTypes
                    .AsNoTracking()
                    .OrderBy(x =>
                        x.Name)
                    .ToList();

                // ... Типы болюсных инсулинов
                BolusInsulinTypes = InsulinTypes
                    .Where(x =>
                        !x.IsBasal)
                    .ToList();
            }

            // Инициализация времени инъекции
            if (eating == null)
            {
                Eating.InjectionTime = Calculation.TimeSpanWithoutSeconds(DateTime.Now.TimeOfDay);
                Eating.EndEating = Calculation.DateTimeWithoutSeconds(DateTime.Now.AddHours(2));
                Eating.BasalInjectionTime = Calculation.DateTimeWithoutSeconds(DateTime.Now);
            }

            // Инициализация инъекций
            if (Eating.Injections != null)
            {
                foreach (var el in Eating.Injections)
                {
                    if (el.BolusType == null)
                        el.BolusType = GlobalParameters.User.BolusType;
                    Injections.Add(el);
                }
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

            if (Eating.BasalType == null)
                Eating.BasalType = GlobalParameters.User.BasalType;

            if (Eating.BolusType == null)
                Eating.BolusType = GlobalParameters.User.BolusType;

            // Инициализация данных для расчёта
            InitPrevious();
            CalculateTotal();
        }

        #endregion

        #region Properties

        #region --System

        /// <summary>
        /// Видно ли поле для ввода базального инсулина
        /// </summary>
        public bool IsBasalDoseVisibility =>
            !GlobalParameters.User.IsPump;

        /// <summary>
        /// Видно ли поле для ввода базальной скорости
        /// </summary>
        public bool IsBasalRateVisibility =>
            GlobalParameters.User.IsPump &&
            GlobalParameters.User.IsActiveBasal;

        /// <summary>
        /// Видно ли поля замены расходных материалов
        /// </summary>
        public bool IsExpendableMaterialsVisibility =>
            GlobalParameters.User.IsPump;

        /// <summary>
        /// Видно ли поле для ввода начала менструального цикла
        /// </summary>
        public bool IsMenstrualCycleStartVisibility =>
            GlobalParameters.IsCycleSettingsAccess &&
            !GlobalParameters.User.IsPregnancy &&
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

        private bool isModalSnack;
        /// <summary>
        /// Отображается ли модальное окно ввода перекуса
        /// </summary>
        public bool IsModalSnack
        {
            get => isModalSnack;
            set
            {
                isModalSnack = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Первый расчёт
        /// </summary>
        private bool IsFirstCalculation = true;

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
        /// Дата последней менструации
        /// </summary>
        private DateTime? LastMenstruationDate { get; set; }

        private decimal? extremum;
        /// <summary>
        /// Верхняя граница
        /// </summary>
        public decimal? Extremum
        {
            get => extremum;
            set
            {
                if (extremum != value)
                {
                    extremum = value;
                    OnPropertyChanged();
                }
            }
        }

        private decimal? infinum;
        /// <summary>
        /// Нижняя граница
        /// </summary>
        public decimal? Infinum
        {
            get => infinum;
            set
            {
                if (infinum != value)
                {
                    infinum = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Дни для расчёта ФЧИ по циклу
        /// </summary>
        private List<DateTime> EquivalentDays { get; set; } =
            new List<DateTime>();

        #endregion

        #region --Cache

        /// <summary>
        /// Приёмы пищи
        /// </summary>
        private List<Models.Eating> Eatings =
            new List<Models.Eating>();

        /// <summary>
        /// Приёмы пищи исключая не учитываемые
        /// </summary>
        private List<Models.Eating> EatingsWithoutIgnored =
            new List<Models.Eating>();

        /// <summary>
        /// Приёмы пищи исключая не учитываемые для расчёта по циклу (включают только ФЧИ фактический, ФЧИ по дню цикла и тип приёма пищи)
        /// </summary>
        private List<Models.Eating> EatingsWithoutIgnoredForCycle =
            new List<Models.Eating>();

        /// <summary>
        /// Средние ФЧИ по приёму пищи
        /// </summary>
        private Dictionary<Guid, decimal?> AverageEatingTypeSensitivityDictionary { get; set; } =
            new Dictionary<Guid, decimal?>();

        /// <summary>
        /// Средние ФЧИ по нагрузкам
        /// </summary>
        private Dictionary<(Guid, int, int), decimal?> AverageExerciseTypeSensitivityDictionary { get; set; } =
            new Dictionary<(Guid, int, int), decimal?>();

        /// <summary>
        /// Средние ФЧИ по дню цикла
        /// </summary>
        private Dictionary<Guid, decimal?> AverageEatingTypeCycleSensitivityDictionary { get; set; } =
            new Dictionary<Guid, decimal?>();

        /// <summary>
        /// Инфинум и Экстремум для текущего типа приёма пищи
        /// </summary>
        private Dictionary<Guid, (decimal? infinum, decimal? extremum)> InfinumExtremumDictionary { get; set; } =
            new Dictionary<Guid, (decimal? infinum, decimal? extremum)>();

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
                if (Eating.Exercise.ExerciseType?.Id != value?.Id)
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

        #region --Insulin Sensitivity Auto

        /// <summary>
        /// ФЧИ рассчитанный по средним
        /// </summary>
        public decimal? InsulinSensitivityAutoOne
        {
            get => Eating.InsulinSensitivityAutoOne;
            set
            {
                Eating.InsulinSensitivityAutoOne = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// ФЧИ рассчитанный по нагрузкам
        /// </summary>
        public decimal? InsulinSensitivityAutoTwo
        {
            get => Eating.InsulinSensitivityAutoTwo;
            set
            {
                Eating.InsulinSensitivityAutoTwo = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// ФЧИ рассчитанный по дню цикла
        /// </summary>
        public decimal? InsulinSensitivityAutoThree
        {
            get => Eating.InsulinSensitivityAutoThree;
            set
            {
                Eating.InsulinSensitivityAutoThree = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// ФЧИ рассчитанный по дню использования канюли
        /// </summary>
        public decimal? InsulinSensitivityAutoFour
        {
            get => Eating.InsulinSensitivityAutoFour;
            set
            {
                Eating.InsulinSensitivityAutoFour = value;
                OnPropertyChanged();
            }
        }

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

        /// <summary>
        /// Вес ФЧИ рассчитанный по средним
        /// </summary>
        public decimal InsulinSensitivityAutoOneWeight { get; private set; } = 1;

        /// <summary>
        /// Вес ФЧИ рассчитанный по нагрузкам
        /// </summary>
        public decimal InsulinSensitivityAutoTwoWeight { get; private set; } = 1;

        /// <summary>
        /// Вес ФЧИ рассчитанный по дню цикла
        /// </summary>
        public decimal InsulinSensitivityAutoThreeWeight { get; private set; } = 1;

        /// <summary>
        /// Вес ФЧИ рассчитанный по дню использования канюли
        /// </summary>
        public decimal InsulinSensitivityAutoFourWeight { get; private set; } = 1;

        #endregion

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

        #region ----Snack

        private int proteinSnack;
        /// <summary>
        /// Белки. Перекус
        /// </summary>
        public int ProteinSnack
        {
            get => proteinSnack;
            set
            {
                if (proteinSnack != value)
                {
                    proteinSnack = value;
                    OnPropertyChanged();
                }
            }
        }

        private int fatSnack;
        /// <summary>
        /// Жиры. Перекус
        /// </summary>
        public int FatSnack
        {
            get => fatSnack;
            set
            {
                if (fatSnack != value)
                {
                    fatSnack = value;
                    OnPropertyChanged();
                }
            }
        }

        private int сarbohydrateSnack;
        /// <summary>
        /// Углеводы. Перекус
        /// </summary>
        public int CarbohydrateSnack
        {
            get => сarbohydrateSnack;
            set
            {
                if (сarbohydrateSnack != value)
                {
                    сarbohydrateSnack = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        private int remainderCarbohydrate;
        /// <summary>
        /// Углеводы с предыдущего приёма пищи
        /// </summary>
        public int RemainderCarbohydrate
        {
            get => remainderCarbohydrate;
            set
            {
                if (remainderCarbohydrate != value)
                {
                    remainderCarbohydrate = value;
                    OnPropertyChanged();

                    OnPropertyChanged(nameof(CarbohydrateTitle));
                }
            }
        }

        /// <summary>
        /// Заголовок строки углеводов
        /// </summary>
        public string CarbohydrateTitle =>
            RemainderCarbohydrate == 0
                ? $"Углеводы:"
                : $"Углеводы (+{RemainderCarbohydrate}):";

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
                if (Eating.BasalInjectionTime == null || Calculation.TimeSpanWithoutSeconds(Eating.BasalInjectionTime.Value.TimeOfDay) != Calculation.TimeSpanWithoutSeconds(value))
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

        /// <summary>
        /// Базальная скорость
        /// </summary>
        public decimal BasalRate
        {
            get => Eating.BasalRate;
            set
            {
                if (Eating.BasalRate != value)
                {
                    Eating.BasalRate = value;
                    CalculateTotal();
                }
            }
        }

        /// <summary>
        /// Временная базальная скорость
        /// </summary>
        public int BasalRateCoefficient
        {
            get => (int)Eating.BasalRateCoefficient;
            set
            {
                if (Eating.BasalRateCoefficient != value)
                {
                    Eating.BasalRateCoefficient = value;
                    CalculateTotal();
                }
            }
        }

        /// <summary>
        /// Тип базального инсулина
        /// </summary>
        public Models.InsulinType BasalType
        {
            get => Eating.BasalType;
            set
            {
                if (Eating.BasalType?.Id != value?.Id)
                {
                    Eating.BasalType = value;
                    CalculateTotal();
                }
            }
        }

        private decimal? basalRecommended;
        /// <summary>
        /// Рекомендуемая доза базального инсулина
        /// </summary>
        public decimal? BasalRecommended
        {
            get => basalRecommended;
            set
            {
                if (basalRecommended != value)
                {
                    basalRecommended = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region --Pump

        /// <summary>
        /// Сообщение связанные с использованием помпы
        /// </summary>
        public string PumpMessages { get; set; } = "";

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
                if (Calculation.TimeSpanWithoutSeconds(Eating.InjectionTime) != Calculation.TimeSpanWithoutSeconds(value))
                {
                    Eating.InjectionTime = value;
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
                if (Eating.EndEating?.Date != value.Date)
                {
                    var begin = Calculation.DateTimeUnionTimeSpan(Eating.DateCreated, Eating.InjectionTime);
                    var end = Calculation.DateTimeUnionTimeSpan(value, EndEatingTime);

                    if (begin.AddHours(2) <= end)
                    {
                        Eating.EndEating = end;
                        CalculateTotal();
                    }
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
                if (Eating.EndEating == null || Calculation.TimeSpanWithoutSeconds(Eating.EndEating.Value.TimeOfDay) != Calculation.TimeSpanWithoutSeconds(value))
                {
                    var begin = Calculation.DateTimeUnionTimeSpan(Eating.DateCreated, Eating.InjectionTime);
                    var end = Calculation.DateTimeUnionTimeSpan(EndEatingDate, value);

                    if (begin.AddHours(2) <= end)
                    {
                        Eating.EndEating = end;
                        CalculateTotal();
                    }
                }
            }
        }

        /// <summary>
        /// Пауза
        /// </summary>
        public int Pause
        {
            get => Eating.Pause;
            set
            {
                if (Eating.Pause != value)
                {
                    Eating.Pause = value;
                    CalculateTotal();
                }
            }
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
                    OnPropertyChanged();

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
                    OnPropertyChanged();

                    CalculateTotal();
                }
            }
        }

        /// <summary>
        /// Тип болюсного инсулина
        /// </summary>
        public Models.InsulinType BolusType
        {
            get => Eating.BolusType;
            set
            {
                if (Eating.BolusType?.Id != value?.Id)
                {
                    Eating.BolusType = value;
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

        private string additionallyInjection;
        /// <summary>
        /// Дробление инъекции
        /// </summary>
        public string AdditionallyInjection
        {
            get => additionallyInjection;
            set
            {
                additionallyInjection = value;
                OnPropertyChanged();
            }
        }

        private DateTime dateTimeWorkingTime;
        /// <summary>
        /// Дата и время отработки (по БЖУ)
        /// </summary>
        public DateTime DateTimeWorkingTime
        {
            get => dateTimeWorkingTime;
            set
            {
                if (dateTimeWorkingTime != value)
                {
                    dateTimeWorkingTime = value;
                    OnPropertyChanged();
                }
            }
        }

        private string assimilatedNutritional;
        /// <summary>
        /// Кол-во усвоенных БЖУ
        /// </summary>
        public string AssimilatedNutritional
        {
            get => assimilatedNutritional;
            set
            {
                assimilatedNutritional = value;
                OnPropertyChanged();
            }
        }

        private string assimilatedNutritionalWithAbsorptionRate;
        /// <summary>
        /// Кол-во усвоенных БЖУ по скорости
        /// </summary>
        public string AssimilatedNutritionalWithAbsorptionRate
        {
            get => assimilatedNutritionalWithAbsorptionRate;
            set
            {
                assimilatedNutritionalWithAbsorptionRate = value;
                OnPropertyChanged();
            }
        }

        private string assimilatedNutritionalStartTime;
        /// <summary>
        /// Время подколок
        /// </summary>
        public string AssimilatedNutritionalStartTime
        {
            get => assimilatedNutritionalStartTime;
            set
            {
                assimilatedNutritionalStartTime = value;
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
                var period = GlobalParameters.User.PeriodOfCalculation;

                // Ограничение для корректного учёта расходных материалов
                if (period < 60 && period > 0)
                    period = 60;

                var query = db.Eatings.AsQueryable();
                if (period != 0)
                {
                    var utcPeriod = DateTime.Now.Date.AddDays(-period).ToFileTimeUtc();
                    query = query
                        .Where(x => x.FileTimeUtcDateCreated >= utcPeriod);
                }

                Eatings = query
                    .Where(x =>
                        x.Id != Eating.Id &&
                        x.InsulinSensitivityFact != null)
                    .OrderByDescending(x =>
                        x.DateCreated)
                    .Include(x => x.Exercise)
                    .Include(x => x.Injections)
                    .Include(x => x.IntermediateDimensions)
                    .Include(x => x.Injections)
                        .ThenInclude(x => x.BolusType)
                    .Include(x => x.BolusType)
                    .Include(x => x.BasalType)
                    .AsNoTracking()
                    .ToList();

                EatingsWithoutIgnored = Eatings
                    .Where(x =>
                        !x.IsIgnored)
                    .ToList();

                // Предыдущие приёмы пищи
                PreviousEatings = EatingsWithoutIgnored
                    .Take(3)
                    .ToList();

                // Предыдущий приём пищи
                var previousEating = (PreviousEatings?.Count ?? 0) > 0
                    ? PreviousEatings[0]
                    : null;

                // Рекомендации по базальному инсулину
                var basals = Eatings
                    .GroupBy(x =>
                        x.DateCreated.Date)
                    .OrderByDescending(x =>
                        x.Key)
                    .Take(20)
                    .ToList();

                var basalsCalculate = new List<(decimal dose, decimal insulinSensitivity)>();
                foreach (var el in basals)
                {
                    var value = el.Sum(x => x.BasalDose);

                    var rateCollection = el.Where(x =>
                        x.BasalRate != 0);

                    if (rateCollection.Count() != 0)
                        value += Math.Round(rateCollection.Average(x => x.BasalRate) * 24, 1, MidpointRounding.AwayFromZero);

                    var insulinSensitivityCollection = el
                        .Where(x =>
                            !x.IsIgnored);

                    var insulinSensitivity = insulinSensitivityCollection.Count() > 0
                        ? insulinSensitivityCollection.Average(x => x.InsulinSensitivityFact)
                        : 0;

                    if (insulinSensitivity != 0 && value != 0)
                        basalsCalculate.Add((value, insulinSensitivity.Value));
                }

                if (PreviousEatings.Count > 0 && basalsCalculate.Count > 0)
                    BasalRecommended = Math.Round(basalsCalculate.Average(x => x.dose * x.insulinSensitivity) /
                        PreviousEatings.Average(x => x.InsulinSensitivityFact.Value), 1, MidpointRounding.AwayFromZero);

                if (GlobalParameters.User.IsPump || GlobalParameters.User.IsMonitoring)
                {
                    var messages = new List<string>(5);
                    for (int i = 0; i < messages.Capacity; i++)
                        messages.Add(null);

                    foreach (var eating in Eatings)
                    {
                        var days = (DateTime.Now.Date - eating.DateCreated.Date).TotalDays;

                        if (eating.IsCannulaReplacement && messages[0] == null)
                            messages[0] = days >= GlobalParameters.User.CannulaLifespan
                                ? $"Канюля используется уже {days} дней."
                                : "";

                        if (eating.IsCatheterReplacement && messages[1] == null)
                            messages[1] = days >= GlobalParameters.User.CatheterLifespan
                                ? $"Катетер используется уже {days} дней."
                                : "";

                        if (eating.IsCartridgeReplacement && messages[2] == null)
                            messages[2] = days >= GlobalParameters.User.CartridgeLifespan
                                ? $"Картридж используется уже {days} дней."
                                : "";

                        if (eating.IsBatteryReplacement && messages[3] == null)
                             messages[3] = days >= GlobalParameters.User.BatteryLifespan
                                ? $"Батарейка используется уже {days} дней."
                                : "";

                        if (eating.IsMonitoringReplacement && messages[4] == null)
                            messages[4] = days >= GlobalParameters.User.MonitoringLifespan
                               ? $"Сенсор используется уже {days} дней."
                               : "";
                    }

                    messages = messages
                        .Where(x =>
                            !string.IsNullOrWhiteSpace(x))
                        .ToList();
                    PumpMessages = string.Join("\n", messages);

                    if (!string.IsNullOrWhiteSpace(PumpMessages))
                        PumpMessages += $"\nЗамените издели{(messages.Count == 1 ? "e" : "я")} на нов{(messages.Count == 1 ? "ое" : "ые")}";

                    CalculateInsulinSensitivityFour();
                }

                // Исходный сахар
                if (previousEating?.GlucoseEnd != null && Eating.Id == Guid.Empty)
                    Eating.GlucoseStart = previousEating.GlucoseEnd.Value;

                // Базальная скорость
                if (IsBasalRateVisibility && Eating.Id == Guid.Empty)
                    Eating.BasalRate = previousEating?.BasalRate ?? 0;

                // Получение сахара и базальной скорости из Nightscout
                if (GlobalParameters.User.IsNightscoutStartParameters && !string.IsNullOrWhiteSpace(GlobalParameters.User.NightscoutUri) && Eating.Id == Guid.Empty)
                {
                    try
                    {
                        var baseUri = GlobalParameters.User.NightscoutUri.TrimEnd('/');
                        var date = DateTimeOffset.Now;

                        using (var client = new HttpClient())
                        {
                            client.Timeout = TimeSpan.FromSeconds(5);

                            if (!string.IsNullOrWhiteSpace(GlobalParameters.User.NightscoutApiKey))
                            {
                                var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(GlobalParameters.User.NightscoutApiKey));
                                var hashStr = string.Concat(hash.Select(b => b.ToString("x2")));

                                client.DefaultRequestHeaders.Add("api-secret", hashStr);
                            }                            

                            // Проверка доступа к серверу
                            var result = client.GetAsync(baseUri + "/status").Result;
                            if (!result.IsSuccessStatusCode)
                                throw new Exception("Нет доступа к серверу");

                            // Получение текущего сахара
                            result = client.GetAsync(baseUri + $"/entries.json?find[dateString][$gte]={date.Subtract(DateTimeOffset.Now.Offset):yyyy-MM-dd}&count=1").Result;
                            if (!result.IsSuccessStatusCode)
                                throw new Exception("Не удалось получить данные о текущем сахаре");

                            var data = result.Content.ReadAsStringAsync().Result;
                            var glucose = JsonConvert.DeserializeObject<List<BusinessLogicLayer.Service.Models.NightscoutEntry>>(data)?.FirstOrDefault();

                            if (glucose == null)
                                throw new Exception("Нет данных о текущем сахаре");

                            if (Math.Abs((date - glucose.dateString).TotalMinutes) > 10)
                                throw new Exception("Данные о текущем сахаре устарели более чем на 10 минут");

                            Eating.GlucoseStart = Math.Round(glucose.sgv / 18, 1);

                            // Получение подколок
                            result = client.GetAsync(baseUri + $"/treatments.json?find[eventType]=Temp%20Basal&count=1").Result;
                            if (!result.IsSuccessStatusCode)
                                throw new Exception("Не удалось получить данные о БС");

                            data = result.Content.ReadAsStringAsync().Result;
                            var insulin = JsonConvert.DeserializeObject<List<BusinessLogicLayer.Service.Models.NightscoutTreatment>>(data).FirstOrDefault();

                            if (insulin != null)
                            {
                                var percent = (int)Math.Round((insulin.percent ?? 0) + 100, 0);

                                BasalRateCoefficient = percent;
                                BasalRate = percent != 0
                                    ? Math.Round((insulin.rate ?? 0) * 100 / percent, 2)
                                    : 0;
                            }
                        }
                    }
                    catch { }
                }

                // Средний ФЧИ предыдущего типа приёма пищи
                if (previousEating != null)
                {
                    var previousAverageEatingTypeSensitivityQuery = EatingsWithoutIgnored
                        .Where(x =>
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
                        var exercisesQuery = EatingsWithoutIgnored
                            .Where(x =>
                                x.Exercise.ExerciseTypeId == previous.Exercise.ExerciseTypeId &&
                                x.Exercise.HoursAfterInjection == previous.Exercise.HoursAfterInjection);

                        if (GlobalParameters.User.PeriodOfCalculation > 0)
                            exercisesQuery = exercisesQuery
                                .Where(x =>
                                    x.DateCreated.Date >= Period.Date);
                        
                        var exercises = exercisesQuery
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
                Basals = Eatings
                    .Where(x =>
                        x.BasalDose != 0M)
                    .Take(4)
                    .ToList();

                // Эквивалентный день предыдущего цикла
                if (GlobalParameters.User.Gender == false)
                {
                    var cycles = db.MenstrualCycles
                        .AsNoTracking()
                        .OrderBy(x =>
                            x.DateStart)
                        .ToList();

                    if ((cycles?.Count ?? 0) > 0)
                    {
                        var cycleDay = (int)Math.Round((Eating.DateCreated.Date - cycles.Last().DateStart.Date).TotalDays, 0, MidpointRounding.AwayFromZero);

                        for (int i = 0; i < cycles.Count; i++)
                        {
                            var equivalentDay = cycles[i].DateStart.AddDays(cycleDay);
                            if ((i != (cycles.Count - 1)) && equivalentDay.Date < cycles[i + 1].DateStart.Date)
                                EquivalentDays.Add(equivalentDay);

                            if (i == (cycles.Count - 1))
                                EquivalentDays.Add(equivalentDay);
                        }

                        var tDates = new List<DateTime>();
                        foreach (var date in EquivalentDays)
                        {
                            tDates.Add(date.AddDays(-1));
                            tDates.Add(date.AddDays(1));
                        }
                        EquivalentDays.AddRange(tDates);
                    }

                    if ((cycles?.Count ?? 0) > 0)
                        LastMenstruationDate = cycles.Last().DateStart;

                    if (EquivalentDays.Count > 0)
                    {
                        var equivalentDaysUtc = EquivalentDays
                            .Select(x => x.Date.ToFileTimeUtc())
                            .ToList();

                        EatingsWithoutIgnoredForCycle = db.Eatings
                            .AsNoTracking()
                            .Where(x =>
                                equivalentDaysUtc.Contains(x.FileTimeUtcDateCreated) &&
                                x.Id != Eating.Id &&
                                x.InsulinSensitivityFact != null &&
                                !x.IsIgnored)
                            .Select(x =>
                                new Models.Eating()
                                {
                                    InsulinSensitivityFact = x.InsulinSensitivityFact,
                                    InsulinSensitivityAutoThree = x.InsulinSensitivityAutoThree,
                                    EatingTypeId = x.EatingTypeId
                                })
                            .ToList();
                    }
                }

                // Расчёт весов ФЧИ
                if (GlobalParameters.User.IsWeightedAverageInsulinSensitivity)
                {
                    // ... Формула 1
                    var eatings = EatingsWithoutIgnored
                        .Where(x =>
                            x.InsulinSensitivityFact != null &&
                            x.InsulinSensitivityAutoOne != null);

                    if (GlobalParameters.User.PeriodOfCalculation > 0)
                        eatings = eatings
                            .Where(x =>
                                x.DateCreated.Date >= Period.Date);

                    if (eatings.Count() > 0)
                        InsulinSensitivityAutoOneWeight = (decimal)eatings
                            .Average(x =>
                            {
                                var divider = x.InsulinSensitivityFact.Value > x.InsulinSensitivityAutoOne.Value
                                    ? x.InsulinSensitivityFact.Value
                                    : x.InsulinSensitivityAutoOne.Value;

                                return (int)Methods.Round(100 - Math.Abs(x.InsulinSensitivityFact.Value - x.InsulinSensitivityAutoOne.Value) / divider * 100, 0);
                            });

                    InsulinSensitivityAutoOneWeight = Methods.Round(InsulinSensitivityAutoOneWeight, 0);

                    // ... Формула 2
                    eatings = EatingsWithoutIgnored
                        .Where(x =>
                            x.InsulinSensitivityFact != null &&
                            x.InsulinSensitivityAutoTwo != null);

                    if (GlobalParameters.User.PeriodOfCalculation > 0)
                        eatings = eatings
                            .Where(x =>
                                x.DateCreated.Date >= Period.Date);

                    if (eatings.Count() > 0)
                        InsulinSensitivityAutoTwoWeight = (decimal)eatings
                            .Average(x =>
                            {
                                var divider = x.InsulinSensitivityFact.Value > x.InsulinSensitivityAutoTwo.Value
                                    ? x.InsulinSensitivityFact.Value
                                    : x.InsulinSensitivityAutoTwo.Value;

                                return (int)Methods.Round(100 - Math.Abs(x.InsulinSensitivityFact.Value - x.InsulinSensitivityAutoTwo.Value) / divider * 100, 0);
                            });

                    InsulinSensitivityAutoTwoWeight = Methods.Round(InsulinSensitivityAutoTwoWeight, 0);

                    // ... Формула 3
                    eatings = EatingsWithoutIgnoredForCycle
                        .Where(x =>
                            x.InsulinSensitivityFact != null &&
                            x.InsulinSensitivityAutoThree != null);

                    if (eatings.Count() > 0)
                        InsulinSensitivityAutoThreeWeight = (decimal)eatings
                            .Average(x =>
                            {
                                var divider = x.InsulinSensitivityFact.Value > x.InsulinSensitivityAutoThree.Value
                                    ? x.InsulinSensitivityFact.Value
                                    : x.InsulinSensitivityAutoThree.Value;

                                return (int)Methods.Round(100 - Math.Abs(x.InsulinSensitivityFact.Value - x.InsulinSensitivityAutoThree.Value) / divider * 100, 0);
                            });

                    InsulinSensitivityAutoThreeWeight = Methods.Round(InsulinSensitivityAutoThreeWeight, 0);

                    // ... Формула 4
                    var lastMonth = DateTime.Now.AddMonths(-1);

                    eatings = EatingsWithoutIgnored
                        .Where(x =>
                            x.InsulinSensitivityFact != null &&
                            x.InsulinSensitivityAutoFour != null &&
                            x.DateCreated.Date >= lastMonth.Date);

                    if (eatings.Count() > 0)
                        InsulinSensitivityAutoFourWeight = (decimal)eatings
                            .Average(x =>
                            {
                                var divider = x.InsulinSensitivityFact.Value > x.InsulinSensitivityAutoFour.Value
                                    ? x.InsulinSensitivityFact.Value
                                    : x.InsulinSensitivityAutoFour.Value;

                                return (int)Methods.Round(100 - Math.Abs(x.InsulinSensitivityFact.Value - x.InsulinSensitivityAutoFour.Value) / divider * 100, 0);
                            });

                    InsulinSensitivityAutoFourWeight = Methods.Round(InsulinSensitivityAutoFourWeight, 0);
                }
            }
        }

        /// <summary>
        /// Рассчитывает автоматический ФЧИ по первой формуле
        /// </summary>
        private void CalculateInsulinSensitivityOne()
        {
            // Рассчёт ФЧИ по первой формуле
            InsulinSensitivityAutoOne = null;
            bool check =
                GlobalParameters.User.IsAverageCalculateActive &&
                (PreviousEatings?.Count ?? 0) > 0 &&
                PreviousEatings[0].InsulinSensitivityFact != null &&
                (PreviousAverageEatingTypeSensitivity ?? 0) != 0 &&
                EatingType != null;

            if (check)
            {
                decimal? averageEatingTypeSensitivity = null;

                if (AverageEatingTypeSensitivityDictionary.ContainsKey(EatingType.Id))
                    averageEatingTypeSensitivity = AverageEatingTypeSensitivityDictionary[EatingType.Id];
                else
                {
                    var averageEatingTypeSensitivityCollectionQuery = EatingsWithoutIgnored
                        .Where(x =>
                            x.EatingTypeId == EatingType.Id &&
                            x.InsulinSensitivityFact != null);

                    if (GlobalParameters.User.PeriodOfCalculation > 0)
                        averageEatingTypeSensitivityCollectionQuery = averageEatingTypeSensitivityCollectionQuery
                            .Where(x =>
                                x.DateCreated.Date >= Period.Date);

                    // Средний ФЧИ текущего типа приёма пищи
                    var averageEatingTypeSensitivityCollection = averageEatingTypeSensitivityCollectionQuery
                        .ToList();

                    averageEatingTypeSensitivity = (averageEatingTypeSensitivityCollection?.Count ?? 0) > 0
                        ? averageEatingTypeSensitivityCollection
                            .Average(x =>
                                x.InsulinSensitivityFact)
                        : null;

                    AverageEatingTypeSensitivityDictionary.Add(EatingType.Id, averageEatingTypeSensitivity);
                }

                if (averageEatingTypeSensitivity != null)
                    InsulinSensitivityAutoOne = PreviousEatings[0].InsulinSensitivityFact * (averageEatingTypeSensitivity / PreviousAverageEatingTypeSensitivity);
            }
        }

        /// <summary>
        /// Рассчитывает автоматический ФЧИ по второй формуле
        /// </summary>
        private void CalculateInsulinSensitivityTwo()
        {
            // Расчёт ФЧИ по второй формуле
            InsulinSensitivityAutoTwo = null;
            var check =
                GlobalParameters.User.IsExerciseCalculateActive &&
                (PreviousEatings?.Count ?? 0) == 3 &&
                PreviousEatings.All(x => x.InsulinSensitivityFact != null) &&
                (PreviousAverageExerciseTypeSensitivitys?.Count ?? 0) == 3 &&
                PreviousAverageExerciseTypeSensitivitys.All(x => (x ?? 0) != 0) &&
                ExerciseType != null;

            if (check)
            {
                decimal? averageExerciseTypeSensitivity = null;

                if (AverageExerciseTypeSensitivityDictionary.ContainsKey((ExerciseType.Id, HoursAfterInjection, Duration)))
                    averageExerciseTypeSensitivity = AverageExerciseTypeSensitivityDictionary[(ExerciseType.Id, HoursAfterInjection, Duration)];
                else
                {
                    var averageExerciseTypeSensitivityCollectionQuery = EatingsWithoutIgnored
                        .Where(x =>
                            x.Exercise.ExerciseTypeId == ExerciseType.Id &&
                            x.Exercise.HoursAfterInjection == HoursAfterInjection);

                    if (GlobalParameters.User.PeriodOfCalculation > 0)
                        averageExerciseTypeSensitivityCollectionQuery = averageExerciseTypeSensitivityCollectionQuery
                            .Where(x =>
                                x.DateCreated.Date >= Period.Date);

                    // Средний ФЧИ текущего типа нагрузки
                    var averageExerciseTypeSensitivityCollection = averageExerciseTypeSensitivityCollectionQuery
                        .ToList();

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

                    AverageExerciseTypeSensitivityDictionary.Add((ExerciseType.Id, HoursAfterInjection, Duration), averageExerciseTypeSensitivity);
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
                    if (!GlobalParameters.User.IsPump && !GlobalParameters.User.IsActiveBasal)
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

                    InsulinSensitivityAutoTwo = average * averageExerciseTypeSensitivity * basal;
                }
            }
        }

        /// <summary>
        /// Рассчитывает автоматический ФЧИ по третьей формуле
        /// </summary>
        private void CalculateInsulinSensitivityThree()
        {
            // Рассчёт ФЧИ по третьей формуле
            InsulinSensitivityAutoThree = null;
            var check =
                GlobalParameters.User.IsCycleCalculateActive &&
                !GlobalParameters.User.Gender &&
                EatingType != null;

            if (check)
            {
                decimal? averageEatingTypeCycleSensitivity = null;

                if (AverageEatingTypeCycleSensitivityDictionary.ContainsKey(EatingType.Id))
                    averageEatingTypeCycleSensitivity = AverageEatingTypeCycleSensitivityDictionary[EatingType.Id];
                else
                {
                    var averageEatingTypeSensitivityCollection = EatingsWithoutIgnoredForCycle
                        .Where(x => x.EatingTypeId == EatingType.Id)
                        .ToList();

                    if ((averageEatingTypeSensitivityCollection?.Count ?? 0) > 0)
                        averageEatingTypeCycleSensitivity = averageEatingTypeSensitivityCollection
                            .Average(x =>
                                x.InsulinSensitivityFact);

                    AverageEatingTypeCycleSensitivityDictionary.Add(EatingType.Id, averageEatingTypeCycleSensitivity);
                }

                InsulinSensitivityAutoThree = averageEatingTypeCycleSensitivity;
            }
        }

        /// <summary>
        /// Рассчитывает автоматический ФЧИ по четвёртой формуле
        /// </summary>
        private void CalculateInsulinSensitivityFour()
        {
            InsulinSensitivityAutoFour = null;

            if (GlobalParameters.User.IsCannulaCalculateActive && GlobalParameters.User.IsPump)
            {
                var lastMonth = DateTime.Now.AddMonths(-1);

                var days = EatingsWithoutIgnored
                    .Where(x => x.DateCreated.Date >= lastMonth.Date)
                    .GroupBy(x => x.DateCreated.Date)
                    .OrderByDescending(x => x.Key)
                    .ToList();

                // Средний ФЧИ за вчерашний день
                var yesterday = days
                    .FirstOrDefault(x =>
                        x.Key.Date == Eating.DateCreated.Date.AddDays(-1));

                decimal? yesterdayInsulinSensitivity = null;
                if (yesterday != null)
                    yesterdayInsulinSensitivity = yesterday
                        .Average(x =>
                            x.InsulinSensitivityFact.Value);

                if (yesterdayInsulinSensitivity == null)
                    return;

                // Вычисления дня использования канюли вчера и сегодня
                double? todayCannula = null;
                double? yesterdayCannula = null;

                foreach (var eating in Eatings)
                {
                    if (eating.IsCannulaReplacement)
                    {
                        if (todayCannula == null)
                            todayCannula = (Eating.DateCreated.Date - eating.DateCreated.Date).TotalDays;

                        if ((yesterdayCannula ?? -1) < 0)
                            yesterdayCannula = (Eating.DateCreated.Date.AddDays(-1) - eating.DateCreated.Date).TotalDays;
                    }

                    if (todayCannula != null && (yesterdayCannula ?? -1) > 0)
                        break;
                }

                if (todayCannula == null || yesterdayCannula == null)
                    return;

                // Вычисление эквивалентных дней
                var daysCannulaReplacement = new List<DateTime>();
                foreach (var day in days)
                {
                    var isCannulaReplacement = day
                        .Any(x =>
                            x.IsCannulaReplacement);

                    if (isCannulaReplacement)
                        daysCannulaReplacement.Add(day.Key.Date);
                }

                daysCannulaReplacement = daysCannulaReplacement
                    .OrderBy(x => x)
                    .ToList();

                var todayEquivalent = new List<DateTime>();
                var yesterdayEquivalent = new List<DateTime>();

                if ((daysCannulaReplacement?.Count ?? 0) > 0)
                {
                    for (int i = 0; i < daysCannulaReplacement.Count; i++)
                    {
                        var equivalentDay = daysCannulaReplacement[i].AddDays(todayCannula.Value);

                        if ((i != (daysCannulaReplacement.Count - 1)) && equivalentDay.Date < daysCannulaReplacement[i + 1].Date)
                            todayEquivalent.Add(equivalentDay);

                        if (i == (daysCannulaReplacement.Count - 1))
                            todayEquivalent.Add(equivalentDay);

                        equivalentDay = daysCannulaReplacement[i].AddDays(yesterdayCannula.Value);

                        if ((i != (daysCannulaReplacement.Count - 1)) && equivalentDay.Date < daysCannulaReplacement[i + 1].Date)
                            yesterdayEquivalent.Add(equivalentDay);

                        if (i == (daysCannulaReplacement.Count - 1))
                            yesterdayEquivalent.Add(equivalentDay);
                    }
                }

                if (todayEquivalent.Count == 0 || yesterdayEquivalent.Count == 0)
                    return;

                // Средний ФЧИ за последний месяц с днём канюли, как сегодня
                var averageTodayCollection = EatingsWithoutIgnored
                    .Where(x =>
                        x.InsulinSensitivityFact != null &&
                        todayEquivalent
                            .Any(y =>
                                y.Date == x.DateCreated.Date))
                    .ToList();

                decimal? averageToday = null;
                if ((averageTodayCollection?.Count ?? 0) > 0)
                    averageToday = averageTodayCollection
                        .Average(x =>
                            x.InsulinSensitivityFact);

                if (averageToday == null)
                    return;

                // Средний ФЧИ за последний месяц с днём канюли, как вчера
                var averageYesterdayCollection = EatingsWithoutIgnored
                    .Where(x =>
                        x.InsulinSensitivityFact != null &&
                        yesterdayEquivalent
                            .Any(y =>
                                y.Date == x.DateCreated.Date))
                    .ToList();

                decimal? averageYesterday = null;
                if ((averageYesterdayCollection?.Count ?? 0) > 0)
                    averageYesterday = averageYesterdayCollection
                        .Average(x =>
                            x.InsulinSensitivityFact);

                if (averageYesterday == null)
                    return;

                InsulinSensitivityAutoFour = yesterdayInsulinSensitivity * averageToday / averageYesterday;
            }
        }

        /// <summary>
        /// Рассчитывает автоматический ФЧИ (средний)
        /// </summary>
        private void CalculateInsulinSensitivityAuto()
        {
            var values = new List<(decimal? insulinSensitivity, decimal weight)>()
            {
                (Eating.InsulinSensitivityAutoOne, InsulinSensitivityAutoOneWeight),
                (Eating.InsulinSensitivityAutoTwo, InsulinSensitivityAutoTwoWeight),
                (Eating.InsulinSensitivityAutoThree, InsulinSensitivityAutoThreeWeight),
                (Eating.InsulinSensitivityAutoFour, InsulinSensitivityAutoFourWeight)
            };

            values = values
                .Where(x => x.insulinSensitivity != null)
                .ToList();

            if (Infinum != null || Extremum != null)
            {
                for (int i = 0; i < (values?.Count ?? 0); i++)
                {
                    if (Infinum != null && values[i].insulinSensitivity < Infinum)
                        values[i] = (Infinum, values[i].weight);

                    if (Extremum != null && values[i].insulinSensitivity > Extremum)
                        values[i] = (Extremum, values[i].weight);
                }
            }

            InsulinSensitivityAuto = (values?.Count ?? 0) == 0 && values.Sum(x => x.weight) > 0
                ? (decimal?)null
                : Math.Round(values.Sum(x => x.insulinSensitivity.Value * x.weight) / values.Sum(x => x.weight), 3, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Расчёт дозы болюсного инсулина
        /// </summary>
        private void CalculateBolusDose(int carbohydrate, int protein, int fat)
        {
            var bolusDose = (InsulinSensitivityAuto ?? 0) != 0 || (InsulinSensitivityUser ?? 0) != 0
                ? Calculation.GetBolusDose(GlucoseStart, GlobalParameters.User.TargetGlucose,
                    GlobalParameters.User.CarbohydrateCoefficient, GlobalParameters.User.ProteinCoefficient, GlobalParameters.User.FatCoefficient,
                    protein, fat, carbohydrate,
                    (InsulinSensitivityUser ?? 0) != 0
                        ? InsulinSensitivityUser.Value
                        : InsulinSensitivityAuto.Value) - GlobalMethods.GetActiveInsulin(Eating, Injections,
                            Calculation.DateTimeUnionTimeSpan(Eating.DateCreated, Eating.InjectionTime), Eating.EndEating, Eating.Id, true,
                            Eating.Carbohydrate, Eating.Pause, Eatings).insulin
                : (decimal?)null;

            if (bolusDose != null && Eating.EndEating != null)
            {
                var begin = Calculation.DateTimeUnionTimeSpan(Eating.DateCreated, Eating.InjectionTime);
                bolusDose = begin == Eating.EndEating.Value
                    ? null 
                    : bolusDose / (decimal)(1 - Calculation.GetActiveInsulinPercent(
                        begin, Eating.EndEating.Value, (int)BolusType.Duration, BolusType.Profile));
            }                

            BolusDoseCalculate = bolusDose != null
                ? bolusDose
                : (decimal?)null;

            // Вычисление соотношения инсулина на У и БЖ
            var proteinAndFat = Protein * GlobalParameters.User.ProteinCoefficient +
                Fat * GlobalParameters.User.FatCoefficient;

            BolusDoseCarbohydrate = BolusDoseCalculate != null
                ? (proteinAndFat + Carbohydrate) == 0
                    ? 0
                    : Math.Round(BolusDoseCalculate.Value / (proteinAndFat + Carbohydrate) * Carbohydrate, 2, MidpointRounding.AwayFromZero)
                : (decimal?)null;

            BolusDoseFatAndProtein = BolusDoseCalculate != null
                ? (proteinAndFat + Carbohydrate) == 0
                    ? 0
                    : Math.Round(BolusDoseCalculate.Value / (proteinAndFat + Carbohydrate) * proteinAndFat, 2, MidpointRounding.AwayFromZero)
                : (decimal?)null;
        }

        /// <summary>
        /// Расчёт ожидаемого сахара
        /// </summary>
        private void CalculateExpectedGlucose(int carbohydrate, int protein, int fat)
        {
            ExpectedGlucose = (InsulinSensitivityAuto != null || InsulinSensitivityUser != null)
                ? Calculation.GetExpectedGlucose(GlucoseStart, BolusDoseTotal,
                    GlobalParameters.User.CarbohydrateCoefficient, GlobalParameters.User.ProteinCoefficient, GlobalParameters.User.FatCoefficient,
                    protein, fat, carbohydrate,
                    InsulinSensitivityUser != null
                        ? InsulinSensitivityUser.Value
                        : InsulinSensitivityAuto.Value)
                : (decimal?)null;
        }

        /// <summary>
        /// Расчёт фактического ФЧИ
        /// </summary>
        private void CalculateInsulinSensitivityFact(int carbohydrate, int protein, int fat)
        {
            InsulinSensitivityFact = BolusDoseTotal > 0 && GlucoseEnd != null
                ? Calculation.GetInsulinSensitivityFact(GlucoseStart, GlucoseEnd.Value,
                    GlobalParameters.User.CarbohydrateCoefficient, GlobalParameters.User.ProteinCoefficient, GlobalParameters.User.FatCoefficient,
                    protein, fat, carbohydrate,
                    BolusDoseTotal)
                : (decimal?)null;
        }

        /// <summary>
        /// Расчёт ФЧИ пользователя (если он не введён)
        /// </summary>
        private void CalculateInsulinSensitivityUser(int carbohydrate, int protein, int fat)
        {
            Eating.InsulinSensitivityUser = BolusDoseTotal > 0 && GlucoseEnd != null && InsulinSensitivityUser == null
                ? Math.Round(Calculation.GetInsulinSensitivityFact(GlucoseStart, GlobalParameters.User.TargetGlucose,
                    GlobalParameters.User.CarbohydrateCoefficient, GlobalParameters.User.ProteinCoefficient, GlobalParameters.User.FatCoefficient,
                    protein, fat, carbohydrate,
                    BolusDoseTotal), 3, MidpointRounding.AwayFromZero)
                : InsulinSensitivityUser;
            OnPropertyChanged(nameof(InsulinSensitivityUser));
        }

        /// <summary>
        /// Расчёт точности расчётного ФЧИ
        /// </summary>
        private void CalculateAccuracyAuto()
        {
            if ((InsulinSensitivityFact ?? 0) != 0 && InsulinSensitivityAuto != null)
            {
                var divider = InsulinSensitivityFact.Value > InsulinSensitivityAuto.Value
                    ? InsulinSensitivityFact.Value
                    : InsulinSensitivityAuto.Value;

                Eating.AccuracyAuto = (int)Math.Round(100 - Math.Abs(InsulinSensitivityFact.Value - InsulinSensitivityAuto.Value) / divider * 100, 0, MidpointRounding.AwayFromZero);
            }
            else Eating.AccuracyAuto = null;
        }

        /// <summary>
        /// Расчёт точности ФЧИ пользователя
        /// </summary>
        private void CalculateAccuracyUser()
        {
            if ((InsulinSensitivityFact ?? 0) != 0 && InsulinSensitivityUser != null)
            {
                var divider = InsulinSensitivityFact.Value > InsulinSensitivityUser.Value
                    ? InsulinSensitivityFact.Value
                    : InsulinSensitivityUser.Value;

                Eating.AccuracyUser = (int)Math.Round(100 - Math.Abs(InsulinSensitivityFact.Value - InsulinSensitivityUser.Value) / divider * 100, 0, MidpointRounding.AwayFromZero);
            }
            else Eating.AccuracyUser = null;
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
                if (InfinumExtremumDictionary.ContainsKey(EatingType.Id))
                {
                    var value = InfinumExtremumDictionary[EatingType.Id];

                    Infinum = value.infinum;
                    Extremum = value.extremum;
                }
                else
                {
                    var yesterday = DateTime.Now.AddDays(-1);
                    var ratioCollectionQuery = EatingsWithoutIgnored
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
                        var min = ratios.Min();
                        var max = ratios.Max();

                        if (min < 0.95M)
                            Infinum = PreviousEatings[0].InsulinSensitivityFact.Value * min;

                        if (max > 1.05M)
                            Extremum = PreviousEatings[0].InsulinSensitivityFact.Value * max;
                    }

                    InfinumExtremumDictionary.Add(EatingType.Id, (Infinum, Extremum));
                }
            }
        }

        /// <summary>
        /// Определяет, какую ошибку компенсации совершил пользователь
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
        /// Определяет причину ошибки прогноза
        /// </summary>
        /// <returns></returns>
        private string GetForecastError()
        {
            if (Eating.InsulinSensitivityFact == null || Eating.AccuracyAuto == null || Eating.AccuracyAuto > 85)
                return null;

            if (Eating.InsulinSensitivityAutoTwo == null && GlobalParameters.User.IsExerciseCalculateActive)
                return "Новая активность";

            var values = new List<decimal>()
            {
                Math.Abs(Eating.InsulinSensitivityFact.Value - (Eating.InsulinSensitivityAutoOne ?? Eating.InsulinSensitivityFact.Value)),
                Math.Abs(Eating.InsulinSensitivityFact.Value - (Eating.InsulinSensitivityAutoTwo ?? Eating.InsulinSensitivityFact.Value)),
                Math.Abs(Eating.InsulinSensitivityFact.Value - (Eating.InsulinSensitivityAutoThree ?? Eating.InsulinSensitivityFact.Value))
            };

            var max = values.Max();
            var index = values.IndexOf(max);

            if (index == 0 && Eating.InsulinSensitivityAutoOne != null)
                return $"Резкая смена потребности\n(ФЧИ по средним {Math.Round(Eating.InsulinSensitivityAutoOne.Value, 3, MidpointRounding.AwayFromZero)})";
            if (index == 1 && Eating.InsulinSensitivityAutoTwo != null)
                return $"Нетипичная активность\n(ФЧИ по нагрузке {Math.Round(Eating.InsulinSensitivityAutoTwo.Value, 3, MidpointRounding.AwayFromZero)})";
            if (index == 2 && Eating.InsulinSensitivityAutoThree != null)
                return $"Чувствительность в текущий день цикла отличается от предыдущих\n(ФЧИ по дню цикла {Math.Round(Eating.InsulinSensitivityAutoThree.Value, 3, MidpointRounding.AwayFromZero)})";

            return null;
        }

        /// <summary>
        /// Задаёт время отработки пищи
        /// </summary>
        private void SetWorkingTime()
        {
            var workingTime = Math.Round(-15 + Eating.Pause + (
                (Carbohydrate / (double)GlobalParameters.User.AbsorptionRateOfCarbohydrates) + 
                (Protein / (double)GlobalParameters.User.AbsorptionRateOfProteins) + 
                (Fat / (double)GlobalParameters.User.AbsorptionRateOfFats)) * 60, 0, MidpointRounding.AwayFromZero);

            if (workingTime < 120)
                workingTime = 120;

            DateTimeWorkingTime = Calculation.DateTimeUnionTimeSpan(Eating.DateCreated, Eating.InjectionTime).AddMinutes(workingTime);
            Eating.WorkingTime = InjectionTime.Add(new TimeSpan(0, (int)workingTime, 0));

            Eating.EndEating = Calculation.DateTimeUnionTimeSpan(Eating.DateCreated, Eating.InjectionTime).AddMinutes(workingTime);
            OnPropertyChanged(nameof(EndEatingTime));
            OnPropertyChanged(nameof(EndEatingDate));
        }

        /// <summary>
        /// Вычисляет, необходимо ли раздробить дозу
        /// </summary>
        /// <returns></returns>
        private string GetIsAdditionallyInjection()
        {
            var time = Eating.WorkingTime - TimeSpan.FromHours((double)BolusType.Duration) - TimeSpan.FromMinutes(BolusType.Offset);
            if ((time - Eating.InjectionTime).TotalMinutes >= 15)
                return $"Внимание! Доза на эту еду должна вводиться дробно!\nПоследняя инъекция: {time.Hours:00}:{time.Minutes:00}";
            return "";
        }

        /// <summary>
        /// Вычисляет кол-во усвоенных БЖУ
        /// </summary>
        /// <returns></returns>
        private string GetAssimilatedNutritional()
        {
            if (Eating.GlucoseEnd != null || (InsulinSensitivityAuto == null && InsulinSensitivityUser == null) || (IntermediateDimensions?.Count ?? 0) == 0)
                return "";
            
            var insulinSensitivity = InsulinSensitivityUser != null
                ? InsulinSensitivityUser.Value
                : InsulinSensitivityAuto.Value;

            var lastDimension = IntermediateDimensions
                .OrderByDescending(x =>
                    x.DimensionDate.Date)
                .ThenByDescending(x =>
                    x.DimensionTime)
                .FirstOrDefault();

            var dateTimeLastDimension = Calculation.DateTimeUnionTimeSpan(lastDimension.DimensionDate, lastDimension.DimensionTime);
            var active = GlobalMethods.GetActiveInsulin(beginPeriod: dateTimeLastDimension);

            var delta = (BolusDoseTotal - active.insulin) * insulinSensitivity +
                lastDimension.Glucose - Eating.GlucoseStart;

            // Углеводы
            var carbohydrateCondition = Eating.Carbohydrate * GlobalParameters.User.CarbohydrateCoefficient - delta;
            var carbohydrate = carbohydrateCondition > 0
                ? delta / GlobalParameters.User.CarbohydrateCoefficient
                : Eating.Carbohydrate;

            // Белки
            var proteinCondition = Eating.Protein * GlobalParameters.User.ProteinCoefficient * GlobalParameters.User.CarbohydrateCoefficient - 
                (delta - carbohydrate * GlobalParameters.User.CarbohydrateCoefficient);
            var protein = carbohydrateCondition > 0
                ? 0
                : proteinCondition > 0
                    ? (delta - carbohydrate * GlobalParameters.User.CarbohydrateCoefficient) / (GlobalParameters.User.ProteinCoefficient * GlobalParameters.User.CarbohydrateCoefficient)
                    : Eating.Protein;

            // Жиры
            var fatCondition = Eating.Fat * GlobalParameters.User.FatCoefficient * GlobalParameters.User.CarbohydrateCoefficient -
                (delta - carbohydrate * GlobalParameters.User.CarbohydrateCoefficient - protein * GlobalParameters.User.ProteinCoefficient * GlobalParameters.User.CarbohydrateCoefficient);
            var fat = proteinCondition > 0
                ? 0
                : fatCondition > 0
                    ? (delta - carbohydrate * GlobalParameters.User.CarbohydrateCoefficient - protein * GlobalParameters.User.ProteinCoefficient * GlobalParameters.User.CarbohydrateCoefficient) / (GlobalParameters.User.FatCoefficient * GlobalParameters.User.CarbohydrateCoefficient)
                    : Eating.Fat;

            return $"Усвоилось по замеру: {Math.Round(carbohydrate, 0)} У; {Math.Round(protein, 0)} Б; {Math.Round(fat, 0)} Ж";
        }

        /// <summary>
        /// Вычисляет кол-во усвоенных БЖУ по скорости абсорбции
        /// </summary>
        /// <returns></returns>
        private (int carbohydrate, int protein, int fat) GetAssimilatedNutritionalWithAbsorptionRate(int carbohydate, int protein, int fat, DateTime beginPeriod, DateTime endPeriod)
        {
            var hours = (endPeriod - beginPeriod).TotalHours;

            // Углеводы
            var carbohydrateTime = (double)(carbohydate / GlobalParameters.User.AbsorptionRateOfCarbohydrates);
            var carbohydrateAssimilated = hours >= carbohydrateTime
                ? carbohydate
                : hours > 0
                    ? (double)GlobalParameters.User.AbsorptionRateOfCarbohydrates * hours
                    : 0;

            hours -= carbohydrateTime;

            // Белки
            var proteinTime = (double)(protein / GlobalParameters.User.AbsorptionRateOfProteins);
            var proteinAssimilated = hours >= proteinTime
                ? protein
                : hours > 0
                    ? (double)GlobalParameters.User.AbsorptionRateOfProteins * hours
                    : 0;

            hours -= proteinTime;

            // Жиры
            var fatTime = (double)(fat / GlobalParameters.User.AbsorptionRateOfFats);
            var fatAssimilated = hours >= fatTime
                ? fat
                : hours > 0
                    ? (double)GlobalParameters.User.AbsorptionRateOfFats * hours
                    : 0;

            return ((int)Math.Round(carbohydrateAssimilated, 0), (int)Math.Round(proteinAssimilated, 0), (int)Math.Round(fatAssimilated, 0));
        }

        /// <summary>
        /// Вычисляет время подколок
        /// </summary>
        /// <returns></returns>
        private string GetAssimilatedNutritionalStartTime(int carbohydate, int protein, int fat, DateTime beginPeriod)
        {
            if (carbohydate == 0 && protein == 0 && fat == 0)
                return "";

            var hours = (double)(carbohydate / GlobalParameters.User.AbsorptionRateOfCarbohydrates);
            var proteinTime = beginPeriod.AddHours(hours < 0.5 ? 0.5 : hours);

            hours = (double)(protein / GlobalParameters.User.AbsorptionRateOfProteins);
            var fatTime = proteinTime.AddHours(hours < 1 ? 0 : hours);

            var result = $"Внести в xDrip как:\n";

            if (carbohydate != 0)
                result += $"  {beginPeriod:dd.MM HH:mm} - {carbohydate} у.\n";

            if (protein != 0)
                result += $"  {proteinTime:dd.MM HH:mm} - {(protein * GlobalParameters.User.ProteinCoefficient + (hours < 1 ? fat * GlobalParameters.User.FatCoefficient : 0)):N0} у.\n";

            if (fat != 0 && hours >= 1)
                result += $"  {fatTime:dd.MM HH:mm} - {fat * GlobalParameters.User.FatCoefficient:N0} у.\n";

            return result.Trim('\n');
        }

        /// <summary>
        /// Расчёт всех значений
        /// </summary>
        private void CalculateTotal([CallerMemberName] string prop = "")
        {
            var startEating = Calculation.DateTimeUnionTimeSpan(Eating.DateCreated, Eating.InjectionTime);

            // Кол-во усвоенных БЖУ по скорости абсорбции в текущем приёме пищи на текущий момент
            var assimilatedCurrent = GetAssimilatedNutritionalWithAbsorptionRate(Carbohydrate, Protein, Fat, startEating.AddMinutes(Pause), DateTime.Now);

            // Кол-во усвоенных БЖУ по скорости абсорбции в предыдущем приёме пищи на момент начала текущего
            var previousEating = (PreviousEatings?.Count ?? 0) > 0
                ? PreviousEatings[0]
                : null;

            (int carbohydrate, int protein, int fat) assimilatedPrevious = previousEating == null
                ? (0, 0, 0)
                : GetAssimilatedNutritionalWithAbsorptionRate(
                    previousEating.Carbohydrate, previousEating.Protein, previousEating.Fat,
                    Calculation.DateTimeUnionTimeSpan(previousEating.DateCreated, previousEating.InjectionTime).AddMinutes(previousEating.Pause), startEating);

            RemainderCarbohydrate = previousEating == null
                ? 0
                : (int)Math.Round(
                    (previousEating.Carbohydrate - assimilatedPrevious.carbohydrate) +
                    (previousEating.Protein - assimilatedPrevious.protein) * GlobalParameters.User.ProteinCoefficient +
                    (previousEating.Fat - assimilatedPrevious.fat) * GlobalParameters.User.FatCoefficient, 0);

            var carbohydrate = assimilatedCurrent.carbohydrate + RemainderCarbohydrate;
            var protein = assimilatedCurrent.protein;
            var fat = assimilatedCurrent.fat;

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

            // Время отработки пищи
            var properties = new string[] { nameof(Carbohydrate), nameof(Protein), nameof(Fat), nameof(Pause), nameof(InjectionTime), nameof(SaveSnackCommand) };

            if (properties.Contains(prop) || IsFirstCalculation)
                SetWorkingTime();

            var active = GlobalMethods.GetActiveInsulin(Eating, Injections,
                startEating, Eating.EndEating, Eating.Id, false,
                Eating.Carbohydrate, Eating.Pause, Eatings);

            BolusDoseTotal = active.insulin;
            ActiveInformation = string.Join("\n", active.informations);            

            // Доза болюсного инсулина
            CalculateBolusDose(Carbohydrate + RemainderCarbohydrate, Protein, Fat);

            // Ожидаемый сахар
            CalculateExpectedGlucose(Carbohydrate + RemainderCarbohydrate, Protein, Fat);            

            // Дробление инъекции
            AdditionallyInjection = GetIsAdditionallyInjection();

            // Фактический ФЧИ
            CalculateInsulinSensitivityFact(carbohydrate, protein, fat);

            // ФЧИ пользователя
            CalculateInsulinSensitivityUser(carbohydrate, protein, fat);

            // Сообщение о кол-ве усвоенных БЖУ
            AssimilatedNutritional = GetAssimilatedNutritional();            
            AssimilatedNutritionalWithAbsorptionRate = $"Усвоилось по скорости: {assimilatedCurrent.carbohydrate} У; {assimilatedCurrent.protein} Б; {assimilatedCurrent.fat} Ж";

            AssimilatedNutritionalStartTime = GetAssimilatedNutritionalStartTime(Carbohydrate, Protein, Fat, startEating.AddMinutes(Pause));

            // Точность
            CalculateAccuracyUser();
            CalculateAccuracyAuto();

            IsFirstCalculation = false;
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

        private List<Models.InsulinType> insulinTypes;
        /// <summary>
        /// Типы инсулина
        /// </summary>
        public List<Models.InsulinType> InsulinTypes
        {
            get => insulinTypes;
            set
            {
                insulinTypes = value;
                OnPropertyChanged(nameof(InsulinTypes));
            }
        }

        private List<Models.InsulinType> bolusInsulinTypes;
        /// <summary>
        /// Типы болюсного инсулина
        /// </summary>
        public List<Models.InsulinType> BolusInsulinTypes
        {
            get => bolusInsulinTypes;
            set
            {
                bolusInsulinTypes = value;
                OnPropertyChanged(nameof(BolusInsulinTypes));
            }
        }

        #endregion

        #region Commands

        #region --Ok

        private async void OkExecute()
        {
            AsyncBase.Open();
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
                    ? new Models.Eating() 
                    { 
                        Id = Guid.NewGuid(), 
                        DateCreated = Eating.DateCreated,
                        FileTimeUtcDateCreated = Eating.DateCreated.ToFileTimeUtc()
                    }
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
                        injection.BolusTypeId = item.BolusType.Id;
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
                            BolusDose = injection.BolusDose,
                            BolusTypeId = injection.BolusType.Id
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
                eating.BasalRate = Eating.BasalRate;
                eating.BasalRateCoefficient = Eating.BasalRateCoefficient;

                eating.BolusDoseCalculate = Eating.BolusDoseCalculate;
                eating.BolusDoseFact = Eating.BolusDoseFact;
                eating.BolusDoseTotal = BolusDoseTotal;

                eating.InsulinSensitivityAutoOne = Eating.InsulinSensitivityAutoOne;
                eating.InsulinSensitivityAutoTwo = Eating.InsulinSensitivityAutoTwo;
                eating.InsulinSensitivityAutoThree = Eating.InsulinSensitivityAutoThree;
                eating.InsulinSensitivityAutoFour = Eating.InsulinSensitivityAutoFour;

                eating.InsulinSensitivityUser = Eating.InsulinSensitivityUser;
                eating.InsulinSensitivityFact = Eating.InsulinSensitivityFact;

                eating.AccuracyAuto = Eating.AccuracyAuto;
                eating.AccuracyUser = Eating.AccuracyUser;

                // eating.IsMenstrualCycleStart = Eating.IsMenstrualCycleStart;
                eating.Comment = Eating.Comment;

                eating.BasalTypeId = Eating.BasalType.Id;
                eating.BolusTypeId = Eating.BolusType.Id;

                eating.EatingTypeId = Eating.EatingType.Id;
                eating.UserId = GlobalParameters.User.Id;

                eating.ExpectedGlucose = Eating.ExpectedGlucose;
                eating.Error = GetError();
                eating.ForecastError = GetForecastError();

                // eating.WriteOff = GlobalParameters.User.BasalType.Duration;
                eating.WorkingTime = Eating.WorkingTime;
                eating.Pause = Eating.Pause;
                eating.ExerciseId = exercise.Id;
                eating.EndEating = Eating.EndEating;

                eating.IsCannulaReplacement = Eating.IsCannulaReplacement;
                eating.IsCatheterReplacement = Eating.IsCatheterReplacement;
                eating.IsCartridgeReplacement = Eating.IsCartridgeReplacement;
                eating.IsBatteryReplacement = Eating.IsBatteryReplacement;
                eating.IsMonitoringReplacement = Eating.IsMonitoringReplacement;

                if (Eating.Id == Guid.Empty)
                    db.Eatings.Add(eating);

                if (IsMenstrualCycleStart)
                    db.MenstrualCycles.Add(new Models.MenstrualCycle()
                    {
                        Id = Guid.NewGuid(),
                        DateStart = DateTime.Now,
                        UserId = GlobalParameters.User.Id
                    });

                if (GlucoseEnd != null)
                {
                    var total = db.ExpendableMaterials
                        .Include(x => x.ExpendableMaterialType)
                        .ToList()
                        .GroupBy(x =>
                            x.ExpendableMaterialTypeId);

                    var strip = (int)(total.FirstOrDefault(x => x.Key == 1)?.Sum(x => x.Count) ?? 0);
                    var monitoring = (int)(total.FirstOrDefault(x => x.Key == 2)?.Sum(x => x.Count) ?? 0);
                    var basal = total.FirstOrDefault(x => x.Key == 3)?.Sum(x => x.Count) ?? 0;
                    var bolus = total.FirstOrDefault(x => x.Key == 4)?.Sum(x => x.Count) ?? 0;
                    var catheter = (int)(total.FirstOrDefault(x => x.Key == 5)?.Sum(x => x.Count) ?? 0);
                    var cannula = (int)(total.FirstOrDefault(x => x.Key == 6)?.Sum(x => x.Count) ?? 0);
                    var cartridge = (int)(total.FirstOrDefault(x => x.Key == 7)?.Sum(x => x.Count) ?? 0);
                    var needle = (int)(total.FirstOrDefault(x => x.Key == 8)?.Sum(x => x.Count) ?? 0);

                    var date = DateTime.Now;

                    // Полоски
                    if (!GlobalParameters.User.IsMonitoring && strip > 0)
                        db.ExpendableMaterials.Add(new Models.ExpendableMaterial()
                        {
                            Id = Guid.NewGuid(),
                            ExpendableMaterialTypeId = 1,
                            Count = -1 - IntermediateDimensions?.Count ?? 0,
                            DateCreated = date
                        });

                    // Сенсор
                    if (Eating.IsMonitoringReplacement && monitoring > 0)
                        db.ExpendableMaterials.Add(new Models.ExpendableMaterial()
                        {
                            Id = Guid.NewGuid(),
                            ExpendableMaterialTypeId = 2,
                            Count = -1,
                            DateCreated = date
                        });

                    // Базальный инсулин
                    if (basal > 0)
                        db.ExpendableMaterials.Add(new Models.ExpendableMaterial()
                        {
                            Id = Guid.NewGuid(),
                            ExpendableMaterialTypeId = 3,
                            Count = -BasalDose,
                            DateCreated = date
                        });

                    // Болюсный инсулин
                    if (bolus > 0)
                        db.ExpendableMaterials.Add(new Models.ExpendableMaterial()
                        {
                            Id = Guid.NewGuid(),
                            ExpendableMaterialTypeId = 4,
                            Count = -BolusDoseFact - Injections?.Sum(x => x.BolusDose) ?? 0,
                            DateCreated = date
                        });

                    // Катетер
                    if (Eating.IsCatheterReplacement && catheter > 0)
                        db.ExpendableMaterials.Add(new Models.ExpendableMaterial()
                        {
                            Id = Guid.NewGuid(),
                            ExpendableMaterialTypeId = 5,
                            Count = -1,
                            DateCreated = date
                        });

                    // Канюля
                    if (Eating.IsCannulaReplacement && cannula > 0)
                        db.ExpendableMaterials.Add(new Models.ExpendableMaterial()
                        {
                            Id = Guid.NewGuid(),
                            ExpendableMaterialTypeId = 6,
                            Count = -1,
                            DateCreated = date
                        });

                    // Картридж
                    if (Eating.IsCartridgeReplacement && cartridge > 0)
                        db.ExpendableMaterials.Add(new Models.ExpendableMaterial()
                        {
                            Id = Guid.NewGuid(),
                            ExpendableMaterialTypeId = 7,
                            Count = -1,
                            DateCreated = date
                        });

                    // Иглы
                    if (!GlobalParameters.User.IsPump && needle > 0)
                        db.ExpendableMaterials.Add(new Models.ExpendableMaterial()
                        {
                            Id = Guid.NewGuid(),
                            ExpendableMaterialTypeId = 8,
                            Count = -1,
                            DateCreated = date
                        });
                }

                db.SaveChanges();

                MessagingCenter.Send(this, "Eating", eating.Id);
                await GlobalParameters.Navigation.PopAsync();
            }
            AsyncBase.Close();
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
            Pause >= 0 &&
            // Типы инсулинов
            BolusType != null &&
            BasalType != null;

        public ICommand OkCommand =>
            new Command(OkExecute);

        #endregion

        #region --Remove

        private async void RemoveExecute()
        {
            AsyncBase.Open();
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
            AsyncBase.Close();
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
                    InjectionDate = DateTime.Now,
                    BolusType = GlobalParameters.User.BolusType
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
                {
                    IntermediateDimensions.Remove((Models.IntermediateDimension)obj);
                    CalculateTotal();
                }
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

                CalculateTotal();
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

        #region ~--Snack

        public ICommand AddSnackCommand =>
            new Command(() =>
            {
                ProteinSnack = FatSnack = CarbohydrateSnack = 0;
                IsModalSnack = true;
            });

        public ICommand SaveSnackCommand =>
            new Command(() =>
            {
                Eating.Protein += ProteinSnack;
                Eating.Fat += FatSnack;
                Eating.Carbohydrate += CarbohydrateSnack;

                OnPropertyChanged(nameof(Protein));
                OnPropertyChanged(nameof(Fat));
                OnPropertyChanged(nameof(Carbohydrate));

                CalculateTotal();
                IsModalSnack = false;
            });

        public ICommand CancelSnackCommand =>
            new Command(() => IsModalSnack = false);

        #endregion

        #region --Close With Nightscout

        private async void CloseWithNightscoutExecute()
        {
            AsyncBase.Open();
            
            try
            {
                if (!CloseWithNightscoutCanExecute())
                    throw new Exception("Отсутствует строка полключения к Nightscout");

                var baseUri = GlobalParameters.User.NightscoutUri.TrimEnd('/');
                var date = DateTimeOffset.Now;
                
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);

                    if (!string.IsNullOrWhiteSpace(GlobalParameters.User.NightscoutApiKey))
                    {
                        var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(GlobalParameters.User.NightscoutApiKey));
                        var hashStr = string.Concat(hash.Select(b => b.ToString("x2")));

                        client.DefaultRequestHeaders.Add("api-secret", hashStr);
                    }

                    // Проверка доступа к серверу
                    var result = await client.GetAsync(baseUri + "/status");
                    if (!result.IsSuccessStatusCode)
                        throw new Exception("Нет доступа к серверу");

                    // Получение текущего сахара
                    result = await client.GetAsync(baseUri + $"/entries.json?find[dateString][$gte]={date.Subtract(DateTimeOffset.Now.Offset):yyyy-MM-dd}&count=1");
                    if (!result.IsSuccessStatusCode)
                        throw new Exception("Не удалось получить данные о текущем сахаре");

                    var data = await result.Content.ReadAsStringAsync();
                    var glucose = JsonConvert.DeserializeObject<List<BusinessLogicLayer.Service.Models.NightscoutEntry>>(data)?.FirstOrDefault();

                    if (glucose == null)
                        throw new Exception("Нет данных о текущем сахаре");

                    if (Math.Abs((date - glucose.dateString).TotalMinutes) > 10)
                        throw new Exception("Данные о текущем сахаре устарели более чем на 10 минут");

                    // Получение подколок
                    result = await client.GetAsync(baseUri + $"/treatments.json?find[insulin][$gte]=0.1&count=100");
                    if (!result.IsSuccessStatusCode)
                        throw new Exception("Не удалось получить данные о подколках");

                    data = await result.Content.ReadAsStringAsync();
                    var insulins = JsonConvert.DeserializeObject<List<BusinessLogicLayer.Service.Models.NightscoutTreatment>>(data);

                    // Удаление подколок позже времени основной инъекции
                    var dateStart = Calculation.DateTimeUnionTimeSpan(Eating.DateCreated, Eating.InjectionTime);

                    var removable = Injections
                        .Where(x => 
                            !x.IsBasalRateCoefficient &&
                            Calculation.DateTimeUnionTimeSpan(x.InjectionDate, x.InjectionTime) >= dateStart)
                        .ToList();

                    foreach (var i in removable)
                        Injections.Remove(i);

                    // Зануление основной инъекции
                    BolusDoseFact = 0;

                    // Добавление подколок
                    insulins = insulins
                        .Where(x => x.created_at >= dateStart)
                        .OrderBy(x => x.created_at)
                        .ToList();

                    foreach (var insulin in insulins)
                        Injections.Add(new Models.Injection()
                        {
                            Id = Guid.NewGuid(),
                            InjectionTime = Calculation.TimeSpanWithoutSeconds(insulin.created_at.Add(DateTimeOffset.Now.Offset).TimeOfDay),
                            InjectionDate = insulin.created_at.Add(DateTimeOffset.Now.Offset).Date,
                            BolusType = GlobalParameters.User.BolusType,
                            BolusDose = insulin.insulin.Value
                        });

                    // Получение ВБС
                    result = await client.GetAsync(baseUri + $"/treatments.json?find[eventType]=Temp%20Basal&count=150");
                    if (!result.IsSuccessStatusCode)
                        throw new Exception("Не удалось получить данные о подколках");

                    data = await result.Content.ReadAsStringAsync();
                    insulins = JsonConvert.DeserializeObject<List<BusinessLogicLayer.Service.Models.NightscoutTreatment>>(data);

                    // ... Удаление
                    removable = Injections
                        .Where(x => x.IsBasalRateCoefficient)
                        .ToList();

                    foreach (var i in removable)
                        Injections.Remove(i);

                    // ... Добавление
                    insulins = insulins
                        .Where(x => 
                            (x.percent ?? 0) > 0 &&
                            (x.created_at >= dateStart ||
                            x.created_at.AddMinutes((double)(x.duration ?? 0)) >= dateStart))
                        .OrderBy(x => x.created_at)
                        .ToList();

                    foreach (var insulin in insulins)
                    {
                        var injectionStartDateTime = insulin.created_at.Add(DateTimeOffset.Now.Offset).DateTime;
                        var injectionEndDateTime = injectionStartDateTime.AddMinutes((double)(insulin.duration ?? 0));

                        if (injectionStartDateTime < dateStart)
                            injectionStartDateTime = dateStart;

                        //if (Eating.EndEating != null && injectionEndDateTime > Eating.EndEating)
                        //    injectionEndDateTime = Eating.EndEating.Value;

                        var delta = (injectionEndDateTime - injectionStartDateTime).TotalSeconds;
                        var injectionDateTime = injectionStartDateTime.AddSeconds(delta / 2);

                        var dose = Math.Round(((insulin.rate ?? 0) * (insulin.percent ?? 0) * ((decimal)delta / 60)) / (((insulin.percent ?? 0) + 100) * 60), 2);
                        if (dose <= 0)
                            continue;

                        Injections.Add(new Models.Injection()
                        {
                            Id = Guid.NewGuid(),
                            InjectionTime = Calculation.TimeSpanWithoutSeconds(injectionDateTime.TimeOfDay),
                            InjectionDate = injectionDateTime.Date,
                            BolusType = GlobalParameters.User.BolusType,
                            BolusDose = Math.Round(((insulin.rate ?? 0) * (insulin.percent ?? 0) * ((decimal)delta / 60)) / (((insulin.percent ?? 0) + 100) * 60), 2),
                            IsBasalRateCoefficient = true
                        });
                    }

                    Injections = Injections
                        .OrderBy(x => x.InjectionDate)
                        .ThenBy(x => x.InjectionTime)
                        .ToObservable();

                    OnPropertyChanged(nameof(Injections));

                    // Добавление сахара на отработке
                    GlucoseEnd = Math.Round(glucose.sgv / 18, 1);

                    // Пересчёт параметров
                    CalculateTotal();
                }
            }
            catch (Exception ex)
            {
                await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Ошибка",
                    ex.Message,
                    "Ok");
            }

            AsyncBase.Close();
        }

        private bool CloseWithNightscoutCanExecute() =>
            !string.IsNullOrWhiteSpace(GlobalParameters.User.NightscoutUri);

        public ICommand CloseWithNightscoutCommand =>
            new Command(CloseWithNightscoutExecute);

        #endregion

        #endregion
    }
}
