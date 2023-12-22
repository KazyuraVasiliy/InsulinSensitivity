using BusinessLogicLayer.Service;
using BusinessLogicLayer.ViewModel;
using DataAccessLayer.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using Models = DataAccessLayer.Models;

namespace InsulinSensitivity.ExpendableMaterial
{
    public class ExpendableMaterialViewModel : ObservableBase
    {
        #region Constructors

        public ExpendableMaterialViewModel()
        {
            // Инициализация коллекций
            InitData();
        }

        #endregion

        #region Properties

        private string comment;
        /// <summary>
        /// Комментарий
        /// </summary>
        public string Comment
        {
            get => comment;
            set
            {
                comment = value;
                OnPropertyChanged();
            }
        }

        private int strip;
        /// <summary>
        /// Тест-полоски
        /// </summary>
        public int Strip
        {
            get => strip;
            set
            {
                if (value != strip)
                {
                    strip = value;
                    OnPropertyChanged();
                }
            }
        }

        private int monitoring;
        /// <summary>
        /// Сенсор
        /// </summary>
        public int Monitoring
        {
            get => monitoring;
            set
            {
                if (value != monitoring)
                {
                    monitoring = value;
                    OnPropertyChanged();
                }
            }
        }

        private decimal basal;
        /// <summary>
        /// Базальный инсулин
        /// </summary>
        public decimal Basal
        {
            get => basal;
            set
            {
                if (value != basal)
                {
                    basal = value;
                    OnPropertyChanged();
                }
            }
        }

        private decimal bolus;
        /// <summary>
        /// Болюсный инсулин
        /// </summary>
        public decimal Bolus
        {
            get => bolus;
            set
            {
                if (value != bolus)
                {
                    bolus = value;
                    OnPropertyChanged();
                }
            }
        }

        private int catheter;
        /// <summary>
        /// Катетеры
        /// </summary>
        public int Catheter
        {
            get => catheter;
            set
            {
                if (value != catheter)
                {
                    catheter = value;
                    OnPropertyChanged();
                }
            }
        }

        private int cannula;
        /// <summary>
        /// Канюли
        /// </summary>
        public int Cannula
        {
            get => cannula;
            set
            {
                if (value != cannula)
                {
                    cannula = value;
                    OnPropertyChanged();
                }
            }
        }

        private int cartridge;
        /// <summary>
        /// Картриджи
        /// </summary>
        public int Cartridge
        {
            get => cartridge;
            set
            {
                if (value != cartridge)
                {
                    cartridge = value;
                    OnPropertyChanged();
                }
            }
        }

        private int needle;
        /// <summary>
        /// Иглы
        /// </summary>
        public int Needle
        {
            get => needle;
            set
            {
                if (value != needle)
                {
                    needle = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime monitoringRecomendationDate;
        /// <summary>
        /// Рекомендованная дата установки сенсора
        /// </summary>
        public DateTime MonitoringRecomendationDate
        {
            get => monitoringRecomendationDate;
            set
            {
                if (value != MonitoringRecomendationDate)
                {
                    monitoringRecomendationDate = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Collections

        /// <summary>
        /// История изменения расходных материалов
        /// </summary>
        public ObservableCollection<Models.ExpendableMaterial> History { get; private set; }

        /// <summary>
        /// Сроки "годности" (окончания)
        /// </summary>
        public ObservableCollection<ExpendableMaterialModel> ShelfLifes { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Инициализирует данные
        /// </summary>
        private void InitData()
        {
            using (var db = new ApplicationContext(GlobalParameters.DbPath))
            {
                // Комментарий
                Comment = GlobalParameters.User.Comment;

                // История изменений
                History = db.ExpendableMaterials
                    .AsNoTracking()
                    .Include(x => x.ExpendableMaterialType)
                    .ToList()
                    .OrderByDescending(x =>
                        x.DateCreated)
                    .ToObservable();
                OnPropertyChanged(nameof(History));

                // Итоговые значения
                var total = History
                    .GroupBy(x =>
                        x.ExpendableMaterialTypeId);

                Strip = (int)(total.FirstOrDefault(x => x.Key == 1)?.Sum(x => x.Count) ?? 0);
                Monitoring = (int)(total.FirstOrDefault(x => x.Key == 2)?.Sum(x => x.Count) ?? 0);
                Basal = total.FirstOrDefault(x => x.Key == 3)?.Sum(x => x.Count) ?? 0;
                Bolus = total.FirstOrDefault(x => x.Key == 4)?.Sum(x => x.Count) ?? 0;
                Catheter = (int)(total.FirstOrDefault(x => x.Key == 5)?.Sum(x => x.Count) ?? 0);
                Cannula = (int)(total.FirstOrDefault(x => x.Key == 6)?.Sum(x => x.Count) ?? 0);
                Cartridge = (int)(total.FirstOrDefault(x => x.Key == 7)?.Sum(x => x.Count) ?? 0);
                Needle = (int)(total.FirstOrDefault(x => x.Key == 8)?.Sum(x => x.Count) ?? 0);

                // Расход
                var lastMonth = DateTime.Now.AddMonths(-1).ToFileTimeUtc();

                var eatings = db.Eatings
                    .AsNoTracking()
                    .Where(x =>
                        x.FileTimeUtcDateCreated >= lastMonth &&
                        x.InsulinSensitivityFact != null)
                    .Include(x => x.EatingType)
                    .Include(x => x.Exercise)
                        .ThenInclude(x => x.ExerciseType)
                    .ToList();

                var types = db.ExpendableMaterialTypes
                    .AsNoTracking()
                    .ToList();

                // Базальный инсулин
                var basalPerDay = eatings
                    .GroupBy(x =>
                        x.DateCreated.Date)
                    .Select(x =>
                        x.Sum(y => y.BasalDose) +
                            (x.Where(y => y.BasalRate != 0).Count() > 0
                                ? Math.Round(x.Where(y => y.BasalRate != 0).Average(y => y.BasalRate) * 24, 1, MidpointRounding.AwayFromZero)
                                : 0));

                var avgBasalPerDay = basalPerDay.Count() > 0
                    ? basalPerDay.Average()
                    : 0;

                var basal = new ExpendableMaterialModel()
                {
                    Days = GlobalParameters.User.IsPump || avgBasalPerDay == 0
                        ? 0
                        : Basal / avgBasalPerDay,

                    MaterialType = types
                        .FirstOrDefault(x =>
                            x.Id == 3),
                };

                // Катетеры
                var catheter = new ExpendableMaterialModel()
                {
                    Days = basal.Days + Catheter * GlobalParameters.Settings.CatheterLifespan,
                    MaterialType = types
                        .FirstOrDefault(x =>
                            x.Id == 5),
                };

                // Канюли
                var cannula = new ExpendableMaterialModel()
                {
                    Days = basal.Days + Cannula * GlobalParameters.Settings.CannulaLifespan,
                    MaterialType = types
                        .FirstOrDefault(x =>
                            x.Id == 6),
                };

                // Картриджи
                var cartridge = new ExpendableMaterialModel()
                {
                    Days = basal.Days + Cartridge * GlobalParameters.Settings.CartridgeLifespan,
                    MaterialType = types
                        .FirstOrDefault(x =>
                            x.Id == 7),
                };

                // Иглы
                var needle = new ExpendableMaterialModel()
                {
                    Days = GlobalParameters.User.IsPump
                        ? 0
                        : Needle / 2,

                    MaterialType = types
                        .FirstOrDefault(x =>
                            x.Id == 8),
                };

                // Болюс
                var bolusPerDay = eatings
                    .GroupBy(x =>
                        x.DateCreated.Date)
                    .Select(x =>
                        x.Sum(y =>
                            y.BolusDoseFact +
                            (y.Injections?.Sum(z => z.BolusDose) ?? 0)));

                var avgBolusPerDay = bolusPerDay.Count() > 0
                    ? bolusPerDay.Average()
                    : 0;

                var bolus = new ExpendableMaterialModel()
                {
                    Days = avgBolusPerDay == 0 || avgBasalPerDay == 0
                        ? 0
                        : (Bolus / avgBolusPerDay < Basal / avgBasalPerDay) || Cannula == 0
                            ? Bolus / avgBolusPerDay
                            : (Bolus - Basal * avgBolusPerDay / avgBasalPerDay) / (avgBasalPerDay + avgBolusPerDay),

                    MaterialType = types
                        .FirstOrDefault(x =>
                            x.Id == 4),
                };

                // Сенсор
                var lastMonitoring = eatings
                    .Where(x =>
                        x.IsMonitoringReplacement)
                    .OrderByDescending(x =>
                        x.DateCreated.Date)
                    .FirstOrDefault();

                var daysLeft = lastMonitoring != null
                    ? GlobalParameters.Settings.MonitoringLifespan - (DateTime.Now.Date - lastMonitoring.DateCreated.Date).TotalDays
                    : 0;

                if (daysLeft < 0)
                    daysLeft = 0;

                var monitoring = new ExpendableMaterialModel()
                {
                    Days = Monitoring * GlobalParameters.Settings.MonitoringLifespan + (decimal)daysLeft,

                    MaterialType = types
                        .FirstOrDefault(x =>
                            x.Id == 2),
                };

                // Тест-полоски
                var strip = new ExpendableMaterialModel()
                {
                    Days = Strip - 2 * monitoring.Days < 0
                        ? Strip / 2
                        : (Strip - 2 * monitoring.Days) / 9 + monitoring.Days,

                    MaterialType = types
                        .FirstOrDefault(x =>
                            x.Id == 1),
                };

                // Рекомендация по установке сенсора
                MonitoringRecomendationDate = Strip - 2 * monitoring.Days < 0
                    ? DateTime.Now.Date
                    : DateTime.Now.Date.AddDays((double)(Strip - 2 * monitoring.Days) / 9);

                ShelfLifes = new ObservableCollection<ExpendableMaterialModel>()
                {
                    strip,
                    monitoring,
                    basal,
                    bolus,
                    catheter,
                    cannula,
                    cartridge,
                    needle
                };

                ShelfLifes = ShelfLifes
                    .Where(x =>
                        x.Days > 0)
                    .OrderBy(x =>
                        x.DateTime)
                    .ToObservable();

                OnPropertyChanged(nameof(ShelfLifes));
            }
        }

        #endregion

        #region Command

        #region --Save

        private async void SaveExecute()
        {
            try
            {
                bool question = await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                   "Сохранить изменения?",
                   "Вы уверены, что хотите сохранить изменения",
                   "Да",
                   "Нет");

                if (!question)
                    return;

                // Old
                var total = History
                    .GroupBy(x =>
                        x.ExpendableMaterialTypeId);

                var strip = (int)(total.FirstOrDefault(x => x.Key == 1)?.Sum(x => x.Count) ?? 0);
                var monitoring = (int)(total.FirstOrDefault(x => x.Key == 2)?.Sum(x => x.Count) ?? 0);
                var basal = total.FirstOrDefault(x => x.Key == 3)?.Sum(x => x.Count) ?? 0;
                var bolus = total.FirstOrDefault(x => x.Key == 4)?.Sum(x => x.Count) ?? 0;
                var catheter = (int)(total.FirstOrDefault(x => x.Key == 5)?.Sum(x => x.Count) ?? 0);
                var cannula = (int)(total.FirstOrDefault(x => x.Key == 6)?.Sum(x => x.Count) ?? 0);
                var cartridge = (int)(total.FirstOrDefault(x => x.Key == 7)?.Sum(x => x.Count) ?? 0);
                var needle = (int)(total.FirstOrDefault(x => x.Key == 8)?.Sum(x => x.Count) ?? 0);

                var date = DateTime.Now;

                using (var db = new ApplicationContext(GlobalParameters.DbPath))
                {
                    if (Strip != strip)
                        db.ExpendableMaterials.Add(new Models.ExpendableMaterial()
                        {
                            Id = Guid.NewGuid(),
                            ExpendableMaterialTypeId = 1,
                            Count = Strip - strip,
                            ChangeType = 1,
                            DateCreated = date
                        });

                    if (Monitoring != monitoring)
                        db.ExpendableMaterials.Add(new Models.ExpendableMaterial()
                        {
                            Id = Guid.NewGuid(),
                            ExpendableMaterialTypeId = 2,
                            Count = Monitoring - monitoring,
                            ChangeType = 1,
                            DateCreated = date
                        });

                    if (Basal != basal)
                        db.ExpendableMaterials.Add(new Models.ExpendableMaterial()
                        {
                            Id = Guid.NewGuid(),
                            ExpendableMaterialTypeId = 3,
                            Count = Basal - basal,
                            ChangeType = 1,
                            DateCreated = date
                        });

                    if (Bolus != bolus)
                        db.ExpendableMaterials.Add(new Models.ExpendableMaterial()
                        {
                            Id = Guid.NewGuid(),
                            ExpendableMaterialTypeId = 4,
                            Count = Bolus - bolus,
                            ChangeType = 1,
                            DateCreated = date
                        });

                    if (Catheter != catheter)
                        db.ExpendableMaterials.Add(new Models.ExpendableMaterial()
                        {
                            Id = Guid.NewGuid(),
                            ExpendableMaterialTypeId = 5,
                            Count = Catheter - catheter,
                            ChangeType = 1,
                            DateCreated = date
                        });

                    if (Cannula != cannula)
                        db.ExpendableMaterials.Add(new Models.ExpendableMaterial()
                        {
                            Id = Guid.NewGuid(),
                            ExpendableMaterialTypeId = 6,
                            Count = Cannula - cannula,
                            ChangeType = 1,
                            DateCreated = date
                        });

                    if (Cartridge != cartridge)
                        db.ExpendableMaterials.Add(new Models.ExpendableMaterial()
                        {
                            Id = Guid.NewGuid(),
                            ExpendableMaterialTypeId = 7,
                            Count = Cartridge - cartridge,
                            ChangeType = 1,
                            DateCreated = date
                        });

                    if (Needle != needle)
                        db.ExpendableMaterials.Add(new Models.ExpendableMaterial()
                        {
                            Id = Guid.NewGuid(),
                            ExpendableMaterialTypeId = 8,
                            Count = Needle - needle,
                            ChangeType = 1,
                            DateCreated = date
                        });

                    // Комментарий
                    db.Users.Find(GlobalParameters.User.Id).Comment = Comment;
                    db.SaveChanges();

                    GlobalParameters.User = db.Users
                        .Include(x => x.BasalType)
                        .Include(x => x.BolusType)
                        .FirstOrDefault();
                    InitData();
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

        public ICommand SaveCommand =>
            new Command(SaveExecute);

        #endregion

        #endregion
    }
}
