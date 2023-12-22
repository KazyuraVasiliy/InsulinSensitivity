using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

using DataAccessLayer.Contexts;
using BusinessLogicLayer.ViewModel;
using BusinessLogicLayer.Service;
using Models = DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace InsulinSensitivity.ExerciseType
{
    public class ExerciseTypePageViewModel : ObservableBase
    {
        #region Constructors

        public ExerciseTypePageViewModel()
        {
            // Инициализация коллекций
            InitExerciseTypes();
        }

        #endregion

        #region Properties

        private bool isRefreshing;
        /// <summary>
        /// Указывает на то, что обновление завершено
        /// </summary>
        public bool IsRefreshing
        {
            get => isRefreshing;
            set
            {
                isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }

        #endregion

        #region Collections

        /// <summary>
        /// Список типов
        /// </summary>
        public ObservableCollection<Models.ExerciseType> Types { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Инициализирует список нагрузок
        /// </summary>
        private void InitExerciseTypes()
        {
            using (var db = new ApplicationContext(GlobalParameters.DbPath))
                Types = db.ExerciseTypes
                    .AsNoTracking()
                    .Where(x =>
                        x.DateDeleted == null)
                    .OrderBy(x =>
                        x.Name)
                    .ToObservable();
            OnPropertyChanged(nameof(Types));
        }

        #endregion

        #region Commands

        #region --Add

        private async void AddExecute()
        {
            try
            {
                string result = await GlobalParameters.Navigation.NavigationStack.Last().DisplayPromptAsync(
                    "Добавление", 
                    "Введите тип нагрузки",
                    accept: "Ok",
                    cancel: "Отмена");

                if (string.IsNullOrWhiteSpace(result))
                    return;

                result = result.Trim();
                using (var db = new ApplicationContext(GlobalParameters.DbPath))
                {
                    var type = db.ExerciseTypes
                        .ToList()
                        .FirstOrDefault(x =>
                            Methods.StringEqual(x.Name, result));

                    if (type != null)
                    {
                        if (type.DateDeleted == null)
                        {
                            await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                                "Ошибка",
                                "Такой тип уже существует",
                                "Ok");
                        }
                        else
                        {
                            bool question = await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                                "Восстановить?",
                                "Такой тип существовал, но был удалён. Восстановить?",
                                "Да",
                                "Нет");

                            if (question)
                            {
                                type.DateDeleted = null;
                                Types.Add(type);
                            }
                        }
                    }
                    else
                    {
                        type = new Models.ExerciseType()
                        {
                            Id = Guid.NewGuid(),
                            Name = result
                        };

                        db.ExerciseTypes.Add(type);
                        Types.Add(type);
                    }

                    db.SaveChanges();
                }                
            }
            catch (Exception ex)
            {
                await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Ошибка",
                    ex.Message + ex?.InnerException?.Message,
                    "Ok");
            }
        }

        public ICommand AddCommand =>
            new Command(AddExecute);

        #endregion

        #region --Edit

        private async void EditExecute(object obj)
        {
            try
            {
                var typeObj = (Models.ExerciseType)obj;
                string result = await GlobalParameters.Navigation.NavigationStack.Last().DisplayPromptAsync(
                    "Изменение",
                    "Введите новое наименование",
                    accept: "Ok",
                    cancel: "Отмена",
                    initialValue: typeObj.Name);

                if (string.IsNullOrWhiteSpace(result))
                    return;

                result = result.Trim();
                using (var db = new ApplicationContext(GlobalParameters.DbPath))
                {
                    var type = db.ExerciseTypes
                        .ToList()
                        .FirstOrDefault(x =>
                            Methods.StringEqual(x.Name, result) &&
                            x.DateDeleted == null);

                    if (type != null)
                    {
                        await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                            "Ошибка",
                            "Такой тип уже существует",
                            "Ok");
                    }
                    else
                    {
                        type = db.ExerciseTypes.Find(typeObj.Id);
                        type.Name = result;

                        db.SaveChanges();

                        Types.Remove(typeObj);
                        Types.Add(type);
                    }                    
                }
            }
            catch (Exception ex)
            {
                await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Ошибка",
                    ex.Message + ex?.InnerException?.Message,
                    "Ok");
            }
        }

        public ICommand EditCommand =>
            new Command(EditExecute);

        #endregion

        #region --Remove

        private async void RemoveExecute(object obj)
        {
            try
            {
                bool question = await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Удалить?",
                    "Вы уверены, что хотите удалить запись?",
                    "Да",
                    "Нет");

                if (!question)
                    return;

                var typeObj = (Models.ExerciseType)obj;
                using (var db = new ApplicationContext(GlobalParameters.DbPath))
                {
                    var type = db.ExerciseTypes.Find(typeObj.Id);
                    if (type != null)
                        type.DateDeleted = DateTimeOffset.Now;

                    db.SaveChanges();
                    Types.Remove(typeObj);
                }
            }
            catch (Exception ex)
            {
                await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Ошибка",
                    ex.Message + ex?.InnerException?.Message,
                    "Ok");
            }
        }

        public ICommand RemoveCommand =>
            new Command(RemoveExecute);

        #endregion

        #region --Refresh

        private void RefreshExecute()
        {
            InitExerciseTypes();
            IsRefreshing = false;
        }

        public ICommand RefreshCommand =>
            new Command(RefreshExecute);

        #endregion

        #endregion
    }
}
