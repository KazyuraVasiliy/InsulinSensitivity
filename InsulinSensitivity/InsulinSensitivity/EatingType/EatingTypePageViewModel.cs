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

namespace InsulinSensitivity.EatingType
{
    public class EatingTypePageViewModel : ObservableBase
    {
        #region Constructors

        public EatingTypePageViewModel()
        {
            // Инициализация коллекций
            InitEatingTypes();
        }

        #endregion

        #region Collections

        /// <summary>
        /// Список типов
        /// </summary>
        public ObservableCollection<Models.EatingType> Types { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Инициализирует список типов приёмов пищи
        /// </summary>
        private void InitEatingTypes()
        {
            using (var db = new ApplicationContext(GlobalParameters.DbPath))
                Types = db.EatingTypes
                    .ToList()
                    .OrderBy(x =>
                        x.TimeStart)
                    .ToObservable();
            OnPropertyChanged(nameof(Types));
        }

        #endregion
    }
}
