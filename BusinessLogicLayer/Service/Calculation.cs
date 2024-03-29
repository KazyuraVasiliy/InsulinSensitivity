﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Service
{
    public static class Calculation
    {
        /// <summary>
        /// Возвращает ИМТ
        /// </summary>
        /// <param name="height">Рост</param>
        /// <param name="weight">Вес</param>
        /// <returns></returns>
        private static int GetBodyMassIndex(int height, int weight) =>
            (int)Math.Round((decimal)(weight / (height * height)), 0, MidpointRounding.AwayFromZero);

        /// <summary>
        /// Возвращает коэффициент для расчёта объёма крови
        /// </summary>
        /// <param name="birthDate">Дата рождения</param>
        /// <param name="gender">Пол</param>
        /// <param name="height">Рост</param>
        /// <param name="weight">Вес</param>
        /// <returns></returns>
        private static int GetBloodFactor(DateTime birthDate, bool gender, int height, int weight)
        {
            DateTime date = DateTime.Now;
            int bmi = GetBodyMassIndex(height, weight);

            int bloodFactor = 0;

            if (birthDate.AddYears(18) > date)
            {
                // ... Ребёнок до полугода
                if (birthDate.AddMonths(6) > date)
                    bloodFactor = 87;

                // ... Ребёнок до 1 года
                else if (birthDate.AddMonths(12) > date)
                    bloodFactor = 86;

                // ... Ребёнок до 6 лет
                else if (birthDate.AddYears(6) > date)
                    bloodFactor = 80;

                else if (birthDate.AddYears(7) > date)
                    bloodFactor = 78;

                else if (birthDate.AddYears(8) > date)
                    bloodFactor = 77;

                else if (birthDate.AddYears(9) > date)
                    bloodFactor = 76;

                else if (birthDate.AddYears(10) > date)
                    bloodFactor = 75;

                else if (birthDate.AddYears(11) > date)
                    bloodFactor = 74;

                else if (birthDate.AddYears(12) > date)
                    bloodFactor = 73;

                else if (birthDate.AddYears(13) > date)
                    bloodFactor = 72;

                else bloodFactor = 71;
            }

            else
            {
                if (bmi >= 30)
                    bloodFactor = 55;

                if ((bmi >= 25 && bmi < 30) || bmi <= 20)
                    bloodFactor = 60;

                if (bmi > 20 && bmi < 25)
                    bloodFactor = 65;

                if (gender == true)
                    bloodFactor += 5;
            }

            return bloodFactor;
        }

        /// <summary>
        /// Возвращает УК
        /// </summary>
        /// <param name="birthDate">Дата рождения</param>
        /// <param name="gender">Пол</param>
        /// <param name="height">Рост</param>
        /// <param name="weight">Вес</param>
        /// <returns></returns>
        public static decimal GetCarbohydrateCoefficient(DateTime birthDate, bool gender, int height, int weight) =>
            750M / (weight * GetBloodFactor(birthDate, gender, height, weight));

        /// <summary>
        /// Возвращает фактический ФЧИ
        /// </summary>
        /// <param name="glucoseStart">Исходный сахар</param>
        /// <param name="glucoseEnd">Целевой сахар</param>
        /// <param name="carbohydrateCoefficient">УК</param>
        /// <param name="proteinCoefficient">БК</param>
        /// <param name="fatCoefficient">ЖК</param>
        /// <param name="protein">Белки</param>
        /// <param name="fat">Жиры</param>
        /// <param name="carbohydrate">Углеводы</param>
        /// <param name="bolusDose">Доза</param>
        /// <returns></returns>
        public static decimal GetInsulinSensitivityFact(decimal glucoseStart, decimal glucoseEnd,
            decimal carbohydrateCoefficient, decimal proteinCoefficient, decimal fatCoefficient,
            decimal protein, decimal fat, decimal carbohydrate,
            decimal bolusDose) =>
            (glucoseStart - glucoseEnd + carbohydrateCoefficient * (proteinCoefficient * protein + fatCoefficient * fat + carbohydrate)) / bolusDose;

        /// <summary>
        /// Возвращает дозу болюсного инсулина
        /// </summary>
        /// <param name="glucoseStart">Исходный сахар</param>
        /// <param name="glucoseEnd">Сахар на отработке</param>
        /// <param name="carbohydrateCoefficient">УК</param>
        /// <param name="proteinCoefficient">БК</param>
        /// <param name="fatCoefficient">ЖК</param>
        /// <param name="protein">Белки</param>
        /// <param name="fat">Жиры</param>
        /// <param name="carbohydrate">Углеводы</param>
        /// <param name="insulinSensitivityFact">ФЧИ</param>
        /// <returns></returns>
        public static decimal GetBolusDose(decimal glucoseStart, decimal glucoseEnd,
            decimal carbohydrateCoefficient, decimal proteinCoefficient, decimal fatCoefficient,
            decimal protein, decimal fat, decimal carbohydrate,
            decimal insulinSensitivityFact) =>
            (glucoseStart - glucoseEnd + carbohydrateCoefficient * (proteinCoefficient * protein + fatCoefficient * fat + carbohydrate)) / insulinSensitivityFact;

        /// <summary>
        /// Возвращает ожидаемый сахар
        /// </summary>
        /// <param name="glucoseStart">Исходный сахар</param>
        /// <param name="bolusDose">Доза болюсного инсулина</param>
        /// <param name="carbohydrateCoefficient">УК</param>
        /// <param name="proteinCoefficient">БК</param>
        /// <param name="fatCoefficient">ЖК</param>
        /// <param name="protein">Белки</param>
        /// <param name="fat">Жиры</param>
        /// <param name="carbohydrate">Углеводы</param>
        /// <param name="insulinSensitivityFact">ФЧИ</param>
        /// <returns></returns>
        public static decimal GetExpectedGlucose(decimal glucoseStart, decimal bolusDose,
            decimal carbohydrateCoefficient, decimal proteinCoefficient, decimal fatCoefficient,
            decimal protein, decimal fat, decimal carbohydrate,
            decimal insulinSensitivityFact) =>
            glucoseStart - insulinSensitivityFact * bolusDose + carbohydrateCoefficient * (proteinCoefficient * protein + fatCoefficient * fat + carbohydrate);

        /// <summary>
        /// Возвращает значение функции ошибок
        /// </summary>
        /// <param name="x">Аргумент</param>
        /// <remarks>
        /// https://www.johndcook.com/blog/csharp_erf/
        /// </remarks>
        /// <returns></returns>
        private static double Erf(double x)
        {
            // constants
            double a1 = 0.254829592;
            double a2 = -0.284496736;
            double a3 = 1.421413741;
            double a4 = -1.453152027;
            double a5 = 1.061405429;
            double p = 0.3275911;

            // Save the sign of x
            int sign = 1;
            if (x < 0)
                sign = -1;
            x = Math.Abs(x);

            // A&S formula 7.1.26
            double t = 1.0 / (1.0 + p * x);
            double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

            return sign * y;
        }

        /// <summary>
        /// Возвращает значение интеграла функции 4.26 * 10^(-5) * x^(3 / 2) * e^(-1.5 * x / 75)
        /// </summary>
        /// <remarks>
        /// https://github.com/LoopKit/Loop/issues/388
        /// Novolog (6H)
        /// https://www.integral-calculator.com
        /// </remarks>
        /// <param name="x">Аргумент</param>
        /// <returns></returns>
        private static double InsulinActivityCurvesIntegrateNovolog(double x) =>
            (213 * ((-25 * Math.Pow(x, 3D / 2) - 1875 * Math.Sqrt(x)) * Math.Exp(-x / 50) + (9375 * Math.Sqrt(Math.PI) * Erf(Math.Sqrt(x) / (5 * Math.Sqrt(2)))) / Math.Sqrt(2))) / 2500000;

        /// <summary>
        /// Возвращает значение интеграла функции 3.31 * 10^(-4) * x * e^(-x / 55), from x = 0 to infinity+
        /// </summary>
        /// <remarks>
        /// https://github.com/LoopKit/Loop/issues/388
        /// Fiasp (6H)
        /// https://www.integral-calculator.com
        /// </remarks>
        /// <param name="x">Аргумент</param>
        /// <returns></returns>
        private static double InsulinActivityCurvesIntegrateFiasp(double x) =>
            -(3641 * (x + 55) * Math.Exp(-x / 55)) / 200000 + 1.001275;

        /// <summary>
        /// Возвращает значение интеграла функции 4.96 * 10^(-4) * x * e^(-x / 45), from x = 0 to infinity+
        /// </summary>
        /// <remarks>
        /// https://bionicwookiee.com/2022/12/04/insulin-timings-2022/
        /// Lyumzhev
        /// https://www.integral-calculator.com
        /// </remarks>
        /// <param name="x">Аргумент</param>
        /// <returns></returns>
        private static double InsulinActivityCurvesIntegrateLyumzhev(double x) =>
            -(279 * (x + 45) * Math.Exp(-x / 45)) / 12500 + 1.0044;

        /// <summary>
        /// Выбор профиля инсулина
        /// </summary>
        private static Dictionary<int, Func<double, double>> InsulinActivityCurvesIntegrate =
            new Dictionary<int, Func<double, double>>()
            {
                [0] = InsulinActivityCurvesIntegrateNovolog,
                [1] = InsulinActivityCurvesIntegrateFiasp,
                [2] = InsulinActivityCurvesIntegrateLyumzhev
            };

        /// <summary>
        /// Возвращает количество минут прошедшее после инъекции
        /// </summary>
        /// <param name="start">Время инъекции</param>
        /// <param name="now">Текущее время</param>
        /// <returns></returns>
        private static double GetMinutesAfterInjection(DateTime start, DateTime now) =>
            (now - start).TotalMinutes;

        /// <summary>
        /// Объединяет DateTime и TimeSpan без секунд
        /// </summary>
        /// <param name="d">DateTime</param>
        /// <param name="t">TimeSpan</param>
        /// <returns>DateTime, где TimeOfDay = t</returns>
        public static DateTime DateTimeUnionTimeSpan(DateTime d, TimeSpan t) =>
            new DateTime(d.Year, d.Month, d.Day, t.Hours, t.Minutes, 0);

        /// <summary>
        /// Объединяет DateTime и TimeSpan без минут и секунд
        /// </summary>
        /// <param name="d">DateTime</param>
        /// <param name="t">TimeSpan</param>
        /// <returns>DateTime, где TimeOfDay = t</returns>
        public static DateTime DateTimeUnionTimeSpanWithoutMinutes(DateTime d, TimeSpan t) =>
            new DateTime(d.Year, d.Month, d.Day, t.Hours, 0, 0);

        /// <summary>
        /// Создаёт новый DateTime исключая секунды
        /// </summary>
        /// <param name="d">DateTime</param>
        /// <returns></returns>
        public static DateTime DateTimeWithoutSeconds(DateTime d) =>
            new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, 0);

        /// <summary>
        /// Создаёт новый TimeSpan исключая секунды
        /// </summary>
        /// <param name="t">TimeSpan</param>
        /// <returns></returns>
        public static TimeSpan TimeSpanWithoutSeconds(TimeSpan t) =>
            new TimeSpan(t.Hours, t.Minutes, 0);

        /// <summary>
        /// Возвращает процент активного инсулина
        /// </summary>
        /// <param name="start">Дата и время инъекции</param>
        /// <param name="now">Текущая дата и время</param>
        /// <param name="duration">Длительность инсулина в часах</param>
        /// <returns></returns>
        public static double GetActiveInsulinPercent(DateTime start, DateTime now, int duration, int profile)
        {
            if (now == start)
                return 1;

            var minutes = GetMinutesAfterInjection(start, now);
            return minutes >= duration * 60 || now < start
                ? 0
                : 1 - InsulinActivityCurvesIntegrate[profile](minutes * 6 / duration);
        }
    }
}
