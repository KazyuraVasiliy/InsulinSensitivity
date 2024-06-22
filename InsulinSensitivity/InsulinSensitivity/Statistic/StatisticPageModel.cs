using System;

namespace InsulinSensitivity.Statistic
{
    public class BasalDose
    {
        /// <summary>
        /// Дата
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Доза
        /// </summary>
        public decimal Dose { get; set; }
    }
}
