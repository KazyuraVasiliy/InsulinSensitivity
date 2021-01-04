﻿using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using System.Linq;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using Xamarin.Essentials;
using System.IO;

using DataAccessLayer.Contexts;
using BusinessLogicLayer.ViewModel;
using BusinessLogicLayer.Service;
using BusinessLogicLayer.Service.Interfaces;
using Models = DataAccessLayer.Models;

namespace InsulinSensitivity
{
    public class MainPageMasterViewModel : ObservableBase
    {
        #region Constructor

        public MainPageMasterViewModel() =>
            Items = new List<MainPageMasterItemModel>()
            {
                new MainPageMasterItemModel("\xe12c", "Пользователь", EditUserCommand, true),
                new MainPageMasterItemModel("\xe125", "Экспорт", ExportCommand),
            };

        #endregion

        #region Properties

        //private MainPageMaster page;
        ///// <summary>
        ///// Страница
        ///// </summary>
        //public MainPageMaster Page
        //{
        //    get
        //    {
        //        if (page == null)
        //            page = (MainPageMaster)GlobalParameters.Navigation.NavigationStack
        //                .FirstOrDefault(x =>
        //                    x is MainPageMaster page &&
        //                    page.BindingContext == this);
        //        return page;
        //    }
        //}

        #endregion

        #region Collections

        /// <summary>
        /// Меню
        /// </summary>
        public List<MainPageMasterItemModel> Items { get; private set; }

        #endregion

        #region Commands

        #region --Edit User

        private async void EditUserExecute()
        {
            if (GlobalParameters.User == null)
                return;

            var userPage = new User.UserPage();
            userPage.BindingContext = new User.UserPageViewModel(GlobalParameters.User.Id);

            await GlobalParameters.Navigation.PushAsync(userPage, true);
        }

        public ICommand EditUserCommand =>
            new Command(EditUserExecute);

        #endregion

        #region --Export

        private async void ExportExecute()
        {
            try
            {
                if (GlobalParameters.User == null)
                    return;

                bool question = await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Экспортировать?",
                    "Вы уверены, что хотите экспортировать данные?",
                    "Да",
                    "Нет");

                if (!question)
                    return;

                if (await Permission.CheckAndRequestStorageWritePermission() != PermissionStatus.Granted)
                    return;

                var data = new LinkedList<string>();
                using (var db = new ApplicationContext(GlobalParameters.DbPath))
                {
                    data.AddLast($"Имя;Рост;Вес;УК;БК;ЖК;Гипоглекимия;Низкий;Целевой;Высокий;Гипергликимия;Базальный;Болюсный;Дозировка;Помпа");
                    data.AddLast($"{GlobalParameters.User.Name};{GlobalParameters.User.Height};{GlobalParameters.User.Weight};" +
                        $"{GlobalParameters.User.CarbohydrateCoefficient};{GlobalParameters.User.ProteinCoefficient};{GlobalParameters.User.FatCoefficient};" +
                        $"{GlobalParameters.User.Hypoglycemia};{GlobalParameters.User.LowSugar};{GlobalParameters.User.TargetGlucose};{GlobalParameters.User.HighSugar};{GlobalParameters.User.Hyperglycemia};" +
                        $"{GlobalParameters.User.BasalType.Name};{GlobalParameters.User.BolusType.Name};" +
                        $"{GlobalParameters.User.DosingAccuracy};{GlobalParameters.User.IsPump}");

                    var eatings = db.Eatings
                        .Include(x => x.Exercise)
                        .Include(x => x.Exercise.ExerciseType)
                        .ToList()
                        .OrderByDescending(x =>
                            x.DateCreated)
                        .ThenByDescending(x =>
                            x.InjectionTime);

                    data.AddLast($"Дата;Время;Исходный;Отработка;Б;Ж;У;Нагрузка;Длительность;Часов после инъекции;База;Активный до;Активный после;" +
                        $"ФЧИ по средним;ФЧИ по нагрузкам;ФЧИ пользователя;ФЧИ факт;Доза расчётная;Доза фактическая;Ожидаемый сахар;Точность программы;Точность пользователя");
                    foreach (var el in eatings)
                        data.AddLast($"{el.DateCreated:dd.MM.yyyy};{el.InjectionTime};" +
                            $"{el.GlucoseStart};{el.GlucoseEnd};" +
                            $"{el.Protein};{el.Fat};{el.Carbohydrate};" +
                            $"{el.Exercise.ExerciseType.Name};{el.Exercise.Duration};{el.Exercise.HoursAfterInjection};" +
                            $"{el.BasalDose};{el.ActiveInsulinStart};{el.ActiveInsulinEnd};" +
                            $"{el.InsulinSensitivityAutoOne};{el.InsulinSensitivityAutoTwo};{el.InsulinSensitivityUser};{el.InsulinSensitivityFact};" +
                            $"{el.BolusDoseCalculate};{el.BolusDoseFact};" +
                            $"{el.ExpectedGlucose};" +
                            $"{el.AccuracyAuto};{el.AccuracyUser}");
                }

                string directoryName = "InsulinSensitivity";
                string fileName = Path.Combine(directoryName, "IS_Export.csv");

                var fileWorker = DependencyService.Get<IFileWorker>();
                await fileWorker.CreateDirectoryAsync(directoryName);

                if (await fileWorker.ExistsAsync(fileName))
                {
                    bool isRewrited = await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                        "Подтверждение",
                        "Файл уже существует, перезаписать его?",
                        "Да",
                        "Нет");

                    if (isRewrited == false)
                        return;
                }

                await DependencyService.Get<IFileWorker>().SaveTextAsync(fileName, string.Join("\n", data));
                await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Информация",
                    $"Файл успешно создан: {DependencyService.Get<IFileWorker>().GetPath(fileName)}",
                    "Ok");
            }
            catch (Exception ex)
            {
                await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Ошибка",
                    ex.Message + ex?.InnerException?.Message,
                    "Ok");
            }
        }

        public ICommand ExportCommand =>
             new Command(ExportExecute);

        #endregion

        #endregion
    }
}
