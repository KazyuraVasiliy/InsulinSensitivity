using System;

namespace DataAccessLayer.Models
{
    public class ExpendableMaterial : IEquatable<ExpendableMaterial>
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Тип расходных материалов
        /// </summary>
        public int ExpendableMaterialTypeId { get; set; }

        /// <summary>
        /// Количество
        /// </summary>
        public decimal Count { get; set; }

        /// <summary>
        /// Причина изменения
        /// </summary>
        public int ChangeType { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Тип расходных материалов (Навигационное свойство)
        /// </summary>
        public virtual ExpendableMaterialType ExpendableMaterialType { get; set; }

        /// <summary>
        /// Сравнивает экземпляры
        /// </summary>
        /// <param name="other">Сущность, с которой идёт сравнение</param>
        /// <returns></returns>
        public bool Equals(ExpendableMaterial other) =>
            other == null
                ? false
                : Id == other.Id;
    }
}