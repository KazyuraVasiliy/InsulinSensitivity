namespace BusinessLogicLayer.Service.Models
{
    public class BloodFactor
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="age"></param>
        /// <param name="imt"></param>
        /// <param name="man"></param>
        /// <param name="woman"></param>
        public BloodFactor(string age, string imt, string man, string woman)
        {
            Age = age;
            IMT = imt;
            Man = man;
            Woman = woman;
        }

        /// <summary>
        /// Возраст
        /// </summary>
        public string Age { get; set; }

        /// <summary>
        /// ИМТ
        /// </summary>
        public string IMT { get; set; }

        /// <summary>
        /// Мужчина
        /// </summary>
        public string Man {  get; set; }

        /// <summary>
        /// Женщина
        /// </summary>
        public string Woman { get; set; }
    }
}
