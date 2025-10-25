using System;

namespace DataAccessLayer.Models
{
    public class Pregnancy
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Дата начала беременности
        /// </summary>
        public DateTime DateStart { get; set; }

        /// <summary>
        /// Дата родов
        /// </summary>
        public DateTime? DateEnd { get; set; }

        /// <summary>
        /// Открытая беременность
        /// </summary>
        public bool IsOpen { get; set; }

        /// <summary>
        /// Пользователь
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Пользователь (Навигационное свойство)
        /// </summary>
        public virtual User User { get; set; }
    }
}
