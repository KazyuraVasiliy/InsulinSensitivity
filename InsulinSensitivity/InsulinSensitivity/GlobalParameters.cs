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
        }
    }
}
