using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Service.Models
{
    public class GlucoseToString
    {
        /// <summary>
        /// Доза
        /// </summary>
        public decimal Glucose { get; set; }

        /// <summary>
        /// Задержка
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Строковое представление
        /// </summary>
        /// <returns></returns>
        public override string ToString() =>
            $"{Glucose}{(Offset != 0 ? $" ({Offset} мин)" : "")}";
    }
}
