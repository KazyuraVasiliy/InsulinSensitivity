﻿using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models
{
    public class User
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Имя
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Пол (true - мужской)
        /// </summary>
        public bool Gender { get; set; }

        /// <summary>
        /// Дата рождения
        /// </summary>
        public DateTime BirthDate { get; set; }

        /// <summary>
        /// Рост
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Вес
        /// </summary>
        public int Weight { get; set; }

        /// <summary>
        /// Гипергликемия
        /// </summary>
        public decimal Hyperglycemia { get; set; }

        /// <summary>
        /// Высокий сахар
        /// </summary>
        public decimal HighSugar { get; set; }

        /// <summary>
        /// Целевая глюкоза
        /// </summary>
        public decimal TargetGlucose { get; set; }

        /// <summary>
        /// Низкий сахар
        /// </summary>
        public decimal LowSugar { get; set; }

        /// <summary>
        /// Гипогликемия
        /// </summary>
        public decimal Hypoglycemia { get; set; }

        /// <summary>
        /// Помпа
        /// </summary>
        public bool IsPump { get; set; }

        /// <summary>
        /// Мониторинг
        /// </summary>
        public bool IsMonitoring { get; set; }

        /// <summary>
        /// Заметки
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Точность дозирования инсулина
        /// </summary>
        public decimal DosingAccuracy { get; set; }

        /// <summary>
        /// Углеводный коэффициент
        /// </summary>
        public decimal CarbohydrateCoefficient { get; set; }

        /// <summary>
        /// Белковый коэффициент
        /// </summary>
        public decimal ProteinCoefficient { get; set; }

        /// <summary>
        /// Жировой коэффициент
        /// </summary>
        public decimal FatCoefficient { get; set; }

        /// <summary>
        /// Скорость абсорбции углеводов
        /// </summary>
        public decimal AbsorptionRateOfCarbohydrates { get; set; }

        /// <summary>
        /// Скорость абсорбции белков
        /// </summary>
        public decimal AbsorptionRateOfProteins { get; set; }

        /// <summary>
        /// Скорость абсорбции жиров
        /// </summary>
        public decimal AbsorptionRateOfFats { get; set; }

        /// <summary>
        /// Тип базального инсулина
        /// </summary>
        public Guid BasalTypeId { get; set; }

        /// <summary>
        /// Количество дней, за которые учитываются средние значения
        /// </summary>
        public int PeriodOfCalculation { get; set; }

        /// <summary>
        /// Временная базальная скорость по умолчанию
        /// </summary>
        public decimal DefaultBasalRateCoefficient { get; set; }

        /// <summary>
        /// Строка подключения к Nightscout
        /// </summary>
        public string NightscoutUri { get; set; }

        /// <summary>
        /// Получать стартовые параметры из Nightscout
        /// </summary>
        public bool IsNightscoutStartParameters { get; set; }

        /// <summary>
        /// Тип базального инсулина (Навигационное свойство)
        /// </summary>
        public InsulinType BasalType { get; set; }

        /// <summary>
        /// Тип болюсного инсулина
        /// </summary>
        public Guid BolusTypeId { get; set; }

        /// <summary>
        /// Тип болюсного инсулина (Навигационное свойство)
        /// </summary>
        public InsulinType BolusType { get; set; }

        /// <summary>
        /// Менструальные циклы (Навигационное свойство)
        /// </summary>
        public List<MenstrualCycle> MenstrualCycles { get; set; }
    }
}