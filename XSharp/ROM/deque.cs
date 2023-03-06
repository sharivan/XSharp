using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace XSharp.ROM;

[Serializable]
[ComVisible(false)]
public class Deque<T> : IEnumerable<T>, ICollection, IEnumerable
{
    // fields

    private List<T> front;
    private List<T> back;
    private int frontDeleted;
    private int backDeleted;

    // properties

    public int Capacity => front.Capacity + back.Capacity;

    public int Count => front.Count + back.Count - frontDeleted - backDeleted;

    public bool IsEmpty => Count == 0;

    public IEnumerable<T> Reversed
    {
        get
        {
            if (back.Count - backDeleted > 0)
            {
                for (int i = back.Count - 1; i >= backDeleted; i--)
                    yield return back[i];
            }

            if (front.Count - frontDeleted > 0)
            {
                for (int i = frontDeleted; i < front.Count; i++)
                    yield return front[i];
            }
        }
    }

    // constructors

    public Deque()
    {
        front = new List<T>();
        back = new List<T>();
    }

    public Deque(int capacity)
    {
        if (capacity < 0)
            throw new ArgumentException("Capacity cannot be negative");
        int temp = capacity / 2;
        int temp2 = capacity - temp;
        front = new List<T>(temp);
        back = new List<T>(temp2);
    }

    public Deque(IEnumerable<T> backCollection) : this(backCollection, null)
    {
    }

    public Deque(IEnumerable<T> backCollection, IEnumerable<T> frontCollection)
    {
        if (backCollection == null && frontCollection == null)
            throw new ArgumentException("Collections cannot both be null");
        front = new List<T>();
        back = new List<T>();

        if (backCollection != null)
        {
            foreach (T item in backCollection)
                back.Add(item);
        }

        if (frontCollection != null)
        {
            foreach (T item in frontCollection)
                front.Add(item);
        }
    }

    // methods

    public void AddFirst(T item)
    {
        if (frontDeleted > 0 && front.Count == front.Capacity)
        {
            front.RemoveRange(0, frontDeleted);
            frontDeleted = 0;
        }

        front.Add(item);
    }

    public void AddLast(T item)
    {
        if (backDeleted > 0 && back.Count == back.Capacity)
        {
            back.RemoveRange(0, backDeleted);
            backDeleted = 0;
        }

        back.Add(item);
    }

    public void AddRangeFirst(IEnumerable<T> range)
    {
        if (range != null)
        {
            foreach (T item in range)
                AddFirst(item);
        }
    }

    public void AddRangeLast(IEnumerable<T> range)
    {
        if (range != null)
        {
            foreach (T item in range)
                AddLast(item);
        }
    }

    public void Clear()
    {
        front.Clear();
        back.Clear();
        frontDeleted = 0;
        backDeleted = 0;
    }

    public bool Contains(T item)
    {
        for (int i = frontDeleted; i < front.Count; i++)
        {
            if (Equals(front[i], item))
                return true;
        }

        for (int i = backDeleted; i < back.Count; i++)
        {
            if (Equals(back[i], item))
                return true;
        }

        return false;
    }

    public void CopyTo(T[] array, int index)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));

        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (array.Length < index + Count)
            throw new ArgumentException("Index is invalid");

        int i = index;

        foreach (T item in this)
        {
            array[i++] = item;
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        if (front.Count - frontDeleted > 0)
        {
            for (int i = front.Count - 1; i >= frontDeleted; i--)
                yield return front[i];
        }

        if (back.Count - backDeleted > 0)
        {
            for (int i = backDeleted; i < back.Count; i++)
                yield return back[i];
        }
    }

    public T PeekFirst()
    {
        return front.Count > frontDeleted
            ? front[^1]
            : back.Count > backDeleted ? back[backDeleted] : throw new InvalidOperationException("Can't peek at empty Deque");
    }

    public T PeekLast()
    {
        return back.Count > backDeleted
            ? back[^1]
            : front.Count > frontDeleted ? front[frontDeleted] : throw new InvalidOperationException("Can't peek at empty Deque");
    }

    public T PopFirst()
    {
        T result;

        if (front.Count > frontDeleted)
        {
            result = front[^1];
            front.RemoveAt(front.Count - 1);
        }
        else if (back.Count > backDeleted)
        {
            result = back[backDeleted];
            backDeleted++;
        }
        else
        {
            throw new InvalidOperationException("Can't pop empty Deque");
        }

        return result;
    }

    public T PopLast()
    {
        T result;

        if (back.Count > backDeleted)
        {
            result = back[^1];
            back.RemoveAt(back.Count - 1);
        }
        else if (front.Count > frontDeleted)
        {
            result = front[frontDeleted];
            frontDeleted++;
        }
        else
        {
            throw new InvalidOperationException("Can't pop empty Deque");
        }

        return result;
    }

    public void Reverse()
    {
        (back, front) = (front, back);
        (backDeleted, frontDeleted) = (frontDeleted, backDeleted);
    }

    public T[] ToArray()
    {
        if (Count == 0)
            return Array.Empty<T>();

        var result = new T[Count];
        CopyTo(result, 0);
        return result;
    }

    public void TrimExcess()
    {
        if (frontDeleted > 0)
        {
            front.RemoveRange(0, frontDeleted);
            frontDeleted = 0;
        }

        if (backDeleted > 0)
        {
            back.RemoveRange(0, backDeleted);
            backDeleted = 0;
        }

        front.TrimExcess();
        back.TrimExcess();
    }

    public bool TryPeekFirst(out T item)
    {
        if (!IsEmpty)
        {
            item = PeekFirst();
            return true;
        }

        item = default;
        return false;
    }

    public bool TryPeekLast(out T item)
    {
        if (!IsEmpty)
        {
            item = PeekLast();
            return true;
        }

        item = default;
        return false;
    }

    public bool TryPopFirst(out T item)
    {
        if (!IsEmpty)
        {
            item = PopFirst();
            return true;
        }

        item = default;
        return false;
    }

    public bool TryPopLast(out T item)
    {
        if (!IsEmpty)
        {
            item = PopLast();
            return true;
        }

        item = default;
        return false;
    }

    // explicit property implementations

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => this;

    // explicit method implementations

    void ICollection.CopyTo(Array array, int index)
    {
        CopyTo((T[]) array, index);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

class DequeTest
{
    static void Main()
    {
        int[] arrFront = { 5, 4, 3, 2, 1 };
        int[] arrBack = { 6, 7, 8, 9, 10 };

        // create new Deque using these arrays
        var d = new Deque<int>(arrBack, arrFront);

        // iterate from first to last
        Console.Write("The Deque contains  : ");
        foreach (int i in d)
            Console.Write("{0} ", i); // 1 to 10
        Console.WriteLine();

        // iterate from last to first
        Console.Write("Or in reverse order : ");
        foreach (int i in d.Reversed)
            Console.Write("{0} ", i); // 10 to 1
        Console.WriteLine();

        // permanently reverse the order of the items
        d.Reverse();

        // iterate from first to last again
        Console.Write("After permanent reversal : ");
        foreach (int i in d)
            Console.Write("{0} ", i); // 10 to 1
        Console.WriteLine();

        // add items at front
        Console.WriteLine("Added 11 and 12 at the front");
        d.AddRangeFirst(new int[] { 11, 12 });

        // add item at back
        Console.WriteLine("Added 0 at the back");
        d.AddLast(0);

        Console.WriteLine("The first item is : {0}", d.PeekFirst()); // 12
        if (d.TryPeekLast(out int num))
        {
            Console.WriteLine("The last item is : {0}", num); // 0 
        }

        // pop last item
        Console.WriteLine("Popped last item");
        num = d.PopLast();

        // pop first item
        Console.WriteLine("Popped first item");
        d.TryPopFirst(out num);

        if (d.Contains(11))
        {
            // iterate again
            Console.Write("The Deque now contains : ");
            foreach (int i in d)
                Console.Write("{0} ", i); // 11 to 1
            Console.WriteLine();
        }

        // peek at last item
        Console.WriteLine("The last item is : {0}", d.PeekLast());  // 1 

        // count items
        Console.WriteLine("The number of items is : {0}", d.Count); // 11

        // convert to an array
        int[] ia = d.ToArray();

        // reload to a new Deque adding all items at front so they'll now be reversed       
        d = new Deque<int>(null, ia);
        Console.Write("The new Deque contains : ");
        foreach (int i in d)
            Console.Write("{0} ", i); // 1 to 11

        Console.WriteLine("\nThe capacity is : {0}", d.Capacity);
        d.TrimExcess();
        Console.WriteLine("After trimming the capacity is now : {0}", d.Capacity);

        // copy to an existing array
        ia = new int[d.Count];
        d.CopyTo(ia, 0);

        // clear the Deque (No pun intended!)
        d.Clear();
        Console.WriteLine("After clearing the Deque is now empty : {0}", d.IsEmpty);
        Console.WriteLine("The third element used to be : {0}", ia[2]);

        Console.ReadKey();
    }
}