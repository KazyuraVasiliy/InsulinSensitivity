using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Service.Models
{
    public class Injection
    {
        /// <summary>
        /// Время инъекции
        /// </summary>
        public DateTime InjectionTime { get; set; }

        /// <summary>
        /// Начало действия
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// Окончание действия
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        /// Доза
        /// </summary>
        public decimal Dose { get; set; }

        /// <summary>
        /// Длительность
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Это базальный инсулин?
        /// </summary>
        public bool IsBasal { get; set; }

        /// <summary>
        /// Наименование инсулина
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Профиль
        /// </summary>
        public int Profile { get; set; }
    }
}
