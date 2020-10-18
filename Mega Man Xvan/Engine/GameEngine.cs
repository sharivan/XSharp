using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.DirectInput;
using SharpDX.IO;

using Types;

using MMX.Math;
using MMX.Geometry;
using MMX.ROM;

using Device9 = SharpDX.Direct3D9.Device;
using DXSprite = SharpDX.Direct3D9.Sprite;
using MMXBox = MMX.Geometry.Box;

using static MMX.Engine.Consts;

namespace MMX.Engine
{
    public class GameEngine : IDisposable
    {
        public const VertexFormat D3DFVF_TLVERTEX = VertexFormat.Position | VertexFormat.Diffuse | VertexFormat.Texture1;
        public const int VERTEX_SIZE = 24;

        private static readonly byte[] VERTEX_SHADER_BYTECODE = new byte[]
        {
              0,   2, 254, 255, 254, 255,
             20,   0,  67,  84,  65,  66,
             28,   0,   0,   0,  35,   0,
              0,   0,   0,   2, 254, 255,
              0,   0,   0,   0,   0,   0,
              0,   0,   0,   1,   0,   0,
             28,   0,   0,   0, 118, 115,
             95,  50,  95,  48,   0,  77,
            105,  99, 114, 111, 115, 111,
            102, 116,  32,  40,  82,  41,
             32,  72,  76,  83,  76,  32,
             83, 104,  97, 100, 101, 114,
             32,  67, 111, 109, 112, 105,
            108, 101, 114,  32,  49,  48,
             46,  49,   0, 171,  81,   0,
              0,   5,   0,   0,  15, 160,
              0,   0,   0,   0,   0,   0,
              0,   0,   0,   0,   0,   0,
              0,   0,   0,   0,   1,   0,
              0,   2,   0,   0,  15, 192,
              0,   0,   0, 160, 255, 255,
              0,   0
        };

        private static readonly byte[] PIXEL_SHADER_BYTECODE = new byte[]
        {
              0,   2, 255, 255, 254, 255,
             42,   0,  67,  84,  65,  66,
             28,   0,   0,   0, 123,   0,
              0,   0,   0,   2, 255, 255,
              2,   0,   0,   0,  28,   0,
              0,   0,   0,   1,   0,   0,
            116,   0,   0,   0,  68,   0,
              0,   0,   3,   0,   0,   0,
              1,   0,   0,   0,  76,   0,
              0,   0,   0,   0,   0,   0,
             92,   0,   0,   0,   3,   0,
              1,   0,   1,   0,   0,   0,
            100,   0,   0,   0,   0,   0,
              0,   0, 105, 109,  97, 103,
            101,   0, 171, 171,   4,   0,
             12,   0,   1,   0,   1,   0,
              1,   0,   0,   0,   0,   0,
              0,   0, 112,  97, 108, 101,
            116, 116, 101,   0,   4,   0,
             11,   0,   1,   0,   1,   0,
              1,   0,   0,   0,   0,   0,
              0,   0, 112, 115,  95,  50,
             95,  48,   0,  77, 105,  99,
            114, 111, 115, 111, 102, 116,
             32,  40,  82,  41,  32,  72,
             76,  83,  76,  32,  83, 104,
             97, 100, 101, 114,  32,  67,
            111, 109, 112, 105, 108, 101,
            114,  32,  49,  48,  46,  49,
              0, 171,  81,   0,   0,   5,
              0,   0,  15, 160,   0,   0,
            127,  63,   0,   0,   0,  59,
              0,   0,   0,   0,   0,   0,
              0,   0,  31,   0,   0,   2,
              0,   0,   0, 128,   0,   0,
              3, 176,  31,   0,   0,   2,
              0,   0,   0, 144,   0,   8,
             15, 160,  31,   0,   0,   2,
              0,   0,   0, 144,   1,   8,
             15, 160,  66,   0,   0,   3,
              0,   0,  15, 128,   0,   0,
            228, 176,   0,   8, 228, 160,
              4,   0,   0,   4,   0,   0,
              3, 128,   0,   0,   0, 128,
              0,   0,   0, 160,   0,   0,
             85, 160,  66,   0,   0,   3,
              0,   0,  15, 128,   0,   0,
            228, 128,   1,   8, 228, 160,
              1,   0,   0,   2,   0,   8,
             15, 128,   0,   0, 228, 128,
            255, 255,   0,   0
        };

        private Form form;
        private Device9 device;
        private VertexBuffer vb;
        private VertexShader vertexShader;
        private PixelShader pixelShader;
        private DXSprite sprite;

        private Texture xSmallSS;

        private World world;
        internal Partition<Entity> partition;
        private Player player;
        private long engineTime;
        private Random random;
        private List<Entity> entities;
        internal List<Entity> addedEntities;
        internal List<Entity> removedEntities;
        private List<RespawnEntry> scheduleRespawns;
        private int currentLevel;
        //private int enemyCount;
        private bool changeLevel;
        private int levelToChange;
        private bool gameOver;
        private string currentStageMusic;
        private bool loadingLevel;
        private bool paused;
        private bool noCameraConstraints;

        private int currentSaveSlot;

        private SoundCollection sounds;
        private long lastCurrentMemoryUsage;
        private MMXBox drawBox;
        private FixedSingle drawScale;

        private Checkpoint currentCheckpoint;
        internal Vector extensionOrigin;
        internal List<Vector> extensions;
        internal MMXBox cameraConstraintsBox;

        private SpriteSheet xSpriteSheet;
        private SpriteSheet xWeaponsSpriteSheet;
        private SpriteSheet xEffectsSpriteSheet;

        private Texture x1NormalPalette;
        private Texture chargeLevel1Palette;
        private Texture chargeLevel2Palette;

        /*private SolidColorBrush screenBoxBrush;
        private SolidColorBrush hitBoxBrush;
        private SolidColorBrush hitBoxBorderBrush;
        private SolidColorBrush touchingMapBrush;
        private SolidColorBrush playerOriginBrush;
        private SolidColorBrush coordsTextBrush;
        private SolidColorBrush checkpointBoxBrush;
        private SolidColorBrush triggerBoxBrush;
        private SolidColorBrush cameraEventExtensionBrush;
        private SolidColorBrush downColliderBrush;
        private SolidColorBrush upColliderBrush;
        private SolidColorBrush leftColliderBrush;
        private SolidColorBrush rightColliderBrush;

        private TextFormat coordsTextFormat;
        private TextFormat highlightMapTextFormat;*/

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
        private bool wasPressingToggleDrawX;

        private MMXCore mmx;
        private bool romLoaded;

        private DirectInput directInput;
        private Keyboard keyboard;
        private Joystick joystick;

        private bool drawBackground = true;
        private bool drawDownLayer = true;
        private bool drawSprites = true;
        private bool drawX = true;
        private bool drawUpLayer = true;

        private bool drawCollisionBox = DEBUG_DRAW_COLLISION_BOX;
        private bool showColliders = DEBUG_SHOW_COLLIDERS;
        private bool drawMapBounds = DEBUG_DRAW_MAP_BOUNDS;
        private bool drawTouchingMapBounds = DEBUG_HIGHLIGHT_TOUCHING_MAPS;
        private bool drawHighlightedPointingTiles = DEBUG_HIGHLIGHT_POINTED_TILES;
        private bool drawPlayerOriginAxis = DEBUG_DRAW_PLAYER_ORIGIN_AXIS;
        private bool showInfoText = DEBUG_SHOW_INFO_TEXT;
        private bool showCheckpointBounds = DEBUG_DRAW_CHECKPOINT;
        private bool showTriggerBounds = DEBUG_SHOW_TRIGGERS;
        private bool showTriggerCameraLook = DEBUG_SHOW_CAMERA_TRIGGER_EXTENSIONS;

        public Form Form
        {
            get
            {
                return form;
            }
        }

        public Device9 Device
        {
            get
            {
                return device;
            }
        }

        public VertexBuffer VertexBuffer
        {
            get
            {
                return vb;
            }
        }

        public RectangleF RenderRectangle
        {
            get
            {
                return ToRectangleF(drawBox);
            }
        }

        public World World
        {
            get
            {
                return world;
            }
        }

        public Player Player
        {
            get
            {
                return player;
            }
        }

        public Vector ExtensionOrigin
        {
            get
            {
                return extensionOrigin;
            }

            set
            {
                extensionOrigin = value;
            }
        }

        public int ExtensionCount
        {
            get
            {
                return extensions.Count;
            }
        }

        public Vector MinCameraPos
        {
            get
            {
                return noCameraConstraints ? world.BoundingBox.LeftTop : cameraConstraintsBox.LeftTop;
            }
        }

        public Vector MaxCameraPos
        {
            get
            {
                return noCameraConstraints ? world.BoundingBox.RightBottom : cameraConstraintsBox.RightBottom;
            }
        }

        public bool Paused
        {
            get
            {
                return paused;
            }

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
            get
            {
                return drawScale;
            }

            set
            {
                drawScale = value;
            }
        }

        public VertexShader VertexShader
        {
            get
            {
                return vertexShader;
            }
        }

        public PixelShader PixelShader
        {
            get
            {
                return pixelShader;
            }
        }

        public Texture X1NormalPalette
        {
            get
            {
                return x1NormalPalette;
            }
        }

        public Texture ChargeLevel1Palette
        {
            get
            {
                return chargeLevel1Palette;
            }
        }

        public Texture ChargeLevel2Palette
        {
            get
            {
                return chargeLevel2Palette;
            }
        }

        public Checkpoint CurrentCheckpoint
        {
            get
            {
                return currentCheckpoint;
            }

            set
            {
                if (currentCheckpoint != value)
                {
                    currentCheckpoint = value;

                    if (romLoaded)
                    {
                        mmx.SetLevel(INITIAL_LEVEL, (ushort) currentCheckpoint.Point);
                        mmx.LoadTilesAndPalettes();
                        mmx.LoadPalette(world, false);
                        mmx.RefreshMapCache(world, false);
                    }
                }
            }
        }

        public GameEngine(Form form, Device9 device)
        {
            this.form = form;
            this.device = device;

            ShaderBytecode function = new ShaderBytecode(VERTEX_SHADER_BYTECODE);
            vertexShader = new VertexShader(device, function);

            function = new ShaderBytecode(PIXEL_SHADER_BYTECODE);
            pixelShader = new PixelShader(device, function);

            device.VertexShader = null;
            device.PixelShader = null;
            device.VertexFormat = D3DFVF_TLVERTEX;

            vb = new VertexBuffer(device, VERTEX_SIZE * 4, Usage.WriteOnly, D3DFVF_TLVERTEX, Pool.Managed);

            device.SetRenderState(RenderState.ZEnable, false);
            device.SetRenderState(RenderState.Lighting, false);
            device.SetRenderState(RenderState.AlphaBlendEnable, true);
            device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
            device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
            device.SetTextureStageState(0, TextureStage.AlphaOperation, TextureOperation.Modulate);
            device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
            device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);

            sprite = new DXSprite(device);

            SetupQuad(vb);

            noCameraConstraints = NO_CAMERA_CONSTRAINTS;

            extensions = new List<Vector>();

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

            /*screenBoxBrush = new SolidColorBrush(context, new Color4(1, 1, 0, 0.5F));
            hitBoxBrush = new SolidColorBrush(context, new Color4(0, 1, 0, 0.5F));
            hitBoxBorderBrush = new SolidColorBrush(context, new Color4(0, 1, 0, 1));
            touchingMapBrush = new SolidColorBrush(context, new Color4(0, 0, 1, 1));
            playerOriginBrush = new SolidColorBrush(context, new Color4(0, 1, 1, 1));
            coordsTextBrush = new SolidColorBrush(context, new Color4(1, 1, 1, 1));
            checkpointBoxBrush = new SolidColorBrush(context, new Color4(1, 0.5F, 0, 1));
            triggerBoxBrush = new SolidColorBrush(context, new Color4(0, 1, 0, 1));
            cameraEventExtensionBrush = new SolidColorBrush(context, new Color4(1, 1, 0, 1));
            downColliderBrush = new SolidColorBrush(context, new Color4(0, 1, 0, 1));
            upColliderBrush = new SolidColorBrush(context, new Color4(0, 0, 1, 1));
            leftColliderBrush = new SolidColorBrush(context, new Color4(1, 0, 0, 1));
            rightColliderBrush = new SolidColorBrush(context, new Color4(1, 1, 0, 1));

            coordsTextFormat = new TextFormat(dwFactory, "Arial", 24)
            {
                TextAlignment = DWTextAlignment.Leading,
                ParagraphAlignment = ParagraphAlignment.Center
            };
            highlightMapTextFormat = new TextFormat(dwFactory, "Arial", 24)
            {
                TextAlignment = DWTextAlignment.Leading,
                ParagraphAlignment = ParagraphAlignment.Center
            };*/

            world = new World(this, 32, 32);
            partition = new Partition<Entity>(world.BoundingBox, world.SceneRowCount, world.SceneColCount);
            cameraConstraintsBox = world.BoundingBox;

            random = new Random();
            entities = new List<Entity>();
            addedEntities = new List<Entity>();
            removedEntities = new List<Entity>();
            scheduleRespawns = new List<RespawnEntry>();

            loadingLevel = true;

            drawScale = DEFAULT_DRAW_SCALE;
            UpdateScale();

            Vector normalOffset = new Vector(HITBOX_WIDTH * 0.5, HITBOX_HEIGHT + 3);
            MMXBox normalCollisionBox = new MMXBox(new Vector(-HITBOX_WIDTH * 0.5, -HITBOX_HEIGHT - 3), Vector.NULL_VECTOR, new Vector(HITBOX_WIDTH, HITBOX_HEIGHT + 3));
            Vector dashingOffset = new Vector(DASHING_HITBOX_WIDTH * 0.5, DASHING_HITBOX_HEIGHT + 3);
            MMXBox dashingCollisionBox = new MMXBox(new Vector(-DASHING_HITBOX_WIDTH * 0.5, -DASHING_HITBOX_HEIGHT - 3), Vector.NULL_VECTOR, new Vector(DASHING_HITBOX_WIDTH, DASHING_HITBOX_HEIGHT + 3));

            x1NormalPalette = CreatePalette(X1_NORMAL_PALETTE);
            chargeLevel1Palette = CreatePalette(CHARGE_LEVEL_1_PALETTE);
            chargeLevel2Palette = CreatePalette(CHARGE_LEVEL_2_PALETTE);

            xSpriteSheet = new SpriteSheet(this, "X", true, true);
            xWeaponsSpriteSheet = new SpriteSheet(this, "X Weapons", true, true);
            xEffectsSpriteSheet = new SpriteSheet(this, "X Effects", true, true);

            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Mega_Man_Xvan.resources.sprites.X.X[small].png"))
            {
                var newBitmap = CreateD2DBitmapFromStream(stream);
                xSpriteSheet.CurrentBitmap = newBitmap;
            }

            xSpriteSheet.CurrentPalette = x1NormalPalette;

            //xSmallSS = CreateD2DBitmapFromFile("resources\\sprites\\X\\X[small].png");
            //xSpriteSheet.CurrentBitmap = xSmallSS;

            var sequence = xSpriteSheet.AddFrameSquence("Spawn");
            sequence.BoudingBoxOriginOffset = normalOffset;
            sequence.CollisionBox = normalCollisionBox;
            sequence.AddFrame(-4, 25, 5, 15, 8, 48);

            sequence = xSpriteSheet.AddFrameSquence("SpawnEnd");
            sequence.BoudingBoxOriginOffset = normalOffset;
            sequence.CollisionBox = normalCollisionBox;
            sequence.AddFrame(-4, 32, 5, 15, 8, 48);
            sequence.AddFrame(3, -3, 19, 34, 22, 29, 2);
            sequence.AddFrame(8, 11, 46, 21, 30, 42);
            sequence.AddFrame(8, 8, 84, 24, 30, 39);
            sequence.AddFrame(8, 5, 120, 27, 30, 36);
            sequence.AddFrame(8, 4, 156, 28, 30, 34);
            sequence.AddFrame(8, 1, 191, 31, 30, 32, 3);

            sequence = xSpriteSheet.AddFrameSquence("Stand", 0);
            sequence.BoudingBoxOriginOffset = normalOffset;
            sequence.CollisionBox = normalCollisionBox;
            sequence.AddFrame(9, 3, 226, 29, 30, 34, 80);
            sequence.AddFrame(9, 3, 261, 29, 30, 34, 4);
            sequence.AddFrame(9, 3, 295, 29, 30, 34, 8);
            sequence.AddFrame(9, 3, 261, 29, 30, 34, 4);
            sequence.AddFrame(9, 3, 226, 29, 30, 34, 48);
            sequence.AddFrame(9, 3, 261, 29, 30, 34, 4);
            sequence.AddFrame(9, 3, 295, 29, 30, 34, 4);
            sequence.AddFrame(9, 3, 261, 29, 30, 34, 4);
            sequence.AddFrame(9, 3, 226, 29, 30, 34, 4);
            sequence.AddFrame(9, 3, 261, 29, 30, 34, 4);
            sequence.AddFrame(9, 3, 295, 29, 30, 34, 4);
            sequence.AddFrame(9, 3, 261, 29, 30, 34, 4);

            sequence = xSpriteSheet.AddFrameSquence("Shooting", 4);
            sequence.BoudingBoxOriginOffset = normalOffset;
            sequence.CollisionBox = normalCollisionBox;
            sequence.AddFrame(9, 3, 365, 29, 30, 34, 4);
            sequence.AddFrame(9, 3, 402, 29, 29, 34, 12);

            sequence = xSpriteSheet.AddFrameSquence("PreWalking");
            sequence.BoudingBoxOriginOffset = normalOffset;
            sequence.CollisionBox = normalCollisionBox;
            sequence.AddFrame(8, 3, 5, 67, 30, 34, 5);

            sequence = xSpriteSheet.AddFrameSquence("Walking", 0);
            sequence.BoudingBoxOriginOffset = normalOffset;
            sequence.CollisionBox = normalCollisionBox;
            sequence.AddFrame(1, 3, 50, 67, 20, 34);
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

            sequence = xSpriteSheet.AddFrameSquence("ShootWalking", 0);
            sequence.BoudingBoxOriginOffset = normalOffset;
            sequence.CollisionBox = normalCollisionBox;
            sequence.AddFrame(1, 3, 41, 107, 29, 34);
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
            sequence.BoudingBoxOriginOffset = normalOffset;
            sequence.CollisionBox = normalCollisionBox;
            sequence.AddFrame(1, 0, 6, 148, 25, 37, 3);
            sequence.AddFrame(-5, 1, 37, 148, 15, 41);

            sequence = xSpriteSheet.AddFrameSquence("ShootJumping");
            sequence.BoudingBoxOriginOffset = normalOffset;
            sequence.CollisionBox = normalCollisionBox;
            sequence.AddFrame(1, 0, 201, 148, 29, 37, 3);
            sequence.AddFrame(-5, 1, 240, 148, 24, 41);

            sequence = xSpriteSheet.AddFrameSquence("GoingUp", 0);
            sequence.BoudingBoxOriginOffset = normalOffset;
            sequence.CollisionBox = normalCollisionBox;
            sequence.AddFrame(-1, 5, 56, 146, 19, 46);

            sequence = xSpriteSheet.AddFrameSquence("ShootGoingUp", 0);
            sequence.BoudingBoxOriginOffset = normalOffset;
            sequence.CollisionBox = normalCollisionBox;
            sequence.AddFrame(-1, 5, 271, 146, 27, 46);

            sequence = xSpriteSheet.AddFrameSquence("Falling", 4);
            sequence.BoudingBoxOriginOffset = normalOffset;
            sequence.CollisionBox = normalCollisionBox;
            sequence.AddFrame(1, 5, 80, 150, 23, 41, 4);
            sequence.AddFrame(5, 6, 108, 150, 27, 42);

            sequence = xSpriteSheet.AddFrameSquence("ShootFalling", 4);
            sequence.BoudingBoxOriginOffset = normalOffset;
            sequence.CollisionBox = normalCollisionBox;
            sequence.AddFrame(1, 5, 304, 150, 31, 41, 4);
            sequence.AddFrame(5, 6, 341, 150, 31, 42);

            sequence = xSpriteSheet.AddFrameSquence("Landing");
            sequence.BoudingBoxOriginOffset = normalOffset;
            sequence.CollisionBox = normalCollisionBox;
            sequence.AddFrame(1, 2, 139, 151, 24, 38, 2);
            sequence.AddFrame(8, 1, 166, 153, 30, 32, 2);

            sequence = xSpriteSheet.AddFrameSquence("ShootLanding");
            sequence.BoudingBoxOriginOffset = normalOffset;
            sequence.CollisionBox = normalCollisionBox;
            sequence.AddFrame(1, 2, 378, 151, 30, 38, 2);
            sequence.AddFrame(8, 1, 413, 153, 36, 32, 2);

            sequence = xSpriteSheet.AddFrameSquence("PreDashing");
            sequence.BoudingBoxOriginOffset = dashingOffset;
            sequence.CollisionBox = dashingCollisionBox;
            sequence.AddFrame(4, 12, 4, 335, 28, 31, 3);

            sequence = xSpriteSheet.AddFrameSquence("ShootPreDashing");
            sequence.BoudingBoxOriginOffset = dashingOffset;
            sequence.CollisionBox = dashingCollisionBox;
            sequence.AddFrame(4, 12, 76, 335, 37, 31, 3);

            sequence = xSpriteSheet.AddFrameSquence("Dashing", 0);
            sequence.BoudingBoxOriginOffset = dashingOffset;
            sequence.CollisionBox = dashingCollisionBox;
            sequence.AddFrame(14, 7, 34, 341, 38, 26);

            sequence = xSpriteSheet.AddFrameSquence("ShootDashing", 0);
            sequence.BoudingBoxOriginOffset = dashingOffset;
            sequence.CollisionBox = dashingCollisionBox;
            sequence.AddFrame(14, 7, 115, 341, 48, 26);

            sequence = xSpriteSheet.AddFrameSquence("PostDashing");
            sequence.BoudingBoxOriginOffset = normalOffset;
            sequence.CollisionBox = normalCollisionBox;
            sequence.AddFrame(5, 0, 4, 335, 28, 31, 8);

            sequence = xSpriteSheet.AddFrameSquence("ShootPostDashing");
            sequence.BoudingBoxOriginOffset = normalOffset;
            sequence.CollisionBox = normalCollisionBox;
            sequence.AddFrame(5, 0, 4, 341, 38, 31, 8);

            sequence = xSpriteSheet.AddFrameSquence("WallSliding", 11);
            sequence.BoudingBoxOriginOffset = normalOffset;
            sequence.CollisionBox = normalCollisionBox;
            sequence.AddFrame(5, 5, 5, 197, 25, 42, 5);
            sequence.AddFrame(9, 7, 33, 196, 27, 43, 6);
            sequence.AddFrame(9, 8, 64, 196, 28, 42);

            sequence = xSpriteSheet.AddFrameSquence("ShootWallSliding", 11);
            sequence.BoudingBoxOriginOffset = normalOffset;
            sequence.CollisionBox = normalCollisionBox;
            sequence.AddFrame(5, 2, 158, 200, 31, 39, 5);
            sequence.AddFrame(9, 7, 201, 196, 32, 43, 6);
            sequence.AddFrame(9, 8, 240, 196, 32, 42);

            sequence = xSpriteSheet.AddFrameSquence("WallJumping");
            sequence.BoudingBoxOriginOffset = normalOffset;
            sequence.CollisionBox = normalCollisionBox;
            sequence.AddFrame(7, 2, 95, 199, 30, 39, 3);
            sequence.AddFrame(5, 10, 128, 195, 27, 44);

            sequence = xSpriteSheet.AddFrameSquence("ShootWallJumping");
            sequence.BoudingBoxOriginOffset = normalOffset;
            sequence.CollisionBox = normalCollisionBox;
            sequence.AddFrame(7, 1, 276, 200, 31, 38, 3);
            sequence.AddFrame(5, 5, 315, 200, 32, 39);

            sequence = xSpriteSheet.AddFrameSquence("PreLadderClimbing");
            sequence.BoudingBoxOriginOffset = normalOffset;
            sequence.CollisionBox = normalCollisionBox;
            sequence.AddFrame(3, 4, 7, 267, 21, 36, 8);

            sequence = xSpriteSheet.AddFrameSquence("LadderMoving", 0);
            sequence.BoudingBoxOriginOffset = normalOffset;
            sequence.CollisionBox = normalCollisionBox;
            sequence.AddFrame(2, 10, 111, 261, 18, 49, 8);
            sequence.AddFrame(4, 5, 84, 266, 20, 40, 3);
            sequence.AddFrame(5, 6, 60, 266, 20, 40, 3);
            sequence.AddFrame(5, 14, 36, 261, 18, 49, 8);
            sequence.AddFrame(5, 6, 60, 266, 20, 40, 3);
            sequence.AddFrame(4, 5, 84, 266, 20, 40, 3);

            sequence = xSpriteSheet.AddFrameSquence("ShootLadder", 0);
            sequence.BoudingBoxOriginOffset = normalOffset;
            sequence.CollisionBox = normalCollisionBox;
            sequence.AddFrame(5, 14, 137, 261, 26, 48, 16);

            sequence = xSpriteSheet.AddFrameSquence("TopLadderClimbing");
            sequence.BoudingBoxOriginOffset = normalOffset;
            sequence.AddFrame(5, -11, 169, 281, 21, 32, 8);
            sequence.AddFrame(2, -4, 195, 274, 18, 34, 6);

            sequence = xSpriteSheet.AddFrameSquence("TopLadderDescending");
            sequence.BoudingBoxOriginOffset = normalOffset;
            sequence.CollisionBox = normalCollisionBox;
            sequence.AddFrame(2, -4, 195, 274, 18, 34, 6);
            sequence.AddFrame(5, -11, 169, 281, 21, 32, 8);

            xSpriteSheet.ReleaseCurrentBitmap();

            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Mega_Man_Xvan.resources.sprites.X.Weapons.png"))
            {
                var texture = CreateD2DBitmapFromStream(stream);
                xWeaponsSpriteSheet.CurrentBitmap = texture;
            }

            MMXBox lemonCollisionBox = new MMXBox(Vector.NULL_VECTOR, new Vector(-LEMON_HITBOX_WIDTH * 0.5, -LEMON_HITBOX_HEIGHT * 0.5), new Vector(LEMON_HITBOX_WIDTH * 0.5, LEMON_HITBOX_HEIGHT * 0.5));

            sequence = xWeaponsSpriteSheet.AddFrameSquence("LemonShot", 0);
            sequence.BoudingBoxOriginOffset = lemonCollisionBox.Maxs;
            sequence.CollisionBox = lemonCollisionBox;
            sequence.AddFrame(5, -1, 123, 253, 8, 6);

            sequence = xWeaponsSpriteSheet.AddFrameSquence("LemonShotExplode");
            sequence.BoudingBoxOriginOffset = lemonCollisionBox.Maxs;
            sequence.CollisionBox = lemonCollisionBox;
            sequence.AddFrame(2, 1, 137, 250, 12, 12, 4);
            sequence.AddFrame(2, 2, 154, 249, 13, 13, 2);
            sequence.AddFrame(3, 3, 172, 248, 15, 15);

            MMXBox semiChargedShotCollisionBox1 = new MMXBox(Vector.NULL_VECTOR, new Vector(-SEMI_CHARGED_HITBOX_WIDTH_1 * 0.5, -SEMI_CHARGED_HITBOX_HEIGHT_1 * 0.5), new Vector(SEMI_CHARGED_HITBOX_WIDTH_1 * 0.5, SEMI_CHARGED_HITBOX_HEIGHT_1 * 0.5));
            MMXBox semiChargedShotCollisionBox2 = new MMXBox(Vector.NULL_VECTOR, new Vector(-SEMI_CHARGED_HITBOX_WIDTH_2 * 0.5, -SEMI_CHARGED_HITBOX_HEIGHT_2 * 0.5), new Vector(SEMI_CHARGED_HITBOX_WIDTH_2 * 0.5, SEMI_CHARGED_HITBOX_HEIGHT_2 * 0.5));
            MMXBox semiChargedShotCollisionBox3 = new MMXBox(Vector.NULL_VECTOR, new Vector(-SEMI_CHARGED_HITBOX_WIDTH_3 * 0.5, -SEMI_CHARGED_HITBOX_HEIGHT_3 * 0.5), new Vector(SEMI_CHARGED_HITBOX_WIDTH_3 * 0.5, SEMI_CHARGED_HITBOX_HEIGHT_3 * 0.5));

            sequence = xWeaponsSpriteSheet.AddFrameSquence("SemiChargedShotFiring");
            sequence.BoudingBoxOriginOffset = semiChargedShotCollisionBox1.Maxs;
            sequence.CollisionBox = semiChargedShotCollisionBox1;
            sequence.AddFrame(-5, 3, 128, 563, 14, 14);
            sequence.BoudingBoxOriginOffset = semiChargedShotCollisionBox2.Maxs;
            sequence.CollisionBox = semiChargedShotCollisionBox2;
            sequence.AddFrame(-9, -6, 128, 563, 14, 14);
            sequence.AddFrame(-9, -1, 147, 558, 24, 24);
            sequence.BoudingBoxOriginOffset = semiChargedShotCollisionBox3.Maxs;
            sequence.CollisionBox = semiChargedShotCollisionBox3;
            sequence.AddFrame(-11, -3, 147, 558, 24, 24);
            sequence.AddFrame(-11, -3, 176, 564, 28, 12);

            sequence = xWeaponsSpriteSheet.AddFrameSquence("SemiChargedShot");
            sequence.BoudingBoxOriginOffset = semiChargedShotCollisionBox3.Maxs;
            sequence.CollisionBox = semiChargedShotCollisionBox3;
            sequence.AddFrame(3, -3, 176, 564, 28, 12);
            sequence.AddFrame(3, -5, 210, 566, 32, 8, 3);
            sequence.AddFrame(9, -5, 210, 566, 32, 8);
            sequence.AddFrame(7, 1, 379, 562, 38, 16);
            sequence.AddFrame(9, -3, 333, 564, 36, 12, 1, true); // loop point
            sequence.AddFrame(8, 1, 292, 559, 36, 22, 2);
            sequence.AddFrame(9, -3, 333, 564, 36, 12);
            sequence.AddFrame(7, 1, 379, 562, 38, 16, 2);

            sequence = xWeaponsSpriteSheet.AddFrameSquence("SemiChargedShotHit");
            sequence.BoudingBoxOriginOffset = semiChargedShotCollisionBox2.Maxs;
            sequence.CollisionBox = semiChargedShotCollisionBox2;
            sequence.AddFrame(-9, -6, 424, 563, 14, 14);
            sequence.AddFrame(-9, -1, 443, 558, 24, 24);

            sequence = xWeaponsSpriteSheet.AddFrameSquence("SemiChargedShotExplode");
            sequence.BoudingBoxOriginOffset = semiChargedShotCollisionBox1.Maxs;
            sequence.AddFrame(487, 273, 16, 16);
            sequence.AddFrame(507, 269, 24, 24);
            sequence.AddFrame(535, 273, 16, 16);
            sequence.AddFrame(555, 270, 22, 22);
            sequence.AddFrame(581, 269, 24, 24);
            sequence.AddFrame(609, 269, 24, 24);

            sequence = xWeaponsSpriteSheet.AddFrameSquence("ChargedShotFiring");
            sequence.BoudingBoxOriginOffset = semiChargedShotCollisionBox1.Maxs;
            sequence.AddFrame(5, -1, 144, 599, 14, 20);
            sequence.AddFrame(5, -1, 166, 601, 23, 16);

            sequence = xWeaponsSpriteSheet.AddFrameSquence("ChargedShot", 0);
            sequence.BoudingBoxOriginOffset = semiChargedShotCollisionBox1.Maxs;
            sequence.AddFrame(5, -1, 193, 594, 46, 31);
            sequence.AddFrame(5, -1, 244, 595, 45, 29);
            sequence.AddFrame(5, -1, 293, 594, 42, 31);

            sequence = xWeaponsSpriteSheet.AddFrameSquence("ChargedShotHit");
            sequence.BoudingBoxOriginOffset = semiChargedShotCollisionBox1.Maxs;
            sequence.AddFrame(2, 1, 387, 599, 14, 20);
            sequence.AddFrame(2, 2, 405, 595, 24, 28);

            sequence = xWeaponsSpriteSheet.AddFrameSquence("ChargedShotExplode");
            sequence.BoudingBoxOriginOffset = semiChargedShotCollisionBox1.Maxs;
            sequence.AddFrame(2, 1, 441, 596, 28, 28);
            sequence.AddFrame(2, 2, 473, 597, 26, 26);
            sequence.AddFrame(3, 3, 503, 596, 28, 28);
            sequence.AddFrame(3, 3, 535, 595, 30, 30);
            sequence.AddFrame(3, 3, 569, 594, 32, 32);
            sequence.AddFrame(3, 3, 605, 594, 32, 32);

            xWeaponsSpriteSheet.ReleaseCurrentBitmap();

            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Mega_Man_Xvan.resources.sprites.X.Effects.png"))
            {
                var texture = CreateD2DBitmapFromStream(stream);
                xEffectsSpriteSheet.CurrentBitmap = texture;
            }

            xEffectsSpriteSheet.ReleaseCurrentBitmap();

            if (LOAD_ROM)
            {
                mmx = new MMXCore();
                mmx.LoadNewRom(Assembly.GetExecutingAssembly().GetManifestResourceStream("Mega_Man_Xvan.resources.roms." + ROM_NAME + ".smc"));
                mmx.Init();

                if (mmx.CheckROM() != 0)
                {
                    romLoaded = true;
                    mmx.LoadFont();
                    mmx.LoadProperties();
                }
            }

            LoadLevel(INITIAL_LEVEL);

            if (romLoaded)
                mmx.UpdateVRAMCache();
        }

        public Texture CreateD2DBitmapFromFile(string filePath)
        {
            Texture result = Texture.FromFile(device, filePath, Usage.None, Pool.SystemMemory);
            return result;
        }

        public Texture CreateD2DBitmapFromStream(Stream stream)
        {
            Texture result = Texture.FromStream(device, stream, Usage.None, Pool.SystemMemory);
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

        public static void SetupQuad(VertexBuffer vb)
        {
            DataStream vbData = vb.Lock(0, 4 * VERTEX_SIZE, LockFlags.None);

            WriteVertex(vbData, 0, 0, 0, 0);
            WriteVertex(vbData, 1, 0, 1, 0);
            WriteVertex(vbData, 1, -1, 1, 1);
            WriteVertex(vbData, 0, -1, 0, 1);

            vb.Unlock();
        }

        public void RenderTexture(Texture texture, Texture palette, MMXBox box, Matrix transform)
        {
            RectangleF rDest = WorldBoxToScreen(box);

            float x = rDest.Left;
            float y = rDest.Top;

            Matrix matScaling = Matrix.Scaling(1, 1, 1);
            Matrix matTranslation = Matrix.Translation(x, y, 0);
            Matrix matTransform = matScaling * matTranslation * transform;

            sprite.Begin();

            device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Point);
            device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Point);

            device.VertexShader = null;

            if (palette != null)
            {
                device.PixelShader = pixelShader;
                device.SetTexture(1, palette);
            }
            else
                device.PixelShader = null;

            sprite.Transform = matTransform;
            sprite.Draw(texture, Color.FromRgba(0xffffffff));

            sprite.End();

            /*device.SetStreamSource(0, vb, 0, VERTEX_SIZE);

            float x = rDest.Left - (float) world.Screen.Width * 0.5f;
            float y = -rDest.Top + (float) world.Screen.Height * 0.5f;

            Matrix matScaling = Matrix.Scaling(rDest.Width, rDest.Height, 1);
            Matrix matTranslation = Matrix.Translation(x, y, 1);
            Matrix matTransform = matScaling * matTranslation * transform;

            device.SetTransform(TransformState.World, matTransform);
            device.SetTexture(0, texture);

            device.VertexShader = null;

            if (palette != null)
            {
                device.PixelShader = pixelShader;
                device.SetTexture(1, palette);
            }
            else
                device.PixelShader = null;

            device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Point);
            device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Point);
            device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 2);*/
        }

        public void PaintTexture(Texture texture, MMXBox box, Matrix transform)
        {
            RenderTexture(texture, null, box, transform);
        }

        public void PaintTexture(Texture texture, Vector v, Matrix transform)
        {
            var description = texture.GetLevelDescription(0);
            RenderTexture(texture, null, new MMXBox(v.X, v.Y, description.Width, description.Height), transform);
        }

        public void PaintTexture(Texture texture, FixedSingle x, FixedSingle y, Matrix transform)
        {
            var description = texture.GetLevelDescription(0);
            RenderTexture(texture, null, new MMXBox(x, y, description.Width, description.Height), transform);
        }

        public void PaintTexture(Texture texture, Texture palette, Vector v, Matrix transform)
        {
            var description = texture.GetLevelDescription(0);
            RenderTexture(texture, palette, new MMXBox(v.X, v.Y, description.Width, description.Height), transform);
        }

        public void PaintTexture(Texture texture, Texture palette, FixedSingle x, FixedSingle y, Matrix transform)
        {
            var description = texture.GetLevelDescription(0);
            RenderTexture(texture, palette, new MMXBox(x, y, description.Width, description.Height), transform);
        }

        private void OnFrame()
        {
            bool nextFrame = !frameAdvance;
            Keys keys = Keys.NONE;

            if (form.Focused)
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
                        if (player != null)
                            player.NoClip = !player.NoClip;
                    }
                }
                else
                    wasPressingToggleNoClip = false;

                if (state.IsPressed(Key.M))
                {
                    if (!wasPressingToggleCameraConstraints)
                    {
                        wasPressingToggleCameraConstraints = true;
                        noCameraConstraints = !noCameraConstraints;
                    }
                }
                else
                    wasPressingToggleCameraConstraints = false;

                if (state.IsPressed(Key.D1))
                {
                    if (!wasPressingToggleDrawCollisionBox)
                    {
                        wasPressingToggleDrawCollisionBox = true;
                        drawCollisionBox = !drawCollisionBox;
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

                /*if (state.IsPressed(Key.D0))
                {
                    if (!wasPressingToggleDrawX)
                    {
                        wasPressingToggleDrawX = true;
                        drawX = !drawX;
                    }
                }
                else
                    wasPressingToggleDrawX = false;*/

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
                    catch (SharpDXException)
                    {
                        joystick = null;
                    }
                }
            }

            if (!nextFrame)
                return;

            engineTime++;

            if (!loadingLevel)
            {
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

                if (addedEntities.Count > 0)
                {
                    foreach (Entity added in addedEntities)
                    {
                        entities.Add(added);
                        partition.Insert(added);
                        added.index = entities.Count - 1;
                    }

                    addedEntities.Clear();
                }

                if (player != null)
                    player.PushKeys(keys);

                if (paused)
                    player.OnFrame();
                else
                {
                    foreach (Entity obj in entities)
                    {
                        if (changeLevel)
                            break;

                        if (!obj.MarkedToRemove)
                            obj.OnFrame();
                    }

                    if (removedEntities.Count > 0)
                    {
                        foreach (Entity removed in removedEntities)
                        {
                            entities.Remove(removed);
                            partition.Remove(removed);

                            removed.Dispose();
                        }

                        removedEntities.Clear();
                    }
                }
            }

            world.OnFrame();

            if (changeLevel)
            {
                changeLevel = false;
                LoadLevel(levelToChange);
            }

            if (gameOver)
            {

            }
        }

        public static Vector2 ToVector2(Vector v)
        {
            return new Vector2((float) v.X, (float) v.Y);
        }

        public static Vector3 ToVector3(Vector v)
        {
            return new Vector3((float) v.X, (float) v.Y, 0);
        }

        public static Rectangle ToRectangle(MMXBox box)
        {
            return new Rectangle((int) box.Left, (int) box.Top, (int) box.Width, (int) box.Height);
        }

        public static RectangleF ToRectangleF(MMXBox box)
        {
            return new RectangleF((float) box.Left, (float) box.Top, (float) box.Width, (float) box.Height);
        }

        public Vector2 WorldVectorToScreen(Vector v)
        {
            return ToVector2(((v - world.Screen.LeftTop.Round()) * drawScale + drawBox.Origin));
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
            return (new Vector(p.X, p.Y) - drawBox.Origin) / drawScale + world.Screen.LeftTop;
        }

        public Vector ScreenVector2ToWorld(Vector2 v)
        {
            return (new Vector(v.X, v.Y) - drawBox.Origin) / drawScale + world.Screen.LeftTop;
        }

        public RectangleF WorldBoxToScreen(MMXBox box)
        {
            return ToRectangleF(((box.LeftTopOrigin() - world.Screen.LeftTop.Round()) * drawScale + drawBox.Origin));
        }

        public long GetEngineTime()
        {
            return engineTime;
        }

        internal void RepaintHP()
        {
            //Invalidate(TransformBox(DEFAULT_HEART_BOX), false);
            //Update();
        }

        internal void RepaintLives()
        {
            //Invalidate(TransformBox(DEFAULT_LIVES_BOX), false);
            //Update();
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
            //Invalidate();
        }

        public void ContinueGame()
        {
            paused = false;
            //Invalidate();
        }

        public void ScheduleRespawn(Player player)
        {
            ScheduleRespawn(player, RESPAWN_TIME);
        }

        public void ScheduleRespawn(Player player, FixedSingle time)
        {
            scheduleRespawns.Add(new RespawnEntry(player, GetEngineTime() + time));
        }

        public void NextLevel()
        {
            changeLevel = true;
            levelToChange = currentLevel + 1;
        }

        internal void OnGameOver()
        {
            gameOver = true;
            //nextGameOverThink = engineTime + GAME_OVER_PANEL_SHOW_DELAY;
        }

        private void SpawnPlayer()
        {
            Vector spawnPos;
            Vector cameraPos;
            if (romLoaded)
            {
                spawnPos = mmx.CharacterPos + Vector.DOWN_VECTOR * (HITBOX_HEIGHT + 1);
                cameraPos = mmx.CameraPos;
            }
            else
            {
                spawnPos = new Vector(SCREEN_WIDTH * 0.5f, 0);
                cameraPos = new Vector(SCREEN_WIDTH * 0.5f, 0);
            }

            world.Screen.LeftTop = cameraPos;

            player = new Player(this, "X", spawnPos, xSpriteSheet);
            player.Palette = x1NormalPalette;
            player.Spawn();

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

                if (romLoaded)
                {
                    mmx.SetLevel((ushort) level, 0);
                    mmx.LoadLevel();
                    mmx.LoadToWorld(world);
                    mmx.LoadTriggers(this);
                    mmx.LoadBackground();
                    mmx.LoadToWorld(world, true);
                }

                world.Tessellate();

                Player oldPlayer = player;
                SpawnPlayer();

                if (oldPlayer != null)
                {
                    player.Lives = oldPlayer.Lives;
                }

                loadingLevel = false;
            }
        }

        private void UnloadLevel()
        {
            foreach (Entity obj in addedEntities)
            {
                Sprite sprite = obj as Sprite;
                if (sprite != null)
                    sprite.Dispose();
            }

            foreach (Entity obj in entities)
            {
                Sprite sprite = obj as Sprite;
                if (sprite != null)
                    sprite.Dispose();
            }

            entities.Clear();
            addedEntities.Clear();
            removedEntities.Clear();
            scheduleRespawns.Clear();
            partition.Clear();

            long currentMemoryUsage = GC.GetTotalMemory(true);
            long delta = currentMemoryUsage - lastCurrentMemoryUsage;
            Debug.WriteLine("**************************Total memory: {0}({1}{2})", currentMemoryUsage, delta > 0 ? "+" : delta < 0 ? "-" : "", delta);
            lastCurrentMemoryUsage = currentMemoryUsage;
        }

        internal void UpdateScale()
        {
            FixedSingle width = form.ClientSize.Width;
            FixedSingle height = form.ClientSize.Height;

            Vector drawOrigin;
            /*if (width / height < SIZE_RATIO)
            {
                drawScale = width / DEFAULT_CLIENT_WIDTH;
                MMXFloat newHeight = drawScale * DEFAULT_CLIENT_HEIGHT;
                drawOrigin = new MMXVector(0, (height - newHeight) / 2);
                height = newHeight;
            }
            else
            {
                drawScale = height / DEFAULT_CLIENT_HEIGHT;
                MMXFloat newWidth = drawScale * DEFAULT_CLIENT_WIDTH;
                drawOrigin = new MMXVector((width - newWidth) / 2, 0);
                width = newWidth;
            }*/
            drawOrigin = Vector.NULL_VECTOR;

            drawBox = new MMXBox(drawOrigin.X, drawOrigin.Y, width, height);
        }

        /*private void DrawLine(FixedSingle x1, FixedSingle y1, FixedSingle x2, FixedSingle y2, SolidColorBrush brush)
        {
            context.DrawLine(WorldVectorToScreen(x1, y1), WorldVectorToScreen(x2, y2), brush);
        }

        private void DrawLine(FixedSingle x1, FixedSingle y1, FixedSingle x2, FixedSingle y2, SolidColorBrush brush, float strokeWidth)
        {
            context.DrawLine(WorldVectorToScreen(x1, y1), WorldVectorToScreen(x2, y2), brush, strokeWidth);
        }

        private void DrawLine(Vector v1, Vector v2, SolidColorBrush brush)
        {
            context.DrawLine(WorldVectorToScreen(v1), WorldVectorToScreen(v2), brush);
        }

        private void DrawLine(Vector v1, Vector v2, SolidColorBrush brush, float strokeWidth)
        {
            context.DrawLine(WorldVectorToScreen(v1), WorldVectorToScreen(v2), brush, strokeWidth);
        }

        private void DrawSlopeMap(MMXBox box, RightTriangle triangle, float strokeWidth)
        {
            Vector tv1 = triangle.Origin;
            Vector tv2 = triangle.HCathetusVertex;
            Vector tv3 = triangle.VCathetusVertex;

            DrawLine(tv2, tv3, touchingMapBrush, strokeWidth);

            FixedSingle h = tv1.Y - box.Top;
            FixedSingle H = MAP_SIZE - h;
            if (H > 0)
            {
                if (triangle.HCathetusVector.X < 0)
                {
                    DrawLine(tv2, box.LeftBottom, touchingMapBrush, strokeWidth);
                    DrawLine(tv3, box.RightBottom, touchingMapBrush, strokeWidth);
                }
                else
                {
                    DrawLine(tv3, box.LeftBottom, touchingMapBrush, strokeWidth);
                    DrawLine(tv2, box.RightBottom, touchingMapBrush, strokeWidth);
                }

                DrawLine(box.LeftBottom, box.RightBottom, touchingMapBrush, strokeWidth);
            }
            else
            {
                DrawLine(tv3, tv1, touchingMapBrush, strokeWidth);
                DrawLine(tv1, tv2, touchingMapBrush, strokeWidth);
            }
        }

        private void DrawHighlightMap(int row, int col, CollisionData collisionData)
        {
            MMXBox mapBox = World.GetMapBoundingBox(row, col);
            if (World.IsSolidBlock(collisionData))
                context.DrawRectangle(WorldBoxToScreen(mapBox), touchingMapBrush, 4);
            else if (World.IsSlope(collisionData))
            {
                RightTriangle st = World.MakeSlopeTriangle(collisionData) + mapBox.LeftTop;
                DrawSlopeMap(mapBox, st, 4);
            }
        }

        private void CheckAndDrawTouchingMap(int row, int col, CollisionData collisionData, MMXBox collisionBox, bool ignoreSlopes = false)
        {
            MMXBox halfCollisionBox1 = new MMXBox(collisionBox.Left, collisionBox.Top, collisionBox.Width / 2, collisionBox.Height);
            MMXBox halfCollisionBox2 = new MMXBox(collisionBox.Left + collisionBox.Width / 2, collisionBox.Top, collisionBox.Width / 2, collisionBox.Height);

            MMXBox mapBox = World.GetMapBoundingBox(row, col);
            if (World.IsSolidBlock(collisionData) && (mapBox & collisionBox).Area > 0)
                context.DrawRectangle(WorldBoxToScreen(mapBox), touchingMapBrush, 4);
            else if (!ignoreSlopes && World.IsSlope(collisionData))
            {
                RightTriangle st = World.MakeSlopeTriangle(collisionData) + mapBox.LeftTop;
                Vector hv = st.HCathetusVector;
                if (hv.X > 0 && st.HasIntersectionWith(halfCollisionBox2, true) || hv.X < 0 && st.HasIntersectionWith(halfCollisionBox1, true))
                    DrawSlopeMap(mapBox, st, 4);
            }
        }

        private void CheckAndDrawTouchingMaps(MMXBox collisionBox, bool ignoreSlopes = false)
        {
            Cell start = World.GetMapCellFromPos(collisionBox.LeftTop);
            Cell end = World.GetMapCellFromPos(collisionBox.RightBottom);

            int startRow = start.Row;
            int startCol = start.Col;

            if (startRow < 0)
                startRow = 0;

            if (startRow >= world.MapRowCount)
                startRow = world.MapRowCount - 1;

            if (startCol < 0)
                startCol = 0;

            if (startCol >= world.MapColCount)
                startCol = world.MapColCount - 1;

            int endRow = end.Row;
            int endCol = end.Col;

            if (endRow < 0)
                endRow = 0;

            if (endRow >= world.MapRowCount)
                endRow = world.MapRowCount - 1;

            if (endCol < 0)
                endCol = 0;

            if (endCol >= world.MapColCount)
                endCol = world.MapColCount - 1;

            for (int row = startRow; row <= endRow; row++)
                for (int col = startCol; col <= endCol; col++)
                {
                    Vector v = new Vector(col * MAP_SIZE, row * MAP_SIZE);
                    Map map = world.GetMapFrom(v);
                    if (map != null)
                        CheckAndDrawTouchingMap(row, col, map.CollisionData, collisionBox, ignoreSlopes);
                }
        }*/

        public void Render()
        {
            OnFrame();

            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            device.BeginScene();

            Matrix orthoLH = Matrix.OrthoLH(SCREEN_WIDTH, SCREEN_HEIGHT, 1.0f, 10.0f);
            device.SetTransform(TransformState.Projection, orthoLH);
            device.SetTransform(TransformState.World, Matrix.Identity);
            device.SetTransform(TransformState.View, Matrix.Identity);

            if (world != null)
            {
                //Vector screenLT = world.Screen.LeftTop;
                //device.Viewport = new Viewport((int) screenLT.X, (int) screenLT.Y, (int) world.Screen.Width, (int) world.Screen.Height);

                if (drawBackground)
                {
                    world.RenderBackground(false);
                    world.RenderBackground(true);
                }

                if (drawDownLayer)
                {
                    world.RenderForeground(false);
                }

                if (drawSprites)
                {
                    // Desenha os sprites
                    List<Entity> objects = partition.Query(world.Screen.BoudingBox);
                    foreach (Entity obj in objects)
                    {
                        if (obj.MarkedToRemove || obj.Equals(player))
                            continue;

                        Sprite sprite = obj as Sprite;
                        if (sprite != null)
                        {
                            sprite.Paint();

                            /*if (drawCollisionBox)
                            {
                                MMXBox collisionBox = sprite.CollisionBox;
                                context.FillRectangle(WorldBoxToScreen(collisionBox), hitBoxBrush);
                                context.DrawRectangle(WorldBoxToScreen(collisionBox), hitBoxBorderBrush, 1);
                            }*/
                        }
                        else
                        {
                            /*AbstractTrigger trigger = obj as AbstractTrigger;
                            if (trigger != null && DEBUG_SHOW_TRIGGERS)
                            {
                                context.DrawRectangle(WorldBoxToScreen(trigger.BoundingBox), triggerBoxBrush, 4);
                                CameraEventTrigger camTrigger = trigger as CameraEventTrigger;
                                if (camTrigger != null)
                                {
                                    Vector extensionOrigin = camTrigger.ExtensionOrigin;
                                    for (int i = 0; i < camTrigger.ExtensionCount; i++)
                                    {
                                        Vector extension = camTrigger.GetExtension(i);
                                        context.DrawLine(WorldVectorToScreen(extensionOrigin), WorldVectorToScreen(extensionOrigin + extension), cameraEventExtensionBrush, 4);
                                    }
                                }
                            }*/
                        }
                    }
                }

                if (drawX)
                {
                    // Desenha o X
                    player.Paint();
                }

                if (drawUpLayer)
                {
                    world.RenderForeground(true);
                }

                //PaintTexture(world.foregroundTilemap, world.Palette, new MMXBox(0, 0, World.TILEMAP_WIDTH, World.TILEMAP_HEIGHT), 0, false, false);
                //PaintTexture(world.backgroundTilemap, world.Palette, new MMXBox(0, World.TILEMAP_WIDTH, World.TILEMAP_WIDTH, World.TILEMAP_HEIGHT), -2, false, false);

                /*if (drawTouchingMapBounds)
                {
                    MMXBox collisionBox = player.CollisionBox;

                    CheckAndDrawTouchingMaps(collisionBox + Vector.LEFT_VECTOR, true);
                    CheckAndDrawTouchingMaps(collisionBox + Vector.UP_VECTOR);
                    CheckAndDrawTouchingMaps(collisionBox + Vector.RIGHT_VECTOR, true);
                    CheckAndDrawTouchingMaps(collisionBox + Vector.DOWN_VECTOR);
                }

                if (drawCollisionBox)
                {
                    MMXBox collisionBox = player.CollisionBox;
                    context.FillRectangle(WorldBoxToScreen(collisionBox), hitBoxBrush);
                    context.DrawRectangle(WorldBoxToScreen(collisionBox), hitBoxBorderBrush, 1);
                }

                if (showColliders)
                {
                    BoxCollider collider = player.Collider;
                    context.FillRectangle(WorldBoxToScreen(collider.DownCollider.ClipBottom(collider.MaskSize - 1)), downColliderBrush);
                    context.FillRectangle(WorldBoxToScreen(collider.UpCollider.ClipTop(collider.MaskSize - 1)), upColliderBrush);
                    context.FillRectangle(WorldBoxToScreen(collider.LeftCollider.ClipLeft(collider.MaskSize - 1)), leftColliderBrush);
                    context.FillRectangle(WorldBoxToScreen(collider.RightCollider.ClipRight(collider.MaskSize - 1)), rightColliderBrush);
                }

                if (drawHighlightedPointingTiles)
                {
                    System.Drawing.Point cursorPos = form.PointToClient(Cursor.Position);
                    Vector v = ScreenPointToVector(cursorPos.X, cursorPos.Y);
                    context.DrawText(string.Format("Mouse Pos: X: {0} Y: {1}", v.X, v.Y), highlightMapTextFormat, new RectangleF(0, 0, 400, 50), touchingMapBrush);

                    Scene scene = world.GetSceneFrom(v, false);
                    if (scene != null)
                    {
                        Cell sceneCell = World.GetSceneCellFromPos(v);
                        MMXBox sceneBox = World.GetSceneBoundingBox(sceneCell);
                        context.DrawRectangle(WorldBoxToScreen(sceneBox), touchingMapBrush, 4);
                        context.DrawText(string.Format("Scene: ID: {0} Row: {1} Col: {2}", scene.ID, sceneCell.Row, sceneCell.Col), highlightMapTextFormat, new RectangleF(0, 50, 400, 50), touchingMapBrush);

                        Block block = world.GetBlockFrom(v, false);
                        if (block != null)
                        {
                            Cell blockCell = World.GetBlockCellFromPos(v);
                            MMXBox blockBox = World.GetBlockBoundingBox(blockCell);
                            context.DrawRectangle(WorldBoxToScreen(blockBox), touchingMapBrush, 4);
                            context.DrawText(string.Format("Block: ID: {0} Row: {1} Col: {2}", block.ID, blockCell.Row, blockCell.Col), highlightMapTextFormat, new RectangleF(0, 100, 400, 50), touchingMapBrush);

                            Map map = world.GetMapFrom(v, false);
                            if (map != null)
                            {
                                Cell mapCell = World.GetMapCellFromPos(v);
                                MMXBox mapBox = World.GetMapBoundingBox(mapCell);
                                context.DrawRectangle(WorldBoxToScreen(mapBox), touchingMapBrush, 4);
                                context.DrawText(string.Format("Map: ID: {0} Row: {1} Col: {2}", map.ID, mapCell.Row, mapCell.Col), highlightMapTextFormat, new RectangleF(0, 150, 400, 50), touchingMapBrush);

                                Tile tile = world.GetTileFrom(v, false);
                                if (tile != null)
                                {
                                    Cell tileCell = World.GetTileCellFromPos(v);
                                    MMXBox tileBox = World.GetTileBoundingBox(tileCell);
                                    context.DrawRectangle(WorldBoxToScreen(tileBox), touchingMapBrush, 4);
                                    context.DrawText(string.Format("Tile: ID: {0} Row: {1} Col: {2}", tile.ID, tileCell.Row, tileCell.Col), highlightMapTextFormat, new RectangleF(0, 200, 400, 50), touchingMapBrush);
                                }
                            }
                        }
                    }
                }*/

                RectangleF drawRect = RenderRectangle;

                //if (DEBUG_DRAW_BOX)
                //    target.DrawRectangle(drawRect, screenBoxBrush, 4);

                /*if (drawPlayerOriginAxis)
                {
                    Vector2 v = WorldVectorToScreen(player.Origin);
                    context.DrawLine(new Vector2(v.X, drawRect.Top), new Vector2(v.X, drawRect.Bottom), playerOriginBrush, 1);
                    context.DrawLine(new Vector2(drawRect.Left, v.Y - 17), new Vector2(drawRect.Right, v.Y - 17), playerOriginBrush, 1);
                }

                if (showCheckpointBounds && currentCheckpoint != null)
                    context.DrawRectangle(WorldBoxToScreen(currentCheckpoint.BoundingBox), checkpointBoxBrush, 4);

                if (showInfoText)
                    context.DrawText(string.Format("X: {0} Y: {1} VX: {2} VY: {3} Checkpoint: {4}", (int) ((float) player.Origin.X * 256), (int) (((float) player.Origin.Y - 17) * 256), (int) ((float) player.Velocity.X * 256), (int) ((float) player.Velocity.Y * -256), currentCheckpoint != null ? currentCheckpoint.Index.ToString() : "none"), coordsTextFormat, new RectangleF(drawRect.Left, drawRect.Bottom - 50, drawRect.Width, 50), coordsTextBrush);*/
            }

            device.EndScene();
            device.Present();
        }

        public void Dispose()
        {
            world.Dispose();

            if (player != null)
                player.Dispose();

            if (mmx != null)
                mmx.Dispose();

            xSpriteSheet.Dispose();
        }

        /*public MMXVector CheckCollisionWithTiles(MMXBox collisionBox, MMXVector dir, CollisionFlags ignore = CollisionFlags.NONE)
        {
            return world.CheckCollision(collisionBox, dir, ignore);
        }*/

        public CollisionFlags GetTouchingFlags(MMXBox collisionBox, Vector dir, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true)
        {
            return world.GetTouchingFlags(collisionBox, dir, ignore, preciseCollisionCheck);
        }

        public CollisionFlags GetTouchingFlags(MMXBox collisionBox, Vector dir, out RightTriangle slopeTriangle, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true)
        {
            return world.GetTouchingFlags(collisionBox, dir, out slopeTriangle, ignore, preciseCollisionCheck);
        }

        public CollisionFlags GetTouchingFlags(MMXBox collisionBox, Vector dir, List<CollisionPlacement> placements, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true)
        {
            return world.GetTouchingFlags(collisionBox, dir, placements, ignore, preciseCollisionCheck);
        }

        public CollisionFlags GetTouchingFlags(MMXBox collisionBox, Vector dir, List<CollisionPlacement> placements, out RightTriangle slopeTriangle, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true)
        {
            return world.GetTouchingFlags(collisionBox, dir, placements, out slopeTriangle, ignore, preciseCollisionCheck);
        }

        /*public Box MoveContactSolid(Box box, Vector dir, Fixed maxDistance, Fixed maskSize, CollisionFlags ignore = CollisionFlags.NONE)
        {
            return world.MoveContactSolid(box, dir, maxDistance, maskSize, ignore);
        }

        public Box MoveContactSolid(Box box, Vector dir, out RightTriangle slope, Fixed maxDistance, Fixed maskSize, CollisionFlags ignore = CollisionFlags.NONE)
        {
            return world.MoveContactSolid(box, dir, out slope, maxDistance, maskSize, ignore);
        }*/

        public MMXBox MoveContactFloor(MMXBox box, FixedSingle maxDistance, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE)
        {
            return world.MoveContactFloor(box, maxDistance, maskSize, ignore);
        }

        public MMXBox MoveContactFloor(MMXBox box, out RightTriangle slope, FixedSingle maxDistance, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE)
        {
            return world.MoveContactFloor(box, out slope, maxDistance, maskSize, ignore);
        }

        public CollisionFlags GetCollisionFlags(MMXBox collisionBox, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true, CollisionSide side = CollisionSide.INNER)
        {
            return world.GetCollisionFlags(collisionBox, ignore, preciseCollisionCheck, side);
        }

        public CollisionFlags GetCollisionFlags(MMXBox collisionBox, out RightTriangle slope, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true, CollisionSide side = CollisionSide.INNER)
        {
            return world.GetCollisionFlags(collisionBox, out slope, ignore, preciseCollisionCheck, side);
        }

        public CollisionFlags GetCollisionFlags(MMXBox collisionBox, List<CollisionPlacement> placements, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true, CollisionSide side = CollisionSide.INNER)
        {
            return world.GetCollisionFlags(collisionBox, placements, ignore, preciseCollisionCheck, side);
        }

        public CollisionFlags GetCollisionFlags(MMXBox collisionBox, List<CollisionPlacement> placements, out RightTriangle slope, CollisionFlags ignore = CollisionFlags.NONE, bool preciseCollisionCheck = true, CollisionSide side = CollisionSide.INNER)
        {
            return world.GetCollisionFlags(collisionBox, placements, out slope, ignore, preciseCollisionCheck, side);
        }

        public CollisionFlags ComputedLandedState(MMXBox box, out RightTriangle slope, FixedSingle maskSize, CollisionFlags ignore = CollisionFlags.NONE)
        {
            return world.ComputedLandedState(box, out slope, maskSize, ignore);
        }

        private void SaveState(BinaryWriter writer)
        {
            var seedArrayInfo = typeof(Random).GetField("SeedArray", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var seedArray = seedArrayInfo.GetValue(random) as int[];
            writer.Write(seedArray.Length);
            for (int i = 0; i < seedArray.Length; i++)
                writer.Write(seedArray[i]);

            if (currentCheckpoint != null)
                writer.Write(currentCheckpoint.Point);

            cameraConstraintsBox.Write(writer);

            writer.Write(gameOver);
            writer.Write(currentStageMusic != null ? currentStageMusic : "");
            writer.Write(paused);

            World.Screen.Center.Write(writer);
            writer.Write(World.Screen.FocusOn.Index);

            if (player != null)
                player.SaveState(writer);
        }

        private void LoadState(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            var seedArray = new int[count];
            for (int i = 0; i < seedArray.Length; i++)
                seedArray[i] = reader.ReadInt32();

            var seedArrayInfo = typeof(Random).GetField("SeedArray", BindingFlags.NonPublic | BindingFlags.Instance);
            seedArrayInfo.SetValue(random, seedArray);

            int checkPointIndex = reader.ReadInt32();
            currentCheckpoint = checkPointIndex != -1 ? (Checkpoint) entities[checkPointIndex] : null;

            cameraConstraintsBox = new MMXBox(reader);

            gameOver = reader.ReadBoolean();
            currentStageMusic = reader.ReadString();
            if (currentStageMusic.Equals(""))
                currentStageMusic = null;

            paused = reader.ReadBoolean();

            World.Screen.Center = new Vector(reader);
            int focusedObjectIndex = reader.ReadInt32();
            World.Screen.FocusOn = entities[focusedObjectIndex];

            if (player != null)
                player.LoadState(reader);
        }

        public void SaveState(int slot = -1)
        {
            if (slot == -1)
                slot = currentSaveSlot;

            string fileName = @"sstates\state." + slot;
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            using (FileStream stream = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    SaveState(writer);
                }
            }
        }

        public void LoadState(int slot = -1)
        {
            if (slot == -1)
                slot = currentSaveSlot;

            try
            {
                string fileName = @"sstates\state." + slot;
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                using (FileStream stream = new FileStream(fileName, FileMode.Open))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        LoadState(reader);
                    }
                }
            }
            catch (IOException)
            {
            }
        }

        public Checkpoint AddCheckpoint(int index, MMXBox boundingBox, Vector characterPos, Vector cameraPos, Vector backgroundPos, Vector forceBackground, uint scroll)
        {
            Checkpoint checkpoint = new Checkpoint(this, index, boundingBox, characterPos, cameraPos, backgroundPos, forceBackground, scroll);
            addedEntities.Add(checkpoint);
            return checkpoint;
        }

        public CameraLockTrigger AddCameraEventTrigger(MMXBox boundingBox, IEnumerable<Vector> extensions)
        {
            CameraLockTrigger trigger = new CameraLockTrigger(this, boundingBox, extensions);
            addedEntities.Add(trigger);
            return trigger;
        }

        public void SetExtensions(Vector origin, IEnumerable<Vector> extensions)
        {
            extensionOrigin = origin;

            this.extensions.Clear();
            this.extensions.AddRange(extensions);

            if (currentCheckpoint != null)
                currentCheckpoint.UpdateBoudingBox();
        }

        public void AddExtension(Vector extension)
        {
            extensions.Add(extension);
        }

        public Vector GetExtension(int index)
        {
            return extensions[index];
        }

        public bool ContainsExtension(Vector extension)
        {
            return extensions.Contains(extension);
        }

        public void ClearExtensions()
        {
            extensions.Clear();
        }

        internal void ShootLemon(Player shooter, Vector origin, Direction direction, bool dashLemon)
        {
            BusterLemon lemon = new BusterLemon(this, shooter, "X Buster Lemon", origin, direction, dashLemon, xWeaponsSpriteSheet);
            lemon.Spawn();
        }

        internal void ShootSemiCharged(Player shooter, Vector origin, Direction direction)
        {
            BusterSemiCharged semiCharged = new BusterSemiCharged(this, shooter, "X Buster Semi Charged", origin, direction, xWeaponsSpriteSheet);
            semiCharged.Spawn();
        }

        public static void WriteTriangle(DataStream vbData, Vector r0, Vector r1, Vector r2, Vector t0, Vector t1, Vector t2)
        {
            WriteVertex(vbData, (float) r0.X, (float) r0.Y, (float) t0.X, (float) t0.Y);
            WriteVertex(vbData, (float) r1.X, (float) r1.Y, (float) t1.X, (float) t1.Y);
            WriteVertex(vbData, (float) r2.X, (float) r2.Y, (float) t2.X, (float) t2.Y);
        }

        public static void WriteSquare(DataStream vbData, Vector vSource, Vector vDest, Vector srcSize, Vector dstSize)
        {
            if (vSource.X < 0 || vSource.X > 1 || vSource.Y < 0 || vSource.Y > 1)
                throw new Exception();

            Vector r0 = new Vector(vDest.X, vDest.Y);
            Vector r1 = new Vector(vDest.X + dstSize.X, vDest.Y);
            Vector r2 = new Vector(vDest.X + dstSize.X, vDest.Y - dstSize.Y);
            Vector r3 = new Vector(vDest.X, vDest.Y - dstSize.Y);

            Vector t0 = new Vector(vSource.X, vSource.Y);
            Vector t1 = new Vector(vSource.X + srcSize.X, vSource.Y);
            Vector t2 = new Vector(vSource.X + srcSize.X, vSource.Y + srcSize.Y);
            Vector t3 = new Vector(vSource.X, vSource.Y + srcSize.Y);

            if (t0.X < 0 || t0.X > 1 || t0.Y < 0 || t0.Y > 1)
                throw new Exception();

            if (t1.X < 0 || t1.X > 1 || t1.Y < 0 || t1.Y > 1)
                throw new Exception();

            if (t2.X < 0 || t2.X > 1 || t2.Y < 0 || t2.Y > 1)
                throw new Exception();

            if (t3.X < 0 || t3.X > 1 || t3.Y < 0 || t3.Y > 1)
                throw new Exception();

            WriteTriangle(vbData, r0, r1, r2, t0, t1, t2);
            WriteTriangle(vbData, r0, r2, r3, t0, t2, t3);
        }

        public void RenderVertexBuffer(VertexBuffer vb, int vertexSize, int primitiveCount, Texture texture, Texture palette, MMXBox box)
        {
            device.SetStreamSource(0, vb, 0, vertexSize);

            RectangleF rDest = WorldBoxToScreen(box);

            float x = rDest.Left - (float) world.Screen.Width * 0.5f;
            float y = -rDest.Top + (float) world.Screen.Height * 0.5f;

            Matrix matScaling = Matrix.Scaling(1, 1, 1);
            Matrix matTranslation = Matrix.Translation(x, y, 1);
            Matrix matTransform = matScaling * matTranslation;

            device.SetTransform(TransformState.World, matTransform);
            device.SetTransform(TransformState.View, Matrix.Identity);
            device.SetTransform(TransformState.Texture0, Matrix.Identity);
            device.SetTransform(TransformState.Texture1, Matrix.Identity);
            device.SetTexture(0, texture);

            device.VertexShader = null;

            if (palette != null)
            {
                device.PixelShader = pixelShader;
                device.SetTexture(1, palette);
            }
            else
                device.PixelShader = null;

            device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Point);
            device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Point);
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, primitiveCount);
        }

        public Texture CreatePalette(Color[] colors)
        {
            if (colors.Length > 256)
                throw new ArgumentException("Length of colors should up to 256.");

            Texture palette = new Texture(device, 256, 1, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
            DataRectangle rect = palette.LockRectangle(0, LockFlags.None);

            using (DataStream stream = new DataStream(rect.DataPointer, 256 * 1 * sizeof(int), true, true))
            {
                for (int i = 0; i < colors.Length; i++)
                    stream.Write(colors[i].ToBgra());

                for (int i = colors.Length; i < 256; i++)
                    stream.Write(0);
            }

            palette.UnlockRectangle(0);
            return palette;
        }

        public static int LookupColor(Texture palette, Color color)
        {
            return LookupColor(palette, color, 0, 256);
        }

        public static int LookupColor(Texture palette, Color color, int start, int count)
        {
            DataRectangle rect = palette.LockRectangle(0, LockFlags.Discard);
            try
            {
                using (DataStream stream = new DataStream(rect.DataPointer, 256 * 1 * sizeof(int), true, true))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        for (int i = start; i < start + count; i++)
                        {
                            int bgra = reader.ReadInt32();
                            Color c = Color.FromBgra(bgra);
                            if (color == c)
                                return i;
                        }
                    }
                }
            }
            finally
            {
                palette.UnlockRectangle(0);
            }

            return -1;
        }
    }
}
