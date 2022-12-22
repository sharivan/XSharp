using System.IO;

using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;
using MMX.Math;

using MMXBox = MMX.Geometry.Box;

namespace MMX.Engine
{
    public delegate void AnimationFrameEvent(Animation animation, int frameSequenceIndex);

    public class Animation
    {
        private readonly SpriteSheet.FrameSequence sequence;
        private bool animating; // Indica se a animação será dinâmica ou estática
        private int currentSequenceIndex; // Quadro atual
        private bool animationEndFired; // Indica se o evento OnAnimationEnd da entidade associada a esta animação foi chamado desde que a animação foi completada

        private readonly AnimationFrameEvent[] animationEvents;

        public MMXBox DrawBox
        {
            get
            {
                var frame = sequence[currentSequenceIndex];
                return Sprite.Origin + frame.BoundingBox;
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
            this.Sprite = sprite;
            this.Index = index;
            this.Sheet = sheet;
            sequence = sheet.GetFrameSequence(frameSequenceName);
            this.InitialSequenceIndex = initialSequenceIndex;
            Visible = startVisible;
            animating = startOn;
            this.Mirrored = mirrored;
            this.Flipped = flipped;
            this.Rotation = rotation;

            Scale = 1;

            currentSequenceIndex = initialSequenceIndex; // Define o frame atual para o frame inicial
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
                currentSequenceIndex = InitialSequenceIndex + startIndex;
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
        public void Reset() => currentSequenceIndex = InitialSequenceIndex;

        /// <summary>
        /// Entidade possuidora da animação
        /// </summary>
        public Sprite Sprite { get; }

        public int Index { get;
            private set;
        }

        /// <summary>
        /// ImageList usado para gerar a animação (cada elemento do ImageList é um frame desta animação)
        /// </summary>
        public SpriteSheet Sheet { get; }

        public string FrameSequenceName => sequence.Name;

        /// <summary>
        /// Frame inicial da animação
        /// </summary>
        public int InitialSequenceIndex { get;
            set; }

        /// <summary>
        /// Frame atual da animação
        /// </summary>
        public int CurrentSequenceIndex
        {
            get => currentSequenceIndex;

            set
            {
                currentSequenceIndex = value >= sequence.Count ? sequence.Count - 1 : value;

                animationEndFired = false;
            }
        }

        public MMXBox CurrentFrameBoundingBox
        {
            get
            {
                if (currentSequenceIndex < 0 || currentSequenceIndex > sequence.Count)
                    return MMXBox.EMPTY_BOX;

                MMXBox boundingBox = sequence[currentSequenceIndex].BoundingBox;
                return boundingBox;
            }
        }

        public MMXBox CurrentFrameCollisionBox
        {
            get
            {
                if (currentSequenceIndex < 0 || currentSequenceIndex > sequence.Count)
                    return MMXBox.EMPTY_BOX;

                MMXBox collisionBox = sequence[currentSequenceIndex].CollisionBox;
                return collisionBox;
            }
        }

        /// <summary>
        /// Visibilidade da animação (true se está visível, false caso contrário)
        /// </summary>
        public bool Visible { get;
            set; }

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

        public FixedSingle Rotation { get;
            set; }

        public FixedSingle Scale { get;
            set; }

        public bool Flipped { get;
            set; }

        public bool Mirrored { get;
            set; }

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
                    Sprite.OnAnimationEnd(this);
                }

                if (sequence.LoopFromSequenceIndex != -1) // e se a animação está em looping, então o próximo frame deverá ser o primeiro frame da animação (não o frame inicial, definido por initialFrame)
                {
                    currentSequenceIndex = sequence.LoopFromSequenceIndex;
                    animationEndFired = false;
                }
            }
        }

        private static RawMatrix5x4 CreateRawMatrix5x4(float[][] values)
        {
            var result = new RawMatrix5x4
            {
                M11 = values[0][0],
                M12 = values[0][1],
                M13 = values[0][2],
                M14 = values[0][3],
                M21 = values[1][0],
                M22 = values[1][1],
                M23 = values[1][2],
                M24 = values[1][3],
                M31 = values[2][0],
                M32 = values[2][1],
                M33 = values[2][2],
                M34 = values[2][3],
                M41 = values[3][0],
                M42 = values[3][1],
                M43 = values[3][2],
                M44 = values[3][3],
                M51 = values[4][0],
                M52 = values[4][1],
                M53 = values[4][2],
                M54 = values[4][3]
            };

            return result;
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

            var frame = sequence[currentSequenceIndex];
            MMXBox srcBox = frame.BoundingBox;
            Texture bitmap = frame.Bitmap;

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

            /*float brightness = 0;
            float contrast = 1;

            float[][] ptsArray =
            {
                new float[] {contrast, 0, 0, 0}, // scale red
                new float[] {0, contrast, 0, 0}, // scale green
                new float[] {0, 0, contrast, 0}, // scale blue
                new float[] {0, 0, 0, 1}, // scale alpha
                new float[] {brightness, brightness, brightness, 0}
            };

            //var effect = new LookupTable3D(sprite.Engine.Context);
            //effect.SetInput(0, bitmap, true);
            //RawMatrix5x4 matrix = CreateRawMatrix5x4(ptsArray);
            //effect.Matrix = matrix;*/

            Sprite.Engine.RenderTexture(bitmap, Sprite.Palette, drawBox.LeftTop, transform);
        }

        public override string ToString() => sequence.Name + (Rotation != 0 ? " rotated " + Rotation : "") + (Scale != 0 ? " scaleed " + Scale : "") + (Mirrored ? " left" : " right") + (Flipped ? " down" : " up");
    }
}
