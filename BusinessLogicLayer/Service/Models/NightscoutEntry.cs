using System;

namespace BusinessLogicLayer.Service.Models
{
    public class NightscoutEntry
    {
        /// <summary>
        /// Глюкоза
        /// </summary>
        public decimal sgv { get; set; }

        /// <summary>
        /// Дата (new)
        /// </summary>
        public DateTimeOffset? created_at { get; set; }

        /// <summary>
        /// Дата (old)
        /// </summary>
        public DateTimeOffset? dateString { get; set; }
    }

    public class NightscoutBasal
    {
        /// <summary>
        /// Время
        /// </summary>
        public TimeSpan time { get; set; }

        /// <summary>
        /// Значение
        /// </summary>
        public decimal value { get; set; }
    }
}
