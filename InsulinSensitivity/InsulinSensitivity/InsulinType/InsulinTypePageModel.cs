using System;
using System.Collections.Generic;
using System.Text;

namespace InsulinSensitivity.InsulinType
{
    public class InsulinTypePageModel
    {
        /// <summary>
        /// Продолжительность
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Смещение
        /// </summary>
        public int Offset { get; set; }
    }
}
