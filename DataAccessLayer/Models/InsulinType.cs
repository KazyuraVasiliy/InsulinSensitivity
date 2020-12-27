using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models
{
    public class InsulinType : IEquatable<InsulinType>
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
        /// База?
        /// </summary>
        public bool IsBasal { get; set; }

        /// <summary>
        /// Время работы инусулина в часах
        /// </summary>
        public decimal Duration { get; set; }

        /// <summary>
        /// Пользователи с базальным инсулином (Навигационное свойство)
        /// </summary>
        public List<Eating> BasalUsers { get; set; }

        /// <summary>
        /// Пользователи с юолюсным инсулином (Навигационное свойство)
        /// </summary>
        public List<Eating> BolusUsers { get; set; }

        /// <summary>
        /// Сравнивает экземпляры
        /// </summary>
        /// <param name="other">Сущность, с которой идёт сравнение</param>
        /// <returns></returns>
        public bool Equals(InsulinType other) =>
            other == null
            ? false
            : Id == other.Id;
    }
}