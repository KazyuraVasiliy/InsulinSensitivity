using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

using DataAccessLayer.Contexts;
using BusinessLogicLayer.ViewModel;
using BusinessLogicLayer.Service;
using Models = DataAccessLayer.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using BusinessLogicLayer.Service.Interfaces;
using Xamarin.Essentials;
using System.IO;

namespace InsulinSensitivity.User
{
    public class UserPageViewModel : ObservableBase
    {
        #region Constructors

        public UserPageViewModel(Guid? userGuid = null)
        {
            if (userGuid != null)
            {
                using (var db = new ApplicationContext(GlobalParameters.DbPath))
                    User = db.Users
                        .Include(x => x.BolusType)
                        .Include(x => x.BasalType)
                        .FirstOrDefault(x => 
                            x.Id == userGuid.Value);
            }
            else User = new Models.User() { BirthDate = DateTime.Now.AddYears(-20) };

            // Инициализация коллекций
            using (var db = new ApplicationContext(GlobalParameters.DbPath))
                InsulinTypes = db.InsulinTypes
                    .OrderBy(x =>
                        x.Name)
                    .ToList();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Текущий пользователь
        /// </summary>
        public Models.User User { get; set; }

        /// <summary>
        /// Видны ли поля доступные при редактировании
        /// </summary>
        public bool IsEditableFieldsVisible =>
            User.Id != Guid.Empty;

        /// <summary>
        /// Рост
        /// </summary>
        public int Height
        {
            get => User.Height;
            set
            {
                User.Height = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Вес
        /// </summary>
        public int Weight
        {
            get => User.Weight;
            set
            {
                User.Weight = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// УК
        /// </summary>
        public decimal CarbohydrateCoefficient
        {
            get => User.CarbohydrateCoefficient;
            set
            {
                User.CarbohydrateCoefficient = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// БК
        /// </summary>
        public decimal ProteinCoefficient
        {
            get => User.ProteinCoefficient;
            set
            {
                User.ProteinCoefficient = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// ЖК
        /// </summary>
        public decimal FatCoefficient
        {
            get => User.FatCoefficient;
            set
            {
                User.FatCoefficient = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Гипоглекимия
        /// </summary>
        public decimal Hypoglycemia
        {
            get => User.Hypoglycemia;
            set
            {
                User.Hypoglycemia = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Низкий сахар
        /// </summary>
        public decimal LowSugar
        {
            get => User.LowSugar;
            set
            {
                User.LowSugar = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Целевая глюкоза
        /// </summary>
        public decimal TargetGlucose
        {
            get => User.TargetGlucose;
            set
            {
                User.TargetGlucose = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Высокий сахар
        /// </summary>
        public decimal HighSugar
        {
            get => User.HighSugar;
            set
            {
                User.HighSugar = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Гипергликимия
        /// </summary>
        public decimal Hyperglycemia
        {
            get => User.Hyperglycemia;
            set
            {
                User.Hyperglycemia = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Collections

        private List<Models.InsulinType> insulinTypes;
        /// <summary>
        /// Типы инсулина
        /// </summary>
        public List<Models.InsulinType> InsulinTypes
        {
            get => insulinTypes;
            set
            {
                insulinTypes = value;
                OnPropertyChanged(nameof(InsulinTypes));
            }
        }

        /// <summary>
        /// Точность дозирования инсулина
        /// </summary>
        public decimal[] DosingAccuracies { get; set; } =
            new decimal[]
            {
                0.01M,
                0.05M,
                0.1M,
                0.5M,
                1M,
                2M
            };

        #endregion

        #region Methods

        /// <summary>
        /// Запрос разрешения на запись файла
        /// </summary>
        /// <remarks>
        /// https://docs.microsoft.com/ru-ru/xamarin/essentials/permissions?tabs=android
        /// </remarks>
        /// <returns></returns>
        public async Task<PermissionStatus> CheckAndRequestStorageWritePermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            if (status == PermissionStatus.Granted)
                return status;

            if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
            {
                // Prompt the user to turn on in settings
                // On iOS once a permission has been denied it may not be requested again from the application
                return status;
            }

            status = await Permissions.RequestAsync<Permissions.StorageWrite>();
            return status;
        }

        #endregion

        #region Commands

        #region --Ok

        private async void OkExecute()
        {
            if (!OkCanExecute())
            {
                await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Ошибка",
                    "Заполните все поля",
                    "Ok");
                return;
            }

            using (var db = new ApplicationContext(GlobalParameters.DbPath))
            {
                var user = User.Id == Guid.Empty
                    ? new Models.User() { Id = Guid.NewGuid() }
                    : db.Users.Find(User.Id);

                user.Name = User.Name;
                user.BirthDate = User.BirthDate;
                user.Gender = User.Gender;

                user.Height = User.Height;
                user.Weight = User.Weight;

                user.Hypoglycemia = User.Hypoglycemia;
                user.LowSugar = User.LowSugar;
                user.TargetGlucose = User.TargetGlucose;
                user.HighSugar = User.HighSugar;
                user.Hyperglycemia = User.Hyperglycemia;

                user.BolusTypeId = User.BolusType.Id;
                user.BasalTypeId = User.BasalType.Id;

                user.DosingAccuracy = User.DosingAccuracy;
                user.IsPump = User.IsPump;

                if (User.Id == Guid.Empty)
                {
                    user.CarbohydrateCoefficient = Math.Round(Calculation.GetCarbohydrateCoefficient(user.BirthDate, user.Gender, user.Height, user.Weight), 2, MidpointRounding.AwayFromZero);
                    user.ProteinCoefficient = 0.3M;
                    user.FatCoefficient = 0.25M;
                }
                else
                {
                    user.CarbohydrateCoefficient = User.CarbohydrateCoefficient;
                    user.ProteinCoefficient = User.ProteinCoefficient;
                    user.FatCoefficient = User.FatCoefficient;
                }

                if (User.Id == Guid.Empty)
                    db.Users.Add(user);
                
                db.SaveChanges();
                GlobalParameters.User = db.Users
                    .Include(x => x.BasalType)
                    .Include(x => x.BolusType)
                    .FirstOrDefault();

                MessagingCenter.Send(this, "User");
                await GlobalParameters.Navigation.PopAsync();
            }
        }

        private bool OkCanExecute() =>
            // Имя
            !string.IsNullOrWhiteSpace(User.Name) &&
            // Рост и Вес
            User.Weight > 0 &&
            User.Height > 0 &&
            // Тип инсулина
            User.BasalType != null &&
            User.BolusType != null &&
            // Сахара
            User.Hypoglycemia > 0 &&
            User.LowSugar > User.Hypoglycemia &&
            User.TargetGlucose > User.LowSugar &&
            User.HighSugar > User.TargetGlucose &&
            User.Hyperglycemia > User.HighSugar &&
            // Точность дозирования
            User.DosingAccuracy > 0;

        public ICommand OkCommand =>
            new Command(OkExecute);

        #endregion

        #region --Export

        private async void ExportExecute()
        {
            try
            {
                bool question = await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Экспортировать?",
                    "Вы уверены, что хотите экспортировать данные?",
                    "Да",
                    "Нет");

                if (!question)
                    return;

                if (await CheckAndRequestStorageWritePermission() != PermissionStatus.Granted)
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
