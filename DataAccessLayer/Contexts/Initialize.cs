using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Essentials;

namespace DataAccessLayer.Contexts
{
    public static class Initialize
    {
        /// <summary>
        /// Инициализирует БД первичной информацией
        /// </summary>
        /// <param name="databasePath">Путь до БД</param>
        public static void Init(string databasePath)
        {
            using (var db = new ApplicationContext(databasePath))
            {
                db.Database.Migrate();

                // Инициализация периодов
                if (db.EatingTypes.Count() == 0)
                {
                    db.EatingTypes.AddRange(new List<Models.EatingType>()
                    {
                        new Models.EatingType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Завтрак",
                            TimeStart = new TimeSpan(5, 0, 0),
                            TimeEnd = new TimeSpan(8, 59, 59),
                            IsBasal = false
                        },
                        new Models.EatingType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Поздний завтрак",
                            TimeStart = new TimeSpan(9, 0, 0),
                            TimeEnd = new TimeSpan(11, 14, 59),
                            IsBasal = false
                        },
                        new Models.EatingType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Обед",
                            TimeStart = new TimeSpan(11, 15, 0),
                            TimeEnd = new TimeSpan(13, 59, 59),
                            IsBasal = false
                        },
                        new Models.EatingType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Поздний обед",
                            TimeStart = new TimeSpan(14, 00, 0),
                            TimeEnd = new TimeSpan(16, 14, 59),
                            IsBasal = false
                        },
                        new Models.EatingType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Ужин",
                            TimeStart = new TimeSpan(16, 15, 0),
                            TimeEnd = new TimeSpan(19, 14, 59),
                            IsBasal = false
                        },
                        new Models.EatingType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Поздний ужин",
                            TimeStart = new TimeSpan(19, 15, 0),
                            TimeEnd = new TimeSpan(23, 59, 59),
                            IsBasal = false
                        },
                        new Models.EatingType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Ночной приём пищи",
                            TimeStart = new TimeSpan(0, 0, 0),
                            TimeEnd = new TimeSpan(4, 59, 59),
                            IsBasal = false
                        }
                    });
                    db.SaveChanges();
                }

                // Инициализация периодов
                if (db.EatingTypes.Count() == 7)
                {
                    // ...Изменение завтрака
                    var breakfast = db.EatingTypes
                        .First(x =>
                            x.Name == "Завтрак");

                    breakfast.TimeStart = new TimeSpan(7, 0, 0);
                    breakfast.TimeEnd = new TimeSpan(8, 59, 59);

                    // ...Изменение ночного приёма
                    var nightEating = db.EatingTypes
                        .First(x =>
                            x.Name == "Ночной приём пищи");

                    nightEating.Name = "Ночь";
                    nightEating.TimeStart = new TimeSpan(0, 0, 0);
                    nightEating.TimeEnd = new TimeSpan(3, 59, 59);

                    // ...Добавление раннего утра
                    db.EatingTypes.AddRange(new List<Models.EatingType>()
                    {
                        new Models.EatingType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Раннее утро",
                            TimeStart = new TimeSpan(4, 0, 0),
                            TimeEnd = new TimeSpan(6, 59, 59),
                            IsBasal = false
                        }
                    });
                    db.SaveChanges();
                }

                // Изменение времени периодов приёмов пищи
                if (db.EatingTypes.Count() == 8)
                {
                    var night = db.EatingTypes
                        .First(x =>
                            x.Name == "Ночь");

                    if (night.TimeEnd != new TimeSpan(2, 59, 59))
                    {
                        var eatingTypes = db.EatingTypes
                            .ToList()
                            .OrderBy(x => x.TimeStart)
                            .ToList();

                        for (int i = 0; i < eatingTypes.Count; i++)
                        {
                            eatingTypes[i].TimeStart = new TimeSpan(3 * i, 0, 0);
                            eatingTypes[i].TimeEnd = new TimeSpan(3 * i + 2, 59, 59);
                        }

                        db.SaveChanges();
                    }
                }

                // Инициализация инсулинов
                if (db.InsulinTypes.Count() == 0)
                {
                    db.InsulinTypes.AddRange(new List<Models.InsulinType>()
                    {
                        new Models.InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Хумалог",
                            IsBasal = false,
                            Duration = 5
                        },
                        new Models.InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Туджео",
                            IsBasal = true,
                            Duration = 24
                        },
                        new Models.InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Тресиба",
                            IsBasal = true,
                            Duration = 48
                        },
                        new Models.InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Росинсулин",
                            IsBasal = false,
                            Duration = 8
                        }
                    });
                    db.SaveChanges();
                }

                if (db.InsulinTypes.Count() == 4)
                {
                    db.InsulinTypes.AddRange(new List<Models.InsulinType>()
                    {
                        new Models.InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Новорапид",
                            IsBasal = false,
                            Duration = 5
                        },
                        new Models.InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Апидра",
                            IsBasal = false,
                            Duration = 5
                        },
                        new Models.InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Фиасп",
                            IsBasal = false,
                            Duration = 4
                        },
                        new Models.InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Левемир",
                            IsBasal = true,
                            Duration = 12
                        },
                        new Models.InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Протафан",
                            IsBasal = true,
                            Duration = 12
                        }
                    });
                    db.SaveChanges();
                }

                if (db.InsulinTypes.Count() == 9)
                {
                    db.InsulinTypes.AddRange(new List<Models.InsulinType>()
                    {
                        new Models.InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Инсуман Базал",
                            IsBasal = true,
                            Duration = 12
                        },
                        new Models.InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Изофан",
                            IsBasal = true,
                            Duration = 12
                        },
                        new Models.InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Гларгин",
                            IsBasal = true,
                            Duration = 24
                        },
                        new Models.InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "РинГлар",
                            IsBasal = true,
                            Duration = 24
                        },
                        new Models.InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Актрапид",
                            IsBasal = false,
                            Duration = 8
                        },
                        new Models.InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Инсуман Рапид",
                            IsBasal = false,
                            Duration = 8
                        },
                        new Models.InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "РинЛиз",
                            IsBasal = false,
                            Duration = 5
                        }
                    });
                    db.SaveChanges();
                }

                if (db.InsulinTypes.Count() == 16)
                {
                    db.InsulinTypes.AddRange(new List<Models.InsulinType>()
                    {
                        new Models.InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Лантус",
                            IsBasal = true,
                            Duration = 24
                        },
                    });
                    db.SaveChanges();
                }

                if (db.InsulinTypes.Count() == 17)
                {
                    db.InsulinTypes.AddRange(new List<Models.InsulinType>()
                    {
                        new Models.InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "РинФаст",
                            IsBasal = false,
                            Duration = 5
                        },
                    });
                    db.SaveChanges();
                }

                if (db.InsulinTypes.Count() == 18)
                {
                    db.InsulinTypes.AddRange(new List<Models.InsulinType>()
                    {
                        new Models.InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Люмжев",
                            IsBasal = false,
                            Duration = 4,
                            Offset = 1,
                            Profile = 2
                        },
                        new Models.InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Люмжев (концентрат)",
                            IsBasal = false,
                            Duration = 4,
                            Offset = 1,
                            Profile = 2,
                            Concentration = 2
                        },
                    });
                    db.SaveChanges();
                }

                // Инициализация смещений
                if (db.InsulinTypes.Any(x => x.Offset == 0))
                {
                    var types = db.InsulinTypes
                        .ToList();

                    foreach (var type in types)
                    {
                        if (type.Name == "Фиасп")
                            type.Offset = 4;

                        if (type.Name == "Апидра")
                            type.Offset = 10;

                        if (type.Name == "Хумалог" || type.Name == "Новорапид" || type.Name == "РинЛиз" || type.Name == "РинФаст")
                            type.Offset = 15;

                        if (type.Name == "Актрапид" || type.Name == "Инсуман Рапид" || type.Name == "Росинсулин")
                            type.Offset = 30;

                        if (type.Name == "Левемир" || type.Name == "Протафан" || type.Name == "Изофан" || type.Name == "Инсуман Базал")
                            type.Offset = 60;

                        if (type.Name == "РинГлар" || type.Name == "Гларгин" || type.Name == "Туджео" || type.Name == "Лантус")
                            type.Offset = 90;

                        if (type.Name == "Тресиба")
                            type.Offset = 120;
                    }

                    db.SaveChanges();
                }

                // Инициализация нагрузок
                if (db.ExerciseTypes.Count() == 0)
                {
                    db.ExerciseTypes.AddRange(new List<Models.ExerciseType>()
                    {
                        new Models.ExerciseType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Без движения",
                            IsEmpty = true
                        },
                        new Models.ExerciseType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Обычная активность",
                            IsDefault = true,
                            IsEmpty = true
                        },
                        new Models.ExerciseType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Ходьба"
                        },
                        new Models.ExerciseType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Бег"
                        },
                        new Models.ExerciseType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Работа по дому"
                        },
                        new Models.ExerciseType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Тренировка"
                        }
                    });
                    db.SaveChanges();
                }

                // Инициализация типов расходного материала
                if (db.ExpendableMaterialTypes.Count() == 0)
                {
                    db.ExpendableMaterialTypes.AddRange(new List<Models.ExpendableMaterialType>()
                    {
                        new Models.ExpendableMaterialType()
                        {
                            Id = 1,
                            Name = "Тест-полоски",
                            Unit = "шт."
                        },
                        new Models.ExpendableMaterialType()
                        {
                            Id = 2,
                            Name = "Сенсоры",
                            Unit = "шт."
                        },
                        new Models.ExpendableMaterialType()
                        {
                            Id = 3,
                            Name = "Базальный инсулин",
                            Unit = "ед."
                        },
                        new Models.ExpendableMaterialType()
                        {
                            Id = 4,
                            Name = "Болюсный инсулин",
                            Unit = "ед."
                        },
                        new Models.ExpendableMaterialType()
                        {
                            Id = 5,
                            Name = "Катетеры",
                            Unit = "шт."
                        },
                        new Models.ExpendableMaterialType()
                        {
                            Id = 6,
                            Name = "Канюли",
                            Unit = "шт."
                        },
                        new Models.ExpendableMaterialType()
                        {
                            Id = 7,
                            Name = "Картриджи",
                            Unit = "шт."
                        },
                        new Models.ExpendableMaterialType()
                        {
                            Id = 8,
                            Name = "Иглы",
                            Unit = "шт."
                        },
                    });
                    db.SaveChanges();
                }

                // Создание временной метки в FileTime формате
                if (db.Eatings.FirstOrDefault()?.FileTimeUtcDateCreated == 0)
                {
                    var eatings = db.Eatings.ToList();
                    foreach (var eating in eatings)
                        eating.FileTimeUtcDateCreated = eating.DateCreated.Date.ToFileTimeUtc();
                    db.SaveChanges();
                }

                // Перенос настроек в БД
                if (db.Users.FirstOrDefault() is User user && user.CatheterLifespan == 0)
                {
                    user.IsActiveBasal = Preferences.Get("isActiveBasal", false);
                    user.IsAverageCalculateActive = Preferences.Get("isAverageCalculateActive", true);
                    user.IsExerciseCalculateActive = Preferences.Get("isExerciseCalculateActive", true);
                    user.IsCycleCalculateActive = Preferences.Get("isCycleCalculateActive", true);
                    user.IsCannulaCalculateActive = Preferences.Get("isCannulaCalculateActive", true);
                    user.EatingDuration = Preferences.Get("eatingDuration", 5);
                    user.LengthGraph = Preferences.Get("lengthGraph", 45);
                    user.CannulaLifespan = Preferences.Get("cannulaLifespan", 3);
                    user.CatheterLifespan = Preferences.Get("catheterLifespan", 3);
                    user.CartridgeLifespan = Preferences.Get("cartridgeLifespan", 6);
                    user.BatteryLifespan = Preferences.Get("batteryLifespan", 30);
                    user.MonitoringLifespan = Preferences.Get("monitoringLifespan", 14);

                    db.SaveChanges();
                }

                // Инициализация профилей
                if (!db.InsulinTypes.Any(x => x.Profile == 1))
                {
                    var type = db.InsulinTypes
                        .FirstOrDefault(x => 
                            x.Name == "Фиасп");

                    if (type != null)
                        type.Profile = 1;

                    db.SaveChanges();
                }
            }
        }
    }
}
