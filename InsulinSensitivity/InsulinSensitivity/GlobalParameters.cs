using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
