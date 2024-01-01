using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BusinessLogicLayer.ViewModel
{
    public class Grouping<K, T> : ObservableCollection<T>
    {
        /// <summary>
        /// Ключ
        /// </summary>
        public K Name { get; private set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="name">Ключ</param>
        /// <param name="items">Элементы</param>
        public Grouping(K name, IEnumerable<T> items)
        {
            Name = name;
            foreach (T item in items)
                Items.Add(item);
        }
    }
}
