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
            set
            {
                bool question = GlobalParameters.Navigation.NavigationStack.Last().DisplayAlert(
                    "Запрос",
                    "Пересчитать пищевые коэффициенты при переходе на новую формулу расчёта?",
                    "Да",
                    "Нет").Result;

                if (question)
                {
                    using (var db = new ApplicationContext(GlobalParameters.DbPath))
                    {
                        var entity = db.Users.Find(GlobalParameters.User.Id);
                        if (entity != null)
                        {
                            if (value)
                            {
                                entity.CarbohydrateCoefficient *= 1.15M;
                                entity.ProteinCoefficient /= 1.15M;
                                entity.FatCoefficient /= 1.15M;
                            }
                            else
                            {
                                entity.CarbohydrateCoefficient /= 1.15M;
                                entity.ProteinCoefficient *= 1.15M;
                                entity.FatCoefficient *= 1.15M;
                            }

                            db.SaveChanges();
                        }
                    }
                }

                GlobalParameters.Settings.IsActiveBasal = value;
                OnPropertyChanged();
            }
        }

        #endregion
    }
}
