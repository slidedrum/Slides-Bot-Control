using System;
using System.Collections;
using System.Collections.Generic;

namespace SlideDrum
{
    public class OrderedSet<T> : IEnumerable<T>, IEnumerable
    {
        //This didn't exist for some reason, so I had an AI make it.  I mostly understand it.  
        //TODO remake it not with AI.

        private readonly List<T> _list = new();
#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
        private readonly Dictionary<T, int> _dict = new();
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.

        public int Count => _list.Count;

        /// <summary>
        /// Adds an item to the set. If <paramref name="first"/> is true, inserts at the front;
        /// otherwise appends to the end (default behaviour).
        /// Returns false if the item is already present.
        /// </summary>
        public bool Add(T item, bool first = false)
        {
            if (_dict.ContainsKey(item))
                return false;

            if (first)
            {
                _list.Insert(0, item);
                for (int i = 0; i < _list.Count; i++)
                    _dict[_list[i]] = i;
            }
            else
            {
                _list.Add(item);
                _dict[item] = _list.Count - 1;
            }

            return true;
        }

        public bool Remove(T item)
        {
            if (!_dict.TryGetValue(item, out int index))
                return false;

            _dict.Remove(item);
            _list.RemoveAt(index);

            // Rebuild indices for every item that shifted left.
            for (int i = index; i < _list.Count; i++)
                _dict[_list[i]] = i;

            return true;
        }

        public void Clear()
        {
            _list.Clear();
            _dict.Clear();
        }
        /// <summary>
        /// Adds an item to the end of the queue if not already present.
        /// </summary>
        public bool Enqueue(T item)
        {
            return Add(item);
        }
        /// <summary>
        /// Inserts <paramref name="newItem"/> immediately before or after <paramref name="currentItem"/>.
        /// Does nothing and returns false if <paramref name="newItem"/> is already in the set.
        /// Throws <see cref="ArgumentException"/> if <paramref name="currentItem"/> is not in the set.
        /// Throws <see cref="ArgumentException"/> if <paramref name="newItem"/> equals <paramref name="currentItem"/>.
        /// </summary>
        public bool Insert(T newItem, T currentItem, bool before)
        {
            if (!_dict.TryGetValue(currentItem, out int anchorIndex))
                throw new ArgumentException("currentItem is not present in the set.", nameof(currentItem));
            if (_dict.ContainsKey(newItem))
                return false;

            int insertIndex = before ? anchorIndex : anchorIndex + 1;
            _list.Insert(insertIndex, newItem);

            // Rebuild every index at or after the insertion point.
            for (int i = insertIndex; i < _list.Count; i++)
                _dict[_list[i]] = i;

            return true;
        }
        /// <summary>
        /// Removes and returns the item at the front of the queue.
        /// Throws InvalidOperationException if empty.
        /// </summary>
        public T Dequeue()
        {
            if (_list.Count == 0)
                throw new InvalidOperationException("The OrderedSet is empty.");

            T item = _list[0];
            Remove(item);
            return item;
        }

        /// <summary>
        /// Returns (but does not remove) the item at the front of the queue.
        /// </summary>
        public T Peek()
        {
            if (_list.Count == 0)
                throw new InvalidOperationException("The OrderedSet is empty.");

            return _list[0];
        }
        public bool Contains(T item) => _dict.ContainsKey(item);

        public T this[int index] => _list[index];

        // Generic enumerator
        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        // Non-generic enumerator
        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

        public List<T> ToList() => new(_list);
    }
}
