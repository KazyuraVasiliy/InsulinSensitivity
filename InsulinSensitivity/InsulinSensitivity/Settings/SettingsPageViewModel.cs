using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

using DataAccessLayer.Contexts;
using BusinessLogicLayer.ViewModel;
using BusinessLogicLayer.Service;
using Models = DataAccessLayer.Models;

namespace InsulinSensitivity.Settings
{
    public class SettingsPageViewModel : ObservableBase
    {
        #region Properties

        /// <summary>
        /// Видна ли настройка по циклам
        /// </summary>
        public bool IsCycleVisibility =>
            !GlobalParameters.User.Gender;

        /// <summary>
        /// Видна ли настройка по помпе
        /// </summary>
        public bool IsPumpVisibility =>
            GlobalParameters.User.IsPump;

        /// <summary>
        /// Учитывается ли активный базальный
        /// </summary>
        public bool IsActiveBasal
        {
            get => GlobalParameters.Settings.IsActiveBasal;
            set => _ = SetActiveBasal(value);
        }

        /// <summary>
        /// Активен ли расчёт ФЧИ по средним значениям
        /// </summary>
        public bool IsAverageCalculateActive
        {
            get => GlobalParameters.Settings.IsAverageCalculateActive;
            set
            {
                if (GlobalParameters.Settings.IsAverageCalculateActive != value)
                {
                    GlobalParameters.Settings.IsAverageCalculateActive = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Активен ли расчёт ФЧИ по нагрузкам
        /// </summary>
        public bool IsExerciseCalculateActive
        {
            get => GlobalParameters.Settings.IsExerciseCalculateActive;
            set
            {
                if (GlobalParameters.Settings.IsExerciseCalculateActive != value)
                {
                    GlobalParameters.Settings.IsExerciseCalculateActive = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Активен ли расчёт ФЧИ по дню цикла
        /// </summary>
        public bool IsCycleCalculateActive
        {
            get => GlobalParameters.Settings.IsCycleCalculateActive;
            set
            {
                if (GlobalParameters.Settings.IsCycleCalculateActive != value)
                {
                    GlobalParameters.Settings.IsCycleCalculateActive = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Активен ли расчёт ФЧИ по дню использования канюли
        /// </summary>
        public bool IsCannulaCalculateActive
        {
            get => GlobalParameters.Settings.IsCannulaCalculateActive;
            set
            {
                if (GlobalParameters.Settings.IsCannulaCalculateActive != value)
                {
                    GlobalParameters.Settings.IsCannulaCalculateActive = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Длительность приёма пищи
        /// </summary>
        public int EatingDuration
        {
            get => GlobalParameters.Settings.EatingDuration;
            set
            {
                if (GlobalParameters.Settings.EatingDuration != value && value > 0)
                {
                    GlobalParameters.Settings.EatingDuration = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Размерность графика ФЧИ по циклу
        /// </summary>
        public int LengthGraph
        {
            get => GlobalParameters.Settings.LengthGraph;
            set
            {
                if (GlobalParameters.Settings.LengthGraph != value && value > 0)
                {
                    GlobalParameters.Settings.LengthGraph = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Продолжительность жизни канюли
        /// </summary>
        public int CannulaLifespan
        {
            get => GlobalParameters.Settings.CannulaLifespan;
            set
            {
                if (GlobalParameters.Settings.CannulaLifespan != value && value > 0)
                {
                    GlobalParameters.Settings.CannulaLifespan = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Продолжительность жизни катетера
        /// </summary>
        public int CatheterLifespan
        {
            get => GlobalParameters.Settings.CatheterLifespan;
            set
            {
                if (GlobalParameters.Settings.CatheterLifespan != value && value > 0)
                {
                    GlobalParameters.Settings.CatheterLifespan = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Продолжительность жизни картриджа
        /// </summary>
        public int CartridgeLifespan
        {
            get => GlobalParameters.Settings.CartridgeLifespan;
            set
            {
                if (GlobalParameters.Settings.CartridgeLifespan != value && value > 0)
                {
                    GlobalParameters.Settings.CartridgeLifespan = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Продолжительность жизни батарейки
        /// </summary>
        public int BatteryLifespan
        {
            get => GlobalParameters.Settings.BatteryLifespan;
            set
            {
                if (GlobalParameters.Settings.BatteryLifespan != value && value > 0)
                {
                    GlobalParameters.Settings.BatteryLifespan = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Продолжительность жизни сенсора
        /// </summary>
        public int MonitoringLifespan
        {
            get => GlobalParameters.Settings.MonitoringLifespan;
            set
            {
                if (GlobalParameters.Settings.MonitoringLifespan != value && value > 0)
                {
                    GlobalParameters.Settings.MonitoringLifespan = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Methods

        private async Task SetActiveBasal(bool value)
        {
            if (value == GlobalParameters.Settings.IsActiveBasal)
                return;

            bool question = await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                "Запрос",
                "Пересчитать пищевые коэффициенты при переходе на новую формулу расчёта?",
                "Да",
                "Нет");

            if (question)
                await AsyncBase.NewTask(() =>
                {
                    using (var db = new ApplicationContext(GlobalParameters.DbPath))
                    {
                        var entity = db.Users.Find(GlobalParameters.User.Id);
                        if (entity != null)
                        {
                            if (value)
                            {
                                entity.CarbohydrateCoefficient = Math.Round(entity.CarbohydrateCoefficient * 1.15M, 2, MidpointRounding.AwayFromZero);
                                entity.ProteinCoefficient = Math.Round(entity.ProteinCoefficient / 1.15M, 2, MidpointRounding.AwayFromZero);
                                entity.FatCoefficient = Math.Round(entity.FatCoefficient / 1.15M, 2, MidpointRounding.AwayFromZero);
                            }
                            else
                            {
                                entity.CarbohydrateCoefficient = Math.Round(entity.CarbohydrateCoefficient / 1.15M, 2, MidpointRounding.AwayFromZero);
                                entity.ProteinCoefficient = Math.Round(entity.ProteinCoefficient * 1.15M, 2, MidpointRounding.AwayFromZero);
                                entity.FatCoefficient = Math.Round(entity.FatCoefficient * 1.15M, 2, MidpointRounding.AwayFromZero);
                            }

                            db.SaveChanges();
                        }
                    }
                }, "Применение опции");

            GlobalParameters.Settings.IsActiveBasal = value;
            OnPropertyChanged(nameof(IsActiveBasal));
        }

        #endregion
    }
}
