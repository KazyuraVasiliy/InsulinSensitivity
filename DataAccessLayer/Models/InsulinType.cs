using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models
{
    public class InsulinType
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
        /// Время работы инусулина
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Пользователи с базальным инсулином (Навигационное свойство)
        /// </summary>
        public List<Eating> BasalUsers { get; set; }

        /// <summary>
        /// Пользователи с юолюсным инсулином (Навигационное свойство)
        /// </summary>
        public List<Eating> BolusUsers { get; set; }
    }
}