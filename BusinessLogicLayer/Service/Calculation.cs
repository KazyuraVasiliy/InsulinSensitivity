using System;
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
        public static int GetBodyMassIndex(int height, int weight) =>
            (int)Math.Round((decimal)(weight / (height * height)), 0, MidpointRounding.AwayFromZero);

        /// <summary>
        /// Возвращает коэффициент для расчёта объёма крови
        /// </summary>
        /// <param name="birthDate">Дата рождения</param>
        /// <param name="gender">Пол</param>
        /// <param name="height">Рост</param>
        /// <param name="weight">Вес</param>
        /// <returns></returns>
        public static int GetBloodFactor(DateTime birthDate, bool gender, int height, int weight)
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
    }
}
