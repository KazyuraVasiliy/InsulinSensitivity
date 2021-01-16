using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.Models
{
    public class MenstrualCycle
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Дата начала цикла
        /// </summary>
        public DateTime DateStart { get; set; }

        /// <summary>
        /// Пользователь
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Пользователь (Навигационное свойство)
        /// </summary>
        public User User { get; set; }
    }
}
