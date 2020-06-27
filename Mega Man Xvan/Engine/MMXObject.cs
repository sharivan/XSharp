using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMX.Engine
{
    public abstract class MMXObject
    {
        protected GameEngine engine;
        internal int index; // Posição deste objeto na lista de objetos do engine
        private MMXVector origin;
        private MMXVector lastOrigin;
        private List<MMXObject> touchingObjects;
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

        public MMXVector Origin
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

        public MMXVector LastOrigin
        {
            get
            {
                return lastOrigin;
            }
        }

        public MMXBox BoundingBox
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

        protected MMXObject(GameEngine engine, MMXVector origin)
        {
            this.engine = engine;
            this.origin = origin;

            touchingObjects = new List<MMXObject>();
        }

        protected virtual void SetOrigin(MMXVector origin)
        {
            lastOrigin = this.origin;
            this.origin = origin;
            engine.partition.Update(this);
        }

        protected abstract MMXBox GetBoundingBox();

        protected virtual void SetBoundingBox(MMXBox boudingBox)
        {
        }

        public virtual void LoadState(BinaryReader reader)
        {
            origin = new MMXVector(reader);
            lastOrigin = new MMXVector(reader);
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

            List<MMXObject> touching = engine.partition.Query(BoundingBox, this);

            // Processa a lista global de objetos que anteriormente estavam tocando esta entidade no frame anterior
            int count = touchingObjects.Count;
            for (int i = 0; i < count; i++)
            {
                // Se pra cada objeto desta lista
                MMXObject obj = touchingObjects[i];
                int index = touching.IndexOf(obj);

                if (index == -1) // ele não estiver na lista de toques local (ou seja, não está mais tocando este objeto)
                {
                    touchingObjects.RemoveAt(i); // então remove-o da lista global
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
            foreach (MMXObject obj in touching)
            {
                touchingObjects.Add(obj); // Adiciona-o na lista global de toques
                OnStartTouch(obj); // e notifique que ele começou a tocar este objeto
            }
        }

        protected virtual void OnStartTouch(MMXObject obj)
        {
        }

        protected virtual void OnTouching(MMXObject obj)
        {
        }

        protected virtual void OnEndTouch(MMXObject obj)
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
