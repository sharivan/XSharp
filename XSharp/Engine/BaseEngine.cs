using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

using XSharp.Engine.Collision;
using XSharp.Engine.Entities;
using XSharp.Engine.Entities.Effects;
using XSharp.Engine.Entities.Enemies;
using XSharp.Engine.Entities.Enemies.Amenhopper;
using XSharp.Engine.Entities.Enemies.AxeMax;
using XSharp.Engine.Entities.Enemies.BombBeen;
using XSharp.Engine.Entities.Enemies.Bosses;
using XSharp.Engine.Entities.Enemies.Bosses.ArmoredArmadillo;
using XSharp.Engine.Entities.Enemies.Bosses.BoomerKuwanger;
using XSharp.Engine.Entities.Enemies.Bosses.Bosspider;
using XSharp.Engine.Entities.Enemies.Bosses.ChillPenguin;
using XSharp.Engine.Entities.Enemies.Bosses.DRex;
using XSharp.Engine.Entities.Enemies.Bosses.FlameMammoth;
using XSharp.Engine.Entities.Enemies.Bosses.LauchOctupus;
using XSharp.Engine.Entities.Enemies.Bosses.RangdaBangda;
using XSharp.Engine.Entities.Enemies.Bosses.Sigma;
using XSharp.Engine.Entities.Enemies.Bosses.SparkMandrill;
using XSharp.Engine.Entities.Enemies.Bosses.StingChameleon;
using XSharp.Engine.Entities.Enemies.Bosses.Vile;
using XSharp.Engine.Entities.Enemies.Crusher;
using XSharp.Engine.Entities.Enemies.DeathRogumerCannon;
using XSharp.Engine.Entities.Enemies.DigLabour;
using XSharp.Engine.Entities.Enemies.DodgeBlaster;
using XSharp.Engine.Entities.Enemies.Flammingle;
using XSharp.Engine.Entities.Enemies.GunVolt;
using XSharp.Engine.Entities.Enemies.Hoganmer;
using XSharp.Engine.Entities.Enemies.Hotarion;
using XSharp.Engine.Entities.Enemies.LiftCannon;
using XSharp.Engine.Entities.Enemies.MegaTortoise;
using XSharp.Engine.Entities.Enemies.MetallC15;
using XSharp.Engine.Entities.Enemies.Minibosses.BeeBlader;
using XSharp.Engine.Entities.Enemies.Minibosses.ThunderSlimer;
using XSharp.Engine.Entities.Enemies.RayBit;
using XSharp.Engine.Entities.Enemies.RayTrap;
using XSharp.Engine.Entities.Enemies.RoadAttackers;
using XSharp.Engine.Entities.Enemies.ScrapRobo;
using XSharp.Engine.Entities.Enemies.SlideCannon;
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
using XSharp.Engine.Input;
using XSharp.Engine.Sound;
using XSharp.Engine.World;
using XSharp.Graphics;
using XSharp.Interop;
using XSharp.Math;
using XSharp.Math.Geometry;
using XSharp.MegaEDX;

using static XSharp.Engine.Consts;
using static XSharp.Engine.World.World;

using MMXWorld = XSharp.Engine.World.World;

namespace XSharp.Engine;

public abstract class BaseEngine : IRenderable, IRenderTarget
{
    public static BaseEngine Engine
    {
        get;
        protected set;
    }

    public static void Initialize(Type engineType, dynamic initializers)
    {
        if (Engine == null)
        {
            Engine = (BaseEngine) Activator.CreateInstance(engineType, true);
            Engine.Initialize(initializers);
        }
    }

    public static void Initialize<EngineType>(dynamic initializers) where EngineType : BaseEngine
    {
        Initialize(typeof(EngineType), initializers);
    }

    public static void Run()
    {
        Engine.Execute();
    }

    public static void RunSingleFrame()
    {
        Engine.RenderSingleFrame();
    }

    public static void DisposeEngine()
    {
        if (Engine != null)
        {
            Engine.Dispose();
            Engine = null;
        }
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

    public abstract DataStream CreateDataStream(IntPtr ptr, int sizeInBytes, bool canRead, bool canWrite);

    private void FillRegion(DataRectangle dataRect, int length, Box box, int paletteIndex)
    {
        int dstX = (int) box.Left;
        int dstY = (int) box.Top;
        int width = (int) box.Width;
        int height = (int) box.Height;

        using var dstDS = CreateDataStream(dataRect.DataPointer, length * sizeof(byte), true, true);
        for (int y = dstY; y < dstY + height; y++)
        {
            for (int x = dstX; x < dstX + width; x++)
            {
                dstDS.Seek(y * dataRect.Pitch + x * sizeof(byte), SeekOrigin.Begin);
                dstDS.Write((byte) paletteIndex);
            }
        }
    }

    private void DrawChargingPointLevel1Small(DataRectangle dataRect, int length, Vector point)
    {
        FillRegion(dataRect, length, new Box(point.X, point.Y, 1, 1), 1);
    }

    private void DrawChargingPointLevel1Large(DataRectangle dataRect, int length, Vector point)
    {
        FillRegion(dataRect, length, new Box(point.X, point.Y, 2, 2), 1);
    }

    private void DrawChargingPointLevel2Small1(DataRectangle dataRect, int length, Vector point)
    {
        FillRegion(dataRect, length, new Box(point.X, point.Y, 1, 1), 2);
    }

    private void DrawChargingPointLevel2Small2(DataRectangle dataRect, int length, Vector point)
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

    private void DrawChargingPointLevel2Large1(DataRectangle dataRect, int length, Vector point)
    {
        FillRegion(dataRect, length, new Box(point.X, point.Y, 2, 2), 2);
    }

    private void DrawChargingPointLevel2Large2(DataRectangle dataRect, int length, Vector point)
    {
        FillRegion(dataRect, length, new Box(point.X, point.Y, 2, 2), 3);

        FillRegion(dataRect, length, new Box(point.X, point.Y - 1, 2, 1), 4);
        FillRegion(dataRect, length, new Box(point.X, point.Y + 2, 2, 1), 4);
        FillRegion(dataRect, length, new Box(point.X - 1, point.Y, 1, 2), 4);
        FillRegion(dataRect, length, new Box(point.X + 2, point.Y, 1, 2), 4);
    }

    private void DrawChargingPointSmall(DataRectangle dataRect, int length, Vector point, int level, int type)
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

    private void DrawChargingPointLarge(DataRectangle dataRect, int length, Vector point, int level, int type)
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

    public static void DisposeResource(IDisposable resource)
    {
        try
        {
            resource?.Dispose();
        }
        catch
        {
        }
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

    protected ITexture whitePixelTexture;
    protected ITexture blackPixelTexture;
    protected ITexture foregroundTilemap;
    protected ITexture backgroundTilemap;

    protected ITexture stageTexture;

    private Palette foregroundPalette;
    private Palette backgroundPalette;

    protected IFont infoFont;
    protected IFont coordsTextFont;
    protected IFont highlightMapTextFont;

    protected ILine line;

    protected IKeyboard keyboard;
    protected IJoystick joystick;

    private List<SpriteSheet> spriteSheets;
    private Dictionary<string, int> spriteSheetsByName;

    private List<Palette> precachedPalettes;
    private Dictionary<string, int> precachedPalettesByName;

    internal List<PrecachedSound> precachedSounds;
    internal Dictionary<string, int> precachedSoundsByName;
    internal Dictionary<string, int> precachedSoundsByFileName;
    private List<SoundChannel> soundChannels;
    private Dictionary<string, int> soundChannelsByName;

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
    private EntitySet<Sprite> freezingSpriteExceptions;
    private List<Sprite>[] sprites;
    private List<HUD>[] huds;
    private ushort currentLevel;
    private bool changeLevel;
    private ushort levelToChange;
    private bool gameOver;
    private bool loadingLevel;
    private bool paused;

    private long lastCurrentMemoryUsage;
    private Box drawBox;
    private EntityReference<Checkpoint> currentCheckpoint;
    private List<Vector> cameraConstraints;

    protected bool frameAdvance;
    protected long frameAdvanceStartTime;
    protected bool recording;
    protected bool playbacking;

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

    protected bool drawBackground = true;
    protected bool drawDownLayer = true;
    protected bool drawSprites = true;
    protected bool drawX = true;
    protected bool drawUpLayer = true;

    protected bool drawHitbox = DEBUG_DRAW_HITBOX;
    protected bool showDrawBox = DEBUG_SHOW_BOUNDING_BOX;
    protected bool showColliders = DEBUG_SHOW_COLLIDERS;
    protected bool drawLevelBounds = DEBUG_DRAW_MAP_BOUNDS;
    protected bool drawTouchingMapBounds = DEBUG_HIGHLIGHT_TOUCHING_MAPS;
    protected bool drawHighlightedPointingTiles = DEBUG_HIGHLIGHT_POINTED_TILES;
    protected bool drawPlayerOriginAxis = DEBUG_DRAW_PLAYER_ORIGIN_AXIS;
    protected bool showInfoText = DEBUG_SHOW_INFO_TEXT;
    protected bool showCheckpointBounds = DEBUG_DRAW_CHECKPOINT;
    protected bool showTriggerBounds = DEBUG_SHOW_TRIGGERS;
    protected bool showTriggerCameraLockDirection = DEBUG_SHOW_CAMERA_TRIGGER_EXTENSIONS;
    protected bool enableSpawningBlackScreen = ENABLE_SPAWNING_BLACK_SCREEN;

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

    public abstract string Title
    {
        get;
        set;
    }

    public abstract Size2F ClientSize
    {
        get;
    }

    public MMXWorld World
    {
        get;
        private set;
    }

    public EntityFactory Entities
    {
        get;
        private set;
    }

    public Box DrawBox => drawBox;

    public ITexture ForegroundTilemap
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

    public ITexture BackgroundTilemap
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

    public Palette ForegroundPalette
    {
        get => foregroundPalette;
        set
        {
            if (foregroundPalette != value)
            {
                //DisposeResource(foregroundPalette);
                foregroundPalette = value;
            }
        }
    }

    public Palette BackgroundPalette
    {
        get => backgroundPalette;
        set
        {
            if (backgroundPalette != value)
            {
                //DisposeResource(backgroundPalette);
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

    public Vector StageSize
    {
        get;
        set;
    } = DEFAULT_CLIENT_SIZE;

    public bool Running
    {
        get;
        protected internal set;
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
        private set;
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

    public bool Editing
    {
        get;
        set;
    } = false;

    public RectangleF RenderRectangle => DrawBox.ToRectangleF();

    public WaveStreamFactory WaveStreamUtil
    {
        get;
        private set;
    }

    protected BaseEngine()
    {
    }

    protected abstract WaveStreamFactory CreateWaveStreamUtil();

    protected virtual void Initialize(dynamic initializers)
    {
        RNG = new RNG();

        Entities = new EntityFactory();
        aliveEntities = [];
        spawnedEntities = [];
        removedEntities = [];
        sprites = new List<Sprite>[NUM_SPRITE_LAYERS];
        huds = new List<HUD>[NUM_SPRITE_LAYERS];

        for (int i = 0; i < sprites.Length; i++)
            sprites[i] = [];

        for (int i = 0; i < huds.Length; i++)
            huds[i] = [];

        freezingSpriteExceptions = [];
        checkpoints = [];

        spriteSheets = [];
        spriteSheetsByName = [];

        precachedPalettes = [];
        precachedPalettesByName = [];

        precachedSounds = [];
        precachedSoundsByName = [];
        precachedSoundsByFileName = [];
        soundChannels = [];
        soundChannelsByName = [];

        FadingControl = new FadingControl();
        infoMessageFadingControl = new FadingControl();

        precacheActions = [];

        NoCameraConstraints = NO_CAMERA_CONSTRAINTS;
        cameraConstraints = [];

        WaveStreamUtil = CreateWaveStreamUtil();

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

        loadingLevel = true;

        DrawScale = DEFAULT_DRAW_SCALE;
        UpdateScale();

        World = new MMXWorld(32, 32);
        partition = new Partition<Entity>(World.ForegroundLayout.BoundingBox, World.ForegroundLayout.SceneRowCount, World.ForegroundLayout.SceneColCount);
        resultSet = [];

        ResetDevice();
        LoadLevel(@"Assets\ROMs\" + ROM_NAME, INITIAL_LEVEL, INITIAL_CHECKPOINT);

        FrameCounter = 0;
        renderFrameCounter = 0;
        lastRenderFrameCounter = 0;
        previousElapsedTicks = 0;
        targetElapsedTime = Stopwatch.Frequency / TICKRATE;

        clock.Start();
        lastMeasuringFPSElapsedTicks = clock.ElapsedTicks;

        Running = true;
    }

    public bool PrecacheSound(string name, string relativePath, out PrecachedSound sound, bool raiseExceptionIfNameExists = true)
    {
        if (precachedSoundsByFileName.TryGetValue(relativePath, out int index))
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

        var stream = WaveStreamUtil.FromFile(@"Assets\Sounds\" + relativePath);
        sound = new PrecachedSound(name, relativePath, stream);
        index = precachedSounds.Count;
        precachedSounds.Add(sound);
        precachedSoundsByName.Add(name, index);
        precachedSoundsByFileName.Add(relativePath, index);
        return true;
    }

    public bool PrecacheSound(string name, string relativePath)
    {
        return PrecacheSound(name, relativePath, out _);
    }

    protected virtual void Unload()
    {
        UnloadLevel();

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
    }

    protected abstract ITexture CreateStageTexture();

    protected abstract void InitGraphicDevice();

    private void CreateFonts()
    {
        var fontDescription = new FontDescription()
        {
            Height = 36,
            Italic = false,
            CharacterSet = FontCharacterSet.Ansi,
            FaceName = "Arial",
            OutputPrecision = FontPrecision.TrueType,
            PitchAndFamily = FontPitchAndFamily.Default,
            Quality = FontQuality.Antialiased,
            Weight = FontWeight.Bold
        };

        infoFont = CreateFont(fontDescription);

        fontDescription = new FontDescription()
        {
            Height = 24,
            Italic = false,
            CharacterSet = FontCharacterSet.Ansi,
            FaceName = "Arial",
            OutputPrecision = FontPrecision.TrueType,
            PitchAndFamily = FontPitchAndFamily.Default,
            Quality = FontQuality.Antialiased,
            Weight = FontWeight.Bold
        };

        coordsTextFont = CreateFont(fontDescription);

        fontDescription = new FontDescription()
        {
            Height = 24,
            Italic = false,
            CharacterSet = FontCharacterSet.Ansi,
            FaceName = "Arial",
            OutputPrecision = FontPrecision.TrueType,
            PitchAndFamily = FontPitchAndFamily.Default,
            Quality = FontQuality.Antialiased,
            Weight = FontWeight.Bold
        };

        highlightMapTextFont = CreateFont(fontDescription);
    }

    protected abstract IKeyboard CreateKeyboard();

    protected abstract IJoystick CreateJoystick();

    protected virtual void ResetDevice()
    {
        DisposeDevice();
        InitGraphicDevice();

        keyboard = CreateKeyboard();
        joystick = CreateJoystick();

        CreateFonts();

        line = CreateLine();

        whitePixelTexture = CreateImageTextureFromEmbeddedResource("Tiles.white_pixel.png", false);
        blackPixelTexture = CreateImageTextureFromEmbeddedResource("Tiles.black_pixel.png", false);

        stageTexture = CreateStageTexture();

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
        sequence.Sheet.CurrentTexture = CreateChargingTexture([new Vector(27, 2), new Vector(27, 46)], [true, true], [2, 2], level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, true, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture([new Vector(27, 3), new Vector(27, 45), new Vector(5, 24), new Vector(49, 24)], [true, true, true, true], [2, 2, 2, 2], level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture([new Vector(6, 24), new Vector(27, 5), new Vector(27, 44), new Vector(48, 24)], [true, true, true, true], [2, 2, 2, 2], level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture([new Vector(7, 24), new Vector(11, 40), new Vector(43, 8), new Vector(47, 24), new Vector(27, 43), new Vector(28, 6)], [true, true, true, true, false, false], [2, 1, 1, 2, 2, 2], level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture([new Vector(8, 24), new Vector(12, 39), new Vector(42, 9), new Vector(46, 24), new Vector(27, 42), new Vector(28, 7)], [true, true, true, true, false, false], [2, 1, 1, 2, 2, 2], level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture([new Vector(11, 8), new Vector(13, 38), new Vector(41, 10), new Vector(43, 40), new Vector(10, 25), new Vector(27, 41), new Vector(27, 7), new Vector(44, 23)], [true, true, true, true, false, false, false, false], [1, 2, 2, 1, 2, 1, 1, 2], level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture([new Vector(12, 9), new Vector(14, 37), new Vector(40, 11), new Vector(42, 39), new Vector(11, 25), new Vector(28, 9), new Vector(43, 23)], [true, true, true, true, false, false, false], [1, 2, 2, 1, 2, 1, 2], level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture([new Vector(13, 10), new Vector(19, 44), new Vector(35, 4), new Vector(41, 38), new Vector(12, 25), new Vector(16, 36), new Vector(39, 13), new Vector(43, 24)], [true, true, true, true, false, false, false, false], [1, 2, 2, 1, 1, 2, 2, 1], level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture([new Vector(14, 11), new Vector(19, 43), new Vector(35, 5), new Vector(40, 37), new Vector(13, 25), new Vector(17, 35), new Vector(38, 14), new Vector(42, 24)], [true, true, true, true, false, false, false, false], [1, 2, 2, 1, 1, 2, 2, 1], level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture([new Vector(7, 16), new Vector(20, 42), new Vector(34, 6), new Vector(47, 32), new Vector(16, 13), new Vector(18, 34), new Vector(37, 15), new Vector(39, 36)], [true, true, true, true, false, false, false, false], [2, 1, 1, 2, 1, 2, 2, 1], level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture([new Vector(8, 16), new Vector(20, 41), new Vector(34, 7), new Vector(46, 32), new Vector(17, 4), new Vector(19, 33), new Vector(36, 16), new Vector(38, 35)], [true, true, true, true, false, false, false, false], [2, 1, 1, 2, 1, 2, 2, 1], level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture([new Vector(9, 16), new Vector(19, 4), new Vector(24, 40), new Vector(33, 8), new Vector(35, 44), new Vector(45, 31), new Vector(18, 15), new Vector(37, 34)], [true, true, true, true, true, true, false, false], [2, 2, 1, 1, 2, 2, 1, 1], level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture([new Vector(10, 17), new Vector(19, 5), new Vector(21, 39), new Vector(39, 9), new Vector(35, 43), new Vector(44, 30), new Vector(19, 16), new Vector(36, 33)], [true, true, true, true, true, true, false, false], [2, 2, 1, 1, 2, 2, 1, 1], level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture([new Vector(7, 32), new Vector(47, 16), new Vector(12, 18), new Vector(21, 7), new Vector(22, 38), new Vector(33, 11), new Vector(35, 43), new Vector(44, 31)], [true, true, false, false, false, false, false, false], [2, 2, 2, 2, 1, 1, 2, 2], level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture([new Vector(8, 32), new Vector(46, 16), new Vector(13, 19), new Vector(20, 8), new Vector(22, 37), new Vector(33, 12), new Vector(36, 42), new Vector(43, 30)], [true, true, false, false, false, false, false, false], [2, 2, 2, 2, 1, 1, 2, 2], level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture([new Vector(21, 8), new Vector(33, 40), new Vector(10, 32), new Vector(14, 19), new Vector(42, 30), new Vector(46, 18)], [true, true, false, false, false, false], [1, 1, 2, 2, 2, 2], level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture([new Vector(21, 9), new Vector(33, 39), new Vector(11, 32), new Vector(15, 20), new Vector(41, 29), new Vector(45, 18)], [true, true, false, false, false, false], [1, 1, 2, 2, 2, 2], level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture([new Vector(11, 29), new Vector(43, 17), new Vector(23, 10)], [true, true, false], [1, 1, 1], level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture([new Vector(13, 30), new Vector(42, 19)], [true, true], [1, 1], level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture([new Vector(14, 29), new Vector(41, 20)], [false, false], [1, 1], level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture([new Vector(15, 29), new Vector(40, 20)], [false, false], [1, 1], level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
        sequence.Sheet.CurrentTexture = CreateChargingTexture([new Vector(27, 1), new Vector(27, 47)], [true, true], [2, 2], level);
        sequence.AddFrame(0, 0, CHARGING_EFFECT_HITBOX_SIZE, CHARGING_EFFECT_HITBOX_SIZE, 1, false, OriginPosition.CENTER);
    }

    public void StartFadingOST(float volume, int frames, bool fadeIn, Action onFadingComplete = null)
    {
        soundChannels[3].SaveVolume();

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

    internal ITexture CreateChargingTexture(Vector[] points, bool[] large, int[] types, int level)
    {
        int width1 = (int) NextHighestPowerOfTwo(CHARGING_EFFECT_HITBOX_SIZE);
        int height1 = (int) NextHighestPowerOfTwo(CHARGING_EFFECT_HITBOX_SIZE);
        int length = width1 * height1;

        var result = CreateEmptyTexture(width1, height1);
        DataRectangle rect = result.LockRectangle(true);

        ZeroDataRect(rect, length);

        for (int i = 0; i < points.Length; i++)
        {
            if (large[i])
                DrawChargingPointLarge(rect, length, points[i], level, types[i]);
            else
                DrawChargingPointSmall(rect, length, points[i], level, types[i]);
        }

        result.UnlockRectangle();
        return result;
    }

    public abstract ITexture CreateEmptyTexture(int width, int height, Format format = Format.L8);

    public abstract ITexture CreateImageTextureFromFile(string filePath, bool systemMemory = true);

    public ITexture CreateImageTextureFromEmbeddedResource(string path, bool systemMemory = true)
    {
        return CreateImageTextureFromEmbeddedResource(Assembly.GetExecutingAssembly(), path, systemMemory);
    }

    public ITexture CreateImageTextureFromEmbeddedResource(Assembly assembly, string path, bool systemMemory = true)
    {
        string assemblyName = assembly.GetName().Name;
        using var stream = assembly.GetManifestResourceStream($"{assemblyName}.Assets.{path}");
        var texture = CreateImageTextureFromStream(stream, systemMemory);
        return texture;
    }

    public abstract ITexture CreateImageTextureFromStream(Stream stream, bool systemMemory = true);

    protected abstract IFont CreateFont(FontDescription description);

    protected abstract ILine CreateLine();

    public abstract void RenderSprite(ITexture texture, Palette palette, FadingControl fadingControl, Box box, Matrix transform, int repeatX = 1, int repeatY = 1);

    public void RenderSprite(ITexture texture, FadingControl fadingControl, Box box, Matrix transform, int repeatX = 1, int repeatY = 1)
    {
        RenderSprite(texture, null, fadingControl, box, transform, repeatX, repeatY);
    }

    public void RenderSprite(ITexture texture, FadingControl fadingControl, Vector v, Matrix transform, int repeatX = 1, int repeatY = 1)
    {
        RenderSprite(texture, null, fadingControl, new Box(v.X, v.Y, texture.Width, texture.Height), transform, repeatX, repeatY);
    }

    public void RenderSprite(ITexture texture, FadingControl fadingControl, FixedSingle x, FixedSingle y, Matrix transform, int repeatX = 1, int repeatY = 1)
    {
        RenderSprite(texture, null, fadingControl, new Box(x, y, texture.Width, texture.Height), transform, repeatX, repeatY);
    }

    public void RenderSprite(ITexture texture, Palette palette, FadingControl fadingControl, Vector v, Matrix transform, int repeatX = 1, int repeatY = 1)
    {
        RenderSprite(texture, palette, fadingControl, new Box(v.X, v.Y, texture.Width, texture.Height), transform, repeatX, repeatY);
    }

    public void RenderSprite(ITexture texture, Palette palette, FadingControl fadingControl, FixedSingle x, FixedSingle y, Matrix transform, int repeatX = 1, int repeatY = 1)
    {
        RenderSprite(texture, palette, fadingControl, new Box(x, y, texture.Width, texture.Height), transform, repeatX, repeatY);
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

        if (romLoaded)
        {
            if (mmx.Type == 0)
            {
                switch (mmx.Level)
                {
                    case 8:
                        PlayOST("Chill Penguin", 83, 50.152);
                        break;

                    case 12:
                        PlayOST("Sigma Stage", 9.285, 4.677);
                        break;
                }
            }
        }

        FadingControl.Reset();
        FadingControl.Start(Color.Black, 26, FadingFlags.COLORS, FadingFlags.COLORS, StartReadyHUD);
    }

    private Keys HandleInput(out bool nextFrame)
    {
        Keys keys = Keys.NONE;
        nextFrame = !frameAdvance;

        if (ENABLE_BACKGROUND_INPUT || IsFocused())
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
                    frameAdvanceStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                }
                else if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - frameAdvanceStartTime >= 1000)
                {
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
                catch (Exception e)
                {
                    ShowErrorMessage(e.Message);
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

    protected virtual bool OnFrame()
    {
        Keys keys = HandleInput(out bool nextFrame);

        if (!nextFrame)
            return false;

        RNG.UpdateSeed((ulong) FrameCounter);

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

        if (Editing)
        {
            var origin = Camera.Origin;

            if (keys.HasLeft())
            {
                origin += 4 * Vector.LEFT_VECTOR;
            }
            else if (keys.HasRight())
            {
                origin += 4 * Vector.RIGHT_VECTOR;
            }

            if (keys.HasUp())
            {
                origin += 4 * Vector.UP_VECTOR;
            }
            else if (keys.HasDown())
            {
                origin += 4 * Vector.DOWN_VECTOR;
            }

            if (origin != Camera.Origin)
                Camera.SetOrigin(origin, false);

            return false;
        }

        HandleScreenEffects();

        if (!loadingLevel)
        {
            if (spawnedEntities.Count > 0)
            {
                foreach (var added in spawnedEntities)
                {
                    aliveEntities.Add(added);
                    added.NotifySpawn();

                    if (added is Sprite sprite)
                    {
                        if (sprite is HUD hud)
                        {
                            int newLayer = hud.Layer;
                            if (newLayer < 0 || newLayer >= huds.Length)
                                throw new IndexOutOfRangeException($"Invalid layer '{newLayer}'.");

                            var layer = huds[newLayer];

                            if (hud.priority >= 0)
                            {
                                layer.Insert(hud.priority, hud);

                                for (int i = hud.priority + 1; i < layer.Count; i++)
                                {
                                    var otherHUD = layer[i];
                                    otherHUD.priority = i;
                                }
                            }
                            else
                            {
                                hud.priority = layer.Count;
                                layer.Add(hud);
                            }

                            hud.priority = layer.Count - 1;
                        }
                        else
                        {
                            int newLayer = sprite.Layer;
                            if (newLayer < 0 || newLayer >= huds.Length)
                                throw new IndexOutOfRangeException($"Invalid layer '{newLayer}'.");

                            var layer = sprites[newLayer];

                            if (sprite.priority >= 0)
                            {
                                layer.Insert(sprite.priority, sprite);

                                for (int i = sprite.priority + 1; i < layer.Count; i++)
                                {
                                    var otherSprite = layer[i];
                                    otherSprite.priority = i;
                                }
                            }
                            else
                            {
                                sprite.priority = layer.Count;
                                layer.Add(sprite);
                            }

                            sprite.priority = layer.Count - 1;
                        }
                    }

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

    public Vector GetDrawScale()
    {
        var scaleX = StageSize.X / Camera.Width;
        var scaleY = StageSize.Y / Camera.Height;
        return (scaleX, scaleY);
    }

    public Vector GetCameraScale()
    {
        var scaleX = Camera.Width / SCREEN_WIDTH;
        var scaleY = Camera.Height / SCREEN_HEIGHT;
        return (scaleX, scaleY);
    }

    public Vector2 WorldVectorToScreen(Vector v, bool transform = true)
    {
        var drawScale = transform ? GetDrawScale() : (1, 1);
        return ((v.RoundToFloor() - Camera.LeftTop.RoundToFloor()) * drawScale + (transform ? drawBox.LeftTop : (0, 0))).ToVector2();
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
        var drawScale = GetDrawScale();
        return (new Vector(p.X, p.Y) - drawBox.LeftTop) / drawScale + Camera.LeftTop;
    }

    public Vector ScreenVector2ToWorld(Vector2 v)
    {
        var drawScale = GetDrawScale();
        return (new Vector(v.X, v.Y) - drawBox.LeftTop) / drawScale + Camera.LeftTop;
    }

    public RectangleF WorldBoxToScreen(Box box, bool transform = true)
    {
        var drawScale = transform ? GetDrawScale() : (1, 1);
        return ((box.LeftTopOrigin().RoundOriginToFloor() - Camera.LeftTop.RoundToFloor()) * drawScale + (transform ? drawBox.LeftTop : (0, 0))).ToRectangleF();
    }

    public IReadOnlyList<Sprite> GetSprites(int layer)
    {
        if (layer < 0 || layer >= sprites.Length)
            throw new IndexOutOfRangeException($"Invalid layer '{layer}'.");

        return sprites[layer];
    }

    public IReadOnlyList<HUD> GetHUDs(int layer)
    {
        if (layer < 0 || layer >= huds.Length)
            throw new IndexOutOfRangeException($"Invalid layer '{layer}'.");

        return huds[layer];
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
        HP?.Spawn();

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

            player = Entities.Create<Player>(new
            {
                Name = "X",
                Respawnable = true
            });

            hp = Entities.Create<PlayerHealthHUD>(new
            {
                Name = "HP",
                Respawnable = true
            });

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

                if (mmx.Type == 0)
                {
                    switch (mmx.Level)
                    {
                        case 8:
                            PrecacheSound("Chill Penguin", @"OST\X1\12 - Chill Penguin.mp3");
                            break;

                        case 12:
                            PrecacheSound("Sigma Stage", @"OST\X1\27 - Sigma Intro 2.mp3");
                            break;
                    }
                }
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
        var size = ClientSize;
        FixedSingle width = size.Width;
        FixedSingle height = size.Height;

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

    public abstract void DrawLine(Vector2 from, Vector2 to, float width, Color color, FadingControl fadingControl = null);

    public void DrawRectangle(Box box, float borderWith, Color color, FadingControl fadingControl = null)
    {
        DrawRectangle(WorldBoxToScreen(box), borderWith, color, fadingControl);
    }

    public abstract void DrawRectangle(RectangleF rect, float borderWith, Color color, FadingControl fadingControl = null);

    public void FillRectangle(Box box, Color color, FadingControl fadingControl = null)
    {
        FillRectangle(WorldBoxToScreen(box), color, fadingControl);
    }

    public abstract void FillRectangle(RectangleF rect, Color color, FadingControl fadingControl = null);

    public void DrawText(string text, IFont font, RectangleF drawRect, FontDrawFlags drawFlags, Color color, FadingControl fadingControl = null)
    {
        DrawText(text, font, drawRect, drawFlags, Matrix.Identity, color, out _, fadingControl);
    }

    public void DrawText(string text, IFont font, RectangleF drawRect, FontDrawFlags drawFlags, Color color, out RectangleF fontDimension, FadingControl fadingControl = null)
    {
        DrawText(text, font, drawRect, drawFlags, Matrix.Identity, color, out fontDimension, fadingControl);
    }

    public void DrawText(string text, IFont font, RectangleF drawRect, FontDrawFlags drawFlags, Matrix transform, Color color, FadingControl fadingControl = null)
    {
        DrawText(text, font, drawRect, drawFlags, transform, color, out _, fadingControl);
    }

    public void DrawText(string text, IFont font, RectangleF drawRect, FontDrawFlags drawFlags, float offsetX, float offsetY, Color color, FadingControl fadingControl = null)
    {
        Matrix transform = Matrix.Translation(offsetX, offsetY, 0);
        DrawText(text, font, drawRect, drawFlags, transform, color, out _, fadingControl);
    }

    public void DrawText(string text, IFont font, RectangleF drawRect, FontDrawFlags drawFlags, float offsetX, float offsetY, Color color, out RectangleF fontDimension, FadingControl fadingControl = null)
    {
        Matrix transform = Matrix.Translation(offsetX, offsetY, 0);
        DrawText(text, font, drawRect, drawFlags, transform, color, out fontDimension, fadingControl);
    }

    public abstract void DrawText(string text, IFont font, RectangleF drawRect, FontDrawFlags drawFlags, Matrix transform, Color color, out RectangleF fontDimension, FadingControl fadingControl = null);

    public void ShowInfoMessage(string message, int time = 3000, int fadingTime = 1000)
    {
        infoMessage = message;
        infoMessageShowingTime = time;
        infoMessageFadingTime = fadingTime;
        infoMessageStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }

    public abstract void ShowErrorMessage(string message);

    protected abstract void DisposeDeviceResources();

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
        DisposeResource(backgroundPalette);

        DisposeResource(infoFont);
        DisposeResource(coordsTextFont);
        DisposeResource(highlightMapTextFont);

        DisposeResource(line);

        DisposeDeviceResources();
    }

    protected abstract bool BeginScene();

    protected abstract void EndScene();

    protected abstract void Present();

    public void DrawTexture(ITexture texture, Palette palette = null, bool linear = false)
    {
        DrawTexture((0, 0, StageSize.X, StageSize.Y), texture, palette, linear);
    }

    public abstract void DrawTexture(Box destBox, ITexture texture, Palette palette = null, bool linear = false);

    private void RenderSingleFrame()
    {
        #region FPS and title update   
        var elapsedTicks = clock.ElapsedTicks;
        var deltaMilliseconds = 1000 * (elapsedTicks - lastMeasuringFPSElapsedTicks) / Stopwatch.Frequency;
        if (deltaMilliseconds >= 1000)
        {
            lastMeasuringFPSElapsedTicks = elapsedTicks;

            var deltaFrames = renderFrameCounter - lastRenderFrameCounter;
            lastRenderFrameCounter = renderFrameCounter;

            var fps = 1000.0 * deltaFrames / deltaMilliseconds;

            // Update window title with FPS once every second
            Title = $"X# - FPS: {fps:F2} ({(float) deltaMilliseconds / deltaFrames:F2}ms/frame)";
        }
        #endregion

        Render(this);
        renderFrameCounter++;
    }

    private void Render()
    {
        RenderSingleFrame();

        long elapsedTicks = clock.ElapsedTicks;
        long remainingTicks = targetElapsedTime - (elapsedTicks - previousElapsedTicks);

        if (remainingTicks > 0)
        {
            int msRemaining = (int) (1000 * remainingTicks / Stopwatch.Frequency);
            if (msRemaining > 0)
                Thread.Sleep(msRemaining);
        }

        elapsedTicks = clock.ElapsedTicks;
        previousElapsedTicks = elapsedTicks;
    }

    public abstract bool IsFocused();

    protected abstract Point GetCursorPosition();

    public abstract Point PointToClient(Point point);

    protected abstract IRenderTarget GetRenderTarget(int level);

    protected abstract void SetRenderTarget(int level, IRenderTarget target);

    protected abstract void Clear(Color color);

    protected abstract void PrepareRender();

    public virtual void Render(IRenderTarget target)
    {
        bool nextFrame = OnFrame();

        if (!BeginScene())
            return;

        PrepareRender();

        var backBuffer = GetRenderTarget(0);
        var stageRenderTarget = stageTexture.RenderTarget;

        SetRenderTarget(0, stageRenderTarget);
        Clear(Color.Transparent);

        if (!Editing && drawBackground)
        {
            World.RenderBackground(0);
            World.RenderBackground(1);
        }

        if (drawDownLayer)
            World.RenderForeground(0);

        if (drawSprites)
        {
            // Render down layer sprites
            foreach (var sprite in sprites[0])
            {
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
            foreach (var sprite in sprites[1])
            {
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

        SetRenderTarget(0, backBuffer);
        DrawTexture(stageTexture, null, SAMPLER_STATE_LINEAR);

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

        if (!Editing && (respawning || SpawningBlackScreen))
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
            var cursorPos = PointToClient(GetCursorPosition());
            Vector v = ScreenPointToVector(cursorPos.X, cursorPos.Y);
            DrawText($"Mouse Pos: X: {(float) v.X} Y: {(float) v.Y}", highlightMapTextFont, new RectangleF(0, 0, 400, 50), FontDrawFlags.Left | FontDrawFlags.Top, TOUCHING_MAP_COLOR);

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
            var v = WorldVectorToScreen(Player.Origin);

            line.Width = 2;

            line.Begin();
            line.Draw([new(v.X, v.Y - (float) Camera.Height), new(v.X, v.Y + (float) Camera.Height)], Color.Blue);
            line.Draw([new(v.X - (float) Camera.Width, v.Y), new(v.X + (float) Camera.Width, v.Y)], Color.Blue);
            line.End();
        }

        if (showCheckpointBounds && CurrentCheckpoint != null)
            DrawRectangle(CurrentCheckpoint.Hitbox, 4, Color.Yellow);

        if (showInfoText && Player != null)
        {
            string text = $"Checkpoint: {(CurrentCheckpoint != null ? currentCheckpoint.TargetIndex.ToString() : "none")}";
            DrawText(text, infoFont, drawRect, FontDrawFlags.Bottom | FontDrawFlags.Left, Color.White, out RectangleF fontDimension);

            text = $"Camera: CX: {(float) Camera.Left * 256}({(float) (Camera.Left - lastCameraLeftTop.X) * 256}) CY: {(float) Camera.Top * 256}({(float) (Camera.Top - lastCameraLeftTop.Y) * 256})";
            DrawText(text, infoFont, drawRect, FontDrawFlags.Bottom | FontDrawFlags.Left, 0, fontDimension.Top - fontDimension.Bottom, Color.White, out fontDimension);

            text = $"Player: X: {(float) Player.Origin.X * 256}({(float) (Player.Origin.X - lastPlayerOrigin.X) * 256}) Y: {(float) Player.Origin.Y * 256}({(float) (Player.Origin.Y - lastPlayerOrigin.Y) * 256}) VX: {(float) Player.Velocity.X * 256}({(float) (Player.Velocity.X - lastPlayerVelocity.X) * 256}) VY: {(float) Player.Velocity.Y * -256}({(float) (Player.Velocity.Y - lastPlayerVelocity.Y) * -256}) Gravity: {(float) Player.GetGravity() * 256}";
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
            EndScene();
            Present();
        }
        catch (Exception)
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
        serializer.DeserializeProperty(nameof(OnFadingOSTComplete), typeof(BaseEngine), this);
        SpawningBlackScreen = serializer.ReadBool();
        SpawningBlackScreenFrameCounter = serializer.ReadInt();
        DyingEffectActive = serializer.ReadBool();
        DyingEffectFrameCounter = serializer.ReadInt();
        Freezing = serializer.ReadBool();
        FreezingFrames = serializer.ReadInt();
        serializer.DeserializeProperty(nameof(OnFreezeComplete), typeof(BaseEngine), this);
        FreezeFrameCounter = serializer.ReadInt();
        FreezingFrameCounter = serializer.ReadInt();
        FreezingSprites = serializer.ReadBool();
        FreezingSpritesFrames = serializer.ReadInt();
        FreezingSpritesFrameCounter = serializer.ReadInt();
        serializer.DeserializeProperty(nameof(OnFreezeSpritesComplete), typeof(BaseEngine), this);
        serializer.DeserializeProperty(nameof(DelayedAction), typeof(BaseEngine), this);
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

                serializer.WriteString(sound.RelativePath, false);
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
                0x01 when mmx.Type == 0 => AddHoganmer(subid, origin),
                0x02 when mmx.Type == 0 => AddChillPenguin(origin),
                0x03 when mmx.Type == 0 => AddThunderSlimer(subid, origin),
                0x04 when mmx.Type == 0 => AddFlammingle(subid, origin),
                0x05 when mmx.Type == 0 => AddBoomerKuwanger(origin),
                0x06 when mmx.Type == 0 => AddPlanty(subid, origin),
                0x07 when mmx.Type == 0 => AddLaunchOctupus(subid, origin),
                0x09 when mmx.Type == 1 => AddScriver(subid, origin),
                0x0A when mmx.Type == 0 => AddStingChameleon(subid, origin),
                0x0B when mmx.Type == 0 => AddAxeMax(subid, origin),
                0x0C when mmx.Type == 0 => AddFlameMammoth(origin),
                0x0D when mmx.Type == 0 => AddRushRoader(subid, origin),
                0x0F when mmx.Type == 0 => AddCrusher(subid, origin),
                0x11 when mmx.Type == 0 => AddRoadAttacker(subid, origin),
                0x13 when mmx.Type == 0 => AddDodgeBlaster(subid, origin),
                0x14 when mmx.Type == 0 => AddArmoredArmadillo(subid, origin),
                0x15 when mmx.Type == 0 => AddSpiky(subid, origin),
                0x16 when mmx.Type == 0 => AddHoverPlatform(subid, origin),
                0x17 when mmx.Type == 0 => AddTurnCannon(subid, origin),
                0x19 when mmx.Type == 0 => AddBombBeen(subid, origin),
                0x1D when mmx.Type == 0 => AddGulpfer(subid, origin),
                0x1E when mmx.Type == 0 => AddMadPecker(subid, origin),
                0x20 when mmx.Type == 0 => AddAmenhopper(subid, origin),
                0x22 when mmx.Type == 0 => AddBeeBlader(subid, origin),
                0x27 when mmx.Type == 0 => AddBallDeVoux(subid, origin),
                0x29 when mmx.Type == 0 => AddGunVolt(subid, origin),
                0x2B when mmx.Type == 0 => AddMineCart(subid, origin),
                0x2C when mmx.Type == 0 => AddMoleBorer(subid, origin),
                0x2C when mmx.Type == 1 => AddProbe8201U(subid, origin),
                0x2D when mmx.Type == 0 => AddBattonBoneG(subid, origin),
                0x2E when mmx.Type == 0 => AddMetallC15(subid, origin),
                0x2F => AddArmorSoldier(subid, origin),
                0x30 when mmx.Type == 0 => AddDigLabour(subid, origin),
                0x31 when mmx.Type == 0 => AddSparkMandrill(subid, origin),
                0x36 when mmx.Type == 0 => AddJamminger(subid, origin),
                0x37 when mmx.Type == 0 => AddHotarion(subid, origin),
                0x39 when mmx.Type == 0 => AddCompressor(subid, origin),
                0x3A when mmx.Type == 0 => AddTombot(subid, origin),
                0x3B when mmx.Type == 0 => AddLadderYadder(subid, origin),
                0x3D when mmx.Type == 0 => AddBKElevator(subid, origin),
                0x40 when mmx.Type == 0 => AddCoil(subid, origin),
                0x42 when mmx.Type == 0 => AddRayField(subid, origin),
                0x44 when mmx.Type == 0 => AddRayTrap(subid, origin),
                0x46 when mmx.Type == 0 => AddMissiles(subid, origin),
                0x47 when mmx.Type == 0 => AddFlamePillar(subid, origin),
                0x49 when mmx.Type == 0 => AddSkyClaw(subid, origin),
                0x4C when mmx.Type == 0 => AddDrippingLava(subid, origin),
                0x4D => AddCapsule(subid, origin),
                0x4F when mmx.Type == 0 => AddRollingGabyoall(subid, origin),
                0x50 when mmx.Type == 0 => AddDeathRogumerCannon(subid, origin),
                0x50 when mmx.Type == 1 => AddBattonBoneG(subid, origin),
                0x51 when mmx.Type == 0 => AddRayBit(subid, origin),
                0x53 when mmx.Type == 0 => AddSnowShooter(subid, origin),
                0x54 when mmx.Type == 0 => AddSnowball(subid, origin),
                0x57 when mmx.Type == 0 => AddIgloo(subid, origin),
                0x59 when mmx.Type == 0 => AddLiftCannon(subid, origin),
                0x5B when mmx.Type == 0 => AddMegaTortoise(subid, origin),
                0x5D when mmx.Type == 0 => AddRangdaBangda(subid, origin),
                0x62 when mmx.Type == 0 => AddDRex(subid, origin),
                0x63 when mmx.Type == 0 => AddBosspider(subid, origin),
                0x64 when mmx.Type == 0 => AddPrisionCapsule(subid, origin),
                0x65 when mmx.Type == 0 => AddJediSigma(origin),
                0x66 when mmx.Type == 0 => AddVile(subid, origin),
                0x67 when mmx.Type == 0 => AddRideArmorVile(subid, origin),
                _ => mmx.Type == 0 && mmx.Level == 8 ? AddScriver(subid, origin) : null
            };
    }

    private void OnBossDefeated(Boss boss, Player killer)
    {
        killer.StopMoving();
        killer.Invincible = true;
        killer.InputLocked = true;
        killer.FaceToScreenCenter();
        PlayVictorySound();
        DoDelayedAction((int) (6.5 * 60), () => killer.StartTeleporting(true));
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

    public EntityReference<Hoganmer> AddHoganmer(ushort subid, Vector origin)
    {
        Hoganmer entity = Entities.Create<Hoganmer>(new
        {
            Origin = origin
        });

        entity.Place(false);
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<ChillPenguin> AddChillPenguin(Vector origin)
    {
        ChillPenguin entity = Entities.Create<ChillPenguin>(new
        {
            Origin = origin
        });

        entity.BossDefeatedEvent += OnBossDefeated;
        Boss = entity;

        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<ThunderSlimer> AddThunderSlimer(ushort subid, Vector origin)
    {
        ThunderSlimer entity = Entities.Create<ThunderSlimer>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
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

    public EntityReference<BoomerKuwanger> AddBoomerKuwanger(Vector origin)
    {
        BoomerKuwanger entity = Entities.Create<BoomerKuwanger>(new
        {
            Origin = origin
        });

        entity.BossDefeatedEvent += OnBossDefeated;
        Boss = entity;

        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<Planty> AddPlanty(ushort subid, Vector origin)
    {
        Planty entity = Entities.Create<Planty>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<LaunchOctupus> AddLaunchOctupus(ushort subid, Vector origin)
    {
        LaunchOctupus entity = Entities.Create<LaunchOctupus>(new
        {
            Origin = origin
        });

        entity.BossDefeatedEvent += OnBossDefeated;
        Boss = entity;

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

    public EntityReference<StingChameleon> AddStingChameleon(ushort subid, Vector origin)
    {
        StingChameleon entity = Entities.Create<StingChameleon>(new
        {
            Origin = origin
        });

        entity.BossDefeatedEvent += OnBossDefeated;
        Boss = entity;

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

    public EntityReference<FlameMammoth> AddFlameMammoth(Vector origin)
    {
        FlameMammoth entity = Entities.Create<FlameMammoth>(new
        {
            Origin = origin
        });

        entity.BossDefeatedEvent += OnBossDefeated;
        Boss = entity;

        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<RushRoader> AddRushRoader(ushort subid, Vector origin)
    {
        RushRoader entity = Entities.Create<RushRoader>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<Crusher> AddCrusher(ushort subid, Vector origin)
    {
        Crusher entity = Entities.Create<Crusher>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<RoadAttacker> AddRoadAttacker(ushort subid, Vector origin)
    {
        RoadAttacker entity = Entities.Create<RoadAttacker>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<DodgeBlaster> AddDodgeBlaster(ushort subid, Vector origin)
    {
        DodgeBlaster entity = Entities.Create<DodgeBlaster>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<ArmoredArmadillo> AddArmoredArmadillo(ushort subid, Vector origin)
    {
        ArmoredArmadillo entity = Entities.Create<ArmoredArmadillo>(new
        {
            Origin = origin
        });

        entity.BossDefeatedEvent += OnBossDefeated;
        Boss = entity;

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

    public EntityReference<Gulpfer> AddGulpfer(ushort subid, Vector origin)
    {
        Gulpfer entity = Entities.Create<Gulpfer>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<MadPecker> AddMadPecker(ushort subid, Vector origin)
    {
        MadPecker entity = Entities.Create<MadPecker>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<Amenhopper> AddAmenhopper(ushort subid, Vector origin)
    {
        Amenhopper entity = Entities.Create<Amenhopper>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<BeeBlader> AddBeeBlader(ushort subid, Vector origin)
    {
        BeeBlader entity = Entities.Create<BeeBlader>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<ScrapRobo> AddScrapRobo(ushort subid, Vector origin)
    {
        ScrapRobo entity = Entities.Create<ScrapRobo>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<SlideCannon> AddSlideCannon(ushort subid, Vector origin)
    {
        SlideCannon entity = Entities.Create<SlideCannon>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<BallDeVoux> AddBallDeVoux(ushort subid, Vector origin)
    {
        BallDeVoux entity = Entities.Create<BallDeVoux>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<GunVolt> AddGunVolt(ushort subid, Vector origin)
    {
        GunVolt entity = Entities.Create<GunVolt>(new
        {
            Origin = origin
        });

        entity.Place(false, Direction.RIGHT | Direction.BOTH_VERTICAL);
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<MineCart> AddMineCart(ushort subid, Vector origin)
    {
        MineCart entity = Entities.Create<MineCart>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<MoleBorer> AddMoleBorer(ushort subid, Vector origin)
    {
        MoleBorer entity = Entities.Create<MoleBorer>(new
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

    public EntityReference<RollingGabyoall> AddRollingGabyoall(ushort subid, Vector origin)
    {
        RollingGabyoall entity = Entities.Create<RollingGabyoall>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<DeathRogumerCannon> AddDeathRogumerCannon(ushort subid, Vector origin)
    {
        DeathRogumerCannon entity = Entities.Create<DeathRogumerCannon>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<BattonBoneG> AddBattonBoneG(ushort subid, Vector origin)
    {
        // TODO : If subid == 0 the enemy will be Batton M-501. Implement it.

        BattonBoneG entity = Entities.Create<BattonBoneG>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<MetallC15> AddMetallC15(ushort subid, Vector origin)
    {
        MetallC15 entity = Entities.Create<MetallC15>(new
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

    public EntityReference<DigLabour> AddDigLabour(ushort subid, Vector origin)
    {
        DigLabour entity = Entities.Create<DigLabour>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<SparkMandrill> AddSparkMandrill(ushort subid, Vector origin)
    {
        SparkMandrill entity = Entities.Create<SparkMandrill>(new
        {
            Origin = origin
        });

        entity.BossDefeatedEvent += OnBossDefeated;
        Boss = entity;

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

    public EntityReference<Hotarion> AddHotarion(ushort subid, Vector origin)
    {
        Hotarion entity = Entities.Create<Hotarion>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<Compressor> AddCompressor(ushort subid, Vector origin)
    {
        Compressor entity = Entities.Create<Compressor>(new
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

    public EntityReference<LadderYadder> AddLadderYadder(ushort subid, Vector origin)
    {
        LadderYadder entity = Entities.Create<LadderYadder>(new
        {
            Origin = origin
        });

        entity.Place(false);
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<BKElevator> AddBKElevator(ushort subid, Vector origin)
    {
        BKElevator entity = Entities.Create<BKElevator>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<Coil> AddCoil(ushort subid, Vector origin)
    {
        Coil entity = Entities.Create<Coil>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<RayField> AddRayField(ushort subid, Vector origin)
    {
        RayField entity = Entities.Create<RayField>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<RayTrap> AddRayTrap(ushort subid, Vector origin)
    {
        RayTrap entity = Entities.Create<RayTrap>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<Missiles> AddMissiles(ushort subid, Vector origin)
    {
        Missiles entity = Entities.Create<Missiles>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<FlamePillar> AddFlamePillar(ushort subid, Vector origin)
    {
        FlamePillar entity = Entities.Create<FlamePillar>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<SkyClaw> AddSkyClaw(ushort subid, Vector origin)
    {
        SkyClaw entity = Entities.Create<SkyClaw>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<DrippingLava> AddDrippingLava(ushort subid, Vector origin)
    {
        DrippingLava entity = Entities.Create<DrippingLava>(new
        {
            Origin = origin
        });

        entity.Place();
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

        entity.Place(true, Direction.RIGHT | Direction.BOTH_VERTICAL);
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

    public EntityReference<LiftCannon> AddLiftCannon(ushort subid, Vector origin)
    {
        LiftCannon entity = Entities.Create<LiftCannon>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<MegaTortoise> AddMegaTortoise(ushort subid, Vector origin)
    {
        MegaTortoise entity = Entities.Create<MegaTortoise>(new
        {
            Origin = origin
        });

        entity.Place(false);
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<RangdaBangda> AddRangdaBangda(ushort subid, Vector origin)
    {
        RangdaBangda entity = Entities.Create<RangdaBangda>(new
        {
            Origin = origin
        });

        entity.BossDefeatedEvent += OnBossDefeated;
        Boss = entity;

        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<DRex> AddDRex(ushort subid, Vector origin)
    {
        DRex entity = Entities.Create<DRex>(new
        {
            Origin = origin
        });

        entity.BossDefeatedEvent += OnBossDefeated;
        Boss = entity;

        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<Bosspider> AddBosspider(ushort subid, Vector origin)
    {
        Bosspider entity = Entities.Create<Bosspider>(new
        {
            Origin = origin
        });

        entity.BossDefeatedEvent += OnBossDefeated;
        Boss = entity;

        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<PrisionCapsule> AddPrisionCapsule(ushort subid, Vector origin)
    {
        PrisionCapsule entity = Entities.Create<PrisionCapsule>(new
        {
            Origin = origin
        });

        entity.Place();
        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<JediSigma> AddJediSigma(Vector origin)
    {
        JediSigma entity = Entities.Create<JediSigma>(new
        {
            Origin = origin
        });

        entity.BossDefeatedEvent += OnBossDefeated;
        Boss = entity;

        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<Vile> AddVile(ushort subid, Vector origin)
    {
        Vile entity = Entities.Create<Vile>(new
        {
            Origin = origin
        });

        entity.BossDefeatedEvent += OnBossDefeated;
        Boss = entity;

        return Entities.GetReferenceTo(entity);
    }

    public EntityReference<RideArmorVile> AddRideArmorVile(ushort subid, Vector origin)
    {
        RideArmorVile entity = Entities.Create<RideArmorVile>(new
        {
            Origin = origin
        });

        entity.BossDefeatedEvent += OnBossDefeated;
        Boss = entity;

        return Entities.GetReferenceTo(entity);
    }

    private void SpawnX(Vector origin)
    {
        Player.Origin = origin;
        Player.Spawn();
        Player.Lives = lastLives;
    }

    private void RemoveEntity(Entity entity, bool force = false)
    {
        if (entity is Sprite sprite)
        {
            if (sprite is HUD hud)
            {
                var layer = huds[sprite.Layer];
                layer.Remove(hud);

                int priority = hud.priority;
                hud.priority = -1;

                for (int i = priority; i < layer.Count; i++)
                {
                    hud = layer[i];
                    hud.priority = i;
                }
            }
            else
            {
                var layer = sprites[sprite.Layer];
                layer.Remove(sprite);

                int priority = sprite.priority;
                sprite.priority = -1;

                for (int i = priority; i < layer.Count; i++)
                {
                    sprite = layer[i];
                    sprite.priority = i;
                }
            }
        }

        entity.Cleanup();
        entity.Alive = false;
        entity.Dead = true;
        entity.DeathFrame = FrameCounter;

        aliveEntities.Remove(entity);

        if (force || !entity.KilledOnOffscreen && !entity.Respawnable)
            Entities.Remove(entity);
        else if (entity.SpawnOnNear)
            entity.ResetFromInitParams();
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

    protected abstract Palette CreatePalette(ITexture texture, int index, string name, int count);

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

        var texture = CreateEmptyTexture(capacity, 1, Format.A8R8G8B8);
        DataRectangle rect = texture.LockRectangle();

        using (var stream = CreateDataStream(rect.DataPointer, capacity * 1 * sizeof(int), true, true))
        {
            for (int i = 0; i < colors.Length; i++)
                stream.Write(colors[i].ToBgra());

            for (int i = colors.Length; i < capacity; i++)
                stream.Write(0);
        }

        texture.UnlockRectangle();

        index = precachedPalettes.Count;
        var result = CreatePalette(texture, index, name, colors.Length);

        precachedPalettes.Add(result);
        precachedPalettesByName.Add(name, index);

        return result;
    }

    public ITexture PrecachePaletteTexture(ITexture image, int capacity = 256)
    {
        var palette = CreateEmptyTexture(capacity, 1, Format.A8R8G8B8);

        DataRectangle paletteRect = palette.LockRectangle();
        DataRectangle imageRect = image.LockRectangle();

        try
        {
            using var paletteStream = CreateDataStream(paletteRect.DataPointer, capacity * 1 * sizeof(int), true, true);

            int width = image.Width;
            int height = image.Height;
            using var imageStream = CreateDataStream(imageRect.DataPointer, width * height * sizeof(int), true, true);

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
            image.UnlockRectangle();
            palette.UnlockRectangle();
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

    protected abstract SpriteSheet CreateSpriteSheet(bool disposeTexture = false, bool precache = false);

    protected abstract SpriteSheet CreateSpriteSheet(ITexture texture, bool disposeTexture = false, bool precache = false);

    protected abstract SpriteSheet CreateSpriteSheet(string imageFileName, bool precache = false);

    public SpriteSheet CreateSpriteSheet(string name, bool disposeTexture = false, bool precache = false, bool raiseExceptionIfNameAlreadyExists = true)
    {
        return AddSpriteSheet(name, CreateSpriteSheet(disposeTexture, precache), raiseExceptionIfNameAlreadyExists);
    }

    public SpriteSheet CreateSpriteSheet(string name, ITexture texture, bool disposeTexture = false, bool precache = false, bool raiseExceptionIfNameAlreadyExists = true)
    {
        return AddSpriteSheet(name, CreateSpriteSheet(texture, disposeTexture, precache), raiseExceptionIfNameAlreadyExists);
    }

    public SpriteSheet CreateSpriteSheet(string name, string imageFileName, bool precache = false, bool raiseExceptionIfNameAlreadyExists = true)
    {
        return AddSpriteSheet(name, CreateSpriteSheet(imageFileName, precache), raiseExceptionIfNameAlreadyExists);
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

    protected abstract SoundChannel CreateSoundChannel(float volume = 1);

    internal SoundChannel CreateSoundChannel(string name, float volume)
    {
        if (soundChannelsByName.ContainsKey(name))
            throw new DuplicateSoundChannelNameException(name);

        int index = soundChannels.Count;
        var channel = CreateSoundChannel(volume);
        channel.Index = index;
        channel.name = name;

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

    protected abstract void DoRenderLoop(Action action);

    private void Execute()
    {
        while (Running)
        {
            // Main loop
            DoRenderLoop(Render);
        }
    }

    public void PlaySound(int channelIndex, string name, double stopTime, double loopTime, bool ignoreUpdatesUntilPlayed = false)
    {
        if (!ENABLE_OST && channelIndex == 3)
            return;

        var sound = precachedSounds[precachedSoundsByName[name]];
        var channel = soundChannels[channelIndex];

        channel.Play(sound, stopTime, loopTime, ignoreUpdatesUntilPlayed);
        channel.RestoreVolume();
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

    public void StopOST()
    {
        StopSound(3);
    }

    public void StopOST(string name)
    {
        StopSound(3, name);
    }

    public void StopBossBattleOST()
    {
        StopOST("Boss Battle");
    }

    public void StopSound(int channelIndex)
    {
        var channel = soundChannels[channelIndex];
        channel.StopStream();
    }

    public void StopSound(int channelIndex, string name)
    {
        var sound = precachedSounds[precachedSoundsByName[name]];
        var channel = soundChannels[channelIndex];

        if (channel.IsPlaying(sound))
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

    internal EntityReference<BossDoor> AddBossDoor(byte eventSubId, Vector pos)
    {
        bool secondDoor = (eventSubId & 0x80) != 0;
        BossDoor door = Entities.Create<BossDoor>(new
        {
            Origin = pos,
            Bidirectional = false,
            StartBossBattle = secondDoor,
            AwaysVisible = false
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

    public void PlaySigmaBossIntroOST()
    {
        PlayOST("Sigma Boss Intro", 16.418, 8.226);
    }

    public void PlayBossBatleOST()
    {
        PlayOST("Boss Battle", 57, 28.798);
    }

    public void PlaySigmaBossBatleOST()
    {
        PlayOST("Sigma Boss Battle", 55.348, 0.023);
    }

    public void PlayJediSigmaBatleOST()
    {
        PlayOST("Jedi Sigma Battle", 35.020, 17.339);
    }

    public void PlayWolfSigmaIntroIntroOST()
    {
        PlayOST("Wolf Sigma Intro");
    }

    public void PlayWolfSigmaBatleOST()
    {
        PlayOST("Boss Battle", 105.582, 52.020);
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

    internal void StartSigmaBossBattle()
    {
        if (romLoaded && Boss != null)
        {
            BossBattle = true;
            BossIntroducing = true;
            PlaySigmaBossIntroOST();

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

    internal void UpdateSpriteLayer(Sprite sprite, int newLayer)
    {
        if (sprite is HUD hud)
        {
            if (newLayer < 0 || newLayer >= huds.Length)
                throw new IndexOutOfRangeException($"Invalid layer '{newLayer}'.");

            var layer = huds[hud.Layer];
            layer.Remove(hud);

            for (int i = hud.Priority; i < layer.Count; i++)
            {
                var otherHUD = layer[i];
                otherHUD.priority = i;
            }

            layer = huds[newLayer];
            layer.Add(hud);
            hud.priority = layer.Count - 1;
        }
        else
        {
            if (newLayer < 0 || newLayer >= sprites.Length)
                throw new IndexOutOfRangeException($"Invalid layer '{newLayer}'.");

            var layer = sprites[sprite.Layer];
            layer.Remove(sprite);

            for (int i = sprite.Priority; i < layer.Count; i++)
            {
                var otherSprite = layer[i];
                otherSprite.priority = i;
            }

            layer = sprites[newLayer];
            layer.Add(sprite);
            sprite.priority = layer.Count - 1;
        }

        sprite.layer = newLayer;
    }

    internal void UpdateSpritePriority(Sprite sprite, int priority)
    {
        int layerIndex = sprite.Layer;
        int lastPriority = sprite.Priority;

        if (sprite is HUD hud)
        {
            var layer = huds[layerIndex];

            if (priority < 0)
                priority = 0;
            else if (priority >= layer.Count)
                priority = layer.Count - 1;

            int start = System.Math.Min(priority, lastPriority);
            int end = System.Math.Max(priority, lastPriority);

            if (layer.Contains(hud))
                layer.Remove(hud);

            layer.Insert(priority, hud);

            for (int i = start; i <= end; i++)
            {
                hud = layer[i];
                hud.priority = i;
            }
        }
        else
        {
            var layer = sprites[layerIndex];

            if (priority < 0)
                priority = 0;
            else if (priority >= layer.Count)
                priority = layer.Count - 1;

            if (layer.Contains(sprite))
                layer.Remove(sprite);

            int start = System.Math.Min(priority, lastPriority);
            int end = System.Math.Max(priority, lastPriority);

            layer.Insert(priority, sprite);

            for (int i = start; i <= end; i++)
            {
                sprite = layer[i];
                sprite.priority = i;
            }
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

    public abstract void WriteVertex(DataStream vbData, float x, float y, float u, float v);

    public virtual void WriteTriangle(DataStream vbData, Vector r0, Vector r1, Vector r2, Vector t0, Vector t1, Vector t2)
    {
        WriteVertex(vbData, (float) r0.X, (float) r0.Y, (float) t0.X, (float) t0.Y);
        WriteVertex(vbData, (float) r1.X, (float) r1.Y, (float) t1.X, (float) t1.Y);
        WriteVertex(vbData, (float) r2.X, (float) r2.Y, (float) t2.X, (float) t2.Y);
    }

    public virtual void WriteSquare(DataStream vbData, Vector vSource, Vector vDest, Vector srcSize, Vector dstSize, bool flipped = false, bool mirrored = false)
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

    protected internal abstract Scene CreateScene(int id);

    public void Dispose()
    {
        Running = false;
        Unload();
        GC.SuppressFinalize(this);
    }
}