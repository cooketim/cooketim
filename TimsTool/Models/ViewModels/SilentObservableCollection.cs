using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Models.ViewModels
{
    /// <summary>
    /// SilentObservableCollection is a ObservableCollection with some extensions.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SilentObservableCollection<T> :ObservableCollection<T>
    {
        public SilentObservableCollection(IEnumerable<T> enumerable) : base(enumerable) { }
        public SilentObservableCollection() : base() { }

        /// <summary>
        /// Adds a range of items to the observable collection.
        /// Instead of iterating through all elements and adding them
        /// one by one (which causes OnPropertyChanged events), all
        /// the items gets added instantly without firing events.
        /// After adding all elements, the OnPropertyChanged event will be fired.
        /// </summary>
        /// <param name="enumerable"></param>
        public void AddRange(IEnumerable<T> enumerable)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");

            CheckReentrancy();

            int startIndex = Count;

            foreach (T item in enumerable) Items.Add(item);

            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void Reset(IEnumerable<T> enumerable)
        {
            Items.Clear();

            AddRange(enumerable);
        }

        public void AddSorted(T item, IComparer<T> comparer = null)
        {
            if (comparer == null)
                comparer = Comparer<T>.Default;

            int i = 0;
            while (i < Items.Count && comparer.Compare(Items[i], item) < 0)
                i++;
            InsertItem(i, item);
        }

        public int RemoveAll(Func<T, bool> condition)
        {
            var itemsToRemove = this.Where(condition).ToList();

            foreach (var itemToRemove in itemsToRemove)
            {
                Remove(itemToRemove);
            }

            return itemsToRemove.Count;
        }
    }
}
