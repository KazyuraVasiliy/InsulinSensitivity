using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Service.Models
{
    public class InjectionToString
    {
        /// <summary>
        /// Доза
        /// </summary>
        public decimal Dose { get; set; }

        /// <summary>
        /// Задержка
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Наименование инсулина
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Строковое представление
        /// </summary>
        /// <returns></returns>
        public override string ToString() =>
            $"{Dose}{(Offset != 0 ? $" ({Offset} мин)" : "")}";
    }
}
