using NAudio.Wave;
using NLua;
using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.DirectInput;
using SharpDX.Mathematics.Interop;
using SharpDX.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using XSharp.Engine.Entities;
using XSharp.Engine.Entities.Effects;
using XSharp.Engine.Entities.Enemies;
using XSharp.Engine.Entities.Enemies.Bosses;
using XSharp.Engine.Entities.Enemies.Bosses.Penguin;
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
using XSharp.ROM;
using static XSharp.Engine.Consts;
using static XSharp.Engine.World.World;
using Configuration = System.Configuration.Configuration;
using D3D9LockFlags = SharpDX.Direct3D9.LockFlags;
using Device9 = SharpDX.Direct3D9.Device;
using DeviceType = SharpDX.Direct3D9.DeviceType;
using DXSprite = SharpDX.Direct3D9.Sprite;
using MMXBox = XSharp.Math.Geometry.Box;
using MMXWorld = XSharp.Engine.World.World;
using ResultCode = SharpDX.Direct3D9.ResultCode;
using SoundStream = XSharp.Engine.Sound.SoundStream;
using Sprite = XSharp.Engine.Entities.Sprite;

namespace XSharp.Engine
{
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
    }

    public class GameEngine
    {
        public static GameEngine Engine
        {
            get;
            private set;
        }

        public static void Initialize(Form form)
        {
            Engine = new GameEngine(form);
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

        private Texture worldTexture;
        private Texture spritesTexture;

        private int lastLives;
        private bool respawning;

        private EffectHandle psFadingLevelHandle;
        private EffectHandle psFadingColorHandle;
        private EffectHandle plsFadingLevelHandle;
        private EffectHandle plsFadingColorHandle;

        private readonly List<SpriteSheet> spriteSheets;
        private readonly Dictionary<string, SpriteSheet> spriteSheetsByName;

        private readonly List<Texture> palettes;

        private readonly List<WaveStream> soundStreams;
        private readonly List<(WaveOutEvent player, SoundStream stream, bool initialized)> soundChannels;

        internal Partition<Entity> partition;
        private HashSet<Entity> resultSet;

        private readonly List<Checkpoint> checkpoints;
        internal readonly Entity[] entities;
        private readonly Dictionary<string, Entity> entitiesByName;
        private int firstFreeEntityIndex;
        private Entity firstEntity;
        private Entity lastEntity;
        private int entityCount;
        internal List<Entity> spawnedEntities;
        internal List<Entity> removedEntities;
        internal Dictionary<Entity, RespawnEntry> autoRespawnableEntities;
        private readonly List<Sprite> freezingSpriteExceptions;
        private readonly List<Sprite>[] sprites;
        private readonly List<HUD>[] huds;
        private ushort currentLevel;
        private bool changeLevel;
        private ushort levelToChange;
        private bool gameOver;
        private bool loadingLevel;
        private bool paused;
        private Lua lua;

        private int currentSaveSlot;

        private long lastCurrentMemoryUsage;
        private MMXBox drawBox;
        private Checkpoint currentCheckpoint;
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

        private MMXCore mmx;
        private bool romLoaded;

        private readonly DirectInput directInput;
        private readonly Keyboard keyboard;
        private Joystick joystick;

        private bool drawBackground = true;
        private bool drawDownLayer = true;
        private bool drawSprites = true;
        private bool drawX = true;
        private bool drawUpLayer = true;

        private bool drawHitbox = DEBUG_DRAW_HITBOX;
        private bool showDrawBox = DEBUG_SHOW_BOUNDING_BOX;
        private bool showColliders = DEBUG_SHOW_COLLIDERS;
        private bool drawMapBounds = DEBUG_DRAW_MAP_BOUNDS;
        private bool drawTouchingMapBounds = DEBUG_HIGHLIGHT_TOUCHING_MAPS;
        private bool drawHighlightedPointingTiles = DEBUG_HIGHLIGHT_POINTED_TILES;
        private bool drawPlayerOriginAxis = DEBUG_DRAW_PLAYER_ORIGIN_AXIS;
        private bool showInfoText = DEBUG_SHOW_INFO_TEXT;
        private bool showCheckpointBounds = DEBUG_DRAW_CHECKPOINT;
        private bool showTriggerBounds = DEBUG_SHOW_TRIGGERS;
        private bool showTriggerCameraLook = DEBUG_SHOW_CAMERA_TRIGGER_EXTENSIONS;
        private bool enableSpawningBlackScreen = ENABLE_SPAWNING_BLACK_SCREEN;

        // Create Clock and FPS counters
        private readonly Stopwatch clock = new();
        private readonly double clockFrequency = Stopwatch.Frequency;
        private readonly Stopwatch fpsTimer = new();
        private int fpsFrames = 0;
        private double nextTick = 0;
        private readonly double tick = 1000D / TICKRATE;

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

        public Player Player
        {
            get;
            private set;
        }

        public PlayerHealthHUD HP
        {
            get;
            private set;
        }

        public ReadyHUD ReadyHUD
        {
            get;
            private set;
        }

        public FixedSingle HealthCapacity
        {
            get;
            set;
        } = X_INITIAL_HEALT_CAPACITY;

        public MMXBox CameraConstraintsBox
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

        public Vector MinCameraPos => NoCameraConstraints || World.Camera.NoConstraints ? World.BoundingBox.LeftTop : CameraConstraintsBox.LeftTop;

        public Vector MaxCameraPos => NoCameraConstraints || World.Camera.NoConstraints ? World.BoundingBox.RightBottom : CameraConstraintsBox.RightBottom;

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

        public void SetCheckpoint(Checkpoint value, int objectTile = -1, int backgroundTile = -1, int palette = -1)
        {
            if (currentCheckpoint != value)
            {
                currentCheckpoint = value;
                if (currentCheckpoint != null)
                {
                    CameraConstraintsBox = currentCheckpoint.Hitbox;

                    if (romLoaded)
                    {
                        mmx.SetLevel(mmx.Level, currentCheckpoint.Point, objectTile, backgroundTile, palette);
                        mmx.LoadTilesAndPalettes();
                        mmx.LoadPalette(this, false);
                        mmx.LoadPalette(this, true);
                        mmx.RefreshMapCache(this, false);
                        mmx.RefreshMapCache(this, true);
                    }
                }
                else
                    CameraConstraintsBox = World.BoundingBox;
            }
        }

        public int ObjectTile
        {
            get => mmx.ObjLoad;

            set
            {
                mmx.SetLevel(mmx.Level, currentCheckpoint.Point, value, mmx.TileLoad, mmx.PalLoad);
                mmx.LoadTilesAndPalettes();
                mmx.LoadPalette(this, false);
                mmx.LoadPalette(this, true);
                mmx.RefreshMapCache(this, false);
                mmx.RefreshMapCache(this, true);
            }
        }

        public int BackgroundTile
        {
            get => mmx.TileLoad;

            set
            {
                mmx.SetLevel(mmx.Level, currentCheckpoint.Point, mmx.ObjLoad, value, mmx.PalLoad);
                mmx.LoadTilesAndPalettes();
                mmx.LoadPalette(this, false);
                mmx.LoadPalette(this, true);
                mmx.RefreshMapCache(this, false);
                mmx.RefreshMapCache(this, true);
            }
        }

        public int Palette
        {
            get => mmx.PalLoad;

            set
            {
                mmx.SetLevel(mmx.Level, currentCheckpoint.Point, mmx.ObjLoad, mmx.TileLoad, value);
                mmx.LoadTilesAndPalettes();
                mmx.LoadPalette(this, false);
                mmx.LoadPalette(this, true);
                mmx.RefreshMapCache(this, false);
                mmx.RefreshMapCache(this, true);
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

        public IReadOnlyList<Checkpoint> Checkpoints => checkpoints;

        public long FrameCounter
        {
            get;
            private set;
        } = 0;

        public FadingControl FadingSettings
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

        public Random RNG
        {
            get;
        }

        public Boss Boss
        {
            get;
            private set;
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

        private GameEngine(Control control)
        {
            Control = control;

            clock.Start();
            fpsTimer.Start();

            freezingSpriteExceptions = new List<Sprite>();

            spriteSheets = new List<SpriteSheet>();
            spriteSheetsByName = new Dictionary<string, SpriteSheet>();

            palettes = new List<Texture>();

            soundStreams = new List<WaveStream>();
            soundChannels = new List<(WaveOutEvent, SoundStream, bool)>();

            FadingSettings = new FadingControl();

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
                Windowed = true,
                SwapEffect = SwapEffect.Discard,
                PresentationInterval = PresentInterval.One,
                AutoDepthStencilFormat = Format.D16,
                EnableAutoDepthStencil = true,
                BackBufferFormat = Format.X8R8G8B8,
                BackBufferHeight = Control.ClientSize.Height,
                BackBufferWidth = Control.ClientSize.Width
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

            for (int i = 0; i < 8; i++)
            {
                var player = new WaveOutEvent()
                {
                    Volume = 0.25f
                };

                var ss = new SoundStream();

                soundChannels.Add((player, ss, false));
            }

            soundChannels[3].player.Volume = 0.5f;

            // 0
            var stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\01 - MMX - X Regular Shot.wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 1
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx2\X Semi Charged Shot.wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 2
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\02 - MMX - X Charge Shot.wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 3
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\04 - MMX - X Charge.wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 4
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\07 - MMX - X Dash.wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 5
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\08 - MMX - X Jump.wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 6
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\09 - MMX - X Land.wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 7
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\17 - MMX - X Fade In.wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 8
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\30 - MMX - Small Hit.wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 9
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\10 - MMX - X Hurt.wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 10
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\11 - MMX - X Die.wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 11
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\11 - MMX - X Die.wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 12
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\56 - MMX - Enemy Die (1).wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 13
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\57 - MMX - Enemy Die (2).wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 14
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\58 - MMX - Enemy Die (3).wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 15
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\59 - MMX - Enemy Die (4).wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 16
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\16 - MMX - X Upgrade Complete.wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 17
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\18 - MMX - X Fade Out.wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 18
            stream = WaveStreamUtil.FromFile(@"resources\sounds\ost\mmx\12 - Chill Penguin.mp3", SoundFormat.MP3);
            soundStreams.Add(stream);

            // 19
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\12 - MMX - X Life Gain.wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 20
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\13 - MMX - X Extra Life.wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 21
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\14 - MMX - X Sub Tank-Heart Powerup.wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 22
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\Door Opening.wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 23
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\Door Closing.wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 24
            stream = WaveStreamUtil.FromFile(@"resources\sounds\ost\mmx\19 - Boss Intro.mp3", SoundFormat.MP3);
            soundStreams.Add(stream);

            // 25
            stream = WaveStreamUtil.FromFile(@"resources\sounds\ost\mmx\20 - Boss Battle.mp3", SoundFormat.MP3);
            soundStreams.Add(stream);

            // 26
            stream = WaveStreamUtil.FromFile(@"resources\sounds\ost\mmx\21 - Boss Defeated.mp3", SoundFormat.MP3);
            soundStreams.Add(stream);

            // 27
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\31 - MMX - Big Hit.wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 28
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\91 - MMX - Misc. dash, jump, move (3).wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 29
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\52 - MMX - Chill Penguin Breath.wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 30
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\34 - MMX - Ice.wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 31
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\35 - MMX - Ice Freeze.wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 32
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\36 - MMX - Ice Break.wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 33
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\29 - MMX - Enemy Helmet Hit.wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 34
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\Boss Explosion.wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 35
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\Boss Final Explode.wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            // 36
            stream = WaveStreamUtil.FromFile(@"resources\sounds\mmx\68 - MMX - Enemy Sound (05).wav", SoundFormat.WAVE);
            soundStreams.Add(stream);

            directInput = new DirectInput();

            keyboard = new Keyboard(directInput);
            keyboard.Properties.BufferSize = 2048;
            keyboard.Acquire();

            var joystickGuid = Guid.Empty;
            foreach (var deviceInstance in directInput.GetDevices(SharpDX.DirectInput.DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices))
                joystickGuid = deviceInstance.InstanceGuid;

            if (joystickGuid == Guid.Empty)
                foreach (var deviceInstance in directInput.GetDevices(SharpDX.DirectInput.DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
                    joystickGuid = deviceInstance.InstanceGuid;

            if (joystickGuid != Guid.Empty)
            {
                joystick = new Joystick(directInput, joystickGuid);
                joystick.Properties.BufferSize = 2048;
                joystick.Acquire();
            }

            RNG = new Random();
            checkpoints = new List<Checkpoint>();
            entities = new Entity[MAX_ENTITIES];
            entitiesByName = new Dictionary<string, Entity>();
            spawnedEntities = new List<Entity>();
            removedEntities = new List<Entity>();
            autoRespawnableEntities = new Dictionary<Entity, RespawnEntry>();
            sprites = new List<Sprite>[NUM_SPRITE_LAYERS];
            huds = new List<HUD>[NUM_SPRITE_LAYERS];

            for (int i = 0; i < sprites.Length; i++)
                sprites[i] = new List<Sprite>();

            for (int i = 0; i < huds.Length; i++)
                huds[i] = new List<HUD>();

            firstFreeEntityIndex = 0;
            firstEntity = null;
            lastEntity = null;
            entityCount = 0;

            loadingLevel = true;

            DrawScale = DEFAULT_DRAW_SCALE;
            UpdateScale();

            Running = true;
        }

        private void Unload()
        {
            UnloadLevel();

            DisposeResource(lua);

            foreach (var (channel, stream, _) in soundChannels)
            {
                DisposeResource(stream);
                DisposeResource(channel);
            }

            soundChannels.Clear();

            foreach (var stream in soundStreams)
                DisposeResource(stream);

            soundStreams.Clear();

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
            device.SetTextureStageState(0, TextureStage.AlphaOperation, TextureOperation.Modulate);
            device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
            device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);

            worldTexture = new Texture(device, SCREEN_WIDTH, SCREEN_HEIGHT, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
            spritesTexture = new Texture(device, SCREEN_WIDTH, SCREEN_HEIGHT, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);

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
                Quality = FontQuality.ClearType,
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
                Quality = FontQuality.ClearType,
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
                Quality = FontQuality.ClearType,
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
            //SetupQuad(VertexBuffer);

            // Create palettes

            // 0
            var x1NormalPalette = CreatePalette(X1_NORMAL_PALETTE);
            palettes.Add(x1NormalPalette);

            // 1
            var chargeLevel1Palette = CreatePalette(CHARGE_LEVEL_1_PALETTE);
            palettes.Add(chargeLevel1Palette);

            // 2
            var chargeLevel2Palette = CreatePalette(CHARGE_LEVEL_2_PALETTE);
            palettes.Add(chargeLevel2Palette);

            // 3
            var chargingEffectPalette = CreatePalette(CHARGE_EFFECT_PALETTE);
            palettes.Add(chargingEffectPalette);

            // 4
            var flashingPalette = CreatePalette(FLASHING_PALETTE);
            palettes.Add(flashingPalette);

            // 5
            var drillerPalette = CreatePalette(DRILLER_PALETTE);
            palettes.Add(drillerPalette);

            // 6
            var batPalette = CreatePalette(BAT_PALETTE);
            palettes.Add(batPalette);

            // 7
            var penguinPalette = CreatePalette(PENGUIN_PALETTE);
            palettes.Add(penguinPalette);

            // Create sprite sheets

            // 0
            var xSpriteSheet = AddSpriteSheet("X", true, true);

            // 1
            var xWeaponsSpriteSheet = AddSpriteSheet("X Weapons", true, true);

            // 2
            var xEffectsSpriteSheet = AddSpriteSheet("X Effects", true, true);

            // 3
            var xChargingEffectsSpriteSheet = AddSpriteSheet("X Charging Effects", true, false);

            // 4
            var drillerSpriteSheet = AddSpriteSheet("Driller", true, true);

            // 5
            var explosionSpriteSheet = AddSpriteSheet("Explosion", true, true);

            // 6
            var hpSpriteSheet = AddSpriteSheet("HP", true, true);

            // 7
            var readySpriteSheet = AddSpriteSheet("Ready", true, true);

            // 8
            var batSpriteSheet = AddSpriteSheet("Bat", true, true);

            // 9
            var bossDoorSpriteSheet = AddSpriteSheet("Boos Door", true, true);

            // 10
            var penguinSpriteSheet = AddSpriteSheet("Penguin", true, true);

            // 11
            var mistSpriteSheet = AddSpriteSheet("Mist", true, true);

            // Setup frame sequences (animations)

            // X
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XSharp.resources.sprites.X.X[small].png"))
            {
                var texture = CreateImageTextureFromStream(stream);
                xSpriteSheet.CurrentTexture = texture;
            }

            xSpriteSheet.CurrentPalette = x1NormalPalette;

            var sequence = xSpriteSheet.AddFrameSquence("Spawn");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(-4, 17, 5, 15, 8, 48);

            sequence = xSpriteSheet.AddFrameSquence("SpawnEnd");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(-4, 17, 5, 15, 8, 48);
            sequence.AddFrame(3, -2, 19, 34, 22, 29, 2);
            sequence.AddFrame(8, 11, 46, 21, 30, 42);
            sequence.AddFrame(8, 8, 84, 24, 30, 39);
            sequence.AddFrame(8, 5, 120, 27, 30, 36);
            sequence.AddFrame(8, 3, 156, 28, 30, 34);
            sequence.AddFrame(8, 1, 191, 31, 30, 32, 3);

            sequence = xSpriteSheet.AddFrameSquence("Stand");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(8, 3, 226, 29, 30, 34, 80, true);
            sequence.AddFrame(8, 3, 261, 29, 30, 34, 4);
            sequence.AddFrame(8, 3, 295, 29, 30, 34, 8);
            sequence.AddFrame(8, 3, 261, 29, 30, 34, 4);
            sequence.AddFrame(8, 3, 226, 29, 30, 34, 48);
            sequence.AddFrame(8, 3, 261, 29, 30, 34, 4);
            sequence.AddFrame(8, 3, 295, 29, 30, 34, 4);
            sequence.AddFrame(8, 3, 261, 29, 30, 34, 4);
            sequence.AddFrame(8, 3, 226, 29, 30, 34, 4);
            sequence.AddFrame(8, 3, 261, 29, 30, 34, 4);
            sequence.AddFrame(8, 3, 295, 29, 30, 34, 4);
            sequence.AddFrame(8, 3, 261, 29, 30, 34, 4);

            sequence = xSpriteSheet.AddFrameSquence("Tired");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(8, 4, 166, 331, 30, 35, 10, true);
            sequence.AddFrame(8, 3, 198, 332, 30, 34, 10);
            sequence.AddFrame(8, 2, 230, 333, 30, 33, 10);
            sequence.AddFrame(8, 3, 198, 332, 30, 34, 10);
            sequence.AddFrame(8, 4, 166, 331, 30, 35, 10);
            sequence.AddFrame(8, 3, 198, 332, 30, 34, 10);
            sequence.AddFrame(8, 2, 230, 333, 30, 33, 10);
            sequence.AddFrame(8, 3, 198, 332, 30, 34, 10);
            sequence.AddFrame(8, 4, 166, 331, 30, 35, 10);
            sequence.AddFrame(8, 3, 262, 332, 30, 34, 2);
            sequence.AddFrame(8, 3, 294, 332, 30, 34, 6);
            sequence.AddFrame(8, 3, 262, 332, 30, 34, 2);
            sequence.AddFrame(8, 2, 230, 333, 30, 33, 10);
            sequence.AddFrame(8, 3, 198, 332, 30, 34, 10);

            sequence = xSpriteSheet.AddFrameSquence("Shooting");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(8, 3, 365, 29, 30, 34, 4);
            sequence.AddFrame(8, 3, 402, 29, 29, 34, 12, true);

            sequence = xSpriteSheet.AddFrameSquence("PreWalking");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(8, 3, 5, 67, 30, 34, 5);

            sequence = xSpriteSheet.AddFrameSquence("Walking");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(1, 3, 50, 67, 20, 34, 1, true);
            sequence.AddFrame(3, 4, 75, 67, 23, 35, 2);
            sequence.AddFrame(7, 3, 105, 68, 32, 34, 3);
            sequence.AddFrame(10, 2, 145, 68, 34, 33, 3);
            sequence.AddFrame(5, 2, 190, 68, 26, 33, 3);
            sequence.AddFrame(3, 3, 222, 67, 22, 34, 2);
            sequence.AddFrame(5, 4, 248, 67, 25, 35, 2);
            sequence.AddFrame(5, 3, 280, 67, 30, 34, 3);
            sequence.AddFrame(8, 2, 318, 68, 34, 33, 3);
            sequence.AddFrame(7, 2, 359, 68, 29, 33, 3);
            sequence.AddFrame(1, 3, 50, 67, 20, 34);

            sequence = xSpriteSheet.AddFrameSquence("ShootWalking");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(1, 3, 41, 107, 29, 34, 1, true);
            sequence.AddFrame(3, 4, 76, 107, 32, 35, 2);
            sequence.AddFrame(7, 3, 115, 108, 35, 34, 3);
            sequence.AddFrame(10, 2, 159, 108, 38, 33, 3);
            sequence.AddFrame(5, 2, 204, 108, 34, 33, 3);
            sequence.AddFrame(3, 3, 246, 107, 31, 34, 2);
            sequence.AddFrame(5, 4, 284, 107, 33, 35, 2);
            sequence.AddFrame(5, 3, 326, 107, 35, 34, 3);
            sequence.AddFrame(8, 2, 369, 108, 37, 33, 3);
            sequence.AddFrame(7, 2, 413, 108, 35, 33, 3);
            sequence.AddFrame(1, 3, 41, 107, 29, 34);

            sequence = xSpriteSheet.AddFrameSquence("Jumping");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(1, 0, 6, 148, 25, 37, 3);
            sequence.AddFrame(-5, 1, 37, 148, 15, 41);

            sequence = xSpriteSheet.AddFrameSquence("ShootJumping");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(1, 0, 201, 148, 29, 37, 3);
            sequence.AddFrame(-5, 1, 240, 148, 24, 41);

            sequence = xSpriteSheet.AddFrameSquence("GoingUp");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(-1, 5, 56, 146, 19, 46, 1, true);

            sequence = xSpriteSheet.AddFrameSquence("ShootGoingUp");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(-1, 5, 271, 146, 27, 46, 1, true);

            sequence = xSpriteSheet.AddFrameSquence("Falling");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(1, 5, 80, 150, 23, 41, 4);
            sequence.AddFrame(5, 6, 108, 150, 27, 42, 1, true);

            sequence = xSpriteSheet.AddFrameSquence("ShootFalling");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(1, 5, 304, 150, 31, 41, 4);
            sequence.AddFrame(5 - 3, 6, 341, 150, 31, 42, 1, true);

            sequence = xSpriteSheet.AddFrameSquence("Landing");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(1, 2, 139, 151, 24, 38, 2);
            sequence.AddFrame(8, 1, 166, 153, 30, 32, 2);

            sequence = xSpriteSheet.AddFrameSquence("ShootLanding");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(1, 2, 378, 151, 30, 38, 2);
            sequence.AddFrame(8, 1, 413, 153, 36, 32, 2);

            sequence = xSpriteSheet.AddFrameSquence("PreDashing");
            sequence.OriginOffset = -DASHING_HITBOX.Origin - DASHING_HITBOX.Mins;
            sequence.CollisionBox = DASHING_HITBOX;
            sequence.AddFrame(4, 12, 4, 335, 28, 31, 3);

            sequence = xSpriteSheet.AddFrameSquence("ShootPreDashing");
            sequence.OriginOffset = -DASHING_HITBOX.Origin - DASHING_HITBOX.Mins;
            sequence.CollisionBox = DASHING_HITBOX;
            sequence.AddFrame(4, 12, 76, 335, 37, 31, 3);

            sequence = xSpriteSheet.AddFrameSquence("Dashing");
            sequence.OriginOffset = -DASHING_HITBOX.Origin - DASHING_HITBOX.Mins;
            sequence.CollisionBox = DASHING_HITBOX;
            sequence.AddFrame(14, 7, 34, 341, 38, 26, 1, true);

            sequence = xSpriteSheet.AddFrameSquence("ShootDashing");
            sequence.OriginOffset = -DASHING_HITBOX.Origin - DASHING_HITBOX.Mins;
            sequence.CollisionBox = DASHING_HITBOX;
            sequence.AddFrame(14, 7, 115, 341, 48, 26, 1, true);

            sequence = xSpriteSheet.AddFrameSquence("PostDashing");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(5, 0, 4, 335, 28, 31, 8);

            sequence = xSpriteSheet.AddFrameSquence("ShootPostDashing");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(5, 0, 76, 335, 37, 31, 8);

            sequence = xSpriteSheet.AddFrameSquence("WallSliding");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(5, 5, 5, 197, 25, 42, 5);
            sequence.AddFrame(9, 7, 33, 196, 27, 43, 6);
            sequence.AddFrame(9, 8, 64, 196, 28, 42, 1, true);

            sequence = xSpriteSheet.AddFrameSquence("ShootWallSliding");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(5, 2 - 3, 158, 200, 31, 39, 5);
            sequence.AddFrame(9 + 5, 7, 201, 196, 32, 43, 6);
            sequence.AddFrame(9 + 4, 8, 240, 196, 32, 42, 1, true);

            sequence = xSpriteSheet.AddFrameSquence("WallJumping");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(7, 2, 95, 199, 30, 39, 3);
            sequence.AddFrame(5, 10, 128, 195, 27, 44);

            sequence = xSpriteSheet.AddFrameSquence("ShootWallJumping");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(7, 1, 276, 200, 31, 38, 3);
            sequence.AddFrame(5, 5, 315, 200, 32, 39);

            sequence = xSpriteSheet.AddFrameSquence("PreLadderClimbing");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(3, 4, 7, 267, 21, 36, 8);

            sequence = xSpriteSheet.AddFrameSquence("LadderMoving");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(2, 10, 111, 261, 18, 49, 8, true);
            sequence.AddFrame(4, 5, 84, 266, 20, 40, 3);
            sequence.AddFrame(5, 6, 60, 266, 20, 40, 3);
            sequence.AddFrame(5, 14, 36, 261, 18, 49, 8);
            sequence.AddFrame(5, 6, 60, 266, 20, 40, 3);
            sequence.AddFrame(4, 5, 84, 266, 20, 40, 3);

            sequence = xSpriteSheet.AddFrameSquence("ShootLadder");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(5, 14, 137, 261, 26, 48, 16, true);

            sequence = xSpriteSheet.AddFrameSquence("TopLadderClimbing");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.AddFrame(5, -11, 169, 281, 21, 32, 4);
            sequence.AddFrame(2, -4, 195, 274, 18, 34, 4);

            sequence = xSpriteSheet.AddFrameSquence("TopLadderDescending");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(2, -4, 195, 274, 18, 34, 4);
            sequence.AddFrame(5, -11, 169, 281, 21, 32, 4);

            sequence = xSpriteSheet.AddFrameSquence("TakingDamage");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(6, 1, 478, 157, 25, 36, 4);
            sequence.AddFrame(10, -1, 509, 159, 29, 34, 1);
            sequence.AddFrame(10, -1, 543, 159, 29, 34, 1);
            sequence.AddFrame(11, 9, 578, 149, 32, 48, 2);
            sequence.AddFrame(10, 1, 616, 159, 31, 34, 2);
            sequence.AddFrame(11, 11, 654, 149, 32, 48, 2);
            sequence.AddFrame(10, 1, 692, 159, 29, 34, 2);
            sequence.AddFrame(11, 11, 727, 149, 32, 48, 2);
            sequence.AddFrame(12, -1, 768, 159, 31, 34, 1);
            sequence.AddFrame(10, 1, 804, 158, 29, 35, 1);
            sequence.AddFrame(10, -1, 509, 159, 29, 34, 8);
            sequence.AddFrame(6, 1, 478, 157, 25, 36, 2);

            sequence = xSpriteSheet.AddFrameSquence("Dying");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(6, 1, 478, 157, 25, 36, 30);

            sequence = xSpriteSheet.AddFrameSquence("Victory");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(7, 14, 221, 256, 28, 45, 10);
            sequence.AddFrame(7, 14, 259, 256, 29, 45, 1);
            sequence.AddFrame(7, 17, 297, 253, 34, 48, 2);
            sequence.AddFrame(7, 14, 335, 256, 29, 45, 2);
            sequence.AddFrame(7, 14, 373, 256, 31, 45, 3);
            sequence.AddFrame(7, 14, 221, 256, 28, 45, 40);

            sequence = xSpriteSheet.AddFrameSquence("PreTeleporting");
            sequence.OriginOffset = -HITBOX.Origin - HITBOX.Mins;
            sequence.CollisionBox = HITBOX;
            sequence.AddFrame(8, 1, 191, 31, 30, 32, 3);
            sequence.AddFrame(8, 4, 156, 28, 30, 34);
            sequence.AddFrame(8, 5, 120, 27, 30, 36);
            sequence.AddFrame(8, 8, 84, 24, 30, 39);
            sequence.AddFrame(8, 11, 46, 21, 30, 42);
            sequence.AddFrame(3, -3, 19, 34, 22, 29, 2);
            sequence.AddFrame(-4, 32, 5, 15, 8, 48);

            sequence = xSpriteSheet.AddFrameSquence("DyingExplosion");
            sequence.AddFrame(396, 344, 6, 6, 8, false, OriginPosition.CENTER);
            sequence.AddFrame(406, 343, 8, 8, 8, false, OriginPosition.CENTER);
            sequence.AddFrame(417, 342, 9, 9, 8, false, OriginPosition.CENTER);
            sequence.AddFrame(429, 341, 11, 11, 8, true, OriginPosition.CENTER);
            sequence.AddFrame(443, 339, 15, 15, 8, false, OriginPosition.CENTER);

            xSpriteSheet.ReleaseCurrentTexture();

            // X weapons
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XSharp.resources.sprites.X.Weapons.png"))
            {
                var texture = CreateImageTextureFromStream(stream);
                xWeaponsSpriteSheet.CurrentTexture = texture;
            }

            sequence = xWeaponsSpriteSheet.AddFrameSquence("LemonShot", 0);
            sequence.OriginOffset = -LEMON_HITBOX.Origin - LEMON_HITBOX.Mins;
            sequence.CollisionBox = LEMON_HITBOX;
            sequence.AddFrame(0, -1, 123, 253, 8, 6);

            sequence = xWeaponsSpriteSheet.AddFrameSquence("LemonShotExplode");
            sequence.OriginOffset = -LEMON_HITBOX.Origin - LEMON_HITBOX.Mins;
            sequence.CollisionBox = LEMON_HITBOX;
            sequence.AddFrame(2, 1, 137, 250, 12, 12, 4);
            sequence.AddFrame(2, 2, 154, 249, 13, 13, 2);
            sequence.AddFrame(3, 3, 172, 248, 15, 15);

            sequence = xWeaponsSpriteSheet.AddFrameSquence("SemiChargedShotFiring");
            sequence.OriginOffset = -SEMI_CHARGED_HITBOX1.Origin - SEMI_CHARGED_HITBOX1.Mins;
            sequence.CollisionBox = SEMI_CHARGED_HITBOX1;
            sequence.AddFrame(-5, -2, 128, 563, 14, 14);
            sequence.OriginOffset = -SEMI_CHARGED_HITBOX2.Origin - SEMI_CHARGED_HITBOX2.Mins;
            sequence.CollisionBox = SEMI_CHARGED_HITBOX2;
            sequence.AddFrame(-9, -6, 128, 563, 14, 14);
            sequence.AddFrame(-9, -1, 147, 558, 24, 24);
            sequence.OriginOffset = -SEMI_CHARGED_HITBOX3.Origin - SEMI_CHARGED_HITBOX3.Mins;
            sequence.CollisionBox = SEMI_CHARGED_HITBOX3;
            sequence.AddFrame(-11, 3, 147, 558, 24, 24);
            sequence.AddFrame(-11, -3, 176, 564, 28, 12);

            sequence = xWeaponsSpriteSheet.AddFrameSquence("SemiChargedShot");
            sequence.OriginOffset = -SEMI_CHARGED_HITBOX3.Origin - SEMI_CHARGED_HITBOX3.Mins;
            sequence.CollisionBox = SEMI_CHARGED_HITBOX3;
            sequence.AddFrame(3, -3, 176, 564, 28, 12);
            sequence.AddFrame(3, -5, 210, 566, 32, 8, 3);
            sequence.AddFrame(9, -5, 210, 566, 32, 8);
            sequence.AddFrame(7, -1, 379, 562, 38, 16);
            sequence.AddFrame(9, -3, 333, 564, 38, 12, 1, true); // loop point
            sequence.AddFrame(8, 1, 292, 559, 36, 22, 2);
            sequence.AddFrame(9, -3, 333, 564, 38, 12);
            sequence.AddFrame(7, -1, 379, 562, 38, 16, 2);

            sequence = xWeaponsSpriteSheet.AddFrameSquence("SemiChargedShotHit");
            sequence.OriginOffset = -SEMI_CHARGED_HITBOX2.Origin - SEMI_CHARGED_HITBOX2.Mins;
            sequence.CollisionBox = SEMI_CHARGED_HITBOX2;
            sequence.AddFrame(-9, -6, 424, 563, 14, 14, 2);
            sequence.AddFrame(-9, -1, 443, 558, 24, 24, 4);
            sequence.AddFrame(-9, -6, 424, 563, 14, 14, 4);

            sequence = xWeaponsSpriteSheet.AddFrameSquence("SemiChargedShotExplode");
            sequence.OriginOffset = -SEMI_CHARGED_HITBOX1.Origin - SEMI_CHARGED_HITBOX1.Mins;
            sequence.AddFrame(487, 273, 16, 16);
            sequence.AddFrame(507, 269, 24, 24);
            sequence.AddFrame(535, 273, 16, 16);
            sequence.AddFrame(555, 270, 22, 22);
            sequence.AddFrame(581, 269, 24, 24);
            sequence.AddFrame(609, 269, 24, 24);

            sequence = xWeaponsSpriteSheet.AddFrameSquence("ChargedShotFiring");
            sequence.OriginOffset = -CHARGED_HITBOX1.Origin - CHARGED_HITBOX1.Mins;
            sequence.CollisionBox = CHARGED_HITBOX1;
            sequence.AddFrame(-3, 1, 144, 440, 14, 20);
            sequence.AddFrame(-2, -1, 170, 321, 23, 16, 3);
            sequence.OriginOffset = -CHARGED_HITBOX2.Origin - CHARGED_HITBOX2.Mins;
            sequence.CollisionBox = CHARGED_HITBOX2;
            sequence.AddFrame(-25, -10, 170, 321, 23, 16, 3);

            sequence = xWeaponsSpriteSheet.AddFrameSquence("ChargedShot", 0);
            sequence.OriginOffset = -CHARGED_HITBOX2.Origin - CHARGED_HITBOX2.Mins;
            sequence.CollisionBox = CHARGED_HITBOX2;
            sequence.AddFrame(7, -2, 164, 433, 47, 32, 2, true);
            sequence.AddFrame(2, -2, 216, 433, 40, 32, 2);
            sequence.AddFrame(9, -2, 261, 432, 46, 32, 2);

            sequence = xWeaponsSpriteSheet.AddFrameSquence("ChargedShotHit");
            sequence.OriginOffset = -CHARGED_HITBOX2.Origin - CHARGED_HITBOX2.Mins;
            sequence.CollisionBox = CHARGED_HITBOX2;
            sequence.AddFrame(-26, -8, 315, 438, 14, 20, 2);
            sequence.AddFrame(-25, -4, 336, 434, 24, 28, 2);
            sequence.AddFrame(-26, -8, 315, 438, 14, 20, 4);

            sequence = xWeaponsSpriteSheet.AddFrameSquence("ChargedShotExplode");
            sequence.OriginOffset = -CHARGED_HITBOX2.Origin - CHARGED_HITBOX2.Mins;
            sequence.AddFrame(368, 434, 28, 28);
            sequence.AddFrame(400, 435, 26, 26);
            sequence.AddFrame(430, 434, 28, 28);
            sequence.AddFrame(462, 433, 30, 30);
            sequence.AddFrame(496, 432, 32, 32);
            sequence.AddFrame(532, 432, 32, 32);

            var smallHealthRecoverDroppingCollisionBox = new MMXBox(Vector.NULL_VECTOR, (-4, -8), (4, 0));

            sequence = xWeaponsSpriteSheet.AddFrameSquence("SmallHealthRecoverDropping");
            sequence.OriginOffset = -smallHealthRecoverDroppingCollisionBox.Mins;
            sequence.CollisionBox = smallHealthRecoverDroppingCollisionBox;
            sequence.AddFrame(0, 0, 6, 138, 8, 8);
            sequence.AddFrame(0, 0, 24, 114, 8, 8);
            sequence.AddFrame(0, 0, 6, 138, 8, 8, 1, true);

            var smallHealthRecoverIdleCollisionBox = new MMXBox(Vector.NULL_VECTOR, (-5, -8), (5, 0));

            sequence = xWeaponsSpriteSheet.AddFrameSquence("SmallHealthRecoverIdle");
            sequence.OriginOffset = -smallHealthRecoverIdleCollisionBox.Mins;
            sequence.CollisionBox = smallHealthRecoverIdleCollisionBox;
            sequence.AddFrame(0, 0, 22, 138, 10, 8, 1, true);
            sequence.AddFrame(0, 0, 40, 138, 10, 8, 2);
            sequence.AddFrame(0, 0, 58, 138, 10, 8, 2);
            sequence.AddFrame(0, 0, 40, 138, 10, 8, 2);
            sequence.AddFrame(0, 0, 22, 138, 10, 8, 1);

            var bigHealthRecoverDroppingCollisionBox = new MMXBox(Vector.NULL_VECTOR, (-7, -12), (7, 0));

            sequence = xWeaponsSpriteSheet.AddFrameSquence("BigHealthRecoverDropping");
            sequence.OriginOffset = -bigHealthRecoverDroppingCollisionBox.Mins;
            sequence.CollisionBox = bigHealthRecoverDroppingCollisionBox;
            sequence.AddFrame(0, 0, 3, 150, 14, 12);
            sequence.AddFrame(0, 0, 24, 114, 14, 12);
            sequence.AddFrame(0, 0, 3, 150, 14, 12, 1, true);

            var bigHealthRecoverIdleCollisionBox = new MMXBox(Vector.NULL_VECTOR, (-8, -12), (8, 0));

            sequence = xWeaponsSpriteSheet.AddFrameSquence("BigHealthRecoverIdle");
            sequence.OriginOffset = -bigHealthRecoverIdleCollisionBox.Mins;
            sequence.CollisionBox = bigHealthRecoverIdleCollisionBox;
            sequence.AddFrame(0, 0, 19, 150, 16, 12, 1, true);
            sequence.AddFrame(0, 0, 37, 150, 16, 12, 2);
            sequence.AddFrame(0, 0, 55, 150, 16, 12, 2);
            sequence.AddFrame(0, 0, 37, 150, 16, 12, 2);
            sequence.AddFrame(0, 0, 19, 150, 16, 12, 1);

            var smallAmmoRecoverCollisionBox = new MMXBox(Vector.NULL_VECTOR, (-4, -8), (4, 0));

            sequence = xWeaponsSpriteSheet.AddFrameSquence("SmallAmmoRecover");
            sequence.OriginOffset = -smallAmmoRecoverCollisionBox.Mins;
            sequence.CollisionBox = smallAmmoRecoverCollisionBox;
            sequence.AddFrame(0, 0, 84, 138, 8, 8, 2, true);
            sequence.AddFrame(0, 0, 100, 138, 8, 8, 2);
            sequence.AddFrame(0, 0, 116, 138, 8, 8, 2);
            sequence.AddFrame(0, 0, 100, 138, 8, 8, 2);

            var bigAmmoRecoverCollisionBox = new MMXBox(Vector.NULL_VECTOR, (-7, -14), (7, 0));

            sequence = xWeaponsSpriteSheet.AddFrameSquence("BigAmmoRecover");
            sequence.OriginOffset = -bigAmmoRecoverCollisionBox.Mins;
            sequence.CollisionBox = bigAmmoRecoverCollisionBox;
            sequence.AddFrame(0, 0, 81, 148, 14, 14, 2, true);
            sequence.AddFrame(0, 0, 97, 148, 14, 14, 2);
            sequence.AddFrame(0, 0, 113, 148, 14, 14, 2);
            sequence.AddFrame(0, 0, 97, 148, 14, 14, 2);

            var lifeUpCollisionBox = new MMXBox(Vector.NULL_VECTOR, (-8, -16), (8, 0));

            sequence = xWeaponsSpriteSheet.AddFrameSquence("LifeUp");
            sequence.OriginOffset = -lifeUpCollisionBox.Mins;
            sequence.CollisionBox = lifeUpCollisionBox;
            sequence.AddFrame(0, 0, 137, 146, 16, 16, 4, true);
            sequence.AddFrame(0, 0, 157, 146, 16, 16, 4);

            var heartTankCollisionBox = new MMXBox(Vector.NULL_VECTOR, (-8, -17), (8, 0));

            sequence = xWeaponsSpriteSheet.AddFrameSquence("HeartTank");
            sequence.OriginOffset = -heartTankCollisionBox.Mins;
            sequence.CollisionBox = heartTankCollisionBox;
            sequence.AddFrame(-1, 0, 183, 147, 14, 15, 11, true);
            sequence.AddFrame(-2, -1, 199, 147, 12, 15, 11);
            sequence.AddFrame(-3, -2, 213, 147, 10, 15, 11);
            sequence.AddFrame(-2, -1, 225, 147, 12, 15, 11);

            var subTankCollisionBox = new MMXBox(Vector.NULL_VECTOR, (-8, -19), (8, 0));

            sequence = xWeaponsSpriteSheet.AddFrameSquence("SubTank");
            sequence.OriginOffset = -subTankCollisionBox.Mins;
            sequence.CollisionBox = subTankCollisionBox;
            sequence.AddFrame(2, 0, 247, 143, 20, 19, 4, true);
            sequence.AddFrame(2, 0, 269, 143, 20, 19, 4);

            xWeaponsSpriteSheet.ReleaseCurrentTexture();

            // X effects
            sequence = xChargingEffectsSpriteSheet.AddFrameSquence("ChargingLevel1");
            AddChargingEffectFrames(sequence, 1);

            sequence = xChargingEffectsSpriteSheet.AddFrameSquence("ChargingLevel2");
            AddChargingEffectFrames(sequence, 2);

            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XSharp.resources.sprites.X.Effects.png"))
            {
                var texture = CreateImageTextureFromStream(stream);
                xEffectsSpriteSheet.CurrentTexture = texture;
            }

            sequence = xEffectsSpriteSheet.AddFrameSquence("WallKickEffect");
            sequence.AddFrame(0, 201, 11, 12, 2, false, OriginPosition.LEFT_TOP);

            sequence = xEffectsSpriteSheet.AddFrameSquence("PreDashSparkEffect");
            sequence.AddFrame(19, 124, 16, 32, 2, false, OriginPosition.LEFT_BOTTOM);

            sequence = xEffectsSpriteSheet.AddFrameSquence("DashSparkEffect");
            sequence.AddFrame(103, 124, 18, 32, 3, false, OriginPosition.LEFT_BOTTOM);
            sequence.AddFrame(139, 124, 23, 32, 2, false, OriginPosition.LEFT_BOTTOM);
            sequence.AddFrame(178, 124, 27, 32, 2, false, OriginPosition.LEFT_BOTTOM);
            sequence.AddFrame(219, 124, 27, 32, 2, false, OriginPosition.LEFT_BOTTOM);
            sequence.AddFrame(260, 124, 27, 32, 2, false, OriginPosition.LEFT_BOTTOM);
            sequence.AddFrame(301, 124, 27, 32, 2, false, OriginPosition.LEFT_BOTTOM);
            sequence.AddFrame(346, 124, 27, 32, 1, false, OriginPosition.LEFT_BOTTOM);

            sequence = xEffectsSpriteSheet.AddFrameSquence("DashSmokeEffect");
            sequence.AddFrame(3, 164, 8, 28, 4, false, OriginPosition.LEFT_BOTTOM);
            sequence.AddFrame(17, 164, 8, 28, 1, false, OriginPosition.LEFT_BOTTOM);
            sequence.AddFrame(44, 164, 10, 28, 2, false, OriginPosition.LEFT_BOTTOM);
            sequence.AddFrame(58, 164, 10, 28, 1, false, OriginPosition.LEFT_BOTTOM);
            sequence.AddFrame(71, 164, 13, 28, 2, false, OriginPosition.LEFT_BOTTOM);
            sequence.AddFrame(85, 164, 13, 28, 1, false, OriginPosition.LEFT_BOTTOM);
            sequence.AddFrame(99, 164, 13, 28, 1, false, OriginPosition.LEFT_BOTTOM);
            sequence.AddFrame(112, 164, 14, 28, 2, false, OriginPosition.LEFT_BOTTOM);
            sequence.AddFrame(126, 164, 14, 28, 1, false, OriginPosition.LEFT_BOTTOM);
            sequence.AddFrame(140, 164, 14, 28, 1, false, OriginPosition.LEFT_BOTTOM);
            sequence.AddFrame(154, 164, 13, 28, 1, false, OriginPosition.LEFT_BOTTOM);

            sequence = xEffectsSpriteSheet.AddFrameSquence("WallSlideEffect");
            sequence.AddFrame(0, 228, 8, 8, 4, false, OriginPosition.CENTER);
            sequence.AddFrame(12, 227, 10, 11, 5, false, OriginPosition.CENTER);
            sequence.AddFrame(29, 226, 13, 13, 5, false, OriginPosition.CENTER);
            sequence.AddFrame(49, 226, 14, 14, 3, false, OriginPosition.CENTER);
            sequence.AddFrame(70, 226, 14, 14, 3, false, OriginPosition.CENTER);
            sequence.AddFrame(90, 226, 14, 14, 3, false, OriginPosition.CENTER);

            xEffectsSpriteSheet.ReleaseCurrentTexture();

            // Enemies

            // Driller
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XSharp.resources.sprites.Enemies.X2.mmx2-driller.png"))
            {
                var texture = CreateImageTextureFromStream(stream);
                drillerSpriteSheet.CurrentTexture = texture;
            }

            drillerSpriteSheet.CurrentPalette = drillerPalette;

            // 0
            sequence = drillerSpriteSheet.AddFrameSquence("Idle");
            sequence.OriginOffset = -DRILLER_HITBOX.Origin - DRILLER_HITBOX.Mins;
            sequence.CollisionBox = DRILLER_HITBOX;
            sequence.AddFrame(-5, 6, 4, 4, 35, 30, 1, true);

            // 1
            sequence = drillerSpriteSheet.AddFrameSquence("Jumping");
            sequence.OriginOffset = -DRILLER_HITBOX.Origin - DRILLER_HITBOX.Mins;
            sequence.CollisionBox = DRILLER_HITBOX;
            sequence.AddFrame(-3, 6, 40, 4, 37, 30, 5);
            sequence.AddFrame(-7, 6, 78, 4, 35, 30, 5);
            sequence.AddFrame(4, -3, 115, 4, 43, 30, 1, true);

            // 2
            sequence = drillerSpriteSheet.AddFrameSquence("Landing");
            sequence.OriginOffset = -DRILLER_HITBOX.Origin - DRILLER_HITBOX.Mins;
            sequence.CollisionBox = DRILLER_HITBOX;
            sequence.AddFrame(-3, 6, 40, 4, 37, 30, 5);

            // 3
            sequence = drillerSpriteSheet.AddFrameSquence("Drilling");
            sequence.OriginOffset = -DRILLER_DRILLING_HITBOX.Origin - DRILLER_DRILLING_HITBOX.Mins;
            sequence.CollisionBox = DRILLER_DRILLING_HITBOX;
            sequence.AddFrame(-3, 0, 160, 10, 48, 24, 2, true);
            sequence.AddFrame(-4, 1, 209, 9, 46, 25, 2);
            sequence.AddFrame(-4, 0, 256, 10, 48, 24, 2);
            sequence.AddFrame(-4, 1, 305, 9, 46, 25, 2);

            // Bat
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XSharp.resources.sprites.Enemies.X2.mmx2-bat.png"))
            {
                var texture = CreateImageTextureFromStream(stream);
                batSpriteSheet.CurrentTexture = texture;
            }

            batSpriteSheet.CurrentPalette = batPalette;

            var batIdleHitbox = new MMXBox(Vector.NULL_VECTOR, new Vector(-6, -18), new Vector(6, 0));
            var batAttackingHitbox = new MMXBox(Vector.NULL_VECTOR, new Vector(-8, -14), new Vector(8, 0));

            // 0
            sequence = batSpriteSheet.AddFrameSquence("Idle");
            sequence.OriginOffset = -batIdleHitbox.Mins;
            sequence.CollisionBox = batIdleHitbox;
            sequence.AddFrame(0, 4, 7, 1, 14, 23, 1, true);

            // 1
            sequence = batSpriteSheet.AddFrameSquence("Attacking");
            sequence.OriginOffset = -batAttackingHitbox.Mins;
            sequence.CollisionBox = batAttackingHitbox;
            sequence.AddFrame(4, 7, 22, 1, 30, 23, 1, true);
            sequence.AddFrame(10, 8, 53, 1, 39, 23, 3);
            sequence.AddFrame(5, 6, 93, 1, 29, 23, 3);
            sequence.AddFrame(3, 2, 123, 1, 23, 23, 3);
            sequence.AddFrame(3, 5, 147, 1, 23, 23, 4);
            sequence.AddFrame(4, 7, 22, 1, 30, 23, 3);

            batSpriteSheet.ReleaseCurrentTexture();

            // Explosion
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XSharp.resources.sprites.Effects.Explosion.png"))
            {
                var texture = CreateImageTextureFromStream(stream);
                explosionSpriteSheet.CurrentTexture = texture;
            }

            sequence = explosionSpriteSheet.AddFrameSquence("Explosion");
            sequence.AddFrame(0, 0, 38, 48, 1, false, OriginPosition.CENTER);
            sequence.AddFrame(38, 0, 38, 48, 2, false, OriginPosition.CENTER);
            sequence.AddFrame(0, 0, 38, 48, 3, false, OriginPosition.CENTER);
            sequence.AddFrame(76, 0, 38, 48, 3, false, OriginPosition.CENTER);
            sequence.AddFrame(114, 0, 38, 48, 3, false, OriginPosition.CENTER);
            sequence.AddFrame(0, 48, 38, 48, 3, false, OriginPosition.CENTER);
            sequence.AddFrame(38, 48, 38, 48, 3, false, OriginPosition.CENTER);
            sequence.AddFrame(76, 48, 38, 48, 2, false, OriginPosition.CENTER);
            sequence.AddFrame(114, 48, 38, 48, 2, false, OriginPosition.CENTER);

            explosionSpriteSheet.ReleaseCurrentTexture();

            // HP
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XSharp.resources.sprites.HUD.HP.png"))
            {
                var texture = CreateImageTextureFromStream(stream);
                hpSpriteSheet.CurrentTexture = texture;
            }

            sequence = hpSpriteSheet.AddFrameSquence("HPTop");
            sequence.AddFrame(0, 0, 14, 4, 1, true, OriginPosition.LEFT_TOP);

            sequence = hpSpriteSheet.AddFrameSquence("HPBottom");
            sequence.AddFrame(0, 4, 14, 16, 1, true, OriginPosition.LEFT_TOP);

            sequence = hpSpriteSheet.AddFrameSquence("HPMiddle");
            sequence.AddFrame(0, 20, 14, 2, 1, true, OriginPosition.LEFT_TOP);

            sequence = hpSpriteSheet.AddFrameSquence("HPMiddleEmpty");
            sequence.AddFrame(0, 22, 14, 2, 1, true, OriginPosition.LEFT_TOP);

            sequence = hpSpriteSheet.AddFrameSquence("RideArmor");
            sequence.AddFrame(14, 0, 12, 11, 1, true, OriginPosition.LEFT_TOP);

            sequence = hpSpriteSheet.AddFrameSquence("Zero");
            sequence.AddFrame(26, 0, 12, 11, 1, true, OriginPosition.LEFT_TOP);

            sequence = hpSpriteSheet.AddFrameSquence("X1Boss");
            sequence.AddFrame(38, 0, 12, 11, 1, true, OriginPosition.LEFT_TOP);

            sequence = hpSpriteSheet.AddFrameSquence("Boss");
            sequence.AddFrame(50, 0, 12, 11, 1, true, OriginPosition.LEFT_TOP);

            sequence = hpSpriteSheet.AddFrameSquence("Doppler");
            sequence.AddFrame(14, 11, 12, 11, 1, true, OriginPosition.LEFT_TOP);

            sequence = hpSpriteSheet.AddFrameSquence("W");
            sequence.AddFrame(26, 11, 12, 11, 1, true, OriginPosition.LEFT_TOP);

            sequence = hpSpriteSheet.AddFrameSquence("DopplerPrototype");
            sequence.AddFrame(38, 11, 12, 11, 1, true, OriginPosition.LEFT_TOP);

            sequence = hpSpriteSheet.AddFrameSquence("X");
            sequence.AddFrame(50, 11, 12, 11, 1, true, OriginPosition.LEFT_TOP);

            hpSpriteSheet.ReleaseCurrentTexture();

            // Ready
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XSharp.resources.sprites.HUD.Ready.png"))
            {
                var texture = CreateImageTextureFromStream(stream);
                readySpriteSheet.CurrentTexture = texture;
            }

            sequence = readySpriteSheet.AddFrameSquence("Ready");
            sequence.AddFrame(5, 22, 8, 13, 1, false, OriginPosition.LEFT_TOP);
            sequence.AddFrame(21, 22, 16, 13, 2, false, OriginPosition.LEFT_TOP);
            sequence.AddFrame(45, 22, 16, 13, 2, false, OriginPosition.LEFT_TOP);
            sequence.AddFrame(68, 22, 24, 13, 2, false, OriginPosition.LEFT_TOP);
            sequence.AddFrame(107, 22, 24, 13, 2, false, OriginPosition.LEFT_TOP);
            sequence.AddFrame(139, 22, 31, 13, 2, false, OriginPosition.LEFT_TOP);
            sequence.AddFrame(181, 22, 30, 13, 2, false, OriginPosition.LEFT_TOP);
            sequence.AddFrame(220, 22, 39, 13, 2, false, OriginPosition.LEFT_TOP);
            sequence.AddFrame(267, 22, 39, 13, 2, false, OriginPosition.LEFT_TOP);
            sequence.AddFrame(314, 22, 39, 13, 2, false, OriginPosition.LEFT_TOP);
            sequence.AddFrame(367, 22, 39, 13, 2, false, OriginPosition.LEFT_TOP);
            sequence.AddFrame(413, 22, 39, 13, 10, false, OriginPosition.LEFT_TOP);
            sequence.AddFrame(0, 0, 1, 1, 8, false, OriginPosition.LEFT_TOP);
            sequence.AddFrame(413, 22, 39, 13, 8, false, OriginPosition.LEFT_TOP);
            sequence.AddFrame(0, 0, 1, 1, 8, false, OriginPosition.LEFT_TOP);
            sequence.AddFrame(413, 22, 39, 13, 8, false, OriginPosition.LEFT_TOP);
            sequence.AddFrame(0, 0, 1, 1, 8, false, OriginPosition.LEFT_TOP);
            sequence.AddFrame(413, 22, 39, 13, 8, false, OriginPosition.LEFT_TOP);
            sequence.AddFrame(0, 0, 1, 1, 8, false, OriginPosition.LEFT_TOP);
            sequence.AddFrame(413, 22, 39, 13, 8, false, OriginPosition.LEFT_TOP);
            sequence.AddFrame(0, 0, 1, 1, 8, false, OriginPosition.LEFT_TOP);
            sequence.AddFrame(413, 22, 39, 13, 8, false, OriginPosition.LEFT_TOP);
            sequence.AddFrame(0, 0, 1, 1, 8, false, OriginPosition.LEFT_TOP);
            sequence.AddFrame(413, 22, 39, 13, 9, false, OriginPosition.LEFT_TOP);

            readySpriteSheet.ReleaseCurrentTexture();

            // Ready
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XSharp.resources.sprites.Objects.BossDoor.png"))
            {
                var texture = CreateImageTextureFromStream(stream);
                bossDoorSpriteSheet.CurrentTexture = texture;
            }

            var bossDoorHitbox = new MMXBox(Vector.NULL_VECTOR, (-8, -23), (24, 25));

            sequence = bossDoorSpriteSheet.AddFrameSquence("Closed");
            sequence.OriginOffset = -bossDoorHitbox.Mins;
            sequence.CollisionBox = bossDoorHitbox;
            sequence.AddFrame(0, 0, 32, 0, 32, 48, 1, true);

            sequence = bossDoorSpriteSheet.AddFrameSquence("Opening");
            sequence.OriginOffset = -bossDoorHitbox.Mins;
            sequence.CollisionBox = bossDoorHitbox;
            sequence.AddFrame(0, 0, 64, 0, 32, 48, 4);
            sequence.AddFrame(0, 0, 96, 0, 32, 48, 2);
            sequence.AddFrame(0, 0, 128, 0, 32, 48, 4);
            sequence.AddFrame(0, 0, 64, 0, 32, 48, 2);
            sequence.AddFrame(0, 0, 160, 0, 32, 48, 4);
            sequence.AddFrame(0, 0, 192, 0, 32, 48, 4);
            sequence.AddFrame(0, 0, 224, 0, 32, 48, 4);
            sequence.AddFrame(0, 0, 64, 0, 32, 48, 4);
            sequence.AddFrame(0, 0, 160, 0, 32, 48, 2);
            sequence.AddFrame(0, 0, 192, 0, 32, 48, 4);
            sequence.AddFrame(0, 0, 224, 0, 32, 48, 2);
            sequence.AddFrame(0, 0, 96, 0, 32, 48, 2);
            sequence.AddFrame(0, 0, 128, 0, 32, 48, 4);
            sequence.AddFrame(0, 0, 96, 0, 32, 48, 2);
            sequence.AddFrame(0, 0, 256, 0, 32, 48, 4);
            sequence.AddFrame(0, 0, 288, 0, 32, 48, 4);

            sequence = bossDoorSpriteSheet.AddFrameSquence("PlayerCrossing");
            sequence.OriginOffset = -bossDoorHitbox.Mins;
            sequence.CollisionBox = bossDoorHitbox;
            sequence.AddFrame(0, 0, 32, 48, 1, true);

            sequence = bossDoorSpriteSheet.AddFrameSquence("Closing");
            sequence.OriginOffset = -bossDoorHitbox.Mins;
            sequence.CollisionBox = bossDoorHitbox;
            sequence.AddFrame(0, 0, 288, 0, 32, 48, 4);
            sequence.AddFrame(0, 0, 256, 0, 32, 48, 4);
            sequence.AddFrame(0, 0, 96, 0, 32, 48, 2);
            sequence.AddFrame(0, 0, 128, 0, 32, 48, 4);
            sequence.AddFrame(0, 0, 96, 0, 32, 48, 2);
            sequence.AddFrame(0, 0, 224, 0, 32, 48, 2);
            sequence.AddFrame(0, 0, 192, 0, 32, 48, 4);
            sequence.AddFrame(0, 0, 160, 0, 32, 48, 2);
            sequence.AddFrame(0, 0, 64, 0, 32, 48, 4);
            sequence.AddFrame(0, 0, 224, 0, 32, 48, 4);
            sequence.AddFrame(0, 0, 192, 0, 32, 48, 4);
            sequence.AddFrame(0, 0, 160, 0, 32, 48, 4);
            sequence.AddFrame(0, 0, 64, 0, 32, 48, 2);
            sequence.AddFrame(0, 0, 128, 0, 32, 48, 4);
            sequence.AddFrame(0, 0, 96, 0, 32, 48, 2);
            sequence.AddFrame(0, 0, 64, 0, 32, 48, 4);

            bossDoorSpriteSheet.ReleaseCurrentTexture();

            // Penguin
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XSharp.resources.sprites.Enemies.Bosses.X1.Penguin.png"))
            {
                var texture = CreateImageTextureFromStream(stream);
                penguinSpriteSheet.CurrentTexture = texture;
            }

            penguinSpriteSheet.CurrentPalette = penguinPalette;

            // 0
            sequence = penguinSpriteSheet.AddFrameSquence("FallingIntroducing");
            sequence.OriginOffset = -PENGUIN_COLLISION_BOX.Origin - PENGUIN_COLLISION_BOX.Mins;
            sequence.CollisionBox = PENGUIN_COLLISION_BOX;
            sequence.AddFrame(2, 15, 170, 20, 35, 44, 1, true);

            sequence = penguinSpriteSheet.AddFrameSquence("LandingIntroducing");
            sequence.OriginOffset = -PENGUIN_COLLISION_BOX.Origin - PENGUIN_COLLISION_BOX.Mins;
            sequence.CollisionBox = PENGUIN_COLLISION_BOX;
            sequence.AddFrame(7, 1, 136, 172, 39, 35, 6);
            sequence.AddFrame(6, -2, 96, 175, 38, 32, 6);

            sequence = penguinSpriteSheet.AddFrameSquence("Introducing");
            sequence.OriginOffset = -PENGUIN_COLLISION_BOX.Origin - PENGUIN_COLLISION_BOX.Mins;
            sequence.CollisionBox = PENGUIN_COLLISION_BOX;
            sequence.AddFrame(6, 2, 6, 177, 38, 36, 6);
            sequence.AddFrame(7, 0, 136, 172, 39, 35, 6);
            sequence.AddFrame(6, -2, 96, 175, 38, 32, 5);
            sequence.AddFrame(3, 3, 48, 76, 42, 37, 5);
            sequence.AddFrame(3, 3, 94, 76, 43, 37, 5);
            sequence.AddFrame(7, 2, 141, 77, 39, 36, 1, true);

            sequence = penguinSpriteSheet.AddFrameSquence("IntroducingEnd");
            sequence.OriginOffset = -PENGUIN_COLLISION_BOX.Origin - PENGUIN_COLLISION_BOX.Mins;
            sequence.CollisionBox = PENGUIN_COLLISION_BOX;
            sequence.AddFrame(6, 2, 184, 77, 38, 36, 1, true);

            sequence = penguinSpriteSheet.AddFrameSquence("Idle");
            sequence.OriginOffset = -PENGUIN_HITBOX.Origin - PENGUIN_HITBOX.Mins;
            sequence.CollisionBox = PENGUIN_HITBOX;
            sequence.AddFrame(10, 4, 6, 77, 38, 36, 15);
            sequence.AddFrame(10, 4, 184, 77, 38, 36, 15);
            sequence.AddFrame(10, 4, 6, 77, 38, 36, 15);
            sequence.AddFrame(10, 4, 184, 77, 38, 36, 15);
            sequence.AddFrame(10, 4, 6, 77, 38, 36, 7);
            sequence.AddFrame(11, 3, 136, 172, 39, 35, 7, true);
            sequence.AddFrame(10, 4, 6, 77, 38, 36, 7);

            sequence = penguinSpriteSheet.AddFrameSquence("ShootingIce");
            sequence.OriginOffset = -PENGUIN_HITBOX.Origin - PENGUIN_HITBOX.Mins;
            sequence.CollisionBox = PENGUIN_HITBOX;
            sequence.AddFrame(11, 3, 136, 172, 39, 35, 6);
            sequence.AddFrame(7, 5, 48, 76, 42, 37, 5);
            sequence.AddFrame(7, 5, 94, 76, 43, 37, 5);
            sequence.AddFrame(16, 3, 132, 129, 42, 35, 5);
            sequence.AddFrame(14, 3, 174, 129, 43, 35, 5);

            sequence = penguinSpriteSheet.AddFrameSquence("PreSliding");
            sequence.OriginOffset = -PENGUIN_HITBOX.Origin - PENGUIN_HITBOX.Mins;
            sequence.CollisionBox = PENGUIN_HITBOX;
            sequence.AddFrame(7, 5, 48, 76, 42, 37, 11);
            sequence.AddFrame(7, 5, 94, 76, 43, 37, 11);
            sequence.AddFrame(8, 3, 90, 130, 37, 34, 7);

            sequence = penguinSpriteSheet.AddFrameSquence("Sliding");
            sequence.OriginOffset = -PENGUIN_SLIDE_HITBOX.Origin - PENGUIN_SLIDE_HITBOX.Mins;
            sequence.CollisionBox = PENGUIN_SLIDE_HITBOX;
            sequence.AddFrame(0, 9, 221, 133, 40, 31, 1, true);

            sequence = penguinSpriteSheet.AddFrameSquence("Blowing");
            sequence.OriginOffset = -PENGUIN_HITBOX.Origin - PENGUIN_HITBOX.Mins;
            sequence.CollisionBox = PENGUIN_HITBOX;
            sequence.AddFrame(7, 5, 48, 76, 42, 37, 11);
            sequence.AddFrame(7, 5, 94, 76, 43, 37, 11);
            sequence.AddFrame(16, 3, 132, 129, 42, 35, 4, true);
            sequence.AddFrame(14, 3, 174, 129, 43, 35, 4);

            sequence = penguinSpriteSheet.AddFrameSquence("PreJumping");
            sequence.OriginOffset = -PENGUIN_HITBOX.Origin - PENGUIN_HITBOX.Mins;
            sequence.CollisionBox = PENGUIN_HITBOX;
            sequence.AddFrame(5, 0, 96, 175, 38, 32, 5);
            sequence.AddFrame(8, 6, 8, 127, 36, 38, 5);

            sequence = penguinSpriteSheet.AddFrameSquence("Jumping");
            sequence.OriginOffset = -PENGUIN_JUMP_HITBOX.Origin - PENGUIN_JUMP_HITBOX.Mins;
            sequence.CollisionBox = PENGUIN_JUMP_HITBOX;
            sequence.AddFrame(10, 4, 47, 127, 37, 38, 1, true);

            sequence = penguinSpriteSheet.AddFrameSquence("Falling");
            sequence.OriginOffset = -PENGUIN_HITBOX.Origin - PENGUIN_HITBOX.Mins;
            sequence.CollisionBox = PENGUIN_HITBOX;
            sequence.AddFrame(5, 14, 170, 20, 35, 44, 1, true);

            sequence = penguinSpriteSheet.AddFrameSquence("Landing");
            sequence.OriginOffset = -PENGUIN_HITBOX.Origin - PENGUIN_HITBOX.Mins;
            sequence.CollisionBox = PENGUIN_HITBOX;
            sequence.AddFrame(11, 3, 136, 172, 39, 35, 6);
            sequence.AddFrame(10, 0, 96, 175, 38, 32, 6);

            sequence = penguinSpriteSheet.AddFrameSquence("Hanging");
            sequence.OriginOffset = -PENGUIN_HITBOX.Origin - PENGUIN_HITBOX.Mins;
            sequence.CollisionBox = PENGUIN_HITBOX;
            sequence.AddFrame(5, 19, 12, 4, 36, 60, 8);
            sequence.AddFrame(5, 19, 54, 4, 34, 58, 8);
            sequence.AddFrame(5, 19, 95, 4, 32, 60, 8);
            sequence.AddFrame(6, 20, 131, 4, 32, 60, 1, true);

            sequence = penguinSpriteSheet.AddFrameSquence("TakingDamage");
            sequence.OriginOffset = -PENGUIN_TAKING_DAMAGE_HITBOX.Origin - PENGUIN_TAKING_DAMAGE_HITBOX.Mins;
            sequence.CollisionBox = PENGUIN_TAKING_DAMAGE_HITBOX;
            sequence.AddFrame(14, 4, 9, 169, 35, 41, 1, true);

            sequence = penguinSpriteSheet.AddFrameSquence("Dying");
            sequence.OriginOffset = -PENGUIN_TAKING_DAMAGE_HITBOX.Origin - PENGUIN_TAKING_DAMAGE_HITBOX.Mins;
            sequence.CollisionBox = PENGUIN_TAKING_DAMAGE_HITBOX;
            sequence.AddFrame(15, 3, 9, 169, 35, 41, 1, true);

            sequence = penguinSpriteSheet.AddFrameSquence("InFlames");
            sequence.OriginOffset = -PENGUIN_TAKING_DAMAGE_HITBOX.Origin - PENGUIN_TAKING_DAMAGE_HITBOX.Mins;
            sequence.CollisionBox = PENGUIN_TAKING_DAMAGE_HITBOX;
            sequence.AddFrame(16, 7, 52, 165, 38, 47, 21);

            penguinSpriteSheet.CurrentPalette = null;

            sequence = penguinSpriteSheet.AddFrameSquence("Ice");
            sequence.OriginOffset = -PENGUIN_ICE_HITBOX.Origin - PENGUIN_ICE_HITBOX.Mins;
            sequence.CollisionBox = PENGUIN_ICE_HITBOX;
            sequence.AddFrame(0, 2, 57, 232, 14, 14, 1, true);

            sequence = penguinSpriteSheet.AddFrameSquence("IceFragment");
            sequence.OriginOffset = -PENGUIN_ICE_FRAGMENT_HITBOX.Origin - PENGUIN_ICE_FRAGMENT_HITBOX.Mins;
            sequence.CollisionBox = PENGUIN_ICE_FRAGMENT_HITBOX;
            sequence.AddFrame(0, 0, 58, 216, 8, 8, 1, true);

            sequence = penguinSpriteSheet.AddFrameSquence("Sculpture");
            sequence.OriginOffset = -PENGUIN_SCULPTURE_HITBOX.Origin - PENGUIN_SCULPTURE_HITBOX.Mins;
            sequence.CollisionBox = PENGUIN_SCULPTURE_HITBOX;
            sequence.AddFrame(-2, -7, 82, 233, 13, 16, 19);
            sequence.AddFrame(2, -3, 104, 224, 19, 24, 19);
            sequence.AddFrame(4, -1, 183, 226, 23, 28, 19);
            sequence.AddFrame(5, 0, 133, 217, 28, 32, 1, true);

            sequence = penguinSpriteSheet.AddFrameSquence("Lever");
            sequence.OriginOffset = -PENGUIN_LEVER_HITBOX.Origin - PENGUIN_LEVER_HITBOX.Mins;
            sequence.CollisionBox = PENGUIN_LEVER_HITBOX;
            sequence.AddFrame(-8, -4, 169, 225, 10, 16, 1, true);

            sequence = penguinSpriteSheet.AddFrameSquence("Snow");
            sequence.OriginOffset = -PENGUIN_SNOW_HITBOX.Origin - PENGUIN_SNOW_HITBOX.Mins;
            sequence.CollisionBox = PENGUIN_SNOW_HITBOX;
            sequence.AddFrame(1, 1, 186, 172, 18, 18, 1, true);
            sequence.AddFrame(1, 1, 204, 172, 18, 18, 1);
            sequence.AddFrame(2, 2, 222, 172, 18, 18, 1);
            sequence.AddFrame(2, 2, 240, 172, 18, 18, 1);
            sequence.AddFrame(2, 2, 258, 172, 18, 18, 1);
            sequence.AddFrame(3, 3, 276, 172, 18, 18, 1);
            sequence.AddFrame(3, 2, 186, 190, 18, 18, 1);
            sequence.AddFrame(3, 2, 204, 190, 18, 18, 1);
            sequence.AddFrame(4, 3, 222, 190, 18, 18, 1);
            sequence.AddFrame(3, 3, 240, 190, 18, 18, 1);
            sequence.AddFrame(3, 3, 258, 190, 18, 18, 1);
            sequence.AddFrame(4, 4, 276, 190, 18, 18, 1);
            sequence.AddFrame(2, 3, 186, 208, 18, 18, 1);
            sequence.AddFrame(2, 3, 204, 208, 18, 18, 1);
            sequence.AddFrame(3, 4, 222, 208, 18, 18, 1);

            sequence = penguinSpriteSheet.AddFrameSquence("FrozenBlock");
            sequence.OriginOffset = -PENGUIN_FROZEN_BLOCK_HITBOX.Origin - PENGUIN_FROZEN_BLOCK_HITBOX.Mins;
            sequence.CollisionBox = PENGUIN_FROZEN_BLOCK_HITBOX;
            sequence.AddFrame(14, 3, 6, 216, 37, 38, 1, true);

            penguinSpriteSheet.ReleaseCurrentTexture();

            // Snow
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XSharp.resources.sprites.Effects.Mist.png"))
            {
                var texture = CreateImageTextureFromStream(stream);
                mistSpriteSheet.CurrentTexture = texture;
            }

            sequence = mistSpriteSheet.AddFrameSquence("Mist");
            sequence.OriginOffset = Vector.NULL_VECTOR;
            sequence.CollisionBox = (Vector.NULL_VECTOR, Vector.NULL_VECTOR, (SCENE_SIZE, SCENE_SIZE));
            sequence.AddFrame(0, 0, 0, 0, 256, 256, 1, true);

            mistSpriteSheet.ReleaseCurrentTexture();

            // Load tiles & object positions from the ROM (if exist)

            if (romLoaded)
            {
                mmx.SetLevel(mmx.Level, currentCheckpoint.Point, mmx.ObjLoad, mmx.TileLoad, mmx.PalLoad);
                mmx.LoadTilesAndPalettes();
                mmx.LoadPalette(this, false);
                mmx.LoadPalette(this, true);
                mmx.RefreshMapCache(this, false);
                mmx.RefreshMapCache(this, true);

                World.Tessellate();
            }

            for (var entity = firstEntity; entity != null; entity = entity.next)
                if (entity is Sprite sprite)
                    sprite.OnDeviceReset();
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
            drawMapBounds = section.DrawMapBounds;
            drawTouchingMapBounds = section.DrawTouchingMapBounds;
            drawHighlightedPointingTiles = section.DrawHighlightedPointingTiles;
            drawPlayerOriginAxis = section.DrawPlayerOriginAxis;
            showInfoText = section.ShowInfoText;
            showCheckpointBounds = section.ShowCheckpointBounds;
            showTriggerBounds = section.ShowTriggerBounds;
            showTriggerCameraLook = section.ShowTriggerCameraLook;
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
            section.DrawMapBounds = drawMapBounds;
            section.DrawTouchingMapBounds = drawTouchingMapBounds;
            section.DrawHighlightedPointingTiles = drawHighlightedPointingTiles;
            section.DrawPlayerOriginAxis = drawPlayerOriginAxis;
            section.ShowInfoText = showInfoText;
            section.ShowCheckpointBounds = showCheckpointBounds;
            section.ShowTriggerBounds = showTriggerBounds;
            section.ShowTriggerCameraLook = showTriggerCameraLook;

            config.Save();
        }

        private void AddChargingEffectFrames(SpriteSheet.FrameSequence sequence, int level)
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
            FadingOSTInitialVolume = soundChannels[3].player.Volume;
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

        private static void FillRegion(DataRectangle dataRect, int length, MMXBox box, int paletteIndex)
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
            FillRegion(dataRect, length, new MMXBox(point.X, point.Y, 1, 1), 1);
        }

        private static void DrawChargingPointLevel1Large(DataRectangle dataRect, int length, Vector point)
        {
            FillRegion(dataRect, length, new MMXBox(point.X, point.Y, 2, 2), 1);
        }

        private static void DrawChargingPointLevel2Small1(DataRectangle dataRect, int length, Vector point)
        {
            FillRegion(dataRect, length, new MMXBox(point.X, point.Y, 1, 1), 2);
        }

        private static void DrawChargingPointLevel2Small2(DataRectangle dataRect, int length, Vector point)
        {
            FillRegion(dataRect, length, new MMXBox(point.X, point.Y, 1, 1), 3);

            FillRegion(dataRect, length, new MMXBox(point.X, point.Y - 1, 1, 1), 4);
            FillRegion(dataRect, length, new MMXBox(point.X, point.Y + 1, 1, 1), 4);
            FillRegion(dataRect, length, new MMXBox(point.X - 1, point.Y, 1, 1), 4);
            FillRegion(dataRect, length, new MMXBox(point.X + 1, point.Y, 1, 1), 4);

            FillRegion(dataRect, length, new MMXBox(point.X - 1, point.Y - 1, 1, 1), 5);
            FillRegion(dataRect, length, new MMXBox(point.X + 1, point.Y - 1, 1, 1), 5);
            FillRegion(dataRect, length, new MMXBox(point.X - 1, point.Y + 1, 1, 1), 5);
            FillRegion(dataRect, length, new MMXBox(point.X + 1, point.Y + 1, 1, 1), 5);
        }

        private static void DrawChargingPointLevel2Large1(DataRectangle dataRect, int length, Vector point)
        {
            FillRegion(dataRect, length, new MMXBox(point.X, point.Y, 2, 2), 2);
        }

        private static void DrawChargingPointLevel2Large2(DataRectangle dataRect, int length, Vector point)
        {
            FillRegion(dataRect, length, new MMXBox(point.X, point.Y, 2, 2), 3);

            FillRegion(dataRect, length, new MMXBox(point.X, point.Y - 1, 2, 1), 4);
            FillRegion(dataRect, length, new MMXBox(point.X, point.Y + 2, 2, 1), 4);
            FillRegion(dataRect, length, new MMXBox(point.X - 1, point.Y, 1, 2), 4);
            FillRegion(dataRect, length, new MMXBox(point.X + 2, point.Y, 1, 2), 4);
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

        private Texture CreateChargingTexture(Vector[] points, bool[] large, int[] types, int level)
        {
            int width1 = (int) NextHighestPowerOfTwo(CHARGING_EFFECT_HITBOX_SIZE);
            int height1 = (int) NextHighestPowerOfTwo(CHARGING_EFFECT_HITBOX_SIZE);
            int length = width1 * height1;

            var result = new Texture(Device, width1, height1, 1, Usage.None, Format.L8, Pool.Managed);
            DataRectangle rect = result.LockRectangle(0, D3D9LockFlags.Discard);

            ZeroDataRect(rect, length);

            for (int i = 0; i < points.Length; i++)
                if (large[i])
                    DrawChargingPointLarge(rect, length, points[i], level, types[i]);
                else
                    DrawChargingPointSmall(rect, length, points[i], level, types[i]);

            result.UnlockRectangle(0);
            return result;
        }

        public Texture CreateImageTextureFromFile(string filePath)
        {
            var result = Texture.FromFile(Device, filePath, Usage.None, Pool.SystemMemory);
            return result;
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

        public void RenderSprite(Texture texture, Texture palette, FadingControl fadingSettings, MMXBox box, Matrix transform, int repeatX = 1, int repeatY = 1)
        {
            RectangleF rDest = WorldBoxToScreen(box);

            //var matScaling = Matrix.Scaling(4, 4, 1);
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

            if (fadingSettings != null)
            {
                shader.Function.ConstantTable.SetValue(Device, fadingLevelHandle, fadingSettings.FadingLevel);
                shader.Function.ConstantTable.SetValue(Device, fadingColorHandle, fadingSettings.FadingColor.ToVector4());
            }
            else
                shader.Function.ConstantTable.SetValue(Device, fadingLevelHandle, Vector4.Zero);

            for (int i = 0; i < repeatX; i++)
                for (int j = 0; j < repeatY; j++)
                {
                    float x = rDest.Left + i * rDest.Width;
                    float y = rDest.Top + j * rDest.Height;
                    var matTranslation = Matrix.Translation(x, y, 0);
                    Matrix matTransform = matTranslation * transform * matScaling;
                    sprite.Transform = matTransform;
                    sprite.Draw(texture, Color.FromRgba(0xffffffff));
                }

            sprite.End();
        }

        public void RenderSprite(Texture texture, FadingControl fadingSettings, MMXBox box, Matrix transform, int repeatX = 1, int repeatY = 1)
        {
            RenderSprite(texture, null, fadingSettings, box, transform, repeatX, repeatY);
        }

        public void RenderSprite(Texture texture, FadingControl fadingSettings, Vector v, Matrix transform, int repeatX = 1, int repeatY = 1)
        {
            var description = texture.GetLevelDescription(0);
            RenderSprite(texture, null, fadingSettings, new MMXBox(v.X, v.Y, description.Width, description.Height), transform, repeatX, repeatY);
        }

        public void RenderSprite(Texture texture, FadingControl fadingSettings, FixedSingle x, FixedSingle y, Matrix transform, int repeatX = 1, int repeatY = 1)
        {
            var description = texture.GetLevelDescription(0);
            RenderSprite(texture, null, fadingSettings, new MMXBox(x, y, description.Width, description.Height), transform, repeatX, repeatY);
        }

        public void RenderSprite(Texture texture, Texture palette, FadingControl fadingSettings, Vector v, Matrix transform, int repeatX = 1, int repeatY = 1)
        {
            var description = texture.GetLevelDescription(0);
            RenderSprite(texture, palette, fadingSettings, new MMXBox(v.X, v.Y, description.Width, description.Height), transform, repeatX, repeatY);
        }

        public void RenderSprite(Texture texture, Texture palette, FadingControl fadingSettings, FixedSingle x, FixedSingle y, Matrix transform, int repeatX = 1, int repeatY = 1)
        {
            var description = texture.GetLevelDescription(0);
            RenderSprite(texture, palette, fadingSettings, new MMXBox(x, y, description.Width, description.Height), transform, repeatX, repeatY);
        }

        public T CreateEntity<T>(dynamic initParams) where T : Entity
        {
            Type type = typeof(T);
            var entity = Activator.CreateInstance<T>();

            if (initParams != null)
            {
                Type attrsType = initParams.GetType();
                System.Reflection.PropertyInfo[] attributes = attrsType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (var attr in attributes)
                {
                    string attrName = attr.Name;
                    object value = attr.GetValue(initParams);

                    var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(prop => prop.Name == attrName);
                    var property = properties.FirstOrDefault(prop => prop.DeclaringType == type) ?? properties.First();
                    if (property == null)
                        throw new ArgumentException($"Attribute '{attr}' in entity class '{type.Name}' doesn't exist or isn't public.");

                    if (!property.CanWrite)
                        throw new ArgumentException($"Attribute '{attr}' is not writable.");

                    Type propertyType = property.PropertyType;
                    Type valueType = value.GetType();
                    if (valueType != propertyType && !propertyType.IsAssignableFrom(valueType))
                    {
                        TypeConverter conv = TypeDescriptor.GetConverter(propertyType);
                        value = conv.ConvertFrom(value);
                    }

                    property.SetValue(entity, value);
                }
            }

            AddEntity(entity);
            return entity;
        }

        public T CreateEntity<T>() where T : Entity
        {
            return CreateEntity<T>(null);
        }

        private void AddEntity(Entity entity)
        {
            if (entity.Index >= 0)
                return;

            if (entityCount == MAX_ENTITIES)
                throw new IndexOutOfRangeException("Max entities reached the limit.");

            if (lastEntity != null)
                lastEntity.next = entity;

            entity.previous = lastEntity;
            entity.next = null;

            firstEntity ??= entity;
            lastEntity = entity;

            entity.Index = firstFreeEntityIndex;
            entities[firstFreeEntityIndex++] = entity;

            if (entity.Name is not null and not "")
                entitiesByName.Add(entity.Name, entity);

            entityCount++;

            for (int i = firstFreeEntityIndex; i < MAX_ENTITIES; i++)
                if (entities[i] == null)
                {
                    firstFreeEntityIndex = i;
                    break;
                }
        }

        private void RemoveEntity(Entity entity)
        {
            int index = entity.Index;

            var next = entity.next;
            var previous = entity.previous;

            if (next != null)
                next.previous = previous;

            if (previous != null)
                previous.next = next;

            if (entity == firstEntity)
                firstEntity = next;

            if (entity == lastEntity)
                lastEntity = previous;

            entities[index] = null;

            if (entity.Name is not null and not "")
                entitiesByName.Remove(entity.Name);

            entity.Index = -1;
            entityCount--;

            if (index < firstFreeEntityIndex)
                firstFreeEntityIndex = index;
        }

        private void ClearEntities()
        {
            for (int i = 0; i < MAX_ENTITIES; i++)
            {
                var entity = entities[i];
                if (entity != null)
                {
                    foreach (Entity child in entity.childs)
                        child.parent = null;

                    entity.childs.Clear();
                    entities[i] = null;
                }
            }

            foreach (var layer in sprites)
                layer.Clear();

            foreach (var layer in huds)
                layer.Clear();

            autoRespawnableEntities.Clear();
            spawnedEntities.Clear();
            removedEntities.Clear();
            entitiesByName.Clear();
            freezingSpriteExceptions.Clear();
            partition.Clear();

            firstFreeEntityIndex = 0;
            entityCount = 0;
            firstEntity = null;
            lastEntity = null;
        }

        private void OnSpawningBlackScreenComplete()
        {
            FadingOST = false;
            FadingOSTLevel = 0;
            soundChannels[3].player.Volume = 0.5f;

            if (ENABLE_OST && romLoaded && mmx.Type == 0 && mmx.Level == 8)
                PlayOST(18, 83, 50.152);

            FadingSettings.Reset();
            FadingSettings.Start(Color.Black, 26, FadingFlags.COLORS, FadingFlags.COLORS, StartReadyHUD);
        }

        private Keys HandleInput(out bool nextFrame)
        {
            Keys keys = Keys.NONE;
            nextFrame = !frameAdvance;

            if (Control.Focused)
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

                if (state.IsPressed(Key.Backslash) && state.IsPressed(Key.LeftControl))
                {
                    if (!wasPressingToggleFrameAdvance)
                    {
                        wasPressingToggleFrameAdvance = true;
                        frameAdvance = !frameAdvance;
                        nextFrame = !frameAdvance;
                    }
                }
                else
                    wasPressingToggleFrameAdvance = false;

                if (state.IsPressed(Key.Backslash) && !state.IsPressed(Key.LeftControl))
                {
                    if (!wasPressingNextFrame)
                    {
                        wasPressingNextFrame = true;
                        frameAdvance = true;
                        nextFrame = true;
                    }
                }
                else
                    wasPressingNextFrame = false;

                if (state.IsPressed(Key.F5) && !state.IsPressed(Key.LeftShift))
                {
                    if (!wasPressingSaveState)
                    {
                        wasPressingSaveState = true;
                        SaveState();
                    }
                }
                else
                    wasPressingSaveState = false;

                if (state.IsPressed(Key.F7) && !state.IsPressed(Key.LeftShift))
                {
                    if (!wasPressingLoadState)
                    {
                        wasPressingLoadState = true;
                        LoadState();
                    }
                }
                else
                    wasPressingLoadState = false;

                if (state.IsPressed(Key.Equals))
                {
                    if (!wasPressingNextSlot)
                    {
                        wasPressingNextSlot = true;
                        currentSaveSlot++;
                        if (currentSaveSlot >= SAVE_SLOT_COUNT)
                            currentSaveSlot = 0;
                    }
                }
                else
                    wasPressingNextSlot = false;

                if (state.IsPressed(Key.Minus))
                {
                    if (!wasPressingPreviousSlot)
                    {
                        wasPressingPreviousSlot = true;
                        currentSaveSlot--;
                        if (currentSaveSlot < 0)
                            currentSaveSlot = SAVE_SLOT_COUNT - 1;
                    }
                }
                else
                    wasPressingPreviousSlot = false;

                if (state.IsPressed(Key.N))
                {
                    if (!wasPressingToggleNoClip)
                    {
                        wasPressingToggleNoClip = true;
                        if (Player != null)
                            Player.NoClip = !Player.NoClip;
                    }
                }
                else
                    wasPressingToggleNoClip = false;

                if (state.IsPressed(Key.M))
                {
                    if (!wasPressingToggleCameraConstraints)
                    {
                        wasPressingToggleCameraConstraints = true;
                        NoCameraConstraints = !NoCameraConstraints;
                    }
                }
                else
                    wasPressingToggleCameraConstraints = false;

                if (state.IsPressed(Key.D1))
                {
                    if (!wasPressingToggleDrawCollisionBox)
                    {
                        wasPressingToggleDrawCollisionBox = true;
                        drawHitbox = !drawHitbox;
                    }
                }
                else
                    wasPressingToggleDrawCollisionBox = false;

                if (state.IsPressed(Key.D2))
                {
                    if (!wasPressingToggleShowColliders)
                    {
                        wasPressingToggleShowColliders = true;
                        showColliders = !showColliders;
                    }
                }
                else
                    wasPressingToggleShowColliders = false;

                if (state.IsPressed(Key.D3))
                {
                    if (!wasPressingToggleDrawMapBounds)
                    {
                        wasPressingToggleDrawMapBounds = true;
                        drawMapBounds = !drawMapBounds;
                    }
                }
                else
                    wasPressingToggleDrawMapBounds = false;

                if (state.IsPressed(Key.D4))
                {
                    if (!wasPressingToggleDrawTouchingMapBounds)
                    {
                        wasPressingToggleDrawTouchingMapBounds = true;
                        drawTouchingMapBounds = !drawTouchingMapBounds;
                    }
                }
                else
                    wasPressingToggleDrawTouchingMapBounds = false;

                if (state.IsPressed(Key.D5))
                {
                    if (!wasPressingToggleDrawHighlightedPointingTiles)
                    {
                        wasPressingToggleDrawHighlightedPointingTiles = true;
                        drawHighlightedPointingTiles = !drawHighlightedPointingTiles;
                    }
                }
                else
                    wasPressingToggleDrawHighlightedPointingTiles = false;

                if (state.IsPressed(Key.D6))
                {
                    if (!wasPressingToggleDrawPlayerOriginAxis)
                    {
                        wasPressingToggleDrawPlayerOriginAxis = true;
                        drawPlayerOriginAxis = !drawPlayerOriginAxis;
                    }
                }
                else
                    wasPressingToggleDrawPlayerOriginAxis = false;

                if (state.IsPressed(Key.D7))
                {
                    if (!wasPressingToggleShowInfoText)
                    {
                        wasPressingToggleShowInfoText = true;
                        showInfoText = !showInfoText;
                    }
                }
                else
                    wasPressingToggleShowInfoText = false;

                if (state.IsPressed(Key.D8))
                {
                    if (!wasPressingToggleShowCheckpointBounds)
                    {
                        wasPressingToggleShowCheckpointBounds = true;
                        showCheckpointBounds = !showCheckpointBounds;
                    }
                }
                else
                    wasPressingToggleShowCheckpointBounds = false;

                if (state.IsPressed(Key.D9))
                {
                    if (!wasPressingToggleShowTriggerBounds)
                    {
                        wasPressingToggleShowTriggerBounds = true;
                        showTriggerBounds = !showTriggerBounds;
                    }
                }
                else
                    wasPressingToggleShowTriggerBounds = false;

                if (state.IsPressed(Key.D0))
                {
                    if (!wasPressingToggleShowTriggerCameraLook)
                    {
                        wasPressingToggleShowTriggerCameraLook = true;
                        showTriggerCameraLook = !showTriggerCameraLook;
                    }
                }
                else
                    wasPressingToggleShowTriggerCameraLook = false;

                if (state.IsPressed(Key.F1))
                {
                    if (!wasPressingToggleDrawBackground)
                    {
                        wasPressingToggleDrawBackground = true;
                        drawBackground = !drawBackground;
                    }
                }
                else
                    wasPressingToggleDrawBackground = false;

                if (state.IsPressed(Key.F2))
                {
                    if (!wasPressingToggleDrawDownLayer)
                    {
                        wasPressingToggleDrawDownLayer = true;
                        drawDownLayer = !drawDownLayer;
                    }
                }
                else
                    wasPressingToggleDrawDownLayer = false;

                if (state.IsPressed(Key.F3))
                {
                    if (!wasPressingToggleDrawUpLayer)
                    {
                        wasPressingToggleDrawUpLayer = true;
                        drawUpLayer = !drawUpLayer;
                    }
                }
                else
                    wasPressingToggleDrawUpLayer = false;

                if (state.IsPressed(Key.F4))
                {
                    if (!wasPressingToggleDrawSprites)
                    {
                        wasPressingToggleDrawSprites = true;
                        drawSprites = !drawSprites;
                    }
                }
                else
                    wasPressingToggleDrawSprites = false;

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
            FadingSettings.OnFrame();

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

                    soundChannels[3].player.Volume = FadingOSTInitialVolume * (1 - FadingOSTLevel) + FadingOSTVolume * FadingOSTLevel;
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
                if (!Player.DyeByAbiss)
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
                    FreezingFrameCounter++;
            }

            if (DelayedAction != null)
            {
                if (DelayedActionFrameCounter >= DelayedActionFrames)
                {
                    DelayedAction?.Invoke();
                    DelayedAction = null;
                }
                else
                    DelayedActionFrameCounter++;
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
                    FreezingSpritesFrameCounter++;
            }
        }

        private bool OnFrame()
        {
            Keys keys = HandleInput(out bool nextFrame);

            if (!nextFrame)
                return false;

            FrameCounter++;

            //lua.DoString("if engine.Player ~= null then engine.Player:ShootLemon() end");

            HandleScreenEffects();

            if (!loadingLevel)
            {
                if (spawnedEntities.Count > 0)
                {
                    for (int i = 0; i < spawnedEntities.Count; i++)
                    {
                        var added = spawnedEntities[i];

                        if (added is Sprite sprite)
                        {
                            if (sprite is HUD hud)
                                huds[sprite.Layer].Add(hud);
                            else
                                sprites[sprite.Layer].Add(sprite);
                        }

                        added.OnSpawn();
                        added.PostSpawn();
                    }

                    spawnedEntities.Clear();
                }

                Player?.PushKeys(keys);

                if (Freezing)
                    return false;

                if (paused || Player != null && Player.Freezed && (!FreezingSprites || freezingSpriteExceptions.Contains(Player)))
                {
                    Player?.OnFrame();
                    World.Camera.OnFrame();
                }
                else
                {
                    if (FreezingSprites)
                    {
                        for (var entity = firstEntity; entity != null; entity = entity.next)
                            if (entity is not Sprite sprite || freezingSpriteExceptions.Contains(sprite))
                                if (entity.Alive && entity != World.Camera)
                                    entity.OnFrame();
                    }
                    else
                    {
                        for (var entity = firstEntity; entity != null; entity = entity.next)
                            if (entity.Alive && entity != World.Camera)
                                entity.OnFrame();
                    }

                    World.Camera.OnFrame();

                    if (removedEntities.Count > 0)
                    {
                        for (int i = 0; i < removedEntities.Count; i++)
                        {
                            var removed = removedEntities[i];

                            if (removed is Sprite sprite)
                            {
                                if (sprite is HUD hud)
                                    huds[sprite.Layer].Remove(hud);
                                else
                                    sprites[sprite.Layer].Remove(sprite);
                            }

                            removed.Cleanup();
                            removed.Alive = false;
                            removed.Dead = true;
                            removed.DeathFrame = FrameCounter;

                            if (!removed.Respawnable)
                            {
                                Engine.partition.Remove(removed);
                                RemoveEntity(removed);
                            }
                            else if (removed.RespawnOnNear)
                                removed.Origin = autoRespawnableEntities[removed].Origin;
                        }

                        removedEntities.Clear();
                    }
                }

                World.OnFrame();
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

        public static Rectangle ToRectangle(MMXBox box)
        {
            return new((int) box.Left, (int) box.Top, (int) box.Width, (int) box.Height);
        }

        public static RectangleF ToRectangleF(MMXBox box)
        {
            return new((float) box.Left, (float) box.Top, (float) box.Width, (float) box.Height);
        }

        public Vector2 WorldVectorToScreen(Vector v)
        {
            return ToVector2((v - World.Camera.LeftTop) * DrawScale + drawBox.LeftTop);
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
            return (new Vector(p.X, p.Y) - drawBox.LeftTop) / DrawScale + World.Camera.LeftTop;
        }

        public Vector ScreenVector2ToWorld(Vector2 v)
        {
            return (new Vector(v.X, v.Y) - drawBox.LeftTop) / DrawScale + World.Camera.LeftTop;
        }

        public RectangleF WorldBoxToScreen(MMXBox box)
        {
            return ToRectangleF((box.LeftTopOrigin() - World.Camera.LeftTop) * DrawScale + drawBox.LeftTop);
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

            World.Camera.RightTop = cameraPos;

            SpawnX(spawnPos);
            CreateHP();

            World.Camera.FocusOn = Player;

            CurrentCheckpoint = checkpoints.Count > 0 ? checkpoints[mmx.Point] : null;
        }

        public void LoadLevel()
        {
            LoadLevel(currentLevel, (ushort) (CurrentCheckpoint != null ? CurrentCheckpoint.Index : 0));
        }

        public void LoadLevel(ushort level, ushort checkpoint = 0)
        {
            paused = false;
            lock (this)
            {
                loadingLevel = true;

                UnloadLevel();

                currentLevel = level;

                if (romLoaded)
                {
                    mmx.SetLevel(level, checkpoint);

                    mmx.LoadLevel();
                    mmx.LoadEvents(this);
                    mmx.LoadToWorld(this, false);

                    mmx.LoadBackground();
                    mmx.LoadToWorld(this, true);

                    CurrentCheckpoint = checkpoints[mmx.Point];
                    CameraConstraintsBox = CurrentCheckpoint.Hitbox;
                }
                else
                    CameraConstraintsBox = World.BoundingBox;

                World.Tessellate();

                loadingLevel = false;
                respawning = false;

                World.Camera.Spawn();

                if (enableSpawningBlackScreen)
                {
                    SpawningBlackScreen = true;
                    SpawningBlackScreenFrameCounter = 0;
                }
                else
                    OnSpawningBlackScreenComplete();

                World.Camera.FocusOn = null;
                var cameraPos = romLoaded ? mmx.CameraPos : Vector.NULL_VECTOR;
                World.Camera.LeftTop = cameraPos;
            }
        }

        private void UnloadLevel()
        {
            ClearEntities();

            DisposeResource(World);

            checkpoints.Clear();
            autoRespawnableEntities.Clear();
            partition.Clear();

            lastLives = Player != null ? Player.Lives : X_INITIAL_LIVES;
            Player = null;
            ReadyHUD = null;
            HP = null;

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

            drawBox = new MMXBox(drawOrigin.X, drawOrigin.Y, width, height);
        }

        private void DrawSlopeMap(MMXBox box, RightTriangle triangle, float strokeWidth)
        {
            Vector tv1 = triangle.Origin;
            Vector tv2 = triangle.VCathetusOpositeVertex;
            Vector tv3 = triangle.HCathetusOpositeVertex;

            DrawLine(tv2, tv3, strokeWidth, TOUCHING_MAP_COLOR);

            FixedSingle h = tv1.Y - box.Top;
            FixedSingle H = MAP_SIZE - h;
            if (H > 0)
            {
                if (triangle.HCathetusVector.X < 0)
                {
                    DrawLine(tv2, box.LeftBottom, strokeWidth, TOUCHING_MAP_COLOR);
                    DrawLine(tv3, box.RightBottom, strokeWidth, TOUCHING_MAP_COLOR);
                }
                else
                {
                    DrawLine(tv3, box.LeftBottom, strokeWidth, TOUCHING_MAP_COLOR);
                    DrawLine(tv2, box.RightBottom, strokeWidth, TOUCHING_MAP_COLOR);
                }

                DrawLine(box.LeftBottom, box.RightBottom, strokeWidth, TOUCHING_MAP_COLOR);
            }
            else
            {
                DrawLine(tv3, tv1, strokeWidth, TOUCHING_MAP_COLOR);
                DrawLine(tv1, tv2, strokeWidth, TOUCHING_MAP_COLOR);
            }
        }

        private void DrawHighlightMap(int row, int col, CollisionData collisionData)
        {
            MMXBox mapBox = GetMapBoundingBox(row, col);
            if (collisionData.IsSolidBlock())
                DrawRectangle(mapBox, 4, TOUCHING_MAP_COLOR);
            else if (collisionData.IsSlope())
            {
                RightTriangle st = collisionData.MakeSlopeTriangle() + mapBox.LeftTop;
                DrawSlopeMap(mapBox, st, 4);
            }
        }

        private void CheckAndDrawTouchingMap(int row, int col, CollisionData collisionData, MMXBox collisionBox, bool ignoreSlopes = false)
        {
            var halfCollisionBox1 = new MMXBox(collisionBox.Left, collisionBox.Top, collisionBox.Width * 0.5, collisionBox.Height);
            var halfCollisionBox2 = new MMXBox(collisionBox.Left + collisionBox.Width * 0.5, collisionBox.Top, collisionBox.Width * 0.5, collisionBox.Height);

            MMXBox mapBox = GetMapBoundingBox(row, col);
            if (collisionData.IsSolidBlock() && CollisionChecker.HasIntersection(mapBox, collisionBox))
                DrawRectangle(mapBox, 4, TOUCHING_MAP_COLOR);
            else if (!ignoreSlopes && collisionData.IsSlope())
            {
                RightTriangle st = collisionData.MakeSlopeTriangle() + mapBox.LeftTop;
                Vector hv = st.HCathetusVector;
                if (hv.X > 0 && st.HasIntersectionWith(halfCollisionBox2, EPSLON) || hv.X < 0 && st.HasIntersectionWith(halfCollisionBox1, EPSLON))
                    DrawSlopeMap(mapBox, st, 4);
            }
        }

        private void CheckAndDrawTouchingMaps(MMXBox collisionBox, bool ignoreSlopes = false)
        {
            Cell start = GetMapCellFromPos(collisionBox.LeftTop);
            Cell end = GetMapCellFromPos(collisionBox.RightBottom);

            int startRow = start.Row;
            int startCol = start.Col;

            if (startRow < 0)
                startRow = 0;

            if (startRow >= World.MapRowCount)
                startRow = World.MapRowCount - 1;

            if (startCol < 0)
                startCol = 0;

            if (startCol >= World.MapColCount)
                startCol = World.MapColCount - 1;

            int endRow = end.Row;
            int endCol = end.Col;

            if (endRow < 0)
                endRow = 0;

            if (endRow >= World.MapRowCount)
                endRow = World.MapRowCount - 1;

            if (endCol < 0)
                endCol = 0;

            if (endCol >= World.MapColCount)
                endCol = World.MapColCount - 1;

            for (int row = startRow; row <= endRow; row++)
                for (int col = startCol; col <= endCol; col++)
                {
                    var v = new Vector(col * MAP_SIZE, row * MAP_SIZE);
                    Map map = World.GetMapFrom(v);
                    if (map != null)
                        CheckAndDrawTouchingMap(row, col, map.CollisionData, collisionBox, ignoreSlopes);
                }
        }

        public void DrawLine(Vector from, Vector to, float width, Color color)
        {
            DrawLine(WorldVectorToScreen(from), WorldVectorToScreen(to), width, color);
        }

        public void DrawLine(Vector2 from, Vector2 to, float width, Color color)
        {
            from *= 4;
            to *= 4;

            line.Width = width;

            line.Begin();
            line.Draw(new Vector2[] { from, to }, color);
            line.End();
        }

        public void DrawRectangle(MMXBox box, float borderWith, Color color)
        {
            DrawRectangle(WorldBoxToScreen(box), borderWith, color);
        }

        public void DrawRectangle(RectangleF rect, float borderWith, Color color)
        {
            rect = new RectangleF(rect.X * 4, rect.Y * 4, rect.Width * 4, rect.Height * 4);

            line.Width = borderWith;

            line.Begin();
            line.Draw(new Vector2[] { rect.TopLeft, rect.TopRight, rect.BottomRight, rect.BottomLeft, rect.TopLeft }, color);
            line.End();
        }

        public void FillRectangle(MMXBox box, Color color)
        {
            FillRectangle(WorldBoxToScreen(box), color);
        }

        public void FillRectangle(RectangleF rect, Color color, FadingControl fadingSettings = null)
        {
            float x = 4 * rect.Left;
            float y = 4 * rect.Top;

            var matScaling = Matrix.Scaling(4 * rect.Width, 4 * rect.Height, 1);
            var matTranslation = Matrix.Translation(x, y, 0);
            Matrix matTransform = matScaling * matTranslation;

            sprite.Begin(SpriteFlags.AlphaBlend);

            Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Point);
            Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Point);
           
            if (fadingSettings != null)
            {
                Device.PixelShader = PixelShader;
                PixelShader.Function.ConstantTable.SetValue(Device, psFadingLevelHandle, fadingSettings.FadingLevel);
                PixelShader.Function.ConstantTable.SetValue(Device, psFadingColorHandle, fadingSettings.FadingColor.ToVector4());
            }
            else
                Device.PixelShader = null;

            Device.VertexShader = null;

            sprite.Transform = matTransform;

            sprite.Draw(color == Color.Black ? blackPixelTexture : whitePixelTexture, color);
            sprite.End();
        }

        public void DrawText(string text, Font font, RectangleF drawRect, FontDrawFlags drawFlags, Color color)
        {
            DrawText(text, font, drawRect, drawFlags, Matrix.Identity, color, out _);
        }

        public void DrawText(string text, Font font, RectangleF drawRect, FontDrawFlags drawFlags, Color color, out RawRectangle fontDimension)
        {
            DrawText(text, font, drawRect, drawFlags, Matrix.Identity, color, out fontDimension);
        }

        public void DrawText(string text, Font font, RectangleF drawRect, FontDrawFlags drawFlags, RawMatrix transform, Color color)
        {
            DrawText(text, font, drawRect, drawFlags, transform, color, out _);
        }

        public void DrawText(string text, Font font, RectangleF drawRect, FontDrawFlags drawFlags, float offsetX, float offsetY, Color color)
        {
            RawMatrix transform = Matrix.Translation(offsetX, offsetY, 0);
            DrawText(text, font, drawRect, drawFlags, transform, color, out _);
        }

        public void DrawText(string text, Font font, RectangleF drawRect, FontDrawFlags drawFlags, float offsetX, float offsetY, Color color, out RawRectangle fontDimension)
        {
            RawMatrix transform = Matrix.Translation(offsetX, offsetY, 0);
            DrawText(text, font, drawRect, drawFlags, transform, color, out fontDimension);
        }

        public void DrawText(string text, Font font, RectangleF drawRect, FontDrawFlags drawFlags, RawMatrix transform, Color color, out RawRectangle fontDimension)
        {
            sprite.Begin();

            Device.VertexShader = null;
            Device.PixelShader = null;

            Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Point);
            Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Point);
            sprite.Transform = transform;

            fontDimension = font.MeasureText(sprite, text, drawRect, drawFlags);
            font.DrawText(sprite, text, fontDimension, drawFlags, color);
            sprite.End();
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

            foreach (var palette in palettes)
                DisposeResource(palette);

            palettes.Clear();

            World?.OnDisposeDevice();

            DisposeResource(whitePixelTexture);
            DisposeResource(blackPixelTexture);
            DisposeResource(spritesTexture);
            DisposeResource(worldTexture);
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

            PixelShader.Function.ConstantTable.SetValue(Device, psFadingLevelHandle, FadingSettings.FadingLevel);
            PixelShader.Function.ConstantTable.SetValue(Device, psFadingColorHandle, FadingSettings.FadingColor.ToVector4());

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

            Device.SetStreamSource(0, VertexBuffer, 0, VERTEX_SIZE);
            Device.SetTexture(0, texture);
            Device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
        }

        public void Render()
        {
            // Time in milliseconds
            var totalMillis = clock.ElapsedTicks / clockFrequency * 1000;
            if (totalMillis < nextTick)
                return;

            bool nextFrame = OnFrame();
            fpsFrames++;
            nextTick = totalMillis + tick;

            #region FPS and title update

            if (fpsTimer.ElapsedMilliseconds > 1000)
            {
                var fps = 1000.0 * fpsFrames / fpsTimer.ElapsedMilliseconds;

                // Update window title with FPS once every second
                Control.Text = $"X# - FPS: {fps:F2} ({fpsTimer.ElapsedMilliseconds / fpsFrames:F2}ms/frame)";

                // Restart the FPS counter
                fpsTimer.Reset();
                fpsTimer.Start();
                fpsFrames = 0;
            }
            #endregion

            if (Device == null)
                return;

            if (!BeginScene())
                return;

            var orthoLH = Matrix.OrthoLH(SCREEN_WIDTH, SCREEN_HEIGHT, 0.0f, 1.0f);
            Device.SetTransform(TransformState.Projection, orthoLH);
            Device.SetTransform(TransformState.World, Matrix.Identity);
            Device.SetTransform(TransformState.View, Matrix.Identity);

            var backBuffer = Device.GetRenderTarget(0);
            var worldSurface = worldTexture.GetSurfaceLevel(0);
            var spritesSurface = spritesTexture.GetSurfaceLevel(0);

            if (World != null)
            {
                Device.SetRenderTarget(0, worldSurface);
                Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Transparent, 1.0f, 0);

                if (drawBackground)
                {
                    World.RenderBackground(0);
                    World.RenderBackground(1);
                }

                if (drawDownLayer)
                    World.RenderForeground(0);

                Device.SetRenderTarget(0, backBuffer);
                DrawTexture(worldTexture);

                if (drawSprites)
                {
                    Device.SetRenderTarget(0, spritesSurface);
                    Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Transparent, 1.0f, 0);

                    // Render down layer sprites
                    foreach (var sprite in sprites[0])
                    {
                        if (!sprite.Alive || !sprite.Visible || sprite.IsOffscreen(BoxKind.BOUDINGBOX))
                            continue;

                        if (sprite == Player)
                        {
                            if (drawX)
                                sprite.Render();
                        }
                        else
                            sprite.Render();
                    }

                    Device.SetRenderTarget(0, backBuffer);
                    DrawTexture(spritesTexture, SPRITE_SAMPLER_STATE_LINEAR);
                }

                if (drawUpLayer)
                {
                    Device.SetRenderTarget(0, worldSurface);
                    Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Transparent, 1.0f, 0);

                    World.RenderForeground(1);

                    Device.SetRenderTarget(0, backBuffer);
                    DrawTexture(worldTexture);
                }

                if (drawSprites)
                {
                    Device.SetRenderTarget(0, spritesSurface);
                    Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Transparent, 1.0f, 0);

                    // Render up layer sprites
                    foreach (var sprite in sprites[1])
                    {
                        if (!sprite.Alive || !sprite.Visible || sprite.IsOffscreen(BoxKind.BOUDINGBOX))
                            continue;

                        if (sprite == Player)
                        {
                            if (drawX)
                                sprite.Render();
                        }
                        else
                            sprite.Render();
                    }

                    Device.SetRenderTarget(0, backBuffer);
                    DrawTexture(spritesTexture, SPRITE_SAMPLER_STATE_LINEAR);
                }

                if (drawHitbox || showDrawBox || showTriggerBounds)
                {
                    resultSet.Clear();
                    partition.Query(resultSet, World.BoundingBox, (drawHitbox || showTriggerBounds ? BoxKind.HITBOX : BoxKind.NONE) | (showDrawBox ? BoxKind.BOUDINGBOX : BoxKind.NONE), false);
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
                                    MMXBox hitbox = sprite.Hitbox;
                                    var rect = WorldBoxToScreen(hitbox);

                                    if (!entity.Alive)
                                    {
                                        if (entity.Respawnable)
                                            FillRectangle(rect, DEAD_RESPAWNABLE_HITBOX_COLOR);
                                        else
                                            FillRectangle(rect, DEAD_HITBOX_COLOR);
                                    }
                                    else
                                        FillRectangle(rect, HITBOX_COLOR);
                                }

                                if (showDrawBox)
                                {
                                    MMXBox drawBox = sprite.DrawBox;
                                    var rect = WorldBoxToScreen(drawBox);
                                    FillRectangle(rect, BOUNDING_BOX_COLOR);
                                }

                                if (showColliders)
                                {
                                    WorldCollider collider = sprite.WorldCollider;
                                    FillRectangle(WorldBoxToScreen(collider.HeadBox), HEAD_COLLIDER_COLOR);
                                    FillRectangle(WorldBoxToScreen(collider.ChestBox), CHEST_COLLIDER_COLOR);
                                    FillRectangle(WorldBoxToScreen(collider.LegsBox), LEGS_COLLIDER_COLOR);
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

                                    case CameraLockTrigger cameraLockTrigger when showTriggerCameraLook:
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

                Device.SetRenderTarget(0, spritesSurface);
                Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Transparent, 1.0f, 0);

                foreach (var layer in huds)
                    foreach (var hud in layer)
                    {
                        hud.UpdateOrigin();
                        hud.Render();
                    }

                Device.SetRenderTarget(0, backBuffer);
                DrawTexture(spritesTexture, SPRITE_SAMPLER_STATE_LINEAR);

                if (respawning || SpawningBlackScreen)
                    FillRectangle(WorldBoxToScreen(World.Camera.BoundingBox), Color.Black, FadingSettings);

                if (Player != null)
                {
                    if (drawTouchingMapBounds)
                    {
                        MMXBox collisionBox = Player.CollisionBox;

                        CheckAndDrawTouchingMaps(collisionBox + Vector.LEFT_VECTOR, true);
                        CheckAndDrawTouchingMaps(collisionBox + Vector.UP_VECTOR);
                        CheckAndDrawTouchingMaps(collisionBox + Vector.RIGHT_VECTOR, true);
                        CheckAndDrawTouchingMaps(collisionBox + Vector.DOWN_VECTOR);
                    }

                    if (drawHitbox)
                    {
                        MMXBox hitbox = Player.Hitbox;
                        var rect = WorldBoxToScreen(hitbox);
                        FillRectangle(rect, HITBOX_COLOR);
                    }

                    if (showDrawBox)
                    {
                        MMXBox drawBox = Player.DrawBox;
                        var rect = WorldBoxToScreen(drawBox);
                        FillRectangle(rect, BOUNDING_BOX_COLOR);
                    }

                    if (showColliders)
                    {
                        WorldCollider collider = Player.WorldCollider;
                        FillRectangle(WorldBoxToScreen(collider.HeadBox), HEAD_COLLIDER_COLOR);
                        FillRectangle(WorldBoxToScreen(collider.ChestBox), CHEST_COLLIDER_COLOR);
                        FillRectangle(WorldBoxToScreen(collider.LegsBox), LEGS_COLLIDER_COLOR);
                    }
                }

                if (drawHighlightedPointingTiles)
                {
                    System.Drawing.Point cursorPos = Control.PointToClient(Cursor.Position);
                    Vector v = ScreenPointToVector(cursorPos.X / 4, cursorPos.Y / 4);
                    DrawText($"Mouse Pos: X: {v.X} Y: {v.Y}", highlightMapTextFont, new RectangleF(0, 0, 400, 50), FontDrawFlags.Left | FontDrawFlags.Top, TOUCHING_MAP_COLOR);

                    Scene scene = World.GetSceneFrom(v, false);
                    if (scene != null)
                    {
                        Cell sceneCell = GetSceneCellFromPos(v);
                        MMXBox sceneBox = GetSceneBoundingBox(sceneCell);
                        DrawRectangle(WorldBoxToScreen(sceneBox), 4, TOUCHING_MAP_COLOR);
                        DrawText($"Scene: ID: {scene.ID} Row: {sceneCell.Row} Col: {sceneCell.Col}", highlightMapTextFont, new RectangleF(0, 50, 400, 50), FontDrawFlags.Left | FontDrawFlags.Top, TOUCHING_MAP_COLOR);

                        Block block = World.GetBlockFrom(v, false);
                        if (block != null)
                        {
                            Cell blockCell = GetBlockCellFromPos(v);
                            MMXBox blockBox = GetBlockBoundingBox(blockCell);
                            DrawRectangle(WorldBoxToScreen(blockBox), 4, TOUCHING_MAP_COLOR);
                            DrawText($"Block: ID: {block.ID} Row: {blockCell.Row} Col: {blockCell.Col}", highlightMapTextFont, new RectangleF(0, 100, 400, 50), FontDrawFlags.Left | FontDrawFlags.Top, TOUCHING_MAP_COLOR);

                            Map map = World.GetMapFrom(v, false);
                            if (map != null)
                            {
                                Cell mapCell = GetMapCellFromPos(v);
                                MMXBox mapBox = GetMapBoundingBox(mapCell);
                                DrawRectangle(WorldBoxToScreen(mapBox), 4, TOUCHING_MAP_COLOR);
                                DrawText($"Map: ID: {map.ID} Row: {mapCell.Row} Col: {mapCell.Col}", highlightMapTextFont, new RectangleF(0, 150, 400, 50), FontDrawFlags.Left | FontDrawFlags.Top, TOUCHING_MAP_COLOR);

                                Tile tile = World.GetTileFrom(v, false);
                                if (tile != null)
                                {
                                    Cell tileCell = GetTileCellFromPos(v);
                                    MMXBox tileBox = GetTileBoundingBox(tileCell);
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

                if (showCheckpointBounds && currentCheckpoint != null)
                    DrawRectangle(WorldBoxToScreen(currentCheckpoint.Hitbox), 4, Color.Yellow);

                if (showInfoText && Player != null)
                {
                    string text = $"Checkpoint: {(currentCheckpoint != null ? currentCheckpoint.Index.ToString() : "none")}";
                    DrawText(text, infoFont, drawRect, FontDrawFlags.Bottom | FontDrawFlags.Left, Color.Yellow, out RawRectangle fontDimension);

                    text = $"Camera: CX: {(float) World.Camera.Left * 256} CY: {(float) World.Camera.Top * 256}";
                    DrawText(text, infoFont, drawRect, FontDrawFlags.Bottom | FontDrawFlags.Left, 0, fontDimension.Top - fontDimension.Bottom, Color.Yellow, out fontDimension);

                    text = $"Player: {(float) Player.Origin.X * 256} Y: {(float) Player.Origin.Y * 256} VX: {(float) Player.Velocity.X * 256} VY: {(float) Player.Velocity.Y * -256}";
                    DrawText(text, infoFont, drawRect, FontDrawFlags.Bottom | FontDrawFlags.Left, 0, 2 * (fontDimension.Top - fontDimension.Bottom), Color.Yellow, out fontDimension);
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
            finally
            {
                if (nextFrame)
                {
                    if (Freezing || Player == null || !Player.Freezed)
                    {
                        if (FreezingSprites)
                        {
                            for (var entity = firstEntity; entity != null; entity = entity.next)
                                if (entity is not Sprite sprite || freezingSpriteExceptions.Contains(sprite))
                                    entity.PostThink();
                        }
                        else
                            for (var entity = firstEntity; entity != null; entity = entity.next)
                                entity.PostThink();
                    }
                    else
                        Player?.PostThink();
                }
            }
        }

        private void SaveState(BinaryWriter writer)
        {
            var seedArrayInfo = typeof(Random).GetField("SeedArray", BindingFlags.NonPublic | BindingFlags.Instance);
            var seedArray = seedArrayInfo.GetValue(RNG) as int[];
            writer.Write(seedArray.Length);
            for (int i = 0; i < seedArray.Length; i++)
                writer.Write(seedArray[i]);

            writer.Write(currentCheckpoint != null ? currentCheckpoint.Point : -1);

            CameraConstraintsBox.Write(writer);

            writer.Write(gameOver);
            writer.Write(paused);

            World.Camera.Center.Write(writer);
            writer.Write(World.Camera.FocusOn != null ? World.Camera.FocusOn.Index : -1);

            Player?.SaveState(writer);
        }

        private void LoadState(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            var seedArray = new int[count];
            for (int i = 0; i < seedArray.Length; i++)
                seedArray[i] = reader.ReadInt32();

            var seedArrayInfo = typeof(Random).GetField("SeedArray", BindingFlags.NonPublic | BindingFlags.Instance);
            seedArrayInfo.SetValue(RNG, seedArray);

            int checkPointIndex = reader.ReadInt32();
            currentCheckpoint = checkPointIndex != -1 ? checkpoints[checkPointIndex] : null;

            CameraConstraintsBox = new MMXBox(reader);

            gameOver = reader.ReadBoolean();
            paused = reader.ReadBoolean();

            World.Camera.Center = new Vector(reader);
            int focusedObjectIndex = reader.ReadInt32();
            World.Camera.FocusOn = focusedObjectIndex >= 0 ? entities[focusedObjectIndex] : null;

            Player?.LoadState(reader);
        }

        public void SaveState(int slot = -1)
        {
            if (slot == -1)
                slot = currentSaveSlot;

            string fileName = @"sstates\state." + slot;
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            using var stream = new FileStream(fileName, FileMode.OpenOrCreate);
            using var writer = new BinaryWriter(stream);
            SaveState(writer);
        }

        public void LoadState(int slot = -1)
        {
            if (slot == -1)
                slot = currentSaveSlot;

            try
            {
                string fileName = @"sstates\state." + slot;
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                using var stream = new FileStream(fileName, FileMode.Open);
                using var reader = new BinaryReader(stream);
                LoadState(reader);
            }
            catch (IOException)
            {
            }
        }

        public ChangeDynamicPropertyTrigger AddChangeDynamicPropertyTrigger(Vector origin, DynamicProperty prop, int forward, int backward, SplitterTriggerOrientation orientation)
        {
            var trigger = CreateEntity<ChangeDynamicPropertyTrigger>(new
            {
                Origin = origin,
                Hitbox = (origin, (-SCREEN_WIDTH * 0.5, -SCREEN_HEIGHT * 0.5), (SCREEN_WIDTH * 0.5, SCREEN_HEIGHT * 0.5)),
                Property = prop,
                Forward = forward,
                Backward = backward,
                Orientation = orientation
            });

            trigger.Spawn();
            return trigger;
        }

        public Checkpoint AddCheckpoint(ushort index, MMXBox boundingBox, Vector characterPos, Vector cameraPos, Vector backgroundPos, Vector forceBackground, uint scroll)
        {
            var checkpoint = CreateEntity<Checkpoint>(new
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
            return checkpoint;
        }

        public CheckpointTriggerOnce AddCheckpointTrigger(ushort index, Vector origin)
        {
            var trigger = CreateEntity<CheckpointTriggerOnce>(new
            {
                Origin = origin,
                Hitbox = (origin, (0, -SCREEN_HEIGHT * 0.5), (SCREEN_WIDTH * 0.5, SCREEN_HEIGHT * 0.5)),
                Checkpoint = checkpoints[index]
            });

            trigger.Spawn();
            return trigger;
        }

        public Sprite AddObjectEvent(ushort id, ushort subid, Vector origin)
        {
            return !ENABLE_ENEMIES
                ? null
                : id switch
                {
                    0x02 when mmx.Type == 0 && mmx.Level == 8 => AddPenguin(origin), // Penguin
                    0x09 => AddDriller(origin),
                    0x2D or 0x50 => AddBat(origin),
                    0x2F => AddRideArmor(subid, origin),
                    0x4D => AddCapsule(subid, origin),
                    _ => AddDriller(origin)
                };
        }

        internal Penguin AddPenguin(Vector origin)
        {
            var penguin = CreateEntity<Penguin>(new
            {
                Origin = origin
            });

            penguin.BossDefeatedEvent += OnBossDefeated;
            Boss = penguin;

            return penguin;
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

        public Sprite AddRideArmor(ushort subid, Vector origin)
        {
            // TODO : Implement
            return null;
        }

        public Sprite AddCapsule(ushort subid, Vector origin)
        {
            // TODO : Implement
            return null;
        }

        public CameraLockTrigger AddCameraLockTrigger(MMXBox boundingBox, IEnumerable<Vector> extensions)
        {
            var trigger = CreateEntity<CameraLockTrigger>(new
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
            MMXBox boundingBox = CameraConstraintsBox;
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

            CameraConstraintsBox = new MMXBox(minX, minY, maxX - minX, maxY - minY);

            // TODO : Implement posssibility to smooth speed for one direction only
            if ((Player.Origin - World.Camera.Center).Length.TruncFracPart() >= STEP_SIZE)
            {
                World.Camera.SmoothOnNextMove = true;
                World.Camera.SmoothSpeed = CAMERA_SMOOTH_SPEED;
            }
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

        internal void ShootLemon(Player shooter, Vector origin, bool dashLemon)
        {
            var lemon = CreateEntity<BusterLemon>(new
            {
                Shooter = shooter,
                Origin = origin,
                DashLemon = dashLemon
            });

            lemon.Spawn();
        }

        internal void ShootSemiCharged(Player shooter, Vector origin)
        {
            var semiCharged = CreateEntity<BusterSemiCharged>(new
            {
                Shooter = shooter,
                Origin = origin
            });

            semiCharged.Spawn();
        }

        internal void ShootCharged(Player shooter, Vector origin)
        {
            var charged = CreateEntity<BusterCharged>(new
            {
                Shooter = shooter,
                Origin = origin
            });

            charged.Spawn();
        }

        internal ChargingEffect StartChargingEffect(Player player)
        {
            var effect = CreateEntity<ChargingEffect>(new
            {
                Charger = player
            });

            effect.Spawn();
            return effect;
        }

        internal DashSparkEffect StartDashSparkEffect(Player player)
        {
            var effect = CreateEntity<DashSparkEffect>(new
            {
                Player = player
            });

            effect.Spawn();
            return effect;
        }

        internal DashSmokeEffect StartDashSmokeEffect(Player player)
        {
            var effect = CreateEntity<DashSmokeEffect>(new
            {
                Player = player
            });

            effect.Spawn();
            return effect;
        }

        internal WallSlideEffect StartWallSlideEffect(Player player)
        {
            var effect = CreateEntity<WallSlideEffect>(new
            {
                Player = player
            });

            effect.Spawn();
            return effect;
        }

        internal WallKickEffect StartWallKickEffect(Player player)
        {
            var effect = CreateEntity<WallKickEffect>(new
            {
                Player = player
            });

            effect.Spawn();
            return effect;
        }

        internal ExplosionEffect CreateExplosionEffect(Vector origin, ExplosionEffectSound effectSound = ExplosionEffectSound.ENEMY_DIE_1, int soundChannel = 2)
        {
            var effect = CreateEntity<ExplosionEffect>(new
            {
                Origin = origin,
                EffectSound = effectSound
            });

            effect.Spawn();
            return effect;
        }

        private XDieExplosion CreateXDieExplosionEffect(double phase)
        {
            var effect = CreateEntity<XDieExplosion>(new
            {
                Offset = Player.Origin - World.Camera.LeftTop,
                Phase = phase
            });

            effect.Spawn();
            return effect;
        }

        public Driller AddDriller(Vector origin)
        {
            var driller = CreateEntity<Driller>(new
            {
                Origin = origin
            });

            driller.Place();
            return driller;
        }

        public Bat AddBat(Vector origin)
        {
            var bat = CreateEntity<Bat>(new
            {
                Origin = origin
            });

            bat.Place();
            return bat;
        }

        private void SpawnX(Vector origin)
        {
            if (Player != null)
                RemoveEntity(Player);

            Player = CreateEntity<Player>(new
            {
                Name = "X",
                Origin = origin
            });

            Player.Spawn();
            Player.Lives = lastLives;
        }

        private void CreateHP()
        {
            if (HP != null)
                RemoveEntity(HP);

            HP = CreateEntity<PlayerHealthHUD>(new
            {
                Name = "HP"
            });

            HP.Spawn();
        }

        private void StartReadyHUD()
        {
            if (ReadyHUD != null)
                RemoveEntity(ReadyHUD);

            ReadyHUD = CreateEntity<ReadyHUD>(new
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

        public void RenderVertexBuffer(VertexBuffer vb, int vertexSize, int primitiveCount, Texture texture, Texture palette, FadingControl fadingSettings, MMXBox box)
        {
            Device.SetStreamSource(0, vb, 0, vertexSize);

            RectangleF rDest = WorldBoxToScreen(box);

            float x = rDest.Left - (float) World.Camera.Width * 0.5f;
            float y = -rDest.Top + (float) World.Camera.Height * 0.5f;

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

            if (fadingSettings != null)
            {
                shader.Function.ConstantTable.SetValue(Device, fadingLevelHandle, fadingSettings.FadingLevel);
                shader.Function.ConstantTable.SetValue(Device, fadingColorHandle, fadingSettings.FadingColor.ToVector4());
            }
            else
                shader.Function.ConstantTable.SetValue(Device, fadingLevelHandle, Vector4.Zero);

            Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Point);
            Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Point);

            Device.DrawPrimitives(PrimitiveType.TriangleList, 0, primitiveCount);
        }

        public Texture CreatePalette(Color[] colors, int count = 256)
        {
            if (colors.Length > count)
                throw new ArgumentException($"Length of colors should up to {count}.");

            var palette = new Texture(Device, count, 1, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
            DataRectangle rect = palette.LockRectangle(0, D3D9LockFlags.None);

            using (var stream = new DataStream(rect.DataPointer, count * 1 * sizeof(int), true, true))
            {
                for (int i = 0; i < colors.Length; i++)
                    stream.Write(colors[i].ToBgra());

                for (int i = colors.Length; i < count; i++)
                    stream.Write(0);
            }

            palette.UnlockRectangle(0);
            return palette;
        }

        public Texture CreatePalette(Texture image, int count = 256)
        {
            var palette = new Texture(Device, count, 1, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);

            DataRectangle paletteRect = palette.LockRectangle(0, D3D9LockFlags.None);
            DataRectangle imageRect = image.LockRectangle(0, D3D9LockFlags.None);

            try
            {
                using var paletteStream = new DataStream(paletteRect.DataPointer, count * 1 * sizeof(int), true, true);

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

                for (int i = colors.Count; i < count; i++)
                    paletteStream.Write(0);
            }
            finally
            {
                image.UnlockRectangle(0);
                palette.UnlockRectangle(0);
            }

            return palette;
        }

        public static int LookupColor(Texture palette, Color color)
        {
            return LookupColor(palette, color, 0, 256);
        }

        public static int LookupColor(Texture palette, Color color, int start, int count)
        {
            DataRectangle rect = palette.LockRectangle(0, D3D9LockFlags.Discard);
            try
            {
                int width = palette.GetLevelDescription(0).Width;
                int height = palette.GetLevelDescription(0).Height;

                using var stream = new DataStream(rect.DataPointer, width * height * sizeof(int), true, true);
                using var reader = new BinaryReader(stream);
                for (int i = start; i < start + count; i++)
                {
                    int bgra = reader.ReadInt32();
                    var c = Color.FromBgra(bgra);
                    if (color == c)
                        return i;
                }
            }
            finally
            {
                palette.UnlockRectangle(0);
            }

            return -1;
        }

        public static Color GetPaletteColor(Texture palette, int index)
        {
            DataRectangle rect = palette.LockRectangle(0, D3D9LockFlags.Discard);
            try
            {
                int width = palette.GetLevelDescription(0).Width;
                int height = palette.GetLevelDescription(0).Height;

                using var stream = new DataStream(rect.DataPointer, width * height * sizeof(int), true, true);
                using var reader = new BinaryReader(stream);
                stream.Position = index * sizeof(int);
                int bgra = reader.ReadInt32();
                var c = Color.FromBgra(bgra);
                return c;
            }
            finally
            {
                palette.UnlockRectangle(0);
            }
        }

        public static void SetPaletteColor(Texture palette, int index, Color color)
        {
            DataRectangle rect = palette.LockRectangle(0, D3D9LockFlags.Discard);
            try
            {
                int width = palette.GetLevelDescription(0).Width;
                int height = palette.GetLevelDescription(0).Height;

                using var stream = new DataStream(rect.DataPointer, width * height * sizeof(int), true, true);
                stream.Position = index * sizeof(int);
                stream.Write(color.ToBgra());
            }
            finally
            {
                palette.UnlockRectangle(0);
            }
        }

        private void ReloadLevel()
        {
            if (LOAD_ROM)
            {
                mmx = new MMXCore();
                mmx.LoadNewRom(@"resources\roms\" + ROM_NAME);
                mmx.Init();

                if (mmx.CheckROM() != 0)
                {
                    romLoaded = true;
                    mmx.LoadFont();
                    mmx.LoadProperties();
                }
            }

            LoadLevel(INITIAL_LEVEL, INITIAL_CHECKPOINT);

            if (romLoaded)
                mmx.UpdateVRAMCache();
        }

        private void Execute()
        {
            World = new MMXWorld(32, 32);
            partition = new Partition<Entity>(World.BoundingBox, World.SceneRowCount, World.SceneColCount);
            resultSet = new HashSet<Entity>();
            CameraConstraintsBox = World.BoundingBox;

            ResetDevice();
            ReloadLevel();

            while (Running)
            {
                // Main loop
                RenderLoop.Run(Control, Render);
            }
        }

        public SpriteSheet AddSpriteSheet(string name, bool disposeTexture = false, bool precache = false)
        {
            var result = new SpriteSheet(name, disposeTexture, precache)
            {
                Index = spriteSheets.Count
            };

            spriteSheets.Add(result);
            spriteSheetsByName.Add(name, result);
            return result;
        }

        public SpriteSheet AddSpriteSheet(string name, Texture texture, bool disposeTexture = false, bool precache = false)
        {
            var result = new SpriteSheet(name, texture, disposeTexture, precache)
            {
                Index = spriteSheets.Count
            };

            spriteSheets.Add(result);
            spriteSheetsByName.Add(name, result);
            return result;
        }

        public SpriteSheet AddSpriteSheet(string name, string imageFileName, bool precache = false)
        {
            var result = new SpriteSheet(name, imageFileName, precache)
            {
                Index = spriteSheets.Count
            };

            spriteSheets.Add(result);
            spriteSheetsByName.Add(name, result);
            return result;
        }

        public SpriteSheet GetSpriteSheet(int spriteSheetIndex)
        {
            return spriteSheets[spriteSheetIndex];
        }

        public SpriteSheet GetSpriteSheetByName(string name)
        {
            return spriteSheetsByName[name];
        }

        internal Texture GetPalette(int paletteIndex)
        {
            return paletteIndex >= 0 && paletteIndex < palettes.Count ? palettes[paletteIndex] : null;
        }

        public void PlaySound(int channel, int index, double stopTime, double loopTime, bool ignoreUpdatesUntilPlayed = false)
        {
            var stream = soundStreams[index];
            var (player, ss, initialized) = soundChannels[channel];

            stream.Position = 0;
            ss.UpdateSource(stream, stopTime, loopTime, ignoreUpdatesUntilPlayed);

            if (!ss.Playing)
            {
                ss.Reset();
                ss.Play();
            }

            if (!initialized)
            {
                player.Init(ss);
                soundChannels[channel] = (player, ss, true);
            }

            player.Play();
        }

        public void PlaySound(int channel, int index, double loopTime, bool ignoreUpdatesUntilFinished = false)
        {
            PlaySound(channel, index, -1, loopTime, ignoreUpdatesUntilFinished);
        }

        public void PlaySound(int channel, int index, bool ignoreUpdatesUntilFinished = false)
        {
            PlaySound(channel, index, -1, -1, ignoreUpdatesUntilFinished);
        }

        public void ClearSoundLoopPoint(int channel, int index, bool clearStopPoint = false)
        {
            var stream = soundStreams[index];
            var (_, ss, _) = soundChannels[channel];

            if (ss.Source == stream)
            {
                ss.LoopPoint = -1;
                if (clearStopPoint)
                    ss.StopPoint = -1;
            }
        }

        public void ClearSoundStopPoint(int channel, int index)
        {
            var stream = soundStreams[index];
            var (_, ss, _) = soundChannels[channel];

            if (ss.Source == stream)
                ss.StopPoint = -1;
        }

        public void PlayOST(int index, double stopTime, double loopTime)
        {
            PlaySound(3, index, stopTime, loopTime);
        }

        public void PlayOST(int index, double loopTime)
        {
            PlaySound(3, index, loopTime);
        }

        public void PlayOST(int index)
        {
            PlaySound(3, index);
        }

        public void StopOST(int index)
        {
            StopSound(3, index);
        }

        public void StopBossBattleOST()
        {
            StopOST(25);
        }

        public void StopSound(int channel, int index)
        {
            var stream = soundStreams[index];
            var (_, ss, _) = soundChannels[channel];

            if (ss.Source == stream)
                ss.Stop();
        }

        public void StopSound(int channel)
        {
            var (_, ss, _) = soundChannels[channel];
            ss.Stop();
        }

        public void StopAllSounds(int exceptChannel = -1)
        {
            for (int channel = 0; channel < soundChannels.Count; channel++)
            {
                if (channel != exceptChannel)
                {
                    var (_, ss, _) = soundChannels[channel];
                    ss.Stop();
                }
            }
        }

        internal void ReloadLevelTransition()
        {
            DyingEffectActive = false;
            respawning = true;
            UnloadLevel();
            FadingSettings.Reset();
            FadingSettings.Start(Color.Black, 28, FadingFlags.COLORS, FadingFlags.COLORS, LoadLevel);

            if (ENABLE_OST)
                StartFadingOST(0, 60, () => soundChannels[3].player.Stop());
        }

        internal void StartDyingEffect()
        {
            DyingEffectActive = true;
            DyingEffectFrameCounter = 0;
            FadingSettings.Reset();
            FadingSettings.Start(Color.White, 132, ReloadLevelTransition);
        }

        public SmallHealthRecover DropSmallHealthRecover(Vector origin, int durationFrames)
        {
            var drop = CreateEntity<SmallHealthRecover>(new
            {
                Origin = origin,
                DurationFrames = durationFrames
            });

            drop.Spawn();
            return drop;
        }

        public BigHealthRecover DropBigHealthRecover(Vector origin, int durationFrames)
        {
            var drop = CreateEntity<BigHealthRecover>(new
            {
                Origin = origin,
                DurationFrames = durationFrames
            });

            drop.Spawn();
            return drop;
        }

        public SmallAmmoRecover DropSmallAmmoRecover(Vector origin, int durationFrames)
        {
            var drop = CreateEntity<SmallAmmoRecover>(new
            {
                Origin = origin,
                DurationFrames = durationFrames
            });

            drop.Spawn();
            return drop;
        }

        public BigAmmoRecover DropBigAmmoRecover(Vector origin, int durationFrames)
        {
            var drop = CreateEntity<BigAmmoRecover>(new
            {
                Origin = origin,
                DurationFrames = durationFrames
            });

            drop.Spawn();
            return drop;
        }

        public LifeUp DropLifeUp(Vector origin, int durationFrames)
        {
            var drop = CreateEntity<LifeUp>(new
            {
                Origin = origin,
                DurationFrames = durationFrames
            });

            drop.Spawn();
            return drop;
        }

        public SmallHealthRecover AddSmallHealthRecover(Vector origin)
        {
            var item = CreateEntity<SmallHealthRecover>(new
            {
                Origin = origin,
                DurationFrames = 0
            });

            item.Place();
            return item;
        }

        public BigHealthRecover AddBigHealthRecover(Vector origin)
        {
            var item = CreateEntity<BigHealthRecover>(new
            {
                Origin = origin,
                DurationFrames = 0
            });

            item.Place();
            return item;
        }

        public SmallAmmoRecover AddSmallAmmoRecover(Vector origin)
        {
            var item = CreateEntity<SmallAmmoRecover>(new
            {
                Origin = origin,
                DurationFrames = 0
            });

            item.Place();
            return item;
        }

        public BigAmmoRecover AddBigAmmoRecover(Vector origin)
        {
            var item = CreateEntity<BigAmmoRecover>(new
            {
                Origin = origin,
                DurationFrames = 0
            });

            item.Place();
            return item;
        }

        public LifeUp AddLifeUp(Vector origin)
        {
            var item = CreateEntity<LifeUp>(new
            {
                Origin = origin,
                DurationFrames = 0
            });

            item.Place();
            return item;
        }

        public HeartTank AddHeartTank(Vector origin)
        {
            var item = CreateEntity<HeartTank>(new
            {
                Origin = origin
            });

            item.Place();
            return item;
        }

        public SubTankItem AddSubTank(Vector origin)
        {
            var item = CreateEntity<SubTankItem>(new
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

            PlaySound(0, 19);

            if (amount > 0)
                Freeze(4, () => HealthRecoveringStep(amount));
        }

        internal void StartHeartTankAcquiring()
        {
            PlaySound(0, 21);
            Freeze(84, () => HeartTankAcquiringStep(2));
        }

        internal void HeartTankAcquiringStep(int amount)
        {
            Player.Health++;
            HealthCapacity++;
            amount--;

            PlaySound(0, 19);

            if (amount > 0)
                Freeze(4, () => HeartTankAcquiringStep(amount));

            // TODO : Implement the remaining
        }

        internal void StartSubTankAcquiring()
        {
            PlaySound(0, 21);
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
            var door = CreateEntity<BossDoor>(new
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
                    Scene scene = World.GetSceneFrom(1, 29);
                    scene.SetMap(new Cell(7, 15), World.GetMapByID(0x176));
                    scene.SetMap(new Cell(8, 15), World.GetMapByID(0x207));
                    scene.SetMap(new Cell(9, 15), World.GetMapByID(0x20f));

                    scene = World.GetSceneFrom(1, 30);
                    scene.SetMap(new Cell(7, 0), World.GetMapByID(0x177));
                    scene.SetMap(new Cell(8, 0), World.GetMapByID(0x1b0));
                    scene.SetMap(new Cell(9, 0), World.GetMapByID(0x20b));
                }
                else
                {
                    Scene scene = World.GetSceneFrom(1, 28);
                    scene.SetMap(new Cell(7, 15), World.GetMapByID(0x177));
                    scene.SetMap(new Cell(8, 15), World.GetMapByID(0x1b0));
                    scene.SetMap(new Cell(9, 15), World.GetMapByID(0x20b));

                    scene = World.GetSceneFrom(1, 29);
                    scene.SetMap(new Cell(7, 0), World.GetMapByID(0x177));
                    scene.SetMap(new Cell(8, 0), World.GetMapByID(0x1b0));
                    scene.SetMap(new Cell(9, 0), World.GetMapByID(0x20b));
                }
            }
        }

        private void DoorClosing(bool secondDoor)
        {
            if (romLoaded && mmx.Type == 0 && mmx.Level == 8)
            {
                if (secondDoor)
                {
                    Scene scene = World.GetSceneFrom(1, 29);
                    scene.SetMap(new Cell(7, 15), World.GetMapByID(0x172));
                    scene.SetMap(new Cell(8, 15), World.GetMapByID(0x173));
                    scene.SetMap(new Cell(9, 15), World.GetMapByID(0x174));

                    scene = World.GetSceneFrom(1, 30);
                    scene.SetMap(new Cell(7, 0), World.GetMapByID(0x172));
                    scene.SetMap(new Cell(8, 0), World.GetMapByID(0x173));
                    scene.SetMap(new Cell(9, 0), World.GetMapByID(0x174));
                }
                else
                {
                    Scene scene = World.GetSceneFrom(1, 28);
                    scene.SetMap(new Cell(7, 15), World.GetMapByID(0x172));
                    scene.SetMap(new Cell(8, 15), World.GetMapByID(0x173));
                    scene.SetMap(new Cell(9, 15), World.GetMapByID(0x174));

                    scene = World.GetSceneFrom(1, 29);
                    scene.SetMap(new Cell(7, 0), World.GetMapByID(0x172));
                    scene.SetMap(new Cell(8, 0), World.GetMapByID(0x173));
                    scene.SetMap(new Cell(9, 0), World.GetMapByID(0x174));
                }
            }
        }

        public string GetEntityExclusiveName(string prefix, bool startWithCounterSuffix = false, int startCounterSuffix = 1)
        {
            int counter = startCounterSuffix;
            string possibleName = startWithCounterSuffix ? prefix + counter++ : prefix;

            for (Entity entity = GetEntityByname(prefix); entity != null; entity = GetEntityByname(possibleName))
                possibleName = prefix + counter++;

            return possibleName;
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
            for (Entity entity = firstEntity; entity != null; entity = entity.next)
                if (entity is Enemy)
                    entity.Kill();
        }

        public void KillAllAliveWeapons()
        {
            for (Entity entity = firstEntity; entity != null; entity = entity.next)
                if (entity is Weapon)
                    entity.Kill();
        }

        public void KillAllAliveEnemiesAndWeapons()
        {
            for (Entity entity = firstEntity; entity != null; entity = entity.next)
                if (entity is Enemy or Weapon)
                    entity.Kill();
        }

        public Entity GetEntityByname(string name)
        {
            return entitiesByName.TryGetValue(name, out Entity result) ? result : null;
        }

        internal void UpdatePaletteName(Palette palette, string name)
        {
        }

        internal void UpdateEntityName(Entity entity, string name)
        {
            if (name == entity.Name)
                return;

            if (entitiesByName.ContainsKey(name))
                throw new DuplicateEntityNameException(name);

            if (entity.Alive)
            {
                if (entity.Name is not null and not "")
                    entitiesByName.Remove(entity.Name);

                entitiesByName.Add(name, entity);
            }

            entity.name = name;
        }

        public void PlayBossIntroOST()
        {
            PlayOST(24, 13, 6.328125);
        }

        public void PlayBossBatleOST()
        {
            PlayOST(25, 57, 28.798);
        }

        public void PlayVictorySound()
        {
            PlayOST(26);
        }

        internal void StartBossBattle()
        {
            if (romLoaded && Boss != null)
            {
                BossBattle = true;
                BossIntroducing = true;
                PlayBossIntroOST();

                DoDelayedAction(64, () => Boss.Spawn());
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
            PlayOST(34, 5.651, 0.323);
        }

        public void PlayBossExplosionEnd()
        {
            PlayOST(35);
        }

        internal void OnPlayerTeleported()
        {
            FadingSettings.Reset();
            FadingSettings.Start(Color.Black, 60, ReloadLevel);
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
    }
}