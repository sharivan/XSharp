﻿using System.IO;

using SharpDX;
using SharpDX.Direct3D9;

using MMX.Math;
using MMX.Geometry;

using MMXBox = MMX.Geometry.Box;
using Sprite = MMX.Engine.Entities.Sprite;

namespace MMX.Engine
{
    public delegate void AnimationFrameEvent(Animation animation, int frameSequenceIndex);

    public class Animation
    {
        private SpriteSheet.FrameSequence sequence;
        private bool animating;
        private int currentSequenceIndex;
        private bool animationEndFired;

        private readonly AnimationFrameEvent[] animationEvents;

        public Vector Offset
        {
            get;
            set;
        }

        public MMXBox DrawBox
        {
            get
            {
                var frame = sequence[currentSequenceIndex];
                return Sprite.Origin + Offset + frame.BoundingBox;
            }
        }

        public Animation(Sprite sprite, int index, int spriteSheetIndex, string frameSequenceName, Vector offset, int repeatX, int repeatY, int initialFrame = 0, bool startVisible = true, bool startOn = true, bool mirrored = false, bool flipped = false) :
            this(sprite, index, spriteSheetIndex, frameSequenceName, offset, FixedSingle.ZERO, repeatX, repeatY, initialFrame, startVisible, startOn, mirrored, flipped)
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
        public Animation(Sprite sprite, int index, int spriteSheetIndex, string frameSequenceName, Vector offset, FixedSingle rotation, int repeatX, int repeatY, int initialFrame = 0, bool startVisible = true, bool startOn = true, bool mirrored = false, bool flipped = false)
        {
            Sprite = sprite;
            Index = index;
            SpriteSheetIndex = spriteSheetIndex;
            FrameSequenceName = frameSequenceName;
            Offset = offset;
            RepeatX = repeatX;
            RepeatY = repeatY;

            sequence = Sheet.GetFrameSequence(frameSequenceName);
            InitialSequenceIndex = initialFrame;
            Visible = startVisible;
            animating = startOn;
            Mirrored = mirrored;
            Flipped = flipped;
            Rotation = rotation;

            Scale = 1;

            currentSequenceIndex = initialFrame;
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
                currentSequenceIndex = InitialSequenceIndex + startIndex;
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
            currentSequenceIndex = InitialSequenceIndex;
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

                var collisionBox = sequence[currentSequenceIndex].CollisionBox;

                if (Flipped)
                {
                    if (Mirrored || Sprite.Directional && Sprite.Direction == Direction.LEFT)
                        collisionBox = collisionBox.Flip().Mirror();
                    else
                        collisionBox = collisionBox.Flip();
                }
                else if (Mirrored || Sprite.Directional && Sprite.Direction == Direction.LEFT)
                    collisionBox = collisionBox.Mirror();

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

        public int RepeatX
        {
            get;
            set;
        } = 1;

        public int RepeatY
        {
            get;
            set;
        } = 1;

        /// <summary>
        /// Evento a ser executado a cada frame (tick) do engine
        /// </summary>
        internal void NextFrame()
        {
            // Se a animação não está em execução ou não ouver pelo menos dois quadros na animação então não é necessário computar o próximo quadro da animação
            if (!animating || animationEndFired || sequence.Count == 0)
                return;

            animationEvents[currentSequenceIndex]?.Invoke(this, currentSequenceIndex);
            currentSequenceIndex++;

            if (currentSequenceIndex >= sequence.Count)
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
            // Se não estiver visível ou não ouver frames então não precisa desenhar nada
            if (!Visible || sequence.Count == 0 || RepeatX <= 0 || RepeatY <= 0)
                return;

            var frame = sequence[currentSequenceIndex];
            MMXBox srcBox = frame.BoundingBox;
            Texture texture = frame.Texture;

            MMXBox drawBox = Sprite.Origin + Offset + srcBox;
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
                if (Mirrored || Sprite.Directional && Sprite.Direction == Direction.LEFT)
                    transform *= Matrix.Translation(-center3) * Matrix.Scaling(-1, -1, 1) * Matrix.Translation(center3);
                else
                    transform *= Matrix.Translation(-center3) * Matrix.Scaling(1, -1, 1) * Matrix.Translation(center3);
            }
            else if (Mirrored || Sprite.Directional && Sprite.Direction == Direction.LEFT)
                transform *= Matrix.Translation(-center3) * Matrix.Scaling(-1, 1, 1) * Matrix.Translation(center3);

            Sprite.Engine.RenderSprite(texture, Sprite.Palette, drawBox.LeftTop, transform, RepeatX, RepeatY);
        }

        internal void OnDeviceReset()
        {
            sequence = Sheet.GetFrameSequence(FrameSequenceName);
        }

        public override string ToString()
        {
            return sequence.Name + (Rotation != 0 ? " rotation " + Rotation : "") + (Scale != 0 ? " scale " + Scale : "") + (Mirrored ? " mirrored" : "") + (Flipped ? " flipped" : "");
        }
    }
}