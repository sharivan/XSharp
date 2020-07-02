using MMX.Geometry;
using MMX.Math;
using System;
using System.Collections.Generic;
using System.IO;

using static MMX.Engine.Consts;

namespace MMX.Engine
{
    public abstract class Sprite : Entity, IDisposable
    {
        protected string name; // Nome da entidade
        protected Box lastDrawBox;
        protected Box drawBox; // Retângulo que delimita a àrea máxima de desenho do sprite
        protected List<Animation> animations; // Animações
        private bool tiled; // Especifica se a imagem (ou a animação) desta entidade será desenhanda com preenchimento lado a lado dentro da área de desenho
        private bool directional;

        private float opacity; // Opacidade da imagem (ou animação). Usada para efeito de fading.
        protected bool solid; // Especifica se a entidade será solida ou não a outros elementos do jogo.
        private bool fading; // Especifica se o efeito de fading está ativo
        private bool fadingIn; // Se o efeito de fading estiver ativo, especifica se o tipo de fading em andamento é um fading in
        private int fadingTime; // Se o efeito de fading estiver ativo, especifica o tempo do fading
        private int elapsed; // Se o efeito de fading estiver ativo, indica o tempo decorrido desde o início do fading
        protected bool disposed; // Indica se os recursos associados a esta entidade foram liberados
        private int drawCount; // Armazena a quantidade de pinturas feita pela entidade desde sua criação. Usado somente para depuração.
        private bool checkCollisionWithSprites;
        private bool checkCollisionWithWorld;

        protected BoxCollider collider;

        internal SpriteSheet sheet; // Sprite sheet a ser usado na animação deste sprite

        protected Vector vel; // Velocidade
        private bool noClip;
        protected bool moving; // Indica se o sprite está continuou se movendo desde a última iteração física com os demais elementos do jogo
        protected bool isStatic; // Indica se o sprite é estático
        protected bool breakable; // Indica se ele pode ser quebrado
        protected int health; // HP do sprite
        protected bool invincible; // Indica se o sprite está invencível, não podendo assim sofrer danos
        protected int invincibilityTime; // Indica o tempo de invencibilidade do sprite quando ele estiver invencível
        private long invincibleExpires; // Indica o instante no qual a invencibilidade do sprite irá terminar. Tal instante é dado em segundos e é relativo ao tempo de execução do engine.
        protected bool broke; // Indica se este sprite foi quebrado

        protected bool skipPhysics;

        /// <summary>
        /// Cria uma nova entidade
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="name">Nome da entidade</param>
        /// <param name="tiled">true se o desenho desta entidade será preenchido em sua área de pintura lado a lado</param>
        protected Sprite(GameEngine engine, string name, Vector origin, SpriteSheet sheet, bool tiled = false, bool directional = false):
            base(engine, origin)
        {
            this.name = name;
            this.sheet = sheet;
            this.tiled = tiled;
            this.directional = directional;

            opacity = 1; // Opacidade 1 significa que não existe transparência (opacidade 1 = opacidade 100% = transparência 0%)

            collider = new BoxCollider(engine.World, CollisionBox);
        }

        public override void LoadState(BinaryReader reader)
        {
            base.LoadState(reader);

            opacity = reader.ReadSingle();
            lastDrawBox = new Box(reader);
            drawBox = new Box(reader);

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
            disposed = reader.ReadBoolean();
            drawCount = reader.ReadInt32();
            checkCollisionWithSprites = reader.ReadBoolean();
            checkCollisionWithWorld = reader.ReadBoolean();

            vel = new Vector(reader);
            noClip = reader.ReadBoolean();
            moving = reader.ReadBoolean();
            markedToRemove = reader.ReadBoolean();
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

            writer.Write(opacity);
            lastDrawBox.Write(writer);
            drawBox.Write(writer);

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
            writer.Write(disposed);
            writer.Write(drawCount);
            writer.Write(checkCollisionWithSprites);
            writer.Write(checkCollisionWithWorld);

            vel.Write(writer);
            writer.Write(noClip);
            writer.Write(moving);
            writer.Write(markedToRemove);
            writer.Write(isStatic);
            writer.Write(breakable);
            writer.Write(health);
            writer.Write(invincible);
            writer.Write(invincibilityTime);
            writer.Write(invincibleExpires);
            writer.Write(broke);
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

        public bool NoClip
        {
            get
            {
                return noClip;
            }

            set
            {
                noClip = value;
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
        public int Health
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
        /// Vetor velocidade deste sprite
        /// </summary>
        public Vector Velocity
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

        public BoxCollider Collider
        {
            get
            {
                collider.Box = CollisionBox;
                return collider;
            }
        }

        public FixedSingle Gravity
        {
            get
            {
                return GetGravity();
            }
        }

        public FixedSingle TerminalDownwardSpeed
        {
            get
            {
                return GetTerminalDownwardSpeed();
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

        public bool BlockedUp
        {
            get
            {
                return !noClip && collider.BlockedUp;
            }
        }

        public bool BlockedLeft
        {
            get
            {
                return !noClip && collider.BlockedLeft;
            }
        }

        public bool BlockedRight
        {
            get
            {
                return !noClip && collider.BlockedRight;
            }
        }

        public bool Landed
        {
            get
            {
                return !noClip && collider.Landed && vel.Y >= 0;
            }
        }

        public bool LandedOnSlope
        {
            get
            {
                return !noClip && collider.LandedOnSlope;
            }
        }

        public bool LandedOnTopLadder
        {
            get
            {
                return !noClip && collider.LandedOnTopLadder;
            }
        }

        public RightTriangle LandedSlope
        {
            get
            {
                return collider.LandedSlope;
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
            engine.removedEntities.Add(this);

            if (!disposed)
            {
                animations.Clear();
                disposed = true;
            }

            //engine.Repaint(this);
        }
        /// <summary>
        /// Evento interno que é lançado sempre que o sprite for morto
        /// </summary>
        protected virtual void OnDeath()
        {
            Dispose(); // Por padrão ele apenas dispões ele da memória, liberando todos os recursos associados a ele
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
        protected virtual void OnCreateAnimation(int animationIndex, ref SpriteSheet sheet, ref string frameSequenceName, ref int initialFrame, ref bool startVisible, ref bool startOn)
        {
        }

        public override string ToString()
        {
            return "Sprite [" + name + ", " + Origin + "]";
        }

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
        public virtual void Spawn()
        {
            drawCount = 0;
            disposed = false;
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
                int initialFrame = 0;
                bool startVisible = true;
                bool startOn = true;

                // Chama o evento OnCreateAnimation() passando os como parâmetros os dados da animação a ser criada.
                // O evento OnCreateAnimation() poderá ou não redefinir os dados da animação.
                OnCreateAnimation(animationIndex, ref sheet, ref frameSequenceName, ref initialFrame, ref startVisible, ref startOn);

                if (frameSequenceName != sequence.Name)
                    sequence = sheet.GetFrameSequence(frameSequenceName);

                // Cria-se a animação com os dados retornados de OnCreateAnimation().
                if (directional)
                {
                    animations.Add(new Animation(this, animationIndex, sheet, frameSequenceName, initialFrame, startVisible, startOn));
                    animationIndex++;
                    animations.Add(new Animation(this, animationIndex, sheet, frameSequenceName, initialFrame, startVisible, startOn, true, false));
                    animationIndex++;
                }
                else
                {
                    animations.Add(new Animation(this, animationIndex, sheet, frameSequenceName, initialFrame, startVisible, startOn));
                    animationIndex++;
                }
            }

            // Inicializa todos os campos
            vel = new Vector();
            noClip = false;
            moving = false;
            markedToRemove = false;
            isStatic = false;
            breakable = true;
            health = DEFAULT_HEALTH;
            invincible = false;
            invincibilityTime = DEFAULT_INVINCIBLE_TIME;            
            broke = false;

            engine.addedEntities.Add(this); // Adiciona este sprite a lista de sprites do engine
        }

        /// <summary>
        /// Evento interno que será chamado sempre que o sprite estiver a sofrer um dano.
        /// Classes descententes a esta poderão sobrepor este método para definir o comportamento do dano ou até mesmo cancelá-lo antes mesmo que ele seja processado.
        /// </summary>
        /// <param name="attacker">Atacante, o sprite que irá causar o dano</param>
        /// <param name="region">Retângulo que delimita a área de dano a ser infringida neste sprite pelo atacante</param>
        /// <param name="damage">Quandidade de dano a ser causada pelo atacante. É passado por referência e portanto qualquer alteração deste parâmetro poderá mudar o comportamento do dano sofrido por este sprite.</param>
        /// <returns>true se o dano deverá ser processado, false se o dano deverá ser cancelado</returns>
        protected virtual bool OnTakeDamage(Sprite attacker, Box region, ref int damage)
        {
            return true;
        }

        /// <summary>
        /// Evento interno que será chamado sempre que o sprite sofreu um dano.
        /// </summary>
        /// <param name="attacker">Atacante, o sprite que causou o dano</param>
        /// <param name="region">Retângulo que delimita a área de dano infringido neste sprite pelo atacante</param>
        /// <param name="damage">Quantidade de dano causada pelo atacante</param>
        protected virtual void OnTakeDamagePost(Sprite attacker, Box region, FixedSingle damage)
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
        public void Hurt(Sprite victim, Box region, int damage)
        {
            // Se a vítima já estver quebrada, se estiver marcada para remoção ou seu HP não for maior que zero então não há nada o que se fazer aqui.
            if (victim.broke || victim.markedToRemove || health <= 0)
                return;

            Box intersection = /*victim.hitBox &*/ region; // Calcula a intesecção com a área de dano da vítima e a região dada

            if (intersection.Area == 0) // Se a intersecção for vazia, não aplica o dano
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
            invincibleExpires = engine.GetEngineTime() + (time <= 0 ? invincibilityTime : time); // Calcula o tempo em que a invencibilidade irá acabar
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
        /*protected virtual MMXVector DoCheckCollisionWithWorld(MMXVector delta, CollisionFlags ignore = CollisionFlags.NONE, MMXFloat clipBottom = 0)
        {
            MMXBox collisionBox = GetCollisionBox(clipBottom);
            return engine.CheckCollisionWithTiles(collisionBox, delta, ignore);
        }*/

        protected CollisionFlags GetTouchingFlags(Vector delta, FixedSingle clipBottom, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true)
        {
            return GetTouchingFlags(delta, out RightTriangle slope, clipBottom, ignore, preciseCollisionCheck);
        }

        protected virtual CollisionFlags GetTouchingFlags(Vector delta, out RightTriangle slopeTriangle, FixedSingle clipBottom, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true)
        {
            Box collisionBox = Origin + GetCollisionBox(clipBottom);
            return engine.GetTouchingFlags(collisionBox, delta, out slopeTriangle, ignore, preciseCollisionCheck);
        }

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

        public Box CollisionBox
        {
            get
            {
                return Origin + GetCollisionBox();
            }
        }

        protected Box GetCollisionBox()
        {
            return GetCollisionBox(FixedSingle.ZERO);
        }

        protected abstract Box GetCollisionBox(FixedSingle clipBottom);

        protected override Box GetBoundingBox()
        {
            return CollisionBox;
        }

        private void MoveAlongSlope(BoxCollider collider, RightTriangle slope, FixedSingle dx, bool gravity = true)
        {
            FixedSingle h = slope.HCathetusVector.X;
            int slopeSign = h.Signal;
            int dxs = dx.Signal;
            bool goingDown = dxs == slopeSign;

            FixedSingle dy = (FixedSingle) (((FixedDouble) slope.VCathetus * dx / slope.HCathetus).Abs * dxs * slopeSign);            
            Vector delta = new Vector(dx, dy);
            collider.MoveContactSolid(delta, dx.Abs, (goingDown ? Direction.NONE : Direction.UP) | (dxs > 0 ? Direction.RIGHT : Direction.LEFT) , CollisionFlags.SLOPE);

            if (gravity)
                collider.MoveContactFloor((TILE_SIZE / 2) * QUERY_MAX_DISTANCE);

            if (collider.Landed)
                collider.AdjustOnTheFloor();
        }

        private void MoveX(BoxCollider collider, FixedSingle deltaX, bool gravity = true, bool followSlopes = true)
        {
            Vector dx = new Vector(deltaX, 0);

            Box lastBox = collider.Box;
            bool wasLanded = collider.Landed;
            bool wasLandedOnSlope = collider.LandedOnSlope;
            RightTriangle lastSlope = collider.LandedSlope;
            Box lastLeftCollider = collider.LeftCollider;
            Box lastRightCollider = collider.RightCollider;

            collider.Translate(dx);

            if (wasLanded)
            {
                if (collider.Landed)
                    collider.AdjustOnTheFloor((TILE_SIZE / 2) * QUERY_MAX_DISTANCE);
                else if (gravity)
                    collider.TryMoveContactSlope((TILE_SIZE / 2) * QUERY_MAX_DISTANCE);
            }

            Box union = deltaX > 0 ? lastRightCollider | collider.RightCollider : lastLeftCollider | collider.LeftCollider;
            CollisionFlags collisionFlags = engine.GetCollisionFlags(union, CollisionFlags.NONE, true, deltaX > 0 ? CollisionSide.RIGHT_WALL : deltaX < 0 ? CollisionSide.LEFT_WALL : CollisionSide.NONE);

            if (collisionFlags == CollisionFlags.NONE)
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
                    else if (engine.GetCollisionFlags(collider.DownCollider, CollisionFlags.NONE, false, CollisionSide.FLOOR).HasFlag(CollisionFlags.SLOPE))
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

        private bool IsTouchingASlope(Box box, out RightTriangle slope, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE)
        {
            return engine.ComputedLandedState(box, out slope, maskSize, ignore) == CollisionFlags.SLOPE;
        }

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
                                  //engine.Repaint(this); // Notifica o engine que deverá ser feita este sprite deverá ser redesenhado
                }

                Vector delta = !isStatic && !vel.IsNull ? vel : Vector.NULL_VECTOR;

                if (!delta.IsNull)
                {
                    if (!noClip && checkCollisionWithWorld)
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
                            Vector dy = new Vector(0, delta.Y);
                            Box lastBox = collider.Box;
                            Box lastUpCollider = collider.UpCollider;
                            Box lastDownCollider = collider.DownCollider;
                            collider.Translate(dy);

                            if (dy.Y > 0)
                            {
                                Box union = lastDownCollider | collider.DownCollider;
                                if (engine.GetCollisionFlags(union, CollisionFlags.NONE, true, CollisionSide.FLOOR) != CollisionFlags.NONE)
                                {
                                    collider.Box = lastBox;
                                    collider.MoveContactFloor(dy.Y.Ceil());
                                }
                            }
                            else
                            {
                                Box union = lastUpCollider | collider.UpCollider;
                                if (engine.GetCollisionFlags(union, CollisionFlags.NONE, true, CollisionSide.CEIL) != CollisionFlags.NONE)
                                {
                                    collider.Box = lastBox;
                                    collider.MoveContactSolid(dy, (-dy.Y).Ceil(), Direction.UP);
                                }
                            }
                        }

                        delta = collider.Box.Origin - CollisionBox.Origin;
                    }

                    if (!noClip && checkCollisionWithSprites)
                        delta = DoCheckCollisionWithSprites(delta); // Verifica a colisão deste sprite com os sprites do jogo, verificando também quais sprites estão tocando este sprite e retornando o vetor de deslocamento
                }

                if (delta != Vector.NULL_VECTOR) // Se o deslocamento não for nulo
                {
                    Vector newOrigin = Origin + delta;

                    FixedSingle x = newOrigin.X;
                    FixedSingle y = newOrigin.Y;

                    Box collisionBox = newOrigin + GetCollisionBox();

                    FixedSingle minX = collisionBox.Left;
                    if (minX < 0)
                        x -= minX;

                    FixedSingle minY = collisionBox.Top;
                    if (minY < 0)
                        y -= minY;

                    int worldWidth = engine.World.Width;
                    FixedSingle maxX = collisionBox.Right;
                    if (maxX > worldWidth)
                        x += worldWidth - maxX;

                    FixedSingle maxY = collisionBox.Bottom;
                    int worldHeight = engine.World.Height;
                    if (maxY > worldHeight)
                        y += worldHeight - maxY;

                    Origin = new Vector(x, y);

                    StartMoving(); // Notifica que o sprite começou a se mover, caso ele estivesse parado antes
                }
                else if (moving) // Senão, se ele estava se movendo
                {
                    StopMoving(); // Notifica que ele parou de se mover
                }

                if (!noClip && !isStatic)
                {
                    if (gravity != 0)
                    {
                        vel += gravity * Vector.DOWN_VECTOR;

                        FixedSingle terminalDownwardSpeed = TerminalDownwardSpeed;
                        if (vel.Y > terminalDownwardSpeed)
                            vel = new Vector(vel.X, terminalDownwardSpeed);
                    }

                    if (collider.Landed && vel.Y > 0)
                        vel = vel.XVector;
                }
            }

            if (!Landed && vel.Y > gravity && vel.Y < 2 * gravity)
                vel = new Vector(vel.X, gravity);

            if (BlockedUp && !lastBlockedUp)
                OnBlockedUp();

            if (BlockedLeft && !lastBlockedLeft)
                OnBlockedLeft();

            if (BlockedRight && !lastBlockedRight)
                OnBlockedRight();

            if (Landed && !lastLanded)
                OnLanded();
        }

        public virtual FixedSingle GetGravity()
        {
            return GRAVITY;
        }

        protected virtual FixedSingle GetTerminalDownwardSpeed()
        {
            return TERMINAL_DOWNWARD_SPEED;
        }

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
        protected bool CollisionCheck(Sprite sprite)
        {
            return (ShouldCollide(sprite) || sprite.ShouldCollide(this));
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

        public Box DrawBox
        {
            get
            {
                return Origin + drawBox;
            }
        }

        public Box LastDrawBox
        {
            get
            {
                return Origin + lastDrawBox;
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
                //engine.Repaint(this);
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
                //engine.Repaint(this);
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
            if (!isStatic)
                DoPhysics();
        }

        protected override void PostThink()
        {
            base.PostThink();

            // Se ele estiver invencível, continue gerando o efeito de pisca pisca
            if (invincible && engine.GetEngineTime() >= invincibleExpires)
            {
                invincible = false;
            }

            lastDrawBox = drawBox;
            drawBox = Box.EMPTY_BOX;

            // Processa cada animação
            foreach (Animation animation in animations)
            {
                if (animation.Visible)
                    drawBox |= animation.CurrentFrameBoundingBox;

                animation.OnFrame();

                if (animation.Visible)
                    drawBox |= animation.CurrentFrameBoundingBox;
            }
        }

        /// <summary>
        /// Faz a repintura da entidade
        /// </summary>
        /// <param name="g">Objeto do tipo Graphics que provém as operações de desenho</param>
        public virtual void Paint()
        {
            // Se este objeto já foi disposto (todos seus recursos foram liberados) enão não há nada o que fazer por aqui
            if (disposed)
                return;

            // Realiza a repintura de cada animação
            foreach (Animation animation in animations)
                animation.Render();

            drawCount++; // Incrementa o número de desenhos feitos nesta entidade

            /*if (DEBUG_SHOW_ENTITY_DRAW_COUNT)
            {
                System.Drawing.Font font = new System.Drawing.Font("Arial", 16 * engine.drawScale);
                MMXBox drawBox = BoudingBox;
                using (System.Drawing.Brush brush = new SolidBrush(Color.Yellow))
                {
                    MMXVector mins = drawBox.Origin + drawBox.Mins;
                    string text = drawCount.ToString();
                    System.Drawing.SizeF size = g.MeasureString(text, font);

                    using (Pen pen = new Pen(Color.Blue, 2))
                    {
                        g.DrawRectangle(pen, (engine.drawOrigin + drawBox * engine.drawScale).ToRectangle());
                    }

                    g.DrawString(text, font, brush, engine.drawOrigin.X + (mins.X + (drawBox.Width - size.Width / engine.drawScale) / 2) * engine.drawScale, engine.drawOrigin.Y + (mins.Y + (drawBox.Height - size.Height / engine.drawScale) / 2) * engine.drawScale);
                }
            }*/
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
}
