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
        /// Дата
        /// </summary>
        public DateTimeOffset dateString { get; set; }
    }
}
