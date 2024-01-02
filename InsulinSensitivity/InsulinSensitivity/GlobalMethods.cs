using System;
using System.Collections.Generic;
using System.Text;

using DataAccessLayer.Contexts;
using BusinessLogicLayer.ViewModel;
using BusinessLogicLayer.Service;
using BusinessLogicLayer.Service.Models;
using Models = DataAccessLayer.Models;
using Xamarin.Forms;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace InsulinSensitivity
{
    public static class GlobalMethods
    {
        /// <summary>
        /// Возвращает активный инсулин
        /// </summary>
        /// <param name="currentEating">Текущий приём пищи</param>
        /// <param name="currentInjections">Текущие инъекции</param>
        /// <param name="beginPeriod">Начало приёма пищи</param>
        /// <param name="endPeriod">Предполагаемое время отработки</param>
        /// <param name="excludeId">Идентификатор приёма пищи, который необходимо исключить</param>
        /// <param name="isOnlyStart">Учитывать только те инъекции, которые были поставлены до начала приёма пищи</param>
        /// <param name="carbohydrate">Углеводы</param>
        /// <param name="pause">Пауза</param>
        /// <param name="selectedEatings">Приёмы пищи, на основании которых происходит расчёт</param>
        /// <returns></returns>
        public static (decimal insulin, List<string> informations) GetActiveInsulin(
            // Текущий приём пищи
            Models.Eating currentEating = null, 
            IEnumerable<Models.Injection> currentInjections = null,
            // Период
            DateTime? beginPeriod = null, 
            DateTime? endPeriod = null,
            // Исключённый приём (при изменении текущего)
            Guid? excludeId = null,
            // Инъекции только на начало приёма (для расчётной дозы)
            bool isOnlyStart = false,
            // Углеводы, Пауза (для расчёта время действия базы)
            int? carbohydrate = null,
            int? pause = null,
            // Приёмы пищи
            List<Models.Eating> selectedEatings = null)
        {
            (decimal insulin, List<string> informations) result =
                (0, new List<string>());

            // Определение даты от которой будет расчитан активный инсулин
            // Самая большая длительность 48 часов (2 дня) + день для точности
            var period = beginPeriod == null
                ? DateTime.Now.Date.AddDays(-3)
                : beginPeriod.Value.Date.AddDays(-3);

            // Получение приёмов пищи
            var eatings = new List<Models.Eating>();
            if ((selectedEatings?.Count ?? 0) > 0)
                eatings.AddRange(selectedEatings);

            if ((eatings?.Count ?? 0) == 0)
            {
                using (var db = new ApplicationContext(GlobalParameters.DbPath))
                {
                    var utcPeriod = period.ToFileTimeUtc();
                    eatings = db.Eatings
                        .Where(x =>
                            x.FileTimeUtcDateCreated >= utcPeriod &&
                            x.Id != excludeId)
                        .Include(x => x.Injections)
                            .ThenInclude(x => x.BolusType)
                        .Include(x => x.BolusType)
                        .Include(x => x.BasalType)
                        .AsNoTracking()
                        .ToList();
                }
            }

            eatings = eatings
                .Where(x =>
                    x.DateCreated.Date >= period.Date &&
                    x.Id != excludeId)
                .ToList();

            if (currentEating != null)
                eatings.Add(new Models.Eating()
                {
                    DateCreated = currentEating.DateCreated,
                    InjectionTime = currentEating.InjectionTime,
                    BolusDoseFact = currentEating.BolusDoseFact,
                    BolusType = currentEating.BolusType,

                    BasalDose = currentEating.BasalDose,
                    BasalInjectionTime = currentEating.BasalInjectionTime,
                    BasalType = currentEating.BasalType,

                    Injections = currentInjections?.ToList() ?? new List<Models.Injection>()
                });                

            // Запись всех инъекций в один массив для упрощения дальнейшего анализа
            List<Injection> injections = new List<Injection>();
            foreach (var eating in eatings)
            {
                var basalDuration = (int)(eating.BasalType?.Duration ?? GlobalParameters.User.BasalType.Duration);
                var bolusDuration = (int)(eating.BolusType?.Duration ?? GlobalParameters.User.BolusType.Duration);

                var basalOffset = eating.BasalType?.Offset ?? GlobalParameters.User.BasalType.Offset;
                var bolusOffset = eating.BolusType?.Offset ?? GlobalParameters.User.BolusType.Offset;

                // ... Базальный
                if (eating.BasalInjectionTime != null && eating.BasalDose != 0)
                    injections.Add(new Injection()
                    {
                        InjectionTime = eating.BasalInjectionTime.Value,
                        IsBasal = true,
                        Start = eating.BasalInjectionTime.Value.AddMinutes(basalOffset),
                        End = eating.BasalInjectionTime.Value.AddMinutes(basalOffset + basalDuration * 60),
                        Dose = eating.BasalDose,
                        Duration = basalDuration,
                        Name = eating.BasalType?.Name ?? ""
                    });

                // ... Основная инъекция
                var startInjection = Calculation.DateTimeUnionTimeSpan(eating.DateCreated, eating.InjectionTime);
                injections.Add(new Injection()
                {
                    InjectionTime = startInjection,
                    Start = startInjection.AddMinutes(bolusOffset),
                    End = startInjection.AddMinutes(bolusOffset + bolusDuration * 60),
                    Dose = eating.BolusDoseFact,
                    Duration = bolusDuration,
                    Profile = eating.BolusType?.Profile ?? 0,
                    Name = eating.BolusType?.Name ?? ""
                });

                // ... Подколки
                foreach (var injection in eating?.Injections ?? new List<Models.Injection>())
                {
                    bolusDuration = (int)(injection.BolusType?.Duration ?? GlobalParameters.User.BolusType.Duration);
                    bolusOffset = injection.BolusType?.Offset ?? GlobalParameters.User.BolusType.Offset;

                    startInjection = Calculation.DateTimeUnionTimeSpan(injection.InjectionDate, injection.InjectionTime);
                    injections.Add(new Injection()
                    {
                        InjectionTime = startInjection,
                        Start = startInjection.AddMinutes(bolusOffset),
                        End = startInjection.AddMinutes(bolusOffset + bolusDuration * 60),
                        Dose = injection.BolusDose,
                        Duration = bolusDuration,
                        Profile = injection.BolusType?.Profile ?? 0,
                        Name = injection.BolusType?.Name ?? ""
                    });
                }
            }

            beginPeriod = beginPeriod ?? DateTime.Now;

            DateTime? basalBeginPeriod = beginPeriod;
            DateTime? basalEndPeriod = endPeriod;

            if (pause != null && carbohydrate != null && basalBeginPeriod != null && basalEndPeriod != null)
            {
                basalBeginPeriod = basalBeginPeriod.Value.AddMinutes(pause.Value);
                basalEndPeriod = basalBeginPeriod.Value.AddHours(carbohydrate.Value / (double)GlobalParameters.User.AbsorptionRateOfCarbohydrates);

                if ((basalEndPeriod - basalBeginPeriod).Value.TotalHours > 2)
                    basalEndPeriod = basalBeginPeriod.Value.AddHours(2);
            }

            if (endPeriod == null)
            {
                foreach (var injection in injections.OrderBy(x => x.InjectionTime))
                {
                    if (injection.End <= beginPeriod || injection.Dose <= 0 || injection.Start >= beginPeriod || injection.IsBasal)
                        continue;

                    decimal value = Math.Round(injection.Dose * (decimal)Calculation.GetActiveInsulinPercent(injection.Start, beginPeriod.Value, injection.Duration, injection.Profile),
                        2, MidpointRounding.AwayFromZero);

                    result.insulin += value;
                    result.informations.Add($"— {value:N2} ед. болюса ({injection.Name})\n\tот {injection.InjectionTime:dd.MM HH:mm} ({injection.Dose:N2} ед.)");
                }
            }
            // Если активный инсулин рассчитывается на заданный промежуток, то необходимо вычислить, какая его часть попадёт в интервал
            else
            {
                foreach (var injection in injections.OrderBy(x => x.InjectionTime))
                {
                    DateTime? localBeginPeriod = injection.IsBasal
                        ? basalBeginPeriod
                        : beginPeriod;

                    DateTime? localEndPeriod = injection.IsBasal
                        ? basalEndPeriod
                        : endPeriod;

                    if (injection.End <= localBeginPeriod || injection.Dose <= 0 || injection.Start >= localEndPeriod)
                        continue;

                    if (isOnlyStart && injection.InjectionTime >= localBeginPeriod)
                        continue;

                    if (!GlobalParameters.User.IsActiveBasal && injection.IsBasal)
                        continue;

                    decimal value = 0;
                    var insulinPerHours = injection.Dose / injection.Duration;

                    // | -- Insulin -- | ---
                    // --- | -- Eating --| 
                    if (injection.Start <= localBeginPeriod.Value && injection.End <= localEndPeriod.Value)
                    {
                        value = injection.Duration <= 12
                            ? injection.Dose * (decimal)Calculation.GetActiveInsulinPercent(injection.Start, localBeginPeriod.Value, injection.Duration, injection.Profile)
                            : (decimal)(injection.End - localBeginPeriod.Value).TotalHours * insulinPerHours;
                    }

                    // | ---- Insulin ---- |
                    // -- | - Eating - | --
                    else if (injection.Start <= localBeginPeriod.Value && injection.End > localEndPeriod.Value)
                    {
                        value = injection.Duration <= 12
                            ? injection.Dose * (decimal)Calculation.GetActiveInsulinPercent(injection.Start, localBeginPeriod.Value, injection.Duration, injection.Profile) -
                                (injection.Dose * (decimal)Calculation.GetActiveInsulinPercent(injection.Start, localEndPeriod.Value, injection.Duration, injection.Profile))
                            : (decimal)(localEndPeriod.Value - localBeginPeriod.Value).TotalHours * insulinPerHours;
                    }

                    // -- | - Insulin - | --
                    // | ---- Eating ---- |
                    else if (injection.Start > localBeginPeriod.Value && injection.End <= localEndPeriod.Value)
                        value = injection.Dose;

                    // --- | -- Insulin -- |
                    // | -- Eating -- | ---
                    else if (injection.Start > localBeginPeriod.Value && injection.End > localEndPeriod.Value)
                    {
                        value = injection.Duration <= 12
                            ? injection.Dose - (injection.Dose * (decimal)Calculation.GetActiveInsulinPercent(injection.Start, localEndPeriod.Value, injection.Duration, injection.Profile))
                            : (decimal)(localEndPeriod.Value - injection.Start).TotalHours * insulinPerHours;
                    }

                    value = Math.Round(value, 2, MidpointRounding.AwayFromZero);

                    result.insulin += value;
                    result.informations.Add($"— {value:N2} ед. {(injection.IsBasal ? "базы" : "болюса")} ({injection.Name})\n\tот {injection.InjectionTime:dd.MM HH:mm} ({injection.Dose:N2} ед.)");
                }

                if (currentEating.BasalRate != 0 && basalEndPeriod != null && beginPeriod != null)
                {
                    var value = (decimal)(basalEndPeriod.Value - beginPeriod.Value).TotalHours * currentEating.BasalRate / 100 * currentEating.BasalRateCoefficient;

                    result.insulin += Math.Round(value, 2, MidpointRounding.AwayFromZero);
                    result.informations.Add($"— {value:N2} ед. базы (БС на приём пищи)");
                }
            }

            return result;
        }

        /// <summary>
        /// Отображает подсказку
        /// </summary>
        public static Command ToolTipCommand =>
            new Command(async (object obj) =>
            {
                await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Информация",
                    $"{(string)obj}",
                    "Ok");
            });

        /// <summary>
        /// Отображает запрос (Да / Нет)
        /// </summary>
        public static async Task<bool> AskAQuestion(string question) =>
            await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                "Запрос",
                question,
                "Да",
                "Нет");

        /// <summary>
        /// Отображает ошибку
        /// </summary>
        public static async Task ShowError(string error) =>
            await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                "Ошибка",
                error,
                "Ok");

        /// <summary>
        /// Расчёт активного инсулина для виджета
        /// </summary>
        /// <param name="selectedEatings">Приёмы пищи</param>
        /// <param name="user">Текущий пользователь</param>
        /// <returns>Активный инсулин</returns>
        public static decimal GetActiveInsulinForWidget(List<Models.Eating> selectedEatings, Models.User user)
        {
            decimal insulin = 0;

            // Получение приёмов пищи
            var eatings = new List<DataAccessLayer.Models.Eating>();
            if ((selectedEatings?.Count ?? 0) > 0)
                eatings.AddRange(selectedEatings);

            // Запись всех инъекций в один массив для упрощения дальнейшего анализа
            var injections = new List<BusinessLogicLayer.Service.Models.Injection>();
            foreach (var eating in eatings)
            {
                var basalDuration = (int)(eating.BasalType?.Duration ?? user.BasalType.Duration);
                var bolusDuration = (int)(eating.BolusType?.Duration ?? user.BolusType.Duration);

                var basalOffset = eating.BasalType?.Offset ?? user.BasalType.Offset;
                var bolusOffset = eating.BolusType?.Offset ?? user.BolusType.Offset;

                // ... Базальный
                if (eating.BasalInjectionTime != null && eating.BasalDose != 0)
                    injections.Add(new BusinessLogicLayer.Service.Models.Injection()
                    {
                        InjectionTime = eating.BasalInjectionTime.Value,
                        IsBasal = true,
                        Start = eating.BasalInjectionTime.Value.AddMinutes(basalOffset),
                        End = eating.BasalInjectionTime.Value.AddMinutes(basalOffset + basalDuration * 60),
                        Dose = eating.BasalDose,
                        Duration = basalDuration,
                        Name = eating.BasalType?.Name ?? ""
                    });

                // ... Основная инъекция
                var startInjection = BusinessLogicLayer.Service.Calculation.DateTimeUnionTimeSpan(eating.DateCreated, eating.InjectionTime);
                injections.Add(new BusinessLogicLayer.Service.Models.Injection()
                {
                    InjectionTime = startInjection,
                    Start = startInjection.AddMinutes(bolusOffset),
                    End = startInjection.AddMinutes(bolusOffset + bolusDuration * 60),
                    Dose = eating.BolusDoseFact,
                    Duration = bolusDuration,
                    Profile = eating.BolusType?.Profile ?? 0,
                    Name = eating.BolusType?.Name ?? ""
                });

                // ... Подколки
                foreach (var injection in eating?.Injections ?? new List<DataAccessLayer.Models.Injection>())
                {
                    bolusDuration = (int)(injection.BolusType?.Duration ?? user.BolusType.Duration);
                    bolusOffset = injection.BolusType?.Offset ?? user.BolusType.Offset;

                    startInjection = BusinessLogicLayer.Service.Calculation.DateTimeUnionTimeSpan(injection.InjectionDate, injection.InjectionTime);
                    injections.Add(new BusinessLogicLayer.Service.Models.Injection()
                    {
                        InjectionTime = startInjection,
                        Start = startInjection.AddMinutes(bolusOffset),
                        End = startInjection.AddMinutes(bolusOffset + bolusDuration * 60),
                        Dose = injection.BolusDose,
                        Duration = bolusDuration,
                        Profile = injection.BolusType?.Profile ?? 0,
                        Name = injection.BolusType?.Name ?? ""
                    });
                }
            }

            var beginPeriod = DateTime.Now;

            foreach (var injection in injections.OrderBy(x => x.InjectionTime))
            {
                if (injection.End <= beginPeriod || injection.Dose <= 0 || injection.Start >= beginPeriod || injection.IsBasal)
                    continue;

                decimal value = Math.Round(injection.Dose * (decimal)BusinessLogicLayer.Service.Calculation.GetActiveInsulinPercent(injection.Start, beginPeriod, injection.Duration, injection.Profile),
                    2, MidpointRounding.AwayFromZero);

                insulin += value;
            }

            return Math.Round(insulin, 2);
        }
    }
}
