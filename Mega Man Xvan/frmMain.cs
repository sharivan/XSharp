using Geometry2D;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Input;

using static Engine.Consts;

namespace Mega_Man_Xvan
{
    public partial class frmMain : Form
    {
        public class Animation : IDisposable
        {
            private Sprite sprite; // Entidade que possui esta animação
            private int index;
            private SpriteSheet sheet; // Sprite sheet usado para gerar a animação (cada elemento do ImageList é um frame desta animação)
            private SpriteSheet.FrameSequence sequence;
            private float fps; // Quantidade de quadros por segundo utilizados para exibir a animação
            private int initialFrameSequenceIndex; // Quadro inicial da animação
            private bool visible; // Indica se a animação será visivel (se ela será renderizada ou não)
            private bool animating; // Indica se a animação será dinâmica ou estática
            private bool mirrored;
            private bool flipped;

            private int currentFrameSequenceIndex; // Quadro atual
            private int tick; // Quantidade de ticks desde a criação da animação
            private int nextTick; // Próximo tick no qual deverá ocorrer a renderização do próximo quadro
            private bool flashing; // Indica se a animação será exibia com o efeito piscante (usado pelos sprites no modo de invencibilidade)
            private bool bright; // Indica se o quadro atual da animação estará com o brilho alto ou normal
            private float nextBrightTick; // Proximo tick no qual deverá ocorrer a alternância do estado bright
            private bool animationEndFired; // Indica se o evento OnAnimationEnd da entidade associada a esta animação foi chamado desde que a animação foi completada

            private Bitmap[] bitmaps; // Cache dos frames
            private Bitmap[] brightBitmaps; // Cache dos frames com brilho
            private Box2D[] boundingBoxes;
            private ColorMatrix cmxPic; // Matriz de cores usada para a geração do efeito de transparência, usado pelo efeito fading da entidade
            private ImageAttributes iaPic; // Atributos de imagem usada para a geração do efeito de transparência

            /// <summary>
            /// Cria uma nova animação.
            /// A quantidade de quadros por segundo será definida por padrão pela constante DEFAULT_FPS.
            /// </summary>
            /// <param name="entity">Entidade possuidora da animação</param>
            /// <param name="index">Índice da animação</param>
            /// <param name="imageList">ImageList usado para gerar a animação (cada elemento do ImageList é um frame desta animação)</param>
            /// <param name="initialFrame">Quadro inicial da animação</param>
            /// <param name="startVisible">Especifica se a animação iniciará visível ou não</param>
            /// <param name="startOn">Especifica se a animação começará ativa ou não</param>
            /// <param name="loop">Especifica se a animação estará em looping ou não</param>
            public Animation(Sprite entity, int index, SpriteSheet sheet, string frameSequenceName, int initialFrame = 0, bool startVisible = true, bool startOn = true) :
            this(entity, index, sheet, frameSequenceName, DEFAULT_FPS, initialFrame, startVisible, startOn)
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
            public Animation(Sprite entity, int index, SpriteSheet sheet, string frameSequenceName, float fps, int initialFrame = 0, bool startVisible = true, bool startOn = true, bool mirrored = false, bool flipped = false)
            {
                this.sprite = entity;
                this.index = index;
                this.sheet = sheet;
                sequence = sheet.GetFrameSequence(frameSequenceName);
                this.fps = fps;
                this.initialFrameSequenceIndex = initialFrame;
                visible = startVisible;
                animating = startOn;
                this.mirrored = mirrored;
                this.flipped = flipped;

                currentFrameSequenceIndex = initialFrame; // Define o frame atual para o frame inicial
                tick = 0; // Reseta o número de ticks
                nextTick = (int) (TICKRATE / fps); // Calcula quando deverá ser o próximo tick para que ocorra a troca de quadros
                bright = false;
                nextBrightTick = 0;
                animationEndFired = false;

                cmxPic = new ColorMatrix();
                iaPic = new ImageAttributes();

                Precache(); // Inicializa o cache dos frames
            }

            /// <summary>
            /// Inicializa o cache dos frames da animação.
            /// Com o uso desta técnica a renderização da animação é acelerada durante cada repintura.
            /// </summary>
            private void Precache()
            {
                int count = sequence.Count;
                bitmaps = new Bitmap[count];
                brightBitmaps = new Bitmap[count];
                boundingBoxes = new Box2D[count];

                for (int i = 0; i < count; i++)
                {
                    bitmaps[i] = PrecacheBitmap(sequence[i], false);
                    brightBitmaps[i] = PrecacheBitmap(sequence[i], true);
                    Box2D boundingBox = sheet.GetFrame(sequence[i]);

                    if (mirrored)
                        boundingBox = boundingBox.Mirror();

                    if (flipped)
                        boundingBox = boundingBox.Flip();

                    boundingBoxes[i] = new Box2D(Vector2D.NULL_VECTOR, boundingBox.Mins, boundingBox.Maxs);
                }
            }

            /// <summary>
            /// Realiza o precache de uma imagem específica
            /// </summary>
            /// <param name="image">Imagem</param>
            /// <param name="bright">true se ela estará brilhando, falso se estará normal</param>
            /// <returns>Imagem no formato bitmap</returns>
            private Bitmap PrecacheBitmap(int frameIndex, bool bright)
            {
                Box2D drawBox = sheet.GetFrame(frameIndex);
                Box2D rect = new Box2D(0, 0, 0, 0, drawBox.Width, drawBox.Height);
                Bitmap bitmap = new Bitmap((int) drawBox.Width, (int) drawBox.Height);
                Graphics g = Graphics.FromImage(bitmap);
                {
                    //Matrix transformation
                    Matrix m = null;
                    if (flipped && mirrored)
                    {
                        m = new Matrix(-1, 0, 0, -1, 0, 0);
                        m.Translate(drawBox.Width, drawBox.Height, MatrixOrder.Append);
                    }
                    else if (flipped)
                    {
                        m = new Matrix(1, 0, 0, -1, 0, 0);
                        m.Translate(0, drawBox.Height, MatrixOrder.Append);
                    }
                    else if (mirrored)
                    {
                        m = new Matrix(-1, 0, 0, 1, 0, 0);
                        m.Translate(drawBox.Width, 0, MatrixOrder.Append);
                    }

                    if (m != null)
                        g.Transform = m;

                    if (sprite.Tiled) // Se a propriedade Tiles da entidade for verdadeira, desenha a animação lado a lado de forma a preencher toda a imagem
                        using (TextureBrush brush = sheet.CreateTextureBrush(frameIndex, WrapMode.Tile))
                        {
                            g.FillRectangle(brush, rect.ToRectangleF());
                        }
                    else
                    {
                        if (bright) // Se bright for definito como true, aplica o efeito de brilho intenso na imagem
                        {
                            float brightness = 10; // raise the brightness in 10x
                            float contrast = 1; // no change the contrast
                            float gamma = 1; // no change in gamma

                            float[][] ptsArray =
                            {
                        new float[] {contrast, 0, 0, 0, 0}, // scale red
                        new float[] {0, contrast, 0, 0, 0}, // scale green
                        new float[] {0, 0, contrast, 0, 0}, // scale blue
                        new float[] {0, 0, 0, 1, 0}, // scale alpha
                        new float[] {brightness, brightness, brightness, 0, 1}
                    };

                            using (ImageAttributes imageAttributes = new ImageAttributes())
                            {
                                imageAttributes.ClearColorMatrix();
                                imageAttributes.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                                imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap);

                                sheet.DrawFrame(g, frameIndex, rect, imageAttributes);
                                //g.DrawImage(image, rect, 0, 0, rect.Width, rect.Height, GraphicsUnit.Pixel, imageAttributes);
                            }
                        }
                        else
                            sheet.DrawFrame(g, frameIndex, rect);
                        //g.DrawImage(image, rect); // Senão simplesmente desenha a imagem sem modificações
                    }
                }

                return bitmap;
            }

            /// <summary>
            /// Inicia a animação a partir do quadro atual
            /// </summary>
            public void Start()
            {
                animationEndFired = false;
                animating = true;
                nextTick = tick + (int) (TICKRATE / fps); // calcula o próximo tick no qual deverá ocorrer a troca de quadros
                sprite.Engine.Repaint(sprite); // Notifica o engine que a entidade deverá ser redesenhada na tela
            }

            /// <summary>
            /// Inicia a animação a partir do quadro inicial
            /// </summary>
            public void StartFromBegin()
            {
                Reset();
                Start();
            }

            /// <summary>
            /// Para a animação
            /// </summary>
            public void Stop()
            {
                animating = false;
                sprite.Engine.Repaint(sprite);
            }

            /// <summary>
            /// Reseta a animação, definindo o quadro atual como o quadro inicial
            /// </summary>
            public void Reset()
            {
                currentFrameSequenceIndex = initialFrameSequenceIndex;
                sprite.Engine.Repaint(sprite);
            }

            /// <summary>
            /// Libera todos os recursos associados a esta animação.
            /// Execute este método somente quando não for usar mais este objeto.
            /// </summary>
            public void Dispose()
            {
                foreach (Bitmap bitmap in bitmaps)
                    bitmap.Dispose();

                foreach (Bitmap bitmap in brightBitmaps)
                    bitmap.Dispose();

                iaPic.Dispose();
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
            /// Quantidde de quadros por segundo da animação
            /// </summary>
            public float FPS
            {
                get
                {
                    return fps;
                }
                set
                {
                    fps = value;
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
                    sprite.Engine.Repaint(sprite);
                }
            }

            public Box2D CurrentFrameBoundingBox
            {
                get
                {
                    if (currentFrameSequenceIndex < 0 || currentFrameSequenceIndex > sequence.Count)
                        return Box2D.EMPTY_BOX;

                    Box2D boundingBox = sheet.GetFrame(sequence[currentFrameSequenceIndex]);
                    return new Box2D(Vector2D.NULL_VECTOR, boundingBox.Mins, boundingBox.Maxs);
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
                    sprite.Engine.Repaint(sprite);
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

            /// <summary>
            /// Especifica se a animação será executada com o efeito pisca pisca com seu brilho sendo alternado.
            /// Usado pelos sprites para indicar que estão no modo de invencibilidade.
            /// </summary>
            public bool Flashing
            {
                get
                {
                    return flashing;
                }
                set
                {
                    flashing = value;

                    if (visible)
                        sprite.Engine.Repaint(sprite);
                }
            }

            /// <summary>
            /// Evento a ser executado a cada frame (tick) do engine
            /// </summary>
            public void OnFrame()
            {
                tick++; // Incrementa o número de ticks da animação

                // Verifica se o efeito pisca pisca está ativo e realiza as operações de alternância do brilho
                if (flashing)
                {
                    if (tick >= nextBrightTick)
                    {
                        bright = !bright;
                        nextBrightTick = tick + BRIGHT_TICK;

                        if (visible)
                            sprite.Engine.Repaint(sprite);
                    }
                }
                else if (bright)
                {
                    bright = false;

                    if (visible)
                        sprite.Engine.Repaint(sprite);
                }

                // Se a animação não está em execução ou não ouver pelo menos dois quadros na animação então não é necessário computar o próximo quadro da animação
                if (!animating || sequence.Count <= 1)
                    return;

                // Verifica se está na hora de avançar o quadro da animação.
                if (tick >= nextTick)
                {
                    if (currentFrameSequenceIndex >= sequence.Count - 1) // Se chegamos no final da animação
                    {
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
                        else if (currentFrameSequenceIndex > sequence.Count - 1) // Por via das dúvidas, verifique se o frame atual não passou dos limites
                            currentFrameSequenceIndex = sequence.Count - 1;
                    }
                    else // Senão, avança para o próximo frame
                        currentFrameSequenceIndex++;

                    nextTick = tick + (int) (TICKRATE / fps); // e computa quando deverá ocorrer o próximo avanço de frame

                    if (visible) // Se a animação estiver visível, notifica o engine que a entidade possuidora dela deverá ser redesenhado
                        sprite.Engine.Repaint(sprite);
                }
            }

            /// <summary>
            /// Realiza a pintura da animação
            /// </summary>
            /// <param name="g">Objeto do tipo Graphics usado nas operações de desenho e pintura pela animação</param>
            public void Paint(Graphics g)
            {
                // Se ñão estiver visível ou não ouver frames então não precisa desenhar nada
                if (!visible || sequence.Count == 0)
                    return;

                Box2D drawBox = sprite.Origin + boundingBoxes[currentFrameSequenceIndex]; // Obtém o retângulo de desenho da entidade
                Bitmap bitmap = bright ? brightBitmaps[currentFrameSequenceIndex] : bitmaps[currentFrameSequenceIndex]; // Obtém o frame a ser desenhado a partir do cache
                cmxPic.Matrix33 = sprite.Opacity; // Atualiza a opacidade da imagem
                iaPic.SetColorMatrix(cmxPic, ColorMatrixFlag.Default, ColorAdjustType.Bitmap); // Define a matriz de cores da imagem
                g.DrawImage(bitmap, sprite.Engine.TransformBox(drawBox), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, iaPic); // Desenha o frame
            }
        }

        public class SpriteSheet
        {
            public class FrameSequence
            {
                private SpriteSheet sheet;
                private string name;
                private List<int> indices;
                private int loopFromFrame;
                private Vector2D offset;

                public SpriteSheet Sheet
                {
                    get
                    {
                        return sheet;
                    }
                }

                public string Name
                {
                    get
                    {
                        return name;
                    }
                }

                public int this[int index]
                {
                    get
                    {
                        return indices[index];
                    }
                }

                public int Count
                {
                    get
                    {
                        return indices.Count;
                    }
                }

                public int LoopFromFrame
                {
                    get
                    {
                        return loopFromFrame;
                    }
                }

                public Vector2D Offset
                {
                    get
                    {
                        return offset;
                    }

                    set
                    {
                        offset = value;
                    }
                }

                internal FrameSequence(SpriteSheet sheet, string name, int loopFromFrame = -1)
                {
                    this.sheet = sheet;
                    this.name = name;
                    this.loopFromFrame = loopFromFrame;

                    indices = new List<int>();
                }


                public void Add(int frameIndex)
                {
                    indices.Add(frameIndex);
                }

                public void AddRepeated(int frameIndex, int count)
                {
                    for (int i = 0; i < count; i++)
                        Add(frameIndex);
                }

                public void AddRange(int startFrameIndex, int endFrameIndex)
                {
                    for (int frameIndex = startFrameIndex; frameIndex <= endFrameIndex; frameIndex++)
                        Add(frameIndex);
                }

                public void AddRangeRepeated(int startFrameIndex, int endFrameIndex, int count)
                {
                    for (int frameIndex = startFrameIndex; frameIndex <= endFrameIndex; frameIndex++)
                        AddRepeated(frameIndex, count);
                }

                public void AddRangeRepeatedRange(int startFrameIndex, int endFrameIndex, int count)
                {
                    for (int i = 0; i < count; i++)
                        AddRange(startFrameIndex, endFrameIndex);
                }

                public void AddFrame(int x, int y, int width, int height, int count = 1)
                {
                    sheet.AddFrame(x + offset.X, y + offset.Y, x, y, width, height);
                    AddRepeated(sheet.FrameCount - 1, count);
                }

                public void AddFrame(float cbX, float cbY, int bbX, int bbY, int bbWidth, int bbHeight, int count = 1)
                {
                    sheet.AddFrame(cbX + offset.X, cbY + offset.Y, bbX, bbY, bbWidth, bbHeight);
                    AddRepeated(sheet.FrameCount - 1, count);
                }

                public void AddFrame(Box2D boudingBox, int count = 1)
                {
                    sheet.AddFrame(boudingBox + offset);
                    AddRepeated(sheet.FrameCount - 1, count);
                }

                public void Clear()
                {
                    indices.Clear();
                }

                public void Remove(int index)
                {
                    indices.RemoveAt(index);
                }
            }

            private string name;
            private Image image;

            private List<Box2D> frames;
            private Dictionary<string, FrameSequence> sequences;

            public string Name
            {
                get
                {
                    return name;
                }
            }

            public int FrameCount
            {
                get
                {
                    return frames.Count;
                }
            }

            public int FrameSequenceCount
            {
                get
                {
                    return sequences.Count;
                }
            }

            public SpriteSheet(string name)
            {
                this.name = name;

                frames = new List<Box2D>();
                sequences = new Dictionary<string, FrameSequence>();
            }

            public SpriteSheet(string name, Image image) :
                this(name)
            {
                this.image = image;
            }

            public SpriteSheet(string name, string imageFileName) :
                this(name)
            {
                LoadFromFile(imageFileName);
            }

            public void Load(Image image)
            {
                this.image = image;
            }

            public void LoadFromFile(string imageFileName)
            {
                image = Image.FromFile(imageFileName);
            }

            public void AddFrame(int x, int y, int width, int height)
            {
                AddFrame(new Box2D(x, y, x, y, width, height));
            }

            public void AddFrame(float cbX, float cbY, int bbX, int bbY, int bbWidth, int bbHeight)
            {
                AddFrame(new Box2D(cbX, cbY, bbX, bbY, bbWidth, bbHeight));
            }

            public void AddFrame(Box2D boudingBox)
            {
                frames.Add(boudingBox);
            }

            public Box2D GetFrame(int frameIndex)
            {
                return frames[frameIndex];
            }

            public int IndexOfFrame(Box2D frame)
            {
                return frames.IndexOf(frame);
            }

            public bool ContainsFrame(Box2D frame)
            {
                return frames.Contains(frame);
            }

            public bool RemoveFrame(Box2D frame)
            {
                return frames.Remove(frame);
            }

            public void RemoveFrame(int index)
            {
                frames.RemoveAt(index);
            }

            public void ClearFrames()
            {
                frames.Clear();
            }

            public FrameSequence AddFrameSquence(string name, int loopFromFrame = -1)
            {
                if (sequences.ContainsKey(name))
                    return sequences[name];

                FrameSequence result = new FrameSequence(this, name, loopFromFrame);
                sequences[name] = result;
                return result;
            }

            public Dictionary<string, FrameSequence>.Enumerator GetFrameSequenceEnumerator()
            {
                return sequences.GetEnumerator();
            }

            public bool ContainsFrameSequence(string name)
            {
                return sequences.ContainsKey(name);
            }

            public FrameSequence GetFrameSequence(string name)
            {
                if (sequences.ContainsKey(name))
                    return sequences[name];

                return null;
            }

            public FrameSequence RemoveFrameSequence(string name)
            {
                if (sequences.ContainsKey(name))
                {
                    FrameSequence result = sequences[name];
                    sequences.Remove(name);
                    return result;
                }

                return null;
            }

            public void ClearFrameSequences()
            {
                sequences.Clear();
            }

            public void DrawFrame(Graphics g, int index)
            {
                DrawFrame(g, frames[index]);
            }

            public void DrawFrame(Graphics g, int index, Box2D dstBox)
            {
                DrawFrame(g, dstBox, frames[index]);
            }

            public void DrawFrame(Graphics g, int index, Box2D dstBox, ImageAttributes imageAttr)
            {
                DrawFrame(g, dstBox, frames[index], imageAttr);
            }

            public void DrawFrame(Graphics g, Box2D box)
            {
                g.DrawImage(image, box.ToRectangleF());
            }

            public void DrawFrame(Graphics g, Box2D dstBox, Box2D box)
            {
                Rectangle dstRect = dstBox.ToRectangle();
                RectangleF rect = box.ToRectangleF();
                g.DrawImage(image, dstRect, rect.X, rect.Y, rect.Width, rect.Height, GraphicsUnit.Pixel);
            }

            public void DrawFrame(Graphics g, Box2D dstBox, Box2D box, ImageAttributes imageAttr)
            {
                Rectangle dstRect = dstBox.ToRectangle();
                RectangleF rect = box.ToRectangleF();
                g.DrawImage(image, dstRect, rect.X, rect.Y, rect.Width, rect.Height, GraphicsUnit.Pixel, imageAttr);
            }

            public TextureBrush CreateTextureBrush(int index, WrapMode mode)
            {
                return CreateTextureBrush(frames[index], mode);
            }

            public TextureBrush CreateTextureBrush(Box2D box, WrapMode mode)
            {
                return new TextureBrush(image, mode, box.ToRectangleF());
            }

            public TextureBrush CreateTextureBrush(Rectangle rect, WrapMode mode)
            {
                return new TextureBrush(image, mode, rect);
            }

            public TextureBrush CreateTextureBrush(RectangleF rect, WrapMode mode)
            {
                return new TextureBrush(image, mode, rect);
            }

            public TextureBrush CreateTextureBrush(int index, ImageAttributes imageAttr)
            {
                return CreateTextureBrush(frames[index], imageAttr);
            }

            public TextureBrush CreateTextureBrush(Box2D box, ImageAttributes imageAttr)
            {
                return new TextureBrush(image, box.ToRectangleF(), imageAttr);
            }

            public TextureBrush CreateTextureBrush(Rectangle rect, ImageAttributes imageAttr)
            {
                return new TextureBrush(image, rect, imageAttr);
            }

            public TextureBrush CreateTextureBrush(RectangleF rect, ImageAttributes imageAttr)
            {
                return new TextureBrush(image, rect, imageAttr);
            }
        }

        public class Screen
        {
            private World world;
            private float width;
            private float height;

            private Vector2D lastCenter;
            private Vector2D center;
            private Vector2D vel;
            private Sprite focusOn;

            internal Screen(World world, float width, float height)
            {
                this.world = world;
                this.width = width;
                this.height = height;

                lastCenter = Vector2D.NULL_VECTOR;
                center = new Vector2D(width / 2, height / 2);
                vel = Vector2D.NULL_VECTOR;
                focusOn = null;
            }

            public World World
            {
                get
                {
                    return world;
                }
            }

            public float Width
            {
                get
                {
                    return width;
                }
            }

            public float Height
            {
                get
                {
                    return height;
                }
            }

            private void SetCenter(Vector2D v)
            {
                SetCenter(v.X, v.Y);
            }

            private void SetCenter(float x, float y)
            {
                float w2 = width / 2;
                float h2 = height / 2;

                if (x < w2)
                    x = w2;
                else if (x + w2 >= World.Width)
                    x = World.Width - w2;

                if (y < h2)
                    y = h2;
                else if (y + h2 >= World.Height)
                    y = World.Height - h2;

                center = new Vector2D(x, y);
            }

            public Vector2D Center
            {
                get
                {
                    return center;
                }
                set
                {
                    if (focusOn != null)
                        return;

                    SetCenter(value);
                }
            }

            public Vector2D LeftTop
            {
                get
                {
                    return center - SizeVector / 2;
                }
            }

            public Vector2D RightBottom
            {
                get
                {
                    return center + SizeVector / 2;
                }
            }

            public Sprite FocusOn
            {
                get
                {
                    return focusOn;
                }
                set
                {
                    focusOn = value;
                    if (focusOn != null)
                        SetCenter(focusOn.Origin);
                }
            }

            public Vector2D SizeVector
            {
                get
                {
                    return new Vector2D(width, height);
                }
            }

            public Box2D BoudingBox
            {
                get
                {
                    Vector2D sv2 = SizeVector / 2;
                    return new Box2D(center, -sv2, sv2);
                }
            }

            public Vector2D Velocity
            {
                get
                {
                    return vel;
                }
                set
                {
                    vel = value;
                }
            }

            public Box2D VisibleBox(Box2D box)
            {
                return BoudingBox & box;
            }

            public bool IsVisible(Box2D box)
            {
                return VisibleBox(box).Area() > 0;
            }

            public void OnFrame()
            {
                if (focusOn != null)
                    SetCenter(focusOn.Origin);
                else if (vel != Vector2D.NULL_VECTOR)
                    center += TICK * vel;

                if (center != lastCenter)
                {
                    lastCenter = center;
                    world.Engine.RepaintAll();
                }
            }
        }

        public enum CollisionData
        {
            NONE = 0x00,
            SOLID,
            SLOPE,
            WATER,
            LADDER
        }

        public struct TileSetPosition
        {
            private int row;
            private int col;

            public int Row
            {
                get
                {
                    return row;
                }
            }

            public int Col
            {
                get
                {
                    return col;
                }
            }

            public TileSetPosition(int row, int col)
            {
                this.row = row;
                this.col = col;
            }

            public override int GetHashCode()
            {
                return 65536 * row + col;
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;

                if (!(obj is TileSetPosition))
                    return false;

                TileSetPosition other = (TileSetPosition) obj;
                return other.row == row && other.col == col;
            }

            public override string ToString()
            {
                return row + "," + col;
            }
        }

        public class TileSet : IDisposable
        {
            private int id;
            internal Bitmap pixels;
            private CollisionData collisionData;
            private bool flipped;
            private bool mirrored;
            private bool upLayer;
            private Graphics g;

            public int ID
            {
                get
                {
                    return id;
                }
            }

            public CollisionData CollisionData
            {
                get
                {
                    return collisionData;
                }

                set
                {
                    collisionData = value;
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

            public bool UpLayer
            {
                get
                {
                    return upLayer;
                }

                set
                {
                    upLayer = value;
                }
            }

            internal TileSet(int id, CollisionData collisionData = CollisionData.NONE, bool flipped = false, bool mirrored = false, bool upLayer = false)
            {
                this.id = id;
                this.collisionData = collisionData;
                this.flipped = flipped;
                this.mirrored = mirrored;
                this.upLayer = upLayer;

                pixels = new Bitmap(TILESET_SIZE, TILESET_SIZE);
                g = Graphics.FromImage(pixels);
            }

            internal TileSet(int id, Color[,] pixels, CollisionData collisionData = CollisionData.NONE, bool flipped = false, bool mirrored = false, bool upLayer = false) :
                this(id, collisionData, flipped, mirrored, upLayer)
            {
                SetPixels(pixels);
            }

            internal TileSet(int id, Image image, Point offset, CollisionData collisionData = CollisionData.NONE, bool flipped = false, bool mirrored = false, bool upLayer = false) :
                this(id, collisionData, flipped, mirrored, upLayer)
            {
                SetPixels(image, offset);
            }

            public Color GetPixel(int x, int y)
            {
                return pixels.GetPixel(x, y);
            }

            public void SetPixel(int x, int y, Color value)
            {
                pixels.SetPixel(x, y, value);
            }

            public void SetPixels(Color[,] pixels)
            {
                for (int x = 0; x < TILESET_SIZE; x++)
                    for (int y = 0; y < TILESET_SIZE; y++)
                        this.pixels.SetPixel(x, y, pixels[y, x]);
            }

            public void SetPixels(Image image, Point offset)
            {
                g.DrawImage(image, 0, 0, new Rectangle(offset.X, offset.Y, TILESET_SIZE, TILESET_SIZE), GraphicsUnit.Pixel);
            }

            public void FillColor(Color color)
            {
                g.Clear(color);
            }

            public void Dispose()
            {
                g.Dispose();
                pixels.Dispose();
            }
        }

        public class World : IDisposable
        {
            private frmMain engine;
            private float width;
            private float height;

            private Screen screen;
            private Partition<Sprite> partition;
            private List<TileSet> tilesetList;
            private int tilesetRowCount;
            private int tilesetColCount;
            private TileSet[,] tilesets;
            private Bitmap downLayer;
            private Bitmap upLayer;
            private Graphics downLayerGraphics;
            private Graphics upLayerGraphics;

            internal World(frmMain engine, float width, float height)
            {
                this.engine = engine;

                if (width < LAYOUT_SIZE)
                    width = LAYOUT_SIZE;
                if (height < LAYOUT_SIZE)
                    height = LAYOUT_SIZE;

                this.width = width;
                this.height = height;

                screen = new Screen(this, SCREEN_WIDTH, SCREEN_HEIGHT);
                partition = new Partition<Sprite>(new Box2D(0, 0, width, height), (int) (width / BLOCK_SIZE), (int) (height / BLOCK_SIZE));
                tilesetList = new List<TileSet>();
                tilesetRowCount = (int)(width / TILESET_SIZE);
                tilesetColCount = (int)(height / TILESET_SIZE);
                tilesets = new TileSet[tilesetColCount, tilesetRowCount];
                downLayer = new Bitmap((int) width, (int) height);
                upLayer = new Bitmap((int) width, (int) height);
                downLayerGraphics = Graphics.FromImage(downLayer);
                upLayerGraphics = Graphics.FromImage(upLayer);
            }

            public frmMain Engine
            {
                get
                {
                    return engine;
                }
            }

            public float Width
            {
                get
                {
                    return width;
                }
            }

            public float Height
            {
                get
                {
                    return height;
                }
            }

            public int TileSetRowCount
            {
                get
                {
                    return (int) (height / TILESET_SIZE);
                }
            }

            public int TileSetColCount
            {
                get
                {
                    return (int)(width / TILESET_SIZE);
                }
            }

            public Box2D Size
            {
                get
                {
                    return new Box2D(0, 0, width, height);
                }
            }

            public Screen Screen
            {
                get
                {
                    return screen;
                }
            }

            public TileSet AddTile(CollisionData collisionData = CollisionData.NONE, bool flipped = false, bool mirrored = false, bool upLayer = false)
            {
                TileSet result = new TileSet(tilesetList.Count, collisionData, flipped, mirrored, upLayer);
                tilesetList.Add(result);
                return result;
            }

            public TileSet AddTile(Color[,] pixels, CollisionData collisionData = CollisionData.NONE, bool flipped = false, bool mirrored = false, bool upLayer = false)
            {
                TileSet result = new TileSet(tilesetList.Count, pixels, collisionData, flipped, mirrored, upLayer);
                tilesetList.Add(result);
                return result;
            }

            public TileSet AddTile(Image image, Point offset, CollisionData collisionData = CollisionData.NONE, bool flipped = false, bool mirrored = false, bool upLayer = false)
            {
                TileSet result = new TileSet(tilesetList.Count, image, offset, collisionData, flipped, mirrored, upLayer);
                tilesetList.Add(result);
                return result;
            }

            public TileSet AddTile(int row, int col, CollisionData collisionData = CollisionData.NONE, bool flipped = false, bool mirrored = false, bool upLayer = false)
            {
                TileSet result = AddTile(collisionData, flipped, mirrored, upLayer);
                tilesets[col, row] = result;
                return result;
            }

            public TileSet AddTile(int row, int col, Color[,] pixels, CollisionData collisionData = CollisionData.NONE, bool flipped = false, bool mirrored = false, bool upLayer = false)
            {
                TileSet result = AddTile(pixels, collisionData, flipped, mirrored, upLayer);
                SetTile(row, col, result);
                return result;
            }

            public TileSet AddTile(int row, int col, Image image, Point offset, CollisionData collisionData = CollisionData.NONE, bool flipped = false, bool mirrored = false, bool upLayer = false)
            {
                TileSet result = AddTile(image, offset, collisionData, flipped, mirrored, upLayer);
                SetTile(row, col, result);
                return result;
            }

            public TileSet AddTile(Color color, CollisionData collisionData = CollisionData.NONE, bool flipped = false, bool mirrored = false, bool upLayer = false)
            {
                TileSet result = AddTile(collisionData, flipped, mirrored, upLayer);
                result.FillColor(color);
                return result;
            }

            public TileSet AddTile(int row, int col, Color color, CollisionData collisionData = CollisionData.NONE, bool flipped = false, bool mirrored = false, bool upLayer = false)
            {
                TileSet result = AddTile(color, collisionData, flipped, mirrored, upLayer);
                SetTile(row, col, result);
                return result;
            }

            public TileSet AddRectangle(int row, int col, int rows, int cols, Color color, CollisionData collisionData = CollisionData.NONE, bool flipped = false, bool mirrored = false, bool upLayer = false)
            {
                TileSet tile = AddTile(color, collisionData, flipped, mirrored, upLayer);
                FillRectangle(row, col, rows, cols, tile);
                return tile;
            }

            public void AddRectangle(int row, int col, int rows, int cols, Image image, CollisionData collisionData = CollisionData.NONE, bool flipped = false, bool mirrored = false, bool upLayer = false)
            {
                int imgWidth = image.Width;
                int imgHeight = image.Height;
                int imgColCount = imgWidth / TILESET_SIZE;
                int imgRowCount = imgHeight / TILESET_SIZE;
                TileSet[,] tiles = new TileSet[imgColCount, imgRowCount];

                for (int c = 0; c < imgColCount; c++)
                    for (int r = 0; r < imgRowCount; r++)
                        tiles[c, r] = AddTile(image, new Point(c * TILESET_SIZE, r * TILESET_SIZE), collisionData, flipped, mirrored, upLayer);


                for (int j = 0; j < cols; j++)
                    for (int i = 0; i < rows; i++)
                        SetTile(row + i, col + j, tiles[j % imgColCount, i % imgRowCount]);
            }

            public void SetTile(int row, int col, TileSet tile)
            {
                tilesets[col, row] = tile;

                if (tile.UpLayer)
                    upLayerGraphics.DrawImage(tile.pixels, col * TILESET_SIZE, row * TILESET_SIZE);
                else
                    downLayerGraphics.DrawImage(tile.pixels, col * TILESET_SIZE, row * TILESET_SIZE);

                Box2D tileBox = new Box2D(col * TILESET_SIZE, row * TILESET_SIZE, TILESET_SIZE, TILESET_SIZE);
                Box2D tileVisibleBox = screen.VisibleBox(tileBox);
                if (tileVisibleBox.Area() > 0)
                    engine.Repaint(tileVisibleBox);
            }

            public TileSet GetTileByID(int id)
            {
                if (id < 0 || id >= tilesetList.Count)
                    return null;

                return tilesetList[id];
            }

            public void RemoveTile(TileSet tile)
            {
                tilesetList.Remove(tile);
                for (int col = 0; col < tilesetColCount; col++)
                    for (int row = 0; row < tilesetRowCount; row++)
                        if (tilesets[col, row] == tile)
                        {
                            tilesets[col, row] = null;
                            using (System.Drawing.Brush brush = new SolidBrush(Color.Transparent))
                            {
                                if (tile.UpLayer)
                                    upLayerGraphics.FillRectangle(brush, col * TILESET_SIZE, row * TILESET_SIZE, TILESET_SIZE, TILESET_SIZE);
                                else
                                    downLayerGraphics.FillRectangle(brush, col * TILESET_SIZE, row * TILESET_SIZE, TILESET_SIZE, TILESET_SIZE);
                            }

                            Box2D tileBox = new Box2D(col * TILESET_SIZE, row * TILESET_SIZE, TILESET_SIZE, TILESET_SIZE);
                            Box2D tileVisibleBox = screen.VisibleBox(tileBox);
                            if (tileVisibleBox.Area() > 0)
                                engine.Repaint(tileVisibleBox);
                        }
            }

            public void RemoveTile(int id)
            {
                TileSet tile = GetTileByID(id);
                if (tile == null)
                    return;

                try
                {
                    tilesetList.RemoveAt(id);
                    for (int col = 0; col < tilesetColCount; col++)
                        for (int row = 0; row < tilesetRowCount; row++)
                            if (tilesets[col, row] == tile)
                            {
                                tilesets[col, row] = null;
                                using (System.Drawing.Brush brush = new SolidBrush(Color.Transparent))
                                {
                                    if (tile.UpLayer)
                                        upLayerGraphics.FillRectangle(brush, col * TILESET_SIZE, row * TILESET_SIZE, TILESET_SIZE, TILESET_SIZE);
                                    else
                                        downLayerGraphics.FillRectangle(brush, col * TILESET_SIZE, row * TILESET_SIZE, TILESET_SIZE, TILESET_SIZE);
                                }

                                Box2D tileBox = new Box2D(col * TILESET_SIZE, row * TILESET_SIZE, TILESET_SIZE, TILESET_SIZE);
                                Box2D tileVisibleBox = screen.VisibleBox(tileBox);
                                if (tileVisibleBox.Area() > 0)
                                    engine.Repaint(tileVisibleBox);
                            }
                }
                finally
                {
                    tile.Dispose();
                }
            }

            public void ClearTiles()
            {
                tilesetList.Clear();
                for (int col = 0; col < tilesetColCount; col++)
                    for (int row = 0; row < tilesetRowCount; row++)
                        tilesets[col, row] = null;

                using (System.Drawing.Brush brush = new SolidBrush(Color.Transparent))
                {
                    downLayerGraphics.FillRectangle(brush, 0, 0, width, height);
                    upLayerGraphics.FillRectangle(brush, 0, 0, width, height);
                }

                engine.RepaintAll();
            }

            public void FillRectangle(int row, int col, int rows, int cols, TileSet tile)
            {
                for (int c = 0; c < cols; c++)
                    for (int r = 0; r > rows; r++)
                        tilesets[col + c, row + r] = tile;

                using (TextureBrush brush = new TextureBrush(tile.pixels, WrapMode.Tile))
                {
                    if (tile.UpLayer)
                        upLayerGraphics.FillRectangle(brush, col * TILESET_SIZE, row * TILESET_SIZE, TILESET_SIZE * cols, TILESET_SIZE * rows);
                    else
                        downLayerGraphics.FillRectangle(brush, col * TILESET_SIZE, row * TILESET_SIZE, TILESET_SIZE * cols, TILESET_SIZE * rows);
                }

                Box2D box = new Box2D(col * TILESET_SIZE, row * TILESET_SIZE, TILESET_SIZE * cols, TILESET_SIZE * rows);
                Box2D visibleBox = screen.VisibleBox(box);
                if (visibleBox.Area() > 0)
                    engine.Repaint(visibleBox);
            }

            public void Dispose()
            {
                foreach (TileSet tile in tilesetList)
                    tile.Dispose();

                downLayerGraphics.Dispose();
                downLayer.Dispose();
                upLayerGraphics.Dispose();
                upLayer.Dispose();
            }

            public void PaintDownLayer(Graphics g, Box2D clipBox)
            {
                Box2D screenBox = screen.BoudingBox;
                Box2D intersection = clipBox & screenBox;
                if (intersection.Area() > 0)
                    g.DrawImage(downLayer, engine.TransformBox(clipBox), intersection.ToRectangle(), GraphicsUnit.Pixel);
            }

            public void PaintUpLayer(Graphics g, Box2D clipBox)
            {
                Box2D screenBox = screen.BoudingBox;
                Box2D intersection = clipBox & screenBox;
                if (intersection.Area() > 0)
                    g.DrawImage(upLayer, engine.TransformBox(clipBox), intersection.ToRectangle(), GraphicsUnit.Pixel);
            }

            public void OnFrame()
            {
                screen.OnFrame();
            }

            public TileSetPosition GetTileSetPositionFromPos(Vector2D pos)
            {
                int col = (int) (pos.X / TILESET_SIZE);
                int row = (int) (pos.Y / TILESET_SIZE);

                return new TileSetPosition(row, col);
            }

            public TileSet GetTileSetFrom(Vector2D pos)
            {
                TileSetPosition tsp = GetTileSetPositionFromPos(pos);
                int row = tsp.Row;
                int col = tsp.Col;

                if (row < 0 || col < 0 || row >= tilesetRowCount || col >= tilesetColCount)
                    return null;

                return tilesets[col, row];
            }

            public bool CheckCollision(Box2D collisionBox)
            {
                TileSetPosition start = GetTileSetPositionFromPos(collisionBox.LeftTop);
                TileSetPosition end = GetTileSetPositionFromPos(collisionBox.RightBottom - new Vector2D(1, 1));

                int startRow = start.Row;
                int startCol = start.Col;

                if (startRow < 0)
                    startRow = 0;

                if (startRow >= tilesetRowCount)
                    startRow = tilesetRowCount - 1;

                if (startCol < 0)
                    startCol = 0;

                if (startCol >= tilesetColCount)
                    startCol = tilesetColCount - 1;

                int endRow = end.Row;
                int endCol = end.Col;

                if (endRow < 0)
                    endRow = 0;

                if (endRow >= tilesetRowCount)
                    endRow = tilesetRowCount - 1;

                if (endCol < 0)
                    endCol = 0;

                if (endCol >= tilesetColCount)
                    endCol = tilesetColCount - 1;

                for (int row = startRow; row <= endRow; row++)
                    for (int col = startCol; col <= endCol; col++)
                    {
                        TileSet tile = tilesets[col, row];
                        if (tile != null && tile.CollisionData == CollisionData.SOLID)
                            return true;
                    }

                return false;
            }

            private Box2D CheckCollision(Box2D startBox, Box2D endBox)
            {
                if (!CheckCollision(endBox))
                    return endBox;

                Vector2D sbo = startBox.Origin;
                int sx = (int) sbo.X;
                int sy = (int) sbo.Y;

                Vector2D ebo = endBox.Origin;
                int ex = (int) ebo.X;
                int ey = (int) ebo.Y;

                Vector2D delta = ebo - sbo;
                Box2D midBox = startBox + delta / 2F;

                Vector2D mbo = midBox.Origin;
                int mx = (int) mbo.X;
                int my = (int) mbo.Y;

                if (sx == mx && sy == my || ex == mx && ey == my)
                    return startBox;

                if (CheckCollision(midBox))
                    return CheckCollision(startBox, midBox);

                return CheckCollision(midBox, endBox);
            }

            public Vector2D CheckCollision(Box2D collisionBox, Vector2D delta)
            {
                Box2D newBox = collisionBox + delta;
                newBox = CheckCollision(collisionBox, newBox);

                Vector2D dx = new Vector2D(delta.X > 0 ? 1 : delta.X < 0 ? -1 : 0, 0);
                Vector2D dy = new Vector2D(0, delta.Y > 0 ? 1 : delta.Y < 0 ? -1 : 0);

                if (IsTouching(newBox, dy))
                {
                    if (!IsTouching(newBox, dx))
                        newBox = CheckCollision(newBox, newBox + new Vector2D(delta.X, newBox.Origin.Y - collisionBox.Origin.Y));
                }

                if (IsTouching(newBox, dx))
                    newBox = CheckCollision(newBox, newBox + new Vector2D(newBox.Origin.X - collisionBox.Origin.X, delta.Y));

                return newBox.Origin - collisionBox.Origin;
            }

            private bool IsTouching(Box2D collisionBox, Vector2D dir)
            {
                return CheckCollision(collisionBox + dir);
            }
        }

        public abstract class Sprite : IDisposable
        {
            protected frmMain engine; // Engine
            protected string name; // Nome da entidade
            protected Vector2D origin; // Coordenadas do sprite
            protected Box2D lastBoundingBox;
            protected Box2D boundingBox; // Retângulo que delimita a àrea máxima de desenho do sprite
            protected List<Animation> animations; // Animações
            private bool tiled; // Especifica se a imagem (ou a animação) desta entidade será desenhanda com preenchimento lado a lado dentro da área de desenho
            private bool directional;

            private float opacity; // Opacidade da imagem (ou animação). Usada para efeito de fading.
            protected bool solid; // Especifica se a entidade será solida ou não a outros elementos do jogo.
            private bool fading; // Especifica se o efeito de fading está ativo
            private bool fadingIn; // Se o efeito de fading estiver ativo, especifica se o tipo de fading em andamento é um fading in
            private float fadingTime; // Se o efeito de fading estiver ativo, especifica o tempo do fading
            private float elapsed; // Se o efeito de fading estiver ativo, indica o tempo decorrido desde o início do fading
            protected Vector2D lastDelta; // Indica o deslocamento desta entidade desde o tick anterior
            protected bool disposed; // Indica se os recursos associados a esta entidade foram liberados
            private int drawCount; // Armazena a quantidade de pinturas feita pela entidade desde sua criação. Usado somente para depuração.
            private bool checkCollisionWithSprites;
            private bool checkCollisionWithWorld;
            private bool gravity;
            private bool landed;

            private SpriteSheet sheet; // Sprite sheet a ser usado na animação deste sprite

            private int index; // Posição deste sprite na lista de sprites do engine

            protected Vector2D vel; // Velocidade
            protected bool moving; // Indica se o sprite está continuou se movendo desde a última iteração física com os demais elementos do jogo
            protected bool markedToRemove; // Indica se o sprite foi marcado para remoção, tal remoção só será realizada após ser executada todas as interações físicas entre os elementos do jogo.
            protected bool isStatic; // Indica se o sprite é estático
            protected bool breakable; // Indica se ele pode ser quebrado
            protected bool passable; // Indica se ele pode ser atravessado por outras entidades
            protected bool passStatics; // Indica se ele pode atravessar entidades estáticas
            protected float health; // HP do sprite
            protected float maxDamage; // Dano máximo que o sprite poderá receber de uma só vez
            protected bool invincible; // Indica se o sprite está invencível, não podendo assim sofrer danos
            protected float invincibilityTime; // Indica o tempo de invencibilidade do sprite quando ele estiver invencível
            private float invincibleExpires; // Indica o instante no qual a invencibilidade do sprite irá terminar. Tal instante é dado em segundos e é relativo ao tempo de execução do engine.
            private List<Sprite> touchingSprites; // Lista todos os outros sprites que estão tocando este sprite (que possuem intersecção não vazia entre seus retângulos de colisão)
            protected bool broke; // Indica se este sprite foi quebrado

            /// <summary>
            /// Cria uma nova entidade
            /// </summary>
            /// <param name="engine">Engine</param>
            /// <param name="name">Nome da entidade</param>
            /// <param name="tiled">true se o desenho desta entidade será preenchido em sua área de pintura lado a lado</param>
            protected Sprite(frmMain engine, string name, Vector2D origin, SpriteSheet sheet, bool tiled = false, bool directional = false)
            {
                this.engine = engine;
                this.name = name;
                this.origin = origin;
                this.sheet = sheet;
                this.tiled = tiled;
                this.directional = directional;

                opacity = 1; // Opacidade 1 significa que não existe transparência (opacidade 1 = opacidade 100% = transparência 0%)
            }

            /// <summary>
            /// Posição deste sprite na lista de sprites do engine
            /// </summary>
            public int Index
            {
                get
                {
                    return index;
                }
            }

            /// <summary>
            /// Indica se este sprite é estático
            /// </summary>
            public bool Static
            {
                get
                {
                    return isStatic;
                }
            }

            /// <summary>
            /// Indica se este sprite pode atravessar sprites estáticos
            /// </summary>
            public bool PassStatics
            {
                get
                {
                    return passStatics;
                }
                set
                {
                    passStatics = value;
                }
            }

            /// <summary>
            /// Indica se este sprite ainda está se movendo desde a última interação física com os demais sprites do jogo
            /// </summary>
            public bool Moving
            {
                get
                {
                    return moving;
                }
            }

            /// <summary>
            /// Indica se este sprite foi quebrado
            /// </summary>
            public bool Broke
            {
                get
                {
                    return broke;
                }
            }

            /// <summary>
            /// Indica se este sprite foi marcado para remoção. Tal remoção só ocorrerá depois de serem processadas todas as interações físicas entre os elementos do jogo.
            /// </summary>
            public bool MarkedToRemove
            {
                get
                {
                    return markedToRemove;
                }
            }

            /// <summary>
            /// Indica se este sprite está no modo de invencibilidade
            /// </summary>
            public bool Invincible
            {
                get
                {
                    return invincible;
                }
            }

            /// <summary>
            /// HP deste sprite
            /// </summary>
            public float Health
            {
                get
                {
                    return health;
                }
                set
                {
                    health = value;
                    OnHealthChanged(health); // Lança o evento notificando a mudança do HP

                    if (health == 0) // Se o HP for zero
                        Break(); // quebre-a!
                }
            }

            /// <summary>
            /// Dano máximo que este sprite poderá receber de uma só vez
            /// </summary>
            public float MaxDamage
            {
                get
                {
                    return maxDamage;
                }
                set
                {
                    maxDamage = value;
                }
            }

            /// <summary>
            /// Vetor velocidade deste sprite
            /// </summary>
            public Vector2D Velocity
            {
                get
                {
                    return vel;
                }
                set
                {
                    vel = value;
                }
            }

            public bool CheckCollisionWithSprites
            {
                get
                {
                    return checkCollisionWithSprites;
                }
                set
                {
                    checkCollisionWithSprites = value;
                }
            }

            public bool CheckCollisionWithWorld
            {
                get
                {
                    return checkCollisionWithWorld;
                }
                set
                {
                    checkCollisionWithWorld = value;
                }
            }

            public bool Gravity
            {
                get
                {
                    return gravity;
                }
                set
                {
                    gravity = value;
                }
            }

            public bool Landed
            {
                get
                {
                    return landed;
                }
            }

            /// <summary>
            /// Mata o sprite (sem quebra-la).
            /// </summary>
            public void Kill()
            {
                OnDeath();
            }

            /// <summary>
            /// Libera qualquer recurso associado a esta entidade.
            /// Utilize este método somente quando este objeto não for mais utilizado.
            /// </summary>
            public virtual void Dispose()
            {
                markedToRemove = true;
                engine.removedSprites.Add(this);

                if (!disposed)
                {
                    foreach (Animation animation in animations)
                        animation.Dispose();

                    animations.Clear();
                    disposed = true;
                }

                engine.Repaint(this);
            }
            /// <summary>
            /// Evento interno que é lançado sempre que o sprite for morto
            /// </summary>
            protected virtual void OnDeath()
            {
                Dispose(); // Por padrão ele apenas dispões ele da memória, liberando todos os recursos associados a ele
            }

            /// <summary>
            /// Teleporta o sprite para uma determinada posição
            /// </summary>
            /// <param name="pos">Nova posição onde o sprite irá se localizar</param>
            public void TeleportTo(Vector2D pos)
            {
                Vector2D delta = pos - origin; // Obtém o vetor de deslocamento
                                               // Atualiza a posição de todos os retângulos
                lastDelta += delta; // E atualiza também o vetor de deslocamento absoluto (que indica o quanto ele se deslocou desde o último tick do engine)

                // Atualiza as partições do engine
                if (isStatic)
                    engine.partitionStatics.Update(this);
                else
                    engine.partitionSprites.Update(this);

                engine.Repaint(this); // Notifica o engine que este sprite deverá ser redesenhado
            }

            /// <summary>
            /// Evento interno que ocorrerá toda vez que uma animação estiver a ser criada.
            /// Seus parâmetros (exceto animationIndex) são passados por referencia de forma que eles podem ser alterados dentro do método e assim definir qual será o comportamento da animação antes que ela seja criada.
            /// </summary>
            /// <param name="animationIndex">Índice da animação</param>
            /// <param name="imageList">Lista de imagens contendo cada quadro usado pela animação</param>
            /// <param name="fps">Número de quadros por segundo</param>
            /// <param name="initialFrame">Quadro inicial</param>
            /// <param name="startVisible">true se a animação iniciará visível, false caso contrário</param>
            /// <param name="startOn">true se a animação iniciará em execução, false caso contrário</param>
            /// <param name="loop">true se a animação executará em looping, false caso contrário</param>
            protected virtual void OnCreateAnimation(int animationIndex, ref SpriteSheet sheet, ref string frameSequenceName, ref float fps, ref int initialFrame, ref bool startVisible, ref bool startOn)
            {
            }

            public override string ToString()
            {
                return name + " [" + origin + "]";
            }

            /// <summary>
            /// Aplica um efeito de fade in
            /// </summary>
            /// <param name="time">Tempodo fading</param>
            public void FadeIn(float time)
            {
                fading = true;
                fadingIn = true;
                fadingTime = time;
                elapsed = 0;
            }

            /// <summary>
            /// Aplica um efeito de fade out
            /// </summary>
            /// <param name="time">Tempo do fading</param>
            public void FadeOut(float time)
            {
                fading = true;
                fadingIn = false;
                fadingTime = time;
                elapsed = 0;
            }

            /// <summary>
            /// Spawna a entidade no jogo.
            /// Este método somente pode ser executado uma única vez após a entidade ser criada.
            /// </summary>
            public virtual void Spawn()
            {
                drawCount = 0;
                disposed = false;
                lastDelta = Vector2D.NULL_VECTOR;
                solid = true;
                animations = new List<Animation>();

                // Para cada ImageList definido no array de ImageLists passados previamente pelo construtor.
                Dictionary<string, SpriteSheet.FrameSequence>.Enumerator sequences = sheet.GetFrameSequenceEnumerator();
                int animationIndex = 0;
                while (sequences.MoveNext())
                {
                    var pair = sequences.Current;
                    SpriteSheet.FrameSequence sequence = pair.Value;
                    string frameSequenceName = sequence.Name;
                    float fps = DEFAULT_FPS;
                    int initialFrame = 0;
                    bool startVisible = true;
                    bool startOn = true;

                    // Chama o evento OnCreateAnimation() passando os como parâmetros os dados da animação a ser criada.
                    // O evento OnCreateAnimation() poderá ou não redefinir os dados da animação.
                    OnCreateAnimation(animationIndex, ref sheet, ref frameSequenceName, ref fps, ref initialFrame, ref startVisible, ref startOn);

                    if (frameSequenceName != sequence.Name)
                        sequence = sheet.GetFrameSequence(frameSequenceName);

                    // Cria-se a animação com os dados retornados de OnCreateAnimation().
                    if (directional)
                    {
                        animations.Add(new Animation(this, animationIndex, sheet, frameSequenceName, fps, initialFrame, startVisible, startOn));
                        animationIndex++;
                        animations.Add(new Animation(this, animationIndex, sheet, frameSequenceName, fps, initialFrame, startVisible, startOn, true, false));
                        animationIndex++;
                    }
                    else
                    {
                        animations.Add(new Animation(this, animationIndex, sheet, frameSequenceName, fps, initialFrame, startVisible, startOn));
                        animationIndex++;
                    }
                }

                // Inicializa todos os campos
                vel = new Vector2D();
                moving = false;
                markedToRemove = false;
                isStatic = false;
                breakable = true;
                passable = false;
                passStatics = false;
                health = DEFAULT_HEALTH;
                maxDamage = DEFAULT_MAX_DAMAGE;
                invincible = false;
                invincibilityTime = DEFAULT_INVINCIBLE_TIME;
                touchingSprites = new List<Sprite>();
                broke = false;

                engine.addedSprites.Add(this); // Adiciona este sprite a lista de sprites do engine
            }

            /// <summary>
            /// Evento interno que será chamado sempre que o sprite estiver a sofrer um dano.
            /// Classes descententes a esta poderão sobrepor este método para definir o comportamento do dano ou até mesmo cancelá-lo antes mesmo que ele seja processado.
            /// </summary>
            /// <param name="attacker">Atacante, o sprite que irá causar o dano</param>
            /// <param name="region">Retângulo que delimita a área de dano a ser infringida neste sprite pelo atacante</param>
            /// <param name="damage">Quandidade de dano a ser causada pelo atacante. É passado por referência e portanto qualquer alteração deste parâmetro poderá mudar o comportamento do dano sofrido por este sprite.</param>
            /// <returns>true se o dano deverá ser processado, false se o dano deverá ser cancelado</returns>
            protected virtual bool OnTakeDamage(Sprite attacker, Box2D region, ref float damage)
            {
                return true;
            }

            /// <summary>
            /// Evento interno que será chamado sempre que o sprite sofreu um dano.
            /// </summary>
            /// <param name="attacker">Atacante, o sprite que causou o dano</param>
            /// <param name="region">Retângulo que delimita a área de dano infringido neste sprite pelo atacante</param>
            /// <param name="damage">Quantidade de dano causada pelo atacante</param>
            protected virtual void OnTakeDamagePost(Sprite attacker, Box2D region, float damage)
            {
            }

            /// <summary>
            /// Evento interno que será chamado sempre que o HP deste sprite for alterado
            /// </summary>
            /// <param name="health"></param>
            protected virtual void OnHealthChanged(float health)
            {
            }

            /// <summary>
            /// Causa um dano em uma vítima
            /// A área de dano será causada usando o retângulo de colisão do atacante, normalmente o dano só é causado quando os dois estão colidindo, ou melhor dizendo, quando a intersecção do retângulo de colisão do atacante e o retângulo de dano da vítima for não vazia.
            /// </summary>
            /// <param name="victim">Vítima que sofrerá o dano/param>
            /// <param name="damage">Quantidade de dano a ser causada na vítima</param>
            public void Hurt(Sprite victim, float damage)
            {
                //Hurt(victim, collisionBox, damage);
            }

            /// <summary>
            /// Causa um dano numa determinada região de uma vítima
            /// </summary>
            /// <param name="victim">Vítima que sofrerá o dano</param>
            /// <param name="region">Retângulo delimitando a região no qual o dano será aplicado na vítima. Norlammente o dano só é aplicado quando a interseção deste retângulo com o retângulo de dano da vítima for não vazia.</param>
            /// <param name="damage">Quantidade de dano a ser causada na vítima</param>
            public void Hurt(Sprite victim, Box2D region, float damage)
            {
                // Se a vítima já estver quebrada, se estiver marcada para remoção ou seu HP não for maior que zero então não há nada o que se fazer aqui.
                if (victim.broke || victim.markedToRemove || health <= 0)
                    return;

                Box2D intersection = /*victim.hitBox &*/ region; // Calcula a intesecção com a área de dano da vítima e a região dada

                if (intersection.Area() == 0) // Se a intersecção for vazia, não aplica o dano
                    return;

                if (damage > maxDamage) // Verifica se o dano aplicado é maior que o máximo de dano permitido pela vítima
                    damage = maxDamage; // Se for, trunca o dano a ser aplicado

                // Verifica se a vítima não está em modo de invencibilidade e se seu evento OnTakeDamage indica que o dano não deverá ser aplicado
                if (!victim.invincible && victim.OnTakeDamage(this, region, ref damage))
                {
                    // Lembrando também que a chamada ao evento OnTakeDamage pode alterar a quantidade de dano a ser aplicada na vítima
                    float h = victim.health; // Obtém o HP da vítima
                    h -= damage; // Subtrai o HP da vítima pela quantidade de dano a ser aplicada

                    if (h < 0) // Verifica se o resultado é negativo
                        h = 0; // Se for, o HP deverá então ser zero

                    victim.health = h; // Define o HP da vítima com este novo resultado
                    victim.OnHealthChanged(h); // Notifica a vítima de que seu HP foi alterado
                    victim.OnTakeDamagePost(this, region, damage); // Notifica a vítima de que um dano foi causado

                    if (victim.health == 0) // Verifica se o novo HP da vítima é zero
                        victim.Break(); // Se for, quebre-a!
                    else
                        victim.MakeInvincible(); // Senão, aplica a invencibilidade temporária após sofrer o dano
                }
            }

            /// <summary>
            /// Torna este sprite invencível por um determinado período de tempo em segundos
            /// </summary>
            /// <param name="time">Se for positivo, representará o tempo em segundos no qual este sprite ficará invencível, senão será aplicada a invencibilidade usando o tempo de invencibilidade padrão da vítima.</param>
            public void MakeInvincible(float time = 0)
            {
                invincible = true; // Marca o sprite como invencível
                invincibleExpires = engine.GetEngineTime() + (time <= 0 ? invincibilityTime : time); // Calcula o tempo em que a invencibilidade irá acabar

                // Aplica o efeito de pisca pisca em todas as animações deste sprite
                foreach (Animation animation in animations)
                    animation.Flashing = true;
            }

            /// <summary>
            /// Evento que será chamado sempre que este sprite for adicionado na lista de sprites do engine
            /// </summary>
            /// <param name="index">Posição deste sprite na lista de sprites do engine</param>
            public void OnAdded(int index)
            {
                this.index = index;
            }

            /// <summary>
            /// Evento interno que será chamdo sempre que for checada a colisão deste sprite com outro sprite.
            /// Sobreponha este método em classes bases para alterar o comportamento deste sprite com relação a outro sempre que estiverem a colidir.
            /// </summary>
            /// <param name="sprite">Sprite a ser verificado</param>
            /// <returns>true se os dois sprites deverão colidor, false caso contrário. Como padrão este método sempre retornará false indicando que os dois sprites irão colidir</returns>
            protected virtual bool ShouldCollide(Sprite sprite)
            {
                return false;
            }

            /// <summary>
            /// Verifica a colisão deste sprite com os blocos (ou também com qualquer outro sprite marcada como estático)
            /// </summary>
            /// <returns>Vetor de deslocamento deste sprite após verificada as possíveis colisões</returns>
            protected virtual Vector2D DoCheckCollisionWithWorld(Vector2D delta)
            {
                Box2D collisionBox = GetCollisionBox();
                return engine.CheckCollisionWithTiles(collisionBox, delta);
            }

            /// <summary>
            /// Verifica a colisão com os sprites (que não estejam marcados como estáticos)
            /// </summary>
            /// <param name="delta">Vetor de deslocamento</param>
            /// <param name="touching">Lista de sprites que estarão tocando este sprite, usada como retorno</param>
            /// <returns>Um novo vetor de deslocamento</returns>
            protected Vector2D DoCheckCollisionWithSprites(Vector2D delta, List<Sprite> touching)
            {
                //Box2D newBox = collisionBox + delta; // Calcula o vetor de deslocamento inicial
                Vector2D result = delta;

                // Para cada sprite do engine
                /*for (int i = 0; i < engine.sprites.Count; i++)
                {
                    Sprite sprite = engine.sprites[i];

                    // Se ele for eu mesmo, se estiver marcado para remoção ou se ele for estático, não processe nada aqui
                    if (sprite == this || sprite.markedToRemove || sprite.isStatic)
                        continue;

                    Box2D oldIntersection = collisionBox & sprite.CollisionBox; // Calcula a intersecção do retângulo de colisão anterior deste sprite com o do outro sprite

                    if (oldIntersection.Area() != 0) // Se ela for não vazia
                    {
                        touching.Add(sprite); // Adiciona o outro sprite na lista de toques de retorno
                        continue; // Mas não processa colisão aqui
                    }

                    Box2D intersection = newBox & sprite.CollisionBox;

                    if (intersection.Area() != 0) // Processe colisão somente se a intersecção com o retângulo de colisão atual for não vazia
                    {
                        touching.Add(sprite); // Adicionando a lista de toques de retorno

                        if (CollisionCheck(sprite)) // E verificando se haverá colisão
                            result = Vector2D.NULL_VECTOR; // Se ouver, o novo vetor de deslocamento deverá ser nulo
                    }
                }*/

                return result;
            }

            /// <summary>
            /// Evento interno que será chamado sempre que este sprite começar a tocar outro sprite
            /// </summary>
            /// <param name="sprite">Sprite que começou a me tocar</param>
            protected virtual void OnStartTouch(Sprite sprite)
            {
            }

            /// <summary>
            /// Evento interno que será chamado enquanto este sprite estiver tocando outro sprite. A chamada ocorre somente uma vez a cada tick do engine enquanto os dois sprites estiverem se tocando.
            /// </summary>
            /// <param name="sprite">Sprite que está me tocando</param>
            protected virtual void OnTouching(Sprite sprite)
            {
            }

            /// <summary>
            /// Evento interno que será chamado toda vez que este este sprite parar de tocar outro sprite que estava previamente tocando este
            /// </summary>
            /// <param name="sprite">Sprite que deixou de me tocar</param>
            protected virtual void OnEndTouch(Sprite sprite)
            {
            }

            public Box2D CollisionBox
            {
                get
                {
                    return GetCollisionBox();
                }
            }

            protected virtual Box2D GetCollisionBox()
            {
                return new Box2D(origin, Vector2D.NULL_VECTOR, Vector2D.NULL_VECTOR);
            }

            /// <summary>
            /// Realiza as interações físicas deste sprite com os demais elementos do jogo.
            /// </summary>
            private void DoPhysics()
            {
                landed = DoCheckCollisionWithWorld(Vector2D.DOWN_VECTOR) == Vector2D.NULL_VECTOR;

                if (gravity && !landed)
                    vel += new Vector2D(0, GRAVITY * TICKRATE);

                if (vel.Y > TERMINAL_DOWNWARD_SPEED * TICKRATE)
                    vel = new Vector2D(vel.X, TERMINAL_DOWNWARD_SPEED * TICKRATE);

                // Verifica se ele estava se movendo no último frame mas a velocidade atual agora é nula
                if (vel.IsNull && moving)
                {
                    // Se for...
                    StopMoving(); // notifica que o movimento parou
                    engine.Repaint(this); // Notifica o engine que deverá ser feita este sprite deverá ser redesenhado
                }

                //Vector2D delta = !isStatic && !vel.IsNull ? CheckCollisionWithStatics() : Vector2D.NULL_VECTOR; // Verifica a colisão deste sprite com os blocos do jogo e outros sprites marcados como estáticos, retornando o vetor de deslocamento
                Vector2D delta = !isStatic && !vel.IsNull ? TICK * vel : Vector2D.NULL_VECTOR;
                List<Sprite> touching = new List<Sprite>();

                if (!delta.IsNull)
                {
                    if (checkCollisionWithWorld)
                        delta = DoCheckCollisionWithWorld(delta);

                    if (checkCollisionWithSprites)
                        delta = DoCheckCollisionWithSprites(delta, touching); // Verifica a colisão deste sprite com os sprites do jogo, verificando também quais sprites estão tocando este sprite e retornando o vetor de deslocamento
                }

                lastDelta = delta; // Atualiza o vetor de deslocamento global deste sprite

                if (delta != Vector2D.NULL_VECTOR) // Se o deslocamento não for nulo
                {
                    // Translata todos os retângulos deste sprite com base no novo deslocamento
                    //collisionBox += delta; // o de colisão
                    //drawBox += delta; // de desenho
                    //hitBox += delta; // e de dano
                    origin += delta;

                    // Atualiza a partição do engine
                    if (isStatic)
                        engine.partitionStatics.Update(this);
                    else
                        engine.partitionSprites.Update(this);

                    StartMoving(); // Notifica que o sprite começou a se mover, caso ele estivesse parado antes
                    engine.Repaint(this); // Notifica o engine que este sprite deverá ser redesenhado
                }
                else if (moving) // Senão, se ele estava se movendo
                {
                    StopMoving(); // Notifica que ele parou de se mover
                    engine.Repaint(this); // Notifica o engine que este sprite deverá ser redesenhado
                }

                bool lastLanded = landed;
                landed = DoCheckCollisionWithWorld(new Vector2D(0, 1)) == Vector2D.NULL_VECTOR;
                if (landed && !lastLanded)
                    OnLanded();

                // Processa a lista global de sprites que anteriormente estavam tocando esta entidade no frame anterior
                int count = touchingSprites.Count;
                for (int i = 0; i < count; i++)
                {
                    // Se pra cada sprite desta lista
                    Sprite sprite = touchingSprites[i];
                    int index = touching.IndexOf(sprite);

                    if (index == -1) // ele não estiver na lista de toques local (ou seja, não está mais tocando este sprite)
                    {
                        touchingSprites.RemoveAt(i); // então remove-o da lista global
                        i--;
                        count--;
                        OnEndTouch(sprite); // e notifica que ele nao está mais tocando esta sprite
                    }
                    else // Senão
                    {
                        touching.RemoveAt(index); // Remove da lista local de toques
                        OnTouching(sprite); // Notifica que ele continua tocando este sprite
                    }
                }

                // Para cada sprites que sobrou na lista de toques local
                foreach (Sprite sprite in touching)
                {
                    touchingSprites.Add(sprite); // Adiciona-o na lista global de toques
                    OnStartTouch(sprite); // e notifique que ele começou a tocar este sprite
                }
            }

            protected virtual void OnLanded()
            {
            }

            /// <summary>
            /// Verifica se este sprite deverá colidir com outro sprite e vice versa
            /// </summary>
            /// <param name="sprite">Sprite a ser verificado</param>
            /// <returns>true se a colisão deverá ocorrer, false caso contrário</returns>
            protected bool CollisionCheck(Sprite sprite)
            {
                return (ShouldCollide(sprite) || sprite.ShouldCollide(this));
            }

            /// <summary>
            /// Engine
            /// </summary>
            public frmMain Engine
            {
                get
                {
                    return engine;
                }
            }

            /// <summary>
            /// Nome da entidade
            /// </summary>
            public string Name
            {
                get
                {
                    return name;
                }
            }

            /// <summary>
            /// Coordenadas do sprite
            /// </summary>
            public Vector2D Origin
            {
                get
                {
                    return origin;
                }
            }

            public Box2D BoudingBox
            {
                get
                {
                    return origin + boundingBox;
                }
            }

            public Box2D LastBoundingBox
            {
                get
                {
                    return origin + lastBoundingBox;
                }
            }

            /// <summary>
            /// Especifica se o desenho da entidade será feito lado a lado preenchendo o retângulo de desenho
            /// </summary>
            public bool Tiled
            {
                get
                {
                    return tiled;
                }
                set
                {
                    tiled = value;
                    engine.Repaint(this);
                }
            }

            /// <summary>
            /// Especifica a opacidade da entidade, podendo ser utilizado para causar efeito de transparência
            /// </summary>
            public float Opacity
            {
                get
                {
                    return opacity;
                }
                set
                {
                    opacity = value;
                    engine.Repaint(this);
                }
            }

            /// <summary>
            /// Vetor de deslocamento da entidade desde o último tick
            /// </summary>
            public Vector2D LastDelta
            {
                get
                {
                    return lastDelta;
                }
            }

            /// <summary>
            /// Animação correspondente a um determinado índice
            /// </summary>
            /// <param name="index">Índice da animação</param>
            /// <returns>Animação de índice index</returns>
            public Animation GetAnimation(int index)
            {
                if (index == -1)
                    return null;

                return animations[index];
            }

            /// <summary>
            /// Evento interno que ocorrerá sempre que o efeito de fade in for completo
            /// </summary>
            protected virtual void OnFadeInComplete()
            {
            }

            /// <summary>
            /// Evento interno que ocorrerá sempre que o efeito de fade out for completo
            /// </summary>
            protected virtual void OnFadeOutComplete()
            {
            }

            /// <summary>
            /// Evento que ocorrerá uma vez a cada frame (tick) do engine
            /// </summary>
            public virtual void OnFrame()
            {
                // Se ele estiver marcado para remoção não há nada o que se fazer aqui
                if (markedToRemove)
                    return;

                // Realiza o pré-pensamento do sprite. Nesta chamada verifica-se se deveremos continuar a processar as interações deste sprite com o jogo.
                if (!PreThink())
                    return;

                // Se ele não for estático, processa as interações físicas deste sprite com os outros elementos do jogo
                if (!isStatic)
                DoPhysics();

                // Se ele estiver invencível, continue gerando o efeito de pisca pisca
                if (invincible && engine.GetEngineTime() >= invincibleExpires)
                {
                    invincible = false;

                    foreach (Animation animation in animations)
                        animation.Flashing = false;
                }

                Think(); // Realiza o pensamento do sprite, usado para implementação de inteligência artificial

                // Verifica se está ocorrendo o fading
                if (fading)
                {
                    elapsed += TICK; // Atualiza o tempo decorrido desde o início do fading
                    Opacity = fadingIn ? fadingTime - elapsed / fadingTime : elapsed / fadingTime; // Atualiza a opacidade com base no tempo decorrido do fading (tempo inicial = opacidade 1, tempo final = opacidade 0)

                    if (elapsed >= fadingTime) // Verifica se o tempo decorrido atingiu o tempo do fading
                    {
                        fading = false;

                        // Dispara os eventos de completamento de fading
                        if (fadingIn)
                            OnFadeInComplete();
                        else
                            OnFadeOutComplete();
                    }
                }

                PostThink(); // Realiza o pós-pensamento do sprite

                lastBoundingBox = boundingBox;
                boundingBox = Box2D.EMPTY_BOX;

                // Processa cada animação
                foreach (Animation animation in animations)
                {
                    if (animation.Visible)
                        boundingBox |= animation.CurrentFrameBoundingBox;

                    animation.OnFrame();

                    if (animation.Visible)
                        boundingBox |= animation.CurrentFrameBoundingBox;
                }
            }

            /// <summary>
            /// Faz a repintura da entidade
            /// </summary>
            /// <param name="g">Objeto do tipo Graphics que provém as operações de desenho</param>
            public virtual void Paint(Graphics g)
            {
                // Se este objeto já foi disposto (todos seus recursos foram liberados) enão não há nada o que fazer por aqui
                if (disposed)
                    return;

                // Realiza a repintura de cada animação
                foreach (Animation animation in animations)
                    animation.Paint(g);

                drawCount++; // Incrementa o número de desenhos feitos nesta entidade

                if (DEBUG_DRAW_COLLISION_BOX)
                {
                    Box2D collisionBox = CollisionBox;
                    using (System.Drawing.Brush brush = new SolidBrush(Color.FromArgb(128, 0, 255, 0)))
                    {
                        g.FillRectangle(brush, engine.TransformBox(collisionBox));
                    }
                }

                if (DEBUG_SHOW_ENTITY_DRAW_COUNT)
                {
                    System.Drawing.Font font = new System.Drawing.Font("Arial", 16 * engine.drawScale);
                    Box2D drawBox = BoudingBox;
                    using (System.Drawing.Brush brush = new SolidBrush(Color.Yellow))
                    {
                        Vector2D mins = drawBox.Origin + drawBox.Mins;
                        string text = drawCount.ToString();
                        System.Drawing.SizeF size = g.MeasureString(text, font);

                        using (Pen pen = new Pen(Color.Blue, 2))
                        {
                            g.DrawRectangle(pen, (engine.drawOrigin + drawBox * engine.drawScale).ToRectangle());
                        }

                        g.DrawString(text, font, brush, engine.drawOrigin.X + (mins.X + (drawBox.Width - size.Width / engine.drawScale) / 2) * engine.drawScale, engine.drawOrigin.Y + (mins.Y + (drawBox.Height - size.Height / engine.drawScale) / 2) * engine.drawScale);
                    }
                }
            }

            /// <summary>
            /// Evento interno que ocorrerá sempre que uma animação chegar ao fim
            /// </summary>
            /// <param name="animation"></param>
            internal virtual void OnAnimationEnd(Animation animation)
            {
            }

            /// <summary>
            /// Inicia o movimento deste sprite
            /// </summary>
            private void StartMoving()
            {
                if (moving)
                    return;

                moving = true;
                OnStartMoving(); // Notifica que este sprite começou a se mover
            }

            /// <summary>
            /// Para o movimento deste sprite
            /// </summary>
            private void StopMoving()
            {
                if (!moving)
                    return;

                moving = false;
                OnStopMoving(); // Notifica que este sprite parou de se mover
            }

            /// <summary>
            /// Evento interno que é chamado sempre que este sprite começar a se mover
            /// </summary>
            protected virtual void OnStartMoving()
            {
            }

            /// <summary>
            /// Evento interno que é chamado sempre que este sprite parar de se mover
            /// </summary>
            protected virtual void OnStopMoving()
            {
            }

            /// <summary>
            /// Evento interno que é chamado antes de ser processada as interações físicas deste sprite com os demais elementos do jogo.
            /// Sobreponha este evento em suas casses descendentes se desejar controlar o comportamento deste sprite a cada frame do jogo.
            /// </summary>
            /// <returns>true se as interações físicas deverão ser processadas, false caso contrário</returns>
            protected virtual bool PreThink()
            {
                return true;
            }

            /// <summary>
            /// Evento interno que é chamado após as interações físicas deste sprite com os demais elementos do jogo forem feitas e sua posição e velocidade já tiver sido recalculadas.
            /// Sobreponha este evento em suas classes descendentes para simular um quantum de pensamento de um sprite, muito útil para a implementação de inteligência artificial.
            /// </summary>
            protected virtual void Think()
            {
            }

            /// <summary>
            /// Este evento interno é chamado após ocorrerem as interações físicas deste sprite com os demais elementos do jogo, realizada a chamada do evento Think() e feita a chamada do evento OnFrame() da classe Sprite.
            /// Sobreponha este evento em suas classes descendentes se desejar realizar alguma operação final neste sprite antes do próximo tick do jogo.
            /// </summary>
            protected virtual void PostThink()
            {
            }

            /// <summary>
            /// Evento interno que é chamado antes de ocorrer a quebra desta entidade.
            /// Sobreponha este evento se quiser controlar o comportamento da quebra antes que a mesma ocorra, ou mesmo cancela-la.
            /// </summary>
            /// <returns>true se a quebra deverá ser feita, false caso contrário</returns>
            protected virtual bool OnBreak()
            {
                return true;
            }

            /// <summary>
            /// Evento interno que é chamado após ocorrer a quebra deste sprite
            /// </summary>
            protected virtual void OnBroke()
            {
            }

            /// <summary>
            /// Quebra o sprite!
            /// Ao quebra-lo, marca-o como quebrado, mata-o e finalmente lança o evento OnBroke()
            /// </summary>
            public void Break()
            {
                // Verifica se ele já não está quebrado, está marcado para ser removido, se é quebrável e se a chamada ao evento OnBreak() retornou true (indicando que ele deverá ser quebrado)
                if (!broke && !markedToRemove && breakable && OnBreak())
                {
                    broke = true; // Marca-o como quebrado
                    OnBroke(); // Notifica que ele foi quebrado
                    Kill(); // Mate-o!
                }
            }
        }

        /// <summary>
        /// Direção que o jogador poderá assumir no jogo, sendo ela nenhuma ou uma das quatro possíveis direções: esquerda, cima, direita e baixo.
        /// </summary>
        public enum Direction
        {
            NONE = 0, // Nenhuma
            LEFT = 1, // Esquerda
            UP = 2, // Cima
            RIGHT = 4, // Direita
            DOWN = 8 // Baixo
        }

        public enum Key
        {
            NONE = 0,
            LEFT = 1,
            UP = 2,
            RIGHT = 4,
            DOWN = 8,
            SHOT = 16,
            JUMP = 32,
            DASH = 64
        }

        /// <summary>
        /// Coverte uma direção para um número inteiro
        /// </summary>
        /// <param name="direction">Direção</param>
        /// <returns>Número inteiro associado a direção dada</returns>
        public static int DirectionToInt(Direction direction)
        {
            switch (direction)
            {
                case Direction.NONE:
                    return 0;

                case Direction.LEFT:
                    return 1;

                case Direction.UP:
                    return 2;

                case Direction.RIGHT:
                    return 4;

                case Direction.DOWN:
                    return 8;
            }

            throw new InvalidOperationException("There is no integer liked to direction " + direction.ToString() + ".");
        }

        /// <summary>
        /// Converte um número inteiro para uma direção
        /// </summary>
        /// <param name="value">Número inteiro a ser convertido</param>
        /// <returns>Direção associada ao número inteiro dado</returns>
        public static Direction IntToDirection(int value)
        {
            switch (value)
            {
                case 0:
                    return Direction.NONE;

                case 1:
                    return Direction.LEFT;

                case 2:
                    return Direction.UP;

                case 4:
                    return Direction.RIGHT;

                case 8:
                    return Direction.DOWN;
            }

            throw new InvalidOperationException("There is no direction liked to value " + value + ".");
        }

        public class Player : Sprite
        {
            private int lives; // Quantidade de vidas.
            private int keys; // Conjunto de teclas que estão sendo pressionadas no momento.
            private float nextTimeThink; // Tempo (em segundos) de engine no qual deverá ser notificado ao engine o próximo tick do contador de tempo do Bomberman, para que seja atualizado no top panel o tempo do jogo.
            protected bool death; // Indica se o sprite morreu.
            private float nextDeathThink; // Quando o sprite morrer, aplica-se uma animação que terá uma duração específica. Este campo indicará o tempo do engine em que a animação terminará e o sprite será finalmente marcado para ser removido.

            private int currentAnimationIndex;
            private int spawnAnimationIndex;
            private int spawningEndAnimationIndex;
            private int standLeftAnimationIndex;
            private int standRightAnimationIndex;
            private int walkLeftAnimationIndex;
            private int walkRightAnimationIndex;
            private int jumpLeftAnimationIndex;
            private int jumpRightAnimationIndex;
            private int preFallLeftAnimationIndex;
            private int preFallRightAnimationIndex;
            private int fallLeftAnimationIndex;
            private int fallRightAnimationIndex;
            private int landLeftAnimationIndex;
            private int landRightAnimationIndex;

            private bool leftPressed;
            private bool rightPressed;
            private bool upPressed;
            private bool downPressed;
            private bool jumpPressed;
            private bool jumpReleased;
            private bool shotPressed;
            private bool dashPressed;

            private bool spawing;

            private Direction direction = Direction.RIGHT;

            /// <summary>
            /// Cria um novo Bomberman
            /// </summary>
            /// <param name="engine">Engine</param>
            /// <param name="name">Nome do Bomberman</param>
            /// <param name="box">Retângulo de desenho do Bomberman</param>
            /// <param name="imageLists">Array de lista de imagens que serão usadas na animação do Bomberman</param>
            public Player(frmMain engine, string name, Vector2D origin, SpriteSheet sheet)
            // Dado o retângulo de desenho do Bomberman, o retângulo de colisão será a metade deste enquanto o de dano será um pouco menor ainda.
            // A posição do retângulo de colisão será aquela que ocupa a metade inferior do retângulo de desenho enquanto o retângulo de dano terá o mesmo centro que o retângulo de colisão.
            : base(engine, name, origin, sheet, false, true)
            {
                Gravity = true;
                CheckCollisionWithWorld = true;
            }

            protected override Box2D GetCollisionBox()
            {
                return new Box2D(origin, new Vector2D(-HITBOX_WIDTH / 2, -HITBOX_HEIGHT / 2 - 2), new Vector2D(HITBOX_WIDTH / 2, HITBOX_HEIGHT / 2 + 2));
            }

            protected override void OnHealthChanged(float health)
            {
                engine.RepaintHP(); // Notifica o engine que o HP do caracter foi alterado para que seja redesenhado.
            }

            /// <summary>
            /// Quantidade de vidas que o Bomberman possui.
            /// </summary>
            public int Lives
            {
                get
                {
                    return lives;
                }
                set
                {
                    lives = value;
                    engine.RepaintLives();
                }
            }

            protected Animation CurrentAnimation
            {
                get
                {
                    return GetAnimation(currentAnimationIndex);
                }
            }

            protected int CurrentAnimationIndex
            {
                get
                {
                    return currentAnimationIndex;
                }
                set
                {
                    Animation animation = CurrentAnimation;
                    bool animating;
                    int animationFrame;
                    if (animation != null)
                    {
                        animating = animation.Animating;
                        animationFrame = animation.CurrentFrameSequenceIndex;
                        animation.Stop();
                        animation.Visible = false;
                    }
                    else
                    {
                        animating = false;
                        animationFrame = -1;
                    }
                    
                    currentAnimationIndex = value;
                    animation = CurrentAnimation;
                    animation.CurrentFrameSequenceIndex = animationFrame != -1 ? animationFrame : 0;
                    animation.Animating = animating;
                    animation.Visible = true;
                }
            }

            public Direction Direction
            {
                get
                {
                    return direction;
                }
            }

            protected override void OnStartMoving()
            {
            }

            protected override void OnStopMoving()
            {
            }

            protected override void OnLanded()
            {
                if (!spawing)
                {
                    if ((keys & (int) Key.LEFT) != 0)
                        CurrentAnimationIndex = walkLeftAnimationIndex;
                    else if ((keys & (int) Key.RIGHT) != 0)
                        CurrentAnimationIndex = walkRightAnimationIndex;
                    else
                        CurrentAnimationIndex = direction == Direction.LEFT ? landLeftAnimationIndex : landRightAnimationIndex;
                }
                else
                {
                    CurrentAnimationIndex = spawningEndAnimationIndex;
                }

                CurrentAnimation.StartFromBegin();
            }

            public override void Spawn()
            {
                base.Spawn();

                spawing = true;

                lives = INITIAL_LIVES;

                currentAnimationIndex = -1;

                keys = 0;

                nextTimeThink = engine.GetEngineTime() + 1; // Atualiza o tempo em que ocorrerá o próximo tick do relógio que marca o tempo restante de jogo que o Bomberman devera ter. Cada tick é de um segundo.

                invincibilityTime = BOMBERMAN_INVINCIBILITY_TIME;
                //MakeInvincible(); // Toda vez que o Bomberman spawna, ele fica invencível por um determinado tempo.

                CurrentAnimationIndex = spawnAnimationIndex;
                CurrentAnimation.StartFromBegin();
            }

            protected override void OnDeath()
            {
                // Toda vez que o bomberman morre,
                Lives--; // decrementa sua quantidade de vidas.

                engine.PlaySound("TIME_UP"); // Toca o som de morte do Bomberman.

                base.OnDeath(); // Chama o método OnDeath() da classe base.

                if (lives > 0) // Se ele ainda possuir vidas,
                    engine.ScheduleRespawn(this); // respawna o Bomberman.
                else
                    engine.OnGameOver(); // Senão, Game Over!
            }

            protected override void Think()
            {
                base.Think();

                if (!spawing && !Landed)
                {
                    int currentAnimationIndex = CurrentAnimationIndex;
                    if (vel.Y > 10)
                    {
                        CurrentAnimationIndex = direction == Direction.LEFT ? fallLeftAnimationIndex : fallRightAnimationIndex;
                        CurrentAnimation.StartFromBegin();
                        engine.Repaint(this);
                    }
                    else if (vel.Y > -5)
                    {
                        CurrentAnimationIndex = direction == Direction.LEFT ? preFallLeftAnimationIndex : preFallRightAnimationIndex;
                        CurrentAnimation.StartFromBegin();
                        engine.Repaint(this);
                    }
                }
            }

            protected override void OnStartTouch(Sprite sprite)
            {
            }

            /// <summary>
            /// Obtém a primeira direção possível para um dado conjunto de teclas pressionadas.
            /// </summary>
            /// <param name="bits">Conjunto de bits que indicam quais teclas estão sendo pressionadas.</param>
            /// <returns></returns>
            private static Direction FirstDirection(int bits, bool leftRightOnly = true)
            {
                for (int i = 0; i < 4; i++)
                {
                    int mask = 1 << i;

                    if ((bits & mask) != 0)
                    {
                        Direction direction = IntToDirection(mask);
                        if (leftRightOnly && direction != Direction.LEFT && direction != Direction.RIGHT)
                            continue;

                        return direction;
                    }
                }

                return Direction.NONE;
            }

            /// <summary>
            /// Obtém o vetor unitário numa direção dada
            /// </summary>
            /// <param name="direction">Direção que o vetor derá ter</param>
            /// <returns>Vetor unitário na direção de direction</returns>
            public static Vector2D GetVectorDir(Direction direction)
            {
                switch (direction)
                {
                    case Direction.LEFT:
                        return Vector2D.LEFT_VECTOR;

                    case Direction.UP:
                        return Vector2D.UP_VECTOR;

                    case Direction.RIGHT:
                        return Vector2D.RIGHT_VECTOR;

                    case Direction.DOWN:
                        return Vector2D.DOWN_VECTOR;
                }

                return Vector2D.NULL_VECTOR;
            }

            /// <summary>
            /// Atualiza o conjunto de teclas que estão sendo pressionadas.
            /// </summary>
            /// <param name="value">Conjunto de teclas pressionadas.</param>
            private void UpdateKeys(int value)
            {
                if (spawing || death)
                    return;

                int lastKeys = keys;
                keys = value;

                if ((keys & (int) Key.LEFT) != 0)
                    direction = Direction.LEFT;
                else if ((keys & (int) Key.RIGHT) != 0)
                    direction = Direction.RIGHT;


                if ((lastKeys & (int) Key.LEFT) == 0 && (keys & (int) Key.LEFT) != 0)
                {
                    leftPressed = true;

                    vel = new Vector2D(-WALKING_SPEED * TICKRATE, vel.Y);

                    if (Landed)
                        CurrentAnimationIndex = walkLeftAnimationIndex;
                    else
                        UpdateAnimationDirection();
                }
                else if ((lastKeys & (int) Key.LEFT) != 0 && (keys & (int) Key.LEFT) == 0)
                {
                    leftPressed = false;

                    if ((keys & (int) Key.RIGHT) != 0)
                    {
                        leftPressed = true;
                        vel = new Vector2D(WALKING_SPEED * TICKRATE, vel.Y);

                        if (Landed)
                            CurrentAnimationIndex = walkRightAnimationIndex;
                        else
                            UpdateAnimationDirection();
                    }
                    else
                    {
                        vel = new Vector2D(0, vel.Y);

                        if (Landed)
                            CurrentAnimationIndex = standLeftAnimationIndex;
                        else
                            UpdateAnimationDirection();
                    }
                }
                else if ((lastKeys & (int) Key.RIGHT) == 0 && (keys & (int) Key.RIGHT) != 0)
                {
                    rightPressed = true;

                    vel = new Vector2D(WALKING_SPEED * TICKRATE, vel.Y);

                    if (Landed)
                        CurrentAnimationIndex = walkRightAnimationIndex;
                    else
                        UpdateAnimationDirection();
                }
                else if ((lastKeys & (int) Key.RIGHT) != 0 && (keys & (int) Key.RIGHT) == 0)
                {
                    rightPressed = false;

                    if ((keys & (int) Key.LEFT) != 0)
                    {
                        leftPressed = true;
                        vel = new Vector2D(-WALKING_SPEED * TICKRATE, vel.Y);
                        if (Landed)
                            CurrentAnimationIndex = walkLeftAnimationIndex;
                        else
                            UpdateAnimationDirection();
                    }
                    else
                    {
                        vel = new Vector2D(0, vel.Y);
                        if (Landed)
                            CurrentAnimationIndex = standRightAnimationIndex;
                        else
                            UpdateAnimationDirection();
                    }
                }

                if ((lastKeys & (int) Key.JUMP) == 0 && (keys & (int) Key.JUMP) != 0)
                {
                    jumpPressed = true;

                    if (Landed)
                    {
                        jumpReleased = false;
                        vel = new Vector2D(vel.X, -INITIAL_UPWARD_SPEED_FROM_JUMP * TICKRATE);
                        CurrentAnimationIndex = direction == Direction.LEFT ? jumpLeftAnimationIndex : jumpRightAnimationIndex;
                        CurrentAnimation.StartFromBegin();
                    }
                }
                else if ((lastKeys & (int) Key.JUMP) != 0 && (keys & (int) Key.JUMP) == 0)
                {
                    jumpPressed = false;

                    if (!jumpReleased)
                    {
                        jumpReleased = true;
                        vel = new Vector2D(vel.X, 0);
                    }
                }

                engine.Repaint(this);
            }

            private void UpdateAnimationDirection()
            {
                int currentAnimationIndex = CurrentAnimationIndex;
                if (currentAnimationIndex == standLeftAnimationIndex && direction == Direction.RIGHT)
                    CurrentAnimationIndex = standRightAnimationIndex;
                else if (currentAnimationIndex == standRightAnimationIndex && direction == Direction.LEFT)
                    CurrentAnimationIndex = standLeftAnimationIndex;
                else if (currentAnimationIndex == walkLeftAnimationIndex && direction == Direction.RIGHT)
                    CurrentAnimationIndex = walkRightAnimationIndex;
                else if (currentAnimationIndex == walkRightAnimationIndex && direction == Direction.LEFT)
                    CurrentAnimationIndex = walkLeftAnimationIndex;
                else if (currentAnimationIndex == jumpLeftAnimationIndex && direction == Direction.RIGHT)
                    CurrentAnimationIndex = jumpRightAnimationIndex;
                else if (currentAnimationIndex == jumpRightAnimationIndex && direction == Direction.LEFT)
                    CurrentAnimationIndex = jumpLeftAnimationIndex;
                else if (currentAnimationIndex == preFallLeftAnimationIndex && direction == Direction.RIGHT)
                    CurrentAnimationIndex = preFallRightAnimationIndex;
                else if (currentAnimationIndex == preFallRightAnimationIndex && direction == Direction.LEFT)
                    CurrentAnimationIndex = preFallLeftAnimationIndex;
                else if (currentAnimationIndex == fallLeftAnimationIndex && direction == Direction.RIGHT)
                    CurrentAnimationIndex = fallRightAnimationIndex;
                else if (currentAnimationIndex == fallRightAnimationIndex && direction == Direction.LEFT)
                    CurrentAnimationIndex = fallLeftAnimationIndex;
                else if (currentAnimationIndex == landLeftAnimationIndex && direction == Direction.RIGHT)
                    CurrentAnimationIndex = landRightAnimationIndex;
                else if (currentAnimationIndex == landRightAnimationIndex && direction == Direction.LEFT)
                    CurrentAnimationIndex = landLeftAnimationIndex;
            }

            private void MirrorAnimation()
            {
                int currentAnimationIndex = CurrentAnimationIndex;
                if (currentAnimationIndex == standLeftAnimationIndex)
                    CurrentAnimationIndex = standRightAnimationIndex;
                else if (currentAnimationIndex == standRightAnimationIndex)
                    CurrentAnimationIndex = standLeftAnimationIndex;
                else if (currentAnimationIndex == walkLeftAnimationIndex)
                    CurrentAnimationIndex = walkRightAnimationIndex;
                else if (currentAnimationIndex == walkRightAnimationIndex)
                    CurrentAnimationIndex = walkLeftAnimationIndex;
                else if (currentAnimationIndex == jumpLeftAnimationIndex)
                    CurrentAnimationIndex = jumpRightAnimationIndex;
                else if (currentAnimationIndex == jumpRightAnimationIndex)
                    CurrentAnimationIndex = jumpLeftAnimationIndex;
                else if (currentAnimationIndex == preFallLeftAnimationIndex)
                    CurrentAnimationIndex = preFallRightAnimationIndex;
                else if (currentAnimationIndex == preFallRightAnimationIndex)
                    CurrentAnimationIndex = preFallLeftAnimationIndex;
                else if (currentAnimationIndex == fallLeftAnimationIndex)
                    CurrentAnimationIndex = fallRightAnimationIndex;
                else if (currentAnimationIndex == fallRightAnimationIndex)
                    CurrentAnimationIndex = fallLeftAnimationIndex;
                else if (currentAnimationIndex == landLeftAnimationIndex)
                    CurrentAnimationIndex = landRightAnimationIndex;
                else if (currentAnimationIndex == landRightAnimationIndex)
                    CurrentAnimationIndex = landLeftAnimationIndex;
            }

            /// <summary>
            /// Conjunto de teclas que estão sendo pressionadas pelo jogador que controla o Bomberman.
            /// </summary>
            public int Keys
            {
                get
                {
                    return keys;
                }
                set
                {
                    UpdateKeys(value);
                }
            }

            internal override void OnAnimationEnd(Animation animation)
            {
                if (animation.Index == spawningEndAnimationIndex)
                {
                    spawing = false;
                    CurrentAnimationIndex = direction == Direction.LEFT ? standLeftAnimationIndex : standRightAnimationIndex;
                    CurrentAnimation.StartFromBegin();  
                }
                else if (animation.Index == jumpLeftAnimationIndex || animation.Index == jumpRightAnimationIndex)
                {
                    CurrentAnimationIndex = direction == Direction.LEFT ? preFallLeftAnimationIndex : preFallRightAnimationIndex;
                    CurrentAnimation.StartFromBegin();
                }
                else if (animation.Index == landLeftAnimationIndex || animation.Index == landRightAnimationIndex)
                {
                    CurrentAnimationIndex = direction == Direction.LEFT ? standLeftAnimationIndex : standRightAnimationIndex;
                    CurrentAnimation.StartFromBegin();
                }
            }

            protected override void OnCreateAnimation(int animationIndex, ref SpriteSheet sheet, ref string frameSequenceName, ref float fps, ref int initialFrame, ref bool startVisible, ref bool startOn)
            {
                base.OnCreateAnimation(animationIndex, ref sheet, ref frameSequenceName, ref fps, ref initialFrame, ref startVisible, ref startOn);
                startOn = false; // Por padrão, a animação de um jogador começa parada.
                startVisible = false;

                if (frameSequenceName == "Spawn")
                {
                    spawnAnimationIndex = animationIndex;
                }
                else if (frameSequenceName == "SpawnEnd")
                {
                    spawningEndAnimationIndex = animationIndex;
                }
                else if (frameSequenceName == "Stand")
                {
                    standRightAnimationIndex = animationIndex;
                    standLeftAnimationIndex = animationIndex + 1;
                }
                else if (frameSequenceName == "Walking")
                {
                    walkRightAnimationIndex = animationIndex;
                    walkLeftAnimationIndex = animationIndex + 1;
                }
                else if (frameSequenceName == "Jumping")
                {
                    jumpRightAnimationIndex = animationIndex;
                    jumpLeftAnimationIndex = animationIndex + 1;
                }
                else if (frameSequenceName == "PreFalling")
                {
                    preFallRightAnimationIndex = animationIndex;
                    preFallLeftAnimationIndex = animationIndex + 1;
                }
                else if (frameSequenceName == "Falling")
                {
                    fallRightAnimationIndex = animationIndex;
                    fallLeftAnimationIndex = animationIndex + 1;
                }
                else if (frameSequenceName == "Landing")
                {
                    landRightAnimationIndex = animationIndex;
                    landLeftAnimationIndex = animationIndex + 1;
                }
            }
        }

        /// <summary>
        /// Partição da área de desenho do jogo.
        /// Usada para dispor as entidades de forma a acelerar a busca de uma determinada entidade na tela de acordo com um retângulo de desenho especificado.
        /// </summary>
        /// <typeparam name="T">Tipo da entidade (deve descender da classe Sprite)</typeparam>
        private class Partition<T> where T : Sprite
        {
            /// <summary>
            /// Elemento/Célula de uma partição.
            /// A partição é dividida em uma matriz bidimensional de células onde cada uma delas são retângulos iguais.
            /// Cada célula armazena uma lista de entidades que possuem intersecção não vazia com ela, facilitando assim a busca por entidades que possuem intersecção não vazia com um retângulo dado.
            /// </summary>
            /// <typeparam name="U">Tipo da entidade (deve descender da classe Sprite)</typeparam>
            private class PartitionCell<U> where U : Sprite
            {
                Partition<U> partition; // Partição a qual esta célula pertence
                Box2D box; // Retângulo que delimita a célula
                List<U> values; // Lista de entides que possuem intersecção não vazia com esta célula

                /// <summary>
                /// Cria uma nova célula para a partição
                /// </summary>
                /// <param name="partition">Partição a qual esta célula pertence</param>
                /// <param name="box">Retângulo que delimita esta célula</param>
                public PartitionCell(Partition<U> partition, Box2D box)
                {
                    this.partition = partition;
                    this.box = box;

                    values = new List<U>();
                }

                /// <summary>
                /// Insere uma nova entidade nesta célula
                /// </summary>
                /// <param name="value">Entidade a ser adicionada</param>
                public void Insert(U value)
                {
                    if (!values.Contains(value))
                        values.Add(value);
                }

                /// <summary>
                /// Obtém a lista de entidades desta célula que possui intersecção não vazia com um retângulo dado
                /// </summary>
                /// <param name="box">Retângulo usado para pesquisa</param>
                /// <param name="result">Lista de resultados a ser obtido</param>
                public void Query(Box2D box, List<U> result)
                {
                    // Verifica a lista de entidades da célula
                    foreach (U value in values)
                    {
                        Box2D intersection = value.BoudingBox & box; // Calcula a intersecção do retângulo de desenho da entidade com o retângulo de pesquisa

                        if (intersection.Area() != 0 && !result.Contains(value)) // Se a intersecção for não vazia e se a entidade ainda não estiver na lista de resultados
                            result.Add(value); // adiciona esta entidade à lista
                    }
                }

                /// <summary>
                /// Atualiza uma entidade com relaçõa a esta célula, se necessário adicionando-a ou removendo-a da célula
                /// </summary>
                /// <param name="value">Entidade a ser atualizada nesta célula</param>
                public void Update(U value)
                {
                    Box2D intersection = value.BoudingBox & box; // Calcula a interecção
                    bool intersectionNull = intersection.Area() == 0;

                    if (!intersectionNull && !values.Contains(value)) // Se a intersecção for não vazia e a célula ainda não contém esta entidade
                        values.Add(value); // então adiciona-a em sua lista de entidades
                    else if (intersectionNull && values.Contains(value)) // Senão, se a intesecção for vazia e esta entidade ainda está contida neta célula
                        values.Remove(value); // remove-a da sua lista de entidades
                }

                /// <summary>
                /// Remove uma entidade desta célula
                /// </summary>
                /// <param name="value">Entidade a ser removida</param>
                public void Remove(U value)
                {
                    values.Remove(value);
                }

                /// <summary>
                /// Limpa a lista de entidades desta célula
                /// </summary>
                public void Clear()
                {
                    values.Clear();
                }

                /// <summary>
                /// Obtém a quantidade de entidades que possuem intersecção não vazia com esta célula
                /// </summary>
                public int Count
                {
                    get
                    {
                        return values.Count;
                    }
                }
            }

            private Box2D box; // Retângulo que define esta partição
            private int rows; // Número de linhas da subdivisão
            private int cols; // Número de colunas da subdivisão

            private PartitionCell<T>[,] cells; // Matriz da partição
            private float cellWidth; // Largura de cada subdivisão
            private float cellHeight; // Altura de cada subdivisão

            /// <summary>
            /// Cria uma nova partição
            /// </summary>
            /// <param name="left">Coordenada x do topo superior esquerdo da partição</param>
            /// <param name="top">Coordenada y do topo superior esquerdo da partição</param>
            /// <param name="width">Largura da partição</param>
            /// <param name="height">Altura da partição</param>
            /// <param name="rows">Número de linhas da subdivisão da partição</param>
            /// <param name="cols">Número de colunas da subdivisão da partição</param>
            public Partition(float left, float top, float width, float height, int rows, int cols)
            : this(new Box2D(new Vector2D(left, top), Vector2D.NULL_VECTOR, new Vector2D(width, height)), rows, cols)
            {
            }

            /// <summary>
            /// Cria uma nova partição
            /// </summary>
            /// <param name="rect">Retângulo que delimita a partição</param>
            /// <param name="rows">Número de linhas da subdivisão da partição</param>
            /// <param name="cols">Número de colunas da subdivisão da partição</param>
            public Partition(Rectangle rect, int rows, int cols)
            : this(new Box2D(rect), rows, cols)
            {
            }

            /// <summary>
            /// Cria uma nova partição
            /// </summary>
            /// <param name="rect">Retângulo que delimita a partição</param>
            /// <param name="rows">Número de linhas da subdivisão da partição</param>
            /// <param name="cols">Número de colunas da subdivisão da partição</param>
            public Partition(RectangleF rect, int rows, int cols)
            : this(new Box2D(rect), rows, cols)
            {
            }

            /// <summary>
            /// Cria uma nova partição
            /// </summary>
            /// <param name="box">Retângulo que delimita a partição</param>
            /// <param name="rows">Número de linhas da subdivisão da partição</param>
            /// <param name="cols">Número de colunas da subdivisão da partição</param>
            public Partition(Box2D box, int rows, int cols)
            {
                this.box = box;
                this.rows = rows;
                this.cols = cols;

                cellWidth = box.Width / cols; // Calcula a largura de cada subdivisão
                cellHeight = box.Height / rows; // Calcula a altura de cada subdivisão

                cells = new PartitionCell<T>[cols, rows]; // Cria a matriz de subdivisões
            }

            /// <summary>
            /// Insere uma nova entidade a partição
            /// </summary>
            /// <param name="item">Entidade a ser adicionada</param>
            public void Insert(T item)
            {
                Box2D box = item.BoudingBox;

                // Calcula os mínimos e máximos absolutos do retângulo que delimita esta partição
                Vector2D origin = this.box.Origin;
                Vector2D mins = this.box.Mins + origin;
                Vector2D maxs = this.box.Maxs + origin;

                // Calcula os mínimos e máximos absolutos do retângulo de desenho da entidade a ser adicionada
                Vector2D origin1 = box.Origin;
                Vector2D mins1 = box.Mins + origin1;
                Vector2D maxs1 = box.Maxs + origin1;

                int startCol = (int)((mins1.X - mins.X) / cellWidth); // Calcula a coluna da primeira célula a qual interceptará a entidade
                int startRow = (int)((mins1.Y - mins.Y) / cellHeight); // Calcula a primeira linha da primeira célula a qual interceptará a entidade

                int endCol = (int)((maxs1.X - mins.X - 1) / cellWidth); // Calcula a coluna da última célula a qual interceptará a entidade

                if (endCol >= cols)
                    endCol = cols - 1;

                int endRow = (int)((maxs1.Y - mins.Y - 1) / cellHeight); // Calcula a linha da última célula a qual intercepetará a entidade

                if (endRow >= rows)
                    endRow = rows - 1;

                // Varre todas as possíveis células que podem interceptar a entidade dada
                for (int i = startCol; i <= endCol; i++)
                    for (int j = startRow; j <= endRow; j++)
                    {
                        Box2D box1 = new Box2D(new Vector2D(mins.X + cellWidth * i, mins.Y + cellHeight * j), Vector2D.NULL_VECTOR, new Vector2D(cellWidth, cellHeight));
                        Box2D intersection = box1 & box; // Calcula a intesecção

                        if (intersection.Area() == 0) // Se a intesecção for vazia, não precisa adicionar a entidade a célula
                            continue;

                        if (cells[i, j] == null) // Verifica se a célula já foi criada antes, caso não tenha sido ainda então a cria
                            cells[i, j] = new PartitionCell<T>(this, box1);

                        cells[i, j].Insert(item); // Insere a entidade na célula
                    }
            }

            /// <summary>
            /// Realiza uma busca de quais entidades possuem intesecção não vazia com um retângulo dado
            /// </summary>
            /// <param name="box"></param>
            /// <returns></returns>
            public List<T> Query(Box2D box)
            {
                List<T> result = new List<T>();

                // Calcula os máximos e mínimos absulutos do retângulo que delimita esta partição
                Vector2D origin = this.box.Origin;
                Vector2D mins = this.box.Mins + origin;
                Vector2D maxs = this.box.Maxs + origin;

                // Calcula os máximos e mínimos do retângulo de pesquisa
                Vector2D origin1 = box.Origin;
                Vector2D mins1 = box.Mins + origin1;
                Vector2D maxs1 = box.Maxs + origin1;

                int startCol = (int)((mins1.X - mins.X) / cellWidth); // Calcula a coluna da primeira célula a qual deverá ser consultada

                if (startCol < 0)
                    startCol = 0;

                int startRow = (int)((mins1.Y - mins.Y) / cellHeight); // Calcula a primeira linha da primeira célula a qual deverá ser consultada

                if (startRow < 0)
                    startRow = 0;

                int endCol = (int)((maxs1.X - mins.X - 1) / cellWidth); // Calcula a colna da última célula a qual deverá ser consultada

                if (endCol >= cols)
                    endCol = cols - 1;

                int endRow = (int)((maxs1.Y - mins.Y - 1) / cellHeight); // Calcula a linha da última célula a qual deverá ser consultada

                if (endRow >= rows)
                    endRow = rows - 1;

                // Varre todas as possíveis células que poderão ter intersecção não vazia com o retângulo dado
                for (int i = startCol; i <= endCol; i++)
                    for (int j = startRow; j <= endRow; j++)
                        if (cells[i, j] != null) // Para cada célula que já foi previamente criada
                            cells[i, j].Query(box, result); // consulta quais entidades possuem intersecção não vazia com o retângulo dado

                return result;
            }

            /// <summary>
            /// Atualiza uma entidade nesta partição.
            /// Este método deve ser chamado sempre que a entidade tiver sua posição ou dimensões alteradas.
            /// </summary>
            /// <param name="item">Entidade a ser atualizada dentro da partição</param>
            public void Update(T item)
            {
                Vector2D delta = item.LastDelta; // Obtém o vetor de deslocamento da entidade desde o último tick

                if (delta == Vector2D.NULL_VECTOR) // Se a entidade não se deslocou desde o último tick então não há nada o que se fazer aqui
                    return;

                Box2D box = item.BoudingBox; // Obtém o retângulo de desenho atual da entidade
                Box2D box0 = box - delta; // Obtém o retângulo de desenho da entidade antes do deslocamento (do tick anterior)

                // Calcula os máximos e mínimos absolutos do retângulo que delimita esta partição
                Vector2D origin = this.box.Origin;
                Vector2D mins = this.box.Mins + origin;
                Vector2D maxs = this.box.Maxs + origin;

                // Calcula os máximos e mínimos absolutos do rêtângulo de desenho anterior da entidade
                Vector2D origin0 = box0.Origin;
                Vector2D mins0 = box0.Mins + origin0;
                Vector2D maxs0 = box0.Maxs + origin0;

                // Calcula os máximos e mínimos absolutos do retângulo de desenho atual da entidade
                Vector2D origin1 = box.Origin;
                Vector2D mins1 = box.Mins + origin1;
                Vector2D maxs1 = box.Maxs + origin1;

                int startCol = (int)((Math.Min(mins0.X, mins1.X) - mins.X) / cellWidth); // Calcula a coluna da primeira célula para qual deverá ser verificada
                if (startCol < 0)
                    startCol = 0;
                if (startCol >= cols)
                    startCol = cols - 1;

                int startRow = (int)((Math.Min(mins0.Y, mins1.Y) - mins.Y) / cellHeight); // Calcula a linha da primeira célula para a qual deverá ser verificada
                if (startRow < 0)
                    startRow = 0;
                if (startRow >= rows)
                    startRow = rows - 1;

                int endCol = (int)((Math.Max(maxs0.X, maxs1.X) - mins.X - 1) / cellWidth); // Calcula a coluna da útlima célula para qual deverá ser verificada

                if (endCol < 0)
                    endCol = 0;
                if (endCol >= cols)
                    endCol = cols - 1;

                int endRow = (int)((Math.Max(maxs0.Y, maxs1.Y) - mins.Y - 1) / cellHeight); // Calcula a linha da última célula para qual deverá ser verificada

                if (endRow < 0)
                    endRow = 0;
                if (endRow >= rows)
                    endRow = rows - 1;

                // Varre todas as possíveis células que possui ou possuiam intersecção não vazia com a entidade dada
                for (int i = startCol; i <= endCol; i++)
                    for (int j = startRow; j <= endRow; j++)
                        if (cells[i, j] != null) // Se a célula já existir
                        {
                            cells[i, j].Update(item); // Atualiza a entidade dentro da célula

                            if (cells[i, j].Count == 0) // Se a célula não possuir mais entidades, defina como nula
                                cells[i, j] = null;
                        }
                        else
                        {
                            // Senão...
                            Box2D box1 = new Box2D(new Vector2D(mins.X + cellWidth * i, mins.Y + cellHeight * j), Vector2D.NULL_VECTOR, new Vector2D(cellWidth, cellHeight));
                            Box2D intersection = box1 & box; // Calcula a intersecção desta célula com o retângulo de desenho atual da entidade

                            if (intersection.Area() == 0) // Se ela for vazia, não há nada o que fazer nesta célula
                                continue;

                            // Senão...
                            if (cells[i, j] == null) // Verifica se a célula é nula
                                cells[i, j] = new PartitionCell<T>(this, box1); // Se for, cria uma nova célula nesta posição

                            cells[i, j].Insert(item); // e finalmente insere a entidade nesta célula
                        }
            }

            /// <summary>
            /// Remove uma entidade da partição
            /// </summary>
            /// <param name="item">Entidade a ser removida</param>
            public void Remove(T item)
            {
                Box2D box = item.BoudingBox; // Obtém o retângulo de desenho da entidade

                // Calcula os máximos e mínimos absolutos do retângulo que delimita esta partição
                Vector2D origin = this.box.Origin;
                Vector2D mins = this.box.Mins + origin;
                Vector2D maxs = this.box.Maxs + origin;

                // Calcula os máximos e mínimos absolutos do retângulo de desenho da entidade a ser removida
                Vector2D origin1 = box.Origin;
                Vector2D mins1 = box.Mins + origin1;
                Vector2D maxs1 = box.Maxs + origin1;

                int startCol = (int)((mins1.X - mins.X) / cellWidth); // Calcula a coluna da primeira célula a ser verificada
                int startRow = (int)((mins1.Y - mins.Y) / cellHeight); // Calcula a linha da primeira célula a ser verificada

                int endCol = (int)((maxs1.X - mins.X - 1) / cellWidth); // Calcula a coluna da última célula a ser verificada

                if (endCol >= cols)
                    endCol = cols - 1;

                int endRow = (int)((maxs1.Y - mins.Y - 1) / cellHeight); // Calcula a linha da última célula a ser verificada

                if (endRow >= rows)
                    endRow = rows - 1;

                // Varre todas as possíveis células que podem ter intersecção não vazia com a entidade dada
                for (int i = startCol; i <= endCol; i++)
                    for (int j = startRow; j <= endRow; j++)
                        if (cells[i, j] != null)
                        {
                            cells[i, j].Remove(item); // Remove a entidade da célula caso ela possua intersecção não vazia com a célula

                            if (cells[i, j].Count == 0) // Se a célula não possuir mais entidades
                                cells[i, j] = null; // defina-a como nula
                        }
            }

            /// <summary>
            /// Exclui todas as entidades contidas na partição
            /// </summary>
            public void Clear()
            {
                for (int i = 0; i < cols; i++)
                    for (int j = 0; j < rows; j++)
                        if (cells[i, j] != null)
                        {
                            cells[i, j].Clear();
                            cells[i, j] = null;
                        }
            }
        }

        /// <summary>
        /// Classe auxiliar usada para armazenar informações de respawn
        /// </summary>
        public class RespawnEntry
        {
            private Player player; // Bomberman que será respawnado
            private float time; // Tempo que deverá se esperar para que o respawn ocorra

            /// <summary>
            /// Cria uma nova entrada de respawn
            /// </summary>
            /// <param name="bomberman">Bomberman que será respawnado</param>
            /// <param name="time">Tempo que deverá se esperar para que o respawn ocorra</param>
            public RespawnEntry(Player player, float time)
            {
                this.player = player;
                this.time = time;
            }

            /// <summary>
            /// Bomberman que será respawnado
            /// </summary>
            public Player Player
            {
                get
                {
                    return player;
                }
            }

            /// <summary>
            /// Tempo que deverá se esperar para que o respawn ocorra
            /// </summary>
            public float Time
            {
                get
                {
                    return time;
                }
            }
        }

        private AccurateTimer timer;
        private float engineTime;
        private Random random;
        private World world;
        private Player player;
        private List<Sprite> sprites;
        private List<Sprite> addedSprites;
        private List<Sprite> removedSprites;
        private List<RespawnEntry> scheduleRespawns;
        private int currentLevel;
        private int enemyCount;
        private bool changeLevel;
        private int levelToChange;
        private Box2D repaintBox;
        private List<Sprite> drawList;
        private Partition<Sprite> partitionStatics;
        private Partition<Sprite> partitionSprites;
        private Vector2D drawOrigin;
        private float drawScale;
        private Bitmap background;
        private bool gameOver;
        private bool paused;
        private SoundCollection sounds;
        private string currentStageMusic;
        private long lastCurrentMemoryUsage;
        private bool loadingLevel;
        private SpriteSheet xSpriteSheet;

        public frmMain()
        {
            InitializeComponent();
        }

        private void Repaint(Sprite entity)
        {
            if (!drawList.Contains(entity))
                drawList.Add(entity);
        }

        private void Repaint(Box2D box)
        {
            repaintBox |= box;
        }

        private void RepaintAll()
        {
            repaintBox = world.Screen.BoudingBox;
        }

        private void OnFrame()
        {
            bool flag = false;
            lock (this)
            {
                flag = !paused && !loadingLevel;
            }

            int keys = 0;
            if (flag)
            {
                engineTime += TICK;

                if (!paused)
                {
                    if (Keyboard.IsKeyDown(System.Windows.Input.Key.Left))
                        keys |= (int) Key.LEFT;
                    if (Keyboard.IsKeyDown(System.Windows.Input.Key.Up))
                        keys |= (int) Key.UP;
                    if (Keyboard.IsKeyDown(System.Windows.Input.Key.Right))
                        keys |= (int) Key.RIGHT;
                    if (Keyboard.IsKeyDown(System.Windows.Input.Key.Down))
                        keys |= (int) Key.DOWN;
                    if (Keyboard.IsKeyDown(System.Windows.Input.Key.V))
                        keys |= (int) Key.SHOT;
                    if (Keyboard.IsKeyDown(System.Windows.Input.Key.C))
                        keys |= (int) Key.JUMP;
                    if (Keyboard.IsKeyDown(System.Windows.Input.Key.X))
                        keys |= (int) Key.DASH;
                    if (Keyboard.IsKeyDown(System.Windows.Input.Key.Enter))
                        PauseGame();
                }
                else if (Keyboard.IsKeyDown(System.Windows.Input.Key.Enter))
                    ContinueGame();

                int count = scheduleRespawns.Count;

                for (int i = 0; i < count; i++)
                {
                    RespawnEntry entry = scheduleRespawns[i];

                    if (GetEngineTime() >= entry.Time)
                    {
                        scheduleRespawns.RemoveAt(i);
                        i--;
                        count--;

                        string name = entry.Player.Name;
                        int lives = entry.Player.Lives;

                        SpawnPlayer();
                        player.Lives = lives;
                    }
                }

                if (addedSprites.Count > 0)
                {
                    foreach (Sprite added in addedSprites)
                    {
                        sprites.Add(added);

                        if (added.Static)
                            partitionStatics.Insert(added);
                        else
                            partitionSprites.Insert(added);

                        added.OnAdded(sprites.Count - 1);
                        Repaint(added);
                    }

                    addedSprites.Clear();
                }

                if (player != null)
                    player.Keys = keys;

                foreach (Sprite sprite in sprites)
                {
                    if (changeLevel)
                        break;

                    if (!sprite.MarkedToRemove)
                        sprite.OnFrame();
                }

                if (removedSprites.Count > 0)
                {
                    foreach (Sprite removed in removedSprites)
                    {
                        sprites.Remove(removed);

                        if (removed.Static)
                            partitionStatics.Remove(removed);
                        else
                            partitionSprites.Remove(removed);

                        Repaint(removed);
                    }

                    removedSprites.Clear();
                }

                world.OnFrame();
            }

            if (changeLevel)
            {
                changeLevel = false;
                LoadLevel(levelToChange);
            }

            if (repaintBox.Area() > 0)
            {
                Invalidate(TransformBox(repaintBox), false);
                Update();
            }

            foreach (Sprite entity in drawList)
            {
                Box2D drawBox = entity.BoudingBox;
                Box2D lastDrawBox = entity.LastBoundingBox;

                if (drawBox != lastDrawBox)
                {
                    Box2D intersection = lastDrawBox & drawBox;

                    if (intersection.Area() > 0)
                    {
                        Box2D union = lastDrawBox | drawBox;
                        Invalidate(TransformBox(union), false);
                        Update();
                    }
                    else
                    {
                        Invalidate(TransformBox(lastDrawBox), false);
                        Invalidate(TransformBox(drawBox), false);
                        Update();
                    }
                }
                else
                {
                    Invalidate(TransformBox(drawBox), false);
                    Update();
                }
            }

            repaintBox = Box2D.EMPTY_BOX;
            drawList.Clear();

            if (gameOver)
            {

            }
        }

        public Point TransformVector(Vector2D v)
        {
            return ((v - world.Screen.LeftTop) * drawScale + drawOrigin).ToPoint();
        }

        public Point TransformVector(float x, float y)
        {
            return TransformVector(new Vector2D(x, y));
        }

        public Vector2D TransformPoint(Point p)
        {
            return (new Vector2D(p) - drawOrigin) / drawScale + world.Screen.LeftTop;
        }

        public Vector2D TransformPoint(PointF p)
        {
            return (new Vector2D(p) - drawOrigin) / drawScale + world.Screen.LeftTop;
        }

        public Rectangle TransformBox(Box2D box)
        {
            return ((box.LeftTopOrigin() - world.Screen.LeftTop) * drawScale + drawOrigin).ToRectangle();
        }

        public Box2D TransformRectangle(Rectangle rect)
        {
            return ((new Box2D(rect) - drawOrigin) / drawScale).LeftTopOrigin() + world.Screen.LeftTop;
        }

        public Box2D TransformRectangle(RectangleF rect)
        {
            return ((new Box2D(rect) - drawOrigin) / drawScale).LeftTopOrigin() + world.Screen.LeftTop;
        }

        public static Box2D GetBoxFromCell(int row, int col)
        {
            return GetBoxFromBlock(row, col, BLOCK_SIZE, BLOCK_SIZE);
        }

        public static Box2D GetBoxFromBlock(int row, int col, float width, float height)
        {
            float x = (col + INTERNAL_ORIGIN_COL) * BLOCK_SIZE;
            float y = (row + INTERNAL_ORIGIN_ROW) * BLOCK_SIZE;
            return new Box2D(new Vector2D(x, y), Vector2D.NULL_VECTOR, new Vector2D(width, height));
        }

        public float GetEngineTime()
        {
            return engineTime;
        }

        private void RepaintHP()
        {
            Invalidate(TransformBox(DEFAULT_HEART_BOX), false);
            Update();
        }

        public void RepaintLives()
        {
            Invalidate(TransformBox(DEFAULT_LIVES_BOX), false);
            Update();
        }

        public void PlaySound(string soundName, bool loop = false)
        {
            if (sounds != null)
                sounds.Play(soundName, loop);
        }

        public void StopSound(string soundName)
        {
            if (sounds != null)
                sounds.Stop(soundName);
        }

        public void TogglePauseGame()
        {
            if (paused)
                ContinueGame();
            else
                PauseGame();
        }

        public void PauseGame()
        {
            paused = true;
            PlaySound("pause");
            Invalidate();
        }

        public void ContinueGame()
        {
            paused = false;
            Invalidate();
        }

        public void ScheduleRespawn(Player player)
        {
            ScheduleRespawn(player, RESPAWN_TIME);
        }

        public void ScheduleRespawn(Player player, float time)
        {
            scheduleRespawns.Add(new RespawnEntry(player, GetEngineTime() + time));
        }

        public void NextLevel()
        {
            changeLevel = true;
            levelToChange = currentLevel + 1;
        }

        private void OnGameOver()
        {
            gameOver = true;
            //nextGameOverThink = engineTime + GAME_OVER_PANEL_SHOW_DELAY;
        }

        private void SpawnPlayer()
        {
            player = new Player(this, "X", new Vector2D(SCREEN_WIDTH * 0.5f, 0), xSpriteSheet);
            player.Spawn();
            //player.Velocity = new Vector2D(WALKING_SPEED * TICKRATE, 0);
            world.Screen.FocusOn = player;
        }

        public void LoadLevel(int level)
        {
            paused = false;
            lock (this)
            {
                loadingLevel = true;

                UnloadLevel();

                currentLevel = level;

                Bitmap bmp = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("Mega_Man_Xvan.resources.tiles.Gator_Stage_Floor_Block.png"));
                world.AddRectangle((int)(SCREEN_HEIGHT * 0.75f / TILESET_SIZE) + 35, 0, 4, 40, bmp, CollisionData.SOLID);
                world.AddRectangle((int) (SCREEN_HEIGHT * 0.75f / TILESET_SIZE) + 30, 40, 4, 4, bmp, CollisionData.SOLID);
                world.AddRectangle((int) (SCREEN_HEIGHT * 0.75f / TILESET_SIZE) + 25, 44, 4, 4, bmp, CollisionData.SOLID);
                world.AddRectangle((int) (SCREEN_HEIGHT * 0.75f / TILESET_SIZE) + 20, 48, 4, 4, bmp, CollisionData.SOLID);
                world.AddRectangle((int) (SCREEN_HEIGHT * 0.75f / TILESET_SIZE) + 15, 52, 4, 4, bmp, CollisionData.SOLID);
                world.AddRectangle((int) (SCREEN_HEIGHT * 0.75f / TILESET_SIZE) + 10, 56, 4, 4, bmp, CollisionData.SOLID);
                world.AddRectangle((int) (SCREEN_HEIGHT * 0.75f / TILESET_SIZE) + 5, 60, 4, 4, bmp, CollisionData.SOLID);
                world.AddRectangle((int) (SCREEN_HEIGHT * 0.75f / TILESET_SIZE) + 0, 64, 4, 32, bmp, CollisionData.SOLID);
                world.AddRectangle((int) (SCREEN_HEIGHT * 0.75f / TILESET_SIZE) - 8, 64, 4, 16, bmp, CollisionData.SOLID);
                world.AddRectangle((int) (SCREEN_HEIGHT * 0.75f / TILESET_SIZE) - 12, 80, 4, 16, bmp, CollisionData.SOLID);
                world.AddRectangle((int) (SCREEN_HEIGHT * 0.75f / TILESET_SIZE) + 0, 96, 128, 4, bmp, CollisionData.SOLID);
                world.AddRectangle((int) (SCREEN_HEIGHT * 0.75f / TILESET_SIZE) + 128, 96, 4, 256, bmp, CollisionData.SOLID);
                world.AddRectangle((int) (SCREEN_HEIGHT * 0.75f / TILESET_SIZE) + 0, (int)(SCREEN_WIDTH * 0.75f / TILESET_SIZE) - 44, 4, 48, bmp, CollisionData.SOLID);
                world.AddRectangle(world.TileSetRowCount - 4, 0, 4, world.TileSetColCount, bmp, CollisionData.SOLID);

                Player oldPlayer = player;
                SpawnPlayer();

                if (oldPlayer != null)
                {
                    player.Lives = oldPlayer.Lives;
                }

                loadingLevel = false;
            }

            Invalidate();
        }

        private void UnloadLevel()
        {
            foreach (Sprite sprite in addedSprites)
                sprite.Dispose();

            foreach (Sprite sprite in sprites)
                sprite.Dispose();

            sprites.Clear();
            addedSprites.Clear();
            removedSprites.Clear();
            scheduleRespawns.Clear();
            partitionStatics.Clear();
            partitionSprites.Clear();

            long currentMemoryUsage = GC.GetTotalMemory(true);
            long delta = currentMemoryUsage - lastCurrentMemoryUsage;
            Debug.WriteLine("**************************Total memory: {0}({1}{2})", currentMemoryUsage, delta > 0 ? "+" : delta < 0 ? "-" : "", delta);
            lastCurrentMemoryUsage = currentMemoryUsage;
        }

        private void UpdateScale()
        {
            float width = ClientRectangle.Width;
            float height = ClientRectangle.Height;

            if (width / height < SIZE_RATIO)
            {
                drawScale = width / DEFAULT_CLIENT_WIDTH;
                float newHeight = drawScale * DEFAULT_CLIENT_HEIGHT;
                drawOrigin = new Vector2D(0, (height - newHeight) / 2);
            }
            else
            {
                drawScale = height / DEFAULT_CLIENT_HEIGHT;
                float newWidth = drawScale * DEFAULT_CLIENT_WIDTH;
                drawOrigin = new Vector2D((width - newWidth) / 2, 0);
            }
        }

        private void TimerTick1()
        {
            if (timer.IsRunning)
                OnFrame();
            else
                Close();
        }

        private void frmMain_Paint(object sender, PaintEventArgs e)
        {
            if (world == null)
                return;

            Graphics g = e.Graphics;
            //g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle clipRect = e.ClipRectangle;
            Box2D drawBox = TransformRectangle(clipRect); // Obtém a draw box já com a transformação escalar aplicada

            // Desenha o fundo
            if ((DEFAULT_GAME_AREA_BOX & drawBox).Area() != 0)
                if (background != null)
                    g.DrawImage(background, clipRect, drawBox.ToRectangle(), GraphicsUnit.Pixel);
                else
                    using (System.Drawing.Brush brush = new SolidBrush(Color.Black))
                    {
                        g.FillRectangle(brush, clipRect);
                    }

            world.PaintDownLayer(g, drawBox);

            // Desenha os sprites
            List<Sprite> sprites = partitionSprites.Query(drawBox);

            foreach (Sprite sprite in sprites)
                if (sprite != player && !sprite.MarkedToRemove)
                    sprite.Paint(g);

            // Desenha o X
            if ((player.BoudingBox & drawBox).Area() != 0)
                player.Paint(g);

            world.PaintUpLayer(g, drawBox);

            if (DEBUG_DRAW_CLIPRECT)
                using (Pen pen = new Pen(Color.Yellow, 2))
                {
                    g.DrawRectangle(pen, clipRect.X, clipRect.Y, clipRect.Width, clipRect.Height);
                }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            loadingLevel = true;

            drawOrigin = DEFAULT_DRAW_ORIGIN;
            drawScale = DEFAULT_DRAW_SCALE;

            engineTime = 0;

            random = new Random();
            world = new World(this, 4096, 4096);
            sprites = new List<Sprite>();
            addedSprites = new List<Sprite>();
            removedSprites = new List<Sprite>();
            scheduleRespawns = new List<RespawnEntry>();

            changeLevel = false;

            drawList = new List<Sprite>();

            partitionStatics = new Partition<Sprite>(DEFAULT_CLIENT_RECT, ROW_COUNT, COL_COUNT);
            partitionSprites = new Partition<Sprite>(DEFAULT_CLIENT_RECT, ROW_COUNT, COL_COUNT);

            ClientSize = new Size((int)DEFAULT_CLIENT_WIDTH, (int)DEFAULT_CLIENT_HEIGHT);
            UpdateScale();

            xSpriteSheet = new SpriteSheet("X", new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("Mega_Man_Xvan.resources.sprites.X.X.png")));

            var sequence = xSpriteSheet.AddFrameSquence("Spawn");
            sequence.Offset = HITBOX_SIZE / 2;
            sequence.AddFrame(1, 40, 5, 15, 8, 48);

            sequence = xSpriteSheet.AddFrameSquence("SpawnEnd");
            sequence.Offset = HITBOX_SIZE / 2;
            sequence.AddFrame(1, 32, 5, 15, 8, 48);
            sequence.AddFrame(22, 32, 19, 34, 22, 29, 2);
            sequence.AddFrame(54, 32, 46, 21, 30, 42);
            sequence.AddFrame(92, 32, 84, 24, 30, 39);
            sequence.AddFrame(128, 32, 120, 27, 30, 36);
            sequence.AddFrame(164, 32, 156, 28, 30, 34);
            sequence.AddFrame(199, 32, 191, 31, 30, 32, 3);

            sequence = xSpriteSheet.AddFrameSquence("Stand", 0);
            sequence.Offset = HITBOX_SIZE / 2;
            sequence.AddFrame(234, 32, 226, 29, 30, 34);
            sequence.AddFrame(269, 32, 261, 29, 30, 34);
            sequence.AddFrame(303, 32, 295, 29, 30, 34);
            sequence.AddFrame(338, 32, 330, 29, 30, 34);

            sequence = xSpriteSheet.AddFrameSquence("Shooting", 0);
            sequence.Offset = HITBOX_SIZE / 2;
            sequence.AddFrame(378, 62, 365, 29, 30, 34);
            sequence.AddFrame(414, 62, 402, 29, 29, 34);

            sequence = xSpriteSheet.AddFrameSquence("Walking", 6);
            sequence.Offset = HITBOX_SIZE / 2;
            sequence.AddFrame(235, 32, 226, 29, 30, 34);
            sequence.AddFrame(13, 70, 5, 67, 30, 34, 5);
            sequence.AddFrame(51, 70, 50, 67, 20, 34);
            sequence.AddFrame(78, 71, 75, 67, 23, 35, 2);
            sequence.AddFrame(112, 71, 105, 68, 32, 34, 3);
            sequence.AddFrame(155, 70, 145, 68, 34, 33, 3);
            sequence.AddFrame(195, 70, 190, 68, 26, 33, 3);
            sequence.AddFrame(225, 70, 222, 67, 22, 34, 2);
            sequence.AddFrame(253, 71, 248, 67, 25, 35, 2);
            sequence.AddFrame(285, 70, 280, 67, 30, 34, 3);
            sequence.AddFrame(326, 70, 318, 68, 34, 33, 3);
            sequence.AddFrame(366, 70, 359, 68, 29, 33, 3);
            sequence.AddFrame(51, 70, 50, 67, 20, 34);

            sequence = xSpriteSheet.AddFrameSquence("ShootWalking", 0);
            sequence.Offset = HITBOX_SIZE / 2;
            sequence.AddFrame(19, 101, 41, 107, 29, 34);
            sequence.AddFrame(58, 101, 76, 107, 32, 35);
            sequence.AddFrame(86, 101, 115, 108, 35, 34);
            sequence.AddFrame(120, 101, 159, 108, 38, 33);
            sequence.AddFrame(162, 101, 204, 108, 34, 33);
            sequence.AddFrame(203, 101, 246, 107, 31, 34);
            sequence.AddFrame(232, 101, 284, 107, 33, 35);
            sequence.AddFrame(260, 101, 326, 107, 35, 34);
            sequence.AddFrame(293, 101, 369, 108, 37, 33);
            sequence.AddFrame(334, 101, 413, 108, 35, 33);

            sequence = xSpriteSheet.AddFrameSquence("Jumping", 0);
            sequence.Offset = HITBOX_SIZE / 2;
            //sequence.AddFrame(234, 27, 226, 29, 30, 34);
            sequence.AddFrame(5, 148, 5, 148, 25, 37, 3);
            sequence.AddFrame(32, 149, 37, 148, 15, 41, 4);

            sequence = xSpriteSheet.AddFrameSquence("PreFalling", 0);
            sequence.Offset = HITBOX_SIZE / 2;
            sequence.AddFrame(55, 151, 56, 146, 19, 46);

            sequence = xSpriteSheet.AddFrameSquence("Falling", 4);
            sequence.Offset = HITBOX_SIZE / 2;
            sequence.AddFrame(82, 155, 80, 150, 23, 41, 4);
            sequence.AddFrame(113, 156, 108, 150, 27, 42);

            sequence = xSpriteSheet.AddFrameSquence("Landing", 0);
            sequence.Offset = HITBOX_SIZE / 2;
            sequence.AddFrame(140, 153, 139, 151, 24, 38, 2);
            sequence.AddFrame(174, 154, 166, 153, 30, 32, 2);

            LoadLevel(INITIAL_LEVEL);

            timer = new AccurateTimer(this, new Action(TimerTick1), (int)(1000 * TICK));
        }

        private Vector2D CheckCollisionWithTiles(Box2D collisionBox, Vector2D dir)
        {
            return world.CheckCollision(collisionBox, dir);
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            UpdateScale();
            Invalidate();
        }
    }
}
