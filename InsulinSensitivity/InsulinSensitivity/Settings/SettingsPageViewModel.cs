using System;
using System.Collections.Generic;
using System.Text;

using DataAccessLayer.Contexts;
using BusinessLogicLayer.ViewModel;
using BusinessLogicLayer.Service;
using Models = DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

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
                GlobalParameters.Settings.IsActiveBasal = value;
                OnPropertyChanged();
            }
        }

        #endregion
    }
}
