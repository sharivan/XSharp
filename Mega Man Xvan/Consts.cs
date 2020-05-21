using Geometry2D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class Consts
    {
        // Tick rate
        public static readonly float TICKRATE = 60; // Representa o número de frames por segundo processados pelo engine. Definida por padrão como 32 ciclos por segundo (32 Hz)
        public static readonly float TICK = 1F / TICKRATE; // Intervalo de tempo em segundos de cada tick do jogo que é o intervalo de tempo entre cada frame processado pelo engine.

        // Animation
        public static readonly int BRIGHT_TICK = 5; // Número de ticks entre cada piscada da animação quando a propriedade Flashing for definida como true. Usada na animação de um sprite no modo de invencibilidade.
        public static readonly float DEFAULT_FPS = 60; // FPS padrão da animação do jogo. Representa a quantidade de quadros por segundo que será renderizado na tela por cada sprite composto de animações.

        // Sprite
        public static readonly int CORNER_SIZE = 24; // Medida usada pra definir os limites de uma esquida. Por meio disso o movimento do bomberman de dobrar esquinas fica mais fácil e suave de se fazer.
        public static readonly float DEFAULT_INVINCIBLE_TIME = 2.5F; // Tempo de invencibilidade do sprite após tomar um dano não fatal
        public static readonly float DEFAULT_HEALTH = 100; // Quantidade inicial padrão de hp que um sprite terá após seu spawn
        public static readonly float DEFAULT_MAX_DAMAGE = 100; // Dano máximo padrão que um sprite poderá tomar

        // Directional Sprite
        public static readonly int DEFAULT_SPEED = 128;
        public static readonly float DEATH_TIME = 0.75F;

        // Bomberman
        public static readonly int MAX_BOMBS = 10; // Quantidade máxima de bombas que o bomberman poderá ter
        public static readonly int MAX_RANGE = 12; // Alcance máximo da explosão que o bomberman poderá conseguir
        public static readonly float BOMBERMAN_INVINCIBILITY_TIME = 5; // Tempo de invencibilidade do bomberman após o spawn
        public static readonly float VEST_TIME = 10; // Tempo de invencibilidade do bomberman ao consguir o powerup colete (vest)
        public static readonly int INITIAL_LIVES = 3; // Quantidade inicial de vidas que o bomberman terá ao iniciar o jogo
        public static readonly float INITIAL_TIME = 3 * 60; // 3 minutos. Tempo inicial que o bomberman terá para completar o level após o spawn.
        public static readonly float MAX_TIME = 6 * 60; // 6 minutos. Tempo máximo que o bomberman poderá ter para completar o level.
        public static readonly int MAX_LIVES = 9; // Quantidade máxima de vidas que o bomberman poderá ter
        public static readonly float BOMB_SPEED = 256; // Velocidade da boma após ser chutada

        // Bomb
        public static readonly float BOMB_TIME = 2.5F; // Tempo de detonação da bomba após ser plantada pelo bomberman em segundos

        // Flame
        public static readonly float FLAME_TIMELIFE = 1.5F; // Tempo de duração da explosão da bomba em segundos
        public static readonly float MININAL_OPACITY_TO_HURT = 0.75F; // Opacidade mínima que as chamas da explosão poderá ter para que cause dano em outros sprites do jogo

        // Powerup
        public static readonly float POWERUP_DRAWBOX_BORDER_SIZE = 16;

        // Soft Block
        public static readonly float SOFT_BLOCK_BREAK_TIME = 1.5F; // Tempo em segundos que os blocos quebráveis (soft blocks) levarão para se quebrarem após serem atingidos por uma explosão

        // Engine
        public static readonly int TILESET_SIZE = 8;
        public static readonly int MAP_SIZE = TILESET_SIZE * 2;
        public static readonly int BLOCK_SIZE = MAP_SIZE * 2;
        public static readonly int SCENE_SIZE = 8 * BLOCK_SIZE;
        public static readonly int LAYOUT_SIZE = 4 * SCENE_SIZE;

        public static readonly int SCREEN_WIDTH = 512;
        public static readonly int SCREEN_HEIGHT = 448;

        // Velocidades são mensuradas em pixels por frame enquanto acelerações em pixels por frame ao quadrado.
        public static readonly float GRAVITY = 0.25f;
        public static readonly float TERMINAL_DOWNWARD_SPEED = 5.75f;
        public static readonly float INITIAL_UPWARD_SPEED_FROM_JUMP = 5f;
        public static readonly float INITIAL_UPWARD_SPEED_FROM_SLOPE_JUMP_1 = 5.25f;
        public static readonly float INITIAL_UPWARD_SPEED_FROM_SLOPE_JUMP_2 = 5.25f;
        public static readonly float LADDER_CLIMB_SPEED = 1.5f;
        public static readonly float WALKING_SPEED = 1.5f;
        public static readonly float DASH_SPEED = 3.5f;
        public static readonly float RIDE_ARMOR_DASH_SPEED = 4f;
        public static readonly float CHARGED_SPEED_BURNER_SPEED = 4.5f;
        public static readonly float RIDE_ARMOD_LUNGING_SPEED = 6f;
        public static readonly float MOTORBIKE_TERMINAL_SPEED = 6f;

        // X
        public static readonly int HITBOX_WIDTH = 13;
        public static readonly int HITBOX_HEIGHT = 29;
        public static readonly Vector2D HITBOX_SIZE = new Vector2D(HITBOX_WIDTH, HITBOX_HEIGHT);

        public static readonly bool TIME_CONTROL = true; // Define se haverá ou não contagem de tempo para que o bomberman possa concluir o level. Se haver, caso o tempo se acabe o bomberman morrerá e a contagem será reiniciada após seu spawn (caso ele ainda tenha vidas)
        // Os elementos estáticos do jogo formam uma matriz onde todos seus elementos são constituídos por um mesmo quadrado.
        public static readonly int ROW_COUNT = 14; // Número de linhas contidas no jogo
        public static readonly int COL_COUNT = 15; // Número de colunas contida no jogo
        public static readonly int INTERNAL_ORIGIN_ROW = 2; // Linha correspondente a origem da região interna do jogo
        public static readonly int INTERNAL_ORIGIN_COL = 1; // Coluna correspondete a origem da região interna do jogo
        public static readonly int INTERNAL_ROW_COUNT = ROW_COUNT - 3; // Número de linhas da região interna do jogo
        public static readonly int INTERNAL_COL_COUNT = COL_COUNT - 2; // Número de colunas da região interna do jogo
        public static readonly Box2D CELL_BOX = new Box2D(Vector2D.NULL_VECTOR, Vector2D.NULL_VECTOR, new Vector2D(BLOCK_SIZE, BLOCK_SIZE));
        public static readonly float RESPAWN_TIME = 1; // Tempo de respawn do bomberman em segundos
        public static readonly int STAGE_COUNT = 7; // Número de estágios que o jogo deverá ter, também representa o número de texturas diferentes usadas pelos blocos (hard e softs) e pelo fundo do jogo
        public static readonly int SOFT_BLOCK_COUNT = 50; // Número de blocos quebráveis (soft blocks) do jogo
        public static readonly int CREEP_IMAGE_COUNT = 3; // Número de skins diferentes usadas pelos creeps
        public static readonly int CACTUS_IMAGE_COUNT = 3; // Número de skins diferentes usadas pelos cactus
        public static readonly int MAX_CREEPS = 8; // Número máximo de creeps
        public static readonly int MAX_CACTUS = 4; // Número máximo de cactus
        public static readonly float GENERATE_GRAPH_NODE_VALUES_THINK_TIME = 2; // Intervalo de tempo (em segundos) usado para a geração dos valores dos nós do grafo do jogo para a computação do menor caminho a ser seguido pelos cactus até o bomberman
        public static readonly int MAX_SOFT_BLOCK_POWERUPS_PER_LEVEL = 4; // Número máximo de powerups obtidos atráves da quebra de soft blocks
        public static readonly int POWER_UP_COUNT = 12; // Quanridade de powerups disponíveis no jogo
        public static readonly float[] POWERUP_ODD_DISTRIBUTION = // Distribuição de probabilidade dos powerups do jogo
            {
                15, // Bomb pass
                10, // Bomb up
                15, // Clock
                10, // Fire up
                2.5F, // Heart
                15, // Kick
                2.5F, // Life
                2.5F, // Red bomb
                2.5F, // Remote control
                10, // Roller
                5, // Soft block pass
                10, // Vest
            };
        public static readonly int[,] MAX_POWERUP_PER_LEVEL = // Número máximo de powerups de cada tipo por level
            {
                { // Level 0
                    1, // Bomb pass
                    3, // Bomb up
                    1, // Clock
                    2, // Fire up
                    0, // Heart
                    1, // Kick
                    1, // Life
                    0, // Red bomb
                    0, // Remote control
                    0, // Roller
                    0, // Soft block pass
                    1, // Vest
                },
                { // Level 1
                    1, // Bomb pass
                    3, // Bomb up
                    1, // Clock
                    2, // Fire up
                    0, // Heart
                    1, // Kick
                    1, // Life
                    0, // Red bomb
                    0, // Remote control
                    0, // Roller
                    0, // Soft block pass
                    0, // Vest
                },
                { // Level 2
                    1, // Bomb pass
                    2, // Bomb up
                    1, // Clock
                    1, // Fire up
                    1, // Heart
                    1, // Kick
                    1, // Life
                    0, // Red bomb
                    0, // Remote control
                    1, // Roller
                    1, // Soft block pass
                    1, // Vest
                },
                { // Level 3
                    1, // Bomb pass
                    2, // Bomb up
                    2, // Clock
                    1, // Fire up
                    1, // Heart
                    1, // Kick
                    1, // Life
                    0, // Red bomb
                    1, // Remote control
                    0, // Roller
                    0, // Soft block pass
                    0, // Vest
                },
                { // Level 4
                    1, // Bomb pass
                    3, // Bomb up
                    1, // Clock
                    2, // Fire up
                    0, // Heart
                    1, // Kick
                    1, // Life
                    0, // Red bomb
                    1, // Remote control
                    0, // Roller
                    0, // Soft block pass
                    1, // Vest
                },
                { // Level 5
                    1, // Bomb pass
                    2, // Bomb up
                    1, // Clock
                    1, // Fire up
                    0, // Heart
                    1, // Kick
                    1, // Life
                    1, // Red bomb
                    0, // Remote control
                    0, // Roller
                    0, // Soft block pass
                    2, // Vest
                },
                { // Level 6
                    1, // Bomb pass
                    1, // Bomb up
                    2, // Clock
                    1, // Fire up
                    1, // Heart
                    0, // Kick
                    0, // Life
                    1, // Red bomb
                    0, // Remote control
                    0, // Roller
                    1, // Soft block pass
                    2, // Vest
                }
            };
        public static readonly Vector2D DEFAULT_DRAW_ORIGIN = Vector2D.NULL_VECTOR; // Origem (coordenadas) padrão da renderização do jogo
        public static readonly float DEFAULT_DRAW_SCALE = 1; // Escala padrão de renderização do jogo
        public static readonly float DEFAULT_CLIENT_WIDTH = SCREEN_WIDTH; // Tamanho padrão da largura do retângulo do jogo
        public static readonly float DEFAULT_CLIENT_HEIGHT = SCREEN_HEIGHT; // Tamanho padrão da altura do retângulo do jogo
        public static readonly SizeF DEFAULT_CLIENT_SIZE = new SizeF(DEFAULT_CLIENT_WIDTH, DEFAULT_CLIENT_HEIGHT);
        public static readonly RectangleF DEFAULT_CLIENT_RECT = new RectangleF(DEFAULT_DRAW_ORIGIN.X, DEFAULT_DRAW_ORIGIN.Y, DEFAULT_CLIENT_WIDTH, DEFAULT_CLIENT_HEIGHT);
        public static readonly Box2D DEFAULT_CLIENT_BOX = new Box2D(DEFAULT_CLIENT_RECT);
        public static readonly RectangleF DEFAULT_TOP_PANEL_RECT = new RectangleF(DEFAULT_DRAW_ORIGIN.X, DEFAULT_DRAW_ORIGIN.Y, DEFAULT_CLIENT_WIDTH, BLOCK_SIZE);
        public static readonly Box2D DEFAULT_TOP_PANEL_BOX = new Box2D(DEFAULT_TOP_PANEL_RECT);
        public static readonly Box2D DEFAULT_LEVEL_BOX = CELL_BOX.ScaleRight(2) - BLOCK_SIZE / 8;
        public static readonly Box2D DEFAULT_LIVES_IMG_BOX = CELL_BOX + Vector2D.RIGHT_VECTOR * BLOCK_SIZE * 2 - BLOCK_SIZE / 8;
        public static readonly Box2D DEFAULT_LIVES_BOX = CELL_BOX + Vector2D.RIGHT_VECTOR * BLOCK_SIZE * 3 - BLOCK_SIZE / 8;
        public static readonly Box2D DEFAULT_SCORE_BOX = (CELL_BOX + Vector2D.RIGHT_VECTOR * BLOCK_SIZE * 4).ScaleRight(3) - BLOCK_SIZE / 8;
        public static readonly Box2D DEFAULT_TIME_BOX = (CELL_BOX + Vector2D.RIGHT_VECTOR * BLOCK_SIZE * 7).ScaleRight(2) - BLOCK_SIZE / 8;
        public static readonly Box2D DEFAULT_BOMBS_IMG_BOX = CELL_BOX + Vector2D.RIGHT_VECTOR * BLOCK_SIZE * 9 - BLOCK_SIZE / 8;
        public static readonly Box2D DEFAULT_BOMBS_BOX = CELL_BOX + Vector2D.RIGHT_VECTOR * BLOCK_SIZE * 10 - BLOCK_SIZE / 8;
        public static readonly Box2D DEFAULT_FIRE_IMG_BOX = CELL_BOX + Vector2D.RIGHT_VECTOR * BLOCK_SIZE * 11 - BLOCK_SIZE / 8;
        public static readonly Box2D DEFAULT_FIRE_BOX = CELL_BOX + Vector2D.RIGHT_VECTOR * BLOCK_SIZE * 12 - BLOCK_SIZE / 8;
        public static readonly Box2D DEFAULT_HEART_IMG_BOX = CELL_BOX + Vector2D.RIGHT_VECTOR * BLOCK_SIZE * 13 - BLOCK_SIZE / 8;
        public static readonly Box2D DEFAULT_HEART_BOX = CELL_BOX + Vector2D.RIGHT_VECTOR * BLOCK_SIZE * 14 - BLOCK_SIZE / 8;
        public static readonly RectangleF DEFAULT_GAME_AREA_RECT = new RectangleF(DEFAULT_DRAW_ORIGIN.X, DEFAULT_DRAW_ORIGIN.Y + BLOCK_SIZE, DEFAULT_CLIENT_WIDTH, DEFAULT_CLIENT_HEIGHT - BLOCK_SIZE);
        public static readonly Box2D DEFAULT_GAME_AREA_BOX = new Box2D(DEFAULT_GAME_AREA_RECT);
        public static readonly Vector2D PRESS_ENTER_COORDS = new Vector2D(124, 323);
        public static readonly float PRESS_ENTER_FLASH_TIME = 0.5F;
        public static readonly float SIZE_RATIO = (float)COL_COUNT / (float)ROW_COUNT; // Proporção entre o número de colunas e o número de linhas do jogo. Usado para o cálculo da escala do jogo quando a tela for redimensionada.
        public static readonly float GAME_OVER_PANEL_SHOW_DELAY = 5;

        // Scoring
        public static readonly int MAX_PLAYER_NAME = 12; // Quantidade máxima de caracteres que o nome do jogador poderá ter
        public static readonly int POWERUP_POINTS = 10; // Pontos adquiridos por obter um powerup
        public static readonly int CREEP_POINTS = 20; // Pontos adquiridos por matar um creep
        public static readonly int CACTUS_POINTS = 50; // Pontos adquiridos por matar um cactus
        public static readonly int SOFT_BLOCK_POINTS = 10; // Pontos adquiridos por quebrar um block

        // Debug
        // Constantes usadas para depuração do jogo
        public static readonly bool DEBUG_ROUTE = false;
        public static readonly bool DEBUG_GRAPH = false;
        public static readonly bool DEBUG_SHOW_ENTITY_DRAW_COUNT = false;
        public static readonly bool DEBUG_DRAW_CLIPRECT = false;
        public static readonly bool DEBUG_DRAW_COLLISION_BOX = true;

        // Startup
        public static readonly bool SKIP_MENU = false; // Defina como true se quiser que o jogo seja carregado diretamente sem passar pelo menu inicial
        public static readonly bool SKIP_INTRO = false; // Defina como true se quiser que o menu inicial seja carregado sem exibir a intro
        public static readonly int INITIAL_LEVEL = 0; // Defina aqui o número correspondente ao level que deseja que inicie assim que o jogo for carregado. Lembrando que 0 é o valor correspondente ao level 1.
    }
}
