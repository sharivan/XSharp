using System;
using System.Collections;
using System.Collections.Generic;

using XSharp.Factories;
using XSharp.Serialization;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities;

public class EntityFactory : IndexedNamedFactory<Entity>
{
    public class EntityEnumerator : IEnumerator<Entity>
    {
        private EntityFactory factory;

        public Entity Current
        {
            get;
            protected set;
        }

        object IEnumerator.Current => Current;

        protected internal EntityEnumerator(EntityFactory factory)
        {
            this.factory = factory;
            Current = null;
        }

        public bool MoveNext()
        {
            Current = Current == null ? factory.FirstEntity : Current.next;
            return Current != null;
        }

        public void Reset()
        {
            Current = null;
        }

        public void Dispose()
        {
            Current = null;
            GC.SuppressFinalize(this);
        }
    }

    private Entity[] entities;
    private EntityReference[] references;
    private int count = 0;

    private EntityReference firstEntity = null;
    private EntityReference lastEntity = null;
    private int firstFreeEntityIndex;

    new public Entity this[int index]
    {
        get => entities[index];
        internal set => entities[index] = value;
    }

    public override IReadOnlyList<Entity> Items => entities;

    public override IReadOnlyList<IndexedNamedFactoryItemReference<Entity>> References => references;

    public Entity FirstEntity => firstEntity;

    public Entity LastEntity => lastEntity;

    public override int Count => count;

    internal EntityFactory()
    {
        entities = new Entity[MAX_ENTITIES];
        references = new EntityReference[MAX_ENTITIES];

        firstFreeEntityIndex = 0;
    }

    public override IEnumerator<Entity> GetEnumerator()
    {
        return new EntityEnumerator(this);
    }

    new public EntityReference GetReferenceTo(int index)
    {
        return (EntityReference) base.GetReferenceTo(index);
    }

    new public EntityReference GetReferenceTo(string name)
    {
        return (EntityReference) base.GetReferenceTo(name);
    }

    new public EntityReference GetReferenceTo(Entity entity)
    {
        return entity != null ? GetReferenceTo(entity.Index) : null;
    }

    new public EntityReference GetOrCreateReferenceTo(int index, Type fallBackEntityType, bool forceUseFallBackType = false)
    {
        return (EntityReference) base.GetOrCreateReferenceTo(index, fallBackEntityType, forceUseFallBackType);
    }

    new public EntityReference GetOrCreateReferenceTo(string name, Type fallBackEntityType, bool forceUseFallBackType = false)
    {
        return (EntityReference) base.GetOrCreateReferenceTo(name, fallBackEntityType, forceUseFallBackType);
    }

    public EntityReference<EntityType> GetReferenceTo<EntityType>(int index) where EntityType : Entity
    {
        EntityReference reference = GetReferenceTo(index);
        return reference is EntityReference<EntityType> referenceT ? referenceT : new EntityProxyReference<EntityType>(reference);
    }

    public EntityReference<EntityType> GetReferenceTo<EntityType>(string name) where EntityType : Entity
    {
        EntityReference reference = GetReferenceTo(name);
        return reference is EntityReference<EntityType> referenceT ? referenceT : new EntityProxyReference<EntityType>(reference);
    }

    public EntityReference<EntityType> GetReferenceTo<EntityType>(EntityType item) where EntityType : Entity
    {
        EntityReference reference = GetReferenceTo((Entity) item);
        return reference is EntityReference<EntityType> referenceT ? referenceT : new EntityProxyReference<EntityType>(reference);
    }

    public EntityReference Create(Type type, dynamic initParams)
    {
        if (Count == MAX_ENTITIES)
            throw new IndexOutOfRangeException("Max entities reached the limit.");

        if (!type.IsAssignableTo(typeof(Entity)))
            throw new ArgumentException($"Type '{type}' is not a derived class from Entity class.");

        if (type.IsAbstract)
            throw new ArgumentException($"Type '{type}' is abstract.");

        GameEngine.Engine.CallPrecacheAction(type);

        int index = firstFreeEntityIndex++;
        var entity = (Entity) Activator.CreateInstance(type);
        var reference = GetOrCreateReferenceTo(index, type);

        entity.Index = index;
        entity.reference = reference;
        entities[index] = entity;
        references[index] = reference;

        if (LastEntity is not null)
            LastEntity.next = GetReferenceTo(entity);

        entity.previous = lastEntity;
        entity.next = null;

        firstEntity ??= reference;
        lastEntity = reference;

        if (entity.Name is not null and not "")
            itemsByName.Add(entity.Name, index);

        count++;

        for (int i = firstFreeEntityIndex; i < MAX_ENTITIES; i++)
        {
            if (entities[i] == null)
            {
                firstFreeEntityIndex = i;
                break;
            }
        }

        entity.NotifyCreated();
        entity.Initialize(initParams);
        entity.ResetFromInitParams();

        if (entity.CheckTouchingEntities)
            GameEngine.Engine.partition.Insert(entity);

        return reference;
    }

    public EntityReference<T> Create<T>(dynamic initParams) where T : Entity, new()
    {
        return (EntityReference<T>) Create(typeof(T), initParams);
    }

    public EntityReference<T> Create<T>() where T : Entity, new()
    {
        return Create<T>(null);
    }

    internal void Remove(Entity entity)
    {
        if (entity.CheckTouchingEntities)
            GameEngine.Engine.partition.Remove(entity);

        int index = entity.Index;
        string name = entity.Name;

        EntityReference reference = GetReferenceTo(entity);
        EntityReference next = entity.next;
        EntityReference previous = entity.previous;

        if (next is not null)
            ((Entity) next).previous = previous;

        if (previous is not null)
            ((Entity) previous).next = next;

        if (reference == firstEntity)
            firstEntity = next;

        if (reference == lastEntity)
            lastEntity = previous;

        entities[index] = null;
        references[index] = null;
        reference.Unset();

        if (name is not null and not "")
            itemsByName.Remove(entity.Name);

        entity.Index = -1;
        entity.reference = null;
        count--;

        if (index < firstFreeEntityIndex)
            firstFreeEntityIndex = index;
    }

    internal void Clear()
    {
        for (int i = 0; i < entities.Length; i++)
        {
            var entity = entities[i];
            if (entity != null)
            {
                foreach (Entity child in entity.childs)
                    child.parent = null;

                entity.childs.Clear();
                entities[i] = null;
            }

            var reference = references[i];
            if (reference is not null)
            {
                reference.Unset();
                references[i] = null;
            }
        }

        itemsByName.Clear();
        GameEngine.Engine.partition.Clear();

        firstFreeEntityIndex = 0;
        count = 0;
        firstEntity = null;
        lastEntity = null;
    }

    protected override DuplicateItemNameException<Entity> CreateDuplicateNameException(string name)
    {
        return new DuplicateEntityNameException(name);
    }

    protected override void SetItemFactory(Entity entity, IndexedNamedFactoryItemReference<Entity> reference)
    {
    }

    protected override void SetItemIndex(Entity entity, IndexedNamedFactoryItemReference<Entity> reference, int index)
    {
        entity.Index = index;
        ((EntityReference) reference).TargetIndex = index;
    }

    protected override void SetItemName(Entity entity, IndexedNamedFactoryItemReference<Entity> reference, string name)
    {
        entity.name = name;
    }

    protected override bool CanChangeName(Entity entity)
    {
        return entity.Alive;
    }

    internal void UpdateEntityName(Entity entity, string name)
    {
        UpdateItemName(entity, name);
    }

    protected override Type GetDefaultItemReferenceType(Type itemType)
    {
        return itemType == typeof(Entity) ? typeof(EntityReference) : typeof(EntityReference<>).MakeGenericType(itemType);
    }

    public override void Deserialize(ISerializer input)
    {
        base.Deserialize(input);

        var serializer = (EngineBinarySerializer) input;

        if (entities == null)
            entities = new Entity[MAX_ENTITIES];
        else
            Array.Clear(entities);

        if (references == null)
            references = new EntityReference[MAX_ENTITIES];
        else
            Array.Clear(references);

        count = serializer.ReadInt();
        for (int i = 0; i < count; i++)
        {
            var entity = (Entity) serializer.ReadObject(false, true);
            entities[entity.index] = entity;
            references[entity.index] = GetOrCreateReferenceTo(entity.index, entity.GetType());
        }

        firstFreeEntityIndex = serializer.ReadInt();
        firstEntity = serializer.ReadEntityReference();
        lastEntity = serializer.ReadEntityReference();
    }

    public override void Serialize(ISerializer output)
    {
        base.Serialize(output);

        var serializer = (EngineBinarySerializer) output;

        serializer.WriteInt(count);
        foreach (var entity in entities)
        {
            if (entity != null)
                serializer.WriteObject(entity, false, true);
        }

        serializer.WriteInt(firstFreeEntityIndex);
        serializer.WriteEntityReference(firstEntity);
        serializer.WriteEntityReference(lastEntity);
    }

    protected override void SetReferenceTo(int index, IndexedNamedFactoryItemReference<Entity> reference)
    {
        references[index] = (EntityReference) reference;
    }

    protected override void SetReferenceTo(string name, IndexedNamedFactoryItemReference<Entity> reference)
    {
        if (itemsByName.TryGetValue(name, out int index))
            references[index] = (EntityReference) reference;
    }
}