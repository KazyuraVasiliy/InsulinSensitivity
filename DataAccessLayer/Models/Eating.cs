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
        public decimal? GlucoseEnd { get; set; }

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
        /// Доза болюсного инсулина рассчитанная
        /// </summary>
        public decimal? BolusDoseCalculate { get; set; }

        /// <summary>
        /// Доза болюсного инсулина фактическая
        /// </summary>
        public decimal BolusDoseFact { get; set; }

        /// <summary>
        /// ФЧИ вычисленный автоматически (метод №1)
        /// </summary>
        public decimal? InsulinSensitivityAutoOne { get; set; }

        /// <summary>
        /// ФЧИ вычисленный автоматически (метод №2)
        /// </summary>
        public decimal? InsulinSensitivityAutoTwo { get; set; }

        /// <summary>
        /// ФЧИ вычисленный пользователем
        /// </summary>
        public decimal? InsulinSensitivityUser { get; set; }

        /// <summary>
        /// ФЧИ фактический
        /// </summary>
        public decimal? InsulinSensitivityFact { get; set; }

        /// <summary>
        /// Время отработки
        /// </summary>
        public decimal WriteOff { get; set; }

        /// <summary>
        /// Начало менструального цикла
        /// </summary>
        public bool IsMenstrualCycleStart { get; set; }

        /// <summary>
        /// Точность автоматического ФЧИ
        /// </summary>
        public int? AccuracyAuto { get; set; }

        /// <summary>
        /// Точность ФЧИ пользователя
        /// </summary>
        public int? AccuracyUser { get; set; }

        /// <summary>
        /// Тип приёма пищи
        /// </summary>
        public Guid EatingTypeId { get; set; }

        /// <summary>
        /// Тип приёма пищи (Навигационное свойство)
        /// </summary>
        public EatingType EatingType { get; set; }

        /// <summary>
        /// Комментарий
        /// </summary>
        public string Comment { get; set; }

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