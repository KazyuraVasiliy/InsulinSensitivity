using System;

namespace BusinessLogicLayer.Service.Models
{
    public class NightscoutTreatment
    {
        /// <summary>
        /// Доза инсулина
        /// </summary>
        public decimal insulin { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTimeOffset created_at { get; set; }
    }
}
