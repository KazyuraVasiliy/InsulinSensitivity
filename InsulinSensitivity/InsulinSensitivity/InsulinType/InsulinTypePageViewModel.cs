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

namespace InsulinSensitivity.InsulinType
{
    public class InsulinTypePageViewModel : ObservableBase
    {
        #region Constructors

        public InsulinTypePageViewModel()
        {
            // Инициализация коллекций
            InitInsulinTypes();
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

        /// <summary>
        /// Id типа болюсного инсулина, используемого пользователем
        /// </summary>
        public Guid UserBolusTypeId =>
            GlobalParameters.User?.BolusTypeId ?? Guid.Empty;

        /// <summary>
        /// Id типа базального инсулина, используемого пользователем
        /// </summary>
        public Guid UserBasalTypeId =>
            GlobalParameters.User?.BasalTypeId ?? Guid.Empty;

        private Models.InsulinType selectedInsulin;
        /// <summary>
        /// Выбранный инсулин
        /// </summary>
        public Models.InsulinType SelectedInsulin
        {
            get => selectedInsulin;
            set
            {
                selectedInsulin = value;
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
        /// Список типов
        /// </summary>
        public ObservableCollection<Grouping<string, Models.InsulinType>> Types { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Инициализирует список инсулинов
        /// </summary>
        private void InitInsulinTypes()
        {
            using (var db = new ApplicationContext(GlobalParameters.DbPath))
                Types = db.InsulinTypes
                    .AsNoTracking()
                    .ToList()
                    .GroupBy(x =>
                        x.IsBasal)
                    .Select(x =>
                        new Grouping<string, Models.InsulinType>(x.Key ? "Базальный" : "Болюсный", x.OrderBy(y => y.Name)))
                    .OrderByDescending(x =>
                        x.Name)
                    .ToObservable();
            OnPropertyChanged(nameof(Types));
        }

        #endregion

        #region Commands

        #region --Refresh

        private void RefreshExecute()
        {
            InitInsulinTypes();
            IsRefreshing = false;
        }

        public ICommand RefreshCommand =>
            new Command(RefreshExecute);

        #endregion

        public ICommand EditCommand =>
            new Command((object obj) =>
            {
                SelectedInsulin = (Models.InsulinType)obj;
                IsModal = true;
            });

        #region --Save

        private async void SaveExecute()
        {
            try
            {
                bool isError =
                    !int.TryParse(SelectedInsulin.Duration.ToString(), System.Globalization.NumberStyles.AllowDecimalPoint, null, out int duration) ||
                    (!SelectedInsulin.IsBasal && (duration < 3 || duration > 11)) ||
                    (SelectedInsulin.IsBasal && (duration < 12 || duration > 48)) ||
                    SelectedInsulin.Offset <= 0;

                if (isError)
                {
                    await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                        "Ошибка",
                        "Продолжительность действия болюсного инсулина должна быть целым числом в диапазоне от 3 до 11, базального - целым числом в диапазоне от 12 до 48.\n\nНачало действия должно быть положительным числом",
                        "Ok");
                    return;
                }

                using (var db = new ApplicationContext(GlobalParameters.DbPath))
                {
                    var entity = SelectedInsulin.Id != Guid.Empty
                        ? db.InsulinTypes.Find(SelectedInsulin.Id)
                        : null;

                    if (SelectedInsulin.Id == Guid.Empty)
                    {
                        IsModal = false;
                        return;
                    }

                    //if (SelectedInsulin.Id == Guid.Empty)
                    //    entity = db.InsulinTypes.Add(
                    //        new Models.InsulinType()
                    //        {
                    //            Id = Guid.NewGuid(),
                    //            UserId = GlobalParameters.User.Id
                    //        }).Entity;

                    entity.Duration = SelectedInsulin.Duration;
                    entity.Offset = SelectedInsulin.Offset;

                    db.SaveChanges();
                    InitInsulinTypes();

                    if (SelectedInsulin.Id == GlobalParameters.User.BolusTypeId || SelectedInsulin.Id == GlobalParameters.User.BasalTypeId)
                    {
                        GlobalParameters.User = db.Users
                            .Include(x => x.BasalType)
                            .Include(x => x.BolusType)
                            .FirstOrDefault();

                        MessagingCenter.Send(this, "InsulinType");
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

            IsModal = false;
        }

        public ICommand SaveCommand =>
            new Command(SaveExecute);

        #endregion

        public ICommand CancelCommand =>
            new Command(() => IsModal = false);

        #endregion
    }
}
