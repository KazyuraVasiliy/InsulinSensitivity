using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BusinessLogicLayer.ViewModel
{
    public class AsyncBase : INotifyPropertyChanged
    {
        #region Interface Implementation

        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        #endregion

        #region Properties

        private bool inactive = true;
        /// <summary>
        /// Неактивность асинхронной задачи
        /// </summary>
        public bool Inactive
        {
            get => inactive;
            set
            {
                inactive = value;
                OnPropertyChanged(nameof(Inactive));
            }
        }

        private string description = "";
        /// <summary>
        /// Описание асинхронной задачи
        /// </summary>
        public string Description
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        private int enumerator;
        /// <summary>
        /// Счётчик асинхронных задач
        /// </summary>
        public int Enumerator
        {
            get => enumerator;
            set
            {
                enumerator = value;
                OnPropertyChanged(nameof(Enumerator));
            }
        }

        private int maximum = 1;
        /// <summary>
        /// Верхняя граница ProgressBar
        /// </summary>
        public int Maximum
        {
            get => maximum;
            set
            {
                maximum = value;
                OnPropertyChanged(nameof(Maximum));
            }
        }

        private int value = 0;
        /// <summary>
        /// Текущее значение ProgressBar
        /// </summary>
        public int Value
        {
            get => value;
            set
            {
                this.value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        #endregion

        #region Collections

        /// <summary>
        /// Описания поступающие при открытии задачи
        /// </summary>
        private Dictionary<string, string> Descriptions { get; set; } =
            new Dictionary<string, string>();

        #endregion

        #region Methods

        /// <summary>
        /// Открытие асинхронной задачи
        /// </summary>
        public void Open(string description = null, [CallerMemberName] string caller = null)
        {
            lock (this)
            {
                if (Inactive)
                    Inactive = false;
                Enumerator++;

                if (description != null)
                {
                    if (caller != null)
                    {
                        if (Descriptions.ContainsKey(caller))
                            Descriptions[caller] = Descriptions[caller] + "; " + description;
                        else Descriptions.Add(caller, description);

                        Description = string.Join("; ", Descriptions.Select(x => x.Value).ToArray());
                    }
                    else Description = description;
                }
            }
        }

        /// <summary>
        /// Закрытие асинхронной задачи
        /// </summary>
        public void Close([CallerMemberName] string caller = null)
        {
            lock (this)
            {
                Enumerator--;

                if (caller != null)
                {
                    if (Descriptions.ContainsKey(caller))
                        Descriptions.Remove(caller);
                }

                if (Enumerator == 0)
                {
                    Inactive = true;
                    Descriptions.Clear();
                }

                Description = string.Join("; ", Descriptions.Select(x => x.Value).ToArray());
            }
        }

        /// <summary>
        /// Инициализирует Progress
        /// </summary>
        /// <param name="value">Текущее значение</param>
        /// <param name="maximum">Максимальное значение</param>
        public void InitializeProgress(int value, int maximum)
        {
            Value = value;
            Maximum = maximum;
        }

        /// <summary>
        /// Создаёт новую задачу
        /// </summary>
        /// <param name="action">Задача</param>
        /// <param name="description">Описание</param>
        /// <returns></returns>
        public async Task NewTask(Action action, string description)
        {
            Open(description);
            await Task.Run(action);
            Close();
        }

        #endregion
    }
}