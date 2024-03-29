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
using System.Threading.Tasks;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;

namespace InsulinSensitivity
{
    public class MainPageMasterViewModel : ObservableBase
    {
        #region Fields

        /// <summary>
        /// Отмена асинхронной операции
        /// </summary>
        private CancellationTokenSource token =
            new CancellationTokenSource();

        /// <summary>
        /// Семафор
        /// </summary>
        private Semaphore semaphore =
            new Semaphore(1, 1);

        #endregion

        #region Constructor

        public MainPageMasterViewModel()
        {
            // Инициализация
            Items = new ObservableCollection<MainPageMasterItemModel>()
            {
                new MainPageMasterItemModel("\xe12c", "Пользователь", EditUserCommand),
                new MainPageMasterItemModel("\xe0e3", "Активности", EditExerciseTypeCommand),
                new MainPageMasterItemModel("\xe120", "Периоды", EditEatingTypeCommand),
                new MainPageMasterItemModel("\xe010", "Циклы", EditMenstrualCycleCommand),
                new MainPageMasterItemModel("\xe0e0", "Расходка", EditExpendableMaterialCommand),
                new MainPageMasterItemModel("\xe091", "Инсулины", EditInsulinTypeCommand, true),
                //new MainPageMasterItemModel("\xe062", "Экспорт", ExportCommand),
                new MainPageMasterItemModel("\xe043", "Статистика", StatisticCommand),
                new MainPageMasterItemModel("\xe04f", "Настройки", SettingsCommand, true),
                new MainPageMasterItemModel("\xe125", "Создать резервную копию", CreateBackupCommand),
                new MainPageMasterItemModel("\xe064", "Восстановить из копии", RestoreBackupCommand, true),
                new MainPageMasterItemModel("\xe0be", "Информация", InformationCommand),
                new MainPageMasterItemModel("\xe09d", "Помощь", HelpCommand),
            };

            // Задачи
            _ = UpdateNightscoutStatusProcess(token);

            // Подписки на события
            MessagingCenter.Subscribe<User.UserPageViewModel>(this, "User",
                async x => await UpdateNightscoutStatus());

            MessagingCenter.Subscribe<MainPageDetailViewModel>(this, "Init",
                async x => await UpdateNightscoutStatus());
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

        private bool? isNightscoutEnabled;
        /// <summary>
        /// Статус Nightscout
        /// </summary>
        public bool? IsNightscoutEnabled
        {
            get => isNightscoutEnabled;
            set
            {
                isNightscoutEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool? theme;
        /// <summary>
        /// Тема
        /// </summary>
        public bool? Theme
        {
            get => theme;
            set
            {
                theme = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Collections

        /// <summary>
        /// Меню
        /// </summary>
        public ObservableCollection<MainPageMasterItemModel> Items { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Получает статус Nightscout
        /// </summary>
        /// <returns></returns>
        private async Task UpdateNightscoutStatus()
        {
            semaphore.WaitOne();

            if (GlobalParameters.User == null || string.IsNullOrWhiteSpace(GlobalParameters.User.NightscoutUri))
                IsNightscoutEnabled = null;
            else
            {
                if (!string.IsNullOrWhiteSpace(GlobalParameters.User.NightscoutUri))
                {                    
                    try
                    {
                        var baseUri = GlobalParameters.User.NightscoutUri.TrimEnd('/');
                        using (var client = new HttpClient())
                        {
                            client.Timeout = TimeSpan.FromSeconds(5);

                            if (!string.IsNullOrWhiteSpace(GlobalParameters.User.NightscoutApiKey))
                            {
                                var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(GlobalParameters.User.NightscoutApiKey));
                                var hashStr = string.Concat(hash.Select(b => b.ToString("x2")));

                                client.DefaultRequestHeaders.Add("api-secret", hashStr);
                            }

                            var result = await client.GetAsync(baseUri + "/status");
                            IsNightscoutEnabled = result.IsSuccessStatusCode;
                        }
                    }
                    catch
                    {
                        IsNightscoutEnabled = false;
                    }
                }
            }

            semaphore.Release();
        }

        /// <summary>
        /// Периодически проверяет статус Nightscout
        /// </summary>
        private async Task UpdateNightscoutStatusProcess(CancellationTokenSource token)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                    return;

                await UpdateNightscoutStatus();
                await Task.Delay(1000 * 30);
            }
        }

        #endregion

        #region Commands

        #region --Edit User

        private async void EditUserExecute()
        {
            var userPage = new User.UserPage();
            userPage.BindingContext = new User.UserPageViewModel(GlobalParameters.User?.Id);

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

        #region --Edit Expendable Material

        private async void EditExpendableMaterialExecute()
        {
            if (!EditExpendableMaterialCanExecute())
            {
                await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Ошибка",
                    "Вам недоступен данный раздел",
                    "Ok");
                return;
            }

            var expendableMaterialPage = new ExpendableMaterial.ExpendableMaterialPage();
            expendableMaterialPage.BindingContext = new ExpendableMaterial.ExpendableMaterialViewModel();

            await GlobalParameters.Navigation.PushAsync(expendableMaterialPage, true);
        }

        private bool EditExpendableMaterialCanExecute() =>
            GlobalParameters.User != null;

        public ICommand EditExpendableMaterialCommand =>
            new Command(EditExpendableMaterialExecute, EditExpendableMaterialCanExecute);

        #endregion

        #region --Statistic

        private async void StatisticExecute()
        {
            try
            {
                if (!StatisticCanExecute())
                {
                    await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                        "Ошибка",
                        "Пользователь не создан",
                        "Ok");
                    return;
                }

                var statisticPage = new Statistic.StatisticPage();
                statisticPage.BindingContext = new Statistic.StatisticPageViewModel(statisticPage);

                await GlobalParameters.Navigation.PushAsync(statisticPage, true);
            }
            catch (Exception ex)
            {
                await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Ошибка",
                    ex.Message + ex?.InnerException?.Message,
                    "Ok");
            }
        }

        private bool StatisticCanExecute() =>
            GlobalParameters.User != null;

        public ICommand StatisticCommand =>
            new Command(StatisticExecute);

        #endregion

        #region --Information

        private async void InformationExecute()
        {
            var informationPage = new Information.InformationPage();
            informationPage.BindingContext = new Information.InformationPageViewModel();

            await GlobalParameters.Navigation.PushAsync(informationPage, true);
        }

        public ICommand InformationCommand =>
            new Command(InformationExecute);

        #endregion

        #region --Help

        private async void HelpExecute()
        {
            try
            {
                await Browser.OpenAsync("https://t.me/+TcNRD7VazqQCC4ub", BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Ошибка",
                    ex.Message + ex?.InnerException?.Message,
                    "Ok");
            }
        }

        public ICommand HelpCommand =>
            new Command(HelpExecute);

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
                    data.AddLast($"Имя;Дата рождения;Рост;Вес;УК;БК;ЖК;Гипоглекимия;Низкий;Целевой;Высокий;Гипергликимия;Базальный;Болюсный;Дозировка;Помпа");
                    data.AddLast($"{GlobalParameters.User.Name};{GlobalParameters.User.BirthDate.ToString("dd.MM.yyyy")};{GlobalParameters.User.Height};{GlobalParameters.User.Weight};" +
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
                        $"ФЧИ по средним;ФЧИ по нагрузкам;ФЧИ по циклу;ФЧИ пользователя;ФЧИ факт;Доза расчётная;Доза фактическая;Ожидаемый сахар;Точность программы;Точность пользователя");
                    foreach (var el in eatings)
                        data.AddLast($"{el.DateCreated:dd.MM.yyyy};{el.InjectionTime};{el.EatingType.Name};" +
                            $"{el.GlucoseStart};{el.GlucoseEnd};" +
                            $"{el.Protein};{el.Fat};{el.Carbohydrate};" +
                            $"{el.Exercise.ExerciseType.Name};{el.Exercise.Duration};{el.Exercise.HoursAfterInjection};" +
                            $"{el.BasalDose};{el.ActiveInsulinStart};{el.ActiveInsulinEnd};" +
                            $"{el.InsulinSensitivityAutoOne};{el.InsulinSensitivityAutoTwo};{el.InsulinSensitivityAutoThree};{el.InsulinSensitivityUser};{el.InsulinSensitivityFact};" +
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

        #region --Create Backup

        private async void CreateBackupExecute()
        {
            try
            {
                if (GlobalParameters.User == null)
                    return;

                bool question = await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Создать резервную копию?",
                    "Вы уверены, что хотите создать резервную копию?",
                    "Да",
                    "Нет");

                if (!question)
                    return;

                if (await Permission.CheckAndRequestStorageWritePermission() != PermissionStatus.Granted)
                    return;

                string directoryName = "InsulinSensitivity";
                string fileName = Path.Combine(directoryName, $"{DateTime.Now:dd.MM.yy.HH.mm.ss}.backup.isdb");

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

                var destinationPath = DependencyService.Get<IFileWorker>().GetPath(fileName);
                await DependencyService.Get<IFileWorker>().CopyAsync(GlobalParameters.DbPath, destinationPath);
                await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Информация",
                    $"Резервная копия успешно создана: {DependencyService.Get<IFileWorker>().GetPath(fileName)}",
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

        public ICommand CreateBackupCommand =>
             new Command(CreateBackupExecute);

        #endregion

        #region --Restore Backup

        private async void RestoreBackupExecute()
        {
            try
            {
                bool question = await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Восстановить из резервной копии?",
                    "Вы уверены, что хотите восстановить данные из резервной копии?",
                    "Да",
                    "Нет");

                if (!question)
                    return;

                if (await Permission.CheckAndRequestStorageWritePermission() != PermissionStatus.Granted)
                    return;

                var result = await FilePicker.PickAsync();
                if (result.FullPath == null)
                    return;

                var ext = Path.GetExtension(result.FullPath);
                if (Path.GetExtension(result.FullPath) != ".isdb")
                {
                    await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                        "Ошибка",
                        "Выбранный файл не является резервной копией",
                        "Ok");
                    return;
                }

                await DependencyService.Get<IFileWorker>().CopyAsync(result.FullPath, GlobalParameters.DbPath);
                await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Информация",
                    "Восстановление из резервной копии завершено",
                    "Ok");
                MessagingCenter.Send(this, "RestoreBackup");
            }
            catch (Exception ex)
            {
                await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Ошибка",
                    ex.Message + ex?.InnerException?.Message,
                    "Ok");
            }
        }

        public ICommand RestoreBackupCommand =>
             new Command(RestoreBackupExecute);

        #endregion

        #region --Settings

        private async void SettingsExecute()
        {
            if (!SettingsCanExecute())
            {
                await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Ошибка",
                    "Пользователь не создан",
                    "Ok");
                return;
            }

            var settingsPage = new Settings.SettingsPage();
            settingsPage.BindingContext = new Settings.SettingsPageViewModel();

            await GlobalParameters.Navigation.PushAsync(settingsPage, true);
        }

        private bool SettingsCanExecute() =>
            GlobalParameters.User != null;

        public ICommand SettingsCommand =>
            new Command(SettingsExecute);

        #endregion

        #region --Change Theme

        private void ChangeThemeExecute()
        {
            if (Theme == null)
            {
                App.Current.UserAppTheme = OSAppTheme.Light;

                Theme = true;
                return;
            }
            else if (Theme == true)
            {
                App.Current.UserAppTheme = OSAppTheme.Dark;

                Theme = false;
                return;
            }
            else if (Theme == false)
            {
                App.Current.UserAppTheme = OSAppTheme.Unspecified;

                Theme = null;
                return;
            }
        }

        public ICommand ChangeThemeCommand =>
            new Command(ChangeThemeExecute);

        #endregion

        #endregion
    }
}
