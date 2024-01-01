using System;

namespace DataAccessLayer.Models
{
    public class IntermediateDimension
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Время замера
        /// </summary>
        public TimeSpan DimensionTime { get; set; }

        /// <summary>
        /// Дата замера
        /// </summary>
        public DateTime DimensionDate { get; set; }

        /// <summary>
        /// Сахар
        /// </summary>
        public decimal Glucose { get; set; }

        /// <summary>
        /// Приём пищи
        /// </summary>
        public Guid EatingId { get; set; }

        /// <summary>
        /// Приём пищи (Навигационное свойство)
        /// </summary>
        public virtual Eating Eating { get; set; }
    }
}
