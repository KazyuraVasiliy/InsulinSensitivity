using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using System.Linq;

using DataAccessLayer.Contexts;
using BusinessLogicLayer.ViewModel;
using Models = DataAccessLayer.Models;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace InsulinSensitivity
{
    public class MainPageViewModel : ObservableBase
    {
        #region Constructors and Fields

        /// <summary>
        /// Конструктор
        /// </summary>
        public MainPageViewModel()
        {
            // Инициализация пользователя
            using (var db = new ApplicationContext(GlobalParameters.DbPath))
                GlobalParameters.User = db.Users
                    .Include(x => x.BolusType)
                    .Include(x => x.BasalType)
                    .FirstOrDefault();

            if (GlobalParameters.User != null)
                OnPropertyChanged(nameof(Eatings));

            // Подписки на события
            MessagingCenter.Subscribe<User.UserPageViewModel>(this, "User",
                (sender) =>
                {
                    OnPropertyChanged(nameof(Eatings));
                    OnPropertyChanged(nameof(LastEating));
                    OnPropertyChanged(nameof(TargetGlucose));
                });

            MessagingCenter.Subscribe<Eating.EatingPageViewModel>(this, "Eating",
                (sender) =>
                {
                    OnPropertyChanged(nameof(Eatings));
                    OnPropertyChanged(nameof(LastEating));
                });
        }

        #endregion

        #region Collections

        /// <summary>
        /// Приёмы пищи
        /// </summary>
        public ObservableCollection<Grouping<DateTime, Models.Eating>> Eatings
        {
            get
            {
                if (GlobalParameters.User != null)
                    using (var db = new ApplicationContext(GlobalParameters.DbPath))
                        return new ObservableCollection<Grouping<DateTime, Models.Eating>>(db.Eatings
                            .Where(x =>
                                x.UserId == GlobalParameters.User.Id)
                            .Include(x => x.Exercise)
                                .ThenInclude(x => x.ExerciseType)
                            .Include(x => x.EatingType)
                            .ToList()
                            .GroupBy(x =>
                                x.DateCreated.Date)
                            .OrderByDescending(x =>
                                x.Key)
                            .Select(x =>
                                new Grouping<DateTime, Models.Eating>(x.Key, x.OrderByDescending(y => y.InjectionTime))));
                return new ObservableCollection<Grouping<DateTime, Models.Eating>>();
            }
        }

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

        #endregion

        #region Methods


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

                    var eating = db.Eatings.Find(eatingObj.Id);
                    if (eating != null)
                        db.Eatings.Remove(eating);

                    db.SaveChanges();
                    OnPropertyChanged(nameof(Eatings));
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

        #endregion
    }
}
