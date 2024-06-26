﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BusinessLogicLayer.Service
{
    public static class Methods
    {
        /// <summary>
        /// Указывает, содержит ли исходная строка заданную подстроку без учёта регистра
        /// </summary>
        /// <param name="source">Исходная строка</param>
        /// <param name="substring">Подстрока</param>
        /// <returns>true - содержит</returns>
        public static bool StringContains(string source, string substring) =>
            (source ?? "").ToUpper(CultureInfo.CurrentCulture).Contains((substring ?? "").ToUpper(CultureInfo.CurrentCulture));

        /// <summary>
        /// Указывает, равны ли строки без учёта регистра
        /// </summary>
        /// <param name="one">Первая строка</param>
        /// <param name="two">Вторая строка</param>
        /// <returns>true - содержит</returns>
        public static bool StringEqual(string one, string two) =>
            (one ?? "").ToUpper(CultureInfo.CurrentCulture) == (two ?? "").ToUpper(CultureInfo.CurrentCulture);

        /// <summary>
        /// Округление AwayFromZero
        /// </summary>
        /// <param name="d"></param>
        /// <param name="decimals"></param>
        /// <returns></returns>
        public static decimal Round(decimal d, int decimals) =>
            Math.Round(d, decimals, MidpointRounding.AwayFromZero);

        /// <summary>
        /// Округление AwayFromZero
        /// </summary>
        /// <param name="d"></param>
        /// <param name="decimals"></param>
        /// <returns></returns>
        public static float Round(float d, int decimals) =>
            (float)Math.Round((double)d, decimals, MidpointRounding.AwayFromZero);
    }
}
