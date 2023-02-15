using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace XSharp.Engine.Entities
{
    public class EntityList<T> : IList<T> where T : Entity
    {
        public class EntityListEnumerator : IEnumerator<T>
        {
            private EntityList<T> list;
            private int index = -1;

            public T Current => index >= 0 ? list[index] : null;

            object IEnumerator.Current => Current;

            internal EntityListEnumerator(EntityList<T> list)
            {
                this.list = list;
            }

            public void Dispose()
            {
                list = null;
                index = -1;
            }

            public bool MoveNext()
            {
                index = list.TryGetNextIndexNotNull(index + 1);
                return index >= 0;
            }

            public void Reset()
            {
                index = -1;
            }
        }

        private BitSet bitSet;

        public int Count => bitSet.Count;

        public bool IsReadOnly => false;

        public T this[int index]
        {
            get => bitSet.Test(index) ? (T) GameEngine.Engine.entities[index] : null;

            set
            {
                if (value != null)
                {
                    if (index != value.Index)
                        throw new ArgumentException($"Index '{index}' is different from entity index '{value.Index}'.");

                    bitSet.Set(index);
                }
                else
                    bitSet.Reset(index);
            }
        }

        public EntityList()
        {
            bitSet = new BitSet();
        }

        public EntityList(params T[] entities) : this()
        {
            AddRange(entities);
        }

        public EntityList(EntityList<T> entities) : this()
        {
            AddRange(entities);
        }

        public EntityList(BitSet bitSet) : this()
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
                foreach (var entity in entities)
                    Remove(entity);
        }

        public void RemoveRange(EntityList<T> entities)
        {
            RemoveRange(entities.bitSet);
        }

        public void RemoveRange(BitSet bitSet)
        {
            this.bitSet.Reset(bitSet);
        }

        public bool Contains(T entity)
        {
            return bitSet.Test(entity.Index);
        }

        public bool ContainsAtLeast(int minCount, params T[] entities)
        {
            int count = 0;
            if (entities != null && entities.Length > 0)
            {
                foreach (var entity in entities)
                    if (Contains(entity))
                    {
                        count++;
                        if (count == minCount)
                            return true;
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
                    if (Contains(entity))
                    {
                        count++;
                        if (count > exactCount)
                            return false;
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
                    if (Contains(entity))
                    {
                        count++;
                        if (count > maxCount)
                            return false;
                    }
            }

            return count <= maxCount;
        }

        public bool ContainsAll(params T[] entities)
        {
            if (entities != null && entities.Length > 0)
            {
                foreach (var entity in entities)
                    if (!Contains(entity))
                        return false;
            }

            return true;
        }

        public bool ContainsAtLeastOne(params T[] entities)
        {
            if (entities != null && entities.Length > 0)
            {
                foreach (var entity in entities)
                    if (Contains(entity))
                        return true;
            }

            return false;
        }

        public bool ContainsOnlyOne(params T[] entities)
        {
            bool containsOne = false;
            if (entities != null && entities.Length > 0)
            {
                foreach (var entity in entities)
                    if (Contains(entity))
                    {
                        if (containsOne)
                            return false;

                        containsOne = true;
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
                    if (Contains(entity))
                        result++;
            }

            return result;
        }

        public void Add(T item)
        {
            TestAndAdd(item);
        }

        public void AddRange(params T[] entities)
        {
            if (entities != null && entities.Length > 0)
                foreach (var entity in entities)
                    Add(entity);
        }

        public void AddRange(EntityList<T> entities)
        {
            AddRange(entities.bitSet);
        }

        public void AddRange(BitSet bitSet)
        {
            this.bitSet.Set(bitSet);
        }

        public void Clear()
        {
            bitSet.Clear();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new EntityListEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new EntityListEnumerator(this);
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            for (int index = bitSet.FirstSetBit(0); index >= 0 && arrayIndex < array.Length; index = bitSet.FirstSetBit(index), arrayIndex++)
                array[arrayIndex] = this[index];
        }

        int IList<T>.IndexOf(T item)
        {
            return item.Index;
        }

        void IList<T>.Insert(int index, T item)
        {
            if (item == null)
                Remove(index);

            if (index != item.Index)
                throw new ArgumentException($"Index '{index}' is different from entity index '{item.Index}'.");

            Add(item);
        }

        void IList<T>.RemoveAt(int index)
        {
            bitSet.Reset(index);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("[");

            bool first = true;
            foreach (var entity in this)
            {
                if (first)
                    first = false;
                else
                    builder.Append(", ");

                builder.Append(entity);
            }

            builder.Append("]");
            return builder.ToString();
        }
    }
}