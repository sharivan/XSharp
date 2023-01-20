using System;
using System.Collections.Generic;
using System.IO;

using SharpDX.Direct3D9;

using MMX.Math;
using MMX.Geometry;

using static MMX.Engine.Consts;
using static MMX.Engine.World.World;

using MMXBox = MMX.Geometry.Box;

namespace MMX.Engine.Entities
{
    public abstract class Sprite : Entity, IDisposable
    {
        protected List<Animation> animations; // Animações
        private int currentAnimationIndex;
        protected bool solid; // Especifica se a entidade será solida ou não a outros elementos do jogo.
        private bool fading; // Especifica se o efeito de fading está ativo
        private bool fadingIn; // Se o efeito de fading estiver ativo, especifica se o tipo de fading em andamento é um fading in
        private int fadingTime; // Se o efeito de fading estiver ativo, especifica o tempo do fading
        private int elapsed; // Se o efeito de fading estiver ativo, indica o tempo decorrido desde o início do fading       

        protected BoxCollider collider;

        protected Vector vel; // Velocidade
        protected bool moving; // Indica se o sprite está continuou se movendo desde a última iteração física com os demais elementos do jogo
        protected bool isStatic; // Indica se o sprite é estático
        protected bool breakable; // Indica se ele pode ser quebrado
        protected int health; // HP do sprite
        protected bool invincible; // Indica se o sprite está invencível, não podendo assim sofrer danos
        protected int invincibilityTime; // Indica o tempo de invencibilidade do sprite quando ele estiver invencível
        private long invincibleExpires; // Indica o instante no qual a invencibilidade do sprite irá terminar. Tal instante é dado em segundos e é relativo ao tempo de execução do engine.
        protected bool broke; // Indica se este sprite foi quebrado

        protected bool skipPhysics;

        public string Name
        {
            get;
            private set;
        }

        public int SpriteSheetIndex
        {
            get;
            private set;
        }

        public bool Directional
        {
            get;
            private set;
        }

        public SpriteSheet Sheet => Engine.GetSpriteSheet(SpriteSheetIndex);

        /// <summary>
        /// Cria uma nova entidade
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="name">Nome da entidade</param>
        /// <param name="tiled">true se o desenho desta entidade será preenchido em sua área de pintura lado a lado</param>
        protected Sprite(GameEngine engine, string name, Vector origin, int spriteSheetIndex, bool directional = false) :
            base(engine, origin)
        {
            Name = name;
            SpriteSheetIndex = spriteSheetIndex;
            Directional = directional;

            PaletteIndex = -1;
            Opacity = 1; // Opacidade 1 significa que não existe transparência (opacidade 1 = opacidade 100% = transparência 0%)

            animations = new List<Animation>();
            collider = new BoxCollider(engine.World, CollisionBox);
        }

        public override void LoadState(BinaryReader reader)
        {
            base.LoadState(reader);

            currentAnimationIndex = reader.ReadInt32();
            Opacity = reader.ReadSingle();

            int animationCount = reader.ReadInt32();
            for (int i = 0; i < animationCount; i++)
            {
                Animation animation = animations[i];
                animation.LoadState(reader);
            }

            solid = reader.ReadBoolean();
            fading = reader.ReadBoolean();
            fadingIn = reader.ReadBoolean();
            fadingTime = reader.ReadInt32();
            elapsed = reader.ReadInt32();
            CheckCollisionWithSprites = reader.ReadBoolean();
            CheckCollisionWithWorld = reader.ReadBoolean();

            vel = new Vector(reader);
            NoClip = reader.ReadBoolean();
            moving = reader.ReadBoolean();
            isStatic = reader.ReadBoolean();
            breakable = reader.ReadBoolean();
            health = reader.ReadInt32();
            invincible = reader.ReadBoolean();
            invincibilityTime = reader.ReadInt32();
            invincibleExpires = reader.ReadInt64();
            broke = reader.ReadBoolean();
        }

        public override void SaveState(BinaryWriter writer)
        {
            base.SaveState(writer);

            writer.Write(currentAnimationIndex);
            writer.Write(Opacity);

            if (animations != null)
            {
                writer.Write(animations.Count);
                foreach (Animation animation in animations)
                    animation.SaveState(writer);
            }
            else
                writer.Write(0);

            writer.Write(solid);
            writer.Write(fading);
            writer.Write(fadingIn);
            writer.Write(fadingTime);
            writer.Write(elapsed);
            writer.Write(CheckCollisionWithSprites);
            writer.Write(CheckCollisionWithWorld);

            vel.Write(writer);
            writer.Write(NoClip);
            writer.Write(moving);
            writer.Write(isStatic);
            writer.Write(breakable);
            writer.Write(health);
            writer.Write(invincible);
            writer.Write(invincibilityTime);
            writer.Write(invincibleExpires);
            writer.Write(broke);
        }

        protected Animation CurrentAnimation => GetAnimation(currentAnimationIndex);

        protected int CurrentAnimationIndex
        {
            get => currentAnimationIndex;
            set
            {
                Animation animation = CurrentAnimation;
                bool animating;
                int animationFrame;
                if (animation != null)
                {
                    animating = animation.Animating;
                    animationFrame = animation.CurrentSequenceIndex;
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
                animation.CurrentSequenceIndex = animationFrame != -1 ? animationFrame : 0;
                animation.Animating = animating;
                animation.Visible = true;
            }
        }

        /// <summary>
        /// Indica se este sprite é estático
        /// </summary>
        public bool Static => isStatic;

        public bool NoClip
        {
            get;
            set;
        }

        /// <summary>
        /// Indica se este sprite ainda está se movendo desde a última interação física com os demais sprites do jogo
        /// </summary>
        public bool Moving => moving;

        /// <summary>
        /// Indica se este sprite foi quebrado
        /// </summary>
        public bool Broke => broke;

        /// <summary>
        /// Indica se este sprite está no modo de invencibilidade
        /// </summary>
        public bool Invincible => invincible;

        /// <summary>
        /// HP deste sprite
        /// </summary>
        public int Health
        {
            get => health;
            set
            {
                health = value;
                OnHealthChanged(health); // Lança o evento notificando a mudança do HP

                if (health == 0) // Se o HP for zero
                    Break(); // quebre-a!
            }
        }

        /// <summary>
        /// Vetor velocidade deste sprite
        /// </summary>
        public Vector Velocity
        {
            get => vel;
            set => vel = value;
        }

        public BoxCollider Collider
        {
            get
            {
                collider.Box = CollisionBox;
                return collider;
            }
        }

        public FixedSingle Gravity => GetGravity();

        public FixedSingle TerminalDownwardSpeed => GetTerminalDownwardSpeed();

        public bool CheckCollisionWithSprites
        {
            get;
            set;
        }

        public bool CheckCollisionWithWorld
        {
            get;
            set;
        }

        public bool BlockedUp => !NoClip && collider.BlockedUp;

        public bool BlockedLeft => !NoClip && collider.BlockedLeft;

        public bool BlockedRight => !NoClip && collider.BlockedRight;

        public bool Landed => !NoClip && collider.Landed && vel.Y >= 0;

        public bool LandedOnSlope => !NoClip && collider.LandedOnSlope;

        public bool LandedOnTopLadder => !NoClip && collider.LandedOnTopLadder;

        public RightTriangle LandedSlope => collider.LandedSlope;

        public bool Underwater => !NoClip && collider.Underwater;

        public bool CanGoOutOfMapBounds
        {
            get;
            set;
        }

        public int PaletteIndex
        {
            get;
            set;
        }

        public Texture Palette => Engine.GetPalette(PaletteIndex);

        /// <summary>
        /// Evento interno que ocorrerá toda vez que uma animação estiver a ser criada.
        /// Seus parâmetros (exceto animationIndex) são passados por referencia de forma que eles podem ser alterados dentro do método e assim definir qual será o comportamento da animação antes que ela seja criada.
        /// </summary>
        /// <param name="animationIndex">Índice da animação</param>
        /// <param name="imageList">Lista de imagens contendo cada quadro usado pela animação</param>
        /// <param name="fps">Número de quadros por segundo</param>
        /// <param name="initialSequenceIndex">Quadro inicial</param>
        /// <param name="startVisible">true se a animação iniciará visível, false caso contrário</param>
        /// <param name="startOn">true se a animação iniciará em execução, false caso contrário</param>
        /// <param name="loop">true se a animação executará em looping, false caso contrário</param>
        protected virtual void OnCreateAnimation(int animationIndex, SpriteSheet sheet, ref string frameSequenceName, ref int initialSequenceIndex, ref bool startVisible, ref bool startOn, ref bool add)
        {
        }

        public override string ToString() => "Sprite [" + Name + ", " + Origin + "]";

        /// <summary>
        /// Aplica um efeito de fade in
        /// </summary>
        /// <param name="time">Tempodo fading</param>
        public void FadeIn(int time)
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
        public void FadeOut(int time)
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
        public override void Spawn()
        {
            base.Spawn();

            solid = true;

            // Para cada ImageList definido no array de ImageLists passados previamente pelo construtor.
            Dictionary<string, SpriteSheet.FrameSequence>.Enumerator sequences = Sheet.GetFrameSequenceEnumerator();
            int animationIndex = 0;
            while (sequences.MoveNext())
            {
                var pair = sequences.Current;
                SpriteSheet.FrameSequence sequence = pair.Value;
                string frameSequenceName = sequence.Name;
                int initialFrame = 0;
                bool startVisible = false;
                bool startOn = true;
                bool add = true;

                // Chama o evento OnCreateAnimation() passando os como parâmetros os dados da animação a ser criada.
                // O evento OnCreateAnimation() poderá ou não redefinir os dados da animação.
                OnCreateAnimation(animationIndex, Sheet, ref frameSequenceName, ref initialFrame, ref startVisible, ref startOn, ref add);

                if (add)
                {
                    if (frameSequenceName != sequence.Name)
                        sequence = Sheet.GetFrameSequence(frameSequenceName);

                    // Cria-se a animação com os dados retornados de OnCreateAnimation().
                    if (Directional)
                    {
                        animations.Add(new Animation(this, animationIndex, SpriteSheetIndex, frameSequenceName, initialFrame, startVisible, startOn));
                        animationIndex++;
                        animations.Add(new Animation(this, animationIndex, SpriteSheetIndex, frameSequenceName, initialFrame, startVisible, startOn, true, false));
                        animationIndex++;
                    }
                    else
                    {
                        animations.Add(new Animation(this, animationIndex, SpriteSheetIndex, frameSequenceName, initialFrame, startVisible, startOn));
                        animationIndex++;
                    }
                }
            }

            // Inicializa todos os campos
            vel = Vector.NULL_VECTOR;
            NoClip = false;
            moving = false;
            isStatic = false;
            breakable = true;
            health = DEFAULT_HEALTH;
            invincible = false;
            invincibilityTime = DEFAULT_INVINCIBLE_TIME;
            broke = false;

            currentAnimationIndex = -1;
        }

        /// <summary>
        /// Evento interno que será chamado sempre que o sprite estiver a sofrer um dano.
        /// Classes descententes a esta poderão sobrepor este método para definir o comportamento do dano ou até mesmo cancelá-lo antes mesmo que ele seja processado.
        /// </summary>
        /// <param name="attacker">Atacante, o sprite que irá causar o dano</param>
        /// <param name="region">Retângulo que delimita a área de dano a ser infringida neste sprite pelo atacante</param>
        /// <param name="damage">Quandidade de dano a ser causada pelo atacante. É passado por referência e portanto qualquer alteração deste parâmetro poderá mudar o comportamento do dano sofrido por este sprite.</param>
        /// <returns>true se o dano deverá ser processado, false se o dano deverá ser cancelado</returns>
        protected virtual bool OnTakeDamage(Sprite attacker, MMXBox region, ref int damage) => true;

        /// <summary>
        /// Evento interno que será chamado sempre que o sprite sofreu um dano.
        /// </summary>
        /// <param name="attacker">Atacante, o sprite que causou o dano</param>
        /// <param name="region">Retângulo que delimita a área de dano infringido neste sprite pelo atacante</param>
        /// <param name="damage">Quantidade de dano causada pelo atacante</param>
        protected virtual void OnTakeDamagePost(Sprite attacker, MMXBox region, FixedSingle damage)
        {
        }

        /// <summary>
        /// Evento interno que será chamado sempre que o HP deste sprite for alterado
        /// </summary>
        /// <param name="health"></param>
        protected virtual void OnHealthChanged(FixedSingle health)
        {
        }

        /// <summary>
        /// Causa um dano em uma vítima
        /// A área de dano será causada usando o retângulo de colisão do atacante, normalmente o dano só é causado quando os dois estão colidindo, ou melhor dizendo, quando a intersecção do retângulo de colisão do atacante e o retângulo de dano da vítima for não vazia.
        /// </summary>
        /// <param name="victim">Vítima que sofrerá o dano/param>
        /// <param name="damage">Quantidade de dano a ser causada na vítima</param>
        public void Hurt(Sprite victim, FixedSingle damage)
        {
            //Hurt(victim, collisionBox, damage);
        }

        /// <summary>
        /// Causa um dano numa determinada região de uma vítima
        /// </summary>
        /// <param name="victim">Vítima que sofrerá o dano</param>
        /// <param name="region">Retângulo delimitando a região no qual o dano será aplicado na vítima. Norlammente o dano só é aplicado quando a interseção deste retângulo com o retângulo de dano da vítima for não vazia.</param>
        /// <param name="damage">Quantidade de dano a ser causada na vítima</param>
        public void Hurt(Sprite victim, MMXBox region, int damage)
        {
            // Se a vítima já estver quebrada, se estiver marcada para remoção ou seu HP não for maior que zero então não há nada o que se fazer aqui.
            if (victim.broke || victim.markedToRemove || health <= 0)
                return;

            MMXBox intersection = /*victim.hitBox &*/ region; // Calcula a intesecção com a área de dano da vítima e a região dada

            if (!intersection.IsValid()) // Se a intersecção for vazia, não aplica o dano
                return;

            //if (damage > maxDamage) // Verifica se o dano aplicado é maior que o máximo de dano permitido pela vítima
            //    damage = maxDamage; // Se for, trunca o dano a ser aplicado

            // Verifica se a vítima não está em modo de invencibilidade e se seu evento OnTakeDamage indica que o dano não deverá ser aplicado
            if (!victim.invincible && victim.OnTakeDamage(this, region, ref damage))
            {
                // Lembrando também que a chamada ao evento OnTakeDamage pode alterar a quantidade de dano a ser aplicada na vítima
                int h = victim.health; // Obtém o HP da vítima
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
        public void MakeInvincible(int time = 0)
        {
            invincible = true; // Marca o sprite como invencível
            invincibleExpires = Engine.GetEngineTime() + (time <= 0 ? invincibilityTime : time); // Calcula o tempo em que a invencibilidade irá acabar
        }

        /// <summary>
        /// Evento interno que será chamdo sempre que for checada a colisão deste sprite com outro sprite.
        /// Sobreponha este método em classes bases para alterar o comportamento deste sprite com relação a outro sempre que estiverem a colidir.
        /// </summary>
        /// <param name="sprite">Sprite a ser verificado</param>
        /// <returns>true se os dois sprites deverão colidor, false caso contrário. Como padrão este método sempre retornará false indicando que os dois sprites irão colidir</returns>
        protected virtual bool ShouldCollide(Sprite sprite) => false;

        /// <summary>
        /// Verifica a colisão com os sprites (que não estejam marcados como estáticos)
        /// </summary>
        /// <param name="delta">Vetor de deslocamento</param>
        /// <param name="touching">Lista de sprites que estarão tocando este sprite, usada como retorno</param>
        /// <returns>Um novo vetor de deslocamento</returns>
        protected Vector DoCheckCollisionWithSprites(Vector delta)
        {
            //MMXBox newBox = collisionBox + delta; // Calcula o vetor de deslocamento inicial
            Vector result = delta;

            // Para cada sprite do engine
            /*for (int i = 0; i < engine.sprites.Count; i++)
            {
                Sprite sprite = engine.sprites[i];

                // Se ele for eu mesmo, se estiver marcado para remoção ou se ele for estático, não processe nada aqui
                if (sprite == this || sprite.markedToRemove || sprite.isStatic)
                    continue;

                MMXBox oldIntersection = collisionBox & sprite.CollisionBox; // Calcula a intersecção do retângulo de colisão anterior deste sprite com o do outro sprite

                if (oldIntersection.Area() != 0) // Se ela for não vazia
                {
                    touching.Add(sprite); // Adiciona o outro sprite na lista de toques de retorno
                    continue; // Mas não processa colisão aqui
                }

                MMXBox intersection = newBox & sprite.CollisionBox;

                if (intersection.Area() != 0) // Processe colisão somente se a intersecção com o retângulo de colisão atual for não vazia
                {
                    touching.Add(sprite); // Adicionando a lista de toques de retorno

                    if (CollisionCheck(sprite)) // E verificando se haverá colisão
                        result = MMXVector.NULL_VECTOR; // Se ouver, o novo vetor de deslocamento deverá ser nulo
                }
            }*/

            return result;
        }

        protected override MMXBox GetHitBox() => CollisionBox;

        public MMXBox CollisionBox => Origin + GetCollisionBox();

        protected abstract MMXBox GetCollisionBox();

        protected override MMXBox GetBoundingBox() => DrawBox;

        private void MoveAlongSlope(BoxCollider collider, RightTriangle slope, FixedSingle dx, bool gravity = true)
        {
            FixedSingle h = slope.HCathetusVector.X;
            int slopeSign = h.Signal;
            int dxs = dx.Signal;
            bool goingDown = dxs == slopeSign;

            var dy = (FixedSingle) (((FixedDouble) slope.VCathetus * dx / slope.HCathetus).Abs * dxs * slopeSign);
            var delta = new Vector(dx, dy);
            collider.MoveContactSolid(delta, dx.Abs, (goingDown ? Direction.NONE : Direction.UP) | (dxs > 0 ? Direction.RIGHT : Direction.LEFT), CollisionFlags.SLOPE);

            if (gravity)
                collider.MoveContactFloor(TILE_SIZE / 2 * QUERY_MAX_DISTANCE);

            if (collider.Landed)
                collider.AdjustOnTheFloor();
        }

        private void MoveX(BoxCollider collider, FixedSingle deltaX, bool gravity = true, bool followSlopes = true)
        {
            var dx = new Vector(deltaX, 0);

            MMXBox lastBox = collider.Box;
            bool wasLanded = collider.Landed;
            bool wasLandedOnSlope = collider.LandedOnSlope;
            RightTriangle lastSlope = collider.LandedSlope;
            MMXBox lastLeftCollider = collider.LeftCollider;
            MMXBox lastRightCollider = collider.RightCollider;

            collider.Translate(dx);

            //if (wasLanded)
            //{
            if (collider.Landed)
                collider.AdjustOnTheFloor(TILE_SIZE / 2 * QUERY_MAX_DISTANCE);
            else if (gravity && wasLanded)
                collider.TryMoveContactSlope(TILE_SIZE / 2 * QUERY_MAX_DISTANCE);
            //}

            MMXBox union = deltaX > 0 ? lastRightCollider | collider.RightCollider : lastLeftCollider | collider.LeftCollider;
            CollisionFlags collisionFlags = Engine.GetCollisionFlags(union, CollisionFlags.NONE, true, deltaX > 0 ? CollisionSide.RIGHT_WALL : deltaX < 0 ? CollisionSide.LEFT_WALL : CollisionSide.NONE);

            if (!CanBlockTheMove(collisionFlags))
            {
                if (gravity && followSlopes && wasLanded)
                {
                    if (collider.LandedOnSlope)
                    {
                        RightTriangle slope = collider.LandedSlope;
                        FixedSingle h = slope.HCathetusVector.X;
                        if (h > 0 && deltaX > 0 || h < 0 && deltaX < 0)
                        {
                            FixedSingle x = lastBox.Origin.X;
                            FixedSingle stx = deltaX > 0 ? slope.Left : slope.Right;
                            FixedSingle stx_x = stx - x;
                            if (deltaX > 0 && stx_x > 0 && stx_x <= deltaX || deltaX < 0 && stx_x < 0 && stx_x >= deltaX)
                            {
                                deltaX -= stx_x;
                                dx = new Vector(deltaX, 0);

                                collider.Box = lastBox;
                                if (wasLandedOnSlope)
                                    MoveAlongSlope(collider, lastSlope, stx_x);
                                else
                                    collider.Translate(new Vector(stx_x, 0));

                                MoveAlongSlope(collider, slope, deltaX);
                            }
                            else
                            {
                                if (wasLandedOnSlope)
                                {
                                    collider.Box = lastBox;
                                    MoveAlongSlope(collider, lastSlope, deltaX);
                                }
                            }
                        }
                        else
                        {
                            if (wasLandedOnSlope)
                            {
                                collider.Box = lastBox;
                                MoveAlongSlope(collider, lastSlope, deltaX);
                            }
                        }
                    }
                    else if (Engine.GetCollisionFlags(collider.DownCollider, CollisionFlags.NONE, false, CollisionSide.FLOOR).HasFlag(CollisionFlags.SLOPE))
                        collider.MoveContactFloor();
                }
            }
            else if (collisionFlags.HasFlag(CollisionFlags.SLOPE))
            {
                if (collider.LandedOnSlope)
                {
                    RightTriangle slope = collider.LandedSlope;
                    FixedSingle x = lastBox.Origin.X;
                    FixedSingle stx = deltaX > 0 ? slope.Left : slope.Right;
                    FixedSingle stx_x = stx - x;
                    if (deltaX > 0 && stx_x < 0 && stx_x >= deltaX || deltaX < 0 && stx_x > 0 && stx_x <= deltaX)
                    {
                        deltaX -= stx_x;
                        dx = new Vector(deltaX, 0);

                        collider.Box = lastBox;
                        if (wasLandedOnSlope)
                            MoveAlongSlope(collider, lastSlope, stx_x);
                        else
                            collider.Translate(new Vector(stx_x, 0));

                        MoveAlongSlope(collider, slope, deltaX);
                    }
                    else
                    {
                        if (wasLandedOnSlope)
                        {
                            collider.Box = lastBox;
                            MoveAlongSlope(collider, lastSlope, deltaX);
                        }
                    }
                }
                else if (!wasLanded)
                {
                    collider.Box = lastBox;
                    if (deltaX > 0)
                        collider.MoveContactSolid(Vector.RIGHT_VECTOR, deltaX.Ceil(), Direction.RIGHT, CollisionFlags.NONE);
                    else
                        collider.MoveContactSolid(Vector.LEFT_VECTOR, (-deltaX).Ceil(), Direction.LEFT, CollisionFlags.NONE);
                }
            }
            else
            {
                collider.Box = lastBox;
                if (deltaX > 0)
                    collider.MoveContactSolid(Vector.RIGHT_VECTOR, deltaX.Ceil(), Direction.RIGHT, CollisionFlags.NONE);
                else
                    collider.MoveContactSolid(Vector.LEFT_VECTOR, (-deltaX).Ceil(), Direction.LEFT, CollisionFlags.NONE);
            }
        }

        private bool IsTouchingASlope(MMXBox box, out RightTriangle slope, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE) => Engine.ComputedLandedState(box, out slope, maskSize, ignore) == CollisionFlags.SLOPE;

        /// <summary>
        /// Realiza as interações físicas deste sprite com os demais elementos do jogo.
        /// </summary>
        private void DoPhysics()
        {
            collider.Box = CollisionBox;

            bool lastBlockedUp = BlockedUp;
            bool lastBlockedLeft = BlockedLeft;
            bool lastBlockedRight = BlockedRight;
            bool lastLanded = Landed;

            FixedSingle gravity = Gravity;
            if (!skipPhysics)
            {
                // Verifica se ele estava se movendo no último frame mas a velocidade atual agora é nula
                if (vel.IsNull && moving)
                {
                    // Se for...
                    StopMoving(); // notifica que o movimento parou
                }

                Vector delta = !isStatic && !vel.IsNull ? vel : Vector.NULL_VECTOR;

                if (!delta.IsNull)
                {
                    if (!NoClip && CheckCollisionWithWorld)
                    {
                        if (delta.X != 0)
                        {
                            if (collider.LandedOnSlope /*&& followSlopes*/)
                            {
                                if (collider.LandedSlope.HCathetusSign == delta.X.Signal) // Se está descendo um declive
                                {
                                    if (gravity != 0) // Apenas desça o declive se ouver gravidade
                                    {
                                        MoveAlongSlope(collider, collider.LandedSlope, delta.X, gravity != 0);
                                    }
                                }
                                else // caso contrário, está subindo
                                {
                                    MoveAlongSlope(collider, collider.LandedSlope, delta.X, gravity != 0);
                                }
                            }
                            else
                                MoveX(collider, delta.X, gravity != 0);
                        }

                        if (delta.Y != 0)
                        {
                            var dy = new Vector(0, delta.Y);
                            MMXBox lastBox = collider.Box;
                            MMXBox lastUpCollider = collider.UpCollider;
                            MMXBox lastDownCollider = collider.DownCollider;
                            collider.Translate(dy);

                            if (dy.Y > 0)
                            {
                                MMXBox union = lastDownCollider | collider.DownCollider;
                                if (CanBlockTheMove(Engine.GetCollisionFlags(union, CollisionFlags.NONE, true, CollisionSide.FLOOR)))
                                {
                                    collider.Box = lastBox;
                                    collider.MoveContactFloor(dy.Y.Ceil());
                                }
                            }
                            else
                            {
                                MMXBox union = lastUpCollider | collider.UpCollider;
                                if (CanBlockTheMove(Engine.GetCollisionFlags(union, CollisionFlags.NONE, true, CollisionSide.CEIL)))
                                {
                                    collider.Box = lastBox;
                                    collider.MoveContactSolid(dy, (-dy.Y).Ceil(), Direction.UP);
                                }
                            }
                        }

                        delta = collider.Box.Origin - CollisionBox.Origin;
                    }

                    if (!NoClip && CheckCollisionWithSprites)
                        delta = DoCheckCollisionWithSprites(delta); // Verifica a colisão deste sprite com os sprites do jogo, verificando também quais sprites estão tocando este sprite e retornando o vetor de deslocamento
                }

                if (delta != Vector.NULL_VECTOR) // Se o deslocamento não for nulo
                {
                    Vector newOrigin = Origin + delta;

                    FixedSingle x = newOrigin.X;
                    FixedSingle y = newOrigin.Y;

                    if (!CanGoOutOfMapBounds)
                    {
                        MMXBox limit = Engine.World.BoundingBox;
                        if (!Engine.noCameraConstraints)
                            limit &= Engine.cameraConstraintsBox.ClipTop(-2 * HITBOX_HEIGHT).ClipBottom(-2 * HITBOX_HEIGHT);

                        MMXBox collisionBox = newOrigin + GetCollisionBox();

                        FixedSingle minX = collisionBox.Left;
                        FixedSingle limitLeft = limit.Left;
                        if (minX < limitLeft)
                            x -= minX - limitLeft;

                        FixedSingle minY = collisionBox.Top;
                        FixedSingle limitTop = limit.Top;
                        if (minY < limitTop)
                            y -= minY - limitTop;
                        
                        FixedSingle maxX = collisionBox.Right;
                        FixedSingle limitRight = limit.Right;
                        if (maxX > limitRight)
                            x += limitRight - maxX;

                        FixedSingle maxY = collisionBox.Bottom;
                        FixedSingle limitBottom = limit.Bottom;
                        if (maxY > limitBottom)
                            y += limitBottom - maxY;
                    }

                    Origin = new Vector(x, y);

                    StartMoving(); // Notifica que o sprite começou a se mover, caso ele estivesse parado antes
                }
                else if (moving) // Senão, se ele estava se movendo
                {
                    StopMoving(); // Notifica que ele parou de se mover
                }

                if (!NoClip && !isStatic)
                {
                    if (gravity != 0)
                    {
                        vel += gravity * Vector.DOWN_VECTOR;

                        FixedSingle terminalDownwardSpeed = TerminalDownwardSpeed;
                        if (vel.Y > terminalDownwardSpeed)
                            vel = new Vector(vel.X, terminalDownwardSpeed);
                    }

                    if (CheckCollisionWithWorld && collider.Landed && vel.Y > 0)
                        vel = vel.XVector;
                }
            }

            if (!Landed && vel.Y > gravity && vel.Y < 2 * gravity)
                vel = new Vector(vel.X, gravity);

            if (CheckCollisionWithWorld)
            {
                if (BlockedUp && !lastBlockedUp)
                    OnBlockedUp();

                if (BlockedLeft && !lastBlockedLeft)
                    OnBlockedLeft();

                if (BlockedRight && !lastBlockedRight)
                    OnBlockedRight();

                if (Landed && !lastLanded)
                    OnLanded();
            }
        }

        public virtual FixedSingle GetGravity() => Underwater ? UNDERWATER_GRAVITY : GRAVITY;

        protected virtual FixedSingle GetTerminalDownwardSpeed() => Underwater ? UNDERWATER_TERMINAL_DOWNWARD_SPEED : TERMINAL_DOWNWARD_SPEED;

        protected virtual void OnBlockedRight()
        {
        }

        protected virtual void OnBlockedLeft()
        {
        }

        protected virtual void OnBlockedUp()
        {
        }

        protected virtual void OnLanded()
        {
        }

        /// <summary>
        /// Verifica se este sprite deverá colidir com outro sprite e vice versa
        /// </summary>
        /// <param name="sprite">Sprite a ser verificado</param>
        /// <returns>true se a colisão deverá ocorrer, false caso contrário</returns>
        protected bool CollisionCheck(Sprite sprite) => ShouldCollide(sprite) || sprite.ShouldCollide(this);

        public MMXBox DrawBox => CurrentAnimation != null ? CurrentAnimation.DrawBox : Origin + MMXBox.EMPTY_BOX;

        /// <summary>
        /// Especifica a opacidade da entidade, podendo ser utilizado para causar efeito de transparência
        /// </summary>
        public float Opacity
        {
            get;
            set;
        }

        /// <summary>
        /// Animação correspondente a um determinado índice
        /// </summary>
        /// <param name="index">Índice da animação</param>
        /// <returns>Animação de índice index</returns>
        public Animation GetAnimation(int index) => animations == null || index < 0 || index >= animations.Count ? null : animations[index];

        protected override bool PreThink()
        {
            skipPhysics = false;
            return base.PreThink();
        }

        /// <summary>
        /// Evento que ocorrerá uma vez a cada frame (tick) do engine
        /// </summary>
        protected override void Think()
        {
            // Se ele não for estático, processa as interações físicas deste sprite com os outros elementos do jogo
            if (!isStatic && !Engine.Paused)
                DoPhysics();
        }

        protected override void PostThink()
        {
            base.PostThink();

            if (Engine.Paused)
                return;

            // Se ele estiver invencível, continue gerando o efeito de pisca pisca
            if (invincible && Engine.GetEngineTime() >= invincibleExpires)
            {
                invincible = false;
            }

            // Processa cada animação
            foreach (Animation animation in animations)
                animation.OnFrame();
        }

        /// <summary>
        /// Faz a repintura da entidade
        /// </summary>
        /// <param name="g">Objeto do tipo Graphics que provém as operações de desenho</param>
        public virtual void Render()
        {
            // Se este objeto já foi disposto (todos seus recursos foram liberados) enão não há nada o que fazer por aqui
            if (!alive || markedToRemove)
                return;

            // Realiza a repintura de cada animação
            foreach (Animation animation in animations)
                animation.Render();
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
        /// Evento interno que é chamado antes de ocorrer a quebra desta entidade.
        /// Sobreponha este evento se quiser controlar o comportamento da quebra antes que a mesma ocorra, ou mesmo cancela-la.
        /// </summary>
        /// <returns>true se a quebra deverá ser feita, false caso contrário</returns>
        protected virtual bool OnBreak() => true;

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
            if (alive && !broke && !markedToRemove && breakable && OnBreak())
            {
                broke = true; // Marca-o como quebrado
                OnBroke(); // Notifica que ele foi quebrado
                Kill(); // Mate-o!
            }
        }

        protected override void OnDeath()
        {
            animations.Clear();
            base.OnDeath();
        }

        internal void OnDeviceReset()
        {
            foreach (var animation in animations)
                animation.OnDeviceReset();
        }
    }
}
