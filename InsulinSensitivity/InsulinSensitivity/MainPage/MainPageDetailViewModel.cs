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
        public MainPageDetailViewModel()
        {
            // Инициализация пользователя
            using (var db = new ApplicationContext(GlobalParameters.DbPath))
                GlobalParameters.User = db.Users
                    .Include(x => x.BolusType)
                    .Include(x => x.BasalType)
                    .FirstOrDefault();

            InitEatings();

            // Подписки на события
            MessagingCenter.Subscribe<User.UserPageViewModel>(this, "User",
                (sender) =>
                {
                    InitEatings();
                    OnPropertyChanged(nameof(LastEating));
                    OnPropertyChanged(nameof(TargetGlucose));
                });

            MessagingCenter.Subscribe<Eating.EatingPageViewModel>(this, "Eating",
                (sender) =>
                {
                    InitEatings();
                    OnPropertyChanged(nameof(LastEating));
                    OnPropertyChanged(nameof(ActiveInsulin));
                });

            MessagingCenter.Subscribe<InsulinType.InsulinTypePageViewModel>(this, "InsulinType",
                (sender) => OnPropertyChanged(nameof(ActiveInsulin)));
        }

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

        /// <summary>
        /// Количество активного инсулина в крови
        /// </summary>
        public decimal? ActiveInsulin =>
            LastEating != null
            ? GlobalMethods.GetActiveInsulin(LastEating)
            : (decimal?)null;

        #endregion

        #region Methods

        /// <summary>
        /// Инициализация приёмов пищи
        /// </summary>
        private void InitEatings()
        {
            if (GlobalParameters.User != null)
                using (var db = new ApplicationContext(GlobalParameters.DbPath))
                    Eatings = new ObservableCollection<Grouping<DateTime, Models.Eating>>(db.Eatings
                        .Where(x =>
                            x.UserId == GlobalParameters.User.Id)
                        .Include(x => x.Exercise)
                            .ThenInclude(x => x.ExerciseType)
                        .Include(x => x.EatingType)
                        .Include(x => x.Injections)
                        .Include(x => x.IntermediateDimensions)
                        .ToList()
                        .GroupBy(x =>
                            x.DateCreated.Date)
                        .OrderByDescending(x =>
                            x.Key)
                        .Select(x =>
                            new Grouping<DateTime, Models.Eating>(x.Key, x.OrderByDescending(y => y.InjectionTime))));
            OnPropertyChanged(nameof(Eatings));
        }

        #endregion

        #region Commands

        #region --Add

        private async void AddExecute()
        {
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
        }

        public ICommand AddCommand =>
            new Command(AddExecute);

        #endregion

        #region --Remove

        private async void RemoveExecute(object obj)
        {
            try
            {
                bool question = await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Удалить?",
                    "Вы уверены, что хотите удалить запись?",
                    "Да",
                    "Нет");

                if (!question)
                    return;

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
            catch (Exception ex)
            {
                await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Ошибка",
                    ex.Message + ex?.InnerException?.Message,
                    "Ok");
            }
        }

        public ICommand RemoveCommand =>
            new Command(RemoveExecute);

        #endregion

        #region --Option

        private async void OptionExecute()
        {
            using (var db = new ApplicationContext(GlobalParameters.DbPath))
            {
                if (GlobalParameters.User != null)
                {
                    var userPage = new User.UserPage()
                    {
                        BindingContext = new User.UserPageViewModel(GlobalParameters.User.Id)
                    };

                    await GlobalParameters.Navigation.PushAsync(userPage, true);
                }
            }
        }

        public ICommand OptionCommand =>
            new Command(OptionExecute);

        #endregion

        #region --Refresh

        private void RefreshExecute()
        {
            OnPropertyChanged(nameof(ActiveInsulin));
            IsRefreshing = false;
        }

        public ICommand RefreshCommand =>
            new Command(RefreshExecute);

        #endregion

        #endregion
    }
}
