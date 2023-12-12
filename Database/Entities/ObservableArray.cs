using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Database.Entities
{
    public class ObservableArray<T> : IEnumerable<T>, INotifyPropertyChanged
    {
        private T[] array;

        public ObservableArray(int size)
        {
            array = new T[size];
        }

        public ObservableArray(IEnumerable<T> collection)
        {
            // Convert the IEnumerable to an array
            array = collection.ToArray();
        }

        public int Length => array.Length;

        public T this[int index]
        {
            get => array[index]; 
            set 
            { 
                array[index] = value;
                OnPropertyChanged($"Item[{index}]");
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)array).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
