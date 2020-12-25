using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models
{
    public class ExerciseType : IEquatable<ExerciseType>
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
        /// Нагрузка по умолчанию
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Базовая нагрузка
        /// </summary>
        public bool IsEmpty { get; set; }

        /// <summary>
        /// Нагрузки (Навигационное свойство)
        /// </summary>
        public List<Exercise> Exercises { get; set; }

        /// <summary>
        /// Сравнивает экземпляры
        /// </summary>
        /// <param name="other">Сущность, с которой идёт сравнение</param>
        /// <returns></returns>
        public bool Equals(ExerciseType other) =>
            other == null
            ? false
            : Id == other.Id;
    }
}