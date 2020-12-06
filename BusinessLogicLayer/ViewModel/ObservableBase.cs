using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BusinessLogicLayer.ViewModel
{
    [Serializable]
    public abstract class ObservableBase : INotifyPropertyChanged
    {
        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName]string prop = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        /// <summary>
        /// Примитивы, для упрощения асинхронной работы
        /// </summary>
        public AsyncBase AsyncBase { get; set; } = 
            new AsyncBase();
    }
}