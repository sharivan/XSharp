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
        private int initialFrameSequenceIndex; // Quadro inicial da animação
        private bool visible; // Indica se a animação será visivel (se ela será renderizada ou não)
        private bool animating; // Indica se a animação será dinâmica ou estática
        private FixedSingle rotation;
        private bool mirrored;
        private bool flipped;       

        private int currentFrameSequenceIndex; // Quadro atual
        private bool animationEndFired; // Indica se o evento OnAnimationEnd da entidade associada a esta animação foi chamado desde que a animação foi completada

        private AnimationFrameEvent[] animationEvents;

        public Box DrawBox
        {
            get
            {
                Tuple<Box, Bitmap> frame = sheet.GetFrame(sequence[currentFrameSequenceIndex]);
                return sprite.Origin + frame.Item1;
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
        /// <param name="initialFrame">Quadro inicial da animação</param>
        /// <param name="startVisible">Especifica se a animação iniciará visível ou não</param>
        /// <param name="startOn">Especifica se a animação começará ativa ou não</param>
        /// <param name="loop">Especifica se a animação estará em looping ou não</param>
        public Animation(Sprite sprite, int index, SpriteSheet sheet, string frameSequenceName, FixedSingle rotation, int initialFrame = 0, bool startVisible = true, bool startOn = true, bool mirrored = false, bool flipped = false)
        {
            this.sprite = sprite;
            this.index = index;
            this.sheet = sheet;
            sequence = sheet.GetFrameSequence(frameSequenceName);
            initialFrameSequenceIndex = initialFrame;
            visible = startVisible;
            animating = startOn;
            this.mirrored = mirrored;
            this.flipped = flipped;
            this.rotation = rotation;

            currentFrameSequenceIndex = initialFrame; // Define o frame atual para o frame inicial
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

            writer.Write(currentFrameSequenceIndex);
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

            currentFrameSequenceIndex = reader.ReadInt32();
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
                currentFrameSequenceIndex = initialFrameSequenceIndex + startIndex;
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
            currentFrameSequenceIndex = initialFrameSequenceIndex;
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
        public int InitialFrameSequenceIndex
        {
            get
            {
                return initialFrameSequenceIndex;
            }
            set
            {
                initialFrameSequenceIndex = value;
            }
        }

        /// <summary>
        /// Frame atual da animação
        /// </summary>
        public int CurrentFrameSequenceIndex
        {
            get
            {
                return currentFrameSequenceIndex;
            }
            set
            {
                if (value >= sequence.Count)
                    currentFrameSequenceIndex = sequence.Count - 1;
                else
                    currentFrameSequenceIndex = value;

                animationEndFired = false;
                //sprite.Engine.Repaint(sprite);
            }
        }

        public Box CurrentFrameBoundingBox
        {
            get
            {
                if (currentFrameSequenceIndex < 0 || currentFrameSequenceIndex > sequence.Count)
                    return Box.EMPTY_BOX;

                Box boundingBox = sheet.GetFrame(sequence[currentFrameSequenceIndex]).Item1;
                return boundingBox;
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
                //sprite.Engine.Repaint(sprite);
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
                return sequence.LoopFromFrame;
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

            animationEvents[currentFrameSequenceIndex]?.Invoke(this, currentFrameSequenceIndex);
            currentFrameSequenceIndex++;

            if (currentFrameSequenceIndex >= sequence.Count) // Se chegamos no final da animação
            {
                currentFrameSequenceIndex = sequence.Count - 1;

                if (!animationEndFired)
                {
                    animationEndFired = true;
                    sprite.OnAnimationEnd(this);
                }

                if (sequence.LoopFromFrame != -1) // e se a animação está em looping, então o próximo frame deverá ser o primeiro frame da animação (não o frame inicial, definido por initialFrame)
                {
                    currentFrameSequenceIndex = sequence.LoopFromFrame;
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

            int frameIndex = sequence[currentFrameSequenceIndex];
            Tuple<Box, Bitmap> frame = sheet.GetFrame(frameIndex);
            Box srcBox = frame.Item1;
            Bitmap bitmap = frame.Item2;
            
            Box drawBox = sprite.Origin + srcBox;
            Vector2 center = sprite.Engine.WorldVectorToScreen(drawBox.Origin);

            Matrix3x2 lastTransform = sprite.Engine.Target.Transform;
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

            sprite.Engine.Target.Transform *= transform;
            sprite.Engine.Target.DrawBitmap(bitmap, sprite.Engine.WorldBoxToScreen(drawBox), 1, INTERPOLATION_MODE, new RectangleF(0, 0, (float) srcBox.Width, (float) srcBox.Height));
            //sprite.Engine.Target.Flush();
            sprite.Engine.Target.Transform = lastTransform;
        }

        public override string ToString()
        {
            return sequence.Name + (mirrored ? " left" : " right") + (flipped ? " down" : " up");
        }
    }
}
