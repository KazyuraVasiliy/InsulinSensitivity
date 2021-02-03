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

        #region --Edit

        private async void EditExecute(object obj)
        {
            try
            {
                var typeObj = (Models.InsulinType)obj;
                if (typeObj.IsBasal)
                {
                    await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                        "Ошибка",
                        "Редактировать продолжительность действия базального инсулина запрещено",
                        "Ok");
                    return;
                }

                string result = await GlobalParameters.Navigation.NavigationStack.Last().DisplayPromptAsync(
                    "Изменение",
                    "Введите новую продолжительность",
                    accept: "Ok",
                    cancel: "Отмена",
                    initialValue: ((int)typeObj.Duration).ToString(),
                    keyboard: Keyboard.Numeric);

                if (string.IsNullOrWhiteSpace(result))
                    return;

                if (!int.TryParse(result, out int duration) || duration < 3 || duration > 9)
                {
                    await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                        "Ошибка",
                        "Продолжительность действия инсулина должна быть целым числом в диапазоне от 3 до 9",
                        "Ok");
                    return;
                }

                using (var db = new ApplicationContext(GlobalParameters.DbPath))
                {
                    var type = db.InsulinTypes.Find(typeObj.Id);
                    type.Duration = duration;

                    db.SaveChanges();
                    InitInsulinTypes();

                    if (type.Id == GlobalParameters.User.BolusTypeId)
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
        }

        public ICommand EditCommand =>
            new Command(EditExecute);

        #endregion

        #region --Refresh

        private void RefreshExecute()
        {
            InitInsulinTypes();
            IsRefreshing = false;
        }

        public ICommand RefreshCommand =>
            new Command(RefreshExecute);

        #endregion

        #endregion
    }
}
