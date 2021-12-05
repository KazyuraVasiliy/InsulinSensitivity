using System;
using System.Collections.Generic;

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
        /// Ожидаемый сахар
        /// </summary>
        public decimal? ExpectedGlucose { get; set; }

        /// <summary>
        /// Пауза после инъекции
        /// </summary>
        public int Pause { get; set; }

        /// <summary>
        /// Ошибка, совершённая во приёма пищи
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Ошибка прогноза
        /// </summary>
        public string ForecastError { get; set; }

        /// <summary>
        /// Количество активного инсулина в крови перед поставновкой инъекции
        /// </summary>
        [Obsolete("This property is obsolete.", false)]
        public decimal ActiveInsulinStart { get; set; }

        /// <summary>
        /// Количество активного инсулина в крови на отработке
        /// </summary>
        [Obsolete("This property is obsolete.", false)]
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
        /// Базальная скорость
        /// </summary>
        public decimal BasalRate { get; set; }

        /// <summary>
        /// Временная базальная скорость
        /// </summary>
        public decimal BasalRateCoefficient { get; set; }

        /// <summary>
        /// Время инъекции базального инсулина
        /// </summary>
        public DateTime? BasalInjectionTime { get; set; }

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
        /// ФЧИ вычисленный автоматически (метод №3)
        /// </summary>
        public decimal? InsulinSensitivityAutoThree { get; set; }

        /// <summary>
        /// ФЧИ вычисленный автоматически (метод №4)
        /// </summary>
        public decimal? InsulinSensitivityAutoFour { get; set; }

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
        [Obsolete("This property is obsolete. Use WorkingTime instead.", false)]
        public decimal WriteOff { get; set; }

        /// <summary>
        /// Время отработки
        /// </summary>
        public TimeSpan WorkingTime { get; set; }

        /// <summary>
        /// Планируемое / Фактическое время отработки для расчёта активного базального инсулина
        /// </summary>
        public DateTime? EndEating { get; set; }

        /// <summary>
        /// Начало менструального цикла
        /// </summary>
        [Obsolete("This property is obsolete. Use MenstrualCycle class instead.", false)]
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

        /// <summary>
        /// Игнорируется ли приём пищи
        /// </summary>
        public bool IsIgnored { get; set; } 

        /// <summary>
        /// Замена канюли
        /// </summary>
        public bool IsCannulaReplacement { get; set; }

        /// <summary>
        /// Замена катетера
        /// </summary>
        public bool IsCatheterReplacement { get; set; }

        /// <summary>
        /// Замена резервуара
        /// </summary>
        public bool IsCartridgeReplacement { get; set; }

        /// <summary>
        /// Замена батарейки
        /// </summary>
        public bool IsBatteryReplacement { get; set; }

        /// <summary>
        /// Итоговая доза инсулина
        /// </summary>
        public decimal? BolusDoseTotal { get; set; }

        /// <summary>
        /// Тип базального инсулина
        /// </summary>
        public Guid? BasalTypeId { get; set; }

        /// <summary>
        /// Тип базального инсулина (Навигационное свойство)
        /// </summary>
        public InsulinType BasalType { get; set; }

        /// <summary>
        /// Тип болюсного инсулина
        /// </summary>
        public Guid? BolusTypeId { get; set; }

        /// <summary>
        /// Тип болюсного инсулина (Навигационное свойство)
        /// </summary>
        public InsulinType BolusType { get; set; }

        /// <summary>
        /// Инъекции (Навигационное свойство)
        /// </summary>
        public List<Injection> Injections { get; set; }

        /// <summary>
        /// Промежуточные измерения (Навигационное свойство)
        /// </summary>
        public List<IntermediateDimension> IntermediateDimensions { get; set; }
    }
}