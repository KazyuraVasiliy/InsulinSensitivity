using System;

namespace DataAccessLayer.Models
{
    public class Injection
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Время инъекции
        /// </summary>
        public TimeSpan InjectionTime { get; set; }

        /// <summary>
        /// Дата инъекции
        /// </summary>
        public DateTime InjectionDate { get; set; }

        /// <summary>
        /// Доза болюсного инсулина
        /// </summary>
        public decimal BolusDose { get; set; }

        /// <summary>
        /// Тип болюсного инсулина
        /// </summary>
        public Guid? BolusTypeId { get; set; }

        /// <summary>
        /// По данным ВБС
        /// </summary>
        public bool IsBasalRateCoefficient { get; set; }

        /// <summary>
        /// Тип болюсного инсулина (Навигационное свойство)
        /// </summary>
        public InsulinType BolusType { get; set; }

        /// <summary>
        /// Приём пищи
        /// </summary>
        public Guid EatingId { get; set; }

        /// <summary>
        /// Приём пищи (Навигационное свойство)
        /// </summary>
        public Eating Eating { get; set; }
    }
}
