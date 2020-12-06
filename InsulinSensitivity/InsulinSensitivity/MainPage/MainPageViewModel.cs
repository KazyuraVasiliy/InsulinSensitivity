using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using System.Linq;

using DataAccessLayer.Contexts;
using BusinessLogicLayer.ViewModel;
using Models = DataAccessLayer.Models;
using System.Windows.Input;

namespace InsulinSensitivity
{
    public class MainPageViewModel : ObservableBase
    {
        #region Constructors and Fields

        /// <summary>
        /// Путь до БД
        /// </summary>
        private readonly string dbPath;

        /// <summary>
        /// Конструктор
        /// </summary>
        public MainPageViewModel()
        {
            dbPath = DependencyService.Get<IPath>().GetDatabasePath(App.DBFILENAME);

            // Подписки на события
            MessagingCenter.Subscribe<User.UserPageViewModel>(this, "CreatedUser",
                (sender) => {  });
        }

        #endregion

        #region Properties

        private INavigation navigation;
        /// <summary>
        /// Навигация
        /// </summary>
        public INavigation Navigation
        {
            get
            {
                if (navigation == null)
                    navigation = App.Current.MainPage.Navigation;
                return navigation;
            }
        }

        #endregion

        #region Collections

        public List<Models.EatingType> EatingTypes
        {
            get
            {
                using (var db = new ApplicationContext(dbPath))
                    return db.EatingTypes.ToList();
            }
        }

        #endregion

        #region Methods


        #endregion

        #region Commands

        #region --User

        private async void AddExecute()
        {
            // Проверка существования пользователя
            using (var db = new ApplicationContext(dbPath))
            {
                if (!db.Users.Any())
                {
                    var userPage = new User.UserPage() {
                        BindingContext = new User.UserPageViewModel() };

                    await Navigation.PushModalAsync(userPage, true);
                }
            }
        }

        public ICommand AddCommand =>
            new Command(AddExecute);

        #endregion

        #endregion
    }
}
