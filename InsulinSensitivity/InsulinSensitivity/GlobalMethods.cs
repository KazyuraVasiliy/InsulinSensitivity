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
        /// <param name="averageInsulinSensitivity">Средний ФЧИ</param>
        /// <param name="averageBasal">Среднесуточная база</param>
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
            // Углеводы, Средний ФЧИ, Среднесуточная база (для расчёта коэффициента базы)
            int? carbohydrate = null,
            decimal? averageInsulinSensitivity = null,
            decimal? averageBasal = null)
        {
            using (var db = new ApplicationContext(GlobalParameters.DbPath))
            {
                (decimal insulin, List<string> informations) result =
                    (0, new List<string>());

                // Самая большая длительность 48 часов (2 дня) + день для точности
                var period = DateTime.Now.Date.AddDays(-3);
                var eatings = db.Eatings
                    .Include(x => x.Injections)
                    .ToList()
                    .Where(x =>
                        x.DateCreated.Date >= period.Date &&
                        x.Id != excludeId)
                    .ToList();

                decimal coefficient = 1;
                if (carbohydrate != null && averageInsulinSensitivity != null && averageBasal != null && beginPeriod != null && endPeriod != null && GlobalParameters.Settings.IsActiveBasal)
                {
                    var limit = averageBasal.Value / 24M * averageInsulinSensitivity.Value / GlobalParameters.User.CarbohydrateCoefficient * (decimal)(endPeriod - beginPeriod).Value.TotalHours;
                    if (carbohydrate.Value < limit)
                        coefficient = carbohydrate.Value / limit;
                }

                if (currentEating != null)
                    eatings.Add(new Models.Eating()
                    {
                        DateCreated = currentEating.DateCreated,
                        InjectionTime = currentEating.InjectionTime,
                        BolusDoseFact = currentEating.BolusDoseFact,

                        BasalDose = currentEating.BasalDose,
                        BasalInjectionTime = currentEating.BasalInjectionTime,

                        Injections = currentInjections?.ToList() ?? new List<Models.Injection>()
                    });

                var basalDuration = (int)GlobalParameters.User.BasalType.Duration;
                var bolusDuration = (int)GlobalParameters.User.BolusType.Duration;

                var basalOffset = GlobalParameters.User.BasalType.Offset;
                var bolusOffset = GlobalParameters.User.BolusType.Offset;

                List<Injection> injections = new List<Injection>();
                foreach (var eating in eatings)
                {
                    // ... Базальный
                    if (eating.BasalInjectionTime != null && eating.BasalDose != 0)
                        injections.Add(new Injection()
                        {
                            InjectionTime = eating.BasalInjectionTime.Value,
                            IsBasal = true,
                            Start = eating.BasalInjectionTime.Value.AddMinutes(basalOffset),
                            End = eating.BasalInjectionTime.Value.AddMinutes(basalOffset + basalDuration * 60),
                            Dose = eating.BasalDose,
                            Duration = basalDuration
                        });

                    // ... Основная инъекция
                    var startInjection = Calculation.DateTimeUnionTimeSpan(eating.DateCreated, eating.InjectionTime);
                    injections.Add(new Injection()
                    {
                        InjectionTime = startInjection,
                        Start = startInjection.AddMinutes(bolusOffset),
                        End = startInjection.AddMinutes(bolusOffset + bolusDuration * 60),
                        Dose = eating.BolusDoseFact,
                        Duration = bolusDuration
                    });

                    // ... Подколки
                    foreach (var injection in eating?.Injections ?? new List<Models.Injection>())
                    {
                        startInjection = Calculation.DateTimeUnionTimeSpan(injection.InjectionDate, injection.InjectionTime);
                        injections.Add(new Injection()
                        {
                            InjectionTime = startInjection,
                            Start = startInjection.AddMinutes(bolusOffset),
                            End = startInjection.AddMinutes(bolusOffset + bolusDuration * 60),
                            Dose = injection.BolusDose,
                            Duration = bolusDuration
                        });
                    }
                }

                beginPeriod = beginPeriod ?? DateTime.Now;

                if (endPeriod == null)
                {
                    foreach (var injection in injections.OrderBy(x => x.InjectionTime))
                    {
                        if (injection.End <= beginPeriod || injection.Dose <= 0 || injection.Start >= beginPeriod || injection.IsBasal)
                            continue;

                        decimal value = Math.Round(injection.Dose * (decimal)Calculation.GetActiveInsulinPercent(injection.Start, beginPeriod.Value, injection.Duration),
                            2, MidpointRounding.AwayFromZero);

                        result.insulin += value;
                        result.informations.Add($"— {value} ед. болюса\n\tот {injection.InjectionTime:dd.MM HH:mm} ({injection.Dose} ед.)");
                    }
                }
                else
                {
                    foreach (var injection in injections.OrderBy(x => x.InjectionTime))
                    {
                        if (injection.End <= beginPeriod || injection.Dose <= 0 || injection.Start >= endPeriod)
                            continue;

                        if (isOnlyStart && injection.InjectionTime >= beginPeriod)
                            continue;

                        if (!GlobalParameters.Settings.IsActiveBasal && injection.IsBasal)
                            continue;

                        decimal value = 0;
                        var insulinPerHours = injection.Dose / injection.Duration;

                        // | -- Insulin -- | ---
                        // --- | -- Eating --| 
                        if (injection.Start <= beginPeriod.Value && injection.End <= endPeriod.Value)
                        {
                            value = injection.Duration <= 12
                                ? injection.Dose * (decimal)Calculation.GetActiveInsulinPercent(injection.Start, beginPeriod.Value, injection.Duration)
                                : (decimal)(injection.End - beginPeriod.Value).TotalHours * insulinPerHours;
                        }

                        // | ---- Insulin ---- |
                        // -- | - Eating - | --
                        else if (injection.Start <= beginPeriod.Value && injection.End > endPeriod.Value)
                        {
                            value = injection.Duration <= 12
                                ? injection.Dose * (decimal)Calculation.GetActiveInsulinPercent(injection.Start, beginPeriod.Value, injection.Duration) -
                                    (injection.Dose * (decimal)Calculation.GetActiveInsulinPercent(injection.Start, endPeriod.Value, injection.Duration))
                                : (decimal)(endPeriod.Value - beginPeriod.Value).TotalHours * insulinPerHours;
                        }

                        // -- | - Insulin - | --
                        // | ---- Eating ---- |
                        else if (injection.Start > beginPeriod.Value && injection.End <= endPeriod.Value)
                            value = injection.Dose;

                        // --- | -- Insulin -- |
                        // | -- Eating -- | ---
                        else if (injection.Start > beginPeriod.Value && injection.End > endPeriod.Value)
                        {
                            value = injection.Duration <= 12
                                ? injection.Dose - (injection.Dose * (decimal)Calculation.GetActiveInsulinPercent(injection.Start, endPeriod.Value, injection.Duration))
                                : (decimal)(endPeriod.Value - injection.Start).TotalHours * insulinPerHours;
                        }

                        if (injection.IsBasal)
                            value *= coefficient;
                        value = Math.Round(value, 2, MidpointRounding.AwayFromZero);

                        result.insulin += value;
                        result.informations.Add($"— {value} ед. {(injection.IsBasal ? "базы" : "болюса")}\n\tот {injection.InjectionTime:dd.MM HH:mm} ({injection.Dose} ед.)");
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
