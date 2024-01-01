using System;

namespace BusinessLogicLayer.Attributes
{
    /// <summary>
    /// Свойство модели
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ModelAttribute : Attribute
    {
        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="name">Наименование модели</param>
        public ModelAttribute(string name = "") =>
            Name = name;
    }
}
