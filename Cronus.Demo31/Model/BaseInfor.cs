using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Cronus.Demo
{
    /// <summary>
    /// Base infor
    /// </summary>
    internal class BaseInfor: INotifyPropertyChanged
    {
        /// <summary>
        /// Property changed event handler
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        /// <summary>
        /// Notify property changed
        /// </summary>
        /// <param name="info"></param>
        protected void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }

    /// <summary>
    /// Obervable extensions: sort
    /// </summary>
    public static class ObervableExtensions
    {
        /// <summary>
        /// Sort
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        public static void Sort<T>(this ObservableCollection<T> collection) where T : IComparable<T>
        {
            var sortedList = collection.OrderBy(x => x).ToList();
            for (int i = 0; i < sortedList.Count; i++)
            {
                collection.Move(collection.IndexOf(sortedList[i]), i);
            }
        }
    }
}
