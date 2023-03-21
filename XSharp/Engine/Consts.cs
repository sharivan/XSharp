using XSharp.Math;
using XSharp.Math.Geometry;

using Color = SharpDX.Color;

namespace XSharp.Engine;

public enum CollisionBoxType
{
    X1,
    X2X3
}

public class Consts
{
    public const int SAVE_SLOT_COUNT = 100;
    public const int KEY_BUFFER_COUNT = 60;
    public static readonly bool ENABLE_BACKGROUND_INPUT = false;

    // Device params

    public const int TICKRATE = 60;
    public const double TICK = 1D / TICKRATE;
    public static readonly bool DOUBLE_BUFFERED = false;
    public static readonly bool VSYNC = true; // TODO : When false the refresh rate doesn't work fine, taking about half of frames. Please fix it!
    public static readonly bool SAMPLER_STATE_LINEAR = true;
    public static readonly bool FULL_SCREEN = false; // TODO : When true the app crash on device creating. Please fix it!

    // Sprite

    public const int DEFAULT_INVINCIBLE_TIME = 60;
    public const int DEFAULT_HEALTH = 16;

    // Directions

    public static readonly FixedSingle STEP_SIZE = 1; // Recommended step size is 1 (one pixel). This is used to do the fine collision checking, original games do it at pixel level, its not needed to do it at sub-pixel level.
    public static readonly FixedSingle QUERY_MAX_DISTANCE = MAP_SIZE;
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

    public static readonly Vector EXTENDED_BORDER_SPAWN_SCREEN_OFFSET = (MAP_SIZE, MAP_SIZE);
    public static readonly Vector EXTENDED_BORDER_LIVE_SCREEN_OFFSET = (2 * BLOCK_SIZE, 2 * BLOCK_SIZE);

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
    public const int MAX_FRAMES_TO_PRESERVE_WALKING_SPEED = 10;
    public const bool PAUSE_AFTER_WALKING_SPEED_ENDS = true; // X2 and X3 use this, but X1 not.
    public static readonly FixedSingle WALKING_SPEED = 376 / 256.0;
    public static readonly FixedSingle SLOPE_DOWNWARD_WALKING_SPEED_1 = 408 / 256.0;
    public static readonly FixedSingle SLOPE_DOWNWARD_WALKING_SPEED_2 = 456 / 256.0;
    public static readonly FixedSingle DASH_SPEED = 885 / 256.0;
    public static readonly FixedSingle CAMERA_BOOS_DOOR_CROSSING_SMOOTH_SPEED = 2.5;
    public static readonly FixedSingle CAMERA_SMOOTH_SPEED = 8;
    public static readonly FixedSingle INITIAL_DAMAGE_RECOIL_SPEED_X = -138 / 256.0;
    public static readonly FixedSingle INITIAL_DAMAGE_RECOIL_SPEED_Y = -512 / 256.0;
    public static readonly Vector INITIAL_DAMAGE_RECOIL_SPEED = (INITIAL_DAMAGE_RECOIL_SPEED_X, INITIAL_DAMAGE_RECOIL_SPEED_Y);
    public const int DASH_DURATION = 33;
    public const int WALL_JUMP_DURATION = 14;
    public const int SHOT_DURATION = 16;
    public const int DAMAGE_RECOIL_DURATION = 32;
    public static readonly FixedSingle NON_LETHAN_SPIKE_DAMAGE = 4;
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

    // X-Buster

    public const int LEMON_DAMAGE = 2;
    public const int SEMI_CHARGED_DAMAGE = 4;
    public const int CHARGED_DAMAGE = 8;

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
    public static readonly Color DOWN_COLLIDER_COLOR = Color.FromRgba(0x8000ff00);
    public static readonly Color UP_COLLIDER_COLOR = Color.FromRgba(0x80ff0000);
    public static readonly Color LEFT_COLLIDER_COLOR = Color.FromRgba(0x800000ff);
    public static readonly Color RIGHT_COLLIDER_COLOR = Color.FromRgba(0x8000ffff);
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

    // Startup

    public static readonly bool ENABLE_ENEMIES = true;
    public static readonly bool ENABLE_SPAWNING_BLACK_SCREEN = true;
    public static readonly bool ENABLE_OST = true;

    public static readonly bool LOAD_ROM = true;
    public const string ROM_NAME = "ShittyDash.mmx";
    public static readonly bool SKIP_MENU = false;
    public static readonly bool SKIP_INTRO = false;
    public const int INITIAL_LEVEL = 8;
    public const int INITIAL_CHECKPOINT = 1;
}