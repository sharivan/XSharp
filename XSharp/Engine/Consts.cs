using MMX.Geometry;
using MMX.Math;

using SharpDX;

namespace MMX.Engine
{
    public class Consts
    {
        public const int SAVE_SLOT_COUNT = 10;
        public const int KEY_BUFFER_COUNT = 60;

        // Tick rate
        public static readonly FixedSingle TICKRATE = 60; // Representa o número de frames por segundo processados pelo engine. Definida por padrão como 32 ciclos por segundo (32 Hz)
        public static readonly FixedSingle TICK = 1D / TICKRATE; // Intervalo de tempo em segundos de cada tick do jogo que é o intervalo de tempo entre cada frame processado pelo engine.

        // Animation
        public const int BRIGHT_TICK = 5; // Número de ticks entre cada piscada da animação quando a propriedade Flashing for definida como true. Usada na animação de um sprite no modo de invencibilidade.

        // Sprite        
        public const int DEFAULT_INVINCIBLE_TIME = 60; // Tempo de invencibilidade do sprite após tomar um dano não fatal
        public const int DEFAULT_HEALTH = 16; // Quantidade inicial padrão de hp que um sprite terá após seu spawn

        // Directions
        public const int STEP_BIT_COUNT = 8;
        public const int STEP_COUNT = 1 << STEP_BIT_COUNT;
        public static readonly FixedSingle STEP_SIZE = FixedSingle.FromRawValue(1 << (FixedSingle.FIXED_BITS_COUNT - STEP_BIT_COUNT));
        public static readonly FixedSingle MASK_SIZE = STEP_SIZE;
        public static readonly FixedSingle QUERY_MAX_DISTANCE = FixedSingle.ONE;
        public static readonly Vector STEP_LEFT_VECTOR = STEP_SIZE * Vector.LEFT_VECTOR;
        public static readonly Vector STEP_UP_VECTOR = STEP_SIZE * Vector.UP_VECTOR;
        public static readonly Vector STEP_RIGHT_VECTOR = STEP_SIZE * Vector.RIGHT_VECTOR;
        public static readonly Vector STEP_DOWN_VECTOR = STEP_SIZE * Vector.DOWN_VECTOR;       

        // Engine
        public const int TILE_SIZE = 8;
        public const int MAP_SIZE = TILE_SIZE * 2; // Em pixels
        public const int BLOCK_SIZE = MAP_SIZE * 2; // Em pixels
        public const int SCENE_SIZE = 8 * BLOCK_SIZE; // Em pixels
        //public static readonly int LAYOUT_SIZE = 4 * SCENE_SIZE; // Em pixels

        public const int SIDE_TILES_PER_MAP = 2;
        public const int SIDE_MAPS_PER_BLOCK = 2;
        public const int SIDE_BLOCKS_PER_SCENE = 8;
        //public static readonly int SIDE_SCENES_PER_LAYOUT = 4;

        public const bool NO_CAMERA_CONSTRAINTS = false;
        public const int SCREEN_WIDTH = 256;
        public const int SCREEN_HEIGHT = 224;
        public static readonly FixedSingle SIZE_RATIO = 8.0 / 7.0; // Proporção entre o número de colunas e o número de linhas do jogo. Usado para o cálculo da escala do jogo quando a tela for redimensionada.

        public static readonly int SIDE_WIDTH_SCENES_PER_SCREEN = ((FixedSingle) SCREEN_WIDTH / SCENE_SIZE).Ceil();
        public static readonly int SIDE_HEIGHT_SCENES_PER_SCREEN = ((FixedSingle) SCREEN_HEIGHT / SCENE_SIZE).Ceil();

        // Tempos são mensurados em frames, velocidades são mensuradas em pixels por frame enquanto acelerações em pixels por frame ao quadrado.
        public static readonly FixedSingle GRAVITY = 0.25;
        public static readonly FixedSingle TERMINAL_DOWNWARD_SPEED = 5.75;
        public static readonly FixedSingle INITIAL_UPWARD_SPEED_FROM_JUMP = new((1363 + 0 * 64) / 256.0);
        public static readonly FixedSingle INITIAL_UPWARD_SPEED_FROM_SLOPE_JUMP_1 = new((1417 + 1 * 64) / 256.0);
        public static readonly FixedSingle INITIAL_UPWARD_SPEED_FROM_SLOPE_JUMP_2 = new((1505 + 1 * 64) / 256.0);
        public static readonly FixedSingle LADDER_CLIMB_SPEED = new(376 / 256.0);
        public static readonly FixedSingle WALL_SLIDE_SPEED = 2;
        public static readonly FixedSingle PRE_WALKING_SPEED = 1;
        public static readonly FixedSingle WALKING_SPEED = new(376 / 256.0);
        public static readonly FixedSingle SLOPE_DOWNWARD_WALKING_SPEED_1 = new(408 / 256.0);
        public static readonly FixedSingle SLOPE_DOWNWARD_WALKING_SPEED_2 = new(456 / 256.0);
        public static readonly FixedSingle DASH_SPEED = new(885 / 256.0);
        public const int DASH_DURATION = 30;
        public const int WALL_JUMP_DURATION = 9;
        public const int SHOT_DURATION = 16;
        public static readonly FixedSingle RIDE_ARMOR_DASH_SPEED = 4;
        public static readonly FixedSingle CHARGED_SPEED_BURNER_SPEED = 4.5;
        public static readonly FixedSingle RIDE_ARMOD_LUNGING_SPEED = 6;
        public static readonly FixedSingle MOTORBIKE_TERMINAL_SPEED = 6;
        public static readonly FixedSingle FALL_ANIMATION_MINIMAL_SPEED = 1.25;
        public static readonly FixedSingle NO_CLIP_SPEED = 6;
        public static readonly FixedSingle NO_CLIP_SPEED_BOOST = 2.5 * NO_CLIP_SPEED;
        public const int MAX_SHOTS = 3;
        public const int LEMON_HITBOX_WIDTH = 8;
        public const int LEMON_HITBOX_HEIGHT = 8;
        public static readonly FixedSingle LEMON_INITIAL_SPEED = new(1024 / 256.0);
        public static readonly FixedSingle LEMON_ACCELERATION = new(64 / 256.0);
        public static readonly FixedSingle LEMON_TERMINAL_SPEED = new(1536 / 256.0);
        public static readonly FixedSingle LEMON_REFLECTION_VSPEED = new(-768 / 256.0);        
        public const int SEMI_CHARGED_HITBOX_WIDTH_1 = 18;
        public const int SEMI_CHARGED_HITBOX_HEIGHT_1 = 18;
        public const int SEMI_CHARGED_HITBOX_WIDTH_2 = 26;
        public const int SEMI_CHARGED_HITBOX_HEIGHT_2 = 26;
        public const int SEMI_CHARGED_HITBOX_WIDTH_3 = 32;
        public const int SEMI_CHARGED_HITBOX_HEIGHT_3 = 18;
        public static readonly FixedSingle SEMI_CHARGED_INITIAL_SPEED = new(1536 / 256.0);
        public static readonly FixedSingle CHARGED_SPEED = new(2048 / 256.0);
        public const int CHARGED_HITBOX_WIDTH_1 = 26;
        public const int CHARGED_HITBOX_HEIGHT_1 = 18;
        public const int CHARGED_HITBOX_WIDTH_2 = 48;
        public const int CHARGED_HITBOX_HEIGHT_2 = 36;
        public const int CHARGING_EFFECT_HITBOX_SIZE = 52;
        public static readonly Box CHARGING_EFFECT_HITBOX = new(Vector.NULL_VECTOR, new Vector(-CHARGING_EFFECT_HITBOX_SIZE / 2, -CHARGING_EFFECT_HITBOX_SIZE / 2), new Vector(CHARGING_EFFECT_HITBOX_SIZE / 2, CHARGING_EFFECT_HITBOX_SIZE / 2));

        // X
        public const int HITBOX_WIDTH = 12;
        public const int HITBOX_HEIGHT = 28;
        public static readonly Vector HITBOX_SIZE = new(HITBOX_WIDTH, HITBOX_HEIGHT);
        public const int DASHING_HITBOX_WIDTH = 12;
        public const int DASHING_HITBOX_HEIGHT = 16;
        public static readonly Vector DASHING_HITBOX_SIZE = new(DASHING_HITBOX_WIDTH, DASHING_HITBOX_HEIGHT);
        public const int INPUT_MOVEMENT_LATENCY = 1;
        //public const int INPUT_SHOT_LATENCY = 2;
        public static readonly FixedSingle LADDER_BOX_VCLIP = 18;
        public static readonly FixedSingle WALL_MAX_DISTANCE_TO_WALL_JUMP = 8;
        public const int ANIMATION_COUNT = 18;

        public const bool TIME_CONTROL = true; // Define se haverá ou não contagem de tempo para que o bomberman possa concluir o level. Se haver, caso o tempo se acabe o bomberman morrerá e a contagem será reiniciada após seu spawn (caso ele ainda tenha vidas)
        // Os elementos estáticos do jogo formam uma matriz onde todos seus elementos são constituídos por um mesmo quadrado.
        public static readonly FixedSingle RESPAWN_TIME = 1; // Tempo de respawn do bomberman em segundos
        public static readonly int STAGE_COUNT = 7; // Número de estágios que o jogo deverá ter, também representa o número de texturas diferentes usadas pelos blocos (hard e softs) e pelo fundo do jogo
        public static readonly Vector DEFAULT_DRAW_ORIGIN = Vector.NULL_VECTOR; // Origem (coordenadas) padrão da renderização do jogo
        public static readonly FixedSingle DEFAULT_DRAW_SCALE = 1; // Escala padrão de renderização do jogo
        public static readonly FixedSingle DEFAULT_CLIENT_WIDTH = DEFAULT_DRAW_SCALE * SCREEN_WIDTH; // Tamanho padrão da largura do retângulo do jogo
        public static readonly FixedSingle DEFAULT_CLIENT_HEIGHT = DEFAULT_DRAW_SCALE * SCREEN_HEIGHT; // Tamanho padrão da altura do retângulo do jogo
        public static readonly Vector DEFAULT_CLIENT_SIZE = new(DEFAULT_CLIENT_WIDTH, DEFAULT_CLIENT_HEIGHT);
        public static readonly Box DEFAULT_CLIENT_BOX = new(DEFAULT_DRAW_ORIGIN.X, DEFAULT_DRAW_ORIGIN.Y, DEFAULT_CLIENT_WIDTH, DEFAULT_CLIENT_HEIGHT);
        public const bool VSYNC = false;

        // Debug
        // Constantes usadas para depuração do jogo
        //public const bool DEBUG_DRAW_BOX = false;
        public const bool DEBUG_DRAW_COLLISION_BOX = false;
        public const bool DEBUG_SHOW_COLLIDERS = false;
        public const bool DEBUG_DRAW_COLLISION_DATA = false;
        public const bool DEBUG_DRAW_MAP_BOUNDS = false;
        public const bool DEBUG_HIGHLIGHT_TOUCHING_MAPS = false;
        public const bool DEBUG_HIGHLIGHT_POINTED_TILES = false;
        public const bool DEBUG_DRAW_PLAYER_ORIGIN_AXIS = false;
        public const bool DEBUG_SHOW_INFO_TEXT = false;
        public const bool DEBUG_DRAW_CHECKPOINT = false;
        public const bool DEBUG_SHOW_TRIGGERS = false;
        public const bool DEBUG_SHOW_CAMERA_TRIGGER_EXTENSIONS = false;
        public const bool DEBUG_DUMP_ROM_MEMORY = false;

        public static readonly Color HITBOX_COLOR = Color.FromRgba(0x8000ff00);
        public static readonly Color HITBOX_BORDER_COLOR = Color.Green;
        public static readonly Color DOWN_COLLIDER_COLOR = Color.Green;
        public static readonly Color UP_COLLIDER_COLOR = Color.Blue;
        public static readonly Color LEFT_COLLIDER_COLOR = Color.Red;
        public static readonly Color RIGHT_COLLIDER_COLOR = Color.Yellow;
        public static readonly Color TRIGGER_BORDER_BOX_COLOR = Color.Green;
        public static readonly Color TRIGGER_BOX_COLOR = Color.FromRgba(0x8000ff00);
        public static readonly Color CAMERA_LOCK_COLOR = Color.Yellow;

        // Palettes
        public static readonly Color[] X1_NORMAL_PALETTE = new Color[]
        {
            Color.Transparent, // 0
            new Color(224, 224, 224, 255), // 1
            new Color(232, 224, 64, 255), // 2
            new Color(240, 64, 16, 255), // 3
            new Color(120, 216, 240, 255), // 4
            new Color(80, 160, 240, 255), // 5
            new Color(24, 88, 176, 255), // 6
            new Color(0, 128, 248, 255), // 7
            new Color(0, 64, 240, 255), // 8
            new Color(32, 48, 128, 255), // 9
            new Color(248, 176, 128, 255), // 10
            new Color(184, 96, 72, 255), // 11
            new Color(128, 64, 32, 255), // 12
            new Color(240, 240, 240, 255), // 13
            new Color(152, 152, 152, 255), // 14
            new Color(24, 24, 24, 255) // 15
        };

        public static readonly Color[] CHARGE_LEVEL_1_PALETTE = new Color[]
        {
            Color.Transparent, // 0
            new Color(248, 248, 248, 255), // 1
            new Color(232, 224, 64, 255), // 2
            new Color(240, 104, 192, 255), // 3
            new Color(160, 240, 240, 255), // 4
            new Color(80, 216, 240, 255), // 5
            new Color(24, 128, 224, 255), // 6
            new Color(0, 184, 248, 255), // 7
            new Color(0, 144, 240, 255), // 8
            new Color(32, 104, 240, 255), // 9
            new Color(248, 176, 128, 255), // 10
            new Color(184, 96, 72, 255), // 11
            new Color(128, 64, 32, 255), // 12
            new Color(248, 248, 248, 255), // 13
            new Color(176, 176, 176, 255), // 14
            new Color(24, 80, 224, 255) // 15
        };

        public static readonly Color[] CHARGE_LEVEL_2_PALETTE = new Color[]
        {
            Color.Transparent, // 0
            new Color(248, 248, 248, 255), // 1
            new Color(232, 224, 64, 255), // 2
            new Color(240, 104, 192, 255), // 3
            new Color(224, 224, 248, 255), // 4
            new Color(200, 176, 248, 255), // 5
            new Color(152, 136, 240, 255), // 6
            new Color(176, 168, 248, 255), // 7
            new Color(176, 136, 248, 255), // 8
            new Color(136, 112, 232, 255), // 9
            new Color(248, 176, 128, 255), // 10
            new Color(184, 96, 72, 255), // 11
            new Color(128, 64, 32, 255), // 12
            new Color(248, 248, 248, 255), // 13
            new Color(176, 176, 176, 255), // 14
            new Color(144, 0, 216, 255) // 15
        };

        public static readonly Color[] CHARGE_EFFECT_PALETTE = new Color[]
        {
            Color.Transparent, // 0
            new Color(136, 248, 248, 255), // 1
            new Color(248, 224, 112, 255), // 2
            new Color(248, 248, 248, 255), // 3
            new Color(240, 176, 56, 255), // 4
            new Color(240, 144, 96, 255) // 5
        };

        // Startup
        public const bool LOAD_ROM = true;
        public const string ROM_NAME = "ShittyDash.mmx";
        public const bool SKIP_MENU = false; // Defina como true se quiser que o jogo seja carregado diretamente sem passar pelo menu inicial
        public const bool SKIP_INTRO = false; // Defina como true se quiser que o menu inicial seja carregado sem exibir a intro
        public const int INITIAL_LEVEL = 8; // Defina aqui o número correspondente ao level que deseja que inicie assim que o jogo for carregado. Lembrando que 0 é o valor correspondente ao level 1.
    }
}
