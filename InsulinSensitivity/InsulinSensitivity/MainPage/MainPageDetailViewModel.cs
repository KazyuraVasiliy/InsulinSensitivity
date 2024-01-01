using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using System.Linq;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

using DataAccessLayer.Contexts;
using BusinessLogicLayer.ViewModel;
using BusinessLogicLayer.Service;
using Models = DataAccessLayer.Models;

namespace InsulinSensitivity
{
    public class MainPageDetailViewModel : ObservableBase
    {
        #region Constructors and Fields

        /// <summary>
        /// Конструктор
        /// </summary>
        public MainPageDetailViewModel() =>
            Init();

        /// <summary>
        /// Тип приёма пищи для фильтрации
        /// </summary>
        private Guid eatingType =
            Guid.Empty;

        /// <summary>
        /// Типы приёмов пищи
        /// </summary>
        private List<Models.EatingType> eatingTypes =
            new List<Models.EatingType>();

        #endregion

        #region Collections

        /// <summary>
        /// Приёмы пищи
        /// </summary>
        public ObservableCollection<Grouping<DateTime, Models.Eating>> Eatings { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Последний (активный) приём пищи
        /// </summary>
        public Models.Eating LastEating
        {
            get => ((Eatings?.Count ?? 0) > 0 && (Eatings[0]?.Count ?? 0) > 0 && Eatings[0][0].GlucoseEnd != null) || (Eatings?.Count ?? 0) == 0
                ? null
                : Eatings[0][0];
        }

        /// <summary>
        /// Целевая глюкоза
        /// </summary>
        public decimal TargetGlucose =>
            GlobalParameters.User.TargetGlucose;

        /// <summary>
        /// УК
        /// </summary>
        public decimal CarbohydrateCoefficient =>
            GlobalParameters.User.CarbohydrateCoefficient;

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

        private decimal? activeInsulin;
        /// <summary>
        /// Количество активного инсулина в крови
        /// </summary>
        public decimal? ActiveInsulin
        {
            get => LastEating != null
                ? activeInsulin
                : (decimal?)null;
            set
            {
                activeInsulin = value;
                OnPropertyChanged();
            }
        }

        private bool isEnabled = true;
        /// <summary>
        /// Активно ли окно
        /// </summary>
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                isEnabled = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Первичная инициализация
        /// </summary>
        private async void Init() =>
            await AsyncBase.NewTask(() =>
            {
                // Инициализация БД
                Initialize.Init(GlobalParameters.DbPath);
                
                using (var db = new ApplicationContext(GlobalParameters.DbPath))
                {
                    // Инициализация пользователя
                    GlobalParameters.User = db.Users
                        .Include(x => x.BolusType)
                        .Include(x => x.BasalType)
                        .FirstOrDefault();

                    // Инициализация типов приёмов пищи
                    eatingTypes = db.EatingTypes
                        .AsNoTracking()
                        .ToList()
                        .OrderBy(x =>
                            x.TimeStart)
                        .ToList();

                    eatingTypes.Insert(0, new Models.EatingType()
                    {
                        Id = Guid.Empty,
                        Name = "Все"
                    });
                }

                InitEatings();
                RemoveCycle();

                // Подписки на события
                MessagingCenter.Subscribe<User.UserPageViewModel>(this, "User",
                    (sender) =>
                    {
                        InitEatings();
                        RemoveCycle();

                        OnPropertyChanged(nameof(TargetGlucose));
                    });

                MessagingCenter.Subscribe<Eating.EatingPageViewModel, Guid>(this, "Eating",
                    (sender, args) => InitEatings(args));

                MessagingCenter.Subscribe<InsulinType.InsulinTypePageViewModel>(this, "InsulinType",
                    (sender) => ActiveInsulin = GlobalMethods.GetActiveInsulin().insulin);

                MessagingCenter.Subscribe<MainPageMasterViewModel>(this, "RestoreBackup",
                    (sender) => Init());
            }, "Инициализация\nПожалуйста, подождите");

        /// <summary>
        /// Инициализация приёмов пищи
        /// </summary>
        private void InitEatings(Guid? args = null)
        {
            if (GlobalParameters.User != null)
            {
                using (var db = new ApplicationContext(GlobalParameters.DbPath))
                {
                    var period = GlobalParameters.User.PeriodOfCalculation;

                    var query = db.Eatings.AsQueryable();
                    if (period != 0)
                    {
                        var utcPeriod = DateTime.Now.Date.AddDays(-period).ToFileTimeUtc();
                        query = query
                            .Where(x => x.FileTimeUtcDateCreated >= utcPeriod);
                    }

                    query = query
                        .Where(x =>
                            x.UserId == GlobalParameters.User.Id &&
                            (eatingType == Guid.Empty ||
                                (eatingType == x.EatingTypeId ||
                                x.GlucoseEnd == null)));

                    if (args != null)
                        query = query
                            .Where(x =>
                                x.Id == args.Value);

                    var queryResult = query
                        .Include(x => x.Exercise)
                            .ThenInclude(x => x.ExerciseType)
                        .Include(x => x.EatingType)
                        .Include(x => x.Injections)
                            .ThenInclude(x => x.BolusType)
                        .Include(x => x.IntermediateDimensions)
                        .Include(x => x.BasalType)
                        .Include(x => x.BolusType)
                        .AsNoTracking()
                        .ToList()
                        .GroupBy(x => x.DateCreated.Date)
                        .OrderByDescending(x => x.Key)
                        .Select(x =>
                            new Grouping<DateTime, Models.Eating>(x.Key, x.OrderByDescending(y => y.InjectionTime)));

                    if (args == null || Eatings == null)
                    {
                        Eatings = queryResult.ToObservable();
                        OnPropertyChanged(nameof(Eatings));
                    }
                    else
                    {
                        foreach (var el in queryResult)
                        {
                            var find = Eatings.FirstOrDefault(x =>
                                x.Name.Date == el.Name.Date);

                            if (find != null)
                            {
                                foreach (var subEl in el)
                                {
                                    var subFind = find.FirstOrDefault(x =>
                                        x.Id == subEl.Id);

                                    if (subFind != null)
                                        find.Remove(subFind);
                                    find.Insert(0, subEl);
                                }
                            }
                            else Eatings.Insert(0, el);
                        }
                    }
                }
            }
            
            OnPropertyChanged(nameof(LastEating));
            ActiveInsulin = GlobalMethods.GetActiveInsulin().insulin;
        }

        /// <summary>
        /// Удаляет цикл из меню
        /// </summary>
        private void RemoveCycle()
        {
            if (GlobalParameters.IsCycleSettingsAccess)
            {
                var master = ((MasterDetailPage)App.Current.MainPage).Master as MainPageMaster;
                var context = master.BindingContext as MainPageMasterViewModel;

                var item = context.Items.FirstOrDefault(x => x.Name == "Циклы");
                if (item != null)
                    context.Items.Remove(item);
            }            
        }

        #endregion

        #region Commands

        #region --Add

        private async void AddExecute()
        {
            IsEnabled = false;
            try
            {
                using (var db = new ApplicationContext(GlobalParameters.DbPath))
                {
                    // Создание пользователя
                    if (GlobalParameters.User == null)
                    {
                        var userPage = new User.UserPage()
                        {
                            BindingContext = new User.UserPageViewModel()
                        };

                        await GlobalParameters.Navigation.PushAsync(userPage, true);
                    }
                    // Создание приёма пищи
                    else
                    {
                        var eatingPage = new Eating.EatingPage()
                        {
                            BindingContext = new Eating.EatingPageViewModel(LastEating)
                        };

                        await GlobalParameters.Navigation.PushAsync(eatingPage, true);
                    }
                }
            }
            catch (Exception ex)
            {
                await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Ошибка",
                    ex.Message + ex?.InnerException?.Message,
                    "Ok");
            }
            IsEnabled = true;
        }

        public ICommand AddCommand =>
            new Command(AddExecute);

        #endregion

        #region --Edit

        //private async void EditExecute(object obj)
        //{
        //    IsEnabled = false;
        //    try
        //    {
        //        var eatingObj = (Models.Eating)obj;
        //        using (var db = new ApplicationContext(GlobalParameters.DbPath))
        //        {
        //            var eatingPage = new Eating.EatingPage()
        //            {
        //                BindingContext = new Eating.EatingPageViewModel(eatingObj)
        //            };

        //            await GlobalParameters.Navigation.PushAsync(eatingPage, true);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
        //            "Ошибка",
        //            ex.Message + ex?.InnerException?.Message,
        //            "Ok");
        //    }
        //    IsEnabled = true;
        //}

        //public ICommand EditCommand =>
        //    new Command(EditExecute);

        #endregion

        #region --Remove

        private async void RemoveExecute(object obj)
        {
            IsEnabled = false;
            try
            {
                bool question = await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Удалить?",
                    "Вы уверены, что хотите удалить запись?",
                    "Да",
                    "Нет");

                if (question)
                {
                    var eatingObj = (Models.Eating)obj;
                    using (var db = new ApplicationContext(GlobalParameters.DbPath))
                    {
                        var exercise = db.Exercises.Find(eatingObj.Exercise.Id);
                        if (exercise != null)
                            db.Exercises.Remove(exercise);

                        var injections = db.Injections
                            .Where(x =>
                                x.EatingId == eatingObj.Id)
                            .ToList();

                        foreach (var injection in injections)
                            db.Injections.Remove(injection);

                        var dimensions = db.IntermediateDimensions
                            .Where(x =>
                                x.EatingId == eatingObj.Id)
                            .ToList();

                        foreach (var dimension in dimensions)
                            db.IntermediateDimensions.Remove(dimension);

                        var eating = db.Eatings.Find(eatingObj.Id);
                        if (eating != null)
                            db.Eatings.Remove(eating);

                        db.SaveChanges();
                        InitEatings();
                    }
                }
            }
            catch (Exception ex)
            {
                await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Ошибка",
                    ex.Message + ex?.InnerException?.Message,
                    "Ok");
            }
            IsEnabled = true;
        }

        public ICommand RemoveCommand =>
            new Command(RemoveExecute);

        #endregion

        #region --Ignore

        private async void IgnoreExecute(object obj)
        {
            IsEnabled = false;
            try
            {               
                var eatingObj = (Models.Eating)obj;

                bool question = await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Запрос",
                    $"Вы уверены, что хотите {(!eatingObj.IsIgnored ? "не " : "")}учитывать запись?",
                    "Да",
                    "Нет");

                if (question)
                {
                    using (var db = new ApplicationContext(GlobalParameters.DbPath))
                    {
                        var eating = db.Eatings.Find(eatingObj.Id);
                        if (eating != null)
                            eating.IsIgnored = !eating.IsIgnored;

                        db.SaveChanges();
                        InitEatings();
                    }
                }
            }
            catch (Exception ex)
            {
                await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Ошибка",
                    ex.Message + ex?.InnerException?.Message,
                    "Ok");
            }
            IsEnabled = true;
        }

        public ICommand IgnoreCommand =>
            new Command(IgnoreExecute);

        #endregion

        #region --Refresh

        private void RefreshExecute()
        {
            ActiveInsulin = GlobalMethods.GetActiveInsulin().insulin;
            IsRefreshing = false;
        }

        public ICommand RefreshCommand =>
            new Command(RefreshExecute);

        #endregion

        #region --Filter

        private async void FilterExecute()
        {
            var buttons = eatingTypes
                .Select(x =>
                    $"{(x.Id == eatingType ? "* " : "")}{x.Name}")
                .ToArray();

            string action = await GlobalParameters.Navigation.NavigationStack.Last()
                .DisplayActionSheet("Фильтр", "Отмена", null, buttons);

            if (!string.IsNullOrWhiteSpace(action) && action != "Отмена")
            {
                eatingType = eatingTypes
                    .FirstOrDefault(x =>
                        action.Contains(x.Name))?.Id ?? Guid.Empty;
                InitEatings();
            }
        }

        public ICommand FilterCommand =>
            new Command(FilterExecute);

        #endregion

        #endregion
    }
}
