using System;
using System.Collections;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Xamarin.Essentials;

using DataAccessLayer.Contexts;
using Models = DataAccessLayer.Models;

namespace InsulinSensitivity
{
    public partial class App : Application
    {
        /// <summary>
        /// Имя БД
        /// </summary>
        public const string DBFILENAME = "InsulinSensitivityApp.db";

        public App()
        {
            InitializeComponent();
            VersionTracking.Track();

            GlobalParameters.DbPath = DependencyService.Get<IPath>().GetDatabasePath(DBFILENAME);

            // Инициализация главной страницы
            MainPage = new MainPage();

            // Инициализация навигации
            GlobalParameters.Navigation = ((MasterDetailPage)MainPage).Detail.Navigation;
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
