using MMX.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMX.Engine
{
    public abstract class Entity
    {
        protected GameEngine engine;
        internal int index; // Posição deste objeto na lista de objetos do engine
        private Vector origin;
        private Entity parent;

        private Vector lastOrigin;
        private List<Entity> touchingEntities;
        private List<Entity> childs;
        protected bool markedToRemove;

        public GameEngine Engine
        {
            get
            {
                return engine;
            }
        }

        /// <summary>
        /// Posição deste objeto na lista de objetos do engine
        /// </summary>
        public int Index
        {
            get
            {
                return index;
            }
        }

        public Vector Origin
        {
            get
            {
                return origin;
            }

            set
            {
                SetOrigin(value);
            }
        }

        public Entity Parent
        {
            get
            {
                return parent;
            }

            set
            {
                if (parent != null)
                    parent.childs.Remove(this);

                parent = value;

                if (parent != null)
                    parent.childs.Add(this);
            }
        }

        public Vector LastOrigin
        {
            get
            {
                return lastOrigin;
            }
        }

        public Box BoundingBox
        {
            get
            {
                return GetBoundingBox();
            }

            set
            {
                SetBoundingBox(value);
            }
        }

        /// <summary>
        /// Indica se este objeto foi marcado para remoção. Tal remoção só ocorrerá depois de serem processadas todas as interações físicas entre os elementos do jogo.
        /// </summary>
        public bool MarkedToRemove
        {
            get
            {
                return markedToRemove;
            }
        }

        public bool Offscreen
        {
            get
            {
                return (BoundingBox & engine.World.Screen.BoudingBox).Area == 0;
            }
        }

        protected Entity(GameEngine engine, Vector origin)
        {
            this.engine = engine;
            this.origin = origin;

            touchingEntities = new List<Entity>();
            childs = new List<Entity>();
        }

        protected virtual void SetOrigin(Vector origin)
        {
            lastOrigin = this.origin;
            this.origin = origin;
            engine.partition.Update(this);

            Vector delta = origin - lastOrigin;
            foreach (Entity child in childs)
                child.Origin += delta;
        }

        protected abstract Box GetBoundingBox();

        protected virtual void SetBoundingBox(Box boudingBox)
        {
        }

        public virtual void LoadState(BinaryReader reader)
        {
            origin = new Vector(reader);
            lastOrigin = new Vector(reader);
            markedToRemove = reader.ReadBoolean();
        }

        public virtual void SaveState(BinaryWriter writer)
        {
            origin.Write(writer);
            lastOrigin.Write(writer);
            writer.Write(markedToRemove);
        }

        public override string ToString()
        {
            return "Object [" + origin + "]";
        }

        public virtual void OnFrame()
        {
            // Se ele estiver marcado para remoção não há nada o que se fazer aqui
            if (markedToRemove)
                return;

            // Realiza o pré-pensamento do objeto. Nesta chamada verifica-se se deveremos continuar a processar as interações deste objeto com o jogo.
            if (!PreThink())
                return;

            Think(); // Realiza o pensamento do objeto, usado para implementação de inteligência artificial

            PostThink(); // Realiza o pós-pensamento do objeto

            List<Entity> touching = engine.partition.Query(BoundingBox, this, childs);

            // Processa a lista global de objetos que anteriormente estavam tocando esta entidade no frame anterior
            int count = touchingEntities.Count;
            for (int i = 0; i < count; i++)
            {
                // Se pra cada objeto desta lista
                Entity obj = touchingEntities[i];
                int index = touching.IndexOf(obj);

                if (index == -1) // ele não estiver na lista de toques local (ou seja, não está mais tocando este objeto)
                {
                    touchingEntities.RemoveAt(i); // então remove-o da lista global
                    i--;
                    count--;
                    OnEndTouch(obj); // e notifica que ele nao está mais tocando esta objeto
                }
                else // Senão
                {
                    touching.RemoveAt(index); // Remove da lista local de toques
                    OnTouching(obj); // Notifica que ele continua tocando este objeto
                }
            }

            // Para cada objetos que sobrou na lista de toques local
            foreach (Entity obj in touching)
            {
                touchingEntities.Add(obj); // Adiciona-o na lista global de toques
                OnStartTouch(obj); // e notifique que ele começou a tocar este objeto
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

        /// <summary>
        /// Evento interno que é chamado antes de ser processada as interações físicas deste objeto com os demais elementos do jogo.
        /// Sobreponha este evento em suas casses descendentes se desejar controlar o comportamento deste objeto a cada frame do jogo.
        /// </summary>
        /// <returns>true se as interações físicas deverão ser processadas, false caso contrário</returns>
        protected virtual bool PreThink()
        {
            return true;
        }

        /// <summary>
        /// Evento interno que é chamado após as interações físicas deste objeto com os demais elementos do jogo forem feitas e sua posição e velocidade já tiver sido recalculadas.
        /// Sobreponha este evento em suas classes descendentes para simular um quantum de pensamento de um objeto, muito útil para a implementação de inteligência artificial.
        /// </summary>
        protected virtual void Think()
        {
        }

        /// <summary>
        /// Este evento interno é chamado após ocorrerem as interações físicas deste objeto com os demais elementos do jogo, realizada a chamada do evento Think() e feita a chamada do evento OnFrame() da classe Sprite.
        /// Sobreponha este evento em suas classes descendentes se desejar realizar alguma operação final neste objeto antes do próximo tick do jogo.
        /// </summary>
        protected virtual void PostThink()
        {
        }
    }
}
