using System;

namespace BusinessLogicLayer.Service.Models
{
    public class NightscoutTreatment
    {
        /// <summary>
        /// Доза инсулина
        /// </summary>
        public decimal? insulin { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTimeOffset created_at { get; set; }

        /// <summary>
        /// БС
        /// </summary>
        public decimal? rate { get; set; }

        /// <summary>
        /// ВБС
        /// </summary>
        public decimal? percent { get; set; }

        /// <summary>
        /// Продолжительность в минутах
        /// </summary>
        public decimal? duration { get; set; }

        /// <summary>
        /// Данные профиля
        /// </summary>
        public BolusCalc boluscalc { get; set; }

        public class BolusCalc
        {
            /// <summary>
            /// Профиль
            /// </summary>
            public string profile { get; set; }
        }
    }
}
