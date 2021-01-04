using DataAccessLayer.Contexts;
using System;
using System.Collections;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using DataAccessLayer.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace InsulinSensitivity
{
    public partial class App : Application
    {
        /// <summary>
        /// Имя БД
        /// </summary>
        public const string DBFILENAME = "InsulinSensitivityApp.db";

        public App()
        {
            InitializeComponent();

            GlobalParameters.DbPath = DependencyService.Get<IPath>().GetDatabasePath(DBFILENAME);
            using (var db = new ApplicationContext(GlobalParameters.DbPath))
            {
                // Создаем БД, если она отсутствует
                //db.Database.EnsureDeleted();
                //db.Database.EnsureCreated();
                db.Database.Migrate();

                // Инициализируем таблицы стартовой информацией
                if (db.EatingTypes.Count() == 0)
                {
                    db.EatingTypes.AddRange(new List<EatingType>()
                    {
                        new EatingType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Завтрак",
                            TimeStart = new TimeSpan(5, 0, 0),
                            TimeEnd = new TimeSpan(8, 59, 59),
                            IsBasal = false
                        },
                        new EatingType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Поздний завтрак",
                            TimeStart = new TimeSpan(9, 0, 0),
                            TimeEnd = new TimeSpan(11, 14, 59),
                            IsBasal = false
                        },
                        new EatingType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Обед",
                            TimeStart = new TimeSpan(11, 15, 0),
                            TimeEnd = new TimeSpan(13, 59, 59),
                            IsBasal = false
                        },
                        new EatingType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Поздний обед",
                            TimeStart = new TimeSpan(14, 00, 0),
                            TimeEnd = new TimeSpan(16, 14, 59),
                            IsBasal = false
                        },
                        new EatingType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Ужин",
                            TimeStart = new TimeSpan(16, 15, 0),
                            TimeEnd = new TimeSpan(19, 14, 59),
                            IsBasal = false
                        },
                        new EatingType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Поздний ужин",
                            TimeStart = new TimeSpan(19, 15, 0),
                            TimeEnd = new TimeSpan(23, 59, 59),
                            IsBasal = false
                        },
                        new EatingType()
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

                // Инициализируем таблицы стартовой информацией
                if (db.InsulinTypes.Count() == 0)
                {
                    db.InsulinTypes.AddRange(new List<InsulinType>()
                    {
                        new InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Хумалог",
                            IsBasal = false,
                            Duration = 5
                        },
                        new InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Туджео",
                            IsBasal = true,
                            Duration = 24
                        },
                        new InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Тресиба",
                            IsBasal = true,
                            Duration = 48
                        },
                        new InsulinType()
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
                    db.InsulinTypes.AddRange(new List<InsulinType>()
                    {
                        new InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Новорапид",
                            IsBasal = false,
                            Duration = 5
                        },
                        new InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Апидра",
                            IsBasal = false,
                            Duration = 5
                        },
                        new InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Фиасп",
                            IsBasal = false,
                            Duration = 4
                        },
                        new InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Левемир",
                            IsBasal = true,
                            Duration = 12
                        },
                        new InsulinType()
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
                    db.InsulinTypes.AddRange(new List<InsulinType>()
                    {
                        new InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Инсуман Базал",
                            IsBasal = true,
                            Duration = 12
                        },
                        new InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Изофан",
                            IsBasal = true,
                            Duration = 12
                        },
                        new InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Гларгин",
                            IsBasal = true,
                            Duration = 24
                        },
                        new InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "РинГлар",
                            IsBasal = true,
                            Duration = 24
                        },
                        new InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Актрапид",
                            IsBasal = false,
                            Duration = 8
                        },
                        new InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Актрапид",
                            IsBasal = false,
                            Duration = 8
                        },
                        new InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Инсуман Рапид",
                            IsBasal = false,
                            Duration = 8
                        },
                        new InsulinType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "РинЛиз",
                            IsBasal = false,
                            Duration = 5
                        }
                    });
                    db.SaveChanges();
                }

                // Инициализируем таблицы стартовой информацией
                if (db.ExerciseTypes.Count() == 0)
                {
                    db.ExerciseTypes.AddRange(new List<ExerciseType>()
                    {
                        new ExerciseType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Без движения",
                            IsEmpty = true
                        },
                        new ExerciseType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Обычная активность",
                            IsDefault = true,
                            IsEmpty = true
                        },
                        new ExerciseType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Ходьба"
                        },
                        new ExerciseType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Бег"
                        },
                        new ExerciseType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Работа по дому"
                        },
                        new ExerciseType()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Тренировка"
                        }
                    });
                    db.SaveChanges();
                }                
            }

            // Инициализация главной страницы
            MainPage = new MainPage();

            // Инициализация навигации
            GlobalParameters.Navigation = ((MasterDetailPage)MainPage).Detail.Navigation;
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
