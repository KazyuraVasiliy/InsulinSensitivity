using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using BusinessLogicLayer.Service.Interfaces;
using Xamarin.Essentials;
using System.IO;

using DataAccessLayer.Contexts;
using BusinessLogicLayer.ViewModel;
using BusinessLogicLayer.Service;
using Models = DataAccessLayer.Models;

namespace InsulinSensitivity.User
{
    public class UserPageViewModel : ObservableBase
    {
        #region Constructors

        public UserPageViewModel(Guid? userGuid = null)
        {
            if (userGuid != null)
            {
                using (var db = new ApplicationContext(GlobalParameters.DbPath))
                    User = db.Users
                        .Include(x => x.BolusType)
                        .Include(x => x.BasalType)
                        .FirstOrDefault(x => 
                            x.Id == userGuid.Value);
            }
            else User = new Models.User() 
            { 
                BirthDate = DateTime.Now.AddYears(-20),
                DosingAccuracy = 1M,

                Hypoglycemia = 3.8M,
                LowSugar = 4M,
                TargetGlucose = 5M,
                HighSugar = 7.8M,
                Hyperglycemia = 9M
            };

            // Инициализация коллекций
            using (var db = new ApplicationContext(GlobalParameters.DbPath))
                InsulinTypes = db.InsulinTypes
                    .OrderBy(x =>
                        x.Name)
                    .ToList();

            BolusInsulinTypes = InsulinTypes
                .Where(x =>
                    !x.IsBasal)
                .ToList();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Текущий пользователь
        /// </summary>
        public Models.User User { get; set; }

        /// <summary>
        /// Видны ли поля доступные при редактировании
        /// </summary>
        public bool IsEditableFieldsVisible =>
            User.Id != Guid.Empty;

        /// <summary>
        /// Рост
        /// </summary>
        public int Height
        {
            get => User.Height;
            set
            {
                User.Height = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Вес
        /// </summary>
        public int Weight
        {
            get => User.Weight;
            set
            {
                User.Weight = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// УК
        /// </summary>
        public decimal CarbohydrateCoefficient
        {
            get => User.CarbohydrateCoefficient;
            set
            {
                User.CarbohydrateCoefficient = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// БК
        /// </summary>
        public decimal ProteinCoefficient
        {
            get => User.ProteinCoefficient;
            set
            {
                User.ProteinCoefficient = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// ЖК
        /// </summary>
        public decimal FatCoefficient
        {
            get => User.FatCoefficient;
            set
            {
                User.FatCoefficient = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// АУ
        /// </summary>
        public decimal AbsorptionRateOfCarbohydrates
        {
            get => User.AbsorptionRateOfCarbohydrates;
            set
            {
                User.AbsorptionRateOfCarbohydrates = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// АБ
        /// </summary>
        public decimal AbsorptionRateOfProteins
        {
            get => User.AbsorptionRateOfProteins;
            set
            {
                User.AbsorptionRateOfProteins = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// АЖ
        /// </summary>
        public decimal AbsorptionRateOfFats
        {
            get => User.AbsorptionRateOfFats;
            set
            {
                User.AbsorptionRateOfFats = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Гипоглекимия
        /// </summary>
        public decimal Hypoglycemia
        {
            get => User.Hypoglycemia;
            set
            {
                User.Hypoglycemia = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Низкий сахар
        /// </summary>
        public decimal LowSugar
        {
            get => User.LowSugar;
            set
            {
                User.LowSugar = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Целевая глюкоза
        /// </summary>
        public decimal TargetGlucose
        {
            get => User.TargetGlucose;
            set
            {
                User.TargetGlucose = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Высокий сахар
        /// </summary>
        public decimal HighSugar
        {
            get => User.HighSugar;
            set
            {
                User.HighSugar = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Гипергликимия
        /// </summary>
        public decimal Hyperglycemia
        {
            get => User.Hyperglycemia;
            set
            {
                User.Hyperglycemia = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Кол-во дней для расчёта
        /// </summary>
        public int PeriodOfCalculation
        {
            get => User.PeriodOfCalculation;
            set
            {
                if (value >= 0)
                    User.PeriodOfCalculation = value;
                OnPropertyChanged();
            }
        }

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

        private List<Models.InsulinType> bolusInsulinTypes;
        /// <summary>
        /// Типы болюсного инсулина
        /// </summary>
        public List<Models.InsulinType> BolusInsulinTypes
        {
            get => bolusInsulinTypes;
            set
            {
                bolusInsulinTypes = value;
                OnPropertyChanged(nameof(BolusInsulinTypes));
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

        #region Commands

        #region --Ok

        private async void OkExecute()
        {
            if (!OkCanExecute())
            {
                await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Ошибка",
                    "Все поля должны быть заполнены.\n" +
                        "Уровни гликемии должны отличаться и идти в порядке возрастания.",
                    "Ok");
                return;
            }

            using (var db = new ApplicationContext(GlobalParameters.DbPath))
            {
                var user = User.Id == Guid.Empty
                    ? new Models.User() { Id = Guid.NewGuid() }
                    : db.Users.Find(User.Id);

                user.Name = User.Name;
                user.BirthDate = User.BirthDate;
                user.Gender = User.Gender;

                user.Height = User.Height;
                user.Weight = User.Weight;

                user.Hypoglycemia = User.Hypoglycemia;
                user.LowSugar = User.LowSugar;
                user.TargetGlucose = User.TargetGlucose;
                user.HighSugar = User.HighSugar;
                user.Hyperglycemia = User.Hyperglycemia;

                user.BolusTypeId = User.BolusType.Id;
                user.BasalTypeId = User.BasalType.Id;

                user.DosingAccuracy = User.DosingAccuracy;
                user.IsPump = User.IsPump;

                user.AbsorptionRateOfCarbohydrates = User.AbsorptionRateOfCarbohydrates;
                user.AbsorptionRateOfProteins = User.AbsorptionRateOfProteins;
                user.AbsorptionRateOfFats = User.AbsorptionRateOfFats;

                if (User.Id == Guid.Empty)
                {
                    user.CarbohydrateCoefficient = Math.Round(Calculation.GetCarbohydrateCoefficient(user.BirthDate, user.Gender, user.Height, user.Weight), 2, MidpointRounding.AwayFromZero);
                    user.ProteinCoefficient = 0.3M;
                    user.FatCoefficient = 0.25M;

                    user.PeriodOfCalculation = 14;
                }
                else
                {
                    user.CarbohydrateCoefficient = User.CarbohydrateCoefficient;
                    user.ProteinCoefficient = User.ProteinCoefficient;
                    user.FatCoefficient = User.FatCoefficient;

                    user.PeriodOfCalculation = User.PeriodOfCalculation;
                }

                if (User.Id == Guid.Empty)
                    db.Users.Add(user);
                
                db.SaveChanges();
                GlobalParameters.User = db.Users
                    .Include(x => x.BasalType)
                    .Include(x => x.BolusType)
                    .FirstOrDefault();

                MessagingCenter.Send(this, "User");
                await GlobalParameters.Navigation.PopAsync();
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
            User.DosingAccuracy > 0 &&
            // Кол-во дней для расчёта
            User.PeriodOfCalculation >= 0 &&
            // Скорость абсорбции
            User.AbsorptionRateOfCarbohydrates > 0 &&
            User.AbsorptionRateOfProteins > 0 &&
            User.AbsorptionRateOfFats > 0 &&
            // Коэффициенты
            User.CarbohydrateCoefficient > 0 &&
            User.ProteinCoefficient > 0 &&
            User.FatCoefficient > 0;

        public ICommand OkCommand =>
            new Command(OkExecute);

        #endregion

        #endregion
    }
}
