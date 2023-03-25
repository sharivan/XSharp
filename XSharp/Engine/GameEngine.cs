using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

using NLua;

using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.DirectInput;
using SharpDX.Mathematics.Interop;
using SharpDX.Windows;

using XSharp.Engine.Collision;
using XSharp.Engine.Entities;
using XSharp.Engine.Entities.Effects;
using XSharp.Engine.Entities.Enemies;
using XSharp.Engine.Entities.Enemies.AxeMax;
using XSharp.Engine.Entities.Enemies.BombBeen;
using XSharp.Engine.Entities.Enemies.Bosses;
using XSharp.Engine.Entities.Enemies.Bosses.Penguin;
using XSharp.Engine.Entities.Enemies.Flammingle;
using XSharp.Engine.Entities.Enemies.RayBit;
using XSharp.Engine.Entities.Enemies.Snowball;
using XSharp.Engine.Entities.Enemies.SnowShooter;
using XSharp.Engine.Entities.Enemies.Tombot;
using XSharp.Engine.Entities.Enemies.TurnCannon;
using XSharp.Engine.Entities.HUD;
using XSharp.Engine.Entities.Items;
using XSharp.Engine.Entities.Objects;
using XSharp.Engine.Entities.Triggers;
using XSharp.Engine.Entities.Weapons;
using XSharp.Engine.Graphics;
using XSharp.Engine.Sound;
using XSharp.Engine.World;
using XSharp.Math;
using XSharp.Math.Geometry;
using XSharp.MegaEDX;

using static XSharp.Engine.Consts;
using static XSharp.Engine.World.World;

using Box = XSharp.Math.Geometry.Box;
using Color = SharpDX.Color;
using Configuration = System.Configuration.Configuration;
using D3D9LockFlags = SharpDX.Direct3D9.LockFlags;
using Device9 = SharpDX.Direct3D9.Device;
using DeviceType = SharpDX.Direct3D9.DeviceType;
using DXSprite = SharpDX.Direct3D9.Sprite;
using Font = SharpDX.Direct3D9.Font;
using MMXWorld = XSharp.Engine.World.World;
using Point = SharpDX.Point;
using Rectangle = SharpDX.Rectangle;
using RectangleF = SharpDX.RectangleF;
using ResultCode = SharpDX.Direct3D9.ResultCode;
using Sprite = XSharp.Engine.Entities.Sprite;

namespace XSharp.Engine;

public sealed class ProgramConfiguratinSection : ConfigurationSection
{
    public ProgramConfiguratinSection()
    {
    }

    [ConfigurationProperty("left",
        DefaultValue = -1,
        IsRequired = false
        )]
    public int Left
    {
        get => (int) this["left"];
        set => this["left"] = value;
    }

    [ConfigurationProperty("top",
        DefaultValue = -1,
        IsRequired = false
        )]
    public int Top
    {
        get => (int) this["top"];

        set => this["top"] = value;
    }

    [ConfigurationProperty("width",
        DefaultValue = -1,
        IsRequired = false
        )]
    public int Width
    {
        get => (int) this["width"];
        set => this["width"] = value;
    }

    [ConfigurationProperty("height",
        DefaultValue = -1,
        IsRequired = false
        )]
    public int Height
    {
        get => (int) this["height"];
        set => this["height"] = value;
    }

    [ConfigurationProperty("drawCollisionBox",
        DefaultValue = DEBUG_DRAW_HITBOX,
        IsRequired = false
        )]
    public bool DrawCollisionBox
    {
        get => (bool) this["drawCollisionBox"];
        set => this["drawCollisionBox"] = value;
    }

    [ConfigurationProperty("showColliders",
        DefaultValue = DEBUG_SHOW_COLLIDERS,
        IsRequired = false
        )]
    public bool ShowColliders
    {
        get => (bool) this["showColliders"];
        set => this["showColliders"] = value;
    }

    [ConfigurationProperty("drawMapBounds",
        DefaultValue = DEBUG_DRAW_MAP_BOUNDS,
        IsRequired = false
        )]
    public bool DrawMapBounds
    {
        get => (bool) this["drawMapBounds"];
        set => this["drawMapBounds"] = value;
    }

    [ConfigurationProperty("drawTouchingMapBounds",
        DefaultValue = DEBUG_HIGHLIGHT_TOUCHING_MAPS,
        IsRequired = false
        )]
    public bool DrawTouchingMapBounds
    {
        get => (bool) this["drawTouchingMapBounds"];
        set => this["drawTouchingMapBounds"] = value;
    }

    [ConfigurationProperty("drawHighlightedPointingTiles",
        DefaultValue = DEBUG_HIGHLIGHT_POINTED_TILES,
        IsRequired = false
        )]
    public bool DrawHighlightedPointingTiles
    {
        get => (bool) this["drawHighlightedPointingTiles"];
        set => this["drawHighlightedPointingTiles"] = value;
    }

    [ConfigurationProperty("drawPlayerOriginAxis",
        DefaultValue = DEBUG_DRAW_PLAYER_ORIGIN_AXIS,
        IsRequired = false
        )]
    public bool DrawPlayerOriginAxis
    {
        get => (bool) this["drawPlayerOriginAxis"];
        set => this["drawPlayerOriginAxis"] = value;
    }

    [ConfigurationProperty("showInfoText",
        DefaultValue = DEBUG_SHOW_INFO_TEXT,
        IsRequired = false
        )]
    public bool ShowInfoText
    {
        get => (bool) this["showInfoText"];
        set => this["showInfoText"] = value;
    }

    [ConfigurationProperty("showCheckpointBounds",
        DefaultValue = DEBUG_DRAW_CHECKPOINT,
        IsRequired = false
        )]
    public bool ShowCheckpointBounds
    {
        get => (bool) this["showCheckpointBounds"];
        set => this["showCheckpointBounds"] = value;
    }

    [ConfigurationProperty("showTriggerBounds",
        DefaultValue = DEBUG_SHOW_TRIGGERS,
        IsRequired = false
        )]
    public bool ShowTriggerBounds
    {
        get => (bool) this["showTriggerBounds"];
        set => this["showTriggerBounds"] = value;
    }

    [ConfigurationProperty("showTriggerCameraLook",
        DefaultValue = DEBUG_SHOW_CAMERA_TRIGGER_EXTENSIONS,
        IsRequired = false
        )]
    public bool ShowTriggerCameraLook
    {
        get => (bool) this["showTriggerCameraLook"];
        set => this["showTriggerCameraLook"] = value;
    }

    [ConfigurationProperty("currentSaveSlot",
        DefaultValue = 0,
        IsRequired = false
        )]
    public int CurrentSaveSlot
    {
        get => (int) this["currentSaveSlot"];
        set => this["currentSaveSlot"] = value;
    }
}

public class GameEngine : IRenderable, IRenderTarget
{
    public static GameEngine Engine
    {
        get;
        private set;
    }

    public static void Initialize(Control control)
    {
        Engine = new GameEngine(control);
    }

    public static void Run()
    {
        Engine.Execute();
    }

    public static void Dispose()
    {
        if (Engine != null)
        {
            Engine.Unload();
            Engine = null;
        }
    }

    public const VertexFormat D3DFVF_TLVERTEX = VertexFormat.Position | VertexFormat.Diffuse | VertexFormat.Texture1;
    public const int VERTEX_SIZE = 5 * sizeof(float) + sizeof(int);

    private PresentParameters presentationParams;
    private DXSprite sprite;
    private Line line;
    private Font infoFont;
    private Font coordsTextFont;
    private Font highlightMapTextFont;
    private Texture whitePixelTexture;
    private Texture blackPixelTexture;
    private Texture foregroundTilemap;
    private Texture backgroundTilemap;
    private Texture foregroundPalette;
    private Texture backgroundPalette;

    private Texture stageTexture;

    private EffectHandle psFadingLevelHandle;
    private EffectHandle psFadingColorHandle;
    private EffectHandle plsFadingLevelHandle;
    private EffectHandle plsFadingColorHandle;

    private readonly List<SpriteSheet> spriteSheets;
    private readonly Dictionary<string, int> spriteSheetsByName;

    private readonly List<Palette> precachedPalettes;
    private readonly Dictionary<string, int> precachedPalettesByName;

    internal readonly List<PrecachedSound> precachedSounds;
    internal readonly Dictionary<string, int> precachedSoundsByName;
    internal readonly Dictionary<string, int> precachedSoundsByFileName;
    private readonly List<SoundChannel> soundChannels;
    private readonly Dictionary<string, int> soundChannelsByName;

    private readonly DirectInput directInput;
    private readonly Keyboard keyboard;
    private Joystick joystick;

    private Lua lua;

    internal Partition<Entity> partition;
    private EntitySet<Entity> resultSet;

    private int lastLives;
    private bool respawning;
    private Vector lastPlayerOrigin;
    private Vector lastPlayerVelocity;
    private Vector lastCameraLeftTop;

    private List<EntityReference<Checkpoint>> checkpoints;
    private EntitySet<Entity> aliveEntities;
    internal EntitySet<Entity> spawnedEntities;
    internal EntitySet<Entity> removedEntities;
    private readonly EntitySet<Sprite> freezingSpriteExceptions;
    private readonly List<Sprite>[] sprites;
    private readonly List<HUD>[] huds;
    private ushort currentLevel;
    private bool changeLevel;
    private ushort levelToChange;
    private bool gameOver;
    private bool loadingLevel;
    private bool paused;

    private long lastCurrentMemoryUsage;
    private Box drawBox;
    private EntityReference<Checkpoint> currentCheckpoint;
    private readonly List<Vector> cameraConstraints;

    private bool frameAdvance;
    private bool recording;
    private bool playbacking;

    private bool wasPressingToggleFrameAdvance;
    private bool wasPressingNextFrame;
    private bool wasPressingSaveState;
    private bool wasPressingLoadState;
    private bool wasPressingNextSlot;
    private bool wasPressingPreviousSlot;
    private bool wasPressingRecord;
    private bool wasPressingPlayback;
    private bool wasPressingToggleNoClip;
    private bool wasPressingToggleCameraConstraints;
    private bool wasPressingToggleDrawCollisionBox;
    private bool wasPressingToggleShowColliders;
    private bool wasPressingToggleDrawMapBounds;
    private bool wasPressingToggleDrawTouchingMapBounds;
    private bool wasPressingToggleDrawHighlightedPointingTiles;
    private bool wasPressingToggleDrawPlayerOriginAxis;
    private bool wasPressingToggleShowInfoText;
    private bool wasPressingToggleShowCheckpointBounds;
    private bool wasPressingToggleShowTriggerBounds;
    private bool wasPressingToggleShowTriggerCameraLook;
    private bool wasPressingToggleDrawBackground;
    private bool wasPressingToggleDrawDownLayer;
    private bool wasPressingToggleDrawUpLayer;
    private bool wasPressingToggleDrawSprites;

    private MMXCoreLoader mmx;
    private bool romLoaded;

    private bool drawBackground = true;
    private bool drawDownLayer = true;
    private bool drawSprites = true;
    private bool drawX = true;
    private bool drawUpLayer = true;

    private bool drawHitbox = DEBUG_DRAW_HITBOX;
    private bool showDrawBox = DEBUG_SHOW_BOUNDING_BOX;
    private bool showColliders = DEBUG_SHOW_COLLIDERS;
    private bool drawLevelBounds = DEBUG_DRAW_MAP_BOUNDS;
    private bool drawTouchingMapBounds = DEBUG_HIGHLIGHT_TOUCHING_MAPS;
    private bool drawHighlightedPointingTiles = DEBUG_HIGHLIGHT_POINTED_TILES;
    private bool drawPlayerOriginAxis = DEBUG_DRAW_PLAYER_ORIGIN_AXIS;
    private bool showInfoText = DEBUG_SHOW_INFO_TEXT;
    private bool showCheckpointBounds = DEBUG_DRAW_CHECKPOINT;
    private bool showTriggerBounds = DEBUG_SHOW_TRIGGERS;
    private bool showTriggerCameraLockDirection = DEBUG_SHOW_CAMERA_TRIGGER_EXTENSIONS;
    private bool enableSpawningBlackScreen = ENABLE_SPAWNING_BLACK_SCREEN;

    // Create Clock and FPS counters
    private readonly Stopwatch clock = new();
    private long previousElapsedTicks;
    private long lastMeasuringFPSElapsedTicks;
    private long targetElapsedTime;
    private long renderFrameCounter;
    private long lastRenderFrameCounter;

    private string infoMessage = null;
    private long infoMessageStartTime;
    private int infoMessageShowingTime;
    private int infoMessageFadingTime;
    private FadingControl infoMessageFadingControl;

    private EntityReference<Camera> camera;
    private EntityReference<Player> player;
    private EntityReference<PlayerHealthHUD> hp;
    private EntityReference<ReadyHUD> readyHUD;
    private EntityReference<Boss> boss;

    internal Dictionary<string, PrecacheAction> precacheActions;

    public Control Control
    {
        get;
    }

    public Direct3D Direct3D
    {
        get;
        private set;
    }

    public Device9 Device
    {
        get;
        private set;
    }

    public VertexBuffer VertexBuffer
    {
        get;
        private set;
    }

    public RectangleF RenderRectangle => ToRectangleF(drawBox);

    public MMXWorld World
    {
        get;
        private set;
    }

    public EntityFactory Entities
    {
        get;
    }

    public IReadOnlySet<Entity> AliveEntities => aliveEntities;

    public Camera Camera => camera;

    public Player Player => player;

    public PlayerHealthHUD HP => hp;

    public ReadyHUD ReadyHUD => readyHUD;

    public FixedSingle HealthCapacity
    {
        get;
        set;
    } = X_INITIAL_HEALT_CAPACITY;

    public Box CameraConstraintsBox
    {
        get;
        set;
    }

    public Vector CameraConstraintsOrigin
    {
        get;
        set;
    }

    public IEnumerable<Vector> CameraConstraints => cameraConstraints;

    public bool NoCameraConstraints
    {
        get;
        set;
    }

    public Vector MinCameraPos => NoCameraConstraints || Camera.NoConstraints ? World.ForegroundLayout.BoundingBox.LeftTop : CameraConstraintsBox.LeftTop;

    public Vector MaxCameraPos => NoCameraConstraints || Camera.NoConstraints ? World.ForegroundLayout.BoundingBox.RightBottom : CameraConstraintsBox.RightBottom;

    public bool Paused
    {
        get => paused;

        set
        {
            if (value && !paused)
                PauseGame();
            else if (!value && paused)
                ContinueGame();
        }
    }

    public FixedSingle DrawScale
    {
        get;
        set;
    } = DEFAULT_DRAW_SCALE;

    public PixelShader PixelShader
    {
        get;
        private set;
    }

    public PixelShader PaletteShader
    {
        get;
        private set;
    }

    public string ROMPath
    {
        get;
        private set;
    } = null;

    public Checkpoint CurrentCheckpoint
    {
        get => currentCheckpoint;
        set => SetCheckpoint(value);
    }

    internal Texture ForegroundTilemap
    {
        get => foregroundTilemap;
        set
        {
            if (foregroundTilemap != value)
            {
                DisposeResource(foregroundTilemap);
                foregroundTilemap = value;
            }
        }
    }

    internal Texture BackgroundTilemap
    {
        get => backgroundTilemap;
        set
        {
            if (backgroundTilemap != value)
            {
                DisposeResource(backgroundTilemap);
                backgroundTilemap = value;
            }
        }
    }

    internal Texture ForegroundPalette
    {
        get => foregroundPalette;
        set
        {
            if (foregroundPalette != value)
            {
                DisposeResource(foregroundPalette);
                foregroundPalette = value;
            }
        }
    }

    internal Texture BackgroundPalette
    {
        get => backgroundPalette;
        set
        {
            if (backgroundPalette != value)
            {
                DisposeResource(backgroundPalette);
                backgroundPalette = value;
            }
        }
    }

    public int ObjectTile
    {
        get => mmx.ObjLoad;

        set
        {
            mmx.SetLevel(mmx.Level, CurrentCheckpoint.Point, value, mmx.TileLoad, mmx.PalLoad);
            mmx.LoadTilesAndPalettes();
            mmx.LoadPalette(false);
            mmx.LoadPalette(true);
            mmx.RefreshMapCache(false);
            mmx.RefreshMapCache(true);
        }
    }

    public int BackgroundTile
    {
        get => mmx.TileLoad;

        set
        {
            mmx.SetLevel(mmx.Level, CurrentCheckpoint.Point, mmx.ObjLoad, value, mmx.PalLoad);
            mmx.LoadTilesAndPalettes();
            mmx.LoadPalette(false);
            mmx.LoadPalette(true);
            mmx.RefreshMapCache(false);
            mmx.RefreshMapCache(true);
        }
    }

    public int Palette
    {
        get => mmx.PalLoad;

        set
        {
            mmx.SetLevel(mmx.Level, CurrentCheckpoint.Point, mmx.ObjLoad, mmx.TileLoad, value);
            mmx.LoadTilesAndPalettes();
            mmx.LoadPalette(false);
            mmx.LoadPalette(true);
            mmx.RefreshMapCache(false);
            mmx.RefreshMapCache(true);
        }
    }

    public Color BackgroundColor
    {
        get;
        set;
    } = Color.Black;

    public bool Running
    {
        get;
        internal set;
    }

    public IReadOnlyList<EntityReference<Checkpoint>> Checkpoints => checkpoints;

    public long FrameCounter
    {
        get;
        private set;
    } = 0;

    public FadingControl FadingControl
    {
        get;
    }

    public float FadingOSTLevel
    {
        get;
        private set;
    }

    public float FadingOSTInitialVolume
    {
        get;
        private set;
    }

    public float FadingOSTVolume
    {
        get;
        private set;
    }

    public bool FadingOST
    {
        get;
        private set;
    }

    public bool FadeInOST
    {
        get;
        private set;
    }

    public long FadingOSTFrames
    {
        get;
        private set;
    }

    public long FadingOSTTick
    {
        get;
        private set;
    }

    public Action OnFadingOSTComplete
    {
        get;
        private set;
    }

    public bool SpawningBlackScreen
    {
        get;
        private set;
    } = false;

    public int SpawningBlackScreenFrameCounter
    {
        get;
        private set;
    } = 0;

    public bool DyingEffectActive
    {
        get;
        private set;
    } = false;

    public int DyingEffectFrameCounter
    {
        get;
        private set;
    } = 0;

    public bool Freezing
    {
        get;
        private set;
    }

    public int FreezingFrames
    {
        get;
        private set;
    }

    public Action OnFreezeComplete
    {
        get;
        private set;
    }

    public int FreezeFrameCounter
    {
        get;
        private set;
    }

    public int FreezingFrameCounter
    {
        get;
        private set;
    }

    public bool FreezingSprites
    {
        get;
        private set;
    } = false;

    public int FreezingSpritesFrames
    {
        get;
        private set;
    } = 0;

    public int FreezingSpritesFrameCounter
    {
        get;
        private set;
    } = 0;

    public Action OnFreezeSpritesComplete
    {
        get;
        private set;
    } = null;

    public Action DelayedAction
    {
        get;
        private set;
    } = null;

    public int DelayedActionFrames
    {
        get;
        private set;
    } = 0;

    public int DelayedActionFrameCounter
    {
        get;
        private set;
    } = 0;

    public RNG RNG
    {
        get;
        private set;
    }

    public Boss Boss
    {
        get => boss;
        private set => boss = value;
    }

    public bool BossBattle
    {
        get;
        private set;
    } = false;

    public bool BossIntroducing
    {
        get;
        private set;
    } = false;

    public int CurrentSaveSlot
    {
        get;
        set;
    } = 0;

    private GameEngine(Control control)
    {
        Control = control;

        RNG = new RNG();

        Entities = new EntityFactory();
        aliveEntities = new EntitySet<Entity>();
        spawnedEntities = new EntitySet<Entity>();
        removedEntities = new EntitySet<Entity>();
        sprites = new List<Sprite>[NUM_SPRITE_LAYERS];
        huds = new List<HUD>[NUM_SPRITE_LAYERS];

        for (int i = 0; i < sprites.Length; i++)
            sprites[i] = new List<Sprite>();

        for (int i = 0; i < huds.Length; i++)
            huds[i] = new List<HUD>();

        freezingSpriteExceptions = new EntitySet<Sprite>();
        checkpoints = new List<EntityReference<Checkpoint>>();

        spriteSheets = new List<SpriteSheet>();
        spriteSheetsByName = new Dictionary<string, int>();

        precachedPalettes = new List<Palette>();
        precachedPalettesByName = new Dictionary<string, int>();

        precachedSounds = new List<PrecachedSound>();
        precachedSoundsByName = new Dictionary<string, int>();
        precachedSoundsByFileName = new Dictionary<string, int>();
        soundChannels = new List<SoundChannel>();
        soundChannelsByName = new Dictionary<string, int>();

        FadingControl = new FadingControl();
        infoMessageFadingControl = new FadingControl();

        precacheActions = new Dictionary<string, PrecacheAction>();

        lua = new Lua();
        lua.LoadCLRPackage(); // TODO : This can be DANGEROUS! Fix in the future by adding restrictions on the scripting.
        lua.DoString(@"import ('XSharp', 'XSharp')");
        lua.DoString(@"import ('XSharp', 'XSharp.Engine')");
        lua.DoString(@"import ('XSharp', 'XSharp.Engine.Entities')");
        lua.DoString(@"import ('XSharp', 'XSharp.Engine.Entities.Effects')");
        lua.DoString(@"import ('XSharp', 'XSharp.Engine.Entities.Enemies')");
        lua.DoString(@"import ('XSharp', 'XSharp.Engine.Entities.Enemies.Bosses')");
        lua.DoString(@"import ('XSharp', 'XSharp.Engine.Entities.HUD')");
        lua.DoString(@"import ('XSharp', 'XSharp.Engine.Entities.Items')");
        lua.DoString(@"import ('XSharp', 'XSharp.Engine.Entities.Objects')");
        lua.DoString(@"import ('XSharp', 'XSharp.Engine.Entities.Triggers')");
        lua.DoString(@"import ('XSharp', 'XSharp.Engine.Entities.Weapons')");
        lua.DoString(@"import ('XSharp', 'XSharp.Engine.Sound')");
        lua.DoString(@"import('XSharp', 'XSharp.Engine.World')");
        lua["engine"] = this;

        presentationParams = new PresentParameters
        {
            Windowed = !FULL_SCREEN,
            SwapEffect = SwapEffect.Discard,
            PresentationInterval = VSYNC ? PresentInterval.One : PresentInterval.Immediate,
            FullScreenRefreshRateInHz = FULL_SCREEN ? TICKRATE : 0,
            AutoDepthStencilFormat = Format.D16,
            EnableAutoDepthStencil = true,
            BackBufferCount = DOUBLE_BUFFERED ? 2 : 1,
            BackBufferFormat = FULL_SCREEN ? Format.X8R8G8B8 : Format.Unknown,
            BackBufferHeight = FULL_SCREEN ? Control.ClientSize.Height : 0,
            BackBufferWidth = FULL_SCREEN ? Control.ClientSize.Width : 0,
            PresentFlags = PresentFlags.LockableBackBuffer
        };

        Direct3D = new Direct3D();

        NoCameraConstraints = NO_CAMERA_CONSTRAINTS;

        cameraConstraints = new List<Vector>();

        // Sound channels:
        // 0 - X
        // 1 - Weapons (including X-Buster)
        // 2 - Effects (charging, explosions, damage hit, etc)
        // 3 - OST
        // 4 - Enemies (including bosses)
        // 5 - Ambient
        // 6 - Unused
        // 7 - Unused

        CreateSoundChannel("X", 0.25f); // 0
        CreateSoundChannel("Weapons", 0.25f); // 1
        CreateSoundChannel("Effects", 0.25f); // 2
        CreateSoundChannel("OST", 0.5f); // 3
        CreateSoundChannel("Enemies", 0.25f); // 4
        CreateSoundChannel("Ambient", 0.25f); // 5
        CreateSoundChannel("Unused1", 0.25f); // 6
        CreateSoundChannel("Unused2", 0.25f); // 7

        directInput = new DirectInput();

        keyboard = new Keyboard(directInput);
        keyboard.Properties.BufferSize = 2048;
        keyboard.Acquire();

        var joystickGuid = Guid.Empty;
        foreach (var deviceInstance in directInput.GetDevices(SharpDX.DirectInput.DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices))
            joystickGuid = deviceInstance.InstanceGuid;

        if (joystickGuid == Guid.Empty)
        {
            foreach (var deviceInstance in directInput.GetDevices(SharpDX.DirectInput.DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
                joystickGuid = deviceInstance.InstanceGuid;
        }

        if (joystickGuid != Guid.Empty)
        {
            joystick = new Joystick(directInput, joystickGuid);
            joystick.Properties.BufferSize = 2048;
            joystick.Acquire();
        }

        loadingLevel = true;

        DrawScale = DEFAULT_DRAW_SCALE;
        UpdateScale();

        Running = true;
    }

    public bool PrecacheSound(string name, string path, out PrecachedSound sound, bool raiseExceptionIfNameExists = true)
    {
        if (precachedSoundsByFileName.TryGetValue(path, out int index))
        {
            sound = precachedSounds[index];
            sound.AddName(name);

            if (!precachedSoundsByName.ContainsKey(name))
                precachedSoundsByName.Add(name, index);

            return false;
        }

        if (precachedSoundsByName.TryGetValue(name, out index))
        {
            if (raiseExceptionIfNameExists)
                throw new DuplicatePrecachedSoundNameException(name);

            sound = precachedSounds[index];
            return false;
        }

        var stream = WaveStreamUtil.FromFile(path);
        sound = new PrecachedSound(name, path, stream);
        index = precachedSounds.Count;
        precachedSounds.Add(sound);
        precachedSoundsByName.Add(name, index);
        precachedSoundsByFileName.Add(path, index);
        return true;
    }

    public bool PrecacheSound(string name, string path)
    {
        return PrecacheSound(name, path, out _);
    }

    private void Unload()
    {
        UnloadLevel();

        DisposeResource(lua);

        foreach (var channel in soundChannels)
            DisposeResource(channel);

        soundChannels.Clear();
        soundChannelsByName.Clear();

        foreach (var sound in precachedSounds)
            DisposeResource(sound);

        precachedSounds.Clear();
        precachedSoundsByName.Clear();
        precachedSoundsByFileName.Clear();

        DisposeResource(mmx);
        DisposeDevice();
        DisposeResource(Direct3D);
    }

    private void ResetDevice()
    {
        DisposeDevice();

        // Creates the Device
        var device = new Device9(Direct3D, 0, DeviceType.Hardware, Control.Handle, CreateFlags.HardwareVertexProcessing | CreateFlags.FpuPreserve | CreateFlags.Multithreaded, presentationParams);
        Device = device;

        var function = ShaderBytecode.CompileFromFile("PixelShader.hlsl", "main", "ps_2_0");
        PixelShader = new PixelShader(device, function);

        psFadingLevelHandle = PixelShader.Function.ConstantTable.GetConstantByName(null, "fadingLevel");
        psFadingColorHandle = PixelShader.Function.ConstantTable.GetConstantByName(null, "fadingColor");

        function = ShaderBytecode.CompileFromFile("PaletteShader.hlsl", "main", "ps_2_0");
        PaletteShader = new PixelShader(device, function);

        plsFadingLevelHandle = PaletteShader.Function.ConstantTable.GetConstantByName(null, "fadingLevel");
        plsFadingColorHandle = PaletteShader.Function.ConstantTable.GetConstantByName(null, "fadingColor");

        device.VertexShader = null;
        device.PixelShader = PixelShader;
        device.VertexFormat = D3DFVF_TLVERTEX;

        VertexBuffer = new VertexBuffer(device, VERTEX_SIZE * 6, Usage.WriteOnly, D3DFVF_TLVERTEX, Pool.Managed);

        device.SetRenderState(RenderState.ZEnable, false);
        device.SetRenderState(RenderState.Lighting, false);
        device.SetRenderState(RenderState.AlphaBlendEnable, true);
        device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
        device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
        device.SetTextureStageState(0, TextureStage.AlphaOperation, TextureOperation.SelectArg1);
        device.SetTextureStageState(0, TextureStage.ColorArg1, TextureArgument.Texture);
        device.SetTextureStageState(1, TextureStage.ColorOperation, TextureOperation.Disable);
        device.SetTextureStageState(1, TextureStage.AlphaOperation, TextureOperation.Disable);
        device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
        device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);

        stageTexture = new Texture(device, SCREEN_WIDTH, SCREEN_HEIGHT, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);

        sprite = new DXSprite(device);
        line = new Line(device);

        var fontDescription = new FontDescription()
        {
            Height = 36,
            Italic = false,
            CharacterSet = FontCharacterSet.Ansi,
            FaceName = "Arial",
            MipLevels = 0,
            OutputPrecision = FontPrecision.TrueType,
            PitchAndFamily = FontPitchAndFamily.Default,
            Quality = FontQuality.Antialiased,
            Weight = FontWeight.Bold
        };

        infoFont = new Font(device, fontDescription);

        fontDescription = new FontDescription()
        {
            Height = 24,
            Italic = false,
            CharacterSet = FontCharacterSet.Ansi,
            FaceName = "Arial",
            MipLevels = 0,
            OutputPrecision = FontPrecision.TrueType,
            PitchAndFamily = FontPitchAndFamily.Default,
            Quality = FontQuality.Antialiased,
            Weight = FontWeight.Bold
        };

        coordsTextFont = new Font(device, fontDescription);

        fontDescription = new FontDescription()
        {
            Height = 24,
            Italic = false,
            CharacterSet = FontCharacterSet.Ansi,
            FaceName = "Arial",
            MipLevels = 0,
            OutputPrecision = FontPrecision.TrueType,
            PitchAndFamily = FontPitchAndFamily.Default,
            Quality = FontQuality.Antialiased,
            Weight = FontWeight.Bold
        };

        highlightMapTextFont = new Font(device, fontDescription);

        using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XSharp.resources.tiles.white_pixel.png"))
        {
            whitePixelTexture = Texture.FromStream(device, stream);
        }

        using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XSharp.resources.tiles.black_pixel.png"))
        {
            blackPixelTexture = Texture.FromStream(device, stream);
        }

        SetupQuad(VertexBuffer, SCREEN_WIDTH * 4, SCREEN_HEIGHT * 4);

        RecallPrecacheActions();

        // Load tiles & object positions from the ROM (if exist)

        if (romLoaded)
        {
            mmx.SetLevel(mmx.Level, CurrentCheckpoint.Point, mmx.ObjLoad, mmx.TileLoad, mmx.PalLoad);
            mmx.LoadTilesAndPalettes();
            mmx.LoadPalette(false);
            mmx.LoadPalette(true);
            mmx.RefreshMapCache(false);
            mmx.RefreshMapCache(true);

            World.Tessellate();
        }

        foreach (var entity in Entities)
        {
            if (entity is Sprite sprite)
                sprite.OnDeviceReset();
        }
    }

    public void LoadConfig()
    {
        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        if (config.Sections["ProgramConfiguratinSection"] is not ProgramConfiguratinSection section)
        {
            section = new ProgramConfiguratinSection();
            config.Sections.Add("ProgramConfiguratinSection", section);
            config.Save();
        }

        if (Control is Form)
        {
            if (section.Left != -1)
                Control.Left = section.Left;

            if (section.Top != -1)
                Control.Top = section.Top;
        }

        drawHitbox = section.DrawCollisionBox;
        showColliders = section.ShowColliders;
        drawLevelBounds = section.DrawMapBounds;
        drawTouchingMapBounds = section.DrawTouchingMapBounds;
        drawHighlightedPointingTiles = section.DrawHighlightedPointingTiles;
        drawPlayerOriginAxis = section.DrawPlayerOriginAxis;
        showInfoText = section.ShowInfoText;
        showCheckpointBounds = section.ShowCheckpointBounds;
        showTriggerBounds = section.ShowTriggerBounds;
        showTriggerCameraLockDirection = section.ShowTriggerCameraLook;
        CurrentSaveSlot = section.CurrentSaveSlot;
    }

    public void SaveConfig()
    {
        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        if (config.Sections["ProgramConfiguratinSection"] is not ProgramConfiguratinSection section)
        {
            section = new ProgramConfiguratinSection();
            config.Sections.Add("ProgramConfiguratinSection", section);
        }

        if (Control is Form)
        {
            section.Left = Control.Left;
            section.Top = Control.Top;
        }

        section.DrawCollisionBox = drawHitbox;
        section.ShowColliders = showColliders;
        section.DrawMapBounds = drawLevelBounds;
        section.DrawTouchingMapBounds = drawTouchingMapBounds;
        section.DrawHighlightedPointingTiles = drawHighlightedPointingTiles;
        section.DrawPlayerOriginAxis = drawPlayerOriginAxis;
        section.ShowInfoText = showInfoText;
        section.ShowCheckpointBounds = showCheckpointBounds;
        section.ShowTriggerBounds = showTriggerBounds;
        section.ShowTriggerCameraLook = showTriggerCameraLockDirection;
        section.CurrentSaveSlot = CurrentSaveSlot;

        config.Save();
    }

    public void SetCheckpoint(Checkpoint value, int objectTile = -1, int backgroundTile = -1, int palette = -1)
    {
        if (CurrentCheckpoint != value)
        {
            currentCheckpoint = Entities.GetReferenceTo(value);
            if (CurrentCheckpoint != null)
            {
                CameraConstraintsBox = CurrentCheckpoint.Hitbox;

                if (romLoaded)
                {
                    mmx.SetLevel(mmx.Level, CurrentCheckpoint.Point, objectTile, backgroundTile, palette);
                    mmx.LoadTilesAndPalettes();
                    mmx.LoadPalette(false);
                    mmx.LoadPalette(true);
                    mmx.RefreshMapCache(false);
                    mmx.RefreshMapCache(true);
                }
            }
            else
            {
                CameraConstraintsBox = World.ForegroundLayout.BoundingBox;
            }
        }
    }

    internal void AddChargingEffectFrames(SpriteSheet.FrameSequence sequence, int level)
    {
        sequence.Sheet.CurrentTexture = CreateChargingTexture(new Vector[] { new Vector(27, 2), new Vector(27, 46) }, new bool[] { true, true }, new int[] { 2, 2 }, level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, true, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture(new Vector[] { new Vector(27, 3), new Vector(27, 45), new Vector(5, 24), new Vector(49, 24) }, new bool[] { true, true, true, true }, new int[] { 2, 2, 2, 2 }, level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture(new Vector[] { new Vector(6, 24), new Vector(27, 5), new Vector(27, 44), new Vector(48, 24) }, new bool[] { true, true, true, true }, new int[] { 2, 2, 2, 2 }, level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture(new Vector[] { new Vector(7, 24), new Vector(11, 40), new Vector(43, 8), new Vector(47, 24), new Vector(27, 43), new Vector(28, 6) }, new bool[] { true, true, true, true, false, false }, new int[] { 2, 1, 1, 2, 2, 2 }, level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture(new Vector[] { new Vector(8, 24), new Vector(12, 39), new Vector(42, 9), new Vector(46, 24), new Vector(27, 42), new Vector(28, 7) }, new bool[] { true, true, true, true, false, false }, new int[] { 2, 1, 1, 2, 2, 2 }, level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture(new Vector[] { new Vector(11, 8), new Vector(13, 38), new Vector(41, 10), new Vector(43, 40), new Vector(10, 25), new Vector(27, 41), new Vector(27, 7), new Vector(44, 23) }, new bool[] { true, true, true, true, false, false, false, false }, new int[] { 1, 2, 2, 1, 2, 1, 1, 2 }, level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture(new Vector[] { new Vector(12, 9), new Vector(14, 37), new Vector(40, 11), new Vector(42, 39), new Vector(11, 25), new Vector(28, 9), new Vector(43, 23) }, new bool[] { true, true, true, true, false, false, false }, new int[] { 1, 2, 2, 1, 2, 1, 2 }, level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture(new Vector[] { new Vector(13, 10), new Vector(19, 44), new Vector(35, 4), new Vector(41, 38), new Vector(12, 25), new Vector(16, 36), new Vector(39, 13), new Vector(43, 24) }, new bool[] { true, true, true, true, false, false, false, false }, new int[] { 1, 2, 2, 1, 1, 2, 2, 1 }, level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture(new Vector[] { new Vector(14, 11), new Vector(19, 43), new Vector(35, 5), new Vector(40, 37), new Vector(13, 25), new Vector(17, 35), new Vector(38, 14), new Vector(42, 24) }, new bool[] { true, true, true, true, false, false, false, false }, new int[] { 1, 2, 2, 1, 1, 2, 2, 1 }, level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture(new Vector[] { new Vector(7, 16), new Vector(20, 42), new Vector(34, 6), new Vector(47, 32), new Vector(16, 13), new Vector(18, 34), new Vector(37, 15), new Vector(39, 36) }, new bool[] { true, true, true, true, false, false, false, false }, new int[] { 2, 1, 1, 2, 1, 2, 2, 1 }, level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture(new Vector[] { new Vector(8, 16), new Vector(20, 41), new Vector(34, 7), new Vector(46, 32), new Vector(17, 4), new Vector(19, 33), new Vector(36, 16), new Vector(38, 35) }, new bool[] { true, true, true, true, false, false, false, false }, new int[] { 2, 1, 1, 2, 1, 2, 2, 1 }, level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture(new Vector[] { new Vector(9, 16), new Vector(19, 4), new Vector(24, 40), new Vector(33, 8), new Vector(35, 44), new Vector(45, 31), new Vector(18, 15), new Vector(37, 34) }, new bool[] { true, true, true, true, true, true, false, false }, new int[] { 2, 2, 1, 1, 2, 2, 1, 1 }, level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture(new Vector[] { new Vector(10, 17), new Vector(19, 5), new Vector(21, 39), new Vector(39, 9), new Vector(35, 43), new Vector(44, 30), new Vector(19, 16), new Vector(36, 33) }, new bool[] { true, true, true, true, true, true, false, false }, new int[] { 2, 2, 1, 1, 2, 2, 1, 1 }, level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture(new Vector[] { new Vector(7, 32), new Vector(47, 16), new Vector(12, 18), new Vector(21, 7), new Vector(22, 38), new Vector(33, 11), new Vector(35, 43), new Vector(44, 31) }, new bool[] { true, true, false, false, false, false, false, false }, new int[] { 2, 2, 2, 2, 1, 1, 2, 2 }, level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture(new Vector[] { new Vector(8, 32), new Vector(46, 16), new Vector(13, 19), new Vector(20, 8), new Vector(22, 37), new Vector(33, 12), new Vector(36, 42), new Vector(43, 30) }, new bool[] { true, true, false, false, false, false, false, false }, new int[] { 2, 2, 2, 2, 1, 1, 2, 2 }, level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture(new Vector[] { new Vector(21, 8), new Vector(33, 40), new Vector(10, 32), new Vector(14, 19), new Vector(42, 30), new Vector(46, 18) }, new bool[] { true, true, false, false, false, false }, new int[] { 1, 1, 2, 2, 2, 2 }, level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture(new Vector[] { new Vector(21, 9), new Vector(33, 39), new Vector(11, 32), new Vector(15, 20), new Vector(41, 29), new Vector(45, 18) }, new bool[] { true, true, false, false, false, false }, new int[] { 1, 1, 2, 2, 2, 2 }, level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture(new Vector[] { new Vector(11, 29), new Vector(43, 17), new Vector(23, 10) }, new bool[] { true, true, false }, new int[] { 1, 1, 1 }, level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture(new Vector[] { new Vector(13, 30), new Vector(42, 19) }, new bool[] { true, true }, new int[] { 1, 1 }, level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture(new Vector[] { new Vector(14, 29), new Vector(41, 20) }, new bool[] { false, false }, new int[] { 1, 1 }, level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture(new Vector[] { new Vector(15, 29), new Vector(40, 20) }, new bool[] { false, false }, new int[] { 1, 1 }, level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture(new Vector[] { new Vector(27, 1), new Vector(27, 47) }, new bool[] { true, true }, new int[] { 2, 2 }, level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
    }

    public void StartFadingOST(float volume, int frames, bool fadeIn, Action onFadingComplete = null)
    {
        FadingOST = true;
        FadingOSTInitialVolume = soundChannels[3].Volume;
        FadingOSTVolume = volume;
        FadingOSTFrames = frames;
        FadingOSTLevel = fadeIn ? 1 : 0;
        FadeInOST = fadeIn;
        FadingOSTTick = 0;
        OnFadingOSTComplete = onFadingComplete;
    }

    public void StartFadingOST(float volume, int frames, Action onFadingComplete = null)
    {
        StartFadingOST(volume, frames, false, onFadingComplete);
    }

    public static uint NextHighestPowerOfTwo(uint v)
    {
        v--;
        v |= v >> 1;
        v |= v >> 2;
        v |= v >> 4;
        v |= v >> 8;
        v |= v >> 16;
        v++;

        return v;
    }

    private static readonly byte[] EMPTY_TEXTURE_DATA = new byte[4096];

    private static void ZeroDataRect(DataRectangle dataRect, int length)
    {
        int remaining = length;
        IntPtr ptr = dataRect.DataPointer;
        while (remaining > 0)
        {
            int bytesToCopy = remaining > EMPTY_TEXTURE_DATA.Length ? EMPTY_TEXTURE_DATA.Length : remaining;
            Marshal.Copy(EMPTY_TEXTURE_DATA, 0, ptr, bytesToCopy);
            ptr += bytesToCopy;
            remaining -= bytesToCopy;
        }
    }

    private static void FillRegion(DataRectangle dataRect, int length, Box box, int paletteIndex)
    {
        int dstX = (int) box.Left;
        int dstY = (int) box.Top;
        int width = (int) box.Width;
        int height = (int) box.Height;

        using var dstDS = new DataStream(dataRect.DataPointer, length * sizeof(byte), true, true);
        for (int y = dstY; y < dstY + height; y++)
        {
            for (int x = dstX; x < dstX + width; x++)
            {
                dstDS.Seek(y * dataRect.Pitch + x * sizeof(byte), SeekOrigin.Begin);
                dstDS.Write((byte) paletteIndex);
            }
        }
    }

    private static void DrawChargingPointLevel1Small(DataRectangle dataRect, int length, Vector point)
    {
        FillRegion(dataRect, length, new Box(point.X, point.Y, 1, 1), 1);
    }

    private static void DrawChargingPointLevel1Large(DataRectangle dataRect, int length, Vector point)
    {
        FillRegion(dataRect, length, new Box(point.X, point.Y, 2, 2), 1);
    }

    private static void DrawChargingPointLevel2Small1(DataRectangle dataRect, int length, Vector point)
    {
        FillRegion(dataRect, length, new Box(point.X, point.Y, 1, 1), 2);
    }

    private static void DrawChargingPointLevel2Small2(DataRectangle dataRect, int length, Vector point)
    {
        FillRegion(dataRect, length, new Box(point.X, point.Y, 1, 1), 3);

        FillRegion(dataRect, length, new Box(point.X, point.Y - 1, 1, 1), 4);
        FillRegion(dataRect, length, new Box(point.X, point.Y + 1, 1, 1), 4);
        FillRegion(dataRect, length, new Box(point.X - 1, point.Y, 1, 1), 4);
        FillRegion(dataRect, length, new Box(point.X + 1, point.Y, 1, 1), 4);

        FillRegion(dataRect, length, new Box(point.X - 1, point.Y - 1, 1, 1), 5);
        FillRegion(dataRect, length, new Box(point.X + 1, point.Y - 1, 1, 1), 5);
        FillRegion(dataRect, length, new Box(point.X - 1, point.Y + 1, 1, 1), 5);
        FillRegion(dataRect, length, new Box(point.X + 1, point.Y + 1, 1, 1), 5);
    }

    private static void DrawChargingPointLevel2Large1(DataRectangle dataRect, int length, Vector point)
    {
        FillRegion(dataRect, length, new Box(point.X, point.Y, 2, 2), 2);
    }

    private static void DrawChargingPointLevel2Large2(DataRectangle dataRect, int length, Vector point)
    {
        FillRegion(dataRect, length, new Box(point.X, point.Y, 2, 2), 3);

        FillRegion(dataRect, length, new Box(point.X, point.Y - 1, 2, 1), 4);
        FillRegion(dataRect, length, new Box(point.X, point.Y + 2, 2, 1), 4);
        FillRegion(dataRect, length, new Box(point.X - 1, point.Y, 1, 2), 4);
        FillRegion(dataRect, length, new Box(point.X + 2, point.Y, 1, 2), 4);
    }

    private static void DrawChargingPointSmall(DataRectangle dataRect, int length, Vector point, int level, int type)
    {
        switch (type)
        {
            case 1:
                switch (level)
                {
                    case 1:
                        DrawChargingPointLevel1Small(dataRect, length, point);
                        break;

                    case 2:
                        DrawChargingPointLevel2Small1(dataRect, length, point);
                        break;
                }

                break;

            case 2:
                switch (level)
                {
                    case 1:
                        DrawChargingPointLevel1Small(dataRect, length, point);
                        break;

                    case 2:
                        DrawChargingPointLevel2Small2(dataRect, length, point);
                        break;
                }

                break;
        }
    }

    private static void DrawChargingPointLarge(DataRectangle dataRect, int length, Vector point, int level, int type)
    {
        switch (type)
        {
            case 1:
                switch (level)
                {
                    case 1:
                        DrawChargingPointLevel1Large(dataRect, length, point);
                        break;

                    case 2:
                        DrawChargingPointLevel2Large1(dataRect, length, point);
                        break;
                }

                break;

            case 2:
                switch (level)
                {
                    case 1:
                        DrawChargingPointLevel1Large(dataRect, length, point);
                        break;

                    case 2:
                        DrawChargingPointLevel2Large2(dataRect, length, point);
                        break;
                }

                break;
        }
    }

    internal Texture CreateChargingTexture(Vector[] points, bool[] large, int[] types, int level)
    {
        int width1 = (int) NextHighestPowerOfTwo(CHARGING_EFFECT_HITBOX_SIZE);
        int height1 = (int) NextHighestPowerOfTwo(CHARGING_EFFECT_HITBOX_SIZE);
        int length = width1 * height1;

        var result = new Texture(Device, width1, height1, 1, Usage.None, Format.L8, Pool.Managed);
        DataRectangle rect = result.LockRectangle(0, D3D9LockFlags.Discard);

        ZeroDataRect(rect, length);

        for (int i = 0; i < points.Length; i++)
        {
            if (large[i])
                DrawChargingPointLarge(rect, length, points[i], level, types[i]);
            else
                DrawChargingPointSmall(rect, length, points[i], level, types[i]);
        }

        result.UnlockRectangle(0);
        return result;
    }

    public Texture CreateImageTextureFromFile(string filePath)
    {
        var result = Texture.FromFile(Device, filePath, Usage.None, Pool.SystemMemory);
        return result;
    }

    public Texture CreateImageTextureFromEmbeddedResource(string path)
    {
        return CreateImageTextureFromEmbeddedResource(Assembly.GetExecutingAssembly(), path);
    }

    public Texture CreateImageTextureFromEmbeddedResource(Assembly assembly, string path)
    {
        string assemblyName = assembly.GetName().Name;
        using (var stream = assembly.GetManifestResourceStream($"{assemblyName}.resources.{path}"))
        {
            var texture = Engine.CreateImageTextureFromStream(stream);
            return texture;
        }
    }

    public Texture CreateImageTextureFromStream(Stream stream)
    {
        var result = Texture.FromStream(Device, stream, Usage.None, Pool.SystemMemory);
        return result;
    }

    private static void WriteVertex(DataStream vbData, float x, float y, float u, float v)
    {
        vbData.Write(x);
        vbData.Write(y);
        vbData.Write(0f);
        vbData.Write(0xffffffff);
        vbData.Write(u);
        vbData.Write(v);
    }

    public static void SetupQuad(VertexBuffer vb, float width, float height)
    {
        DataStream vbData = vb.Lock(0, 0, D3D9LockFlags.None);
        WriteSquare(vbData, (0, 0), (0, 0), (1, 1), (width, height));
        vb.Unlock();
    }

    public static void SetupQuad(VertexBuffer vb)
    {
        DataStream vbData = vb.Lock(0, 4 * VERTEX_SIZE, D3D9LockFlags.None);

        WriteVertex(vbData, 0, 0, 0, 0);
        WriteVertex(vbData, 1, 0, 1, 0);
        WriteVertex(vbData, 1, -1, 1, 1);
        WriteVertex(vbData, 0, -1, 0, 1);

        vb.Unlock();
    }

    public void RenderSprite(Texture texture, Palette palette, FadingControl fadingControl, Box box, Matrix transform, int repeatX = 1, int repeatY = 1)
    {
        RectangleF rDest = WorldBoxToScreen(box);

        var matScaling = Matrix.Scaling(1, 1, 1);

        sprite.Begin(SpriteFlags.AlphaBlend);

        Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Point);
        Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Point);
        Device.SetSamplerState(0, SamplerState.MipFilter, TextureFilter.None);

        PixelShader shader;
        EffectHandle fadingLevelHandle;
        EffectHandle fadingColorHandle;

        if (palette != null)
        {
            fadingLevelHandle = plsFadingLevelHandle;
            fadingColorHandle = plsFadingColorHandle;
            shader = PaletteShader;
            Device.SetTexture(1, palette.Texture);
        }
        else
        {
            fadingLevelHandle = psFadingLevelHandle;
            fadingColorHandle = psFadingColorHandle;
            shader = PixelShader;
        }

        Device.PixelShader = shader;
        Device.VertexShader = null;

        if (fadingControl != null)
        {
            shader.Function.ConstantTable.SetValue(Device, fadingLevelHandle, fadingControl.FadingLevel);
            shader.Function.ConstantTable.SetValue(Device, fadingColorHandle, fadingControl.FadingColor.ToVector4());
        }
        else
        {
            shader.Function.ConstantTable.SetValue(Device, fadingLevelHandle, Vector4.Zero);
        }

        var matTranslation = Matrix.Translation(rDest.Left, rDest.Top, 0);
        Matrix matTransform = matTranslation * transform * matScaling;
        sprite.Transform = matTransform;

        if (repeatX > 1 || repeatY > 1)
        {
            Device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Wrap);
            Device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Wrap);

            sprite.Draw(texture, Color.FromRgba(0xffffffff), ToRectangleF(box.Scale(box.Origin, repeatX, repeatY) - box.Origin));
        }
        else
        {
            Device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
            Device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);

            sprite.Draw(texture, Color.FromRgba(0xffffffff));
        }

        sprite.End();
    }

    public void RenderSprite(Texture texture, FadingControl fadingControl, Box box, Matrix transform, int repeatX = 1, int repeatY = 1)
    {
        RenderSprite(texture, null, fadingControl, box, transform, repeatX, repeatY);
    }

    public void RenderSprite(Texture texture, FadingControl fadingControl, Vector v, Matrix transform, int repeatX = 1, int repeatY = 1)
    {
        var description = texture.GetLevelDescription(0);
        RenderSprite(texture, null, fadingControl, new Box(v.X, v.Y, description.Width, description.Height), transform, repeatX, repeatY);
    }

    public void RenderSprite(Texture texture, FadingControl fadingControl, FixedSingle x, FixedSingle y, Matrix transform, int repeatX = 1, int repeatY = 1)
    {
        var description = texture.GetLevelDescription(0);
        RenderSprite(texture, null, fadingControl, new Box(x, y, description.Width, description.Height), transform, repeatX, repeatY);
    }

    public void RenderSprite(Texture texture, Palette palette, FadingControl fadingControl, Vector v, Matrix transform, int repeatX = 1, int repeatY = 1)
    {
        var description = texture.GetLevelDescription(0);
        RenderSprite(texture, palette, fadingControl, new Box(v.X, v.Y, description.Width, description.Height), transform, repeatX, repeatY);
    }

    public void RenderSprite(Texture texture, Palette palette, FadingControl fadingControl, FixedSingle x, FixedSingle y, Matrix transform, int repeatX = 1, int repeatY = 1)
    {
        var description = texture.GetLevelDescription(0);
        RenderSprite(texture, palette, fadingControl, new Box(x, y, description.Width, description.Height), transform, repeatX, repeatY);
    }

    private void ClearEntities()
    {
        Entities.Clear();
        aliveEntities.Clear();

        foreach (var layer in sprites)
            layer.Clear();

        foreach (var layer in huds)
            layer.Clear();

        spawnedEntities.Clear();
        removedEntities.Clear();

        freezingSpriteExceptions.Clear();
    }

    private void OnSpawningBlackScreenComplete()
    {
        foreach (var channel in soundChannels)
            channel.Stop();

        FadingOST = false;
        FadingOSTLevel = 0;
        soundChannels[3].Volume = 0.5f;

        if (romLoaded && mmx.Type == 0 && mmx.Level == 8)
            PlayOST("Chill Penguin", 83, 50.152);

        FadingControl.Reset();
        FadingControl.Start(Color.Black, 26, FadingFlags.COLORS, FadingFlags.COLORS, StartReadyHUD);
    }

    private Keys HandleInput(out bool nextFrame)
    {
        Keys keys = Keys.NONE;
        nextFrame = !frameAdvance;

        if (ENABLE_BACKGROUND_INPUT || Control.Focused)
        {
            keyboard.Poll();
            var state = keyboard.GetCurrentState();

            if (state.IsPressed(Key.Left))
                keys |= Keys.LEFT;

            if (state.IsPressed(Key.Up))
                keys |= Keys.UP;

            if (state.IsPressed(Key.Right))
                keys |= Keys.RIGHT;

            if (state.IsPressed(Key.Down))
                keys |= Keys.DOWN;

            if (state.IsPressed(Key.V))
                keys |= Keys.SHOT;

            if (state.IsPressed(Key.C))
                keys |= Keys.JUMP;

            if (state.IsPressed(Key.X))
                keys |= Keys.DASH;

            if (state.IsPressed(Key.D))
                keys |= Keys.WEAPON;

            if (state.IsPressed(Key.L))
                keys |= Keys.LWS;

            if (state.IsPressed(Key.R))
                keys |= Keys.RWS;

            if (state.IsPressed(Key.Return))
                keys |= Keys.START;

            if (state.IsPressed(Key.Escape))
                keys |= Keys.SELECT;

            if (state.IsPressed(Key.Pause))
            {
                if (!wasPressingToggleFrameAdvance)
                {
                    wasPressingToggleFrameAdvance = true;
                    frameAdvance = !frameAdvance;
                    nextFrame = !frameAdvance;
                }
            }
            else
            {
                wasPressingToggleFrameAdvance = false;
            }

            if (state.IsPressed(Key.Backslash))
            {
                if (!wasPressingNextFrame)
                {
                    wasPressingNextFrame = true;
                    frameAdvance = true;
                    nextFrame = true;
                }
            }
            else
            {
                wasPressingNextFrame = false;
            }

            if (state.IsPressed(Key.F5) && !state.IsPressed(Key.LeftShift) && !state.IsPressed(Key.RightShift))
            {
                if (!wasPressingSaveState)
                {
                    wasPressingSaveState = true;
                    SaveState();
                }
            }
            else
            {
                wasPressingSaveState = false;
            }

            if (state.IsPressed(Key.F7) && !state.IsPressed(Key.LeftShift) && !state.IsPressed(Key.RightShift))
            {
                if (!wasPressingLoadState)
                {
                    wasPressingLoadState = true;
                    LoadState();
                }
            }
            else
            {
                wasPressingLoadState = false;
            }

            if (state.IsPressed(Key.Equals))
            {
                if (!wasPressingNextSlot)
                {
                    wasPressingNextSlot = true;
                    CurrentSaveSlot++;
                    if (CurrentSaveSlot >= SAVE_SLOT_COUNT)
                        CurrentSaveSlot = 0;

                    ShowInfoMessage($"Current save slot seted to #{CurrentSaveSlot}.");
                }
            }
            else
            {
                wasPressingNextSlot = false;
            }

            if (state.IsPressed(Key.Minus))
            {
                if (!wasPressingPreviousSlot)
                {
                    wasPressingPreviousSlot = true;
                    CurrentSaveSlot--;
                    if (CurrentSaveSlot < 0)
                        CurrentSaveSlot = SAVE_SLOT_COUNT - 1;

                    ShowInfoMessage($"Current save slot seted to #{CurrentSaveSlot}.");
                }
            }
            else
            {
                wasPressingPreviousSlot = false;
            }

            if (state.IsPressed(Key.N))
            {
                if (!wasPressingToggleNoClip)
                {
                    wasPressingToggleNoClip = true;
                    if (Player != null)
                        Player.NoClip = !Player.NoClip;

                    ShowInfoMessage($"No clip {(Player.NoClip ? "activated" : "deactivated")}.");
                }
            }
            else
            {
                wasPressingToggleNoClip = false;
            }

            if (state.IsPressed(Key.M))
            {
                if (!wasPressingToggleCameraConstraints)
                {
                    wasPressingToggleCameraConstraints = true;
                    NoCameraConstraints = !NoCameraConstraints;

                    ShowInfoMessage($"Camera constraints {(NoCameraConstraints ? "activated" : "deactivated")}.");
                }
            }
            else
            {
                wasPressingToggleCameraConstraints = false;
            }

            if (state.IsPressed(Key.D1))
            {
                if (!wasPressingToggleDrawCollisionBox)
                {
                    wasPressingToggleDrawCollisionBox = true;
                    drawHitbox = !drawHitbox;

                    ShowInfoMessage($"Draw hitbox {(drawHitbox ? "activated" : "deactivated")}.");
                }
            }
            else
            {
                wasPressingToggleDrawCollisionBox = false;
            }

            if (state.IsPressed(Key.D2))
            {
                if (!wasPressingToggleShowColliders)
                {
                    wasPressingToggleShowColliders = true;
                    showColliders = !showColliders;

                    ShowInfoMessage($"Show colliders {(showColliders ? "activated" : "deactivated")}.");
                }
            }
            else
            {
                wasPressingToggleShowColliders = false;
            }

            if (state.IsPressed(Key.D3))
            {
                if (!wasPressingToggleDrawMapBounds)
                {
                    wasPressingToggleDrawMapBounds = true;
                    drawLevelBounds = !drawLevelBounds;

                    ShowInfoMessage($"Draw level bounds {(drawLevelBounds ? "activated" : "deactivated")}.");
                }
            }
            else
            {
                wasPressingToggleDrawMapBounds = false;
            }

            if (state.IsPressed(Key.D4))
            {
                if (!wasPressingToggleDrawTouchingMapBounds)
                {
                    wasPressingToggleDrawTouchingMapBounds = true;
                    drawTouchingMapBounds = !drawTouchingMapBounds;

                    ShowInfoMessage($"Draw touching tilemap bounds {(drawTouchingMapBounds ? "activated" : "deactivated")}.");
                }
            }
            else
            {
                wasPressingToggleDrawTouchingMapBounds = false;
            }

            if (state.IsPressed(Key.D5))
            {
                if (!wasPressingToggleDrawHighlightedPointingTiles)
                {
                    wasPressingToggleDrawHighlightedPointingTiles = true;
                    drawHighlightedPointingTiles = !drawHighlightedPointingTiles;

                    ShowInfoMessage($"Draw highlighted pointing tiles {(drawHighlightedPointingTiles ? "activated" : "deactivated")}.");
                }
            }
            else
            {
                wasPressingToggleDrawHighlightedPointingTiles = false;
            }

            if (state.IsPressed(Key.D6))
            {
                if (!wasPressingToggleDrawPlayerOriginAxis)
                {
                    wasPressingToggleDrawPlayerOriginAxis = true;
                    drawPlayerOriginAxis = !drawPlayerOriginAxis;

                    ShowInfoMessage($"Draw player origin axis {(drawPlayerOriginAxis ? "activated" : "deactivated")}.");
                }
            }
            else
            {
                wasPressingToggleDrawPlayerOriginAxis = false;
            }

            if (state.IsPressed(Key.D7))
            {
                if (!wasPressingToggleShowInfoText)
                {
                    wasPressingToggleShowInfoText = true;
                    showInfoText = !showInfoText;

                    ShowInfoMessage($"Show info text {(showInfoText ? "activated" : "deactivated")}.");
                }
            }
            else
            {
                wasPressingToggleShowInfoText = false;
            }

            if (state.IsPressed(Key.D8))
            {
                if (!wasPressingToggleShowCheckpointBounds)
                {
                    wasPressingToggleShowCheckpointBounds = true;
                    showCheckpointBounds = !showCheckpointBounds;

                    ShowInfoMessage($"Show checkpoint bounds {(showCheckpointBounds ? "activated" : "deactivated")}.");
                }
            }
            else
            {
                wasPressingToggleShowCheckpointBounds = false;
            }

            if (state.IsPressed(Key.D9))
            {
                if (!wasPressingToggleShowTriggerBounds)
                {
                    wasPressingToggleShowTriggerBounds = true;
                    showTriggerBounds = !showTriggerBounds;

                    ShowInfoMessage($"Show trigger bounds {(showTriggerBounds ? "activated" : "deactivated")}.");
                }
            }
            else
            {
                wasPressingToggleShowTriggerBounds = false;
            }

            if (state.IsPressed(Key.D0))
            {
                if (!wasPressingToggleShowTriggerCameraLook)
                {
                    wasPressingToggleShowTriggerCameraLook = true;
                    showTriggerCameraLockDirection = !showTriggerCameraLockDirection;

                    ShowInfoMessage($"Show trigger camera lock direction {(showTriggerCameraLockDirection ? "activated" : "deactivated")}.");
                }
            }
            else
            {
                wasPressingToggleShowTriggerCameraLook = false;
            }

            if (state.IsPressed(Key.F1))
            {
                if (!wasPressingToggleDrawBackground)
                {
                    wasPressingToggleDrawBackground = true;
                    drawBackground = !drawBackground;

                    ShowInfoMessage($"Draw background {(drawBackground ? "activated" : "deactivated")}.");
                }
            }
            else
            {
                wasPressingToggleDrawBackground = false;
            }

            if (state.IsPressed(Key.F2))
            {
                if (!wasPressingToggleDrawDownLayer)
                {
                    wasPressingToggleDrawDownLayer = true;
                    drawDownLayer = !drawDownLayer;

                    ShowInfoMessage($"Draw foreground down layer {(drawDownLayer ? "activated" : "deactivated")}.");
                }
            }
            else
            {
                wasPressingToggleDrawDownLayer = false;
            }

            if (state.IsPressed(Key.F3))
            {
                if (!wasPressingToggleDrawUpLayer)
                {
                    wasPressingToggleDrawUpLayer = true;
                    drawUpLayer = !drawUpLayer;

                    ShowInfoMessage($"Draw foreground up layer {(drawUpLayer ? "activated" : "deactivated")}.");
                }
            }
            else
            {
                wasPressingToggleDrawUpLayer = false;
            }

            if (state.IsPressed(Key.F4))
            {
                if (!wasPressingToggleDrawSprites)
                {
                    wasPressingToggleDrawSprites = true;
                    drawSprites = !drawSprites;

                    ShowInfoMessage($"Draw sprites {(drawSprites ? "activated" : "deactivated")}.");
                }
            }
            else
            {
                wasPressingToggleDrawSprites = false;
            }

            if (joystick != null)
            {
                try
                {
                    joystick.Poll();
                    var joyState = joystick.GetCurrentState();
                    bool[] buttons = joyState.Buttons;
                    bool reverseButtons = joystick.Capabilities.ButtonCount == 14;
                    if (buttons != null && buttons.Length > 0)
                    {
                        if (reverseButtons ? buttons[0] : buttons[2])
                            keys |= Keys.SHOT;

                        if (reverseButtons ? buttons[1] : buttons[0])
                            keys |= Keys.JUMP;

                        if (reverseButtons ? buttons[2] : buttons[1])
                            keys |= Keys.DASH;

                        if (buttons[7])
                            keys |= Keys.START;
                    }

                    int[] pov = joyState.PointOfViewControllers;
                    if (pov != null && pov.Length > 0)
                    {
                        switch (pov[0])
                        {
                            case 0:
                                keys |= Keys.UP;
                                break;

                            case 4500:
                                keys |= Keys.UP | Keys.RIGHT;
                                break;

                            case 9000:
                                keys |= Keys.RIGHT;
                                break;

                            case 13500:
                                keys |= Keys.DOWN | Keys.RIGHT;
                                break;

                            case 18000:
                                keys |= Keys.DOWN;
                                break;

                            case 22500:
                                keys |= Keys.DOWN | Keys.LEFT;
                                break;

                            case 27000:
                                keys |= Keys.LEFT;
                                break;

                            case 31500:
                                keys |= Keys.UP | Keys.LEFT;
                                break;
                        }
                    }
                }
                catch (SharpDXException e)
                {
                    MessageBox.Show(e.Message);
                    joystick = null;
                }
            }
        }

        return keys;
    }

    private void HandleScreenEffects()
    {
        FadingControl.DoFrame();

        if (FadingOST)
        {
            FadingOSTTick++;
            if (FadingOSTTick > FadingOSTFrames)
            {
                FadingOST = false;
                OnFadingOSTComplete?.Invoke();
            }
            else
            {
                FadingOSTLevel = (float) FadingOSTTick / FadingOSTFrames;
                if (FadeInOST)
                    FadingOSTLevel = 1 - FadingOSTLevel;

                soundChannels[3].Volume = FadingOSTInitialVolume * (1 - FadingOSTLevel) + FadingOSTVolume * FadingOSTLevel;
            }
        }

        if (SpawningBlackScreen)
        {
            SpawningBlackScreenFrameCounter++;
            if (SpawningBlackScreenFrameCounter >= SPAWNING_BLACK_SCREEN_FRAMES)
            {
                SpawningBlackScreen = false;
                OnSpawningBlackScreenComplete();
            }
        }

        if (DyingEffectActive)
        {
            if (Player != null && !Player.DeadByAbiss)
            {
                if (DyingEffectFrameCounter % 32 == 0)
                    CreateXDieExplosionEffect(DyingEffectFrameCounter % 64 == 0 ? 0 : System.Math.PI / 8);
                else if (DyingEffectFrameCounter == 1)
                    CreateXDieExplosionEffect(System.Math.PI / 8);
            }

            DyingEffectFrameCounter++;
        }

        if (Freezing)
        {
            if (FreezingFrames > 0 && FreezingFrameCounter >= FreezingFrames)
            {
                Freezing = false;
                OnFreezeComplete?.Invoke();
            }
            else
            {
                FreezingFrameCounter++;
            }
        }

        if (DelayedAction != null)
        {
            if (DelayedActionFrameCounter >= DelayedActionFrames)
            {
                DelayedAction?.Invoke();
                DelayedAction = null;
            }
            else
            {
                DelayedActionFrameCounter++;
            }
        }

        if (FreezingSprites)
        {
            if (FreezingSpritesFrames > 0 && FreezingSpritesFrameCounter >= FreezingSpritesFrames)
            {
                FreezingSprites = false;
                OnFreezeSpritesComplete?.Invoke();
                OnFreezeSpritesComplete = null;
            }
            else
            {
                FreezingSpritesFrameCounter++;
            }
        }
    }

    private bool OnFrame()
    {
        Keys keys = HandleInput(out bool nextFrame);

        if (!nextFrame)
            return false;

        RNG.UpdateSeed(FrameCounter);

        if (Player != null)
        {
            lastPlayerOrigin = Player.Origin;
            lastPlayerVelocity = Player.Velocity;
        }
        else
        {
            lastPlayerOrigin = Vector.NULL_VECTOR;
            lastPlayerVelocity = Vector.NULL_VECTOR;
        }

        lastCameraLeftTop = Camera != null ? Camera.LeftTop : Vector.NULL_VECTOR;

        FrameCounter++;

        //lua.DoString("if engine.Player ~= null then engine.Player:ShootLemon() end");

        HandleScreenEffects();

        if (!loadingLevel)
        {
            if (spawnedEntities.Count > 0)
            {
                foreach (var added in spawnedEntities)
                {
                    aliveEntities.Add(added);

                    if (added is Sprite sprite)
                    {
                        if (sprite is HUD hud)
                            huds[sprite.Layer].Add(hud);
                        else
                            sprites[sprite.Layer].Add(sprite);
                    }

                    added.NotifySpawn();
                    added.PostSpawn();
                }

                spawnedEntities.Clear();
            }

            Player?.PushKeys(keys);

            if (Freezing)
                return false;

            if (paused || Player != null && Player.Freezed && (!FreezingSprites || freezingSpriteExceptions.Contains(Player)))
            {
                Player?.PreThink();
                Camera?.PreThink();

                Player?.DoFrame();
            }
            else
            {
                if (FreezingSprites)
                {
                    foreach (var entity in aliveEntities)
                    {
                        if (entity is not Sprite sprite || freezingSpriteExceptions.Contains(sprite))
                        {
                            if (entity != Camera)
                                entity.PreThink();
                        }
                    }

                    Player?.PreThink();
                    Camera?.PreThink();

                    foreach (var entity in aliveEntities)
                    {
                        if (entity is not Sprite sprite || freezingSpriteExceptions.Contains(sprite))
                        {
                            if (entity != Camera)
                                entity.DoFrame();
                        }
                    }
                }
                else
                {
                    foreach (var entity in aliveEntities)
                    {
                        if (entity != Camera)
                            entity.PreThink();
                    }

                    Player?.PreThink();
                    Camera?.PreThink();

                    foreach (var entity in aliveEntities)
                    {
                        if (entity != Camera)
                            entity.DoFrame();
                    }
                }
            }

            World?.OnFrame();
            Camera?.DoFrame();

            if (removedEntities.Count > 0)
            {
                foreach (var removed in removedEntities)
                    RemoveEntity(removed);

                removedEntities.Clear();
            }
        }

        if (changeLevel)
        {
            changeLevel = false;
            LoadLevel(levelToChange);
        }

        if (gameOver)
        {

        }

        return true;
    }

    public static Vector2 ToVector2(Vector v)
    {
        return new((float) v.X, (float) v.Y);
    }

    public static Vector3 ToVector3(Vector v)
    {
        return new((float) v.X, (float) v.Y, 0);
    }

    public static Rectangle ToRectangle(Box box)
    {
        return new((int) box.Left, (int) box.Top, (int) box.Width, (int) box.Height);
    }

    public static RectangleF ToRectangleF(Box box)
    {
        return new((float) box.Left, (float) box.Top, (float) box.Width, (float) box.Height);
    }

    public Vector2 WorldVectorToScreen(Vector v)
    {
        return ToVector2((v.RoundToFloor() - Camera.LeftTop.RoundToFloor()) * DrawScale + drawBox.LeftTop);
    }

    public Vector2 WorldVectorToScreen(FixedSingle x, FixedSingle y)
    {
        return WorldVectorToScreen(new Vector(x, y));
    }

    public Vector ScreenPointToVector(int x, int y)
    {
        return ScreenPointToVector(new Point(x, y));
    }

    public Vector ScreenPointToVector(Point p)
    {
        return (new Vector(p.X, p.Y) - drawBox.LeftTop) / DrawScale + Camera.LeftTop;
    }

    public Vector ScreenVector2ToWorld(Vector2 v)
    {
        return (new Vector(v.X, v.Y) - drawBox.LeftTop) / DrawScale + Camera.LeftTop;
    }

    public RectangleF WorldBoxToScreen(Box box)
    {
        return ToRectangleF((box.LeftTopOrigin().RoundOriginToFloor() - Camera.LeftTop.RoundToFloor()) * DrawScale + drawBox.LeftTop);
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
    }

    public void ContinueGame()
    {
        paused = false;
    }

    public void NextLevel()
    {
        changeLevel = true;
        levelToChange = (ushort) (currentLevel + 1);
    }

    internal void OnGameOver()
    {
        gameOver = true;
    }

    public void SpawnPlayer()
    {
        Vector spawnPos;
        Vector cameraPos;
        if (romLoaded)
        {
            spawnPos = mmx.CharacterPos;
            cameraPos = mmx.CameraPos;
        }
        else
        {
            spawnPos = new Vector(SCREEN_WIDTH * 0.5f, 0);
            cameraPos = new Vector(SCREEN_WIDTH * 0.5f, 0);
        }

        Camera.LeftTop = cameraPos;

        SpawnX(spawnPos);
        CreateHP();

        Camera.FocusOn = Player;
        CurrentCheckpoint = checkpoints.Count > 0 ? checkpoints[mmx.Point] : null;
    }

    public void ReloadLevel()
    {
        LoadLevel(ROMPath, currentLevel, INITIAL_CHECKPOINT);
    }

    public void ReloadLevel(ushort level, ushort point)
    {
        LoadLevel(ROMPath, level, point);
    }

    public void LoadLevel(string romPath, ushort level, ushort point)
    {

        ROMPath = romPath;

        if (LOAD_ROM)
        {
            mmx = new MMXCoreLoader();
            mmx.LoadNewRom(romPath);
            mmx.Init();

            if (mmx.CheckROM() != 0)
            {
                romLoaded = true;
                mmx.LoadFont();
                mmx.LoadProperties();
            }
        }

        LoadLevel(level, point);

        if (romLoaded)
            mmx.UpdateVRAMCache();
    }

    public void LoadLevel()
    {
        LoadLevel(currentLevel, (ushort) (CurrentCheckpoint != null ? CurrentCheckpoint.Point : 0));
    }

    public void LoadLevel(ushort level, ushort checkpoint = 0)
    {
        paused = false;
        lock (this)
        {
            loadingLevel = true;

            UnloadLevel();

            currentLevel = level;

            camera = Entities.Create<Camera>(new
            {
                Width = SCREEN_WIDTH,
                Height = SCREEN_HEIGHT
            });

            Camera.Spawn();

            if (romLoaded)
            {
                mmx.SetLevel(level, checkpoint);

                mmx.LoadLevel();
                mmx.LoadEventsToEngine();
                mmx.LoadToWorld(false);

                mmx.LoadBackground();
                mmx.LoadToWorld(true);

                CurrentCheckpoint = checkpoints[mmx.Point];
                CameraConstraintsBox = CurrentCheckpoint.Hitbox;

                if (mmx.Type == 0 && mmx.Level == 8)
                    PrecacheSound("Chill Penguin", @"resources\sounds\ost\mmx\12 - Chill Penguin.mp3");
            }
            else
            {
                CameraConstraintsBox = World.ForegroundLayout.BoundingBox;
            }

            World.Tessellate();

            loadingLevel = false;
            respawning = false;

            if (enableSpawningBlackScreen)
            {
                SpawningBlackScreen = true;
                SpawningBlackScreenFrameCounter = 0;
            }
            else
            {
                OnSpawningBlackScreenComplete();
            }

            Camera.FocusOn = null;
            var cameraPos = romLoaded ? mmx.CameraPos : Vector.NULL_VECTOR;
            Camera.LeftTop = cameraPos;
        }
    }

    private void UnloadLevel()
    {
        ClearEntities();

        DisposeResource(World);

        checkpoints.Clear();
        partition.Clear();

        lastLives = Player != null ? Player.Lives : X_INITIAL_LIVES;
        player = null;
        readyHUD = null;
        hp = null;

        long currentMemoryUsage = GC.GetTotalMemory(true);
        long delta = currentMemoryUsage - lastCurrentMemoryUsage;
        Debug.WriteLine("**************************Total memory: {0}({1}{2})", currentMemoryUsage, delta > 0 ? "+" : delta < 0 ? "-" : "", delta);
        lastCurrentMemoryUsage = currentMemoryUsage;
    }

    internal void UpdateScale()
    {
        FixedSingle width = Control.ClientSize.Width;
        FixedSingle height = Control.ClientSize.Height;

        Vector drawOrigin;
        /*if (width / height < SIZE_RATIO)
        {
            drawScale = width / DEFAULT_CLIENT_WIDTH;
            MMXFloat newHeight = drawScale * DEFAULT_CLIENT_HEIGHT;
            drawOrigin = new MMXVector(0, (height - newHeight) * 0.5);
            height = newHeight;
        }
        else
        {
            drawScale = height / DEFAULT_CLIENT_HEIGHT;
            MMXFloat newWidth = drawScale * DEFAULT_CLIENT_WIDTH;
            drawOrigin = new MMXVector((width - newWidth) * 0.5, 0);
            width = newWidth;
        }*/
        drawOrigin = Vector.NULL_VECTOR;

        drawBox = new Box(drawOrigin.X, drawOrigin.Y, width, height);
    }

    private void DrawSlopeMap(Box box, RightTriangle triangle, Color color, float strokeWidth)
    {
        Vector tv1 = triangle.Origin;
        Vector tv2 = triangle.VCathetusOpositeVertex;
        Vector tv3 = triangle.HCathetusOpositeVertex;

        DrawLine(tv2, tv3, strokeWidth, color);

        FixedSingle h = tv1.Y - box.Top;
        FixedSingle H = MAP_SIZE - h;
        if (H > 0)
        {
            if (triangle.HCathetusVector.X < 0)
            {
                DrawLine(tv2, box.LeftBottom, strokeWidth, color);
                DrawLine(tv3, box.RightBottom, strokeWidth, color);
            }
            else
            {
                DrawLine(tv3, box.LeftBottom, strokeWidth, color);
                DrawLine(tv2, box.RightBottom, strokeWidth, color);
            }

            DrawLine(box.LeftBottom, box.RightBottom, strokeWidth, color);
        }
        else
        {
            DrawLine(tv3, tv1, strokeWidth, color);
            DrawLine(tv1, tv2, strokeWidth, color);
        }
    }

    private void DrawHighlightMap(int row, int col, CollisionData collisionData, Color color)
    {
        Box mapBox = GetMapBoundingBox(row, col);
        if (collisionData.IsSolidBlock())
        {
            DrawRectangle(mapBox, 4, color);
        }
        else if (collisionData.IsSlope())
        {
            RightTriangle st = collisionData.MakeSlopeTriangle() + mapBox.LeftTop;
            DrawSlopeMap(mapBox, st, color, 4);
        }
    }

    private void CheckAndDrawTouchingMap(int row, int col, CollisionData collisionData, Box collisionBox, Color color, bool ignoreSlopes = false)
    {
        Box halfCollisionBox1 = (collisionBox.Left, collisionBox.Top, collisionBox.Width * 0.5, collisionBox.Height);
        Box halfCollisionBox2 = (collisionBox.Left + collisionBox.Width * 0.5, collisionBox.Top, collisionBox.Width * 0.5, collisionBox.Height);

        Box mapBox = GetMapBoundingBox(row, col);
        if (collisionData.IsSolidBlock() && CollisionChecker.HasIntersection(mapBox, collisionBox))
        {
            DrawRectangle(mapBox, 4, color);
        }
        else if (!ignoreSlopes && collisionData.IsSlope())
        {
            RightTriangle st = collisionData.MakeSlopeTriangle() + mapBox.LeftTop;
            Vector hv = st.HCathetusVector;
            if (hv.X > 0 && st.HasIntersectionWith(halfCollisionBox2) || hv.X < 0 && st.HasIntersectionWith(halfCollisionBox1))
                DrawSlopeMap(mapBox, st, color, 4);
        }
    }

    private void CheckAndDrawTouchingMaps(Box collisionBox, Color color, bool ignoreSlopes = false)
    {
        Cell start = GetMapCellFromPos(collisionBox.LeftTop);
        Cell end = GetMapCellFromPos(collisionBox.RightBottom);

        int startRow = start.Row;
        int startCol = start.Col;

        if (startRow < 0)
            startRow = 0;

        if (startRow >= World.ForegroundLayout.MapRowCount)
            startRow = World.ForegroundLayout.MapRowCount - 1;

        if (startCol < 0)
            startCol = 0;

        if (startCol >= World.ForegroundLayout.MapColCount)
            startCol = World.ForegroundLayout.MapColCount - 1;

        int endRow = end.Row;
        int endCol = end.Col;

        if (endRow < 0)
            endRow = 0;

        if (endRow >= World.ForegroundLayout.MapRowCount)
            endRow = World.ForegroundLayout.MapRowCount - 1;

        if (endCol < 0)
            endCol = 0;

        if (endCol >= World.ForegroundLayout.MapColCount)
            endCol = World.ForegroundLayout.MapColCount - 1;

        for (int row = startRow; row <= endRow; row++)
        {
            for (int col = startCol; col <= endCol; col++)
            {
                var v = new Vector(col * MAP_SIZE, row * MAP_SIZE);
                Map map = World.ForegroundLayout.GetMapFrom(v);
                if (map != null)
                    CheckAndDrawTouchingMap(row, col, map.CollisionData, collisionBox, color, ignoreSlopes);
            }
        }
    }

    public void DrawLine(Vector from, Vector to, float width, Color color, FadingControl fadingControl = null)
    {
        DrawLine(WorldVectorToScreen(from), WorldVectorToScreen(to), width, color, fadingControl);
    }

    public void DrawLine(Vector2 from, Vector2 to, float width, Color color, FadingControl fadingControl = null)
    {
        from *= 4;
        to *= 4;

        line.Width = width;

        line.Begin();

        Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Linear);
        Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Linear);

        Device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
        Device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);

        if (fadingControl != null)
        {
            Device.PixelShader = PixelShader;
            PixelShader.Function.ConstantTable.SetValue(Device, psFadingLevelHandle, fadingControl.FadingLevel);
            PixelShader.Function.ConstantTable.SetValue(Device, psFadingColorHandle, fadingControl.FadingColor.ToVector4());
        }
        else
        {
            Device.PixelShader = null;
        }

        line.Draw(new Vector2[] { from, to }, color);
        line.End();
    }

    public void DrawRectangle(Box box, float borderWith, Color color, FadingControl fadingControl = null)
    {
        DrawRectangle(WorldBoxToScreen(box), borderWith, color, fadingControl);
    }

    public void DrawRectangle(RectangleF rect, float borderWith, Color color, FadingControl fadingControl = null)
    {
        rect = new RectangleF(rect.X * 4, rect.Y * 4 + 1, rect.Width * 4, rect.Height * 4);

        line.Width = borderWith;

        line.Begin();

        Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Linear);
        Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Linear);

        Device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
        Device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);

        if (fadingControl != null)
        {
            Device.PixelShader = PixelShader;
            PixelShader.Function.ConstantTable.SetValue(Device, psFadingLevelHandle, fadingControl.FadingLevel);
            PixelShader.Function.ConstantTable.SetValue(Device, psFadingColorHandle, fadingControl.FadingColor.ToVector4());
        }
        else
        {
            Device.PixelShader = null;
        }

        line.Draw(new Vector2[] { rect.TopLeft, rect.TopRight, rect.BottomRight, rect.BottomLeft, rect.TopLeft }, color);
        line.End();
    }

    public void FillRectangle(Box box, Color color, FadingControl fadingControl = null)
    {
        FillRectangle(WorldBoxToScreen(box), color, fadingControl);
    }

    public void FillRectangle(RectangleF rect, Color color, FadingControl fadingControl = null)
    {
        float x = 4 * rect.Left;
        float y = 4 * rect.Top;

        var matScaling = Matrix.Scaling(4 * rect.Width, 4 * rect.Height, 1);
        var matTranslation = Matrix.Translation(x, y + 1, 0);
        Matrix matTransform = matScaling * matTranslation;

        sprite.Begin(SpriteFlags.AlphaBlend);

        Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Linear);
        Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Linear);

        Device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
        Device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);

        if (fadingControl != null)
        {
            Device.PixelShader = PixelShader;
            PixelShader.Function.ConstantTable.SetValue(Device, psFadingLevelHandle, fadingControl.FadingLevel);
            PixelShader.Function.ConstantTable.SetValue(Device, psFadingColorHandle, fadingControl.FadingColor.ToVector4());
        }
        else
        {
            Device.PixelShader = null;
        }

        Device.VertexShader = null;

        sprite.Transform = matTransform;

        sprite.Draw(color == Color.Black ? blackPixelTexture : whitePixelTexture, color);
        sprite.End();
    }

    public void DrawText(string text, Font font, RectangleF drawRect, FontDrawFlags drawFlags, Color color, FadingControl fadingControl = null)
    {
        DrawText(text, font, drawRect, drawFlags, Matrix.Identity, color, out _, fadingControl);
    }

    public void DrawText(string text, Font font, RectangleF drawRect, FontDrawFlags drawFlags, Color color, out RawRectangle fontDimension, FadingControl fadingControl = null)
    {
        DrawText(text, font, drawRect, drawFlags, Matrix.Identity, color, out fontDimension, fadingControl);
    }

    public void DrawText(string text, Font font, RectangleF drawRect, FontDrawFlags drawFlags, RawMatrix transform, Color color, FadingControl fadingControl = null)
    {
        DrawText(text, font, drawRect, drawFlags, transform, color, out _, fadingControl);
    }

    public void DrawText(string text, Font font, RectangleF drawRect, FontDrawFlags drawFlags, float offsetX, float offsetY, Color color, FadingControl fadingControl = null)
    {
        RawMatrix transform = Matrix.Translation(offsetX, offsetY, 0);
        DrawText(text, font, drawRect, drawFlags, transform, color, out _, fadingControl);
    }

    public void DrawText(string text, Font font, RectangleF drawRect, FontDrawFlags drawFlags, float offsetX, float offsetY, Color color, out RawRectangle fontDimension, FadingControl fadingControl = null)
    {
        RawMatrix transform = Matrix.Translation(offsetX, offsetY, 0);
        DrawText(text, font, drawRect, drawFlags, transform, color, out fontDimension, fadingControl);
    }

    public void DrawText(string text, Font font, RectangleF drawRect, FontDrawFlags drawFlags, RawMatrix transform, Color color, out RawRectangle fontDimension, FadingControl fadingControl = null)
    {
        sprite.Begin();

        Device.VertexShader = null;
        Device.PixelShader = null;

        Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Linear);
        Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Linear);

        Device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
        Device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);

        if (fadingControl != null)
        {
            Device.PixelShader = PixelShader;
            PixelShader.Function.ConstantTable.SetValue(Device, psFadingLevelHandle, fadingControl.FadingLevel);
            PixelShader.Function.ConstantTable.SetValue(Device, psFadingColorHandle, fadingControl.FadingColor.ToVector4());
        }
        else
        {
            Device.PixelShader = null;
        }

        Device.VertexShader = null;

        sprite.Transform = transform;

        fontDimension = font.MeasureText(sprite, text, drawRect, drawFlags);
        font.DrawText(sprite, text, fontDimension, drawFlags, color);
        sprite.End();
    }

    public void ShowInfoMessage(string message, int time = 3000, int fadingTime = 1000)
    {
        infoMessage = message;
        infoMessageShowingTime = time;
        infoMessageFadingTime = fadingTime;
        infoMessageStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }

    private static void DisposeResource(IDisposable resource)
    {
        try
        {
            resource?.Dispose();
        }
        catch
        {
        }
    }

    private void DisposeDevice()
    {
        foreach (var spriteSheet in spriteSheets)
            DisposeResource(spriteSheet);

        spriteSheets.Clear();
        spriteSheetsByName.Clear();

        foreach (var palette in precachedPalettes)
            DisposeResource(palette);

        precachedPalettes.Clear();
        precachedPalettesByName.Clear();

        World?.OnDisposeDevice();

        DisposeResource(whitePixelTexture);
        DisposeResource(blackPixelTexture);
        DisposeResource(stageTexture);
        DisposeResource(foregroundTilemap);
        DisposeResource(backgroundTilemap);
        DisposeResource(foregroundPalette);
        DisposeResource(PixelShader);
        DisposeResource(PaletteShader);
        DisposeResource(sprite);
        DisposeResource(line);
        DisposeResource(infoFont);
        DisposeResource(coordsTextFont);
        DisposeResource(highlightMapTextFont);
        DisposeResource(VertexBuffer);
        DisposeResource(Device);

        Device = null;
    }

    private bool BeginScene()
    {
        var hr = Device.TestCooperativeLevel();
        if (hr.Success)
        {
            Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, BackgroundColor, 1.0f, 0);
            Device.BeginScene();
            return true;
        }

        if (hr == ResultCode.DeviceLost)
            return false;

        if (hr == ResultCode.DeviceNotReset)
        {
            ResetDevice();
            return false;
        }

        Running = false;
        throw new Exception($"Exiting process due device error: {hr} ({hr.Code})");
    }

    private void DrawTexture(Texture texture, bool linear = false)
    {
        Device.PixelShader = PixelShader;
        Device.VertexShader = null;

        PixelShader.Function.ConstantTable.SetValue(Device, psFadingLevelHandle, FadingControl.FadingLevel);
        PixelShader.Function.ConstantTable.SetValue(Device, psFadingColorHandle, FadingControl.FadingColor.ToVector4());

        var matScaling = Matrix.Scaling(0.25f, 0.25f, 1);
        var matTranslation = Matrix.Translation(-1 * SCREEN_WIDTH * 0.5F, +1 * SCREEN_HEIGHT * 0.5F, 1);
        Matrix matTransform = matScaling * matTranslation;

        Device.SetTransform(TransformState.World, matTransform);
        Device.SetTransform(TransformState.View, Matrix.Identity);
        Device.SetTransform(TransformState.Texture0, Matrix.Identity);
        Device.SetTransform(TransformState.Texture1, Matrix.Identity);

        Device.SetSamplerState(0, SamplerState.MagFilter, linear ? TextureFilter.Linear : TextureFilter.Point);
        Device.SetSamplerState(0, SamplerState.MinFilter, linear ? TextureFilter.Linear : TextureFilter.Point);
        Device.SetSamplerState(0, SamplerState.MipFilter, TextureFilter.None);

        Device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
        Device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);

        Device.SetStreamSource(0, VertexBuffer, 0, VERTEX_SIZE);
        Device.SetTexture(0, texture);
        Device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
    }

    public void Render()
    {
        Render(this);
        renderFrameCounter++;

        long elapsedTicks = clock.ElapsedTicks;
        long remainingTicks = targetElapsedTime - (elapsedTicks - previousElapsedTicks);

        if (remainingTicks > 0)
        {
            long msRemaining = 1000 * remainingTicks / Stopwatch.Frequency;
            if (msRemaining > 0)
                Thread.Sleep((int) msRemaining);
        }

        elapsedTicks = clock.ElapsedTicks;
        previousElapsedTicks = elapsedTicks;

        #region FPS and title update       
        var deltaMilliseconds = 1000 * (elapsedTicks - lastMeasuringFPSElapsedTicks) / Stopwatch.Frequency;
        if (deltaMilliseconds >= 1000)
        {
            lastMeasuringFPSElapsedTicks = elapsedTicks;

            var deltaFrames = renderFrameCounter - lastRenderFrameCounter;
            lastRenderFrameCounter = renderFrameCounter;

            var fps = 1000.0 * deltaFrames / deltaMilliseconds;

            // Update window title with FPS once every second
            Control.Text = $"X# - FPS: {fps:F2} ({(float) deltaMilliseconds / deltaFrames:F2}ms/frame)";
        }
        #endregion
    }

    public void Render(IRenderTarget target)
    {
        bool nextFrame = OnFrame();

        if (Device == null)
            return;

        if (!BeginScene())
            return;

        var orthoLH = Matrix.OrthoLH(SCREEN_WIDTH, SCREEN_HEIGHT, 0.0f, 1.0f);
        Device.SetTransform(TransformState.Projection, orthoLH);
        Device.SetTransform(TransformState.World, Matrix.Identity);
        Device.SetTransform(TransformState.View, Matrix.Identity);

        var backBuffer = Device.GetRenderTarget(0);
        var stageSurface = stageTexture.GetSurfaceLevel(0);

        Device.SetRenderTarget(0, stageSurface);
        Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Transparent, 1.0f, 0);

        if (drawBackground)
        {
            World.RenderBackground(0);
            World.RenderBackground(1);
        }

        if (drawDownLayer)
            World.RenderForeground(0);

        if (drawSprites)
        {
            // Render down layer sprites
            for (int i = sprites[0].Count - 1; i >= 0; i--)
            {
                var sprite = sprites[0][i];
                if (!sprite.Alive || !sprite.Visible)
                    continue;

                if (sprite == Player)
                {
                    if (drawX)
                        sprite.Render(this);
                }
                else
                {
                    sprite.Render(this);
                }
            }
        }

        if (drawUpLayer)
            World.RenderForeground(1);

        if (drawSprites)
        {
            // Render up layer sprites
            for (int i = sprites[1].Count - 1; i >= 0; i--)
            {
                var sprite = sprites[1][i];
                if (!sprite.Alive || !sprite.Visible)
                    continue;

                if (sprite == Player)
                {
                    if (drawX)
                        sprite.Render(this);
                }
                else
                {
                    sprite.Render(this);
                }
            }
        }

        foreach (var layer in huds)
        {
            for (int i = layer.Count - 1; i >= 0; i--)
            {
                var hud = layer[i];
                hud.Render(this);
            }
        }

        Device.SetRenderTarget(0, backBuffer);
        DrawTexture(stageTexture, SAMPLER_STATE_LINEAR);

        if (nextFrame)
        {
            if (Freezing || Player == null || !Player.Freezed)
            {
                if (FreezingSprites)
                {
                    foreach (var entity in aliveEntities)
                    {
                        if (entity != Camera && (entity is not Sprite sprite || freezingSpriteExceptions.Contains(sprite)) && (entity is not HUD hud || freezingSpriteExceptions.Contains(hud)))
                            entity.PostThink();
                    }
                }
                else
                {
                    foreach (var entity in aliveEntities)
                    {
                        if (entity != Camera && entity is not HUD)
                            entity.PostThink();
                    }
                }
            }
            else
            {
                Player?.PostThink();
            }

            Camera?.PostThink();

            foreach (var layer in huds)
            {
                foreach (var hud in layer)
                    hud.PostThink();
            }
        }

        if (respawning || SpawningBlackScreen)
            FillRectangle(Camera != null ? WorldBoxToScreen(Camera.BoundingBox) : RenderRectangle, Color.Black, FadingControl);

        if (drawHitbox || showColliders || showDrawBox || showTriggerBounds)
        {
            resultSet.Clear();
            partition.Query(resultSet, World.ForegroundLayout.BoundingBox, false);
            foreach (Entity entity in resultSet)
            {
                if (entity == Player)
                    continue;

                switch (entity)
                {
                    case Sprite sprite when sprite is not HUD and not SpriteEffect:
                    {
                        if (drawHitbox)
                        {
                            Box hitbox = sprite.Hitbox;
                            var rect = WorldBoxToScreen(hitbox);

                            if (!entity.Alive)
                            {
                                if (entity.Respawnable || entity.SpawnOnNear)
                                    FillRectangle(rect, DEAD_RESPAWNABLE_HITBOX_COLOR);
                                else
                                    FillRectangle(rect, DEAD_HITBOX_COLOR);
                            }
                            else
                            {
                                FillRectangle(rect, HITBOX_COLOR);
                            }

                            bool touchingNull = false;
                            foreach (var touching in entity.TouchingEntities)
                            {
                                if (touching == null)
                                    touchingNull = true;
                                else if (touching is Sprite)
                                    DrawLine(entity.Origin, touching.Origin, 2, HITBOX_BORDER_COLOR);
                            }

                            if (touchingNull)
                                DrawRectangle(rect, 2, HITBOX_BORDER_COLOR);
                        }

                        if (showDrawBox)
                        {
                            Box drawBox = sprite.DrawBox;
                            var rect = WorldBoxToScreen(drawBox);
                            FillRectangle(rect, BOUNDING_BOX_COLOR);
                        }

                        if (showColliders)
                        {
                            SpriteCollider collider = sprite.WorldCollider;
                            if (collider != null)
                            {
                                FillRectangle(WorldBoxToScreen(collider.DownCollider), DOWN_COLLIDER_COLOR);
                                FillRectangle(WorldBoxToScreen(collider.UpCollider), UP_COLLIDER_COLOR);
                                FillRectangle(WorldBoxToScreen(collider.LeftCollider), LEFT_COLLIDER_COLOR);
                                FillRectangle(WorldBoxToScreen(collider.RightCollider), RIGHT_COLLIDER_COLOR);
                            }
                        }

                        break;
                    }

                    case BaseTrigger trigger when showTriggerBounds:
                    {
                        var rect = WorldBoxToScreen(trigger.Hitbox);

                        if (trigger is not ChangeDynamicPropertyTrigger)
                        {
                            if (Player != null && trigger.IsTouching(Player))
                                FillRectangle(rect, TRIGGER_BOX_COLOR);
                        }

                        switch (trigger)
                        {
                            case CheckpointTriggerOnce:
                                DrawRectangle(rect, 4, CHECKPOINT_TRIGGER_BORDER_BOX_COLOR);
                                break;

                            case CameraLockTrigger cameraLockTrigger when showTriggerCameraLockDirection:
                            {
                                DrawRectangle(rect, 4, TRIGGER_BORDER_BOX_COLOR);

                                Vector constraintOrigin = cameraLockTrigger.ConstraintOrigin;
                                foreach (var constraint in cameraLockTrigger.Constraints)
                                    DrawLine(WorldVectorToScreen(constraintOrigin), WorldVectorToScreen(constraintOrigin + constraint), 4, CAMERA_LOCK_COLOR);

                                break;
                            }

                            case ChangeDynamicPropertyTrigger changeDynamicPropertyTrigger:
                            {
                                var box = changeDynamicPropertyTrigger.Hitbox;
                                var origin = box.Origin;
                                var mins = box.Mins;
                                var maxs = box.Maxs;
                                switch (changeDynamicPropertyTrigger.Orientation)
                                {
                                    case SplitterTriggerOrientation.HORIZONTAL:
                                        DrawLine(new Vector(origin.X + mins.X, origin.Y), new Vector(origin.X + maxs.X, origin.Y), 4, Color.Purple);
                                        break;

                                    case SplitterTriggerOrientation.VERTICAL:
                                        DrawLine(new Vector(origin.X, origin.Y + mins.Y), new Vector(origin.X, origin.Y + maxs.Y), 4, Color.Purple);
                                        break;

                                }

                                break;
                            }

                            default:
                                DrawRectangle(rect, 4, TRIGGER_BORDER_BOX_COLOR);
                                break;
                        }

                        break;
                    }
                }
            }
        }

        if (Player != null)
        {
            if (drawTouchingMapBounds)
            {
                var collider = Player.WorldCollider;

                CheckAndDrawTouchingMaps(collider.LeftCollider + STEP_LEFT_VECTOR, LEFT_COLLIDER_COLOR, true);
                CheckAndDrawTouchingMaps(collider.UpCollider + STEP_UP_VECTOR, UP_COLLIDER_COLOR);
                CheckAndDrawTouchingMaps(collider.RightCollider + STEP_RIGHT_VECTOR, RIGHT_COLLIDER_COLOR, true);
                CheckAndDrawTouchingMaps(collider.DownCollider + STEP_DOWN_VECTOR, DOWN_COLLIDER_COLOR);
            }

            if (drawHitbox)
            {
                Box hitbox = Player.Hitbox;
                var rect = WorldBoxToScreen(hitbox);
                FillRectangle(rect, HITBOX_COLOR);

                bool touchingNull = false;
                foreach (var touching in Player.TouchingEntities)
                {
                    if (touching == null)
                        touchingNull = true;
                    else if (touching is Sprite)
                        DrawLine(Player.Origin, touching.Origin, 2, HITBOX_BORDER_COLOR);
                }

                if (touchingNull)
                    DrawRectangle(rect, 2, HITBOX_BORDER_COLOR);
            }

            if (showDrawBox)
            {
                Box drawBox = Player.DrawBox;
                var rect = WorldBoxToScreen(drawBox);
                FillRectangle(rect, BOUNDING_BOX_COLOR);
            }

            if (showColliders)
            {
                SpriteCollider collider = Player.WorldCollider;
                if (collider != null)
                {
                    FillRectangle(WorldBoxToScreen(collider.DownCollider), DOWN_COLLIDER_COLOR);
                    FillRectangle(WorldBoxToScreen(collider.UpCollider), UP_COLLIDER_COLOR);
                    FillRectangle(WorldBoxToScreen(collider.LeftCollider), LEFT_COLLIDER_COLOR);
                    FillRectangle(WorldBoxToScreen(collider.RightCollider), RIGHT_COLLIDER_COLOR);
                }
            }
        }

        if (drawHighlightedPointingTiles)
        {
            System.Drawing.Point cursorPos = Control.PointToClient(Cursor.Position);
            Vector v = ScreenPointToVector(cursorPos.X / 4, cursorPos.Y / 4);
            DrawText($"Mouse Pos: X: {v.X} Y: {v.Y}", highlightMapTextFont, new RectangleF(0, 0, 400, 50), FontDrawFlags.Left | FontDrawFlags.Top, TOUCHING_MAP_COLOR);

            Scene scene = World.ForegroundLayout.GetSceneFrom(v);
            if (scene != null)
            {
                Cell sceneCell = GetSceneCellFromPos(v);
                Box sceneBox = GetSceneBoundingBox(sceneCell);
                DrawRectangle(WorldBoxToScreen(sceneBox), 4, TOUCHING_MAP_COLOR);
                DrawText($"Scene: ID: {scene.ID} Row: {sceneCell.Row} Col: {sceneCell.Col}", highlightMapTextFont, new RectangleF(0, 50, 400, 50), FontDrawFlags.Left | FontDrawFlags.Top, TOUCHING_MAP_COLOR);

                Block block = World.ForegroundLayout.GetBlockFrom(v);
                if (block != null)
                {
                    Cell blockCell = GetBlockCellFromPos(v);
                    Box blockBox = GetBlockBoundingBox(blockCell);
                    DrawRectangle(WorldBoxToScreen(blockBox), 4, TOUCHING_MAP_COLOR);
                    DrawText($"Block: ID: {block.ID} Row: {blockCell.Row} Col: {blockCell.Col}", highlightMapTextFont, new RectangleF(0, 100, 400, 50), FontDrawFlags.Left | FontDrawFlags.Top, TOUCHING_MAP_COLOR);

                    Map map = World.ForegroundLayout.GetMapFrom(v);
                    if (map != null)
                    {
                        Cell mapCell = GetMapCellFromPos(v);
                        Box mapBox = GetMapBoundingBox(mapCell);
                        DrawRectangle(WorldBoxToScreen(mapBox), 4, TOUCHING_MAP_COLOR);
                        DrawText($"Map: ID: {map.ID} Row: {mapCell.Row} Col: {mapCell.Col}", highlightMapTextFont, new RectangleF(0, 150, 400, 50), FontDrawFlags.Left | FontDrawFlags.Top, TOUCHING_MAP_COLOR);

                        Tile tile = World.ForegroundLayout.GetTileFrom(v);
                        if (tile != null)
                        {
                            Cell tileCell = GetTileCellFromPos(v);
                            Box tileBox = GetTileBoundingBox(tileCell);
                            DrawRectangle(WorldBoxToScreen(tileBox), 4, TOUCHING_MAP_COLOR);
                            DrawText($"Tile: ID: {tile.ID} Row: {tileCell.Row} Col: {tileCell.Col}", highlightMapTextFont, new RectangleF(0, 200, 400, 50), FontDrawFlags.Left | FontDrawFlags.Top, TOUCHING_MAP_COLOR);
                        }
                    }
                }
            }
        }

        RectangleF drawRect = RenderRectangle;

        if (drawPlayerOriginAxis && Player != null)
        {
            Vector2 v = WorldVectorToScreen(Player.Origin);

            line.Width = 2;

            line.Begin();
            line.Draw(new Vector2[] { 4 * new Vector2(v.X, v.Y - SCREEN_HEIGHT), 4 * new Vector2(v.X, v.Y + SCREEN_HEIGHT) }, Color.Blue);
            line.Draw(new Vector2[] { 4 * new Vector2(v.X - SCREEN_WIDTH, v.Y), 4 * new Vector2(v.X + SCREEN_WIDTH, v.Y) }, Color.Blue);
            line.End();
        }

        if (showCheckpointBounds && CurrentCheckpoint != null)
            DrawRectangle(WorldBoxToScreen(CurrentCheckpoint.Hitbox), 4, Color.Yellow);

        if (showInfoText && Player != null)
        {
            string text = $"Checkpoint: {(CurrentCheckpoint != null ? currentCheckpoint.TargetIndex.ToString() : "none")}";
            DrawText(text, infoFont, drawRect, FontDrawFlags.Bottom | FontDrawFlags.Left, Color.White, out RawRectangle fontDimension);

            text = $"Camera: CX: {(float) Camera.Left * 256}({(float) (Camera.Left - lastCameraLeftTop.X) * 256}) CY: {(float) Camera.Top * 256}({(float) (Camera.Top - lastCameraLeftTop.Y) * 256})";
            DrawText(text, infoFont, drawRect, FontDrawFlags.Bottom | FontDrawFlags.Left, 0, fontDimension.Top - fontDimension.Bottom, Color.White, out fontDimension);

            text = $"Player: X: {(float) (Player.Origin.X * 256)}({(float) ((Player.Origin.X - lastPlayerOrigin.X) * 256)}) Y: {(float) (Player.Origin.Y * 256)}({(float) ((Player.Origin.Y - lastPlayerOrigin.Y) * 256)}) VX: {(float) (Player.Velocity.X * 256)}({(float) (Player.Velocity.X - lastPlayerVelocity.X) * 256}) VY: {(float) (Player.Velocity.Y * -256)}({(float) ((Player.Velocity.Y - lastPlayerVelocity.Y) * -256)}) Gravity: {(float) (Player.GetGravity() * 256)}";
            DrawText(text, infoFont, drawRect, FontDrawFlags.Bottom | FontDrawFlags.Left, 0, 2 * (fontDimension.Top - fontDimension.Bottom), Color.White, out fontDimension);
        }

        if (infoMessage != null)
        {
            long deltaTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() - infoMessageStartTime;
            if (infoMessageShowingTime > 0 && deltaTime >= infoMessageShowingTime)
            {
                infoMessage = null;
            }
            else
            {
                if (infoMessageFadingTime > 0 && deltaTime >= infoMessageFadingTime)
                {
                    var color = new Color(0, 0, 0, (float) (deltaTime - infoMessageFadingTime) / infoMessageFadingTime);
                    infoMessageFadingControl.FadingLevel = color.ToVector4();
                    infoMessageFadingControl.FadingColor = Color.Transparent;
                }
                else
                {
                    infoMessageFadingControl.FadingLevel = Vector4.Zero;
                }

                DrawText(infoMessage, infoFont, drawRect, FontDrawFlags.Top | FontDrawFlags.Left, Color.White, infoMessageFadingControl);
            }
        }

        try
        {
            Device.EndScene();
            Device.Present();
        }
        catch (SharpDXException)
        {
        }
    }

    public void Deserialize(BinaryReader reader)
    {
        using var serializer = new EngineBinarySerializer(reader);

        if (LOAD_ROM)
        {
            string romPath = serializer.ReadString(false);
            ushort level = serializer.ReadUShort();
            ushort point = serializer.ReadUShort();
            int objLoad = serializer.ReadInt();
            int tileLoad = serializer.ReadInt();
            int palLoad = serializer.ReadInt();

            if (ROMPath != romPath)
            {
                ROMPath = romPath;
                currentLevel = level;

                if (LOAD_ROM)
                {
                    World.Clear();

                    DisposeResource(mmx);

                    mmx = new MMXCoreLoader();
                    mmx.LoadNewRom(romPath);
                    mmx.Init();

                    if (mmx.CheckROM() != 0)
                    {
                        romLoaded = true;
                        mmx.LoadFont();
                        mmx.LoadProperties();
                    }

                    mmx.SetLevel(level, point, objLoad, tileLoad, palLoad);

                    mmx.LoadLevel();
                    mmx.LoadEventsToEngine();
                    mmx.LoadToWorld(false);

                    mmx.LoadBackground();
                    mmx.LoadToWorld(true);

                    World.Tessellate();

                    if (romLoaded)
                        mmx.UpdateVRAMCache();
                }
            }
            else if (level != currentLevel)
            {
                currentLevel = level;

                World.Clear();

                mmx.SetLevel(level, point, objLoad, tileLoad, palLoad);

                mmx.LoadLevel();
                mmx.LoadEventsToEngine();
                mmx.LoadToWorld(false);

                mmx.LoadBackground();
                mmx.LoadToWorld(true);

                World.Tessellate();

                if (romLoaded)
                    mmx.UpdateVRAMCache();

                World.Tessellate();
            }
            else
            {
                mmx.SetLevel(level, point, objLoad, tileLoad, palLoad);
                mmx.LoadTilesAndPalettes();
                mmx.LoadPalette(false);
                mmx.LoadPalette(true);
                mmx.RefreshMapCache(false);
                mmx.RefreshMapCache(true);
            }
        }

        Entities.Deserialize(serializer);

        RNG.Deserialize(serializer);

        lastLives = serializer.ReadInt();
        respawning = serializer.ReadBool();

        lastPlayerOrigin = serializer.ReadVector();
        lastPlayerVelocity = serializer.ReadVector();
        lastCameraLeftTop = serializer.ReadVector();

        var entries = new List<PrecachedSound>();
        var entriesByName = new HashSet<string>();
        var entriesByFileName = new HashSet<string>();

        int soundStreamCount = serializer.ReadInt();
        for (int i = 0; i < soundStreamCount; i++)
        {
            int nameCount = serializer.ReadInt();
            var names = new string[nameCount];

            for (int j = 0; j < nameCount; j++)
            {
                string name = serializer.ReadString(false);
                names[j] = name;
            }

            string path = serializer.ReadString(false);

            var entry = new PrecachedSound(path, names);
            entries.Add(entry);

            foreach (var name in names)
                entriesByName.Add(name);

            entriesByFileName.Add(path);
        }

        foreach (var (names, path, format) in entries)
            foreach (var name in names)
                PrecacheSound(name, path);

        // TODO : The folowwing serialization & deserialization code for palettes and sprite sheets may crash the program.
        // Please fix it in future to make full serialization for palettes and sprite sheets as accurate as the same method used for the sounds above. 

        var precachedSoundPaths = new List<string>(precachedSoundsByFileName.Keys);
        foreach (var path in precachedSoundPaths)
        {
            int index = precachedSoundsByFileName[path];
            var sound = precachedSounds[index];
            if (!entriesByFileName.Contains(path))
            {
                precachedSoundsByFileName.Remove(path);

                foreach (var name in sound.Names)
                    precachedSoundsByName.Remove(name);

                precachedSounds[index] = null;
                sound.Dispose();
            }
        }

        var precachedSoundNames = new List<string>(precachedSoundsByName.Keys);
        foreach (var name in precachedSoundNames)
        {
            int index = precachedSoundsByName[name];
            var sound = precachedSounds[index];
            if (!entriesByName.Contains(name))
            {
                precachedSoundsByName.Remove(name);
                sound.RemoveName(name);
            }
        }

        foreach (var channel in soundChannels)
            channel.Deserialize(serializer);

        int paletteCount = serializer.ReadInt();
        var paletteNames = new HashSet<string>();
        for (int i = 0; i < paletteCount; i++)
        {
            string name = serializer.ReadString(false);
            paletteNames.Add(name);
        }

        var precachedPaletteNames = new List<string>(precachedPalettesByName.Keys);
        foreach (var name in precachedPaletteNames)
        {
            int index = precachedPalettesByName[name];
            var palette = precachedPalettes[index];
            if (!paletteNames.Contains(name))
            {
                precachedPalettesByName.Remove(name);
                precachedPalettes[index] = null;
                palette.Dispose();
            }
        }

        int spriteSheetCount = serializer.ReadInt();
        var spriteSheetNames = new HashSet<string>();
        for (int i = 0; i < spriteSheetCount; i++)
        {
            string name = serializer.ReadString(false);
            spriteSheetNames.Add(name);
        }

        var precachedSpriteSheetNames = new List<string>(spriteSheetsByName.Keys);
        foreach (var name in precachedSpriteSheetNames)
        {
            int index = spriteSheetsByName[name];
            var spriteSheet = spriteSheets[index];
            if (!spriteSheetNames.Contains(name))
            {
                spriteSheetsByName.Remove(name);
                spriteSheets[index] = null;
                spriteSheet.Dispose();
            }
        }

        var precacheActions = new Dictionary<string, PrecacheAction>();
        var actionsToCall = new List<PrecacheAction>();
        int precacheActionCount = serializer.ReadInt();
        for (int i = 0; i < precacheActionCount; i++)
        {
            string typeName = serializer.ReadString(false);
            var action = new PrecacheAction(serializer);
            precacheActions.Add(typeName, action);
            if (!this.precacheActions.ContainsKey(typeName))
            {
                this.precacheActions.Add(typeName, action);
                action.Reset(false);
                actionsToCall.Add(action);
            }
        }

        var typeNames = new List<string>(this.precacheActions.Keys);
        foreach (var typeName in typeNames)
        {
            if (!precacheActions.ContainsKey(typeName))
                this.precacheActions.Remove(typeName);
        }

        foreach (var action in actionsToCall)
            action.Call();

        partition.Deserialize(serializer);

        checkpoints.Clear();
        int checkpointCount = serializer.ReadInt();
        for (int i = 0; i < checkpointCount; i++)
        {
            var checkpoint = serializer.ReadEntityReference<Checkpoint>();
            checkpoints.Add(checkpoint);
        }

        int checkPointIndex = serializer.ReadInt();
        currentCheckpoint = checkPointIndex != -1 ? checkpoints[checkPointIndex] : null;

        cameraConstraints.Clear();
        int constraintCount = serializer.ReadInt();
        for (int i = 0; i < constraintCount; i++)
        {
            var constraint = serializer.ReadVector();
            cameraConstraints.Add(constraint);
        }

        HealthCapacity = serializer.ReadFixedSingle();
        CameraConstraintsOrigin = serializer.ReadVector();
        CameraConstraintsBox = serializer.ReadBox();

        gameOver = serializer.ReadBool();
        paused = serializer.ReadBool();

        DrawScale = serializer.ReadFixedSingle();

        BackgroundColor = Color.FromAbgr(serializer.ReadInt());
        Running = serializer.ReadBool();
        FrameCounter = serializer.ReadLong();

        aliveEntities.Deserialize(serializer);
        spawnedEntities.Deserialize(serializer);
        removedEntities.Deserialize(serializer);

        freezingSpriteExceptions.Deserialize(serializer);

        for (int i = 0; i < sprites.Length; i++)
            sprites[i] = (List<Sprite>) serializer.ReadList<Sprite>(false);

        for (int i = 0; i < huds.Length; i++)
            huds[i] = (List<HUD>) serializer.ReadList<HUD>(false);

        camera = serializer.ReadEntityReference<Camera>();
        player = serializer.ReadEntityReference<Player>();
        hp = serializer.ReadEntityReference<PlayerHealthHUD>();
        readyHUD = serializer.ReadEntityReference<ReadyHUD>();
        boss = serializer.ReadEntityReference<Boss>();

        FadingControl.Deserialize(serializer);
        World.ForegroundLayout.FadingControl.Deserialize(serializer);
        World.BackgroundLayout.FadingControl.Deserialize(serializer);

        FadingOSTLevel = serializer.ReadFloat();
        FadingOSTInitialVolume = serializer.ReadFloat();
        FadingOSTVolume = serializer.ReadFloat();
        FadingOST = serializer.ReadBool();
        FadeInOST = serializer.ReadBool();
        FadingOSTFrames = serializer.ReadLong();
        FadingOSTTick = serializer.ReadLong();
        serializer.DeserializeProperty(nameof(OnFadingOSTComplete), this);
        SpawningBlackScreen = serializer.ReadBool();
        SpawningBlackScreenFrameCounter = serializer.ReadInt();
        DyingEffectActive = serializer.ReadBool();
        DyingEffectFrameCounter = serializer.ReadInt();
        Freezing = serializer.ReadBool();
        FreezingFrames = serializer.ReadInt();
        serializer.DeserializeProperty(nameof(OnFreezeComplete), this);
        FreezeFrameCounter = serializer.ReadInt();
        FreezingFrameCounter = serializer.ReadInt();
        FreezingSprites = serializer.ReadBool();
        FreezingSpritesFrames = serializer.ReadInt();
        FreezingSpritesFrameCounter = serializer.ReadInt();
        serializer.DeserializeProperty(nameof(OnFreezeSpritesComplete), this);
        serializer.DeserializeProperty(nameof(DelayedAction), this);
        DelayedActionFrames = serializer.ReadInt();
        DelayedActionFrameCounter = serializer.ReadInt();
        BossBattle = serializer.ReadBool();
        BossIntroducing = serializer.ReadBool();

        serializer.Resolve();
    }

    public void Serialize(BinaryWriter writer)
    {
        using var serializer = new EngineBinarySerializer(writer);

        if (LOAD_ROM)
        {
            serializer.WriteString(ROMPath, false);
            serializer.WriteUShort(currentLevel);
            serializer.WriteUShort(mmx.Point);
            serializer.WriteInt(mmx.ObjLoad);
            serializer.WriteInt(mmx.TileLoad);
            serializer.WriteInt(mmx.PalLoad);
        }

        Entities.Serialize(serializer);

        RNG.Serialize(serializer);

        serializer.WriteInt(lastLives);
        serializer.WriteBool(respawning);

        serializer.WriteVector(lastPlayerOrigin);
        serializer.WriteVector(lastPlayerVelocity);
        serializer.WriteVector(lastCameraLeftTop);

        int precachedSoundCount = 0;
        foreach (var sound in precachedSounds)
        {
            if (sound != null)
                precachedSoundCount++;
        }

        serializer.WriteInt(precachedSoundCount);
        foreach (var sound in precachedSounds)
        {
            if (sound != null)
            {
                serializer.WriteInt(sound.Names.Count);
                foreach (var name in sound.Names)
                    serializer.WriteString(name, false);

                serializer.WriteString(sound.Path, false);
            }
        }

        foreach (var channel in soundChannels)
            channel.Serialize(serializer);

        serializer.WriteInt(precachedPalettesByName.Count);
        foreach (var kv in precachedPalettesByName)
        {
            string name = kv.Key;
            serializer.WriteString(name, false);
        }

        serializer.WriteInt(spriteSheetsByName.Count);
        foreach (var kv in spriteSheetsByName)
        {
            string name = kv.Key;
            serializer.WriteString(name, false);
        }

        serializer.WriteInt(precacheActions.Count);
        foreach (var kv in precacheActions)
        {
            var typeName = kv.Key;
            var action = kv.Value;

            serializer.WriteString(typeName, false);
            action.Serialize(serializer);
        }

        partition.Serialize(serializer);

        serializer.WriteInt(checkpoints.Count);
        foreach (var checkpoint in checkpoints)
            serializer.WriteEntityReference(checkpoint);

        serializer.WriteInt(CurrentCheckpoint != null ? CurrentCheckpoint.Point : -1);

        serializer.WriteInt(cameraConstraints.Count);
        foreach (var constraint in cameraConstraints)
            serializer.WriteVector(constraint);

        serializer.WriteFixedSingle(HealthCapacity);
        serializer.WriteVector(CameraConstraintsOrigin);
        serializer.WriteBox(CameraConstraintsBox);

        serializer.WriteBool(gameOver);
        serializer.WriteBool(paused);

        serializer.WriteFixedSingle(DrawScale);

        serializer.WriteInt(BackgroundColor.ToAbgr());
        serializer.WriteBool(Running);
        serializer.WriteLong(FrameCounter);

        aliveEntities.Serialize(serializer);
        spawnedEntities.Serialize(serializer);
        removedEntities.Serialize(serializer);

        freezingSpriteExceptions.Serialize(serializer);

        foreach (var spriteLayer in sprites)
            serializer.WriteList<Sprite>(spriteLayer, false);

        foreach (var hudLayer in huds)
            serializer.WriteList<HUD>(hudLayer, false);

        serializer.WriteEntityReference(camera);
        serializer.WriteEntityReference(player);
        serializer.WriteEntityReference(hp);
        serializer.WriteEntityReference(readyHUD);
        serializer.WriteEntityReference(boss);

        FadingControl.Serialize(serializer);
        World.ForegroundLayout.FadingControl.Serialize(serializer);
        World.BackgroundLayout.FadingControl.Serialize(serializer);

        serializer.WriteFloat(FadingOSTLevel);
        serializer.WriteFloat(FadingOSTInitialVolume);
        serializer.WriteFloat(FadingOSTVolume);
        serializer.WriteBool(FadingOST);
        serializer.WriteBool(FadeInOST);
        serializer.WriteLong(FadingOSTFrames);
        serializer.WriteLong(FadingOSTTick);
        serializer.WriteDelegate(OnFadingOSTComplete);
        serializer.WriteBool(SpawningBlackScreen);
        serializer.WriteInt(SpawningBlackScreenFrameCounter);
        serializer.WriteBool(DyingEffectActive);
        serializer.WriteInt(DyingEffectFrameCounter);
        serializer.WriteBool(Freezing);
        serializer.WriteInt(FreezingFrames);
        serializer.WriteDelegate(OnFreezeComplete);
        serializer.WriteInt(FreezeFrameCounter);
        serializer.WriteInt(FreezingFrameCounter);
        serializer.WriteBool(FreezingSprites);
        serializer.WriteInt(FreezingSpritesFrames);
        serializer.WriteInt(FreezingSpritesFrameCounter);
        serializer.WriteDelegate(OnFreezeSpritesComplete);
        serializer.WriteDelegate(DelayedAction);
        serializer.WriteInt(DelayedActionFrames);
        serializer.WriteInt(DelayedActionFrameCounter);
        serializer.WriteBool(BossBattle);
        serializer.WriteBool(BossIntroducing);

        writer.Flush();
    }

    public void LoadState(int slot = -1)
    {
        if (slot == -1)
            slot = CurrentSaveSlot;

        try
        {
            string fileName = @"sstates\state." + slot;
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            using var stream = new FileStream(fileName, FileMode.Open);
            using var reader = new BinaryReader(stream);
            Deserialize(reader);

            ShowInfoMessage($"State #{slot} loaded.");
        }
        catch (IOException e)
        {
            ShowInfoMessage($"Error on loading state #{slot}: {e.Message}");
        }
    }

    public void SaveState(int slot = -1)
    {
        if (slot == -1)
            slot = CurrentSaveSlot;

        try
        {
            string fileName = @"sstates\state." + slot;
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            using var stream = new FileStream(fileName, FileMode.Create);
            using var writer = new BinaryWriter(stream);
            Serialize(writer);

            ShowInfoMessage($"State #{slot} saved.");
        }
        catch (IOException e)
        {
            ShowInfoMessage($"Error on saving state #{slot}: {e.Message}");
        }
    }

    public EntityReference<ChangeDynamicPropertyTrigger> AddChangeDynamicPropertyTrigger(Vector origin, DynamicProperty prop, int forward, int backward, SplitterTriggerOrientation orientation)
    {
        ChangeDynamicPropertyTrigger trigger = Entities.Create<ChangeDynamicPropertyTrigger>(new
        {
            Origin = origin,
            Hitbox = (origin, (-SCREEN_WIDTH * 0.5, -SCREEN_HEIGHT * 0.5), (SCREEN_WIDTH * 0.5, SCREEN_HEIGHT * 0.5)),
            Property = prop,
            Forward = forward,
            Backward = backward,
            Orientation = orientation
        });

        trigger.Spawn();
        return Entities.GetReferenceTo(trigger);
    }

    public EntityReference<Checkpoint> AddCheckpoint(ushort index, Box boundingBox, Vector characterPos, Vector cameraPos, Vector backgroundPos, Vector forceBackground, uint scroll)
    {
        Checkpoint checkpoint = Entities.Create<Checkpoint>(new
        {
            Point = index,
            boundingBox.Origin,
            Hitbox = boundingBox,
            CharacterPos = characterPos,
            CameraPos = cameraPos,
            BackgroundPos = backgroundPos,
            ForceBackground = forceBackground,
            Scroll = scroll
        });

        checkpoints.Add(checkpoint);
        checkpoint.Spawn();
        return Entities.GetReferenceTo(checkpoint);
    }

    public EntityReference<CheckpointTriggerOnce> AddCheckpointTrigger(ushort index, Vector origin)
    {
        CheckpointTriggerOnce trigger = Entities.Create<CheckpointTriggerOnce>(new
        {
            Origin = origin,
            Hitbox = (origin, (0, -SCREEN_HEIGHT * 0.5), (SCREEN_WIDTH * 0.5, SCREEN_HEIGHT * 0.5)),
            Checkpoint = (Checkpoint) checkpoints[index]
        });

        trigger.Spawn();
        return Entities.GetReferenceTo(trigger);
    }

    public Sprite AddObjectEvent(ushort id, ushort subid, Vector origin)
    {
        return !ENABLE_ENEMIES && id != 0x2
            ? null
            : id switch
            {
                0x02 when mmx.Type == 0 && mmx.Level == 8 => AddPenguin(origin),
                0x04 when mmx.Type == 0 => AddFlammingle(subid, origin),
                0x09 when mmx.Type == 1 => AddScriver(subid, origin),
                0x0B when mmx.Type == 0 => AddAxeMax(subid, origin),
                0x15 when mmx.Type == 0 => AddSpiky(subid, origin),
                0x16 when mmx.Type == 0 => AddHoverPlatform(subid, origin),
                0x17 when mmx.Type == 0 => AddTurnCannon(subid, origin),
                0x19 when mmx.Type == 0 => AddBombBeen(subid, origin),
                0x2C when mmx.Type == 1 => AddProbe8201U(subid, origin),
                0x2D when mmx.Type == 0 => AddBattonBoneG(subid, origin),
                0x2F => AddArmorSoldier(subid, origin),
                0x36 when mmx.Type == 0 => AddJamminger(subid, origin),
                0x3A when mmx.Type == 0 => AddTombot(subid, origin),
                0x4D => AddCapsule(subid, origin),
                0x50 when mmx.Type == 1 => AddBattonBoneG(subid, origin),
                0x51 when mmx.Type == 0 => AddRayBit(subid, origin),
                0x53 when mmx.Type == 0 => AddSnowShooter(subid, origin),
                0x54 when mmx.Type == 0 => AddSnowball(subid, origin),
                0x57 when mmx.Type == 0 => AddIgloo(subid, origin),
                _ => mmx.Type == 0 && mmx.Level == 8 ? AddScriver(subid, origin) : null
            };
    }

    internal EntityReference<Penguin> AddPenguin(Vector origin)
    {
        Penguin penguin = Entities.Create<Penguin>(new
        {
            Origin = origin
        });

        penguin.BossDefeatedEvent += OnBossDefeated;
        Boss = penguin;

        return Entities.GetReferenceTo(penguin);
    }

    private void OnBossDefeated(Boss boss, Player killer)
    {
        killer.StopMoving();
        killer.Invincible = true;
        killer.InputLocked = true;
        killer.FaceToScreenCenter();
        PlayVictorySound();
        Engine.DoDelayedAction((int) (6.5 * 60), () => killer.StartTeleporting(true));
    }

    public EntityReference<CameraLockTrigger> AddCameraLockTrigger(Box boundingBox, IEnumerable<Vector> extensions)
    {
        CameraLockTrigger trigger = Entities.Create<CameraLockTrigger>(new
        {
            boundingBox.Origin,
            Hitbox = boundingBox
        });

        trigger.AddConstraints(extensions);

        trigger.Spawn();
        return trigger;
    }

    internal void UpdateCameraConstraintsBox()
    {
        Box boundingBox = CameraConstraintsBox;
        FixedSingle minX = boundingBox.Left;
        FixedSingle minY = boundingBox.Top;
        FixedSingle maxX = boundingBox.Right;
        FixedSingle maxY = boundingBox.Bottom;

        foreach (Vector constraint in cameraConstraints)
        {
            if (constraint.Y == 0)
            {
                if (constraint.X < 0)
                    minX = CameraConstraintsOrigin.X + constraint.X;
                else
                    maxX = CameraConstraintsOrigin.X + constraint.X;
            }
            else if (constraint.X == 0)
            {
                if (constraint.Y < 0)
                    minY = CameraConstraintsOrigin.Y + constraint.Y;
                else
                    maxY = CameraConstraintsOrigin.Y + constraint.Y;
            }
        }

        CameraConstraintsBox = new Box(minX, minY, maxX - minX, maxY - minY);
    }

    public void SetCameraConstraints(Vector origin, IEnumerable<Vector> extensions)
    {
        CameraConstraintsOrigin = origin;

        cameraConstraints.Clear();
        cameraConstraints.AddRange(extensions);

        UpdateCameraConstraintsBox();
    }

    public void SetCameraConstraints(Vector origin, params Vector[] extensions)
    {
        SetCameraConstraints(origin, (IEnumerable<Vector>) extensions);
    }

    public void AddConstraint(Vector constraint)
    {
        cameraConstraints.Add(constraint);
    }

    public Vector GetConstraint(int index)
    {
        return cameraConstraints[index];
    }

    public bool ContainsConstraint(Vector constraint)
    {
        return cameraConstraints.Contains(constraint);
    }

    public void ClearConstraints()
    {
        cameraConstraints.Clear();
    }

    internal EntityReference<BusterLemon> ShootLemon(Player shooter, Vector origin, bool dashLemon)
    {
        BusterLemon lemon = Entities.Create<BusterLemon>(new
        {
            Shooter = shooter,
            Origin = origin,
            DashLemon = dashLemon
        });

        lemon.Spawn();
        return Entities.GetReferenceTo(lemon);
    }

    internal EntityReference<BusterSemiCharged> ShootSemiCharged(Player shooter, Vector origin)
    {
        BusterSemiCharged semiCharged = Entities.Create<BusterSemiCharged>(new
        {
            Shooter = shooter,
            Origin = origin
        });

        semiCharged.Spawn();
        return Entities.GetReferenceTo(semiCharged);
    }

    internal EntityReference<BusterCharged> ShootCharged(Player shooter, Vector origin)
    {
        BusterCharged charged = Entities.Create<BusterCharged>(new
        {
            Shooter = shooter,
            Origin = origin
        });

        charged.Spawn();
        return Entities.GetReferenceTo(charged);
    }

    internal EntityReference<ChargingEffect> StartChargingEffect(Player player)
    {
        ChargingEffect effect = Entities.Create<ChargingEffect>(new
        {
            Charger = player
        });

        effect.Spawn();
        return Entities.GetReferenceTo(effect);
    }

    private static Vector GetDashSparkOrigin(Player player)
    {
        return player.Direction switch
        {
            Direction.LEFT => player.Hitbox.LeftTop + (23 - 15, 20),
            Direction.RIGHT => player.Hitbox.RightTop + (-23 + 15, 20),
            _ => Vector.NULL_VECTOR,
        };
    }

    internal EntityReference<DashSparkEffect> StartDashSparkEffect(Player player)
    {
        DashSparkEffect effect = Entities.Create<DashSparkEffect>(new
        {
            Parent = player,
            Origin = GetDashSparkOrigin(player),
            player.Direction
        });

        effect.Spawn();
        return Entities.GetReferenceTo(effect);
    }

    public EntityReference<Smoke> SpawnSmoke(Vector origin)
    {
        return SpawnSmoke(origin, Vector.NULL_VECTOR);
    }

    public EntityReference<Smoke> SpawnSmoke(Vector origin, Vector velocity)
    {
        Smoke effect = Entities.Create<Smoke>(new
        {
            Origin = origin,
            Velocity = velocity
        });

        effect.Spawn();
        return Entities.GetReferenceTo(effect);
    }

    internal EntityReference<DashSmokeEffect> StartDashSmokeEffect(Player player)
    {
        DashSmokeEffect effect = Entities.Create<DashSmokeEffect>(new
        {
            Player = player
        });

        effect.Spawn();
        return Entities.GetReferenceTo(effect);
    }

    internal EntityReference<WallSlideEffect> StartWallSlideEffect(Player player)
    {
        WallSlideEffect effect = Entities.Create<WallSlideEffect>(new
        {
            Player = player
        });

        effect.Spawn();
        return Entities.GetReferenceTo(effect);
    }

    internal EntityReference<WallKickEffect> StartWallKickEffect(Player player)
    {
        WallKickEffect effect = Entities.Create<WallKickEffect>(new
        {
            Player = player
        });

        effect.Spawn();
        return Entities.GetReferenceTo(effect);
    }

    internal EntityReference<ExplosionEffect> CreateExplosionEffect(Vector origin, ExplosionEffectSound effectSound = ExplosionEffectSound.ENEMY_DIE_1)
    {
        ExplosionEffect effect = Entities.Create<ExplosionEffect>(new
        {
            Origin = origin,
            EffectSound = effectSound
        });

        effect.Spawn();
        return Entities.GetReferenceTo(effect);
    }

    private EntityReference<XDieExplosion> CreateXDieExplosionEffect(double phase)
    {
        XDieExplosion effect = Entities.Create<XDieExplosion>(new
        {
            Offset = Player.Origin - Camera.LeftTop,
            Phase = phase
        });

        effect.Spawn();
        return Entities.GetReferenceTo(effect);
    }

    public EntityReference<Flammingle> AddFlammingle(ushort subid, Vector origin)
    {
        Flammingle entity = Entities.Create<Flammingle>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<Scriver> AddScriver(ushort subid, Vector origin)
    {
        Scriver entity = Entities.Create<Scriver>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<AxeMax> AddAxeMax(ushort subid, Vector origin)
    {
        AxeMax entity = Entities.Create<AxeMax>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<Spiky> AddSpiky(ushort subid, Vector origin)
    {
        Spiky entity = Entities.Create<Spiky>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<HoverPlatform> AddHoverPlatform(ushort subid, Vector origin)
    {
        HoverPlatform entity = Entities.Create<HoverPlatform>(new
        {
            Origin = origin,
            Direction = (subid & 0x8) != 0 ? Direction.LEFT : Direction.RIGHT,
            HasTurnCannon = true
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<TurnCannon> AddTurnCannon(ushort subid, Vector origin)
    {
        TurnCannon entity = Entities.Create<TurnCannon>(new
        {
            Origin = origin,
            UpsideDown = (subid & 0x1) != 0
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<BombBeen> AddBombBeen(ushort subid, Vector origin)
    {
        BombBeen entity = Entities.Create<BombBeen>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<Probe8201U> AddProbe8201U(ushort subid, Vector origin)
    {
        Probe8201U entity = Entities.Create<Probe8201U>(new
        {
            Origin = origin,
            MovingVertically = (subid & 0x20) == 0,
            StartMovingBackward = (subid & 0x20) == 0,
            MoveDistance = (subid & 0x04) != 0 ? 7 * Probe8201U.BASE_MOVE_DISTANCE : Probe8201U.BASE_MOVE_DISTANCE
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<BattonBoneG> AddBattonBoneG(ushort subid, Vector origin)
    {
        BattonBoneG entity = Entities.Create<BattonBoneG>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public ArmorSoldier AddArmorSoldier(ushort subid, Vector origin)
    {
        ArmorSoldier entity = Entities.Create<ArmorSoldier>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<Jamminger> AddJamminger(ushort subid, Vector origin)
    {
        Jamminger entity = Entities.Create<Jamminger>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<Tombot> AddTombot(ushort subid, Vector origin)
    {
        Tombot entity = Entities.Create<Tombot>(new
        {
            Origin = origin
        });

        entity.Place(false);
        return Entities.GetReferenceTo(entity);
    }

    public Sprite AddCapsule(ushort subid, Vector origin)
    {
        // TODO : Implement
        return null;
    }

    public EntityReference<RayBit> AddRayBit(ushort subid, Vector origin)
    {
        RayBit entity = Entities.Create<RayBit>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<SnowShooter> AddSnowShooter(ushort subid, Vector origin)
    {
        SnowShooter entity = Entities.Create<SnowShooter>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<Snowball> AddSnowball(ushort subid, Vector origin)
    {
        Snowball entity = Entities.Create<Snowball>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<Igloo> AddIgloo(ushort subid, Vector origin)
    {
        Igloo entity = Entities.Create<Igloo>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    private void SpawnX(Vector origin)
    {
        if (Player != null)
            RemoveEntity(Player, true);

        player = Entities.Create<Player>(new
        {
            Name = "X",
            Origin = origin
        });

        Player.Spawn();
        Player.Lives = lastLives;
    }

    private void RemoveEntity(Entity entity, bool force = false)
    {
        if (entity is Sprite sprite)
        {
            if (sprite is HUD hud)
                huds[sprite.Layer].Remove(hud);
            else
                sprites[sprite.Layer].Remove(sprite);
        }

        entity.Cleanup();
        entity.Alive = false;
        entity.Dead = true;
        entity.DeathFrame = FrameCounter;

        aliveEntities.Remove(entity);

        if (force || !entity.Respawnable)
            Entities.Remove(entity);
        else if (entity.SpawnOnNear)
            entity.ResetFromInitParams();
    }

    private void CreateHP()
    {
        if (HP != null)
            RemoveEntity(HP, true);

        hp = Entities.Create<PlayerHealthHUD>(new
        {
            Name = "HP"
        });

        HP.Spawn();
    }

    private void StartReadyHUD()
    {
        if (ReadyHUD != null)
            RemoveEntity(ReadyHUD, true);

        readyHUD = Entities.Create<ReadyHUD>(new
        {
            Name = "Ready"
        });

        ReadyHUD.Spawn();
    }

    public static void WriteTriangle(DataStream vbData, Vector r0, Vector r1, Vector r2, Vector t0, Vector t1, Vector t2)
    {
        WriteVertex(vbData, (float) r0.X, (float) r0.Y, (float) t0.X, (float) t0.Y);
        WriteVertex(vbData, (float) r1.X, (float) r1.Y, (float) t1.X, (float) t1.Y);
        WriteVertex(vbData, (float) r2.X, (float) r2.Y, (float) t2.X, (float) t2.Y);
    }

    public static void WriteSquare(DataStream vbData, Vector vSource, Vector vDest, Vector srcSize, Vector dstSize, bool flipped = false, bool mirrored = false)
    {
        if (vSource.X < 0 || vSource.X > 1 || vSource.Y < 0 || vSource.Y > 1)
            throw new Exception();

        var r0 = new Vector(vDest.X, vDest.Y);
        var r1 = new Vector(vDest.X + dstSize.X, vDest.Y);
        var r2 = new Vector(vDest.X + dstSize.X, vDest.Y - dstSize.Y);
        var r3 = new Vector(vDest.X, vDest.Y - dstSize.Y);

        var t0 = new Vector(vSource.X, vSource.Y);
        var t1 = new Vector(vSource.X + srcSize.X, vSource.Y);
        var t2 = new Vector(vSource.X + srcSize.X, vSource.Y + srcSize.Y);
        var t3 = new Vector(vSource.X, vSource.Y + srcSize.Y);

        if (t0.X < 0 || t0.X > 1 || t0.Y < 0 || t0.Y > 1)
            throw new Exception();

        if (t1.X < 0 || t1.X > 1 || t1.Y < 0 || t1.Y > 1)
            throw new Exception();

        if (t2.X < 0 || t2.X > 1 || t2.Y < 0 || t2.Y > 1)
            throw new Exception();

        if (t3.X < 0 || t3.X > 1 || t3.Y < 0 || t3.Y > 1)
            throw new Exception();

        if (flipped)
        {
            if (mirrored)
            {
                WriteTriangle(vbData, r0, r1, r2, t2, t3, t0);
                WriteTriangle(vbData, r0, r2, r3, t2, t0, t1);
            }
            else
            {
                WriteTriangle(vbData, r0, r1, r2, t3, t2, t1);
                WriteTriangle(vbData, r0, r2, r3, t3, t1, t0);
            }
        }
        else if (mirrored)
        {
            WriteTriangle(vbData, r0, r1, r2, t1, t0, t3);
            WriteTriangle(vbData, r0, r2, r3, t1, t3, t2);
        }
        else
        {
            WriteTriangle(vbData, r0, r1, r2, t0, t1, t2);
            WriteTriangle(vbData, r0, r2, r3, t0, t2, t3);
        }
    }

    public void RenderVertexBuffer(VertexBuffer vb, int vertexSize, int primitiveCount, Texture texture, Texture palette, FadingControl fadingControl, Box box)
    {
        Device.SetStreamSource(0, vb, 0, vertexSize);

        RectangleF rDest = WorldBoxToScreen(box);

        float x = rDest.Left - SCREEN_WIDTH * 0.5f;
        float y = -rDest.Top + SCREEN_HEIGHT * 0.5f;

        var matScaling = Matrix.Scaling(1, 1, 1);
        var matTranslation = Matrix.Translation(x, y, 0);
        Matrix matTransform = matScaling * matTranslation;

        Device.SetTransform(TransformState.World, matTransform);
        Device.SetTransform(TransformState.View, Matrix.Identity);
        Device.SetTransform(TransformState.Texture0, Matrix.Identity);
        Device.SetTransform(TransformState.Texture1, Matrix.Identity);
        Device.SetTexture(0, texture);

        PixelShader shader;
        EffectHandle fadingLevelHandle;
        EffectHandle fadingColorHandle;

        if (palette != null)
        {
            fadingLevelHandle = plsFadingLevelHandle;
            fadingColorHandle = plsFadingColorHandle;
            shader = PaletteShader;
            Device.SetTexture(1, palette);
        }
        else
        {
            fadingLevelHandle = psFadingLevelHandle;
            fadingColorHandle = psFadingColorHandle;
            shader = PixelShader;
        }

        Device.PixelShader = shader;
        Device.VertexShader = null;

        if (fadingControl != null)
        {
            shader.Function.ConstantTable.SetValue(Device, fadingLevelHandle, fadingControl.FadingLevel);
            shader.Function.ConstantTable.SetValue(Device, fadingColorHandle, fadingControl.FadingColor.ToVector4());
        }
        else
        {
            shader.Function.ConstantTable.SetValue(Device, fadingLevelHandle, Vector4.Zero);
        }

        Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Point);
        Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Point);

        Device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
        Device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);

        Device.DrawPrimitives(PrimitiveType.TriangleList, 0, primitiveCount);
    }

    public Palette PrecachePalette(string name, Color[] colors, int capacity = 256, bool raiseExceptionIfNameAlreadyExists = true)
    {
        if (precachedPalettesByName.TryGetValue(name, out int index))
        {
            if (raiseExceptionIfNameAlreadyExists)
                throw new DuplicatePaletteNameException(name);

            return precachedPalettes[index];
        }

        if (colors.Length > capacity)
            throw new ArgumentException($"Length of colors should up to {capacity}.");

        var texture = new Texture(Device, capacity, 1, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
        DataRectangle rect = texture.LockRectangle(0, D3D9LockFlags.None);

        using (var stream = new DataStream(rect.DataPointer, capacity * 1 * sizeof(int), true, true))
        {
            for (int i = 0; i < colors.Length; i++)
                stream.Write(colors[i].ToBgra());

            for (int i = colors.Length; i < capacity; i++)
                stream.Write(0);
        }

        texture.UnlockRectangle(0);

        index = precachedPalettes.Count;
        var result = new Palette
        {
            Texture = texture,
            Index = index,
            name = name,
            Count = colors.Length
        };

        precachedPalettes.Add(result);
        precachedPalettesByName.Add(name, index);

        return result;
    }

    public Texture PrecachePaletteTexture(Texture image, int capacity = 256)
    {
        var palette = new Texture(Device, capacity, 1, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);

        DataRectangle paletteRect = palette.LockRectangle(0, D3D9LockFlags.None);
        DataRectangle imageRect = image.LockRectangle(0, D3D9LockFlags.None);

        try
        {
            using var paletteStream = new DataStream(paletteRect.DataPointer, capacity * 1 * sizeof(int), true, true);

            int width = image.GetLevelDescription(0).Width;
            int height = image.GetLevelDescription(0).Height;
            using var imageStream = new DataStream(imageRect.DataPointer, width * height * sizeof(int), true, true);

            var colors = new Dictionary<Color, int>();

            while (imageStream.Position < imageStream.Length)
            {
                Color color = imageStream.Read<Color>();
                if (color != Color.Transparent && !colors.ContainsKey(color))
                {
                    int index = colors.Count;
                    colors.Add(color, index);
                    paletteStream.Write(index);
                }
            }

            for (int i = colors.Count; i < capacity; i++)
                paletteStream.Write(0);
        }
        finally
        {
            image.UnlockRectangle(0);
            palette.UnlockRectangle(0);
        }

        return palette;
    }

    public Palette GetPaletteByIndex(int index)
    {
        return index >= 0 && index < precachedPalettes.Count ? precachedPalettes[index] : null;
    }

    public Palette GetPaletteByName(string name)
    {
        return precachedPalettesByName.TryGetValue(name, out int index) ? precachedPalettes[index] : null;
    }

    internal void UpdatePaletteName(Palette palette, string name)
    {
        if (name == palette.Name)
            return;

        if (precachedPalettesByName.ContainsKey(name))
            throw new DuplicatePaletteNameException(name);

        if (palette.Name is not null and not "")
            precachedPalettesByName.Remove(palette.Name);

        precachedPalettesByName.Add(name, palette.Index);

        palette.name = name;
    }

    private SpriteSheet AddSpriteSheet(string name, SpriteSheet sheet, bool raiseExceptionIfNameAlreadyExists = true)
    {
        if (spriteSheetsByName.TryGetValue(name, out int index))
        {
            if (raiseExceptionIfNameAlreadyExists)
                throw new DuplicateSpriteSheetNameException(name);

            return spriteSheets[index];
        }

        index = spriteSheets.Count;
        sheet.Index = index;
        sheet.name = name;

        spriteSheets.Add(sheet);
        spriteSheetsByName.Add(name, index);

        return sheet;
    }

    public SpriteSheet CreateSpriteSheet(string name, bool disposeTexture = false, bool precache = false, bool raiseExceptionIfNameAlreadyExists = true)
    {
        return AddSpriteSheet(name, new SpriteSheet(disposeTexture, precache), raiseExceptionIfNameAlreadyExists);
    }

    public SpriteSheet CreateSpriteSheet(string name, Texture texture, bool disposeTexture = false, bool precache = false, bool raiseExceptionIfNameAlreadyExists = true)
    {
        return AddSpriteSheet(name, new SpriteSheet(texture, disposeTexture, precache), raiseExceptionIfNameAlreadyExists);
    }

    public SpriteSheet CreateSpriteSheet(string name, string imageFileName, bool precache = false, bool raiseExceptionIfNameAlreadyExists = true)
    {
        return AddSpriteSheet(name, new SpriteSheet(imageFileName, precache), raiseExceptionIfNameAlreadyExists);
    }

    public SpriteSheet GetSpriteSheetByIndex(int index)
    {
        return index >= 0 && index < spriteSheets.Count ? spriteSheets[index] : null;
    }

    public SpriteSheet GetSpriteSheetByName(string name)
    {
        return spriteSheetsByName.TryGetValue(name, out int index) ? spriteSheets[index] : null;
    }

    internal void UpdateSpriteSheetName(SpriteSheet sheet, string name)
    {
        if (name == sheet.Name)
            return;

        if (spriteSheetsByName.ContainsKey(name))
            throw new DuplicateSpriteSheetNameException(name);

        if (sheet.Name is not null and not "")
            spriteSheetsByName.Remove(sheet.Name);

        spriteSheetsByName.Add(name, sheet.Index);

        sheet.name = name;
    }

    internal SoundChannel CreateSoundChannel(string name, float volume)
    {
        if (soundChannelsByName.ContainsKey(name))
            throw new DuplicateSoundChannelNameException(name);

        int index = soundChannels.Count;
        var channel = new SoundChannel(volume)
        {
            Index = index,
            name = name
        };

        soundChannels.Add(channel);
        soundChannelsByName.Add(name, index);

        return channel;
    }

    public SoundChannel GetSoundChannelByIndex(int index)
    {
        return index >= 0 && index < soundChannels.Count ? soundChannels[index] : null;
    }

    public SoundChannel GetSoundChannelByName(string name)
    {
        return soundChannelsByName.TryGetValue(name, out int index) ? soundChannels[index] : null;
    }

    internal void UpdateSoundChannelName(SoundChannel channel, string name)
    {
        if (name == channel.Name)
            return;

        if (soundChannelsByName.ContainsKey(name))
            throw new DuplicatePaletteNameException(name);

        if (channel.Name is not null and not "")
            soundChannelsByName.Remove(channel.Name);

        soundChannelsByName.Add(name, channel.Index);

        channel.name = name;
    }

    private void Execute()
    {
        World = new MMXWorld(32, 32);
        partition = new Partition<Entity>(World.ForegroundLayout.BoundingBox, World.ForegroundLayout.SceneRowCount, World.ForegroundLayout.SceneColCount);
        resultSet = new EntitySet<Entity>();

        ResetDevice();
        LoadLevel(@"resources\roms\" + ROM_NAME, INITIAL_LEVEL, INITIAL_CHECKPOINT);

        FrameCounter = 0;
        renderFrameCounter = 0;
        lastRenderFrameCounter = 0;
        previousElapsedTicks = 0;
        targetElapsedTime = Stopwatch.Frequency / TICKRATE;

        clock.Start();
        lastMeasuringFPSElapsedTicks = clock.ElapsedTicks;

        while (Running)
        {
            // Main loop
            RenderLoop.Run(Control, Render);
        }
    }

    public void PlaySound(int channelIndex, string name, double stopTime, double loopTime, bool ignoreUpdatesUntilPlayed = false)
    {
        if (!ENABLE_OST && channelIndex == 3)
            return;

        var sound = precachedSounds[precachedSoundsByName[name]];
        var channel = soundChannels[channelIndex];

        channel.Play(sound, stopTime, loopTime, ignoreUpdatesUntilPlayed);
    }

    public void PlaySound(int channel, string name, double loopTime, bool ignoreUpdatesUntilFinished = false)
    {
        PlaySound(channel, name, -1, loopTime, ignoreUpdatesUntilFinished);
    }

    public void PlaySound(int channel, string name, bool ignoreUpdatesUntilFinished = false)
    {
        PlaySound(channel, name, -1, -1, ignoreUpdatesUntilFinished);
    }

    public void ClearSoundLoopPoint(int channelIndex, string name, bool clearStopPoint = false)
    {
        var sound = precachedSounds[precachedSoundsByName[name]];
        var channel = soundChannels[channelIndex];

        if (channel.IsPlaying(sound))
            channel.ClearSoundLoopPoint(clearStopPoint);
    }

    public void ClearSoundStopPoint(int channelIndex, string name)
    {
        var sound = precachedSounds[precachedSoundsByName[name]];
        var channel = soundChannels[channelIndex];

        if (channel.IsPlaying(sound))
            channel.ClearSoundStopPoint();
    }

    public void PlayOST(string name, double stopTime, double loopTime)
    {
        PlaySound(3, name, stopTime, loopTime);
    }

    public void PlayOST(string name, double loopTime)
    {
        PlaySound(3, name, loopTime);
    }

    public void PlayOST(string name)
    {
        PlaySound(3, name);
    }

    public void StopOST(string name)
    {
        StopSound(3, name);
    }

    public void StopBossBattleOST()
    {
        StopOST("Boss Battle");
    }

    public void StopSound(int channelIndex, string name)
    {
        var sound = precachedSounds[precachedSoundsByName[name]];
        var channel = soundChannels[channelIndex];

        if (channel.IsPlaying(sound))
            channel.StopStream();
    }

    public void StopSound(int channelIndex)
    {
        var channel = soundChannels[channelIndex];
        channel.StopStream();
    }

    public void StopAllSounds(int exceptChannel = -1)
    {
        for (int channelIndex = 0; channelIndex < soundChannels.Count; channelIndex++)
        {
            if (channelIndex != exceptChannel)
            {
                var channel = soundChannels[channelIndex];
                channel.StopStream();
            }
        }
    }

    internal void ReloadLevelTransition()
    {
        DyingEffectActive = false;
        respawning = true;

        FadingControl.Reset();
        FadingControl.Start(Color.Black, 28, FadingFlags.COLORS, FadingFlags.COLORS, LoadLevel);

        StartFadingOST(0, 60, () => soundChannels[3].Stop());
    }

    internal void StartDyingEffect()
    {
        DyingEffectActive = true;
        DyingEffectFrameCounter = 0;
        FadingControl.Reset();
        FadingControl.Start(Color.White, 132, ReloadLevelTransition);
    }

    public SmallHealthRecover DropSmallHealthRecover(Vector origin, int durationFrames)
    {
        SmallHealthRecover drop = Entities.Create<SmallHealthRecover>(new
        {
            Origin = origin,
            DurationFrames = durationFrames
        });

        drop.Spawn();
        return drop;
    }

    public BigHealthRecover DropBigHealthRecover(Vector origin, int durationFrames)
    {
        BigHealthRecover drop = Entities.Create<BigHealthRecover>(new
        {
            Origin = origin,
            DurationFrames = durationFrames
        });

        drop.Spawn();
        return drop;
    }

    public SmallAmmoRecover DropSmallAmmoRecover(Vector origin, int durationFrames)
    {
        SmallAmmoRecover drop = Entities.Create<SmallAmmoRecover>(new
        {
            Origin = origin,
            DurationFrames = durationFrames
        });

        drop.Spawn();
        return drop;
    }

    public BigAmmoRecover DropBigAmmoRecover(Vector origin, int durationFrames)
    {
        BigAmmoRecover drop = Entities.Create<BigAmmoRecover>(new
        {
            Origin = origin,
            DurationFrames = durationFrames
        });

        drop.Spawn();
        return drop;
    }

    public LifeUp DropLifeUp(Vector origin, int durationFrames)
    {
        LifeUp drop = Entities.Create<LifeUp>(new
        {
            Origin = origin,
            DurationFrames = durationFrames
        });

        drop.Spawn();
        return drop;
    }

    public SmallHealthRecover AddSmallHealthRecover(Vector origin)
    {
        SmallHealthRecover item = Entities.Create<SmallHealthRecover>(new
        {
            Origin = origin,
            DurationFrames = 0
        });

        item.Place();
        return item;
    }

    public BigHealthRecover AddBigHealthRecover(Vector origin)
    {
        BigHealthRecover item = Entities.Create<BigHealthRecover>(new
        {
            Origin = origin,
            DurationFrames = 0
        });

        item.Place();
        return item;
    }

    public SmallAmmoRecover AddSmallAmmoRecover(Vector origin)
    {
        SmallAmmoRecover item = Entities.Create<SmallAmmoRecover>(new
        {
            Origin = origin,
            DurationFrames = 0
        });

        item.Place();
        return item;
    }

    public BigAmmoRecover AddBigAmmoRecover(Vector origin)
    {
        BigAmmoRecover item = Entities.Create<BigAmmoRecover>(new
        {
            Origin = origin,
            DurationFrames = 0
        });

        item.Place();
        return item;
    }

    public LifeUp AddLifeUp(Vector origin)
    {
        LifeUp item = Entities.Create<LifeUp>(new
        {
            Origin = origin,
            DurationFrames = 0
        });

        item.Place();
        return item;
    }

    public HeartTank AddHeartTank(Vector origin)
    {
        HeartTank item = Entities.Create<HeartTank>(new
        {
            Origin = origin
        });

        item.Place();
        return item;
    }

    public SubTankItem AddSubTank(Vector origin)
    {
        SubTankItem item = Entities.Create<SubTankItem>(new
        {
            Origin = origin
        });

        item.Place();
        return item;
    }

    internal void StartHealthRecovering(int amount)
    {
        Freeze(4, () => HealthRecoveringStep(amount));
    }

    internal void HealthRecoveringStep(int amount)
    {
        Player.Health++;
        amount--;

        PlaySound(0, "X Life Gain");

        if (amount > 0)
            Freeze(4, () => HealthRecoveringStep(amount));
    }

    internal void StartHeartTankAcquiring()
    {
        PlaySound(0, "X Sub Tank-Heart Powerup");
        Freeze(84, () => HeartTankAcquiringStep(2));
    }

    internal void HeartTankAcquiringStep(int amount)
    {
        Player.Health++;
        HealthCapacity++;
        amount--;

        PlaySound(0, "X Life Gain");

        if (amount > 0)
            Freeze(4, () => HeartTankAcquiringStep(amount));

        // TODO : Implement the remaining
    }

    internal void StartSubTankAcquiring()
    {
        PlaySound(0, "X Sub Tank-Heart Powerup");
        Freeze(84, SubTankAcquiringStep);
    }

    internal void SubTankAcquiringStep()
    {
        // TODO : Implement
    }

    public void Freeze(int frames, Action onFreezeComplete = null)
    {
        Freezing = true;
        FreezingFrames = frames;
        OnFreezeComplete = onFreezeComplete;
        FreezingFrameCounter = 0;
    }

    public void Unfreeze()
    {
        Freezing = false;
        OnFreezeComplete?.Invoke();
    }

    internal BossDoor AddBossDoor(byte eventSubId, Vector pos)
    {
        bool secondDoor = (eventSubId & 0x80) != 0;
        BossDoor door = Entities.Create<BossDoor>(new
        {
            Origin = pos,
            Bidirectional = false,
            StartBossBattle = secondDoor
        });

        door.OpeningEvent += (BossDoor source) => DoorOpening(secondDoor);
        door.ClosedEvent += (BossDoor source) => DoorClosing(secondDoor);
        door.Spawn();

        return door;
    }

    private void DoorOpening(bool secondDoor)
    {
        if (romLoaded && mmx.Type == 0 && mmx.Level == 8)
        {
            if (secondDoor)
            {
                Scene scene = World.ForegroundLayout.GetSceneFrom(1, 29);
                scene.SetMap(new Cell(7, 15), World.ForegroundLayout.GetMapByID(0x176));
                scene.SetMap(new Cell(8, 15), World.ForegroundLayout.GetMapByID(0x207));
                scene.SetMap(new Cell(9, 15), World.ForegroundLayout.GetMapByID(0x20f));

                scene = World.ForegroundLayout.GetSceneFrom(1, 30);
                scene.SetMap(new Cell(7, 0), World.ForegroundLayout.GetMapByID(0x177));
                scene.SetMap(new Cell(8, 0), World.ForegroundLayout.GetMapByID(0x1b0));
                scene.SetMap(new Cell(9, 0), World.ForegroundLayout.GetMapByID(0x20b));
            }
            else
            {
                Scene scene = World.ForegroundLayout.GetSceneFrom(1, 28);
                scene.SetMap(new Cell(7, 15), World.ForegroundLayout.GetMapByID(0x177));
                scene.SetMap(new Cell(8, 15), World.ForegroundLayout.GetMapByID(0x1b0));
                scene.SetMap(new Cell(9, 15), World.ForegroundLayout.GetMapByID(0x20b));

                scene = World.ForegroundLayout.GetSceneFrom(1, 29);
                scene.SetMap(new Cell(7, 0), World.ForegroundLayout.GetMapByID(0x177));
                scene.SetMap(new Cell(8, 0), World.ForegroundLayout.GetMapByID(0x1b0));
                scene.SetMap(new Cell(9, 0), World.ForegroundLayout.GetMapByID(0x20b));
            }
        }
    }

    private void DoorClosing(bool secondDoor)
    {
        if (romLoaded && mmx.Type == 0 && mmx.Level == 8)
        {
            if (secondDoor)
            {
                Scene scene = World.ForegroundLayout.GetSceneFrom(1, 29);
                scene.SetMap(new Cell(7, 15), World.ForegroundLayout.GetMapByID(0x172));
                scene.SetMap(new Cell(8, 15), World.ForegroundLayout.GetMapByID(0x173));
                scene.SetMap(new Cell(9, 15), World.ForegroundLayout.GetMapByID(0x174));

                scene = World.ForegroundLayout.GetSceneFrom(1, 30);
                scene.SetMap(new Cell(7, 0), World.ForegroundLayout.GetMapByID(0x172));
                scene.SetMap(new Cell(8, 0), World.ForegroundLayout.GetMapByID(0x173));
                scene.SetMap(new Cell(9, 0), World.ForegroundLayout.GetMapByID(0x174));
            }
            else
            {
                Scene scene = World.ForegroundLayout.GetSceneFrom(1, 28);
                scene.SetMap(new Cell(7, 15), World.ForegroundLayout.GetMapByID(0x172));
                scene.SetMap(new Cell(8, 15), World.ForegroundLayout.GetMapByID(0x173));
                scene.SetMap(new Cell(9, 15), World.ForegroundLayout.GetMapByID(0x174));

                scene = World.ForegroundLayout.GetSceneFrom(1, 29);
                scene.SetMap(new Cell(7, 0), World.ForegroundLayout.GetMapByID(0x172));
                scene.SetMap(new Cell(8, 0), World.ForegroundLayout.GetMapByID(0x173));
                scene.SetMap(new Cell(9, 0), World.ForegroundLayout.GetMapByID(0x174));
            }
        }
    }

    public void FreezeSprites(int frames, Action onComplete, params Sprite[] exceptions)
    {
        FreezingSprites = true;
        FreezingSpritesFrames = frames;
        FreezingSpritesFrameCounter = 0;
        OnFreezeSpritesComplete = onComplete;

        freezingSpriteExceptions.Clear();

        if (exceptions != null && exceptions.Length > 0)
            freezingSpriteExceptions.AddRange(exceptions);
    }

    public void FreezeSprites(int frames, params Sprite[] exceptions)
    {
        FreezeSprites(frames, null, exceptions);
    }

    public void FreezeSprites(params Sprite[] exceptions)
    {
        FreezeSprites(-1, null, exceptions);
    }

    public void UnfreezeSprites()
    {
        FreezingSprites = false;
        OnFreezeSpritesComplete?.Invoke();
    }

    public void KillAllAliveEnemies()
    {
        foreach (var entity in Entities)
        {
            if (entity is Enemy)
                entity.Kill();
        }
    }

    public void KillAllAliveWeapons()
    {
        foreach (var entity in Entities)
        {
            if (entity is Weapon)
                entity.Kill();
        }
    }

    public void KillAllAliveEnemiesAndWeapons()
    {
        foreach (var entity in Entities)
        {
            if (entity is Enemy or Weapon)
                entity.Kill();
        }
    }

    public void PlayBossIntroOST()
    {
        PlayOST("Boss Intro", 13, 6.328125);
    }

    public void PlayBossBatleOST()
    {
        PlayOST("Boss Battle", 57, 28.798);
    }

    public void PlayVictorySound()
    {
        PlayOST("Boss Defeated");
    }

    internal void StartBossBattle()
    {
        if (romLoaded && Boss != null)
        {
            BossBattle = true;
            BossIntroducing = true;
            PlayBossIntroOST();

            DoDelayedAction(64, Boss.Spawn);
        }
    }

    public void DoDelayedAction(int frames, Action action)
    {
        DelayedAction = action;
        DelayedActionFrames = frames;
        DelayedActionFrameCounter = 0;
    }

    public void PlayBossExplosionLoop()
    {
        PlayOST("Boss Explosion", 5.651, 0.323);
    }

    public void PlayBossExplosionEnd()
    {
        PlayOST("Boss Final Explode");
    }

    internal void OnPlayerTeleported()
    {
        FadingControl.Reset();
        FadingControl.Start(Color.Black, 60, ReloadLevel);
    }

    internal void UpdateSpriteLayer(Sprite sprite, int layer)
    {
        if (sprite is HUD hud)
        {
            huds[hud.Layer].Remove(hud);
            huds[layer].Add(hud);
        }
        else
        {
            sprites[sprite.Layer].Remove(sprite);
            sprites[layer].Add(sprite);
        }
    }

    public void CallPrecacheAction(Type baseType)
    {
        PrecacheAction first = null;
        PrecacheAction previous = null;
        bool shouldCall = false;

        for (var type = baseType; type != null; type = type.BaseType)
        {
            string name = type.Name;
            if (precacheActions.TryGetValue(type.AssemblyQualifiedName, out var action))
            {
                if (previous != null)
                    previous.Parent = action;

                break;
            }

            action = new PrecacheAction(type);
            precacheActions.Add(type.AssemblyQualifiedName, action);
            first ??= action;

            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            foreach (var method in methods)
            {
                Attribute attribute = method.GetCustomAttribute(typeof(PrecacheAttribute));
                if (attribute != null)
                {
                    if (method.GetParameters().Length > 0)
                        throw new Exception("Precache action should have no parameters.");

                    if (method.ReturnType != typeof(void))
                        throw new Exception("Precache action should have void as return type.");

                    shouldCall = true;
                    action.Method = method;

                    break;
                }
            }

            if (previous != null)
                previous.Parent = action;

            previous = action;
        }

        if (shouldCall)
            first.Call();
    }

    public void CallPrecacheAction<EntityType>() where EntityType : Entity
    {
        CallPrecacheAction(typeof(EntityType));
    }

    private void RecallPrecacheActions()
    {
        foreach (var kv in precacheActions)
        {
            var action = kv.Value;
            action.Reset();
        }

        foreach (var kv in precacheActions)
        {
            var action = kv.Value;
            action.Call();
        }
    }
}