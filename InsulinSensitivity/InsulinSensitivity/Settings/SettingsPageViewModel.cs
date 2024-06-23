using BusinessLogicLayer.Attributes;
using BusinessLogicLayer.Service;
using BusinessLogicLayer.ViewModel;
using DataAccessLayer.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Models = DataAccessLayer.Models;

namespace InsulinSensitivity.Settings
{
    public class SettingsPageViewModel : ObservableBase
    {
        #region Constructors

        public SettingsPageViewModel() =>
            _ = InitSettings();

        #endregion

        #region Properties

        /// <summary>
        /// Видны ли настройки менструального цикла и беременности
        /// </summary>
        public bool IsCycleSettingsVisibility =>
            GlobalParameters.IsCycleSettingsAccess;

        /// <summary>
        /// Видна ли настройка по помпе
        /// </summary>
        public bool IsPumpVisibility =>
            GlobalParameters.User.IsPump;

        #region --Settings

        /// <summary>
        /// Расчёт ФЧИ как средневзвешенного
        /// </summary>
        [Model]
        public bool IsWeightedAverageInsulinSensitivity { get; set; }

        /// <summary>
        /// Учитывается ли активный базальный
        /// </summary>
        [Model]
        public bool IsActiveBasal { get; set; }

        /// <summary>
        /// Активен ли расчёт ФЧИ по средним значениям
        /// </summary>
        [Model]
        public bool IsAverageCalculateActive { get; set; }

        /// <summary>
        /// Активен ли расчёт ФЧИ по нагрузкам
        /// </summary>
        [Model]
        public bool IsExerciseCalculateActive { get; set; }

        /// <summary>
        /// Активен ли расчёт ФЧИ по дню цикла
        /// </summary>
        [Model]
        public bool IsCycleCalculateActive { get; set; }

        /// <summary>
        /// Беременность
        /// </summary>
        [Model]
        public bool IsPregnancy { get; set; }

        /// <summary>
        /// Активен ли расчёт ФЧИ по дню использования канюли
        /// </summary>
        [Model]
        public bool IsCannulaCalculateActive { get; set; }

        /// <summary>
        /// Активен ли расчёт ФЧИ по часам
        /// </summary>
        [Model]
        public bool IsHoursCalculateActive { get; set; }

        private int lengthGraph;
        /// <summary>
        /// Размерность графика ФЧИ по циклу
        /// </summary>
        [Model]
        public int LengthGraph
        {
            get => lengthGraph;
            set
            {
                if (lengthGraph != value && value > 0)
                {
                    lengthGraph = value;
                    OnPropertyChanged();
                }
            }
        }

        private int cannulaLifespan;
        /// <summary>
        /// Продолжительность жизни канюли
        /// </summary>
        [Model]
        public int CannulaLifespan
        {
            get => cannulaLifespan;
            set
            {
                if (cannulaLifespan != value && value > 0)
                {
                    cannulaLifespan = value;
                    OnPropertyChanged();
                }
            }
        }

        private int catheterLifespan;
        /// <summary>
        /// Продолжительность жизни катетера
        /// </summary>
        [Model]
        public int CatheterLifespan
        {
            get => catheterLifespan;
            set
            {
                if (catheterLifespan != value && value > 0)
                {
                    catheterLifespan = value;
                    OnPropertyChanged();
                }
            }
        }

        private int cartridgeLifespan;
        /// <summary>
        /// Продолжительность жизни картриджа
        /// </summary>
        [Model]
        public int CartridgeLifespan
        {
            get => cartridgeLifespan;
            set
            {
                if (cartridgeLifespan != value && value > 0)
                {
                    cartridgeLifespan = value;
                    OnPropertyChanged();
                }
            }
        }

        private int batteryLifespan;
        /// <summary>
        /// Продолжительность жизни батарейки
        /// </summary>
        [Model]
        public int BatteryLifespan
        {
            get => batteryLifespan;
            set
            {
                if (batteryLifespan != value && value > 0)
                {
                    batteryLifespan = value;
                    OnPropertyChanged();
                }
            }
        }

        private int monitoringLifespan;
        /// <summary>
        /// Продолжительность жизни сенсора
        /// </summary>
        [Model]
        public int MonitoringLifespan
        {
            get => monitoringLifespan;
            set
            {
                if (monitoringLifespan != value && value > 0)
                {
                    monitoringLifespan = value;
                    OnPropertyChanged();
                }
            }
        }

        private decimal insulinDailyConsumptionForPump;
        /// <summary>
        /// Суточный расход инсулина на помпе
        /// </summary>
        [Model]
        public decimal InsulinDailyConsumptionForPump
        {
            get => insulinDailyConsumptionForPump;
            set
            {
                if (insulinDailyConsumptionForPump != value && value > 0)
                {
                    insulinDailyConsumptionForPump = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Инициализирует настройки
        /// </summary>
        /// <returns></returns>
        private async Task InitSettings()
        {
            AsyncBase.Open("Инициализация настроек");

            using (var db = new ApplicationContext(GlobalParameters.DbPath))
            {
                var user = await db.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == GlobalParameters.User.Id);

                CopyService.Copy(user, this,
                    destinationModel: "");
            }

            AsyncBase.Close();
        }

        #endregion

        #region Commands

        #region --Ok

        private async void OkExecute()
        {
            if (!OkCanExecute())
            {
                await GlobalMethods.ShowError("Продолжительности жизни расходных материалов, длительность приёма пищи и размерность графика ФЧИ должны быть положительными");
                return;
            }

            AsyncBase.Open();

            using (var db = new ApplicationContext(GlobalParameters.DbPath))
            {
                var user = db.Users.Find(GlobalParameters.User.Id);

                if (user.IsActiveBasal != IsActiveBasal)
                {
                    var question = "Пересчитать пищевые коэффициенты при переходе на новую формулу расчёта?";
                    var answer = await GlobalMethods.AskAQuestion(question);

                    if (answer)
                    {
                        if (IsActiveBasal)
                        {
                            user.CarbohydrateCoefficient = Methods.Round(user.CarbohydrateCoefficient * 1.15M, 2);
                            user.ProteinCoefficient = Methods.Round(user.ProteinCoefficient / 1.15M, 2);
                            user.FatCoefficient = Methods.Round(user.FatCoefficient / 1.15M, 2);
                        }
                        else
                        {
                            user.CarbohydrateCoefficient = Methods.Round(user.CarbohydrateCoefficient / 1.15M, 2);
                            user.ProteinCoefficient = Methods.Round(user.ProteinCoefficient * 1.15M, 2);
                            user.FatCoefficient = Methods.Round(user.FatCoefficient * 1.15M, 2);
                        }
                    }
                }

                if (IsPregnancy)
                    IsCycleCalculateActive = false;

                CopyService.Copy(this, user,
                    sourceModel: "");

                await db.SaveChangesAsync();
                GlobalParameters.User = db.Users
                    .Include(x => x.BasalType)
                    .Include(x => x.BolusType)
                    .FirstOrDefault();

                MessagingCenter.Send(this, "User");
                await GlobalParameters.Navigation.PopAsync();
            }

            AsyncBase.Close();
        }

        private bool OkCanExecute() =>
            // Размерность графика ФЧИ
            LengthGraph > 0 &&
            // Продолжительности жизни
            CannulaLifespan > 0 &&
            CatheterLifespan > 0 &&
            CartridgeLifespan > 0 &&
            BatteryLifespan > 0 &&
            MonitoringLifespan > 0;

        public ICommand OkCommand =>
            new Command(OkExecute);

        #endregion

        #endregion
    }
}
