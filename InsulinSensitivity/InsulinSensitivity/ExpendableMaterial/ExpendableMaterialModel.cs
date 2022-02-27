using System;
using Xamarin.Forms;

namespace InsulinSensitivity.ExpendableMaterial
{
    public class ExpendableMaterialModel
    {
        /// <summary>
        /// Осталось дней
        /// </summary>
        public decimal Days { get; set; }

        /// <summary>
        /// Тип
        /// </summary>
        public DataAccessLayer.Models.ExpendableMaterialType MaterialType { get; set; }

        /// <summary>
        /// Дата окончания
        /// </summary>
        public DateTime DateTime =>
            DateTime.Now.AddDays((double)Days);

        /// <summary>
        /// Подсветка
        /// </summary>
        public Color Foreground
        {
            get
            {
                var sub = this.DateTime.Date - DateTime.Now.Date;
                return sub.TotalDays < 30
                    ? Color.Red
                    : sub.TotalDays > 365
                        ? Color.Yellow
                        : Color.Black;
            }            
        }
    }
}
