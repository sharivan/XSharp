using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XSharp.Serialization;
using XSharp.Util;

namespace XSharp.Engine.Entities;

public class EntitySet<T> : ISet<T>, IReadOnlySet<T>, ISerializable where T : Entity
{
    private class EntitySetEnumerator : IEnumerator<T>
    {
        private EntitySet<T> set;
        private IEnumerator<int> bitSetEnumerator;

        int CurrentIndex => bitSetEnumerator.Current;

        public T Current => CurrentIndex >= 0 ? set[CurrentIndex] : null;

        object IEnumerator.Current => Current;

        internal EntitySetEnumerator(EntitySet<T> set)
        {
            this.set = set;
            bitSetEnumerator = set.bitSet.GetEnumerator();
        }

        public void Dispose()
        {
            set = null;
            bitSetEnumerator.Dispose();
            GC.SuppressFinalize(this);
        }

        public bool MoveNext()
        {
            bitSetEnumerator.MoveNext();
            while (Current == null && CurrentIndex >= 0)
                bitSetEnumerator.MoveNext();

            return Current != null;
        }

        public void Reset()
        {
            bitSetEnumerator.Reset();
        }
    }

    private BitSet bitSet;

    public int Count => bitSet.Count;

    public bool IsReadOnly => false;

    public T this[int index] => bitSet[index] ? (T) GameEngine.Engine.Entities[index] : null;

    public EntitySet()
    {
        bitSet = new BitSet();
    }

    public EntitySet(params T[] entities) : this()
    {
        AddRange(entities);
    }

    public EntitySet(EntitySet<T> entities) : this()
    {
        AddRange(entities);
    }

    public EntitySet(BitSet bitSet) : this()
    {
        AddRange(bitSet);
    }

    public int TryGetFirstIndexNotNull()
    {
        return TryGetNextIndexNotNull(0);
    }

    public int TryGetNextIndexNotNull(int start = 0)
    {
        return bitSet.FirstSetBit(start);
    }

    public Entity TryGetFirstNotNull()
    {
        return TryGetNextNotNull(0);
    }

    public Entity TryGetNextNotNull(int start = 0)
    {
        int index = TryGetNextIndexNotNull(start);
        return index >= 0 ? this[index] : (Entity) null;
    }

    public bool TestAndAdd(T entity)
    {
        return bitSet.Set(entity.Index);
    }

    public bool Remove(T entity)
    {
        return Remove(entity.Index);
    }

    public bool Remove(int index)
    {
        return bitSet.Reset(index);
    }

    public void RemoveRange(params T[] entities)
    {
        if (entities != null && entities.Length > 0)
        {
            foreach (var entity in entities)
                Remove(entity);
        }
    }

    public bool Contains(T entity)
    {
        return bitSet[entity.Index];
    }

    public bool ContainsAtLeast(int minCount, params T[] entities)
    {
        int count = 0;
        if (entities != null && entities.Length > 0)
        {
            foreach (var entity in entities)
            {
                if (Contains(entity))
                {
                    count++;
                    if (count == minCount)
                        return true;
                }
            }
        }

        return false;
    }

    public bool ContainsOnly(int exactCount, params T[] entities)
    {
        int count = 0;
        if (entities != null && entities.Length > 0)
        {
            foreach (var entity in entities)
            {
                if (Contains(entity))
                {
                    count++;
                    if (count > exactCount)
                        return false;
                }
            }
        }

        return count == exactCount;
    }

    public bool ContainsUpTo(int maxCount, params T[] entities)
    {
        int count = 0;
        if (entities != null && entities.Length > 0)
        {
            foreach (var entity in entities)
            {
                if (Contains(entity))
                {
                    count++;
                    if (count > maxCount)
                        return false;
                }
            }
        }

        return count <= maxCount;
    }

    public bool ContainsAll(params T[] entities)
    {
        if (entities != null && entities.Length > 0)
        {
            foreach (var entity in entities)
            {
                if (!Contains(entity))
                    return false;
            }
        }

        return true;
    }

    public bool ContainsAtLeastOne(params T[] entities)
    {
        if (entities != null && entities.Length > 0)
        {
            foreach (var entity in entities)
            {
                if (Contains(entity))
                    return true;
            }
        }

        return false;
    }

    public bool ContainsOnlyOne(params T[] entities)
    {
        bool containsOne = false;
        if (entities != null && entities.Length > 0)
        {
            foreach (var entity in entities)
            {
                if (Contains(entity))
                {
                    if (containsOne)
                        return false;

                    containsOne = true;
                }
            }
        }

        return containsOne;
    }

    public int CountOf(params T[] entities)
    {
        int result = 0;
        if (entities != null && entities.Length > 0)
        {
            foreach (var entity in entities)
            {
                if (Contains(entity))
                    result++;
            }
        }

        return result;
    }

    public bool Add(T item)
    {
        return TestAndAdd(item);
    }

    public void AddRange(params T[] entities)
    {
        if (entities != null && entities.Length > 0)
        {
            foreach (var entity in entities)
                Add(entity);
        }
    }

    public void AddRange(EntitySet<T> entities)
    {
        AddRange(entities.bitSet);
    }

    public void AddRange(BitSet mask)
    {
        bitSet.UnionWith(mask);
    }

    public void Clear()
    {
        bitSet.Clear();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return new EntitySetEnumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new EntitySetEnumerator(this);
    }

    void ICollection<T>.CopyTo(T[] array, int arrayIndex)
    {
        for (int index = bitSet.FirstSetBit(0); index >= 0 && arrayIndex < array.Length; index = bitSet.FirstSetBit(index), arrayIndex++)
            array[arrayIndex] = this[index];
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append('[');

        bool first = true;
        foreach (var entity in this)
        {
            if (first)
                first = false;
            else
                builder.Append(", ");

            builder.Append(entity);
        }

        builder.Append(']');
        return builder.ToString();
    }

    public void Deserialize(ISerializer reader)
    {
        bitSet ??= new BitSet();
        bitSet.Deserialize(reader);
    }

    public void Serialize(ISerializer writer)
    {
        bitSet.Serialize(writer);
    }

    public void ExceptWith(BitSet bitSet)
    {
        this.bitSet.ExceptWith(bitSet);
    }

    public void ExceptWith(EntitySet<T> other)
    {
        ExceptWith(other.bitSet);
    }

    public void ExceptWith(IEnumerable<T> other)
    {
        if (other is EntitySet<T> set)
        {
            ExceptWith(set);
        }
        else
        {
            foreach (var entity in other)
                Remove(entity);
        }
    }

    public void IntersectWith(BitSet mask)
    {
        bitSet.IntersectWith(mask);
    }

    public void IntersectWith(EntitySet<T> other)
    {
        IntersectWith(other.bitSet);
    }

    public void IntersectWith(IEnumerable<T> other)
    {
        if (other is EntitySet<T> set)
        {
            IntersectWith(set);
        }
        else
        {
            foreach (var entity in this)
            {
                if (!other.Contains(entity))
                    Remove(entity);
            }
        }
    }

    public bool IsProperSubsetOf(BitSet mask)
    {
        return bitSet.IsProperSubsetOf(mask);
    }

    public bool IsProperSubsetOf(EntitySet<T> other)
    {
        return IsProperSubsetOf(other.bitSet);
    }

    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
        if (other is EntitySet<T> set)
            return IsProperSubsetOf(set);

        int count = 0;
        foreach (var entity in this)
        {
            if (!other.Contains(entity))
                return false;

            count++;
        }

        return count < other.Count();
    }

    public bool IsProperSupersetOf(BitSet mask)
    {
        return mask.IsProperSubsetOf(bitSet);
    }

    public bool IsProperSupersetOf(EntitySet<T> other)
    {
        return IsProperSupersetOf(other);
    }

    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        if (other is EntitySet<T> set)
            return IsProperSupersetOf(set);

        int count = 0;
        foreach (var entity in this)
        {
            if (!other.Contains(entity))
                return false;

            count++;
        }

        return count > other.Count();
    }

    public bool IsSubsetOf(EntitySet<T> other)
    {
        return bitSet.IsSubsetOf(other.bitSet);
    }

    public bool IsSubsetOf(IEnumerable<T> other)
    {
        if (other is EntitySet<T> set)
            return IsSubsetOf(set);

        foreach (var entity in this)
        {
            if (!other.Contains(entity))
                return false;
        }

        return true;
    }

    public bool IsSupersetOf(BitSet mask)
    {
        return bitSet.IsSupersetOf(mask);
    }

    public bool IsSupersetOf(EntitySet<T> other)
    {
        return IsSupersetOf(other.bitSet);
    }

    public bool IsSupersetOf(IEnumerable<T> other)
    {
        if (other is EntitySet<T> set)
            return IsSupersetOf(set);

        foreach (var entity in other)
        {
            if (!Contains(entity))
                return false;
        }

        return true;
    }

    public bool Overlaps(BitSet mask)
    {
        return bitSet.Overlaps(mask);
    }

    public bool Overlaps(EntitySet<T> other)
    {
        return Overlaps(other.bitSet);
    }

    public bool Overlaps(IEnumerable<T> other)
    {
        if (other is EntitySet<T> set)
            return Overlaps(set);

        foreach (var entity in other)
        {
            if (Contains(entity))
                return true;
        }

        return false;
    }

    public bool Equals(BitSet mask)
    {
        return bitSet.Equals(mask);
    }

    public bool Equals(EntitySet<T> other)
    {
        return Equals(other.bitSet);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is not null && (obj is EntitySet<T> set && Equals(set) || obj is BitSet mask && Equals(mask));
    }

    public bool SetEquals(IEnumerable<T> other)
    {
        if (other is EntitySet<T> set)
            return Equals(set);

        if (Count != other.Count())
            return false;

        foreach (var entity in other)
        {
            if (!Contains(entity))
                return false;
        }

        return true;
    }

    public void SymmetricExceptWith(BitSet mask)
    {
        bitSet.SymmetricExceptWith(mask);
    }

    public void SymmetricExceptWith(EntitySet<T> other)
    {
        SymmetricExceptWith(other.bitSet);
    }

    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        if (other is EntitySet<T> set)
        {
            SymmetricExceptWith(set);
        }
        else
        {
            foreach (var entity in this)
            {
                foreach (var otherEntity in other)
                {
                    if (entity == otherEntity)
                        Remove(entity);
                }
            }
        }
    }

    public void UnionWith(BitSet mask)
    {
        bitSet.UnionWith(mask);
    }

    public void UnionWith(EntitySet<T> other)
    {
        UnionWith(other.bitSet);
    }

    public void UnionWith(IEnumerable<T> other)
    {
        if (other is EntitySet<T> set)
        {
            UnionWith(set);
        }
        else
        {
            foreach (var entity in other)
                Add(entity);
        }
    }

    public void Complementary()
    {
        bitSet.Complementary();
    }

    void ICollection<T>.Add(T item)
    {
        Add(item);
    }

    public override int GetHashCode()
    {
        return bitSet.GetHashCode();
    }

    public static bool operator ==(EntitySet<T> left, EntitySet<T> right)
    {
        return ReferenceEquals(left, right) || left is null ? right is null : left.Equals(right);
    }

    public static bool operator !=(EntitySet<T> left, EntitySet<T> right)
    {
        return !(left == right);
    }

    public static bool operator <=(EntitySet<T> left, EntitySet<T> right)
    {
        return ReferenceEquals(left, right) || left is null ? right is null : right.IsSubsetOf(left);
    }

    public static bool operator <(EntitySet<T> left, EntitySet<T> right)
    {
        return !ReferenceEquals(left, right) && left is null && right is null && right.IsProperSubsetOf(left);
    }

    public static bool operator >=(EntitySet<T> left, EntitySet<T> right)
    {
        return !(left < right);
    }

    public static bool operator >(EntitySet<T> left, EntitySet<T> right)
    {
        return !(left <= right);
    }

    public static EntitySet<T> operator &(EntitySet<T> left, EntitySet<T> right)
    {
        var result = new EntitySet<T>(left);
        result.IntersectWith(right);
        return result;
    }

    public static EntitySet<T> operator |(EntitySet<T> left, EntitySet<T> right)
    {
        var result = new EntitySet<T>(left);
        result.UnionWith(right);
        return result;
    }

    public static EntitySet<T> operator ^(EntitySet<T> left, EntitySet<T> right)
    {
        var result = new EntitySet<T>(left);
        result.SymmetricExceptWith(right);
        return result;
    }

    public static EntitySet<T> operator ~(EntitySet<T> set)
    {
        var result = new EntitySet<T>(set);
        result.Complementary();
        return result;
    }

    public static EntitySet<T> operator -(EntitySet<T> left, EntitySet<T> right)
    {
        var result = new EntitySet<T>(left);
        left.ExceptWith(right);
        return result;
    }
}