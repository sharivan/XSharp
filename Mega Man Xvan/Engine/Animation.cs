using MMX.Geometry;
using MMX.Math;
using SharpDX;
using SharpDX.Direct2D1;
using System;
using System.IO;

using static MMX.Engine.Consts;

namespace MMX.Engine
{
    public delegate void AnimationFrameEvent(Animation animation, int frameSequenceIndex);

    public class Animation
    {
        private Sprite sprite; // Entidade que possui esta animação
        private int index;
        private SpriteSheet sheet; // Sprite sheet usado para gerar a animação (cada elemento do ImageList é um frame desta animação)
        private SpriteSheet.FrameSequence sequence;
        private int initialSequenceIndex; // Quadro inicial da animação
        private bool visible; // Indica se a animação será visivel (se ela será renderizada ou não)
        private bool animating; // Indica se a animação será dinâmica ou estática
        private FixedSingle rotation;
        private bool mirrored;
        private bool flipped;       

        private int currentSequenceIndex; // Quadro atual
        private bool animationEndFired; // Indica se o evento OnAnimationEnd da entidade associada a esta animação foi chamado desde que a animação foi completada

        private AnimationFrameEvent[] animationEvents;

        public Box DrawBox
        {
            get
            {
                var frame = sequence[currentSequenceIndex];
                return sprite.Origin + frame.BoundingBox;
            }
        }

        public Animation(Sprite sprite, int index, SpriteSheet sheet, string frameSequenceName, int initialFrame = 0, bool startVisible = true, bool startOn = true, bool mirrored = false, bool flipped = false) :
            this(sprite, index, sheet, frameSequenceName, FixedSingle.ZERO, initialFrame, startVisible, startOn, mirrored, flipped)
        {
        }

        /// <summary>
        /// Cria uma nova animação
        /// </summary>
        /// <param name="entity">Entidade possuidora da animação</param>
        /// <param name="index">Índice da animação</param>
        /// <param name="imageList">ImageList usado para gerar a animação (cada elemento do ImageList é um frame desta animação)</param>
        /// <param name="fps">Quandidade de quadros por segundo da animação</param>
        /// <param name="initialSequenceIndex">Quadro inicial da animação</param>
        /// <param name="startVisible">Especifica se a animação iniciará visível ou não</param>
        /// <param name="startOn">Especifica se a animação começará ativa ou não</param>
        /// <param name="loop">Especifica se a animação estará em looping ou não</param>
        public Animation(Sprite sprite, int index, SpriteSheet sheet, string frameSequenceName, FixedSingle rotation, int initialSequenceIndex = 0, bool startVisible = true, bool startOn = true, bool mirrored = false, bool flipped = false)
        {
            this.sprite = sprite;
            this.index = index;
            this.sheet = sheet;
            sequence = sheet.GetFrameSequence(frameSequenceName);
            this.initialSequenceIndex = initialSequenceIndex;
            visible = startVisible;
            animating = startOn;
            this.mirrored = mirrored;
            this.flipped = flipped;
            this.rotation = rotation;

            currentSequenceIndex = initialSequenceIndex; // Define o frame atual para o frame inicial
            animationEndFired = false;

            int count = sequence.Count;
            animationEvents = new AnimationFrameEvent[count];
        }

        public void SaveState(BinaryWriter writer)
        {
            writer.Write(index);
            writer.Write(visible);
            writer.Write(animating);

            rotation.Write(writer);
            writer.Write(flipped);
            writer.Write(mirrored);

            writer.Write(currentSequenceIndex);
            writer.Write(animationEndFired);
        }

        public void LoadState(BinaryReader reader)
        {
            index = reader.ReadInt32();
            visible = reader.ReadBoolean();
            animating = reader.ReadBoolean();

            rotation = new FixedSingle(reader);
            flipped = reader.ReadBoolean();
            mirrored = reader.ReadBoolean();

            currentSequenceIndex = reader.ReadInt32();
            animationEndFired = reader.ReadBoolean();
        }

        public void SetEvent(int frameSequenceIndex, AnimationFrameEvent animationEvent)
        {
            animationEvents[frameSequenceIndex] = animationEvent;
        }

        public void ClearEvent(int frameSequenceIndex)
        {
            SetEvent(frameSequenceIndex, null);
        }

        /// <summary>
        /// Inicia a animação a partir do quadro atual
        /// </summary>
        public void Start(int startIndex = -1)
        {
            animationEndFired = false;
            animating = true;

            if (startIndex != -1)
                currentSequenceIndex = initialSequenceIndex + startIndex;
        }

        /// <summary>
        /// Inicia a animação a partir do quadro inicial
        /// </summary>
        public void StartFromBegin()
        {
            Start(0);
        }

        /// <summary>
        /// Para a animação
        /// </summary>
        public void Stop()
        {
            animating = false;
        }

        /// <summary>
        /// Reseta a animação, definindo o quadro atual como o quadro inicial
        /// </summary>
        public void Reset()
        {
            currentSequenceIndex = initialSequenceIndex;
        }

        /// <summary>
        /// Entidade possuidora da animação
        /// </summary>
        public Sprite Sprite
        {
            get
            {
                return sprite;
            }
        }

        public int Index
        {
            get
            {
                return index;
            }
        }

        /// <summary>
        /// ImageList usado para gerar a animação (cada elemento do ImageList é um frame desta animação)
        /// </summary>
        public SpriteSheet Sheet
        {
            get
            {
                return sheet;
            }
        }

        public string FrameSequenceName
        {
            get
            {
                return sequence.Name;
            }
        }

        /// <summary>
        /// Frame inicial da animação
        /// </summary>
        public int InitialSequenceIndex
        {
            get
            {
                return initialSequenceIndex;
            }
            set
            {
                initialSequenceIndex = value;
            }
        }

        /// <summary>
        /// Frame atual da animação
        /// </summary>
        public int CurrentSequenceIndex
        {
            get
            {
                return currentSequenceIndex;
            }
            set
            {
                if (value >= sequence.Count)
                    currentSequenceIndex = sequence.Count - 1;
                else
                    currentSequenceIndex = value;

                animationEndFired = false;
            }
        }

        public Box CurrentFrameBoundingBox
        {
            get
            {
                if (currentSequenceIndex < 0 || currentSequenceIndex > sequence.Count)
                    return Box.EMPTY_BOX;

                Box boundingBox = sequence[currentSequenceIndex].BoundingBox;
                return boundingBox;
            }
        }

        public Box CurrentFrameCollisionBox
        {
            get
            {
                if (currentSequenceIndex < 0 || currentSequenceIndex > sequence.Count)
                    return Box.EMPTY_BOX;

                Box collisionBox = sequence[currentSequenceIndex].CollisionBox;
                return collisionBox;
            }
        }

        /// <summary>
        /// Visibilidade da animação (true se está visível, false caso contrário)
        /// </summary>
        public bool Visible
        {
            get
            {
                return visible;
            }

            set
            {
                visible = value;
            }
        }

        /// <summary>
        /// true se a animação está sendo executada, false caso contrário
        /// </summary>
        public bool Animating
        {
            get
            {
                return animating;
            }

            set
            {
                if (value && !animating)
                    Start();
                else if (!value && animating)
                    Stop();
            }
        }

        public int LoopFromFrame
        {
            get
            {
                return sequence.LoopFromSequenceIndex;
            }
        }

        public FixedSingle Rotation
        {
            get
            {
                return rotation;
            }

            set
            {
                rotation = value;
            }
        }

        public bool Flipped
        {
            get
            {
                return flipped;
            }

            set
            {
                flipped = value;
            }
        }

        public bool Mirrored
        {
            get
            {
                return mirrored;
            }

            set
            {
                mirrored = value;
            }
        }

        /// <summary>
        /// Evento a ser executado a cada frame (tick) do engine
        /// </summary>
        public void OnFrame()
        {
            // Se a animação não está em execução ou não ouver pelo menos dois quadros na animação então não é necessário computar o próximo quadro da animação
            if (!animating || animationEndFired || sequence.Count == 0)
                return;

            animationEvents[currentSequenceIndex]?.Invoke(this, currentSequenceIndex);
            currentSequenceIndex++;

            if (currentSequenceIndex >= sequence.Count) // Se chegamos no final da animação
            {
                currentSequenceIndex = sequence.Count - 1;

                if (!animationEndFired)
                {
                    animationEndFired = true;
                    sprite.OnAnimationEnd(this);
                }

                if (sequence.LoopFromSequenceIndex != -1) // e se a animação está em looping, então o próximo frame deverá ser o primeiro frame da animação (não o frame inicial, definido por initialFrame)
                {
                    currentSequenceIndex = sequence.LoopFromSequenceIndex;
                    animationEndFired = false;
                }
            }
        }

        /// <summary>
        /// Realiza a pintura da animação
        /// </summary>
        /// <param name="g">Objeto do tipo Graphics usado nas operações de desenho e pintura pela animação</param>
        public void Render()
        {
            // Se ñão estiver visível ou não ouver frames então não precisa desenhar nada
            if (!visible || sequence.Count == 0)
                return;

            var frame = sequence[currentSequenceIndex];
            Box srcBox = frame.BoundingBox;
            Bitmap bitmap = frame.Bitmap;
            
            Box drawBox = sprite.Origin + srcBox;
            Vector2 center = sprite.Engine.WorldVectorToScreen(drawBox.Origin);

            Matrix3x2 lastTransform = sprite.Engine.Context.Transform;
            Matrix3x2 transform = rotation != FixedSingle.ZERO ? Matrix3x2.Rotation((float) rotation, center) : Matrix3x2.Identity;

            if (flipped)
            {
                if (mirrored)
                    transform = Matrix3x2.Scaling(-1, -1, center) * transform;                   
                else
                    transform = Matrix3x2.Scaling(1, -1, center) * transform;
            }
            else if (mirrored)
                transform = Matrix3x2.Scaling(-1, 1, center) * transform;

            sprite.Engine.Context.Transform *= transform;
            sprite.Engine.Context.DrawBitmap(bitmap, sprite.Engine.WorldBoxToScreen(drawBox), 1, INTERPOLATION_MODE, new RectangleF(0, 0, (float) srcBox.Width, (float) srcBox.Height));
            //sprite.Engine.Target.Flush();
            sprite.Engine.Context.Transform = lastTransform;
        }

        public override string ToString()
        {
            return sequence.Name + (rotation != 0 ? " rotated " + rotation : "") + (mirrored ? " left" : " right") + (flipped ? " down" : " up");
        }
    }
}
