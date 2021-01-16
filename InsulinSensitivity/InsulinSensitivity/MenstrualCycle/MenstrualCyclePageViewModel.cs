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

namespace InsulinSensitivity.MenstrualCycle
{
    public class MenstrualCyclePageViewModel : ObservableBase
    {
        #region Constructors

        public MenstrualCyclePageViewModel()
        {
            // Инициализация коллекций
            InitCycles();
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

        private Models.MenstrualCycle selectedCycle;
        /// <summary>
        /// Выбранный цикл
        /// </summary>
        public Models.MenstrualCycle SelectedCycle
        {
            get => selectedCycle;
            set
            {
                selectedCycle = value;
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
        /// Список циклов
        /// </summary>
        public ObservableCollection<Models.MenstrualCycle> Cycles { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Инициализирует список циклов
        /// </summary>
        private void InitCycles()
        {
            using (var db = new ApplicationContext(GlobalParameters.DbPath))
                Cycles = db.MenstrualCycles
                    .OrderByDescending(x =>
                        x.DateStart)
                    .ToObservable();
            OnPropertyChanged(nameof(Cycles));
        }

        #endregion

        #region Commands

        public ICommand AddCommand =>
            new Command(() =>
            {
                SelectedCycle = new Models.MenstrualCycle() { DateStart = DateTime.Now };
                IsModal = true;
            });

        public ICommand EditCommand =>
            new Command((object obj) =>
            {
                SelectedCycle = (Models.MenstrualCycle)obj;
                IsModal = true;
            });

        #region --Save

        private async void SaveExecute()
        {
            try
            {
                using (var db = new ApplicationContext(GlobalParameters.DbPath))
                {
                    var entity = SelectedCycle.Id != Guid.Empty
                        ? db.MenstrualCycles.Find(SelectedCycle.Id)
                        : null;

                    if (SelectedCycle.Id == Guid.Empty)
                        entity = db.MenstrualCycles.Add(
                            new Models.MenstrualCycle()
                            {
                                Id = Guid.NewGuid(),
                                UserId = GlobalParameters.User.Id
                            }).Entity;

                    entity.DateStart = SelectedCycle.DateStart;
                    db.SaveChanges();

                    if (SelectedCycle.Id != Guid.Empty)
                        Cycles.Remove(SelectedCycle);
                    Cycles.Add(entity);
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

                var typeObj = (Models.MenstrualCycle)obj;
                using (var db = new ApplicationContext(GlobalParameters.DbPath))
                {
                    var entity = db.MenstrualCycles.Find(typeObj.Id);
                    if (entity != null)
                        db.MenstrualCycles.Remove(entity);

                    db.SaveChanges();
                    Cycles.Remove(typeObj);
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
            InitCycles();
            IsRefreshing = false;
        }

        public ICommand RefreshCommand =>
            new Command(RefreshExecute);

        #endregion

        #endregion
    }
}
