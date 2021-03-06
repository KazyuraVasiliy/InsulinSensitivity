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
            int? pause = null)
        {
            using (var db = new ApplicationContext(GlobalParameters.DbPath))
            {
                (decimal insulin, List<string> informations) result =
                    (0, new List<string>());

                // Самая большая длительность 48 часов (2 дня) + день для точности
                var period = DateTime.Now.Date.AddDays(-3);
                var eatings = db.Eatings
                    .Include(x => x.Injections)
                        .ThenInclude(x => x.BolusType)
                    .Include(x => x.BolusType)
                    .Include(x => x.BasalType)
                    .ToList()
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
                }

                if (endPeriod == null)
                {
                    foreach (var injection in injections.OrderBy(x => x.InjectionTime))
                    {
                        if (injection.End <= beginPeriod || injection.Dose <= 0 || injection.Start >= beginPeriod || injection.IsBasal)
                            continue;

                        decimal value = Math.Round(injection.Dose * (decimal)Calculation.GetActiveInsulinPercent(injection.Start, beginPeriod.Value, injection.Duration),
                            2, MidpointRounding.AwayFromZero);

                        result.insulin += value;
                        result.informations.Add($"— {value:N2} ед. болюса ({injection.Name})\n\tот {injection.InjectionTime:dd.MM HH:mm} ({injection.Dose:N2} ед.)");
                    }
                }
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

                        if (!GlobalParameters.Settings.IsActiveBasal && injection.IsBasal)
                            continue;

                        decimal value = 0;
                        var insulinPerHours = injection.Dose / injection.Duration;

                        // | -- Insulin -- | ---
                        // --- | -- Eating --| 
                        if (injection.Start <= localBeginPeriod.Value && injection.End <= localEndPeriod.Value)
                        {
                            value = injection.Duration <= 12
                                ? injection.Dose * (decimal)Calculation.GetActiveInsulinPercent(injection.Start, localBeginPeriod.Value, injection.Duration)
                                : (decimal)(injection.End - localBeginPeriod.Value).TotalHours * insulinPerHours;
                        }

                        // | ---- Insulin ---- |
                        // -- | - Eating - | --
                        else if (injection.Start <= localBeginPeriod.Value && injection.End > localEndPeriod.Value)
                        {
                            value = injection.Duration <= 12
                                ? injection.Dose * (decimal)Calculation.GetActiveInsulinPercent(injection.Start, localBeginPeriod.Value, injection.Duration) -
                                    (injection.Dose * (decimal)Calculation.GetActiveInsulinPercent(injection.Start, localEndPeriod.Value, injection.Duration))
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
                                ? injection.Dose - (injection.Dose * (decimal)Calculation.GetActiveInsulinPercent(injection.Start, localEndPeriod.Value, injection.Duration))
                                : (decimal)(localEndPeriod.Value - injection.Start).TotalHours * insulinPerHours;
                        }

                        value = Math.Round(value, 2, MidpointRounding.AwayFromZero);

                        result.insulin += value;
                        result.informations.Add($"— {value:N2} ед. {(injection.IsBasal ? "базы" : "болюса")} ({injection.Name})\n\tот {injection.InjectionTime:dd.MM HH:mm} ({injection.Dose:N2} ед.)");
                    }
                }

                return result;
            }
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
    }
}
