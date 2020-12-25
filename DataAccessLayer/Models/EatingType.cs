using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models
{
    public class EatingType : IEquatable<EatingType>
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
        /// Время начала
        /// </summary>
        public TimeSpan TimeStart { get; set; }

        /// <summary>
        /// Время окончания
        /// </summary>
        public TimeSpan TimeEnd { get; set; }

        /// <summary>
        /// База?
        /// </summary>
        public bool IsBasal { get; set; }

        /// <summary>
        /// Приёмы пищи (Навигационное свойство)
        /// </summary>
        public List<Eating> Eatings { get; set; }

        /// <summary>
        /// Сравнивает экземпляры
        /// </summary>
        /// <param name="other">Сущность, с которой идёт сравнение</param>
        /// <returns></returns>
        public bool Equals(EatingType other) =>
            other == null
            ? false
            : Id == other.Id;
    }
}