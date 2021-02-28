using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

using DataAccessLayer.Contexts;
using BusinessLogicLayer.ViewModel;
using BusinessLogicLayer.Service;
using Models = DataAccessLayer.Models;

namespace InsulinSensitivity.Settings
{
    public class SettingsPageViewModel : ObservableBase
    {
        #region Properties

        /// <summary>
        /// Учитывается ли активный базальный
        /// </summary>
        public bool IsActiveBasal
        {
            get => GlobalParameters.Settings.IsActiveBasal;
            set => _ = SetActiveBasal(value);
        }

        #endregion

        #region Methods

        private async Task SetActiveBasal(bool value)
        {
            if (value == GlobalParameters.Settings.IsActiveBasal)
                return;

            bool question = await GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                "Запрос",
                "Пересчитать пищевые коэффициенты при переходе на новую формулу расчёта?",
                "Да",
                "Нет");

            if (question)
                await AsyncBase.NewTask(() =>
                {
                    using (var db = new ApplicationContext(GlobalParameters.DbPath))
                    {
                        var entity = db.Users.Find(GlobalParameters.User.Id);
                        if (entity != null)
                        {
                            if (value)
                            {
                                entity.CarbohydrateCoefficient = Math.Round(entity.CarbohydrateCoefficient * 1.15M, 2, MidpointRounding.AwayFromZero);
                                entity.ProteinCoefficient = Math.Round(entity.ProteinCoefficient / 1.15M, 2, MidpointRounding.AwayFromZero);
                                entity.FatCoefficient = Math.Round(entity.FatCoefficient / 1.15M, 2, MidpointRounding.AwayFromZero);
                            }
                            else
                            {
                                entity.CarbohydrateCoefficient = Math.Round(entity.CarbohydrateCoefficient / 1.15M, 2, MidpointRounding.AwayFromZero);
                                entity.ProteinCoefficient = Math.Round(entity.ProteinCoefficient * 1.15M, 2, MidpointRounding.AwayFromZero);
                                entity.FatCoefficient = Math.Round(entity.FatCoefficient * 1.15M, 2, MidpointRounding.AwayFromZero);
                            }

                            db.SaveChanges();
                        }
                    }
                }, "Применение опции");

            GlobalParameters.Settings.IsActiveBasal = value;
            OnPropertyChanged(nameof(IsActiveBasal));
        }

        #endregion
    }
}
