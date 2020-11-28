using System;

namespace DataAccessLayer.Models
{
    public class Eating
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Время инъекции
        /// </summary>
        public TimeSpan InjectionTime { get; set; }

        /// <summary>
        /// Исходный сахар
        /// </summary>
        public decimal GlucoseStart { get; set; }

        /// <summary>
        /// Сахар на отработке
        /// </summary>
        public decimal GlucoseEnd { get; set; }

        /// <summary>
        /// Количество активного инсулина в крови перед поставновкой инъекции
        /// </summary>
        public decimal ActiveInsulinStart { get; set; }

        /// <summary>
        /// Количество активного инсулина в крови на отработке
        /// </summary>
        public decimal ActiveInsulinEnd { get; set; }

        /// <summary>
        /// Углеводы
        /// </summary>
        public int Carbohydrate { get; set; }

        /// <summary>
        /// Белки
        /// </summary>
        public int Protein { get; set; }

        /// <summary>
        /// Жиры
        /// </summary>
        public int Fat { get; set; }

        /// <summary>
        /// Доза базального инсулина
        /// </summary>
        public decimal BasalDose { get; set; }

        /// <summary>
        /// Доза болюсного инсулина
        /// </summary>
        public decimal BolusDose { get; set; }

        /// <summary>
        /// ФЧИ вычисленный автоматически
        /// </summary>
        public decimal? InsulinSensitivityAuto { get; set; }

        /// <summary>
        /// ФЧИ вычисленный пользователем
        /// </summary>
        public decimal? InsulinSensitivityManual { get; set; }

        /// <summary>
        /// ФЧИ фактический
        /// </summary>
        public decimal? InsulinSensitivityFact { get; set; }

        /// <summary>
        /// Время отработки
        /// </summary>
        public TimeSpan WriteOff { get; set; }

        /// <summary>
        /// Тип приёма пищи
        /// </summary>
        public Guid EatingTypeId { get; set; }

        /// <summary>
        /// Тип приёма пищи (Навигационное свойство)
        /// </summary>
        public EatingType EatingType { get; set; }

        /// <summary>
        /// Тип нагрузки
        /// </summary>
        public Guid ExerciseId { get; set; }

        /// <summary>
        /// Тип нагрузки (Навигационное свойство)
        /// </summary>
        public Exercise Exercise { get; set; }

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