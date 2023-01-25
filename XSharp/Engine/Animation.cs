using System.IO;

using SharpDX;
using SharpDX.Direct3D9;

using MMX.Math;

using MMXBox = MMX.Geometry.Box;
using Sprite = MMX.Engine.Entities.Sprite;

namespace MMX.Engine
{
    public delegate void AnimationFrameEvent(Animation animation, int frameSequenceIndex);

    public class Animation
    {
        private SpriteSheet.FrameSequence sequence;
        private bool animating; // Indica se a animação será dinâmica ou estática
        private int currentSequenceIndex; // Quadro atual
        private int currentRenderSequenceIndex;
        private bool animationEndFired; // Indica se o evento OnAnimationEnd da entidade associada a esta animação foi chamado desde que a animação foi completada

        private readonly AnimationFrameEvent[] animationEvents;

        public MMXBox DrawBox
        {
            get
            {
                var frame = sequence[currentRenderSequenceIndex];
                return Sprite.Origin + frame.BoundingBox;
            }
        }

        public Animation(Sprite sprite, int index, int spriteSheetIndex, string frameSequenceName, int initialFrame = 0, bool startVisible = true, bool startOn = true, bool mirrored = false, bool flipped = false) :
            this(sprite, index, spriteSheetIndex, frameSequenceName, FixedSingle.ZERO, initialFrame, startVisible, startOn, mirrored, flipped)
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
        public Animation(Sprite sprite, int index, int spriteSheetIndex, string frameSequenceName, FixedSingle rotation, int initialSequenceIndex = 0, bool startVisible = true, bool startOn = true, bool mirrored = false, bool flipped = false)
        {
            Sprite = sprite;
            Index = index;
            SpriteSheetIndex = spriteSheetIndex;
            FrameSequenceName = frameSequenceName;

            sequence = Sheet.GetFrameSequence(frameSequenceName);
            InitialSequenceIndex = initialSequenceIndex;
            Visible = startVisible;
            animating = startOn;
            Mirrored = mirrored;
            Flipped = flipped;
            Rotation = rotation;

            Scale = 1;

            currentSequenceIndex = initialSequenceIndex;
            currentRenderSequenceIndex = currentSequenceIndex;
            animationEndFired = false;

            int count = sequence.Count;
            animationEvents = new AnimationFrameEvent[count];
        }

        public void SaveState(BinaryWriter writer)
        {
            writer.Write(Index);
            writer.Write(Visible);
            writer.Write(animating);

            Rotation.Write(writer);
            Scale.Write(writer);
            writer.Write(Flipped);
            writer.Write(Mirrored);

            writer.Write(currentSequenceIndex);
            writer.Write(animationEndFired);
        }

        public void LoadState(BinaryReader reader)
        {
            Index = reader.ReadInt32();
            Visible = reader.ReadBoolean();
            animating = reader.ReadBoolean();

            Rotation = new FixedSingle(reader);
            Scale = new FixedSingle(reader);
            Flipped = reader.ReadBoolean();
            Mirrored = reader.ReadBoolean();

            currentSequenceIndex = reader.ReadInt32();
            animationEndFired = reader.ReadBoolean();
        }

        public void SetEvent(int frameSequenceIndex, AnimationFrameEvent animationEvent) => animationEvents[frameSequenceIndex] = animationEvent;

        public void ClearEvent(int frameSequenceIndex) => SetEvent(frameSequenceIndex, null);

        /// <summary>
        /// Inicia a animação a partir do quadro atual
        /// </summary>
        public void Start(int startIndex = -1)
        {
            animationEndFired = false;
            animating = true;

            if (startIndex != -1)
            {
                currentSequenceIndex = InitialSequenceIndex + startIndex;
                currentRenderSequenceIndex = currentSequenceIndex;
            }
        }

        /// <summary>
        /// Inicia a animação a partir do quadro inicial
        /// </summary>
        public void StartFromBegin() => Start(0);

        /// <summary>
        /// Para a animação
        /// </summary>
        public void Stop() => animating = false;

        /// <summary>
        /// Reseta a animação, definindo o quadro atual como o quadro inicial
        /// </summary>
        public void Reset()
        {
            currentSequenceIndex = InitialSequenceIndex;
            currentRenderSequenceIndex = currentSequenceIndex;
        }

        /// <summary>
        /// Entidade possuidora da animação
        /// </summary>
        public Sprite Sprite
        {
            get;
        }

        public int SpriteSheetIndex
        {
            get;
            private set;
        }

        public string FrameSequenceName
        {
            get;
            private set;
        }

        public int Index
        {
            get;
            private set;
        }

        /// <summary>
        /// ImageList usado para gerar a animação (cada elemento do ImageList é um frame desta animação)
        /// </summary>
        public SpriteSheet Sheet => Sprite.Engine.GetSpriteSheet(SpriteSheetIndex);

        /// <summary>
        /// Frame inicial da animação
        /// </summary>
        public int InitialSequenceIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Frame atual da animação
        /// </summary>
        public int CurrentSequenceIndex
        {
            get => currentSequenceIndex;

            set
            {
                currentSequenceIndex = value >= sequence.Count ? sequence.Count - 1 : value;
                currentRenderSequenceIndex = currentSequenceIndex;
                animationEndFired = false;
            }
        }

        public MMXBox CurrentFrameBoundingBox
        {
            get
            {
                if (currentRenderSequenceIndex < 0 || currentRenderSequenceIndex > sequence.Count)
                    return MMXBox.EMPTY_BOX;

                MMXBox boundingBox = sequence[currentRenderSequenceIndex].BoundingBox;
                return boundingBox;
            }
        }

        public MMXBox CurrentFrameCollisionBox
        {
            get
            {
                if (currentRenderSequenceIndex < 0 || currentRenderSequenceIndex > sequence.Count)
                    return MMXBox.EMPTY_BOX;

                MMXBox collisionBox = sequence[currentRenderSequenceIndex].CollisionBox;
                return collisionBox;
            }
        }

        /// <summary>
        /// Visibilidade da animação (true se está visível, false caso contrário)
        /// </summary>
        public bool Visible
        {
            get;
            set;
        }

        /// <summary>
        /// true se a animação está sendo executada, false caso contrário
        /// </summary>
        public bool Animating
        {
            get => animating;

            set
            {
                if (value && !animating)
                    Start();
                else if (!value && animating)
                    Stop();
            }
        }

        public int LoopFromFrame => sequence.LoopFromSequenceIndex;

        public FixedSingle Rotation
        {
            get;
            set;
        }

        public FixedSingle Scale
        {
            get;
            set;
        }

        public bool Flipped
        {
            get;
            set;
        }

        public bool Mirrored
        {
            get;
            set;
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
            currentRenderSequenceIndex = currentSequenceIndex++;

            if (currentSequenceIndex >= sequence.Count) // Se chegamos no final da animação
            {
                currentSequenceIndex = sequence.Count - 1;

                if (!animationEndFired)
                {
                    animationEndFired = true;
                    Sprite.OnAnimationEnd(this);
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
            if (!Visible || sequence.Count == 0)
                return;

            var frame = sequence[currentRenderSequenceIndex];
            MMXBox srcBox = frame.BoundingBox;
            Texture texture = frame.Texture;

            MMXBox drawBox = Sprite.Origin + srcBox;
            Vector2 center = Sprite.Engine.WorldVectorToScreen(drawBox.Origin);
            var center3 = new Vector3(center.X, center.Y, 0);

            Matrix transform = Matrix.Identity;

            float drawScale = (float) Sprite.Engine.DrawScale;
            transform *= Matrix.Scaling(drawScale);

            if (Rotation != FixedSingle.ZERO)
                transform *= Matrix.Translation(-center3) * Matrix.RotationZ((float) Rotation) * Matrix.Translation(center3);

            if (Scale != FixedSingle.ONE)
                transform *= Matrix.Translation(-center3) * Matrix.Scaling((float) Scale) * Matrix.Translation(center3);

            if (Flipped)
            {
                if (Mirrored)
                    transform *= Matrix.Translation(-center3) * Matrix.Scaling(-1, -1, 1) * Matrix.Translation(center3);
                else
                    transform *= Matrix.Translation(-center3) * Matrix.Scaling(1, -1, 1) * Matrix.Translation(center3);
            }
            else if (Mirrored)
                transform *= Matrix.Translation(-center3) * Matrix.Scaling(-1, 1, 1) * Matrix.Translation(center3);

            Sprite.Engine.RenderTexture(texture, Sprite.Palette, drawBox.LeftTop, transform);
        }

        internal void OnDeviceReset() => sequence = Sheet.GetFrameSequence(FrameSequenceName);

        public override string ToString() => sequence.Name + (Rotation != 0 ? " rotated " + Rotation : "") + (Scale != 0 ? " scaleed " + Scale : "") + (Mirrored ? " left" : " right") + (Flipped ? " down" : " up");
    }
}
