using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

using DataAccessLayer.Contexts;
using BusinessLogicLayer.ViewModel;
using Models = DataAccessLayer.Models;
using System.Threading.Tasks;
using System.Linq;

namespace InsulinSensitivity.User
{
    public class UserPageViewModel : ObservableBase
    {
        #region Constructors

        /// <summary>
        /// Путь до БД
        /// </summary>
        private string dbPath;

        public UserPageViewModel(Guid? userGuid = null) =>
            Init(userGuid);

        #endregion

        #region Properties

        /// <summary>
        /// Текущий пользователь
        /// </summary>
        public Models.User User { get; set; }

        /// <summary>
        /// Навигация
        /// </summary>
        public INavigation Navigation { get; set; } =
            App.Current.MainPage.Navigation;

        #endregion

        #region Collections

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

        /// <summary>
        /// Точность дозирования инсулина
        /// </summary>
        public decimal[] DosingAccuracies { get; set; } =
            new decimal[]
            {
                0.01M,
                0.05M,
                0.1M,
                0.5M,
                1M,
                2M
            };

        #endregion

        #region Methods

        private async void Init(Guid? userGuid)
        {
            // Получение пути до БД
            dbPath = DependencyService.Get<IPath>().GetDatabasePath(App.DBFILENAME);

            // Инициализация пользователя
            if (userGuid != null)
            {
                using (var db = new ApplicationContext(dbPath))
                    User = db.Users.Find(userGuid.Value);
            }
            else User = new Models.User();

            // Инициализация коллекций
            AsyncBase.Open("Инициализация");
            await Task.Run(() =>
            {
                // ... Типы инсулина
                using (var db = new ApplicationContext(dbPath))
                    InsulinTypes = db.InsulinTypes
                        .OrderBy(x =>
                            x.Name)
                        .ToList();
            });
            AsyncBase.Close();
        }

        #endregion

        #region Commands

        #region --Ok

        private async void OkExecute()
        {
            if (!OkCanExecute())
            {
                await Navigation.ModalStack.Last().DisplayAlert(
                    "Ошибка",
                    "Заполните все поля",
                    "Ok");
                return;
            }

            using (var db = new ApplicationContext(dbPath))
            {
                var user = User.Id == Guid.Empty
                    ? new Models.User() { Id = Guid.NewGuid() }
                    : db.Users.Find(User.Id);

                user.Name = User.Name;
                user.BirthDate = User.BirthDate;
                user.Height = User.Height;
                user.Weight = User.Weight;

                user.Hypoglycemia = user.Hypoglycemia;
                user.LowSugar = user.LowSugar;
                user.TargetGlucose = user.TargetGlucose;
                user.HighSugar = user.HighSugar;
                user.Hyperglycemia = user.Hyperglycemia;

                user.BolusTypeId = User.BolusType.Id;
                user.BasalTypeId = User.BasalType.Id;

                user.DosingAccuracy = User.DosingAccuracy;
                user.IsPump = User.IsPump;

                if (User.Id == Guid.Empty)
                    db.Users.Add(user);
                db.SaveChanges();

                MessagingCenter.Send(this, "CreatedUser");
                await Navigation.PopModalAsync();
            }
        }

        private bool OkCanExecute() =>
            // Имя
            !string.IsNullOrWhiteSpace(User.Name) &&
            // Рост и Вес
            User.Weight > 0 &&
            User.Height > 0 &&
            // Тип инсулина
            User.BasalType != null &&
            User.BolusType != null &&
            // Сахара
            User.Hypoglycemia > 0 &&
            User.LowSugar > User.Hypoglycemia &&
            User.TargetGlucose > User.LowSugar &&
            User.HighSugar > User.TargetGlucose &&
            User.Hyperglycemia > User.HighSugar &&
            // Точность дозирования
            User.DosingAccuracy > 0;

        public ICommand OkCommand =>
            new Command(OkExecute);

        #endregion

        #endregion
    }
}
