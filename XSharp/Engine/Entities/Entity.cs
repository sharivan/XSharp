using System;
using System.Collections.Generic;
using System.IO;

using MMX.Geometry;

using static MMX.Engine.Consts;
using static MMX.Engine.World.World;

namespace MMX.Engine.Entities
{
    public enum VectorKind
    {
        NONE = 0,
        ORIGIN = 1,
        PLAYER_ORIGIN = 2,
        BOUDINGBOX_CENTER = 4,
        HITBOX_CENTER = 8,
        ALL = 255
    }

    public enum BoxKind
    {
        NONE = 0,
        BOUDINGBOX = 1,
        HITBOX = 2,
        ALL = 255
    }

    public abstract class Entity : IDisposable
    {
        private Vector origin;
        private Entity parent;
        private readonly List<Entity> touchingEntities;
        private readonly List<Entity> childs;
        protected bool alive;
        protected bool markedToRemove;
        protected bool respawnable;

        public GameEngine Engine
        {
            get;
        }

        public int Index
        {
            get;
            internal set;
        }

        public Vector Origin
        {
            get => origin;
            set => SetOrigin(value);
        }

        public Entity Parent
        {
            get => parent;

            set
            {
                if (parent == value)
                    return;

                if (value != null && value.IsParent(this))
                    throw new ArgumentException("Cyclic parenting is not allowed.");

                parent?.childs.Remove(this);
                parent = value;
                parent?.childs.Add(this);
            }
        }

        public IEnumerable<Entity> TouchingEntities => touchingEntities;

        public IEnumerable<Entity> Childs => childs;

        public Vector LastOrigin
        {
            get;
            private set;
        }

        public Box BoundingBox
        {
            get => GetBoundingBox();
            set => SetBoundingBox(value);
        }

        public Box HitBox => GetHitBox();

        public bool Alive => alive;

        public bool MarkedToRemove => markedToRemove;

        public bool Respawnable => respawnable;

        public bool Offscreen => !HasIntersection(BoundingBox, Engine.World.Camera.BoundingBox);

        protected Entity(GameEngine engine, Vector origin)
        {
            Engine = engine;
            this.origin = origin;

            touchingEntities = new List<Entity>();
            childs = new List<Entity>();
        }

        public bool IsTouching(Entity other) => touchingEntities.Contains(other);

        public bool Contains(Entity other) => childs.Contains(other);

        protected virtual void SetOrigin(Vector origin)
        {
            LastOrigin = this.origin;
            this.origin = origin;
            Engine.partition.Update(this);

            Vector delta = origin - LastOrigin;
            foreach (Entity child in childs)
                child.Origin += delta;
        }

        public bool IsParent(Entity entity)
        {
            if (entity == null)
                return false;

            Entity parent = this.parent;
            while (parent != null)
            {
                if (parent == entity)
                    return true;

                parent = parent.parent;
            }

            return false;
        }

        public Vector GetVector(VectorKind kind) => kind switch
        {
            VectorKind.ORIGIN => origin,
            VectorKind.PLAYER_ORIGIN => origin - new Vector(0, 17),
            VectorKind.BOUDINGBOX_CENTER => BoundingBox.Center,
            VectorKind.HITBOX_CENTER => HitBox.Center,
            _ => Vector.NULL_VECTOR
        };

        protected abstract Box GetBoundingBox();

        protected virtual Box GetHitBox() => GetBoundingBox();

        protected virtual void SetBoundingBox(Box boudingBox)
        {
        }

        public Box GetBox(BoxKind kind) => kind switch
        {
            BoxKind.BOUDINGBOX => BoundingBox,
            BoxKind.HITBOX => HitBox,
            _ => Box.EMPTY_BOX,
        };

        public virtual void LoadState(BinaryReader reader)
        {
            origin = new Vector(reader);
            LastOrigin = new Vector(reader);
            alive = reader.ReadBoolean();
            markedToRemove = reader.ReadBoolean();
            respawnable = reader.ReadBoolean();
        }

        public virtual void SaveState(BinaryWriter writer)
        {
            origin.Write(writer);
            LastOrigin.Write(writer);
            writer.Write(alive);
            writer.Write(markedToRemove);
            writer.Write(respawnable);
        }

        public override string ToString() => "Entity [" + origin + "]";

        public virtual void OnFrame()
        {
            if (!alive || markedToRemove)
                return;

            if (!PreThink())
                return;

            Think();

            PostThink();

            List<Entity> touching = Engine.partition.Query(HitBox, this, childs, BoxKind.HITBOX);

            int count = touchingEntities.Count;
            for (int i = 0; i < count; i++)
            {
                Entity obj = touchingEntities[i];
                int index = touching.IndexOf(obj);

                if (index == -1)
                {
                    touchingEntities.RemoveAt(i);
                    i--;
                    count--;
                    OnEndTouch(obj);
                }
                else
                {
                    touching.RemoveAt(index);
                    OnTouching(obj);
                }
            }

            foreach (Entity obj in touching)
            {
                touchingEntities.Add(obj);
                OnStartTouch(obj);
            }
        }

        protected virtual void OnStartTouch(Entity obj)
        {
        }

        protected virtual void OnTouching(Entity obj)
        {
        }

        protected virtual void OnEndTouch(Entity obj)
        {
        }

        protected virtual bool PreThink() => true;

        protected virtual void Think()
        {
        }

        protected virtual void PostThink()
        {
        }

        public void Dispose() => Kill();

        public void Kill()
        {
            if (!alive || markedToRemove)
                return;

            markedToRemove = true;
            Engine.removedEntities.Add(this);

            foreach (Entity child in childs)
                child.parent = null;

            childs.Clear();

            alive = false;

            OnDeath();
        }

        public virtual void Spawn()
        {
            alive = true;
            markedToRemove = false;
            Engine.addedEntities.Add(this);
        }

        protected virtual void OnDeath()
        {
        }

        protected virtual void OnVisible()
        {

        }
    }
}
