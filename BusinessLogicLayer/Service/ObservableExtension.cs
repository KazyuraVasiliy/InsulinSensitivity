using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq;

namespace BusinessLogicLayer.Service
{
    public static class ObservableExtension
    {
        /// <summary>
        /// Создает ObservableCollection<T> из IEnumerable<T>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static ObservableCollection<T> ToObservable<T>(this IEnumerable<T> collection) =>
            new ObservableCollection<T>(collection);

        /// <summary>
        /// Создает ObservableCollection<T> из IQueryable<T>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static ObservableCollection<T> ToObservable<T>(this IQueryable<T> collection) =>
            new ObservableCollection<T>(collection);
    }
}
