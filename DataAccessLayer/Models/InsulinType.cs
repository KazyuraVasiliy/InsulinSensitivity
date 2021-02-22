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
        /// Смещение, через которое начинает действовать инсулин после инъекции (минут)
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Пользователи с базальным инсулином (Навигационное свойство)
        /// </summary>
        public List<User> BasalUsers { get; set; }

        /// <summary>
        /// Пользователи с болюсным инсулином (Навигационное свойство)
        /// </summary>
        public List<User> BolusUsers { get; set; }

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