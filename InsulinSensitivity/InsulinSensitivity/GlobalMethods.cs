using System;
using System.Collections.Generic;
using System.Text;

using DataAccessLayer.Contexts;
using BusinessLogicLayer.ViewModel;
using BusinessLogicLayer.Service;
using Models = DataAccessLayer.Models;
using Xamarin.Forms;
using System.Linq;

namespace InsulinSensitivity
{
    public static class GlobalMethods
    {
        /// <summary>
        /// Возвращает активный инсулин
        /// </summary>
        /// <param name="eating">Приём пищи</param>
        /// <returns></returns>
        public static decimal GetActiveInsulin(Models.Eating eating, IEnumerable<Models.Injection> injections = null)
        {
            decimal activeInsulin = Math.Round(eating.BolusDoseFact * (decimal)Calculation.GetActiveInsulinPercent(
                    Calculation.DateTimeUnionTimeSpan(eating.DateCreated, eating.InjectionTime), DateTime.Now, (int)GlobalParameters.User.BolusType.Duration), 2, MidpointRounding.AwayFromZero);

            foreach (var injection in (injections ?? eating.Injections))
            {
                activeInsulin += Math.Round(injection.BolusDose * (decimal)Calculation.GetActiveInsulinPercent(
                    Calculation.DateTimeUnionTimeSpan(injection.InjectionDate, injection.InjectionTime), DateTime.Now, (int)GlobalParameters.User.BolusType.Duration), 2, MidpointRounding.AwayFromZero);
            }

            return activeInsulin;
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
