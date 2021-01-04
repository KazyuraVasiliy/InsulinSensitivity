using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace InsulinSensitivity
{
    public class MainPageMasterItemModel : BindableObject
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="icon">Иконка</param>
        /// <param name="name">Наименование</param>
        /// <param name="command">Команда</param>
        /// <param name="isSeparator">Отображает границу снизу</param>
        public MainPageMasterItemModel(string icon, string name, ICommand command, bool isSeparator = false)
        {
            Icon = icon;
            Name = name;
            Command = command;
            IsSeparator = isSeparator;
        }

        /// <summary>
        /// Иконка
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Отображает границу снизу
        /// </summary>
        public bool IsSeparator { get; set; }

        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create("Command", typeof(ICommand), typeof(MainPageMasterItemModel), null);

        /// <summary>
        /// Команда
        /// </summary>
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
    }
}
