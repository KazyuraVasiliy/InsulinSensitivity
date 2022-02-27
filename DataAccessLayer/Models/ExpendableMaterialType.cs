using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models
{
    public class ExpendableMaterialType : IEquatable<ExpendableMaterialType>
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Единицы измерения
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Расходные материалы (Навигационное свойство)
        /// </summary>
        public List<ExpendableMaterial> ExpendableMaterials { get; set; }

        /// <summary>
        /// Сравнивает экземпляры
        /// </summary>
        /// <param name="other">Сущность, с которой идёт сравнение</param>
        /// <returns></returns>
        public bool Equals(ExpendableMaterialType other) =>
            other == null
                ? false
                : Id == other.Id;
    }
}