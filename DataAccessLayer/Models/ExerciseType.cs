using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models
{
    public class ExerciseType
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Нагрузки (Навигационное свойство)
        /// </summary>
        public List<Eating> Exercises { get; set; }
    }
}