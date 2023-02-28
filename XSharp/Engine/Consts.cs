using SharpDX;
using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine
{
    public enum CollisionBoxType
    {
        X1,
        X2X3,
        XSHARP
    }

    public class Consts
    {
        public const int SAVE_SLOT_COUNT = 10;
        public const int KEY_BUFFER_COUNT = 60;

        // Tick rate

        public static readonly FixedSingle TICKRATE = 60;
        public static readonly FixedSingle TICK = 1D / TICKRATE;

        // Sprite

        public const int DEFAULT_INVINCIBLE_TIME = 60;
        public const int DEFAULT_HEALTH = 16;

        // Directions

        public static readonly FixedSingle STEP_SIZE = 1; // Recommended step size is 1 (one pixel). This is used to do the fine collision checking, original games do it at pixel level, its not needed to do it at sub-pixel level.
        public static readonly FixedSingle QUERY_MAX_DISTANCE = 1;
        public static readonly Vector STEP_LEFT_VECTOR = STEP_SIZE * Vector.LEFT_VECTOR;
        public static readonly Vector STEP_UP_VECTOR = STEP_SIZE * Vector.UP_VECTOR;
        public static readonly Vector STEP_RIGHT_VECTOR = STEP_SIZE * Vector.RIGHT_VECTOR;
        public static readonly Vector STEP_DOWN_VECTOR = STEP_SIZE * Vector.DOWN_VECTOR;

        // Engine

        public const int BOXKIND_COUNT = 3;

        public const int TILE_SIZE = 8; // In pixels
        public const int MAP_SIZE = TILE_SIZE * 2; // In pixels
        public const int BLOCK_SIZE = MAP_SIZE * 2; // In pixels
        public const int SCENE_SIZE = 8 * BLOCK_SIZE; // In pixels

        public const int SIDE_TILES_PER_MAP = 2;
        public const int SIDE_MAPS_PER_BLOCK = 2;
        public const int SIDE_BLOCKS_PER_SCENE = 8;

        public const bool NO_CAMERA_CONSTRAINTS = false;
        public const int SCREEN_WIDTH = 256; // In pixels
        public const int SCREEN_HEIGHT = 224; // In pixels
        public static readonly FixedSingle SIZE_RATIO = (float) SCREEN_WIDTH / SCREEN_HEIGHT;

        public static readonly int SIDE_WIDTH_SCENES_PER_SCREEN = ((FixedSingle) SCREEN_WIDTH / SCENE_SIZE).Ceil();
        public static readonly int SIDE_HEIGHT_SCENES_PER_SCREEN = ((FixedSingle) SCREEN_HEIGHT / SCENE_SIZE).Ceil();

        public static readonly Vector EXTENDED_BORDER_SCREEN_OFFSET = (2 * BLOCK_SIZE, 2 * BLOCK_SIZE);

        public const int MAX_ENTITIES = 2048;

        public const int SPAWNING_BLACK_SCREEN_FRAMES = 120;

        // Times are measured in frames, velocity in pixel per frames and accelerations in pixels per frame squared.

        public static readonly Vector WORLD_OFFSET = (0, -1);

        public static readonly FixedSingle GRAVITY = 0.25;
        public static readonly FixedSingle UNDERWATER_GRAVITY = 33 / 256.0;
        public static readonly FixedSingle TERMINAL_DOWNWARD_SPEED = 5.75;
        public static readonly FixedSingle TELEPORT_SPEED = 8;
        public static readonly FixedSingle UNDERWATER_TERMINAL_DOWNWARD_SPEED = 737 / 256.0;
        public static readonly FixedSingle INITIAL_UPWARD_SPEED_FROM_JUMP = (1363 + 0 * 64) / 256.0;
        public static readonly FixedSingle INITIAL_UPWARD_SPEED_FROM_SLOPE_JUMP_1 = (1417 + 1 * 64) / 256.0;
        public static readonly FixedSingle INITIAL_UPWARD_SPEED_FROM_SLOPE_JUMP_2 = (1505 + 1 * 64) / 256.0;
        public static readonly FixedSingle LADDER_CLIMB_SPEED = 376 / 256.0;
        public static readonly FixedSingle WALL_SLIDE_SPEED = 2;
        public static readonly FixedSingle UNDERWATER_WALL_SLIDE_SPEED = 1;
        public static readonly FixedSingle PRE_WALKING_SPEED = 1;
        public static readonly FixedSingle WALKING_SPEED = 376 / 256.0;
        public static readonly FixedSingle SLOPE_DOWNWARD_WALKING_SPEED_1 = 408 / 256.0;
        public static readonly FixedSingle SLOPE_DOWNWARD_WALKING_SPEED_2 = 456 / 256.0;
        public static readonly FixedSingle DASH_SPEED = 885 / 256.0;
        public static readonly FixedSingle CAMERA_SMOOTH_SPEED = 2;
        public static readonly FixedSingle INITIAL_DAMAGE_RECOIL_SPEED_X = -138 / 256.0;
        public static readonly FixedSingle INITIAL_DAMAGE_RECOIL_SPEED_Y = -512 / 256.0;
        public static readonly Vector INITIAL_DAMAGE_RECOIL_SPEED = (INITIAL_DAMAGE_RECOIL_SPEED_X, INITIAL_DAMAGE_RECOIL_SPEED_Y);
        public const int DASH_DURATION = 30;
        public const int WALL_JUMP_DURATION = 14;
        public const int SHOT_DURATION = 16;
        public const int DAMAGE_RECOIL_DURATION = 32;
        public static readonly FixedSingle RIDE_ARMOR_DASH_SPEED = 4;
        public static readonly FixedSingle CHARGED_SPEED_BURNER_SPEED = 4.5;
        public static readonly FixedSingle RIDE_ARMOD_LUNGING_SPEED = 6;
        public static readonly FixedSingle RIDE_CHASER_TERMINAL_SPEED = 6;
        public static readonly FixedSingle FALL_ANIMATION_MINIMAL_SPEED = 1.25;
        public static readonly FixedSingle NO_CLIP_SPEED = 6;
        public static readonly FixedSingle NO_CLIP_SPEED_BOOST = 2.5 * NO_CLIP_SPEED;
        public static readonly FixedSingle CROSSING_BOOS_DOOR_SPEED = 116 / 256.0;

        public const int MAX_SHOTS = 3;

        public static readonly Box LEMON_HITBOX = (Vector.NULL_VECTOR, (-4, -4), (4, 4));
        public static readonly FixedSingle LEMON_INITIAL_SPEED = 1024 / 256.0;
        public static readonly FixedSingle LEMON_ACCELERATION = 64 / 256.0;
        public static readonly FixedSingle LEMON_TERMINAL_SPEED = 1536 / 256.0;
        public static readonly FixedSingle LEMON_REFLECTION_VSPEED = -768 / 256.0;

        public static readonly Box SEMI_CHARGED_HITBOX1 = (Vector.NULL_VECTOR, (-9, -9), (9, 9));
        public static readonly Box SEMI_CHARGED_HITBOX2 = (Vector.NULL_VECTOR, (-13, -13), (13, 13));
        public static readonly Box SEMI_CHARGED_HITBOX3 = (Vector.NULL_VECTOR, (-16, -9), (16, 9));
        public static readonly FixedSingle SEMI_CHARGED_INITIAL_SPEED = 1536 / 256.0;
        public static readonly FixedSingle CHARGED_SPEED = 2048 / 256.0;

        public const int CHARGING_EFFECT_HITBOX_SIZE = 52;
        public static readonly Box CHARGED_HITBOX1 = (Vector.NULL_VECTOR, (-13, -9), (13, 9));
        public static readonly Box CHARGED_HITBOX2 = (Vector.NULL_VECTOR, (-24, -18), (24, 18));
        public static readonly Box CHARGING_EFFECT_HITBOX = (Vector.NULL_VECTOR, (-CHARGING_EFFECT_HITBOX_SIZE * 0.5, -CHARGING_EFFECT_HITBOX_SIZE * 0.5), (CHARGING_EFFECT_HITBOX_SIZE * 0.5, CHARGING_EFFECT_HITBOX_SIZE * 0.5));

        // X

        public const CollisionBoxType COLLISION_BOX_TYPE = CollisionBoxType.X2X3;

        public static readonly Box HITBOX = ((0, -1), (-6, -14), (6, 14));
        public static readonly Box DASHING_HITBOX = ((0, 5), (-6, -8), (6, 8));
        public static readonly Box COLLISION_BOX = COLLISION_BOX_TYPE switch
        {
            CollisionBoxType.X1 => ((0, -1), (-7, -17), (7, 17)),
            CollisionBoxType.X2X3 => ((0, -1), (-6, -17), (6, 17)),
            _ => Box.EMPTY_BOX
        };

        public const int INPUT_MOVEMENT_LATENCY = 1;
        public static readonly FixedSingle LADDER_MOVE_OFFSET = 22;
        public static readonly FixedSingle WALL_MAX_DISTANCE_TO_WALL_JUMP = 8;
        public const int X_INITIAL_LIVES = 2;
        public static readonly FixedSingle X_INITIAL_HEALT_CAPACITY = 16;
        public static readonly FixedSingle X_TIRED_PERCENTAGE = 0.25;
        public const int MIN_LIVES = 0;
        public const int MAX_LIVES = 9;

        // Items

        public const int ITEM_DURATION_FRAMES = 240;
        public const int ITEM_BLINKING_FRAMES = 60;
        public const int SMALL_HEALTH_RECOVER_AMOUNT = 2;
        public const int BIG_HEALTH_RECOVER_AMOUNT = 8;
        public const int SMALL_AMMO_RECOVER_AMOUNT = 2;
        public const int BIG_AMMO_RECOVER_AMOUNT = 8;

        // Weapons

        // X-Buster

        public const int LEMON_DAMAGE = 3;
        public const int SEMI_CHARGED_DAMAGE = 6;
        public const int CHARGED_DAMAGE = 9;

        // Enemies

        public const int BOSS_HP = 32;
        public const int DEFAULT_BOSS_INVINCIBILITY_TIME = 68;
        public static readonly FixedSingle BOSS_HP_LEFT = 233;

        // Driller

        public static readonly FixedSingle DRILLER_JUMP_VELOCITY_X = 384 / 256.0;
        public static readonly FixedSingle DRILLER_JUMP_VELOCITY_Y = -1280 / 256.0;
        public const int DRILLER_HEALTH = 4;
        public static readonly FixedSingle DRILLER_CONTACT_DAMAGE = 2;
        public static readonly Box DRILLER_HITBOX = ((-2, 0), (-16, -12), (16, 12));
        public static readonly Box DRILLER_DRILLING_HITBOX = ((8, 0), (-24, -12), (24, 12));
        public static readonly Box DRILLER_COLLISION_BOX = ((-2, 0), (-9, -12), (9, 12));
        public static readonly FixedSingle DRILLER_SIDE_COLLIDER_BOTTOM_CLIP = 6;

        // Bat

        public static readonly FixedSingle BAT_ATTACK_SPEED = 256 / 256.0;
        public static readonly FixedSingle BAT_ESCAPE_SPEED = 512 / 256.0;
        public const int BAT_HEALTH = 3;
        public static readonly FixedSingle BAT_CONTACT_DAMAGE = 1;

        // Penguin

        public static readonly Box PENGUIN_COLLISION_BOX = ((0, 2), (-14, -17), (14, 17));
        public static readonly Box PENGUIN_HITBOX = ((0, 2), (-10, -15), (10, 15));
        public static readonly Box PENGUIN_JUMP_HITBOX = ((2, -12), (-9, -12), (9, 12));
        public static readonly Box PENGUIN_SLIDE_HITBOX = ((-8, 8), (-17, -9), (17, 9));
        public static readonly Box PENGUIN_TAKING_DAMAGE_HITBOX = ((10, -7), (-9, -12), (9, 12));

        public const int PENGUIN_JUMP_FRAMES = 68;
        public static readonly FixedSingle PENGUIN_JUMP_SPEED_Y = 2174 / 256.0;

        public const int PENGUIN_FRAMES_BEFORE_HANGING_JUMP = 22;
        public const int PENGUIN_FRAMES_TO_HANG = 32;
        public const int PENGUIN_FRAMES_BEFORE_SNOW_AFTER_HANGING = 27;
        public const int PENGUIN_FRAMES_BEFORE_STOP_HANGING = 56;
        public const int PENGUIN_MIST_FRAMES = 120;
        public static readonly FixedSingle PENGUIN_HANGING_JUMP_SPEED_Y = 2014 / 256.0;
        public static readonly Vector PENGUIN_HANGING_OFFSET = (10, 24);
        public static readonly FixedSingle PENGUIN_HANGING_SNOWING_SPEED_X = 512 / 256.0;

        public static readonly FixedSingle PENGUIN_KNOCKBACK_SPEED_X = 1;
        public static readonly FixedSingle PENGUIN_KNOCKBACK_SPEED_Y = 545 / 256.0;

        public static readonly FixedSingle PENGUIN_SLIDE_INITIAL_SPEED = 1536 / 256.0;
        public static readonly FixedSingle PENGUIN_SLIDE_DECELARATION = 16 / 256.0;

        public const int PENGUIN_SHOT_START_FRAME = 16;
        public static readonly Vector PENGUIN_SHOT_ORIGIN_OFFSET = (26, -2);

        public const int PENGUIN_SNOW_FRAMES = 40;
        public static readonly Box PENGUIN_SNOW_HITBOX = (Vector.NULL_VECTOR, (-5, -5), (5, 5));
        public static readonly FixedSingle PENGUIN_SNOW_SPEED = 512 / 256.0;
        public static readonly Box PENGUIN_BLOW_HITBOX = (Vector.NULL_VECTOR, (-13, -6), (13, 6));
        public static readonly FixedSingle PENGUIN_BLOW_DISTANCE_FROM_HITBOX = 29;
        public const int PENGUIN_BLOW_FRAMES = 116;

        public const int PENGUIN_BLOW_FRAMES_TO_SPAWN_SCULPTURES = 56;
        public static readonly Vector PENGUIN_SCUPTURE_ORIGIN_OFFSET_1 = (48, -16);
        public static readonly Vector PENGUIN_SCUPTURE_ORIGIN_OFFSET_2 = (80, -16);
        public static readonly Box PENGUIN_SCULPTURE_HITBOX = ((0, 2), (-8, -16), (8, 16));
        public static readonly FixedSingle PENGUIN_SCULPTURE_INITIAL_DISTANCE_FROM_SNOW = 23;
        public const int PENGUIN_SCULPTURE_FRAMES_TO_GRAVITY = 60;

        public static readonly Box PENGUIN_ICE_HITBOX = (Vector.NULL_VECTOR, (-5, -5), (5, 5));
        public static readonly FixedSingle PENGUIN_ICE_SPEED = 1024 / 256.0;
        public static readonly FixedSingle PENGUIN_ICE_SPEED2_X = 512 / 256.0;
        public static readonly FixedSingle PENGUIN_ICE_SPEED2_Y = 545 / 256.0;
        public static readonly FixedSingle PENGUIN_ICE_BUMO_SPEED2_Y = 395 / 256.0;

        public static readonly Box PENGUIN_ICE_FRAGMENT_HITBOX = (Vector.NULL_VECTOR, (-4, -4), (4, 4));

        public static readonly Box PENGUIN_LEVER_HITBOX = (Vector.NULL_VECTOR, (-13, -12), (13, 12));
        public static readonly FixedSingle PENGUIN_LEVER_STEP_Y = 1;
        public const int PENGUIN_LEVER_MOVING_FRAMES = 16;

        public const int HITS_TO_BREAK_FROZEN_BLOCK = 24;
        public static readonly Box PENGUIN_FROZEN_BLOCK_HITBOX = ((0, -1), (-6, -14), (6, 14));

        // Render

        // Layers:
        // 0 - Background down layer
        // 1 - Background up layer
        // 2 - Foreground down layer
        // 3 - Foreground up layer
        // 4 - HUD down layer
        // 5 - HUD up layer
        // 6 - Unused
        // 7 - Unused
        public const int NUM_LAYERS = 8;
        public const int NUM_SPRITE_LAYERS = 2;

        public static readonly Vector DEFAULT_DRAW_ORIGIN = Vector.NULL_VECTOR;
        public static readonly FixedSingle DEFAULT_DRAW_SCALE = 1;
        public static readonly FixedSingle DEFAULT_CLIENT_WIDTH = DEFAULT_DRAW_SCALE * SCREEN_WIDTH;
        public static readonly FixedSingle DEFAULT_CLIENT_HEIGHT = DEFAULT_DRAW_SCALE * SCREEN_HEIGHT;
        public static readonly Vector DEFAULT_CLIENT_SIZE = (DEFAULT_CLIENT_WIDTH, DEFAULT_CLIENT_HEIGHT);
        public static readonly Box DEFAULT_CLIENT_BOX = (DEFAULT_DRAW_ORIGIN.X, DEFAULT_DRAW_ORIGIN.Y, DEFAULT_CLIENT_WIDTH, DEFAULT_CLIENT_HEIGHT);
        public const bool VSYNC = false;
        public const bool SPRITE_SAMPLER_STATE_LINEAR = false;

        // Debug

        public const bool DEBUG_DRAW_HITBOX = false;
        public const bool DEBUG_SHOW_BOUNDING_BOX = false;
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

        public static readonly Color HITBOX_COLOR = Color.FromRgba(0x8000ff00);
        public static readonly Color HITBOX_BORDER_COLOR = Color.Green;
        public static readonly Color DEAD_HITBOX_COLOR = Color.FromRgba(0x800000ff);
        public static readonly Color DEAD_HITBOX_BORDER_COLOR = Color.Red;
        public static readonly Color DEAD_RESPAWNABLE_HITBOX_COLOR = Color.FromRgba(0x80ff0000);
        public static readonly Color DEAD_RESPAWNABLE_HITBOX_BORDER_COLOR = Color.Blue;
        public static readonly Color BOUNDING_BOX_COLOR = Color.FromRgba(0x80ff0000);
        public static readonly Color BOUNDING_BOX_BORDER_COLOR = Color.Red;
        public static readonly Color DOWN_COLLIDER_COLOR = Color.Green;
        public static readonly Color UP_COLLIDER_COLOR = Color.Blue;
        public static readonly Color LEFT_COLLIDER_COLOR = Color.Red;
        public static readonly Color RIGHT_COLLIDER_COLOR = Color.Yellow;
        public static readonly Color TRIGGER_BORDER_BOX_COLOR = Color.Green;
        public static readonly Color TRIGGER_BOX_COLOR = Color.FromRgba(0x8000ff00);
        public static readonly Color CHECKPOINT_TRIGGER_BORDER_BOX_COLOR = Color.LightSeaGreen;
        public static readonly Color CHECKPOINT_TRIGGER_BOX_COLOR = Color.FromRgba(0x8000ff00);
        public static readonly Color CAMERA_LOCK_COLOR = Color.Yellow;
        public static readonly Color TOUCHING_MAP_COLOR = Color.Blue;

        // HUD

        public static readonly FixedSingle HP_LEFT = 9;
        public static readonly FixedSingle HP_BOTTOM = 96;
        public static readonly Vector READY_OFFSET = ((SCREEN_WIDTH - 39) * 0.5, (SCREEN_HEIGHT - 13) * 0.5);

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
            new Color(24, 24, 24, 255), // 15
            Color.Transparent, // 16
            new Color(248, 248, 248, 255), // 17
            new Color(240, 248, 248, 255), // 18
            new Color(232, 248, 248, 255), // 19
            new Color(224, 248, 248, 255), // 20
            new Color(216, 248, 248, 255), // 21
            new Color(208, 248, 248, 255), // 22
            new Color(200, 248, 248, 255), // 23
            new Color(192, 248, 248, 255), // 24
            new Color(184, 248, 248, 255), // 25
            new Color(176, 248, 248, 255), // 26
            new Color(168, 248, 248, 255), // 27
            new Color(160, 248, 248, 255), // 28
            new Color(152, 248, 248, 255), // 29
            new Color(144, 248, 248, 255), // 30
            new Color(136, 248, 248, 255) // 31
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

        public static readonly Color[] FLASHING_PALETTE = new Color[]
        {
            Color.Transparent, // 0
            new Color(248, 248, 248, 255), // 1
            new Color(240, 248, 248, 255), // 2
            new Color(232, 248, 248, 255), // 3
            new Color(224, 248, 248, 255), // 4
            new Color(216, 248, 248, 255), // 5
            new Color(208, 248, 248, 255), // 6
            new Color(200, 248, 248, 255), // 7
            new Color(192, 248, 248, 255), // 8
            new Color(184, 248, 248, 255), // 9
            new Color(176, 248, 248, 255), // 10
            new Color(168, 248, 248, 255), // 11
            new Color(160, 248, 248, 255), // 12
            new Color(152, 248, 248, 255), // 13
            new Color(144, 248, 248, 255), // 14
            new Color(136, 248, 248, 255) // 15
        };

        public static readonly Color[] DRILLER_PALETTE = new Color[]
        {
            Color.Transparent, // 0
            new Color(48, 40, 96, 255), // 1
            new Color(72, 64, 144, 255), // 2
            new Color(112, 104, 224, 255), // 3
            new Color(176, 168, 248, 255), // 4
            new Color(112, 64, 40, 255), // 5
            new Color(168, 104, 56, 255), // 6
            new Color(192, 152, 80, 255), // 7
            new Color(224, 216, 128, 255), // 8
            new Color(16, 128, 80, 255), // 9
            new Color(32, 160, 136, 255), // 10
            new Color(40, 240, 192, 255), // 11
            new Color(72, 80, 72, 255), // 12
            new Color(128, 136, 128, 255), // 13
            new Color(200, 208, 200, 255), // 14
            new Color(32, 32, 32, 255) // 15
        };

        public static readonly Color[] BAT_PALETTE = new Color[]
        {
            Color.Transparent, // 0
            new Color(64, 136, 64, 255), // 1
            new Color(248, 192, 240, 255), // 2
            new Color(240, 48, 80, 255), // 3
            new Color(224, 216, 128, 255), // 4
            new Color(200, 160, 80, 255), // 5           
            new Color(152, 112, 48, 255), // 6
            new Color(120, 72, 48, 255), // 7
            new Color(104, 232, 168, 255), // 8
            new Color(96, 72, 128, 255), // 9
            new Color(136, 104, 184, 255), // 10
            new Color(168, 136, 224, 255), // 11
            new Color(240, 240, 240, 255), // 12
            new Color(160, 160, 160, 255), // 13
            new Color(104, 104, 104, 255), // 14
            new Color(40, 40, 40, 255), // 15
        };

        public static readonly Color[] PENGUIN_PALETTE = new Color[]
        {
            Color.Transparent, // 0
            Color.FromBgra(0xFF303040), // 1
            Color.FromBgra(0xFF3870F0), // 2
            Color.FromBgra(0xFFD08050), // 3
            Color.FromBgra(0xFFF8B050), // 4
            Color.FromBgra(0xFFF0F0F0), // 5           
            Color.FromBgra(0xFFB0B0C8), // 6
            Color.FromBgra(0xFF686880), // 7
            Color.FromBgra(0xFF185068), // 8
            Color.FromBgra(0xFF205898), // 9
            Color.FromBgra(0xFFF03808), // A
            Color.FromBgra(0xFFA83008), // B
            Color.FromBgra(0xFF683010), // C
            Color.FromBgra(0xFF9870D8), // D
            Color.FromBgra(0xFF6848B8), // E
            Color.FromBgra(0xFF282828), // F

            Color.Transparent, // 10
            Color.FromBgra(0xFF404800), // 11
            Color.FromBgra(0xFFF8F8F8), // 12
            Color.FromBgra(0xFFC8D8E0), // 13
            Color.FromBgra(0xFF98C0D0), // 14
            Color.FromBgra(0xFF70A8B8), // 15
            Color.FromBgra(0xFF4090A8), // 16
            Color.FromBgra(0xFF187898), // 17
            Color.FromBgra(0xFF185058), // 18
            Color.FromBgra(0xFF803090), // 19
            Color.FromBgra(0xFFB0F8F8), // 1A
            Color.FromBgra(0xFFA8F8F8), // 1B
            Color.FromBgra(0xFFA0F8F8), // 1C
            Color.FromBgra(0xFF98F8F8), // 1D
            Color.FromBgra(0xFF90F8F8), // 1E
            Color.FromBgra(0xFF88F8F8) // 1F
        };

        // Startup

        public const bool ENABLE_ENEMIES = false;
        public const bool ENABLE_SPAWNING_BLACK_SCREEN = false;
        public const bool ENABLE_OST = false;

        public const bool LOAD_ROM = true;
        public const string ROM_NAME = "BestGame.mmx";
        public const bool SKIP_MENU = false;
        public const bool SKIP_INTRO = false;
        public const int INITIAL_LEVEL = 2;
        public const int INITIAL_CHECKPOINT = 0;
    }
}