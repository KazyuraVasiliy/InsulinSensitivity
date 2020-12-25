using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models
{
    public class Exercise
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Тип нагрузки
        /// </summary>
        public Guid ExerciseTypeId { get; set; }

        /// <summary>
        /// Тип нагрузки (Навигационное свойство)
        /// </summary>
        public ExerciseType ExerciseType { get; set; }

        /// <summary>
        /// Продолжительность в минутах
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Количество часов после инъекции
        /// </summary>
        public int HoursAfterInjection { get; set; }

        /// <summary>
        /// Приёмы пищи (Навигационное свойство)
        /// </summary>
        public List<Eating> Eatings { get; set; }
    }
}