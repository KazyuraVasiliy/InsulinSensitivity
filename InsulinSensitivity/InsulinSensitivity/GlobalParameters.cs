using System;
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
        /// Доступны ли настройки менструального цикла и беременности
        /// </summary>
        /// <remarks>Женщина старше 8 лет</remarks>
        public static bool IsCycleSettingsAccess =>
            User?.Gender == false &&
            (DateTime.MinValue + (DateTime.Now - GlobalParameters.User.BirthDate)).Year - 1 >= 8;
    }
}
