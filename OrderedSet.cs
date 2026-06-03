using BotControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SlideDrum
{
    //public class NewOrderedSet<T> : IEnumerable<T>, IEnumerable
    //{
    //    private readonly List<T> _list = new();
    //    private readonly List<int?> _priorities = new();
    //    private readonly Dictionary<T, int> _dict = new();

    //    public int Count => _list.Count;
    //    /// <summary>
    //    /// Implicitly converts a <see cref="List{T}"/> to an <see cref="OrderedSet{T}"/>,
    //    /// preserving insertion order. Duplicate items are silently skipped.
    //    /// </summary>
    //    public static implicit operator OrderedSet<T>(List<T> list)
    //    {
    //        var set = new OrderedSet<T>();
    //        foreach (var item in list)
    //            set.Add(item);
    //        return set;
    //    }
    //    /// <summary>
    //    /// Implicitly converts a <see cref="HashSet{T}"/> to an <see cref="OrderedSet{T}"/>.
    //    /// Note: <see cref="HashSet{T}"/> has no guaranteed iteration order, so the resulting
    //    /// order is not deterministic.
    //    /// </summary>
    //    public static implicit operator OrderedSet<T>(HashSet<T> hashSet)
    //    {
    //        var set = new OrderedSet<T>();
    //        foreach (var item in hashSet)
    //            set.Add(item);
    //        return set;
    //    }
    //    /// <summary>
    //    /// Updates the priority of an item already in the set. If <paramref name="priority"/> has a value,
    //    /// the item is repositioned to maintain correct priority order. If null is passed, the priority is
    //    /// stripped in-place without moving the item.
    //    /// Throws <see cref="ArgumentException"/> if the item is not present.
    //    /// </summary>
    //    public void SetPriority(T item, int? priority)
    //    {
    //        if (!_dict.ContainsKey(item))
    //            throw new ArgumentException("Item is not present in the set.", nameof(item));

    //        if (priority.HasValue)
    //        {
    //            Remove(item);
    //            Add(item, priority: priority);
    //        }
    //        else
    //        {
    //            _priorities[_dict[item]] = null;
    //        }
    //    }

    //    /// <summary>
    //    /// Returns the index of the item in the set, or -1 if not present.
    //    /// </summary>
    //    public int IndexOf(T item)
    //    {
    //        return _dict.TryGetValue(item, out int index) ? index : -1;
    //    }

    //    /// <summary>
    //    /// Removes the item at the given index.
    //    /// Throws <see cref="ArgumentOutOfRangeException"/> if the index is out of range.
    //    /// </summary>
    //    public void RemoveAt(int index)
    //    {
    //        if (index < 0 || index >= _list.Count)
    //            throw new ArgumentOutOfRangeException(nameof(index));

    //        Remove(_list[index]);
    //    }

    //    /// <summary>
    //    /// Removes and returns the item at the front of the set without throwing if empty.
    //    /// Returns true and sets <paramref name="item"/> if successful; returns false if the set is empty.
    //    /// </summary>
    //    public bool TryDequeue(out T item)
    //    {
    //        if (_list.Count == 0)
    //        {
    //            item = default;
    //            return false;
    //        }

    //        item = Dequeue();
    //        return true;
    //    }

    //    /// <summary>
    //    /// Returns the item at the front of the set without removing it, without throwing if empty.
    //    /// Returns true and sets <paramref name="item"/> if successful; returns false if the set is empty.
    //    /// </summary>
    //    public bool TryPeek(out T item)
    //    {
    //        if (_list.Count == 0)
    //        {
    //            item = default;
    //            return false;
    //        }

    //        item = Peek();
    //        return true;
    //    }

    //    /// <summary>
    //    /// Adds a range of items to the end of the set, skipping any duplicates silently.
    //    /// </summary>
    //    public void AddRange(IEnumerable<T> items)
    //    {
    //        foreach (var item in items)
    //            Add(item);
    //    }

    //    /// <summary>
    //    /// Shifts all priority values by the given amount. Items without a priority are unaffected.
    //    /// Used internally to make room at the front or back when no valid priority slot exists.
    //    /// </summary>
    //    private void ShiftAllPriorities(int amount)
    //    {
    //        for (int i = 0; i < _priorities.Count; i++)
    //        {
    //            if (_priorities[i].HasValue)
    //                _priorities[i] = _priorities[i].Value + amount;
    //        }
    //    }

    //    /// <summary>
    //    /// Moves an existing item to the front of the set. If the item has a priority, it is updated
    //    /// to one less than the current front priority. If that would underflow <see cref="int.MinValue"/>,
    //    /// all other priorities are shifted up by one to make room.
    //    /// Throws <see cref="ArgumentException"/> if the item is not present.
    //    /// </summary>
    //    public void MoveToFront(T item)
    //    {
    //        if (!_dict.ContainsKey(item))
    //            throw new ArgumentException("Item is not present in the set.", nameof(item));

    //        bool hasPriority = _priorities[_dict[item]].HasValue;
    //        Remove(item);

    //        int? newPriority = null;
    //        if (hasPriority)
    //        {
    //            int? frontPriority = _priorities.FirstOrDefault(p => p.HasValue);
    //            if (!frontPriority.HasValue)
    //                newPriority = 0;
    //            else if (frontPriority.Value > int.MinValue)
    //                newPriority = frontPriority.Value - 1;
    //            else
    //            {
    //                ShiftAllPriorities(1);
    //                newPriority = int.MinValue;
    //            }
    //        }

    //        Add(item, first: !hasPriority, priority: newPriority);
    //    }

    //    /// <summary>
    //    /// Moves an existing item to the back of the set. If the item has a priority, it is updated
    //    /// to one more than the current back priority. If that would overflow <see cref="int.MaxValue"/>,
    //    /// all other priorities are shifted down by one to make room.
    //    /// Throws <see cref="ArgumentException"/> if the item is not present.
    //    /// </summary>
    //    public void MoveToBack(T item)
    //    {
    //        if (!_dict.ContainsKey(item))
    //            throw new ArgumentException("Item is not present in the set.", nameof(item));

    //        bool hasPriority = _priorities[_dict[item]].HasValue;
    //        Remove(item);

    //        int? newPriority = null;
    //        if (hasPriority)
    //        {
    //            int? backPriority = _priorities.LastOrDefault(p => p.HasValue);
    //            if (!backPriority.HasValue)
    //                newPriority = 0;
    //            else if (backPriority.Value < int.MaxValue)
    //                newPriority = backPriority.Value + 1;
    //            else
    //            {
    //                ShiftAllPriorities(-1);
    //                newPriority = int.MaxValue;
    //            }
    //        }

    //        Add(item);
    //        if (newPriority.HasValue)
    //            _priorities[_list.Count - 1] = newPriority;
    //    }

    //    /// <summary>
    //    /// Implicitly converts an <see cref="OrderedSet{T}"/> to a <see cref="List{T}"/>,
    //    /// preserving the current order. Equivalent to calling <see cref="ToList"/>.
    //    /// </summary>
    //    public static implicit operator List<T>(OrderedSet<T> set) => set.ToList();

    //    /// <summary>
    //    /// Implicitly converts an <see cref="OrderedSet{T}"/> to a <see cref="HashSet{T}"/>.
    //    /// Order is not preserved in the resulting set.
    //    /// </summary>
    //    public static implicit operator HashSet<T>(OrderedSet<T> set) => new(set._list);

    //    /// <summary>
    //    /// Sorts items in-place by the given key selector. After sorting, any priority values
    //    /// that conflict with the new order are nudged forward minimally (to previous + 1)
    //    /// to restore consistency. Non-priority items are skipped during repair but do not
    //    /// reset the conflict check. Items without a priority are never assigned one.
    //    /// </summary>
    //    public void Sort<TKey>(Func<T, TKey> keySelector) where TKey : IComparable<TKey>
    //    {
    //        var sorted = _list
    //            .Select((item, i) => (item, priority: _priorities[i], i))
    //            .OrderBy(x => keySelector(x.item))
    //            .ToList();

    //        for (int i = 0; i < sorted.Count; i++)
    //        {
    //            _list[i] = sorted[i].item;
    //            _priorities[i] = sorted[i].priority;
    //        }

    //        int? lastPriority = null;
    //        for (int i = 0; i < _priorities.Count; i++)
    //        {
    //            if (!_priorities[i].HasValue)
    //                continue; // skip but do NOT reset lastPriority

    //            if (lastPriority.HasValue && _priorities[i].Value <= lastPriority.Value)
    //                _priorities[i] = lastPriority.Value + 1;

    //            lastPriority = _priorities[i];
    //        }

    //        RebuildIndices(0);
    //    }
    //    /// <summary>
    //    /// Adds an item to the set. If <paramref name="first"/> is true, inserts at the front;
    //    /// otherwise appends to the end (default behaviour).
    //    /// If <paramref name="priority"/> is provided, the item is inserted at the correct position
    //    /// to maintain priority order (lower value = earlier in list).
    //    /// Returns false if the item is already present.
    //    /// </summary>
    //    public bool Add(T item, bool first = false, int? priority = null)
    //    {
    //        if (_dict.ContainsKey(item))
    //            return false;

    //        if (priority.HasValue)
    //        {
    //            int insertIndex = _list.Count;
    //            for (int i = 0; i < _priorities.Count; i++)
    //            {
    //                if (_priorities[i].HasValue && priority.Value < _priorities[i].Value)
    //                {
    //                    insertIndex = i;
    //                    break;
    //                }
    //            }
    //            _list.Insert(insertIndex, item);
    //            _priorities.Insert(insertIndex, priority);
    //            RebuildIndices(insertIndex);
    //            return true;
    //        }

    //        if (first)
    //        {
    //            _list.Insert(0, item);
    //            _priorities.Insert(0, null);
    //            RebuildIndices(0);
    //        }
    //        else
    //        {
    //            _list.Add(item);
    //            _priorities.Add(null);
    //            _dict[item] = _list.Count - 1;
    //        }

    //        return true;
    //    }
    //    /// <summary>
    //    /// Removes the item from the set. Returns false if the item was not present.
    //    /// </summary>
    //    public bool Remove(T item)
    //    {
    //        if (!_dict.TryGetValue(item, out int index))
    //            return false;

    //        _dict.Remove(item);
    //        _list.RemoveAt(index);
    //        _priorities.RemoveAt(index);
    //        RebuildIndices(index);

    //        return true;
    //    }
    //    /// <summary>
    //    /// Removes all items, priorities, and index mappings from the set.
    //    /// </summary>
    //    public void Clear()
    //    {
    //        _list.Clear();
    //        _priorities.Clear();
    //        _dict.Clear();
    //    }

    //    /// <summary>
    //    /// Adds an item to the end of the queue if not already present.
    //    /// </summary>
    //    public bool Enqueue(T item, int? priority = null)
    //    {
    //        return Add(item, priority: priority);
    //    }

    //    /// <summary>
    //    /// Inserts <paramref name="newItem"/> immediately before or after <paramref name="currentItem"/>.
    //    /// If <paramref name="priority"/> is provided and the requested position would violate priority
    //    /// ordering, a warning is logged and the item is placed at the priority-correct position instead.
    //    /// Does nothing and returns false if <paramref name="newItem"/> is already in the set.
    //    /// Throws <see cref="ArgumentException"/> if <paramref name="currentItem"/> is not in the set.
    //    /// </summary>
    //    public bool Insert(T newItem, T currentItem, bool before, int? priority = null)
    //    {
    //        if (!_dict.TryGetValue(currentItem, out int anchorIndex))
    //            throw new ArgumentException("currentItem is not present in the set.", nameof(currentItem));
    //        if (_dict.ContainsKey(newItem))
    //            return false;

    //        int requestedIndex = before ? anchorIndex : anchorIndex + 1;

    //        if (priority.HasValue)
    //        {
    //            bool orderViolated = false;

    //            if (requestedIndex > 0 && _priorities[requestedIndex - 1].HasValue && priority.Value > _priorities[requestedIndex - 1].Value)
    //                orderViolated = true;
    //            if (requestedIndex < _priorities.Count && _priorities[requestedIndex].HasValue && priority.Value < _priorities[requestedIndex].Value)
    //                orderViolated = true;

    //            if (orderViolated)
    //            {
    //                ZiMain.log.LogWarning($"OrderedSet.Insert: priority {priority.Value} would break ordering at requested index {requestedIndex}. Inserting at priority-correct position instead.");
    //                return Add(newItem, priority: priority);
    //            }
    //        }

    //        _list.Insert(requestedIndex, newItem);
    //        _priorities.Insert(requestedIndex, priority);
    //        RebuildIndices(requestedIndex);

    //        return true;
    //    }

    //    /// <summary>
    //    /// Removes and returns the item at the front of the queue.
    //    /// Throws InvalidOperationException if empty.
    //    /// </summary>
    //    public T Dequeue()
    //    {
    //        if (_list.Count == 0)
    //            throw new InvalidOperationException("The OrderedSet is empty.");

    //        T item = _list[0];
    //        Remove(item);
    //        return item;
    //    }

    //    /// <summary>
    //    /// Returns (but does not remove) the item at the front of the queue.
    //    /// </summary>
    //    public T Peek()
    //    {
    //        if (_list.Count == 0)
    //            throw new InvalidOperationException("The OrderedSet is empty.");

    //        return _list[0];
    //    }
    //    /// <summary>
    //    /// Returns true if the item is present in the set.
    //    /// </summary>
    //    public bool Contains(T item) => _dict.ContainsKey(item);
    //    /// <summary>
    //    /// Returns the item at the given index. Throws <see cref="ArgumentOutOfRangeException"/> if out of range.
    //    /// </summary>
    //    public T this[int index] => _list[index];
    //    /// <summary>
    //    /// Returns the priority assigned to the item, or null if it has none.
    //    /// Throws <see cref="ArgumentException"/> if the item is not present in the set.
    //    /// </summary>
    //    public int? GetPriority(T item)
    //    {
    //        if (!_dict.TryGetValue(item, out int index))
    //            throw new ArgumentException("Item is not present in the set.", nameof(item));
    //        return _priorities[index];
    //    }

    //    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
    //    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
    //    /// <summary>
    //    /// Returns a shallow copy of the internal list as a <see cref="List{T}"/>.
    //    /// </summary>
    //    public List<T> ToList() => new(_list);

    //    private void RebuildIndices(int fromIndex)
    //    {
    //        for (int i = fromIndex; i < _list.Count; i++)
    //            _dict[_list[i]] = i;
    //    }
    //}

}
