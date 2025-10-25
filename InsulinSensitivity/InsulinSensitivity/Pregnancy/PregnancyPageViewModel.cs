using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

using DataAccessLayer.Contexts;
using BusinessLogicLayer.ViewModel;
using BusinessLogicLayer.Service;
using Models = DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace InsulinSensitivity.Pregnancy
{
    public class PregnancyPageViewModel : ObservableBase
    {
        #region Constructors

        public PregnancyPageViewModel()
        {
            // Инициализация коллекций
            InitPregnancies();
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

        private Models.Pregnancy selectedPregnancy;
        /// <summary>
        /// Выбранная беременность
        /// </summary>
        public Models.Pregnancy SelectedPregnancy
        {
            get => selectedPregnancy;
            set
            {
                selectedPregnancy = value;
                OnPropertyChanged();
            }
        }

        private bool isModal;
        /// <summary>
        /// Отображается ли модальное окно
        /// </summary>
        public bool IsModal
        {
            get => isModal;
            set
            {
                isModal = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Collections

        /// <summary>
        /// Список беременностей
        /// </summary>
        public ObservableCollection<Models.Pregnancy> Pregnancies { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Инициализирует список беременностей
        /// </summary>
        private void InitPregnancies()
        {
            using (var db = new ApplicationContext(GlobalParameters.DbPath))
                Pregnancies = db.Pregnancies
                    .AsNoTracking()
                    .OrderByDescending(x =>
                        x.DateStart)
                    .ToObservable();
            OnPropertyChanged(nameof(Pregnancies));
        }

        #endregion

        #region Commands

        public ICommand AddCommand =>
            new Command(() =>
            {
                SelectedPregnancy = new Models.Pregnancy() 
                { 
                    DateStart = DateTime.Now,
                    DateEnd = DateTime.Now.AddDays(7 * 40),
                    IsOpen = true,
                };
                IsModal = true;
            });

        public ICommand EditCommand =>
            new Command((object obj) =>
            {
                SelectedPregnancy = (Models.Pregnancy)obj;
                IsModal = true;
            });

        #region --Save

        private async void SaveExecute()
        {
            try
            {
                if (SelectedPregnancy.IsOpen && Pregnancies.Any(x => x.IsOpen && x.Id != SelectedPregnancy.Id))
                    throw new Exception("У вас уже есть открытая беременность");

                using (var db = new ApplicationContext(GlobalParameters.DbPath))
                {
                    var entity = SelectedPregnancy.Id != Guid.Empty
                        ? db.Pregnancies.Find(SelectedPregnancy.Id)
                        : null;

                    if (SelectedPregnancy.Id == Guid.Empty)
                        entity = db.Pregnancies.Add(
                            new Models.Pregnancy()
                            {
                                Id = Guid.NewGuid(),
                                UserId = GlobalParameters.User.Id
                            }).Entity;

                    entity.DateStart = SelectedPregnancy.DateStart;
                    entity.IsOpen = SelectedPregnancy.IsOpen;
                    entity.DateEnd = SelectedPregnancy.IsOpen
                        ? null
                        : SelectedPregnancy.DateEnd;
                    
                    db.SaveChanges();

                    if (SelectedPregnancy.Id != Guid.Empty)
                        Pregnancies.Remove(SelectedPregnancy);
                    Pregnancies.Add(entity);
                }
            }
            catch (Exception ex)
            {
                await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Ошибка",
                    ex.Message + ex?.InnerException?.Message,
                    "Ok");
            }

            IsModal = false;
        }

        public ICommand SaveCommand =>
            new Command(SaveExecute);

        #endregion

        public ICommand CancelCommand =>
            new Command(() => IsModal = false);

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

                var typeObj = (Models.Pregnancy)obj;
                using (var db = new ApplicationContext(GlobalParameters.DbPath))
                {
                    var entity = db.Pregnancies.Find(typeObj.Id);
                    if (entity != null)
                        db.Pregnancies.Remove(entity);

                    db.SaveChanges();
                    Pregnancies.Remove(typeObj);
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

        #region --Refresh

        private void RefreshExecute()
        {
            InitPregnancies();
            IsRefreshing = false;
        }

        public ICommand RefreshCommand =>
            new Command(RefreshExecute);

        #endregion

        #endregion
    }
}
