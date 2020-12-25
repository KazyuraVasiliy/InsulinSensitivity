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

namespace InsulinSensitivity.Eating
{
    public class EatingPageViewModel : ObservableBase
    {
        #region Constructors

        public EatingPageViewModel(Models.Eating eating = null)
        {
            // Инициализация приёма пищи
            Eating = eating == null
                ? new Models.Eating()
                : eating;            

            // Инициализация коллекций
            using (var db = new ApplicationContext(GlobalParameters.DbPath))
            {
                // ... Типы приёмов пищи
                EatingTypes = db.EatingTypes
                    .ToList();

                // ... Типы упражнений
                ExerciseTypes = db.ExerciseTypes
                    .OrderBy(x =>
                        x.Name)
                    .ToList();
            }

            // Инициализация времени инъекции
            if (eating == null)
                Eating.InjectionTime = DateTime.Now.TimeOfDay;

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
            !Basals.Any(x =>
                x.DateCreated.Date == DateTime.Now.Date);

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

        #endregion

        #region --Previous

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

                CalculateInsulinSensitivityOne();
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
                Eating.InjectionTime = value;
                OnPropertyChanged();

                EatingType = EatingTypes
                    .FirstOrDefault(x =>
                        x.TimeStart <= value &&
                        x.TimeEnd >= value);
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
                Eating.GlucoseStart = value;
                OnPropertyChanged();

                CalculateBolusDose();
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
                Eating.GlucoseEnd = value;
                OnPropertyChanged();

                CalculateInsulinSensitivityFact();
            }
        }

        /// <summary>
        /// Белки
        /// </summary>
        public int Protein
        {
            get => Eating.Protein;
            set
            {
                Eating.Protein = value;
                OnPropertyChanged();

                CalculateBolusDose();
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
                Eating.Fat = value;
                OnPropertyChanged();

                CalculateBolusDose();
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
                Eating.Carbohydrate = value;
                OnPropertyChanged();

                CalculateBolusDose();
            }
        }

        /// <summary>
        /// Тип нагрузки
        /// </summary>
        public Models.ExerciseType ExerciseType
        {
            get => Eating.Exercise.ExerciseType;
            set
            {
                Eating.Exercise.ExerciseType = value;
                OnPropertyChanged();

                CalculateInsulinSensitivityTwo();
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
                Eating.Exercise.Duration = value;
                OnPropertyChanged();

                CalculateInsulinSensitivityTwo();
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
                Eating.Exercise.HoursAfterInjection = value;
                OnPropertyChanged();

                CalculateInsulinSensitivityTwo();
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
                Eating.BasalDose = value;
                OnPropertyChanged();

                CalculateInsulinSensitivityTwo();
            }
        }

        /// <summary>
        /// ФЧИ рассчитанный по первой формуле
        /// </summary>
        private decimal? InsulinSensitivityAutoOne
        {
            get => Eating.InsulinSensitivityAutoOne;
            set
            {
                Eating.InsulinSensitivityAutoOne = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(InsulinSensitivityAuto));
            }
        }

        /// <summary>
        /// ФЧИ рассчитанный по второй формуле
        /// </summary>
        private decimal? InsulinSensitivityAutoTwo
        {
            get => Eating.InsulinSensitivityAutoTwo;
            set
            {
                Eating.InsulinSensitivityAutoTwo = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(InsulinSensitivityAuto));
            }
        }

        /// <summary>
        /// ФЧИ рассчитанный (средний)
        /// </summary>
        public decimal? InsulinSensitivityAuto
        {
            get
            {
                var result = InsulinSensitivityAutoOne != null && InsulinSensitivityAutoTwo != null
                    ? (InsulinSensitivityAutoOne + InsulinSensitivityAutoTwo) / 2
                    : InsulinSensitivityAutoOne != null && InsulinSensitivityAutoTwo == null
                        ? InsulinSensitivityAutoOne
                        : InsulinSensitivityAutoOne == null && InsulinSensitivityAutoTwo != null
                            ? InsulinSensitivityAutoTwo
                            : null;

                return result == null
                    ? (decimal?)null
                    : Math.Round(result.Value, 3, MidpointRounding.AwayFromZero);
            }
        }
            

        /// <summary>
        /// ФЧИ фактический
        /// </summary>
        public decimal? InsulinSensitivityFact
        {
            get => Eating.InsulinSensitivityFact == null
                ? (decimal?)null
                : Math.Round(Eating.InsulinSensitivityFact.Value, 3, MidpointRounding.AwayFromZero);
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
                Eating.InsulinSensitivityUser = value;
                OnPropertyChanged();

                CalculateBolusDose();
                CalculateAccuracyUser();
            }
        }

        /// <summary>
        /// Доза болюсного инсулина вычисленная
        /// </summary>
        public decimal? BolusDoseCalculate
        {
            get => Eating.BolusDoseCalculate == null
                ? (decimal?)null
                : Math.Round(Eating.BolusDoseCalculate.Value, 2, MidpointRounding.AwayFromZero);
            set
            {
                Eating.BolusDoseCalculate = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Доза болюсного инсулина фактическая
        /// </summary>
        public decimal BolusDoseFact
        {
            get => Eating.BolusDoseFact;
            set
            {
                Eating.BolusDoseFact = value;
                OnPropertyChanged();

                CalculateInsulinSensitivityFact();
            }
        }

        /// <summary>
        /// Количество активного инсулина в крови перед поставновкой инъекции
        /// </summary>
        public decimal ActiveInsulinStart
        {
            get => Eating.ActiveInsulinStart;
            set
            {
                Eating.ActiveInsulinStart = value;
                OnPropertyChanged();

                CalculateBolusDose();
            }
        }

        /// <summary>
        /// Количество активного инсулина в крови на отработке
        /// </summary>
        public decimal ActiveInsulinEnd
        {
            get => Eating.ActiveInsulinEnd;
            set
            {
                Eating.ActiveInsulinEnd = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Точность автоматического ФЧИ
        /// </summary>
        public int? AccuracyAuto
        {
            get => Eating.AccuracyAuto;
            set
            {
                Eating.AccuracyAuto = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Точность ФЧИ пользователя
        /// </summary>
        public int? AccuracyUser
        {
            get => Eating.AccuracyUser;
            set
            {
                Eating.AccuracyUser = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Начало менструального цикла
        /// </summary>
        public bool IsMenstrualCycleStart
        {
            get => Eating.IsMenstrualCycleStart;
            set
            {
                Eating.IsMenstrualCycleStart = value;
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
                    .Take(3)
                    .ToList();

                // Предыдущий приём пищи
                var previousEating = (PreviousEatings?.Count ?? 0) > 0
                    ? PreviousEatings[0]
                    : null;

                // Средний ФЧИ предыдущего типа приёма пищи
                if (previousEating != null)
                {
                    var previousAverageEatingTypeSensitivityCollection = db.Eatings
                        .Where(x =>
                            x.Id != Eating.Id &&
                            x.EatingTypeId == previousEating.EatingTypeId)
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
                        var exercises = db.Eatings
                            .Where(x =>
                                x.Id != Eating.Id &&
                                x.Exercise.ExerciseTypeId == previous.Exercise.ExerciseTypeId &&
                                x.Exercise.HoursAfterInjection == previous.Exercise.HoursAfterInjection)
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
                    .Take(2)
                    .ToList();

                // Дата начала последней менструации
                LastMenstruationDate = db.Eatings
                    .Where(x =>
                        x.Id != Eating.Id &&
                        x.IsMenstrualCycleStart)
                    .OrderByDescending(x =>
                        x.DateCreated)
                    .Take(1)
                    .FirstOrDefault()?.DateCreated;
            }
        }

        /// <summary>
        /// Рассчитывает автоматический ФЧИ по первой формуле и дозу болюсного инсулина
        /// </summary>
        private void CalculateInsulinSensitivityOne()
        {
            // Рассчёт ФЧИ по первой формуле
            InsulinSensitivityAutoOne = null;
            if ((PreviousEatings?.Count ?? 0) > 0 && PreviousEatings[0].InsulinSensitivityFact != null && PreviousAverageEatingTypeSensitivity != null && EatingType != null)
            {
                using (var db = new ApplicationContext(GlobalParameters.DbPath))
                {
                    // Средний ФЧИ текущего типа приёма пищи
                    var averageEatingTypeSensitivityCollection = db.Eatings
                        .Where(x =>
                            x.Id != Eating.Id &&
                            x.EatingTypeId == EatingType.Id)
                        .ToList();

                    var averageEatingTypeSensitivity = (averageEatingTypeSensitivityCollection?.Count ?? 0) > 0
                        ? averageEatingTypeSensitivityCollection
                            .Average(x =>
                                x.InsulinSensitivityFact)
                        : null;

                    if (averageEatingTypeSensitivity != null)
                        InsulinSensitivityAutoOne = PreviousEatings[0].InsulinSensitivityFact * (averageEatingTypeSensitivity / PreviousAverageEatingTypeSensitivity);
                }
            }

            // Рассчёт дозы болюсного инсулина
            CalculateBolusDose();
        }

        /// <summary>
        /// Рассчитывает автоматический ФЧИ по второй формуле и дозу болюсного инсулина
        /// </summary>
        private void CalculateInsulinSensitivityTwo()
        {
            // Рассчёт ФЧИ по второй формуле
            InsulinSensitivityAutoTwo = null;
            if ((PreviousEatings?.Count ?? 0) == 3 && PreviousEatings.All(x => x.InsulinSensitivityFact != null) && (PreviousAverageExerciseTypeSensitivitys?.Count ?? 0) == 3 && ExerciseType != null)
            {
                using (var db = new ApplicationContext(GlobalParameters.DbPath))
                {
                    // Средний ФЧИ текущего типа нагрузки
                    var averageExerciseTypeSensitivityCollection = db.Eatings
                        .Where(x =>
                            x.Id != Eating.Id &&
                            x.Exercise.ExerciseTypeId == ExerciseType.Id)
                        .ToList();

                    var averageExerciseTypeSensitivity = (averageExerciseTypeSensitivityCollection?.Count ?? 0) > 0
                        ? averageExerciseTypeSensitivityCollection
                            .Average(x =>
                                x.InsulinSensitivityFact)
                        : null;

                    // Средние по предыдущим приёмам пищи
                    decimal average = 0;
                    for (int i = 0; i < 3; i++)
                        average += PreviousEatings[i].InsulinSensitivityFact.Value / PreviousAverageExerciseTypeSensitivitys[i].Value;
                    average /= 3;

                    // Частное базы
                    decimal basal = 1;
                    if (BasalDose != 0 && Basals.First() != null)
                        basal = BasalDose / Basals.First().BasalDose;
                    else if ((Basals?.Count ?? 0) == 2)
                        basal = Basals[0].BasalDose / Basals[1].BasalDose;

                    if (averageExerciseTypeSensitivity != null)
                        InsulinSensitivityAutoTwo = average * averageExerciseTypeSensitivity * ((basal + 1) / 2);
                }
            }

            // Рассчёт дозы болюсного инсулина
            CalculateBolusDose();
        }

        /// <summary>
        /// Рассчёт дозы болюсного инсулина
        /// </summary>
        private void CalculateBolusDose()
        {
            BolusDoseCalculate = InsulinSensitivityAuto != null || InsulinSensitivityUser != null
                ? Calculation.GetBolusDose(GlucoseStart, GlobalParameters.User.TargetGlucose,
                    GlobalParameters.User.CarbohydrateCoefficient, GlobalParameters.User.ProteinCoefficient, GlobalParameters.User.FatCoefficient,
                    Protein, Fat, Carbohydrate,
                    InsulinSensitivityUser != null
                    ? InsulinSensitivityUser.Value
                    : InsulinSensitivityAuto.Value) - ActiveInsulinStart
                : (decimal?)null;
        }

        /// <summary>
        /// Рассчёт фактического ФЧИ
        /// </summary>
        private void CalculateInsulinSensitivityFact()
        {
            InsulinSensitivityFact = BolusDoseFact > 0 && GlucoseEnd != null
                ? Calculation.GetInsulinSensitivityFact(GlucoseStart, GlucoseEnd.Value,
                    GlobalParameters.User.CarbohydrateCoefficient, GlobalParameters.User.ProteinCoefficient, GlobalParameters.User.FatCoefficient,
                    Protein, Fat, Carbohydrate,
                    BolusDoseFact)
                : (decimal?)null;

            CalculateAccuracyAuto();
            CalculateAccuracyUser();
        }

        /// <summary>
        /// Рассчёт точности рассчётного ФЧИ
        /// </summary>
        private void CalculateAccuracyAuto()
        {
            AccuracyAuto = InsulinSensitivityFact != null && InsulinSensitivityAuto != null
                ? (int)Math.Round((InsulinSensitivityFact.Value - Math.Abs(InsulinSensitivityFact.Value - InsulinSensitivityAuto.Value)) / InsulinSensitivityFact.Value * 100, 0, MidpointRounding.AwayFromZero)
                : (int?)null;
        }

        /// <summary>
        /// Рассчёт точности ФЧИ пользователя
        /// </summary>
        private void CalculateAccuracyUser()
        {
            AccuracyUser = InsulinSensitivityFact != null && InsulinSensitivityUser != null
                ? (int)Math.Round((InsulinSensitivityFact.Value - Math.Abs(InsulinSensitivityFact.Value - InsulinSensitivityUser.Value)) / InsulinSensitivityFact.Value * 100, 0, MidpointRounding.AwayFromZero)
                : (int?)null;
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
        /// Типы нагрзуки
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
                var exercise = Eating.Exercise.Id == Guid.Empty
                    ? new Models.Exercise() { Id = Guid.NewGuid() }
                    : db.Exercises.Find(Eating.Exercise.Id);

                exercise.ExerciseTypeId = Eating.Exercise.ExerciseType.Id;
                exercise.Duration = Eating.Exercise.Duration;
                exercise.HoursAfterInjection = Eating.Exercise.HoursAfterInjection;

                if (Eating.Exercise.Id == Guid.Empty)
                    db.Exercises.Add(exercise);

                var eating = Eating.Id == Guid.Empty
                    ? new Models.Eating() { Id = Guid.NewGuid(), DateCreated = DateTime.Now }
                    : db.Eatings.Find(Eating.Id);

                eating.InjectionTime = Eating.InjectionTime;
                eating.GlucoseStart = Eating.GlucoseStart;
                eating.GlucoseEnd = Eating.GlucoseEnd;

                eating.ActiveInsulinStart = Eating.ActiveInsulinStart;
                eating.ActiveInsulinEnd = Eating.ActiveInsulinEnd;

                eating.Protein = Eating.Protein;
                eating.Fat = Eating.Fat;
                eating.Carbohydrate = Eating.Carbohydrate;

                eating.BasalDose = Eating.BasalDose;
                eating.BolusDoseCalculate = Eating.BolusDoseCalculate;
                eating.BolusDoseFact = Eating.BolusDoseFact;

                eating.InsulinSensitivityAutoOne = Eating.InsulinSensitivityAutoOne;
                eating.InsulinSensitivityAutoTwo = Eating.InsulinSensitivityAutoTwo;
                eating.InsulinSensitivityUser = Eating.InsulinSensitivityUser;
                eating.InsulinSensitivityFact = Eating.InsulinSensitivityFact;

                eating.AccuracyAuto = Eating.AccuracyAuto;
                eating.AccuracyUser = Eating.AccuracyUser;

                eating.IsMenstrualCycleStart = Eating.IsMenstrualCycleStart;

                eating.EatingTypeId = Eating.EatingType.Id;
                eating.UserId = GlobalParameters.User.Id;

                eating.WriteOff = GlobalParameters.User.BolusType.Duration;
                eating.ExerciseId = exercise.Id;

                if (Eating.Id == Guid.Empty)
                    db.Eatings.Add(eating);

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
            ActiveInsulinStart >= 0 &&
            ActiveInsulinEnd >= 0 &&
            // Рассчёты
            (InsulinSensitivityFact == null || InsulinSensitivityFact >= 0) &&
            (InsulinSensitivityUser == null || InsulinSensitivityUser >= 0) &&
            BolusDoseFact > 0;

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

        #endregion
    }
}
