using System;
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

        public MainPageMasterViewModel()
        {
            Items = new List<MainPageMasterItemModel>()
            {
                new MainPageMasterItemModel("\xe12c", "Пользователь", EditUserCommand),
                new MainPageMasterItemModel("\xe0e3", "Нагрузки", EditExerciseTypeCommand),
                new MainPageMasterItemModel("\xe120", "Периоды", EditEatingTypeCommand),
                new MainPageMasterItemModel("\xe010", "Циклы", EditMenstrualCycleCommand),
                new MainPageMasterItemModel("\xe091", "Инсулины", EditInsulinTypeCommand, true),
                new MainPageMasterItemModel("\xe125", "Экспорт", ExportCommand),
            };
        }

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

        public string Version =>
            VersionTracking.CurrentVersion;

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

        #region --Edit Exercise Type

        private async void EditExerciseTypeExecute()
        {
            var exerciseTypePage = new ExerciseType.ExerciseTypePage();
            exerciseTypePage.BindingContext = new ExerciseType.ExerciseTypePageViewModel();

            await GlobalParameters.Navigation.PushAsync(exerciseTypePage, true);
        }

        public ICommand EditExerciseTypeCommand =>
            new Command(EditExerciseTypeExecute);

        #endregion

        #region --Edit Eating Type

        private async void EditEatingTypeExecute()
        {
            var eatingTypePage = new EatingType.EatingTypePage();
            eatingTypePage.BindingContext = new EatingType.EatingTypePageViewModel();

            await GlobalParameters.Navigation.PushAsync(eatingTypePage, true);
        }

        public ICommand EditEatingTypeCommand =>
            new Command(EditEatingTypeExecute);

        #endregion

        #region --Edit Insulin Type

        private async void EditInsulinTypeExecute()
        {
            var insulinTypePage = new InsulinType.InsulinTypePage();
            insulinTypePage.BindingContext = new InsulinType.InsulinTypePageViewModel();

            await GlobalParameters.Navigation.PushAsync(insulinTypePage, true);
        }

        public ICommand EditInsulinTypeCommand =>
            new Command(EditInsulinTypeExecute);

        #endregion

        #region --Edit Menstrual Cycle 

        private async void EditMenstrualCycleExecute()
        {
            if (!EditMenstrualCycleCanExecute())
            {
                await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Ошибка",
                    "Вам недоступен данный раздел",
                    "Ok");
                return;
            }

            var menstrualCyclePage = new MenstrualCycle.MenstrualCyclePage();
            menstrualCyclePage.BindingContext = new MenstrualCycle.MenstrualCyclePageViewModel();

            await GlobalParameters.Navigation.PushAsync(menstrualCyclePage, true);
        }

        private bool EditMenstrualCycleCanExecute() =>
            GlobalParameters.User?.Gender == false;

        public ICommand EditMenstrualCycleCommand =>
            new Command(EditMenstrualCycleExecute, EditMenstrualCycleCanExecute);

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
                        .Include(x => x.EatingType)
                        .ToList()
                        .OrderByDescending(x =>
                            x.DateCreated)
                        .ThenByDescending(x =>
                            x.InjectionTime);

                    data.AddLast($"Дата;Время;Период;Исходный;Отработка;Б;Ж;У;Нагрузка;Длительность;Часов после инъекции;База;Активный до;Активный после;" +
                        $"ФЧИ по средним;ФЧИ по нагрузкам;ФЧИ пользователя;ФЧИ факт;Доза расчётная;Доза фактическая;Ожидаемый сахар;Точность программы;Точность пользователя");
                    foreach (var el in eatings)
                        data.AddLast($"{el.DateCreated:dd.MM.yyyy};{el.InjectionTime};{el.EatingType.Name};" +
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
