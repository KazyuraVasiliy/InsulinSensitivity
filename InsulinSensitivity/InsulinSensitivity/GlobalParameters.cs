using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;
using Models = DataAccessLayer.Models;

namespace InsulinSensitivity
{
    public static class GlobalParameters
    {
        /// <summary>
        /// Текущий пользователь
        /// </summary>
        public static Models.User User { get; set; }

        /// <summary>
        /// Путь до БД
        /// </summary>
        public static string DbPath { get; set; }

        /// <summary>
        /// Навигация
        /// </summary>
        public static INavigation Navigation { get; set; }

        /// <summary>
        /// Настройки
        /// </summary>
        public static class Settings
        {
            private static bool? isActiveBasal;
            /// <summary>
            /// Учитывается ли активный базальный
            /// </summary>
            public static bool IsActiveBasal
            {
                get
                {
                    if (isActiveBasal == null)
                        isActiveBasal = Preferences.Get("isActiveBasal", false);
                    return isActiveBasal.Value;
                }
                set
                {
                    Preferences.Set("isActiveBasal", value);
                    isActiveBasal = value;
                }
            }

            private static bool? isAverageCalculateActive;
            /// <summary>
            /// Активен ли расчёт ФЧИ по средним значениям
            /// </summary>
            public static bool IsAverageCalculateActive
            {
                get
                {
                    if (isAverageCalculateActive == null)
                        isAverageCalculateActive = Preferences.Get("isAverageCalculateActive", true);
                    return isAverageCalculateActive.Value;
                }
                set
                {
                    Preferences.Set("isAverageCalculateActive", value);
                    isAverageCalculateActive = value;
                }
            }

            private static bool? isExerciseCalculateActive;
            /// <summary>
            /// Активен ли расчёт ФЧИ по нагрузкам
            /// </summary>
            public static bool IsExerciseCalculateActive
            {
                get
                {
                    if (isExerciseCalculateActive == null)
                        isExerciseCalculateActive = Preferences.Get("isExerciseCalculateActive", true);
                    return isExerciseCalculateActive.Value;
                }
                set
                {
                    Preferences.Set("isExerciseCalculateActive", value);
                    isExerciseCalculateActive = value;
                }
            }

            private static bool? isCycleCalculateActive;
            /// <summary>
            /// Активен ли расчёт ФЧИ по дню цикла
            /// </summary>
            public static bool IsCycleCalculateActive
            {
                get
                {
                    if (isCycleCalculateActive == null)
                        isCycleCalculateActive = Preferences.Get("isCycleCalculateActive", true);
                    return isCycleCalculateActive.Value;
                }
                set
                {
                    Preferences.Set("isCycleCalculateActive", value);
                    isCycleCalculateActive = value;
                }
            }

            private static int? eatingDuration;
            /// <summary>
            /// Длительность приёма пищи
            /// </summary>
            public static int EatingDuration
            {
                get
                {
                    if (eatingDuration == null)
                        eatingDuration = Preferences.Get("eatingDuration", 5);
                    return eatingDuration.Value;
                }
                set
                {
                    Preferences.Set("eatingDuration", value);
                    eatingDuration = value;
                }
            }

            private static int? lengthGraph;
            /// <summary>
            /// Размерность графика ФЧИ по циклу
            /// </summary>
            public static int LengthGraph
            {
                get
                {
                    if (lengthGraph == null)
                        lengthGraph = Preferences.Get("lengthGraph", 45);
                    return lengthGraph.Value;
                }
                set
                {
                    Preferences.Set("lengthGraph", value);
                    lengthGraph = value;
                }
            }

            private static int? cannulaLifespan;
            /// <summary>
            /// Продолжительность жизни канюли
            /// </summary>
            public static int CannulaLifespan
            {
                get
                {
                    if (cannulaLifespan == null)
                        cannulaLifespan = Preferences.Get("cannulaLifespan", 3);
                    return cannulaLifespan.Value;
                }
                set
                {
                    Preferences.Set("cannulaLifespan", value);
                    cannulaLifespan = value;
                }
            }

            private static int? catheterLifespan;
            /// <summary>
            /// Продолжительность жизни катетера
            /// </summary>
            public static int CatheterLifespan
            {
                get
                {
                    if (catheterLifespan == null)
                        catheterLifespan = Preferences.Get("catheterLifespan", 3);
                    return catheterLifespan.Value;
                }
                set
                {
                    Preferences.Set("catheterLifespan", value);
                    catheterLifespan = value;
                }
            }

            private static int? cartridgeLifespan;
            /// <summary>
            /// Продолжительность жизни картриджа
            /// </summary>
            public static int CartridgeLifespan
            {
                get
                {
                    if (cartridgeLifespan == null)
                        cartridgeLifespan = Preferences.Get("cartridgeLifespan", 6);
                    return cartridgeLifespan.Value;
                }
                set
                {
                    Preferences.Set("cartridgeLifespan", value);
                    cartridgeLifespan = value;
                }
            }

            private static int? batteryLifespan;
            /// <summary>
            /// Продолжительность жизни батарейки
            /// </summary>
            public static int BatteryLifespan
            {
                get
                {
                    if (batteryLifespan == null)
                        batteryLifespan = Preferences.Get("batteryLifespan", 30);
                    return batteryLifespan.Value;
                }
                set
                {
                    Preferences.Set("batteryLifespan", value);
                    batteryLifespan = value;
                }
            }
        }
    }
}
