using BusinessLogicLayer.Service.Models;
using BusinessLogicLayer.ViewModel;
using System.Collections.Generic;

namespace InsulinSensitivity.Information
{
    public class InformationPageViewModel : ObservableBase
    {
        public List<BloodFactor> BloodFactors =>
            new List<BloodFactor>()
            {
                new BloodFactor("Возраст", "ИМТ", "Мужчина", "Женщина"),
                new BloodFactor("< 6 мес.", "-", "87", "87"),
                new BloodFactor("< 1 года", "-", "86", "86"),
                new BloodFactor("< 6 лет", "-", "80", "80"),
                new BloodFactor("< 7 лет", "-", "78", "78"),
                new BloodFactor("< 8 лет", "-", "77", "77"),
                new BloodFactor("< 9 лет", "-", "76", "76"),
                new BloodFactor("< 10 лет", "-", "75", "75"),
                new BloodFactor("< 11 лет", "-", "74", "74"),
                new BloodFactor("< 12 лет", "-", "73", "73"),
                new BloodFactor("< 13 лет", "-", "72", "72"),
                new BloodFactor("< 18 лет", "-", "71", "71"),
                new BloodFactor(">= 18 лет", ">= 30", "60", "55"),
                new BloodFactor(">= 18 лет", "20 < x < 25", "70", "65"),
                new BloodFactor(">= 18 лет", "Иначе", "65", "60"),
            };
    }
}
