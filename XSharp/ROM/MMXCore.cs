/*
 * The code below was taken from the MegaEDX v1.3 project. Such code was originally written in C++ which I translated to C#.
 * 
 * For more information, consult the original projects:

    MegaEDX: https://github.com/Xeeynamo/MegaEdX
    MegaEDX v1.3: https://github.com/rbrummett/megaedx_v1.3
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using SharpDX;
using SharpDX.Direct3D9;

using MMX.Math;
using MMX.Geometry;
using MMX.Engine;
using MMX.Engine.World;

using static MMX.Engine.Consts;

using MMXBox = MMX.Geometry.Box;

namespace MMX.ROM
{
    public struct STileInfo
    {
        internal uint num;
        internal byte value;
        internal Dictionary<byte, uint> count;

        internal STileInfo(uint num, byte value)
        {
            this.num = num;
            this.value = value;

            count = new Dictionary<byte, uint>();
        }
    };

    public struct CheckPointInfo
    {
        internal uint offset;

        // checkpoint data structure
        internal uint objLoad;
        internal uint tileLoad;
        internal uint palLoad;
        // X2/X3 extra byte
        internal uint byte0;
        internal uint chX;
        internal uint chY;
        internal uint camX;
        internal uint camY;
        internal uint bkgX;
        internal uint bkgY;
        internal uint minX;
        internal uint maxX;
        internal uint minY;
        internal uint maxY;
        internal uint forceX;
        internal uint forceY;
        internal uint scroll;
        internal uint telDwn;
        // X2/X3 extra byte
        internal uint byte1;
        // X3 extra byte
        internal uint byte2;

        internal void Reset()
        {
            offset = 0;
            objLoad = 0;
            tileLoad = 0;
            palLoad = 0;
            chX = 0;
            chY = 0;
            camX = 0;
            camY = 0;
            bkgX = 0;
            bkgY = 0;
            minX = 0;
            maxX = 0;
            minY = 0;
            maxY = 0;
            forceX = 0;
            forceY = 0;
            scroll = 0;
            telDwn = 0;
            byte0 = 0;
            byte1 = 0;
            byte2 = 0;
        }
    };

    public struct EventInfo
    {
        internal byte match; // seems to match some level information?
        internal byte type; // 0=?, 1=cars,lights?, 2=?, 3=enemy
        internal ushort ypos;
        internal byte eventId;
        internal byte eventSubId;
        internal byte eventFlag;
        internal ushort xpos;
    };

    public struct PropertyInfo
    {
        internal uint hp;
        internal uint damageMod;

        internal void Reset()
        {
            hp = 0;
            damageMod = 0;
        }
    };

    public class MMXCore : SNESCore
    {
        internal const uint NUM_SPRITE_TILES = 0x2000;
        internal const uint NUM_SPRITE_PALETTES = 0x200;

        internal static readonly byte[] vrambase = {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00,
            0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00,
            0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00,
            0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff,
            0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00,
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff,
            0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00,
            0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff,
            0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff,
            0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff,
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00,
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff,
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
        };

        internal static readonly uint[] p_layout = { 0x868D24, 0x868888, 0x8689B3, 0x808199 };
        internal static readonly uint[] p_scenes = { 0x868D93, 0x8688F7, 0x868A22, 0x808257 };
        internal static readonly uint[] p_blocks = { 0x868E02, 0x868966, 0x868A91, 0 };
        internal static readonly uint[] p_maps = { 0x868E71, 0x8689D5, 0x868B00, 0x8081B3 };
        internal static readonly uint[] p_collis = { 0x868EE0, 0x868A44, 0x868B6F, 0 };
        /*const*/
        internal static readonly uint[] p_checkp = { 0x86A780, 0x86A4C5, 0x86A8E4, 0 };
        internal static readonly uint[] p_palett = { 0x868133, 0x86817A, 0x868180, 0 };
        internal static readonly uint[] p_font = { 0x86F744, 0x86FA4C, 0x86F77D, 0 };
        internal static readonly uint[] p_unknow = { 0x86A1D5, 0, 0, 0 }; // Unknow
        internal static readonly uint[] p_gfxcfg = { 0x86F56F, 0x86F831, 0x86F3C3, 0x80B75B };
        internal static readonly uint[] p_gfxpos = { 0x86F6F7, 0x86F9FF, 0x86F730, 0x81E391 };
        internal static readonly uint[] p_events = { 0x8582C2, 0x29D3D1, 0x3CCE4B, 0x80C18B };
        internal static readonly uint[] p_borders = { 0x86E4E2, 0x82EBE9, 0x83DE43, 0 };
        internal static readonly uint[] p_locks = { 0x86ECD0, 0x82FAE4, 0x83F2CC, 0 };
        internal static readonly uint[] p_properties = { 0, 0, 0x86E28E, 0 };
        internal static readonly uint[] p_spriteAssembly = { 0x8D8000, 0x8D8000, 0x8D8000, 0 };
        internal static readonly uint[] p_spriteOffset = { 0x86A5E4, 0x86A34D, 0x86E28E, 0 };
        internal static readonly uint[] p_objOffset = { 0x86DE9B, 0x86A34D, 0, 0 };

        // enemy
        internal static readonly uint[] p_gfxobj = { 0x86ACEE, 0xAAB2D4, 0x888623, 0 };
        internal static readonly uint[] p_gfxpal = { 0x86ACF1, 0xAAB2D7, 0x888626, 0 };
        // TODO: sprite
        // TODO: misc object

        // capsule
        internal static readonly uint[] p_capsulepos = { 0, 0x86D6F1, 0, 0 };

        internal static readonly uint[] p_blayout = { 0x868F4F, 0x868AB3, 0x868BDE, 0 };
        internal static readonly uint[] p_bscenes = { 0x868FBE, 0x868B22, 0x868C4D, 0 };
        internal static readonly uint[] p_bblocks = { 0x86902D, 0x868B91, 0x868CBC, 0 };

        private byte type;
        private readonly byte[] vram;
        private readonly ushort[] mapping;
        private readonly byte[] mappingBG;
        private readonly byte[] sceneLayout;

        private readonly uint[] palettesOffset;
        private uint tileCmpPos;
        private ushort tileCmpDest;
        private ushort tileCmpSize;
        private ushort tileCmpRealSize;
        private uint tileDecPos;
        private ushort tileDecDest;
        private ushort tileDecSize;

        // checkpoints
        private readonly List<CheckPointInfo> checkpointInfoTable;

        public void UpdateVRAMCache()
        {
            for (int i = 0; i < 0x400; i++)
                Raw2tile4bpp(vramCache, i * 0x40, vram, i * 0x20);
        }

        // graphics to palette
        private readonly Dictionary<uint, uint> graphicsToPalette;
        private readonly Dictionary<uint, uint> graphicsToAssembly;
        private byte levelWidth;
        private byte levelHeight;
        private byte sceneUsed;
        private uint pPalette;
        private uint pPalBase;
        private uint pLayout;
        private uint pScenes;
        private uint pBlocks;
        private uint pMaps;
        private uint pCollisions;
        private readonly uint pEvents;
        private uint pBorders;
        private uint pLocks;
        private readonly uint pProperties;
        private uint pGfx;
        private readonly uint pGfxPos;
        private uint pGfxObj;
        private uint pGfxPal;
        private uint pSpriteAssembly;
        private readonly uint[] pSpriteOffset;
        private uint pCapsulePos;

        private uint numLevels;
        private uint numTiles;
        private uint numMaps;
        private uint numBlocks;
        private uint numDecs;
        private uint numCheckpoints;
        private readonly uint numGfxIds;

        private int objLoad;
        private int tileLoad;
        private int palLoad;

        private uint tileDecStart;
        private uint tileDecEnd;

        private bool sortOk;

        // ROM expansion
        private static readonly string expandedROMString = "EXPANDED ROM ";
        private static readonly ushort expandedROMVersion;
        private static readonly uint expandedROMHeaderSize;
        private static readonly uint expandedROMTrampolineOffset;

        private uint eventBank;
        private uint checkpointBank;
        private uint lockBank;
        private bool expandedROM;
        private uint expandedVersion;
        private uint expandedLayoutSize;
        private uint expandedEventSize;
        private readonly uint expandedSceneSize;
        private uint expandedCheckpointSize;
        private uint expandedLayoutScenes;

        // Events
        private readonly List<EventInfo>[] eventTable;

        // Properties
        private readonly PropertyInfo[] propertyTable;

        // SPRITE
        private readonly HashSet<uint> spriteUpdate;

        // FONT
        private readonly ushort[] fontPalCache;
        private readonly byte[] fontCache;

        public ushort Level
        {
            get;
            private set;
        }

        public Vector BackgroundPos
        {
            get
            {
                if (checkpointInfoTable == null || checkpointInfoTable.Count == 0)
                    return Vector.NULL_VECTOR;

                FixedSingle x = ReadWord(checkpointInfoTable[Point].bkgX);
                FixedSingle y = ReadWord(checkpointInfoTable[Point].bkgY);
                return new Vector(x, y);
            }
        }

        public Vector CameraPos
        {
            get
            {
                if (checkpointInfoTable == null || checkpointInfoTable.Count == 0)
                    return Vector.NULL_VECTOR;

                FixedSingle x = ReadWord(checkpointInfoTable[Point].camX);
                FixedSingle y = ReadWord(checkpointInfoTable[Point].camY);
                return new Vector(x, y);
            }
        }

        public Vector CharacterPos
        {
            get
            {
                if (checkpointInfoTable == null || checkpointInfoTable.Count == 0)
                    return Vector.NULL_VECTOR;

                FixedSingle x = ReadWord(checkpointInfoTable[Point].chX);
                FixedSingle y = ReadWord(checkpointInfoTable[Point].chY);
                return new Vector(x, y);
            }
        }

        public Vector MinCharacterPos
        {
            get
            {
                if (checkpointInfoTable == null || checkpointInfoTable.Count == 0)
                    return Vector.NULL_VECTOR;

                FixedSingle x = ReadWord(checkpointInfoTable[Point].minX);
                FixedSingle y = ReadWord(checkpointInfoTable[Point].minY);
                return new Vector(x, y);
            }
        }

        public Vector MaxCharacterPos
        {
            get
            {
                if (checkpointInfoTable == null || checkpointInfoTable.Count == 0)
                    return Vector.NULL_VECTOR;

                FixedSingle x = ReadWord(checkpointInfoTable[Point].maxX) + SCREEN_WIDTH;
                FixedSingle y = ReadWord(checkpointInfoTable[Point].maxY) + SCREEN_HEIGHT;
                return new Vector(x, y);
            }
        }

        public ushort Point
        {
            get;
            set;
        }

        public int CheckpointCount => (int) numCheckpoints;

        public int ObjLoad
        {
            get => checkpointInfoTable.Count == 0 || objLoad >= 0 ? objLoad : rom[checkpointInfoTable[Point].objLoad];
            set => objLoad = value;
        }

        public int TileLoad
        {
            get => checkpointInfoTable.Count == 0 || tileLoad >= 0 ? tileLoad : rom[checkpointInfoTable[Point].tileLoad];
            set => tileLoad = value;
        }

        public int PalLoad
        {
            get => checkpointInfoTable.Count == 0 || palLoad >= 0 ? palLoad : rom[checkpointInfoTable[Point].palLoad];
            set => palLoad = value;
        }

        public MMXCore()
        {
            type = 0xff;
            vram = new byte[0x10000];
            mapping = new ushort[0x10 * 0x10 * 0x100];
            mappingBG = new byte[0x10 * 0x10 * 0x100 * 0x2];
            sceneLayout = new byte[0x400];

            palettesOffset = new uint[16];

            checkpointInfoTable = new List<CheckPointInfo>();

            graphicsToPalette = new Dictionary<uint, uint>();
            graphicsToAssembly = new Dictionary<uint, uint>();

            pSpriteOffset = new uint[4];

            eventTable = new List<EventInfo>[256];
            for (int i = 0; i < 256; i++)
                eventTable[i] = new List<EventInfo>();

            propertyTable = new PropertyInfo[256];

            spriteUpdate = new HashSet<uint>();

            fontPalCache = new ushort[0x20];
            fontCache = new byte[0x4800];
        }

        internal byte CheckROM()
        {
            LoadHeader();
            uint expandedHeader = 0;

            uint headerTitleInt0 = header.GetTitleUInt(0);
            uint headerTitleInt4 = header.GetTitleUInt(4);
            ushort headerTitleShort8 = header.GetTitleUShort(8);

            if (headerTitleInt0 is 0x4147454D or 0x6167654D or 0x4B434F52)
                if (headerTitleInt4 is 0x204E414D or 0x206E616D or 0x264E414D)
                {
                    switch (headerTitleShort8)
                    {
                        case 0x2058:
                            //Megaman X1
                            type = 0;
                            numLevels = 13;
                            eventBank = (p_events[type]) >> 16;
                            checkpointBank = (p_checkp[type]) >> 16;
                            lockBank = (p_borders[type]) >> 16;
                            expandedROM = header.romSize == 0xC && romSize == 0x280000 && "EXPANDED ROM  " == ReadASCIIString(0x180000 + 0x8000 - expandedROMHeaderSize, 14);
                            if (expandedROM)
                            {
                                eventBank = 0xB2;
                                checkpointBank = 0x93;
                                lockBank = 0xBB;
                                expandedHeader = 0x180000 + 0x8000 - expandedROMHeaderSize;
                            }

                            break;
                        case 0x3258:
                            //Megaman X2
                            type = 1;
                            numLevels = 13;
                            eventBank = (p_events[type]) >> 16;
                            expandedROM = header.romSize == 0xC && romSize == 0x280000 && "EXPANDED ROM  " == ReadASCIIString(0x180000 + 0x8000 - expandedROMHeaderSize, 14);
                            checkpointBank = (p_checkp[type]) >> 16;
                            lockBank = (p_borders[type]) >> 16;
                            if (expandedROM)
                            {
                                eventBank = 0xB2;
                                checkpointBank = (p_events[type]) >> 16;
                                lockBank = 0xBB;
                                expandedHeader = 0x180000 + 0x8000 - expandedROMHeaderSize;
                            }

                            break;
                        case 0x3358:
                            //Megaman X3
                            type = 2;
                            numLevels = 15;
                            eventBank = (p_events[type]) >> 16;
                            checkpointBank = (p_checkp[type]) >> 16;
                            lockBank = (p_borders[type]) >> 16;
                            expandedROM = header.romSize == 0xC && romSize == 0x300000 && "EXPANDED ROM  " == ReadASCIIString(0x200000 + 0x8000 - expandedROMHeaderSize, 14);
                            if (expandedROM)
                            {
                                eventBank = 0xC2;
                                checkpointBank = (p_events[type]) >> 16;
                                lockBank = 0xCB;
                                expandedHeader = 0x200000 + 0x8000 - expandedROMHeaderSize;
                            }

                            break;
                        case 0x2026:
                        case 0x4F46:
                            //Rockman & Forte. English & Japanese??
                            type = 3;
                            numLevels = 13;
                            eventBank = (p_events[type]) >> 16;
                            checkpointBank = (p_checkp[type]) >> 16;
                            lockBank = (p_borders[type]) >> 16;
                            expandedROM = header.romSize == 0xD && romSize == 0x600000 && "EXPANDED ROM  " == ReadASCIIString(0x200000 + 0x8000 - expandedROMHeaderSize, 14);
                            if (expandedROM)
                            {
                                // FIXME:
                                eventBank = (p_events[type]) >> 16;
                                checkpointBank = (p_checkp[type]) >> 16;
                                lockBank = (p_borders[type]) >> 16;
                                expandedHeader = 0x400000 + 0x8000 - expandedROMHeaderSize;
                            }

                            break;
                        default:
                            type = 0xFF;
                            expandedROM = false;
                            return 0;
                    }

                    if (expandedROM)
                    {
                        expandedVersion = ReadWord(expandedHeader + 0xE);
                        expandedLayoutSize = 0;
                        expandedEventSize = 0;
                        expandedCheckpointSize = 0;
                        expandedLayoutScenes = 0;

                        if (expandedVersion == 0)
                        {
                            expandedLayoutSize = 0x800;
                            expandedLayoutScenes = 0x40;
                        }

                        if (expandedVersion >= 1)
                        {
                            expandedLayoutSize = ReadWord(expandedHeader + 0x10);
                            expandedEventSize = ReadWord(expandedHeader + 0x12);
                            expandedLayoutScenes = 0x40;
                        }

                        if (expandedVersion >= 3)
                        {
                            expandedCheckpointSize = ReadWord(expandedHeader + 0x14);
                            expandedLayoutScenes = 0x40;
                        }
                    }

                    return (byte) (type + 1);
                }

            type = 0xFF;
            return 0;
        }

        internal uint GetFontPointer()
        {
            if (type < 3)
            {
                uint address = Snes2pc((int) p_font[type]);
                ushort offset = ReadWord(address); //0xc180;
                return Snes2pc(offset + ((type == 0) ? 0x9C0000 : 0x1C0000));
            }
            else
            {
                uint pConfigGfx = Snes2pc(ReadWord(Snes2pc((int) (p_gfxcfg[type] + 0x0))) | 0x80 << 16);
                byte gfxID = rom[pConfigGfx];
                //tileCmpSize = ReadWord(pConfigGfx + 1); //SReadWord(p_gfxpos[type] + gfxID * 5 + 3);
                //tileCmpDest = (ReadWord(pConfigGfx + 3) << 1) - 0x2000;
                tileCmpPos = Snes2pc((int) ReadDWord(Snes2pc((int) (p_gfxpos[type] + gfxID * 5 + 0))));
                return tileCmpPos;
            }
        }

        internal void LoadFont()
        {
            byte[] textCache = new byte[0x2000];
            CompressionCore.GFXRLE(rom, 0, textCache, 0, (int) GetFontPointer(), 0x1000, type);

            for (int i = 0; i < 0x20; i++) // Decompress the 32 colors
                fontPalCache[i] = Get16Color((uint) (((type == 0) ? 0x2D140 : (type == 1) ? 0x2CF20 : (type == 2) ? 0x632C0 : 0x50000) + i * 2)); // 0x2D3E0
            for (int i = 0; i < 0x100; i++)
            { // Decompress all 256 tiles in ram
                int tempChar = (type == 0) ? i : i + 0x10;
                Tile2bpp2raw(textCache, i * 0x10, fontCache, tempChar * 0x40 + 0x400);
            }

            return; //why is there a return for a void function that doesn't return anything?
        }

        internal uint GetCheckPointPointer(uint p) =>
            //notice the bitwise operations
            Snes2pc((int) (((p_checkp[type] & 0xFFFF) | (checkpointBank << 16)) + SReadWord(p_checkp[type] + SReadWord((uint) (p_checkp[type] + Level * 2)) + p * 2)));

        internal uint GetCheckPointBasePointer() => Snes2pc((int) (((p_checkp[type] & 0xFFFF) | (checkpointBank << 16)) + SReadWord(p_checkp[type] + SReadWord((uint) (p_checkp[type] + Level * 2)) + 0 * 2)));

        private static readonly ushort[,] origEventSize = { { 0x2c8,0x211,0x250,0x4b3,0x2ea,0x32c,0x2e2,0x260,0x2d2,0x37f,0x254,0x2b2,0x27,0 },
                                        { 0x235,0x4a7,0x338,0x489,0x310,0x382,0x3b6,0x3da,0x45c,0x303,0x212,0x30f,0xbd,0 },
                                        { 0x2f1,0x3b4,0x3a7,0x3d9,0x3da,0x455,0x3c9,0x405,0x33b,0x22b,0x3cb,0x2ba,0x274,0xe6 } };

        //unsigned GetEventSize();
        internal uint GetOrigEventSize() => expandedROM ? expandedEventSize : origEventSize[type, Level];

        private static readonly ushort[,] origLayoutSize = { { 0x12, 0x32, 0x38, 0x64, 0x22, 0x3a, 0x1e, 0x6a, 0x2a, 0x3c, 0x22, 0x1a, 0x00 },
                                        { 0x8c, 0x3e, 0x38, 0x40, 0x42, 0x5c, 0x2a, 0x4e, 0x5e, 0x5a, 0x16, 0x5a, 0x00 },
                                        { 0x4c, 0x4c, 0x38, 0x42, 0x60, 0x54, 0x4e, 0x52, 0x30, 0x2e, 0x4e, 0x46, 0x22 } };

        internal uint GetOrigLayoutSize() => expandedROM ? expandedLayoutSize : origLayoutSize[type, Level];

        internal System.Drawing.Rectangle GetBoundingBox(ref EventInfo e)
        {
            var rect = new System.Drawing.Rectangle(0, 0, 0, 0);

            if (e.type == 0x2 && e.eventId == 0 && pLocks != 0)
            {
                if (pLocks != 0)
                {
                    // look up the subid to get the camera lock

                    uint b = 0;
                    if (expandedROM && expandedROMVersion >= 4)
                    {
                        b = Snes2pc((int) (lockBank << 16) | (0x8000 + Level * 0x800 + e.eventSubId * 0x20));
                    }
                    else
                    {
                        ushort borderOffset = ReadWord((uint) (Snes2pc(pBorders) + 2 * e.eventSubId));
                        b = Snes2pc(borderOffset | ((pBorders >> 16) << 16));
                    }

                    int right = ReadShort(b);
                    b += 2;
                    int left = ReadShort(b);
                    b += 2;
                    int bottom = ReadShort(b);
                    b += 2;
                    int top = ReadShort(b);
                    b += 2;

                    rect = new System.Drawing.Rectangle(left, top, right - left, bottom - top);
                }
            }
            else if (e.type == 0x2 && e.eventId >= 0x15 && e.eventId <= 0x18)
            {
                // draw green line
                int left = e.xpos + ((e.eventId & 0x8) != 0 ? -128 : -5);
                int top = e.ypos + ((e.eventId & 0x8) == 0 ? -112 : -5);
                int bottom = e.ypos + ((e.eventId & 0x8) == 0 ? 112 : 5);
                int right = e.xpos + ((e.eventId & 0x8) != 0 ? 128 : 5);

                rect = new System.Drawing.Rectangle(left, top, right - left, bottom - top);
            }
            else if (pSpriteAssembly != 0 && pSpriteOffset[e.type] != 0
                && (e.type != 1 || type == 0 && e.eventId == 0x21)
                && (e.type != 0 || e.eventId == 0xB && e.eventSubId == 0x4)
                && !(type == 1 && e.eventId == 0x2) // something near the arm doesn't have graphics
                )
            {
                // draw associated object sprite

                uint assemblyNum = ReadDWord((uint) (pSpriteOffset[e.type] + (e.eventId - 1) * (type == 2 ? 5 : 2)));

                // workarounds for some custom types
                if (type == 0 && e.type == 1 && e.eventId == 0x21)
                {
                    // X1 highway trucks/cars
                    assemblyNum = (uint) (((e.eventSubId & 0x30) >> 4) + 0x3A);
                }
                else if (e.type == 0 && e.eventId == 0xB && e.eventSubId == 0x4)
                {
                    // X1/X2 heart tank
                    assemblyNum = 0x38;
                }

                uint mapAddr = ReadDWord(Snes2pc((int) ReadDWord(pSpriteAssembly + assemblyNum * 3)) + 0);

                uint baseMap = Snes2pc((int) mapAddr);
                byte tileCnt = rom[baseMap++];

                var boundingBox = new System.Drawing.Rectangle(0, 0, ushort.MaxValue, ushort.MaxValue);

                for (int i = 0; i < tileCnt; ++i)
                {
                    uint map = (uint) (baseMap + (tileCnt - i - 1) * 4);
                    sbyte xpos = 0;
                    sbyte ypos = 0;
                    uint tile = 0;
                    uint info = 0;

                    if (type == 0)
                    {
                        xpos = (sbyte) rom[map++];
                        ypos = (sbyte) rom[map++];
                        tile = rom[map++];
                        info = rom[map++];
                    }
                    else
                    {
                        xpos = (sbyte) rom[map + 1];
                        ypos = (sbyte) rom[map + 2];
                        tile = rom[map + 3];
                        info = rom[map + 0];

                        map += 4;
                    }

                    if (type == 2)
                    {
                        // temporary fix for the boss sprites that have assembly information that is off by 0x20 or 0x40.
                        tile -= (uint) ((assemblyNum is 0x61 or 0x92) ? 0x20 :
                            (assemblyNum is 0x68 or 0x79 or 0xae) ? 0x40 :
                            0x0);
                        tile &= 0xFF;
                    }

                    bool largeSprite = (info & 0x20) != 0;

                    for (int j = 0; j < (largeSprite ? 4 : 1); j++)
                    {
                        int xposOffset = j % 2 * 8;
                        int yposOffset = j / 2 * 8;

                        int screenX = e.xpos + xpos + xposOffset;
                        int screenY = e.ypos + ypos + yposOffset;

                        int left = boundingBox.Left;
                        int top = boundingBox.Top;
                        int right = boundingBox.Right;
                        int bottom = boundingBox.Bottom;

                        if (screenX < left)
                            left = screenX;

                        if (right < screenX + 8)
                            right = screenX + 8;

                        if (screenY < top)
                            top = screenY;

                        if (bottom < screenY + 8)
                            bottom = screenY + 8;

                        boundingBox = new System.Drawing.Rectangle(left, top, right - left, bottom - top);
                    }
                }

                rect = boundingBox;
            }
            else
            {
                int left = e.xpos - 5;
                int top = e.ypos - 5;
                int bottom = e.ypos + 5;
                int right = e.xpos + 5;

                rect = new System.Drawing.Rectangle(left, top, right - left, bottom - top);
            }

            return rect;
        }

        internal bool ExpandROM()
        {
            bool ok = true;
            uint a = 0;

            switch (type)
            {
                case 0:
                {
                    ok &= header.romSize == 0xB;
                    ok &= romSize == 0x180000;
                    break;
                }
                case 1:
                {
                    ok &= header.romSize == 0xB;
                    ok &= romSize == 0x180000 || romSize == 0x200000 && Compare(0x100000, 0x180000, 0x80000) == 0;
                    break;
                }
                case 2:
                {
                    ok &= header.romSize == 0xB;
                    ok &= romSize == 0x200000;
                    break;
                }
                case 3:
                {
                    ok &= header.romSize == 0xC;
                    ok &= romSize == 0x400000;
                    break;
                }
                default:
                {
                    ok = false;
                    break;
                }
            }

            if (type < 3)
            {
                if (ok)
                {
                    // make the ROM bigger
                    // Add 1MB
                    Fill((uint) romSize, 0xFF, 0x100000);
                    uint arg1 = (uint) (romSize + 0x8000 - expandedROMHeaderSize);
                    WriteASCIIString(arg1, expandedROMString);
                    WriteWord((uint) (romSize + 0x8000 - expandedROMHeaderSize + 0xE), expandedROMVersion);
                    ushort v = 0x800;
                    WriteWord((uint) (romSize + 0x8000 - expandedROMHeaderSize + 0x10), v);
                    v = 0x800;
                    WriteWord((uint) (romSize + 0x8000 - expandedROMHeaderSize + 0x12), v);
                    v = 0x10;
                    WriteWord((uint) (romSize + 0x8000 - expandedROMHeaderSize + 0x14), v);
                    // sceneUsed already stored in the ROM
                }

                uint currentOffset = (uint) romSize;

                if (ok)
                {
                    // relocate the layout for each level
                    for (int i = 0; i < numLevels; ++i)
                    {
                        ushort pLevel = (ushort) (i * 3);
                        var levelLayout = Snes2pc((int) SReadDWord(p_layout[type] + pLevel));
                        uint layout = levelLayout;

                        uint count;
                        for (count = 3; rom[layout + count] != 0xFF; ++count)
                            ;
                        count++;

                        if (count <= 0x800)
                        {
                            Array.Copy(rom, layout, rom, currentOffset + i * 0x800, count);
                        }
                        else
                        {
                            ok = false;
                            break;
                        }
                    }

                    currentOffset += 0x10 * 0x800;
                }

                if (ok)
                {
                    // relocate the background layout for each level
                    for (int i = 0; i < numLevels; ++i)
                    {
                        ushort pLevel = (ushort) (i * 3);
                        var levelLayout = Snes2pc((int) SReadDWord(p_blayout[type] + pLevel));
                        uint layout = levelLayout;

                        uint count;
                        for (count = 3; rom[layout + count] != 0xFF; ++count)
                            ;
                        count++;

                        if (count <= 0x800)
                        {
                            Array.Copy(rom, layout, rom, currentOffset + i * 0x800, count);
                        }
                        else
                        {
                            ok = false;
                            break;
                        }
                    }

                    currentOffset += 0x10 * 0x800;
                }

                if (ok)
                {
                    // copy events.  fix addresses in ROM
                    for (int i = 0; i < numLevels; ++i)
                    {
                        uint pEvents = Snes2pc((int) (SReadWord((uint) (p_events[type] + i * 2)) | (eventBank << 16)));
                        uint pevent = pEvents;
                        uint peventBase = pevent;

                        uint count = 0;

                        uint blockId = 0xFF;
                        uint nextBlockId = rom[pevent++];
                        count++;

                        while (blockId != nextBlockId && blockId < 0x100)
                        {
                            bool eventDone = true;

                            blockId = nextBlockId;
                            do
                            {
                                eventDone = (ReadWord(pevent + 5) & 0x8000) != 0;

                                pevent += 7;
                                count += 7;
                            } while (!eventDone);

                            // get the next id
                            nextBlockId = rom[pevent++];
                            count++;
                        }

                        if (count <= 0x800)
                        {
                            Array.Copy(rom, peventBase, rom, currentOffset + i * 0x800, count);
                        }
                        else
                        {
                            ok = false;
                            break;
                        }
                    }

                    currentOffset += 0x10 * 0x800;
                }

                if (ok)
                {
                    // relocate the scenes
                    for (int i = 0; i < numLevels; ++i)
                    {
                        ushort pLevel = (ushort) (i * 3);
                        uint sceneLayout = Snes2pc((int) SReadDWord(p_scenes[type] + pLevel));
                        uint layout = sceneLayout;

                        uint levelLayout = Snes2pc((int) SReadDWord(p_layout[type] + pLevel));
                        uint s = rom[levelLayout + 2];

                        // copy the existing scene data
                        Array.Copy(rom, layout, rom, currentOffset + i * 0x800 * 0x80, s * 0x80);
                        Array.Clear(rom, (int) (currentOffset + i * 0x800 * 0x80 + s * 0x80), (int) (0x80 * 0x80 - s * 0x80));
                    }

                    currentOffset += 0x10 * 0x80 * 0x80;
                }

                if (ok)
                {
                    if (type == 0)
                    {
                        if ((ReadWord(Snes2pc(0x80DB92)) != 0x85A9 || ReadWord(Snes2pc(0x80DBD9)) != 0x85A9 || ReadWord(Snes2pc(0x80DD4C)) != 0x85A9)
                            && (ReadWord(Snes2pc(0x80DB7C)) != 0x85A9 || ReadWord(Snes2pc(0x80DBC3)) != 0x85A9 || ReadWord(Snes2pc(0x80DD36)) != 0x85A9))
                        {
                            ok = false;
                        }
                    }
                    else if (type == 1)
                    {
                        if (ReadWord(Snes2pc(0x80DB96)) != 0x29A9 || ReadWord(Snes2pc(0x80DBDD)) != 0x29A9 || ReadWord(Snes2pc(0x80DD44)) != 0x29A9)
                        {
                            ok = false;
                        }
                    }
                    else if (type == 2)
                    {
                        // FIXME: X3 has a problem where banks >= 0xC0 don't have RAM shadowed into the lower offsets
                        // so LDs to ROM intermixed with STs to RAM fail.  This is a problem for the events but not the layout
                        if (ReadWord(Snes2pc(0x80DD80)) != 0x3CA9 || ReadWord(Snes2pc(0x80DDC7)) != 0x3CA9 || ReadWord(Snes2pc(0x80DF2E)) != 0x3CA9)
                        {
                            ok = false;
                        }
                    }
                    else
                    {
                        ok = false;
                    }
                }

                if (ok)
                {
                    uint pEvents = Snes2pc((int) (SReadWord(p_events[type] + 0x300) | (eventBank << 16)));
                    uint pevent = pEvents;

                    //for (unsigned i = 0x0; i < 0x2000; i++) {
                    //	if (*(pevent + i) != 0xFF) {
                    //		ok = false;
                    //		break;
                    //	}
                    //}

                    uint pFunc = Snes2pc(0x80FF00);
                    uint func = pFunc;
                    for (int i = 0x0; i < 0x90; i++)
                    {
                        if (rom[func + i] != 0xFF)
                        {
                            ok = false;
                            break;
                        }
                    }
                }

                if (ok)
                {
                    expandedROM = true;

                    // remove copy protection crap
                    if (type == 0)
                    {
                        // reset upgrades
                        rom[Snes2pc(0x81824E)] = 0x00;
                        // reset checkpoint
                        rom[Snes2pc(0x849FC7)] = 0x00;
                        // 1UP drop reset stage
                        rom[Snes2pc(0x84A41F)] = 0x00;

                        //*LPBYTE(rom + snes2pc(0x84A47F)) = 0xEA;
                        //*LPBYTE(rom + snes2pc(0x84A480)) = 0xEA;
                        //*LPBYTE(rom + snes2pc(0x84A3CC)) = 0x80;
                    }
                    else if (type == 1)
                    {
                    }
                    else if (type == 2)
                    {
                    }

                    // remove old events
                    for (int i = 0; i < numLevels; ++i)
                    {
                        uint pEvents1 = Snes2pc((int) (SReadWord((uint) (p_events[type] + i * 2)) | (eventBank << 16)));
                        uint pevent = pEvents1;
                        uint peventBase = pevent;

                        uint count = 0;

                        uint blockId = 0xFF;
                        uint nextBlockId = rom[pevent++];
                        rom[pevent - 1] = 0xFF;
                        count++;

                        while (blockId != nextBlockId && blockId < 0x100)
                        {
                            bool eventDone = true;

                            blockId = nextBlockId;
                            do
                            {
                                eventDone = (ReadWord(pevent + 5) & 0x8000) != 0;

                                Fill(pevent, 0xFF, 7);
                                pevent += 7;
                                count += 7;
                            } while (!eventDone);

                            // get the next id
                            nextBlockId = rom[pevent++];
                            rom[pevent - 1] = 0xFF;
                            count++;
                        }
                    }

                    // layout
                    for (int i = 0; i < numLevels; ++i)
                    {
                        uint addr = Pc2snes(romSize + i * 0x800);

                        var levelLayout = Snes2pc((int) SReadDWord((uint) (p_layout[type] + i * 3)));
                        uint s = rom[levelLayout + 2];
                        // overwrite the layout data
                        for (uint l = levelLayout + 3; rom[l] != 0xFF; l++)
                            rom[l] = 0xFF;

                        Array.Copy(rom, addr, rom, Snes2pc((int) (p_layout[type] + i * 3)), 3);
                    }

                    // background layout
                    for (int i = 0; i < numLevels; ++i)
                    {
                        uint addr = Pc2snes(romSize + 0x8000 + i * 0x800);

                        var levelLayout = Snes2pc((int) SReadDWord((uint) (p_blayout[type] + i * 3)));
                        uint s = rom[levelLayout + 2];
                        // overwrite the layout data
                        for (uint l = levelLayout + 3; rom[l] != 0xFF; l++)
                            rom[l] = 0xFF;

                        Array.Copy(rom, addr, rom, Snes2pc((int) (p_blayout[type] + i * 3)), 3);
                    }

                    // scenes
                    uint bank = Pc2snes(romSize + 2 * 0x8000 + 0 * 0x800);
                    if (type == 0)
                    {
                        eventBank = bank >> 16;
                        uint offsetAddr = Pc2snes(romSize + 3 * 0x8000 - 2 * 0x10);
                        if (ReadWord(Snes2pc(0x80DB92)) == 0x85A9 || ReadWord(Snes2pc(0x80DBD9)) == 0x85A9 || ReadWord(Snes2pc(0x80DD4C)) == 0x85A9)
                        {
                            // 1.0
                            WriteWord(Snes2pc(0x80DB92), (ushort) (0xA9 | (eventBank << 8)));
                            WriteWord(Snes2pc(0x80DBD9), (ushort) (0xA9 | (eventBank << 8)));
                            WriteWord(Snes2pc(0x80DD4C), (ushort) (0xA9 | (eventBank << 8)));
                            WriteWord(Snes2pc(0x80DB9E), (ushort) (offsetAddr & 0xFFFF));
                        }
                        else
                        {
                            // 1.1
                            WriteWord(Snes2pc(0x80DB7C), (ushort) (0xA9 | (eventBank << 8)));
                            WriteWord(Snes2pc(0x80DBC3), (ushort) (0xA9 | (eventBank << 8)));
                            WriteWord(Snes2pc(0x80DD36), (ushort) (0xA9 | (eventBank << 8)));
                            WriteWord(Snes2pc(0x80DB88), (ushort) (offsetAddr & 0xFFFF));
                        }
                    }
                    else if (type == 1)
                    {
                        eventBank = bank >> 16;
                        WriteWord(Snes2pc(0x80DB96), (ushort) (0xA9 | (eventBank << 8)));
                        WriteWord(Snes2pc(0x80DBDD), (ushort) (0xA9 | (eventBank << 8)));
                        WriteWord(Snes2pc(0x80DD44), (ushort) (0xA9 | (eventBank << 8)));

                        uint offsetAddr = Pc2snes(romSize + 3 * 0x8000 - 2 * 0x10);
                        WriteWord(Snes2pc(0x80DBA2), (ushort) (offsetAddr & 0xFFFF));
                    }
                    else if (type == 2)
                    {
                        eventBank = bank >> 16;
                        WriteWord(Snes2pc(0x80DD80), (ushort) (0xA9 | (eventBank << 8)));
                        WriteWord(Snes2pc(0x80DDC7), (ushort) (0xA9 | (eventBank << 8)));

                        uint offsetAddr = Pc2snes(romSize + 3 * 0x8000 - 2 * 0x10);
                        WriteWord(Snes2pc(0x80DD8C), (ushort) (offsetAddr & 0xFFFF));

                        // swap LD and PLB so LD uses correct bank
                        WriteWord(Snes2pc(0x80DD83), 0xAEAD);
                        WriteWord(Snes2pc(0x80DD85), 0xAB1F);

                        // store the bank in the 3rd B for long LD
                        WriteWord(Snes2pc(0x80DF2E), (ushort) (0xA9 | (eventBank << 8)));
                        WriteWord(Snes2pc(0x80DF30), 0x1A85);

                        // NOP the push/pull of the bank register 
                        rom[Snes2pc(0x80DF43)] = 0xEA;
                        rom[Snes2pc(0x80DF4C)] = 0xEA;

                        // change all the offset LDs to long LDs
                        rom[Snes2pc(0x80DF34)] = 0xA7;
                        rom[Snes2pc(0x80DF50)] = 0xB7;
                        rom[Snes2pc(0x80DF56)] = 0xB7;
                        rom[Snes2pc(0x80DF5C)] = 0xB7;
                        rom[Snes2pc(0x80DF62)] = 0xB7;
                        rom[Snes2pc(0x80DF68)] = 0xB7;
                        rom[Snes2pc(0x80DF6E)] = 0xB7;
                    }

                    for (int i = 0; i < numLevels; ++i)
                    {
                        uint addr = Pc2snes(romSize + 3 * 0x8000 + i * 0x800);
                        uint offsetAddr = Pc2snes(romSize + 3 * 0x8000 - 2 * 0x10);
                        WriteWord(Snes2pc((uint) (offsetAddr + i * 2)), (ushort) addr);
                    }

                    // scenes
                    for (int i = 0; i < numLevels; ++i)
                    {
                        ushort pLevel = (ushort) (i * 3);
                        var sceneLayout = Snes2pc((int) SReadDWord(p_scenes[type] + pLevel));
                        uint layout = sceneLayout;

                        var levelLayout = Snes2pc((int) SReadDWord(p_layout[type] + pLevel));
                        uint s = rom[levelLayout + 2];

                        // overwrite the scene data
                        Fill((int) layout, 0xFF, (int) (s * 0x80));

                        uint addr = Pc2snes(romSize + 3 * 0x8000 + i * 0x80 * 0x80);
                        WriteWord(Snes2pc((int) (p_scenes[type] + i * 3)), (ushort) addr);
                        rom[Snes2pc((int) (p_scenes[type] + i * 3)) + 2] = (byte) (addr >> 16);
                        rom[levelLayout + 2] = 0x40;
                    }

                    // checkpoints
                    checkpointBank = (type == 0) ? 0x93 : ((p_events[type]) >> 16);
                    // copy the current checkpoints table
                    uint pEvents = Snes2pc((int) ((type == 0 ? 0x93AD00 : p_events[type]) + 0x300));
                    uint pnewBase = pEvents;
                    byte[] pointers = new byte[0x10 + 0x10 * 0x20];

                    // fix the level base pointers
                    for (int j = 0; j < 0x10; j++)
                    {
                        pointers[j * 2] = (byte) ((0x10 + j * 0x10) * 2);
                    }

                    var offsetSet = new HashSet<uint>();

                    ushort checkpointBaseOffset = (ushort) ((type == 0 ? 0xAD00 : (p_events[type] & 0xFFFF)) + 0x300 - (p_checkp[type] & 0xFFFF));

                    for (int i = 0; i < numLevels; ++i)
                    {
                        ushort levelValue = SReadWord((uint) (p_checkp[type] + i * 2));
                        uint levelOffset = Snes2pc((int) (p_checkp[type] + levelValue));
                        uint pLevel = levelOffset;

                        ushort endValue = (ushort) ((type == 0) ? 0x0072 : (type == 1) ? 0x0098 : 0x0088);
                        for (int j = 0; j < numLevels; j++)
                        {
                            ushort tempValue = SReadWord((uint) (p_checkp[type] + j * 2));
                            if (levelValue < tempValue && tempValue < endValue)
                            {
                                endValue = tempValue;
                            }
                        }

                        uint endOffset = Snes2pc((int) (p_checkp[type] + endValue));
                        uint pEnd = endOffset;

                        uint count = 0;
                        while (pLevel < pEnd && count < 0x10)
                        {
                            ushort checkpointValue = rom[pLevel++];

                            offsetSet.Add(checkpointValue);
                            WriteWord(pointers, (uint) ((0x10 + i * 0x10 + count) * 2), (ushort) (checkpointBaseOffset + checkpointValue));

                            count++;
                        }
                        // set remaining counts as last
                        Fill(pointers, (uint) ((0x10 + i * 0x10 + count) * 2), 0xFF, (int) ((0x10 - count) * 2));
                    }
                    // copy existing checkpoints
                    Copy(Snes2pc((int) p_checkp[type]), pnewBase, (type == 0) ? 0x56E : (type == 1) ? 0x9B1 : 0x728);
                    // copy new pointer table in
                    uint checkpointTableBase = Snes2pc((int) p_checkp[type]);
                    Array.Copy(pointers, 0, rom, checkpointTableBase, pointers.Length);

                    if (type == 0)
                    {
                        // write new base function
                        Copy(Snes2pc(0x80E68E), Snes2pc(0x80FF00), 0x22);
                        Fill(Snes2pc(0x80E68E), 0xFF, 0x22);
                        // set new bank
                        WriteWord(Snes2pc(0x80FF22), (ushort) (0xA9 | (checkpointBank << 8)));
                        rom[Snes2pc(0x80FF24)] = 0x48;
                        rom[Snes2pc(0x80FF25)] = 0xAB;
                        // add back RTS
                        rom[Snes2pc(0x80FF26)] = 0x60;
                        // change all instances of JMP
                        WriteWord(Snes2pc(0x809DA4), 0xFF00);
                        WriteWord(Snes2pc(0x80E602), 0xFF00);
                        WriteWord(Snes2pc(0x80E679), 0xFF00);
                        //*LPWORD(rom + snes2pc(0x80E681)) = 0xFF00;

                        // revert bank + RTL

                        // fix other functions
                        //1
                        // revert bank + JMP + RTS
                        WriteWord(Snes2pc(0x80FF27), 0xA9 | (0x06 << 8));
                        rom[Snes2pc(0x80FF29)] = 0x48;
                        rom[Snes2pc(0x80FF2A)] = 0xAB;
                        // JSR
                        rom[Snes2pc(0x80FF2B)] = 0x20;
                        WriteWord(Snes2pc(0x80FF2C), 0xB117);
                        // RTS
                        rom[Snes2pc(0x80FF2E)] = 0x60;
                        // FIX JMP
                        WriteWord(Snes2pc(0x809DD1), 0xFF27);

                        //2
                        // revert bank + JMP + RTS
                        rom[Snes2pc(0x80FF2F)] = 0xE2;
                        rom[Snes2pc(0x80FF30)] = 0x20;
                        WriteWord(Snes2pc(0x80FF31), 0xA9 | (0x06 << 8));
                        rom[Snes2pc(0x80FF33)] = 0x48;
                        rom[Snes2pc(0x80FF34)] = 0xAB;
                        rom[Snes2pc(0x80FF35)] = 0xC2;
                        rom[Snes2pc(0x80FF36)] = 0x20;
                        // JSL
                        WriteDWord(Snes2pc(0x80FF37), 0x0180E322);
                        // RTS
                        rom[Snes2pc(0x80FF3B)] = 0x60;
                        // FIX JMP
                        WriteDWord(Snes2pc(0x80E66D), 0xEAFF2F20);

                        //3
                        rom[Snes2pc(0x80E68C)] = 0xE2;
                        rom[Snes2pc(0x80E68D)] = 0x20;
                        WriteWord(Snes2pc(0x80E68E), 0xA9 | (0x06 << 8));
                        rom[Snes2pc(0x80E690)] = 0x48;
                        rom[Snes2pc(0x80E691)] = 0xAB;
                        rom[Snes2pc(0x80E692)] = 0xC2;
                        rom[Snes2pc(0x80E693)] = 0x20;
                        // PLP + RTL
                        rom[Snes2pc(0x80E694)] = 0x28;
                        rom[Snes2pc(0x80E695)] = 0x6B;
                    }
                    else if (type == 1)
                    {
                        // write new base function
                        Copy(Snes2pc(0x80E690), Snes2pc(0x80FF00), 0x22);
                        Fill(Snes2pc(0x80E690), 0xFF, 0x22);
                        // set new bank
                        WriteWord(Snes2pc(0x80FF22), (ushort) (0xA9 | (checkpointBank << 8)));
                        rom[Snes2pc(0x80FF24)] = 0x48;
                        rom[Snes2pc(0x80FF25)] = 0xAB;
                        // add back RTS
                        rom[Snes2pc(0x80FF26)] = 0x60;
                        // change all instances of JMP
                        WriteWord(Snes2pc(0x809D79), 0xFF00);
                        WriteWord(Snes2pc(0x80E5ED), 0xFF00);
                        WriteWord(Snes2pc(0x80E661), 0xFF00);
                        WriteWord(Snes2pc(0x80E681), 0xFF00);

                        // revert bank + RTL

                        // fix other functions
                        //1
                        // revert bank + JMP + RTS
                        WriteWord(Snes2pc(0x80FF27), 0xA9 | (0x06 << 8));
                        rom[Snes2pc(0x80FF29)] = 0x48;
                        rom[Snes2pc(0x80FF2A)] = 0xAB;
                        // JSR
                        rom[Snes2pc(0x80FF2B)] = 0x20;
                        WriteWord(Snes2pc(0x80FF2C), 0xADD3);
                        // RTS
                        rom[Snes2pc(0x80FF2E)] = 0x60;
                        // FIX JMP
                        WriteWord(Snes2pc(0x809DA7), 0xFF27);

                        //2
                        // revert bank + JMP + RTS
                        rom[Snes2pc(0x80FF2F)] = 0xE2;
                        rom[Snes2pc(0x80FF30)] = 0x20;
                        WriteWord(Snes2pc(0x80FF31), 0xA9 | (0x06 << 8));
                        rom[Snes2pc(0x80FF33)] = 0x48;
                        rom[Snes2pc(0x80FF34)] = 0xAB;
                        rom[Snes2pc(0x80FF35)] = 0xC2;
                        rom[Snes2pc(0x80FF36)] = 0x20;
                        // JSL
                        WriteDWord(Snes2pc(0x80FF37), 0x01820B22);
                        // RTS
                        rom[Snes2pc(0x80FF3B)] = 0x60;
                        // FIX JMP
                        WriteDWord(Snes2pc(0x80E655), 0xEAFF2F20);

                        //3
                        WriteWord(Snes2pc(0x80E679), 0x80 | (0x13 << 8));

                        //4
                        rom[Snes2pc(0x80E68E)] = 0xE2;
                        rom[Snes2pc(0x80E68F)] = 0x20;
                        WriteWord(Snes2pc(0x80E690), 0xA9 | (0x06 << 8));
                        rom[Snes2pc(0x80E692)] = 0x48;
                        rom[Snes2pc(0x80E693)] = 0xAB;
                        rom[Snes2pc(0x80E694)] = 0xC2;
                        rom[Snes2pc(0x80E695)] = 0x20;
                        // PLP + RTL
                        rom[Snes2pc(0x80E696)] = 0x28;
                        rom[Snes2pc(0x80E697)] = 0x6B;
                    }
                    else if (type == 2)
                    {
                        // write new base function
                        Copy(Snes2pc(0x80E601), Snes2pc(0x80FF00), 0x22);
                        Fill(Snes2pc(0x80E601), 0xFF, 0x22);
                        // set new bank
                        WriteWord(Snes2pc(0x80FF22), (ushort) (0xA9 | (checkpointBank << 8)));
                        rom[Snes2pc(0x80FF24)] = 0x48;
                        rom[Snes2pc(0x80FF25)] = 0xAB;
                        // add back RTS
                        rom[Snes2pc(0x80FF26)] = 0x60;
                        // change all instances of JMP
                        WriteWord(Snes2pc(0x80A1E5), 0xFF00);
                        WriteWord(Snes2pc(0x80E558), 0xFF00);
                        WriteWord(Snes2pc(0x80E5D2), 0xFF00);
                        WriteWord(Snes2pc(0x80E5F2), 0xFF00);

                        // revert bank + RTL

                        // fix other functions
                        //1
                        // revert bank + JMP + RTS
                        WriteWord(Snes2pc(0x80FF27), 0xA9 | (0x06 << 8));
                        rom[Snes2pc(0x80FF29)] = 0x48;
                        rom[Snes2pc(0x80FF2A)] = 0xAB;
                        // JSR
                        rom[Snes2pc(0x80FF2B)] = 0x20;
                        WriteWord(Snes2pc(0x80FF2C), 0xB297);
                        // RTS
                        rom[Snes2pc(0x80FF2E)] = 0x60;
                        // FIX JMP
                        WriteWord(Snes2pc(0x80A213), 0xFF27);

                        //2
                        // revert bank + JMP + RTS
                        rom[Snes2pc(0x80FF2F)] = 0xE2;
                        rom[Snes2pc(0x80FF30)] = 0x20;
                        WriteWord(Snes2pc(0x80FF31), 0xA9 | (0x06 << 8));
                        rom[Snes2pc(0x80FF33)] = 0x48;
                        rom[Snes2pc(0x80FF34)] = 0xAB;
                        rom[Snes2pc(0x80FF35)] = 0xC2;
                        rom[Snes2pc(0x80FF36)] = 0x20;
                        // JSL
                        WriteDWord(Snes2pc(0x80FF37), 0x01827C22);
                        // RTS
                        rom[Snes2pc(0x80FF3B)] = 0x60;
                        // FIX JMP
                        WriteDWord(Snes2pc(0x80E5C6), 0xEAFF2F20);

                        //3
                        WriteWord(Snes2pc(0x80E5EA), 0x80 | (0x13 << 8));

                        //4
                        rom[Snes2pc(0x80E5FF)] = 0xE2;
                        rom[Snes2pc(0x80E600)] = 0x20;
                        WriteWord(Snes2pc(0x80E601), 0xA9 | (0x06 << 8));
                        rom[Snes2pc(0x80E603)] = 0x48;
                        rom[Snes2pc(0x80E604)] = 0xAB;
                        rom[Snes2pc(0x80E605)] = 0xC2;
                        rom[Snes2pc(0x80E606)] = 0x20;
                        // PLP + RTL
                        rom[Snes2pc(0x80E607)] = 0x28;
                        rom[Snes2pc(0x80E608)] = 0x6B;
                    }

                    // <camera locks>
                    // format of new camera lock:
                    // AA BB CC DD EE FF GG HH
                    // A = right
                    // B = left
                    // C = bottom
                    // D = top
                    // E = right lock
                    // F = left lock
                    // G = bottom lock
                    // H = top lock
                    lockBank = Pc2snes(currentOffset) >> 16;
                    Fill(currentOffset, 0x0, 0x8000);
                    // copy the border lock to new flattened format
                    var nextAddress = new Dictionary<uint, uint>();
                    for (int i = 0; i < numLevels; i++)
                    {
                        uint pB = SReadWord((uint) (p_borders[type] + i * 2)) | ((p_borders[type] >> 16) << 16);
                        uint pBNext = SReadWord((uint) (p_borders[type] + (i + 1) * 2)) | ((p_borders[type] >> 16) << 16);

                        if (nextAddress.ContainsKey(pB))
                        {
                            pBNext = nextAddress[pB];
                        }
                        else if (nextAddress.ContainsKey(pBNext))
                        {
                            pBNext = (uint) (pB + (type == 0 ? 0x0 : type == 1 ? 0x0 : 0x20));
                        }

                        nextAddress[pB] = pBNext;

                        for (int j = 0; j < (pBNext - pB) / 2; j++)
                        {
                            uint borderAddress = SReadWord((uint) (pB + j * 2)) | ((p_borders[type] >> 16) << 16);
                            uint b = Snes2pc((int) borderAddress);

                            for (int k = 0; k < 4; k++)
                            {
                                WriteWord((uint) (currentOffset + i * 0x800 + j * 0x20 + k * 2), ReadWord(b));
                                b += 2;
                            }

                            uint newOffset = 0;
                            while (rom[b] != 0)
                            {
                                // load and save lock in proper spot
                                uint lockOffset = rom[b];
                                uint offset = (lockOffset - 1) << 2;

                                ushort camOffset = ReadWord(pLocks + offset + 0x0);
                                ushort camValue = ReadWord(pLocks + offset + 0x2);

                                WriteWord((uint) (currentOffset + i * 0x800 + j * 0x20 + 0x8 + newOffset), camOffset);
                                newOffset += 2;
                                WriteWord((uint) (currentOffset + i * 0x800 + j * 0x20 + 0x8 + newOffset), camValue);
                                newOffset += 2;

                                b++;
                            }

                            for (; newOffset < 0x18; newOffset += 2)
                            {
                                WriteWord((uint) (currentOffset + i * 0x800 + j * 0x20 + 0x8 + newOffset), 0x0);
                            }
                        }
                    }

                    currentOffset += 0x8000;
                    // write new code
                    // fix jump table
                    WriteWord(Snes2pc(type == 0 ? 0x81F6B6 : type == 1 ? 0x82EB2D : 0x83DD87), (ushort) (ReadWord(Snes2pc(type == 0 ? 0x81F6B6 : type == 1 ? 0x82EB2D : 0x83DD87)) - 0x2)); // TYPE

                    a = (uint) (type == 0 ? 0x81F6C4 : type == 1 ? 0x82EB3B : 0x83DD95); // TYPE
                                                                                         // 1 ASL
                    rom[Snes2pc((int) a++)] = 0x0A;
                    // 1 ASL
                    rom[Snes2pc((int) a++)] = 0x0A;
                    // 1 ASL
                    rom[Snes2pc((int) a++)] = 0x0A;
                    // 1 ASL
                    rom[Snes2pc((int) a++)] = 0x0A;
                    // 2 STA 0
                    rom[Snes2pc((int) a++)] = 0x8D;
                    rom[Snes2pc((int) a++)] = 0x00;
                    rom[Snes2pc((int) a++)] = 0x00;
                    // 3 LDA $1F7A
                    rom[Snes2pc((int) a++)] = 0xAD;
                    WriteWord(Snes2pc((int) a++), (ushort) (type == 0 ? 0x1F7A : type == 1 ? 0x1FAD : 0x1FAE)); // TYPE
                    a++;
                    // 3 AND #FF
                    rom[Snes2pc((int) a++)] = 0x29;
                    rom[Snes2pc((int) a++)] = 0xFF;
                    rom[Snes2pc((int) a++)] = 0x00;
                    // 1 XBA
                    rom[Snes2pc((int) a++)] = 0xEB;
                    // 1 ASL
                    rom[Snes2pc((int) a++)] = 0x0A;
                    // 1 ASL
                    rom[Snes2pc((int) a++)] = 0x0A;
                    // 1 ASL
                    rom[Snes2pc((int) a++)] = 0x0A;
                    // 3 ADC 0
                    rom[Snes2pc((int) a++)] = 0x6D;
                    rom[Snes2pc((int) a++)] = 0x00;
                    rom[Snes2pc((int) a++)] = 0x00;
                    // 2 STA D,4
                    rom[Snes2pc((int) a++)] = 0x85;
                    rom[Snes2pc((int) a++)] = 0x04;

                    // 2 REP #30
                    rom[Snes2pc((int) a++)] = 0xC2;
                    rom[Snes2pc((int) a++)] = 0x20 | 0x10;
                    // 2 LDX D,4
                    rom[Snes2pc((int) a++)] = 0xA6;
                    rom[Snes2pc((int) a++)] = 0x04;
                    // 3 LDA $0BAD
                    rom[Snes2pc((int) a++)] = 0xAD;
                    WriteWord(Snes2pc((int) a++), (ushort) (type == 0 ? 0x0BAD : type == 1 ? 0x9DD : 0x9DD)); // TYPE
                    a++;
                    // 4 CMP BANK:8000,X
                    rom[Snes2pc((int) a++)] = 0xDF;
                    rom[Snes2pc((int) a++)] = 0x00;
                    rom[Snes2pc((int) a++)] = 0x80;
                    rom[Snes2pc((int) a++)] = (byte) lockBank;
                    // 2 BCS
                    rom[Snes2pc((int) a++)] = 0xB0;
                    rom[Snes2pc((int) a++)] = 0x2F; // FIXME
                                                    // 4 CMP BANK:8002,X
                    rom[Snes2pc((int) a++)] = 0xDF;
                    rom[Snes2pc((int) a++)] = 0x02;
                    rom[Snes2pc((int) a++)] = 0x80;
                    rom[Snes2pc((int) a++)] = (byte) lockBank;
                    // 2 BCC
                    rom[Snes2pc((int) a++)] = 0x90;
                    rom[Snes2pc((int) a++)] = 0x29; // FIXME
                                                    // 3 LDA $0BB0
                    rom[Snes2pc((int) a++)] = 0xAD;
                    WriteWord(Snes2pc((int) a++), (ushort) (type == 0 ? 0x0BB0 : type == 1 ? 0x9E0 : 0x9E0)); // TYPE
                    a++;
                    // 4 CMP BANK:8004,X
                    rom[Snes2pc((int) a++)] = 0xDF;
                    rom[Snes2pc((int) a++)] = 0x04;
                    rom[Snes2pc((int) a++)] = 0x80;
                    rom[Snes2pc((int) a++)] = (byte) lockBank;
                    // 2 BCS
                    rom[Snes2pc((int) a++)] = 0xB0;
                    rom[Snes2pc((int) a++)] = 0x20; // FIXME
                                                    // 4 CMP BANK:8006,X
                    rom[Snes2pc((int) a++)] = 0xDF;
                    rom[Snes2pc((int) a++)] = 0x06;
                    rom[Snes2pc((int) a++)] = 0x80;
                    rom[Snes2pc((int) a++)] = (byte) lockBank;
                    // 2 BCC
                    rom[Snes2pc((int) a++)] = 0x90;
                    rom[Snes2pc((int) a++)] = 0x1A; // FIXME
                                                    // 3 LDA #2
                    rom[Snes2pc((int) a++)] = 0xA9;
                    rom[Snes2pc((int) a++)] = 0x02;
                    rom[Snes2pc((int) a++)] = 0x00;
                    // 3 STA $1E52
                    rom[Snes2pc((int) a++)] = 0x8D;
                    WriteWord(Snes2pc((int) a++), (ushort) (type == 0 ? 0x1E52 : type == 1 ? 0x1E62 : 0x1E62)); // TYPE
                    a++;

                    //// 3 JMP FFD0
                    //*LPBYTE(rom + snes2pc(a++)) = 0x4C;
                    //*LPBYTE(rom + snes2pc(a++)) = 0xD0;
                    //*LPBYTE(rom + snes2pc(a++)) = 0xFF;
                    //// 2 SEP #30,$20
                    //*LPBYTE(rom + snes2pc(a++)) = 0xE2;
                    //*LPBYTE(rom + snes2pc(a++)) = 0x20 | 0x10;
                    //// 1 RTS
                    //*LPBYTE(rom + snes2pc(a++)) = 0x60;
                    //// @ FFD0
                    //a = 0x81FFD0;

                    // 4 LDA BANK:8008,X
                    rom[Snes2pc((int) a++)] = 0xBF;
                    rom[Snes2pc((int) a++)] = 0x08;
                    rom[Snes2pc((int) a++)] = 0x80;
                    rom[Snes2pc((int) a++)] = (byte) lockBank;
                    // 2 BEQ
                    rom[Snes2pc((int) a++)] = 0xF0;
                    rom[Snes2pc((int) a++)] = 0x0E; // FIXME
                                                    // 1 TAY
                    rom[Snes2pc((int) a++)] = 0xA8;
                    // 1 INX
                    rom[Snes2pc((int) a++)] = 0xE8;
                    // 1 INX
                    rom[Snes2pc((int) a++)] = 0xE8;
                    // 4 LDA BANK:8008,X
                    rom[Snes2pc((int) a++)] = 0xBF;
                    rom[Snes2pc((int) a++)] = 0x08;
                    rom[Snes2pc((int) a++)] = 0x80;
                    rom[Snes2pc((int) a++)] = (byte) lockBank;
                    // 1 INX
                    rom[Snes2pc((int) a++)] = 0xE8;
                    // 1 INX
                    rom[Snes2pc((int) a++)] = 0xE8;
                    // 3 STA addr,Y
                    rom[Snes2pc((int) a++)] = 0x99;
                    rom[Snes2pc((int) a++)] = 0x00;
                    rom[Snes2pc((int) a++)] = 0x00;
                    // 2 BRA
                    rom[Snes2pc((int) a++)] = 0x80;
                    rom[Snes2pc((int) a++)] = 0xEC; // FIXME

                    // 2 SEP #20,#10
                    rom[Snes2pc((int) a++)] = 0xE2;
                    rom[Snes2pc((int) a++)] = 0x20 | 0x10;
                    // 1 RTS
                    rom[Snes2pc((int) a++)] = 0x60;

                    // fix the event ending functions
                    WriteWord(Snes2pc(type == 0 ? 0x81F6B1 : type == 1 ? 0x82EB27 : 0x83DD81), (ushort) (ReadWord(Snes2pc(type == 0 ? 0x81F6B1 : type == 1 ? 0x82EB27 : 0x83DD81)) - 0x4)); // TYPE
                    a = (uint) (type == 0 ? 0x81F722 : type == 1 ? 0x82EB99 : 0x83DDF3); // TYPE
                    Copy(Snes2pc((int) (a - 0)), Snes2pc((int) (a - 1)), 0x41);
                    Copy(Snes2pc((int) (a - 1)), Snes2pc((int) (a - 2)), 0x36);
                    Copy(Snes2pc((int) (a - 2)), Snes2pc((int) (a - 3)), 0x20);
                    Copy(Snes2pc((int) (a - 3)), Snes2pc((int) (a - 4)), 0x15);
                    a += 0x0F; // 0x81F731;
                    rom[Snes2pc((int) a++)] = 0xBF;
                    rom[Snes2pc((int) a++)] = 0x00;
                    rom[Snes2pc((int) a++)] = 0x80;
                    rom[Snes2pc((int) a++)] = (byte) lockBank;
                    a += 0x08; // 0x81F73D;
                    rom[Snes2pc((int) a++)] = 0xBF;
                    rom[Snes2pc((int) a++)] = 0x02;
                    rom[Snes2pc((int) a++)] = 0x80;
                    rom[Snes2pc((int) a++)] = (byte) lockBank;
                    a += 0x13; //  0x81F754;
                    rom[Snes2pc((int) a++)] = 0xBF;
                    rom[Snes2pc((int) a++)] = 0x04;
                    rom[Snes2pc((int) a++)] = 0x80;
                    rom[Snes2pc((int) a++)] = (byte) lockBank;
                    a += 0x08; // 0x81F760;
                    rom[Snes2pc((int) a++)] = 0xBF;
                    rom[Snes2pc((int) a++)] = 0x06;
                    rom[Snes2pc((int) a++)] = 0x80;
                    rom[Snes2pc((int) a++)] = (byte) lockBank;
                    // fix branches 0x64
                    rom[Snes2pc((int) (a - 0x2C))] += 0x3; // 0x81F738
                    rom[Snes2pc((int) (a - 0x28))] += 0x1; // 0x81F73C
                    rom[Snes2pc((int) (a - 0x20))] += 0x2; // 0x81F744
                    rom[Snes2pc((int) (a - 0x09))] += 0x1; // 0x81F75B
                    rom[Snes2pc((int) (a - 0x05))] += 0x1; // 0x81F75F

                    header.romSize++;
                    romSize += 0x100000;
                    expandedLayoutSize = 0x800;
                    expandedEventSize = 0x800;
                    expandedCheckpointSize = 0x10;
                    expandedLayoutScenes = 0x40;
                    expandedVersion = expandedROMVersion;
                }
            }
            else
            {
                if (ok)
                {
                    // make the ROM bigger
                    // Add 1MB
                    Fill(romSize, 0xFF, 0x200000);
                    WriteASCIIString((uint) (romSize + 0x8000 - expandedROMHeaderSize), expandedROMString);
                    WriteWord((uint) (romSize + 0x8000 - expandedROMHeaderSize + 0xE), expandedROMVersion);
                    ushort v = 0x800;
                    WriteWord((uint) (romSize + 0x8000 - expandedROMHeaderSize + 0x10), v);
                    v = 0x800;
                    WriteWord((uint) (romSize + 0x8000 - expandedROMHeaderSize + 0x12), v);
                    v = 0x10;
                    WriteWord((uint) (romSize + 0x8000 - expandedROMHeaderSize + 0x14), v);
                    // sceneUsed already stored in the ROM

                    // copy startup code
                    Copy(0x8000, romSize + 0x8000, 0x8000);
                }

                uint currentOffset = (uint) (romSize + 0x10000);

                if (ok)
                {
                    expandedROM = true;

                    header.romSize++;
                    header.mapMode |= 0x4;
                    romSize += 0x200000;
                    expandedLayoutSize = 0x800;
                    expandedEventSize = 0x800;
                    expandedCheckpointSize = 0x0;
                    expandedLayoutScenes = 0x40;
                    expandedVersion = expandedROMVersion;
                }
            }

            return ok;
        }

        internal void LoadGFXs()
        {
            pGfx = Snes2pc(p_gfxpos[type]);
            pGfxObj = p_gfxobj[type] != 0 ? Snes2pc(p_gfxobj[type]) : 0x0;
            pGfxPal = p_gfxobj[type] != 0 ? Snes2pc(p_gfxpal[type]) : 0x0;
            pSpriteAssembly = p_spriteAssembly[type] != 0 ? Snes2pc(p_spriteAssembly[type]) : 0x0;
            pSpriteOffset[0] = p_objOffset[type] != 0 ? Snes2pc(p_objOffset[type]) : 0x0; //type not event type
            pSpriteOffset[1] = p_objOffset[type] != 0 ? Snes2pc(p_objOffset[type]) : 0x0;
            pSpriteOffset[3] = p_spriteOffset[type] != 0 ? Snes2pc(p_spriteOffset[type]) : 0x0;

            if (type < 3)
            {
                uint pConfigGfx = Snes2pc(SReadWord((uint) (p_gfxcfg[type] + Level * 2 + 4)) | 0x86 << 16);
                byte gfxID = rom[pConfigGfx];
                tileCmpSize = ReadWord(pConfigGfx + 1);
                tileCmpDest = (ushort) ((ReadWord(pConfigGfx + 3) << 1) - 0x2000);
                tileCmpPos = Snes2pc(SReadDWord((uint) (p_gfxpos[type] + gfxID * 5 + 2)));
                tileCmpRealSize = (ushort) CompressionCore.GFXRLE(rom, 0, vram, tileCmpDest, (int) tileCmpPos, tileCmpSize, type);
            }
            else
            {
                // FIXME: 0x14 offset needs to be per level.  destination needs to be source based
                uint pConfigGfx = Snes2pc(SReadWord(p_gfxcfg[type] + SReadByte(0x80824A + Level) | 0x80 << 16));
                byte gfxID = rom[pConfigGfx];
                tileCmpSize = ReadWord(pConfigGfx + 1); //SReadWord(p_gfxpos[type] + gfxID * 5 + 3);
                tileCmpDest = (ushort) ((ReadWord(pConfigGfx + 3) << 1) - 0x2000);
                tileCmpPos = Snes2pc(SReadDWord((uint) (p_gfxpos[type] + gfxID * 5 + 0)));
                tileCmpRealSize = (ushort) CompressionCore.GFXRLE(rom, 0, vram, tileCmpDest, (int) tileCmpPos, tileCmpSize, type);
            }
        }

        private class TileSortComparator : IComparer<STileInfo>
        {
            public int Compare(STileInfo a, STileInfo b) => a.value - b.value;
        }

        private static readonly IComparer<STileInfo> TileSortComparer = new TileSortComparator();

        internal void LoadTiles()
        {
            byte tileSelect = (byte) TileLoad;

            uint tileOffset = (uint) ((type == 0) ? 0x321D5
                : (type == 1) ? 0x31D6A
                : 0x32085); /*0x1532D4;*/

            // find bounds of dynamic tiles
            tileDecStart = 0x400;
            tileDecEnd = 0;
            numDecs = 0;
            for (int i = 0; i < 0x40; ++i)
            {
                int tbaseIndex = ReadWord((uint) (tileOffset + Level * 2)) + i * 2;
                int tmainIndex = ReadWord((uint) (tileOffset + tbaseIndex));

                numDecs++;

                ushort size = ReadWord((uint) (tileOffset + tmainIndex));
                if (size == 0)
                {
                    if (i == 0)
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }

                uint pos = (uint) ((ReadWord((uint) (tileOffset + tmainIndex + 2)) << 1) - 0x2000);
                //should pos be an unsigned integer?

                uint start = pos / 0x20;
                uint end = (size + pos) / 0x20;
                if (start < tileDecStart)
                {
                    tileDecStart = start;
                }

                if (end > tileDecEnd)
                {
                    tileDecEnd = end;
                }
            }

            // Is it right to start from 1 all the time?  Or do we need to check 0, too?
            for (int i = 0; i <= tileSelect; ++i)
            {
                int baseIndex = ReadWord((uint) (tileOffset + Level * 2)) + i * 2;
                int mainIndex = ReadWord((uint) (tileOffset + baseIndex));

                tileDecSize = ReadWord((uint) (tileOffset + mainIndex));
                if (tileDecSize == 0)
                    continue;
                tileDecDest = (ushort) ((ReadWord((uint) (tileOffset + mainIndex + 2)) << 1) - 0x2000);
                uint addr = ReadDWord((uint) (tileOffset + mainIndex + 4)) & 0xFFFFFF;
                tileDecPos = Snes2pc(addr);

                if (tileDecDest + tileDecSize > (uint) 0x10000)
                {
                    //MessageBox(NULL, "VRAM overflow.", "Error", MB_ICONERROR);
                }
                // skip the load if it's to the RAM address
                // This happens in X3 when zero first appears.  It's not obvious how it handles these tiles
                // Pointer = 0x86A134/86A136
                if (addr != 0x7F0000)
                {
                    Array.Copy(rom, tileDecPos, vram, tileDecDest, tileDecSize);
                }
            }
        }

        internal enum ESort
        {
            SORT_NONE,
            SORT_MIN,
            SORT_MAX,
            SORT_MEDIAN,
            SORT_MEAN,
            SORT_MODE,
            SORT_TOTAL
        };

        internal void SortTiles()
        {
            byte[] tvram = new byte[0x8000];
            var sortTypes = new List<ESort>();

            if (!sortOk)
            {
                sortTypes.Add(ESort.SORT_NONE);
            }
            else
            {
                sortTypes.Add(ESort.SORT_NONE);
                //sortTypes.push_back(SORT_MIN);
                sortTypes.Add(ESort.SORT_MAX);
                //sortTypes.push_back(SORT_MEDIAN);
                //sortTypes.push_back(SORT_MEAN);
                sortTypes.Add(ESort.SORT_MODE);
            }

            ushort newSize = 0;  //GFXRLECmp(nmmx.vram + 0x200, tvram, nmmx.tileCmpSize, nmmx.type);
            ushort[] tileRemap = new ushort[0x400];

            foreach (ESort sortType in sortTypes)
            {
                //ZeroMemory(srcram, 0x8000);
                //memcpy(srcram, nmmx.vram + 0x200, nmmx.tileCmpSize);

                var tileInfo = new STileInfo[0x400];

                for (uint i = 0; i < 0x400; ++i)
                {
                    tileInfo[i].num = i;
                    tileInfo[i].value = (byte) (sortType == ESort.SORT_MIN ? 0xFF : 0x0);
                    tileInfo[i].count = new Dictionary<byte, uint>();

                    for (int p = 0; p < 32; ++p)
                    {
                        byte value = vram[32 * i + p];
                        switch (sortType)
                        {
                            case ESort.SORT_NONE:
                                // do nothing
                                break;
                            case ESort.SORT_MIN:
                                tileInfo[i].value = System.Math.Min(tileInfo[i].value, value);
                                break;
                            case ESort.SORT_MAX:
                                tileInfo[i].value = System.Math.Max(tileInfo[i].value, value);
                                break;
                            case ESort.SORT_MEDIAN:
                                tileInfo[i].count[value]++;
                                if (p == 31)
                                {
                                    uint num = 0;
                                    foreach (var count in tileInfo[i].count)
                                    {
                                        num += count.Value;
                                        if (num >= 16)
                                        {
                                            tileInfo[i].value = count.Key;
                                            break;
                                        }
                                    }
                                }

                                break;
                            case ESort.SORT_MEAN:
                                tileInfo[i].value += value;
                                if (p == 31)
                                {
                                    tileInfo[i].value /= 32;
                                }

                                break;
                            case ESort.SORT_MODE:
                                tileInfo[i].count[value]++;
                                if (p == 31)
                                {
                                    uint num = 0;
                                    foreach (var count in tileInfo[i].count)
                                    {
                                        if (count.Value > num)
                                        {
                                            tileInfo[i].value = count.Key;
                                            num = count.Value;
                                        }
                                    }
                                }

                                break;
                            default:
                                break;
                        }
                    }
                }

                // sort based on type.  skip the first 16 tiles since they go uncompressed.  Same goes for the last set of tiles.
                uint start = 0x200 / 0x20;
                uint end = (uint) (0x400 - (0x8000 - 0x200 - tileCmpSize) / 0x20);

                if (tileDecStart >= end || tileDecEnd <= start)
                {
                    // sort full thing
                    Array.Sort(tileInfo, (int) start, (int) (end - start), TileSortComparer);
                }
                else if (tileDecStart <= start && tileDecEnd >= end)
                {
                    // can't sort anything
                }
                else
                {
                    if (start < tileDecStart)
                    {
                        Array.Sort(tileInfo, (int) start, (int) (tileDecStart - start), TileSortComparer);
                    }

                    if (tileDecEnd < end)
                    {
                        Array.Sort(tileInfo, (int) tileDecEnd, (int) (end - tileDecEnd), TileSortComparer);
                    }
                }

                // move the tiles in memory 
                byte[] sortram = new byte[0x8000];
                for (uint i = 0; i < 0x400; ++i)
                {
                    Array.Copy(vram, 32 * tileInfo[i].num, sortram, i * 32, 32);
                }

                // try compressing
                byte[] cmpram = new byte[0x8000];
                ushort tempSize = CompressionCore.GFXRLECmp(sortram, 0x200, cmpram, 0, tileCmpSize, type);

                if (tempSize < newSize || sortType == ESort.SORT_NONE)
                {
                    newSize = tempSize;
                    Array.Copy(sortram, 0, tvram, 0, tvram.Length);

                    for (int i = 0; i < 0x400; ++i)
                    {
                        tileRemap[tileInfo[i].num] = (ushort) i;
                    }
                }
            }

            // copy the best sorted data
            Array.Copy(tvram, 0, vram, 0, tvram.Length);

            if (sortOk)
            {
                // fix blocks
                for (int i = 0; i < numMaps; ++i)
                {
                    for (int j = 0; j < 4; ++j)
                    {
                        uint tileOffset = (uint) (pMaps + (i << 3) + j * 2);
                        uint tile = (uint) (rom[tileOffset] & 0x3FF);
                        rom[tileOffset] &= ~0x3FF & 0xff;
                        rom[tileOffset] |= (byte) (tileRemap[tile] & 0x3FF);
                    }
                }
            }
        }

        internal void SetLevel(ushort iLevel, ushort iPoint, int objLoad = -1, int tileLoad = -1, int palLoad = -1)
        {
            Level = iLevel;
            Point = iPoint;

            tileCmpSize = 0;
            tileDecSize = 0;

            ObjLoad = objLoad;
            TileLoad = tileLoad;
            PalLoad = palLoad;
        }

        internal void LoadLevel(bool skipEvent = false, bool skipLayout = false)
        {
            //if (nmmx.type == 3) {
            //	LoadGFXs();

            //	// load sprite palettes
            //	for (int i = 0; i<NUM_SPRITE_PALETTES * 16; i++)
            //		palSpriteCache[i] = Get16Color(/*0x2b900*/ 0x2a000 + i * 2);

            //	for (int i = 0; i<0x400; i++)
            //		tile4bpp2raw(vram + (i << 5), vramCache + (i << 6));

            //	return;
            //}
            if (!skipEvent)
            { 
                // && nmmx.type != 1 && level != 7
                //MessageBox(hWID[0], "LoadEvents()", "Test", MB_ICONERROR);
                LoadEvents();

                //MessageBox(hWID[0], "LoadCheckpoints()", "Test", MB_ICONERROR);
                LoadCheckpoints();
            }

            //MessageBox(hWID[0], "LoadTilesAndPalettes()", "Test", MB_ICONERROR);
            LoadTilesAndPalettes();
            //MessageBox(hWID[0], "LoadGraphicsChange()", "Test", MB_ICONERROR);
            LoadGraphicsChange();

            //MessageBox(hWID[0], "Init Pointers", "Test", MB_ICONERROR);
            ushort pLevel = (ushort) (Level * 3);
            if (type < 3)
            {
                pLayout = Snes2pc(SReadDWord(p_layout[type] + pLevel));
                pScenes = Snes2pc(SReadDWord(p_scenes[type] + pLevel));
            }
            else
            {
                pLayout = Snes2pc(0xC50000 | SReadWord((uint) (p_layout[type] + Level * 2)));
                pScenes = Snes2pc(SReadDWord(p_scenes[type] + Level));
            }

            pBlocks = Snes2pc(SReadDWord(p_blocks[type] + pLevel));
            pMaps = Snes2pc(SReadDWord(p_maps[type] + pLevel));
            pCollisions = Snes2pc(SReadDWord(p_collis[type] + pLevel));

            sortOk = true;

            if (type == 1 && (Level == 10 || Level == 11))
            {
                // x-hunters level 10 and 11 share a compressed tile set so the map will get screwed up
                // if we sort one and didn't rewrite the map for both levels
                sortOk = false;
            }

            //MessageBox(hWID[0], "SetNumThings", "Test", MB_ICONERROR);
            if (Level < numLevels - 1)
            {
                // This is a hack to figure out the approximate number of tiles, maps, and blocks.
                // it assumes the level data is stored consecutively in memory.  If it isn't we may
                // think there are more than are available.  This should be ok as long as the user
                // doesn't corrupt this memory.  Ideally, we would read a count.  Another hack
                // would read all the scenes to find the highest numbered block used, etc, to get
                // an approximate number.
                ushort pNextLevel = (ushort) ((Level + 1) * 3);
                numTiles = (uint) ((0x200 + tileCmpSize) / 0x20);
                numBlocks = (Snes2pc(SReadDWord(p_blocks[type] + pNextLevel)) - pBlocks) / 0x8;
                numMaps = (Snes2pc(SReadDWord(p_maps[type] + pNextLevel)) - pMaps) / 0x8;

                if (numTiles > 0x400 || numBlocks > 0x400 || numMaps > 0x400)
                {
                    numTiles = 0x400;
                    numMaps = 0x400;
                    numBlocks = 0x400;

                    sortOk = false;
                }
            }
            else
            {
                numTiles = 0x400;
                numBlocks = 0x40;
                numMaps = 0x400;

                sortOk = false;
            }

            //ppppp = snes2pc(SReadDWord(p_unknow[type] + pLevel));
            //ppppp = ppppp;

            //MessageBox(hWID[0], "LoadLayout()", "Test", MB_ICONERROR);
            if (!skipLayout)
            {
                LoadLayout();
            }

            //MessageBox(hWID[0], "LoadLevelLayout()", "Test", MB_ICONERROR);
            LoadLevelLayout();

            /*if (nmmx.type == 1 && level == 7) {
                //custom order
                LoadEvents();		
                //MessageBox(hWID[0], "LoadTilesAndPalettes()", "Test", MB_ICONERROR);
                LoadTilesAndPalettes();
                //MessageBox(hWID[0], "LoadGraphicsChange()", "Test", MB_ICONERROR);
                LoadGraphicsChange();
                LoadCheckpoints();
            }*/
            expandedLayoutScenes = 0x40;

#pragma warning disable CS0162 // Código inacessível detectado
            if (DEBUG_DUMP_ROM_MEMORY)
            {
                string sFileName = "C:\\Users\\miste\\Documents\\Projects\\c#\\vc#\\XSharp\\XSharp\\resources\\roms\\" + ROM_NAME + ".smc";
                int pos = sFileName.LastIndexOf('.');
                if (pos != -1)
                    sFileName = sFileName.Substring(0, pos);

                sFileName += ".rom.txt";

                using (StreamWriter writer = File.CreateText(sFileName))
                {
                    WriteDump("rom", writer, rom, 0, romSize);
                    writer.Flush();
                }

                sFileName = "C:\\Users\\miste\\Documents\\Projects\\c#\\vc#\\XSharp\\XSharp\\resources\\roms\\" + ROM_NAME + ".smc";
                pos = sFileName.LastIndexOf('.');
                if (pos == -1)
                    sFileName += "[";
                else
                    sFileName = sFileName.Substring(0, pos) + "[";

                sFileName += Level;
                sFileName += "].txt";

                using (StreamWriter writer = File.CreateText(sFileName))
                {
                    WriteDump("palCache", writer, palCache, 0, palCache.Length);
                    WriteDump("palSpriteCache", writer, palSpriteCache, 0, palSpriteCache.Length);
                    WriteDump("vramCache", writer, vramCache, 0, vramCache.Length);
                    WriteDump("spriteCache", writer, spriteCache, 0, spriteCache.Length);
                    WriteDump("vram", writer, vram, 0, vram.Length);
                    WriteDump("mapping", writer, mapping, 0, mapping.Length);
                    WriteDump("mappingBG", writer, mappingBG, 0, mappingBG.Length);
                    WriteDump("sceneLayout", writer, sceneLayout, 0, sceneLayout.Length);
                    WriteDump("palettesOffset", writer, palettesOffset, 0, palettesOffset.Length);
                    WriteDump("fontPalCache", writer, fontPalCache, 0, fontPalCache.Length);
                    WriteDump("fontCache", writer, fontCache, 0, fontCache.Length);
                    writer.Flush();
                }
            }
#pragma warning restore CS0162 // Código inacessível detectado
        }

        internal void LoadBackground(bool skipLayout = false)
        {
            ushort pLevel = (ushort) (Level * 3);
            if (type < 3)
            {
                pLayout = Snes2pc(SReadDWord(p_blayout[type] + pLevel));
                pScenes = Snes2pc(SReadDWord(p_bscenes[type] + pLevel));
            }
            else
            {
            }

            pBlocks = Snes2pc(SReadDWord(p_bblocks[type] + pLevel));
            LoadTilesAndPalettes();
            if (!skipLayout)
            {
                LoadLayout();
            }

            LoadLevelLayout();

            expandedLayoutScenes = 0x20;
        }

        internal void LoadTilesAndPalettes()
        {
            // Load Palettes
            pPalBase = Snes2pc(p_palett[type]);
            if (type < 3)
            {
                uint configPointer = Snes2pc(SReadWord((uint) (p_palett[type] + Level * 2 + 0x60)) | 0x860000);
                byte colorsToLoad = rom[configPointer++];
                pPalette = type == 2 ? Snes2pc(ReadWord(configPointer++) | 0x8C0000) : Snes2pc(ReadWord(configPointer++) | 0x850000);

                for (int i = 0; i < colorsToLoad; i++)
                    palCache[i] = Get16Color((uint) (pPalette + i * 2));

                for (int i = 0; i < colorsToLoad >> 4; i++)
                    palettesOffset[i] = (uint) (pPalette + i * 0x20);
            }
            else
            {
                var indices = new List<uint>(new uint[] { 0x124, 0x0, 0x1E, 0x1B2, 0xA, 0x104 });
                if (Level >= 0)
                {
                    //indices.clear();
                    //indices.push_back(0x17C);
                    //indices.push_back(0x11E);
                    //indices.push_back(2 * WORD(SReadWord(0x808AA6 + 2 * (level - 1))));
                    //indices.push_back(2 * WORD(SReadByte(0x808A8E + 1 * (level - 1))));
                    //indices.push_back(0x0);
                    indices.Add((uint) (2 * SReadByte(0x80823D + Level)));
                    //indices.push_back(2 * WORD(SReadByte(0x808A8E + 1 * (level - 1))));
                }

                foreach (var index in indices)
                {
                    ushort offset = ReadWord(Snes2pc(0x81928A + index));

                    byte colorsToLoad = rom[Snes2pc(0x810000 + offset)];
                    ushort palOffset = ReadWord(Snes2pc(0x810000 + offset + 1));
                    ushort dst = rom[Snes2pc(0x810000 + offset + 3)];

                    if (dst + colorsToLoad > 0x100)
                        continue;

                    uint pPalette = Snes2pc(0xC50000 | palOffset);
                    for (int i = 0; i < colorsToLoad; i++)
                        palCache[dst + i] = Get16Color((uint) (pPalette + i * 2));

                    for (int i = 0; i < colorsToLoad >> 4; i++)
                        palettesOffset[(dst >> 4) + i] = (uint) (pPalette + i * 0x20);
                }
            }

            // load sprite palettes
            for (int i = 0; i < NUM_SPRITE_PALETTES * 16; i++)
                palSpriteCache[i] = Get16Color((uint) (/*0x2b900*/ 0x2a000 + i * 2));

            Array.Copy(vrambase, 0, vram, 0, 0x200);

            if (type < 3)
                LoadPaletteDynamic();

            LoadGFXs();
            if (type < 3)
                LoadTiles();

            for (int i = 0; i < 0x400; i++)
                Tile4bpp2raw(vram, i << 5, vramCache, i << 6);
        }

        internal void LoadEvents()
        {
            pBorders = 0;
            pLocks = 0;

            if (p_events[type] == 0)
                return;

            foreach (var eventList in eventTable)
                eventList.Clear();

            if (type < 3)
            {
                if (p_borders[type] != 0)
                    pBorders = SReadWord((uint) (p_borders[type] + Level * 2)) | ((p_borders[type] >> 16) << 16);
                if (p_locks[type] != 0)
                    pLocks = Snes2pc(p_locks[type]);
                if (p_capsulepos[type] != 0)
                    pCapsulePos = Snes2pc(p_capsulepos[type]);

                uint pEvents = Snes2pc(SReadWord((uint) ((expandedROM ? ((eventBank << 16) | 0xFFE0) : p_events[type]) + Level * 2)) | (eventBank << 16));
                uint pevent = pEvents;
                uint oldpevent = pevent;

                uint blockId = 0xFF;
                uint nextBlockId = rom[pevent++];

                // A BCCDEFF
                // A = Header byte with position (edge of screen)
                // B = 6b level event?, 2b = event type (3=enemy)
                // C = YPOS word
                // D = Event ID
                // E = SubEvent ID
                // F = 3b info (top bit end of event for block), 13b XPOS
                while (blockId != nextBlockId && blockId < 0x100)
                {
                    bool eventDone = true;

                    blockId = nextBlockId;
                    do
                    {
                        var e = new EventInfo
                        {
                            type = rom[pevent++]
                        };

                        e.match = (byte) (e.type >> 2);

                        e.type &= 0x3;

                        e.ypos = ReadWord(pevent);
                        pevent += 2;
                        e.eventId = rom[pevent++];

                        e.eventSubId = rom[pevent++];
                        //temp fix for mmx1/mmx2 to show heart tank graphics
                        if (e.type == 0x0 && e.eventId == 0xB)
                            e.eventSubId = 0x4;

                        e.xpos = ReadWord(pevent);
                        pevent += 2;
                        e.eventFlag = (byte) (e.xpos >> 13);

                        e.xpos &= 0x1fff;

                        eventTable[blockId].Add(e);

                        eventDone = (e.eventFlag & 0x4) != 0;
                    }
                    while (!eventDone);

                    // get the next id
                    nextBlockId = rom[pevent++];
                }
            }
            else
            {
                uint levelAddr = Snes2pc(SReadWord((uint) (p_events[type] + Level * 2)) | (eventBank << 16));
                uint blockId = 0;

                var workQueue = new Deque<uint>(new uint[] { 0x0 });
                var indexSeen = new HashSet<uint>(new uint[] { 0x0 });

                while (!workQueue.IsEmpty)
                {
                    uint index = workQueue.First();
                    workQueue.PopFirst();

                    uint pEvents = Snes2pc(SReadWord(levelAddr + index * 2) | (eventBank << 16));
                    uint pevent = pEvents;

                    for (uint i = 0, count = rom[pevent++]; i < count; ++i)
                    {
                        // 7 bytes per event
                        // ABCDDEE
                        // A = type (6 = enemy, 4 = other?)
                        // B = id
                        // C = subId
                        // D = xpos
                        // E = ypos
                        var e = new EventInfo
                        {
                            type = rom[pevent++],

                            eventId = rom[pevent++],

                            eventSubId = rom[pevent++],

                            xpos = ReadWord(pevent)
                        };
                        pevent += 2;
                        e.ypos = ReadWord(pevent);
                        pevent += 2;

                        // TODO: alternatively we can reverse engineer the block number to re-use the existing selection code

                        blockId = (uint) (e.xpos >> 5);

                        if (blockId < 0x100)
                        {
                            eventTable[blockId].Add(e);
                        }

                        // check for segment change
                        if (e.type == 0x4 && (e.eventId == 0x0 || e.eventId == 0x1 || e.eventId == 0x6 || e.eventId == 0xE))
                        {
                            index = (uint) (e.eventSubId & 0x7F);

                            if (e.eventId is 0x6 or 0xE)
                            {
                                ushort offset = SReadWord(0xC14A3E + 2 * Level);
                                index = SReadByte((uint) (0xC14A3E + offset + 2 * index));
                            }

                            if (!indexSeen.Contains(index))
                            {
                                workQueue.AddLast(index);
                                indexSeen.Add(index);
                            }
                        }
                    }
                }
            }
        }

        internal void LoadProperties()
        {
            if (p_properties[type] == 0)
                return;

            if (type != 2)
            {
                // X1 and X2 have inline LDAs with constants
                for (uint i = 0; i < 107; ++i)
                {
                    propertyTable[i].Reset();

                    uint jsl = SReadDWord(SReadWord(p_properties[type] + i * 2) | ((p_properties[type] >> 16) << 16));
                    uint jslAddr = jsl >> 8;

                    if ((jsl & 0xFF) == 0x60)
                    {
                        // jump table has immediate return
                        continue;
                    }

                    // take the JSL
                    uint func = Snes2pc(jslAddr);

                    if (func > romSize - 0x2000)
                    {
                        continue;
                    }

                    // find the JSR
                    for (uint j = 0; j < 10; ++j)
                    {
                        if (rom[func] is 0xFC or 0x7C)
                        {
                            break;
                        }

                        ++func;
                    }

                    if (rom[func] is not 0xFC and not 0x7C)
                    {
                        continue;
                    }

                    uint jsr = ReadDWord(func);

                    func = Snes2pc(SReadWord(((jslAddr >> 16) << 16) | ((jsr >> 8) & 0xFFFF)) | ((jslAddr >> 16) << 16));

                    var workQueue = new Deque<Tuple<uint, uint, uint>>();
                    workQueue.AddLast(new Tuple<uint, uint, uint>(func, 0, 0));

                    const uint ITER_COUNT = 100;
                    const uint DEPTH_COUNT = 1;

                    while (!workQueue.IsEmpty)
                    {
                        var entry = workQueue.First();

                        bool poppedFunc = false;
                        while (entry.Item2 < ITER_COUNT)
                        {
                            var currentFunc = entry.Item1;
                            entry = new Tuple<uint, uint, uint>(entry.Item1 + 1, entry.Item2 + 1, entry.Item3);
                            workQueue.PopFirst();
                            workQueue.AddFirst(entry);

                            if (rom[currentFunc] == 0xA9 && rom[currentFunc + 2] == 0x85)
                            {
                                var staFunc = currentFunc + 2;
                                // LDA followed by STA

                                for (uint k = 0; k < 3; k++, staFunc += 2)
                                {
                                    // support LDA followed by several STA up to the right one
                                    var addr = staFunc + 1;
                                    if (rom[staFunc] != 0x85)
                                    {
                                        break;
                                    }
                                    else if (rom[addr] == 0x27)
                                    {
                                        propertyTable[i].hp = currentFunc + 1;
                                    }
                                    else if (rom[addr] == 0x28)
                                    {
                                        propertyTable[i].damageMod = currentFunc + 1;
                                    }
                                }
                                // could skip over the LDA and STA here
                            }
                            else if (rom[currentFunc] == 0x22 && entry.Item3 < DEPTH_COUNT)
                            {
                                uint tempJsl = ReadDWord(currentFunc);
                                uint tempJslAddr = tempJsl >> 8;
                                var newFunc = Snes2pc(tempJslAddr);

                                // step into the function
                                if (newFunc < romSize - ITER_COUNT - 1)
                                {
                                    // if we decode what looks like a valid address add that as a new function
                                    workQueue.AddFirst(new Tuple<uint, uint, uint>(newFunc, entry.Item3 + 1, 0));
                                }
                                //workQueue.push_back(std::make_tuple(currentFunc + 1, depth, j + 1));
                                break;
                            }
                            else if (rom[currentFunc] is 0x60 or 0x6B)
                            {
                                // RTS or RTL
                                workQueue.PopFirst();
                                poppedFunc = true;
                                break;
                            }
                        }

                        if (propertyTable[i].hp != 0 && propertyTable[i].damageMod != 0)
                        {
                            break;
                        }

                        if (entry.Item2 >= ITER_COUNT && !poppedFunc)
                        {
                            workQueue.PopFirst();
                        }
                    }
                }
            }
            else
            {
                // X3 has dedicated tables
                for (uint i = 1; i < 107; ++i)
                {
                    propertyTable[i].Reset();

                    uint offset = (i - 1) * 5;
                    propertyTable[i].hp = Snes2pc(p_properties[type]) + offset + 0x3;
                    propertyTable[i].damageMod = Snes2pc(p_properties[type]) + offset + 0x4;
                }
            }
        }

        internal void LoadCheckpoints()
        {
            // count checkpoint trigger events
            numCheckpoints = (uint) ((type < 3) ? 1 : 0);

            var subIds = new HashSet<uint>();
            foreach (var eventList in eventTable)
            {
                foreach (var e in eventList)
                {
                    if (e.type == 0x2 && (e.eventId == 0xB || e.eventId == 0x2))
                    {
                        uint checkpoint1 = (uint) ((e.eventSubId >> 0) & 0xF);

                        // need to figure if the SubId encodes the checkpoint number
                        subIds.Add(checkpoint1);

                        if (numCheckpoints < checkpoint1 + 1)
                            numCheckpoints = checkpoint1 + 1;
                    }
                }
            }

            //numCheckpoints = subIds.size() + 1;

            if (expandedROM && expandedVersion >= 3)
                numCheckpoints = expandedCheckpointSize;

            checkpointInfoTable.Clear();

            for (int i = 0; i < numCheckpoints; ++i)
            {
                uint ptr = GetCheckPointPointer((uint) i);
                var ci = new CheckPointInfo();

                ci.Reset();

                if (expandedCheckpointSize > 0)
                {
                    ci.offset = Snes2pc((uint) (p_checkp[type] + SReadWord((uint) (p_checkp[type] + Level * 2)) + i * 2));
                }

                ci.objLoad = ptr++; // LPBYTE(ptr++);
                ci.tileLoad = ptr++;
                ci.palLoad = ptr++;

                if (type > 0)
                    ci.byte0 = ptr++;

                ci.chX = ptr++;
                ptr++;
                ci.chY = ptr++;
                ptr++;
                ci.camX = ptr++;
                ptr++;
                ci.camY = ptr++;
                ptr++;
                ci.bkgX = ptr++;
                ptr++;
                ci.bkgY = ptr++;
                ptr++;
                ci.minX = ptr++;
                ptr++;
                ci.maxX = ptr++;
                ptr++;
                ci.minY = ptr++;
                ptr++;
                ci.maxY = ptr++;
                ptr++;
                ci.forceX = ptr++;
                ptr++;
                ci.forceY = ptr++;
                ptr++;
                ci.scroll = ptr++;
                ci.telDwn = ptr++;

                if (type > 0)
                    ci.byte1 = ptr++;

                if (type > 1)
                    ci.byte2 = ptr++;

                checkpointInfoTable.Add(ci);
            }
        }

        internal void LoadGraphicsChange()
        {

        }

        internal void SaveLevel()
        {

        }

        internal void SaveTiles()
        {

        }

        internal uint SaveEvents(bool sizeOnly = false)
        {
            if (p_events[type] == 0)
                return 0;

            uint size = 0;

            if (type < 3)
            {
                uint pEvents = Snes2pc(SReadWord((uint) ((expandedROM ? ((eventBank << 16) | 0xFFE0) : p_events[type]) + Level * 2)) | (eventBank << 16));
                uint pevent = pEvents;

                uint blockId = 0;
                uint lastBlockId = 0;

                foreach (var eventList in eventTable)
                {
                    if (eventList.Count > 0)
                    {
                        size += (uint) (7 * eventList.Count + 1);
                    }

                    if (!sizeOnly)
                    {
                        bool firstEvent = true;

                        foreach (var e in eventList)
                        {
                            if (firstEvent)
                            {
                                rom[pevent++] = (byte) blockId;
                                lastBlockId = blockId;
                                firstEvent = false;
                            }

                            // write out all the data 
                            rom[pevent++] = (byte) ((e.match << 2) | e.type);
                            uint lpevent = pevent;
                            WriteWord(lpevent, e.ypos);
                            pevent += 2;
                            rom[pevent++] = e.eventId;
                            rom[pevent++] = e.eventSubId;
                            lpevent = pevent;
                            WriteWord(lpevent, e.eventFlag);
                            WriteWord(lpevent, (ushort) (ReadWord(lpevent) << 13));
                            WriteWord(lpevent, (ushort) (ReadWord(lpevent) | e.xpos));
                            pevent += 2;

                            // clear the end bit
                            rom[pevent - 1] &= ~0x80 & 0xff;

                            if (pCapsulePos != 0 && e.type == 0x3 && e.eventId == 0x4D)
                            {
                                // update the capsule position
                                // skip sigma boss levels for now
                                WriteDWord((uint) (pCapsulePos + Level * 4 + 0), e.xpos);
                                WriteDWord((uint) (pCapsulePos + Level * 4 + 2), e.ypos);
                            }
                        }

                        if (eventList.Count > 0)
                        {
                            // set the end bit
                            rom[pevent - 1] |= 0x80;
                        }

                        ++blockId;
                    }
                }

                if (!sizeOnly)
                {

                    rom[pevent++] = (byte) lastBlockId;
                }

                size++;
            }
            else
            {
            }

            return size;
        }

        internal void SaveSprites()
        {

        }

        internal uint SaveLayout(bool sizeOnly = false)
        {
            uint playout = pLayout; //may need to change this offset location to load after boss fight for level 7
            bool special_case = false;

            // compress layout
            uint tempSceneUsed = (expandedROM && expandedVersion >= 2) ? expandedLayoutScenes : 0x0; //else sceneUsed instead of 0x0?

            // fix layout based on new sizes
            uint oldWidth = rom[playout + 0];
            uint oldHeight = rom[playout + 1];
            byte[] tempSceneLayout = new byte[1024];

            uint i = 0;
            //tempsceneLayout matches sceneLayout after for loop
            for (int y = 0; y < levelHeight; y++)
            {
                if (oldWidth > levelWidth && y >= 1)
                    i += oldWidth - levelWidth;

                for (int x = 0; x < levelWidth; x++)
                {
                    tempSceneLayout[y * levelWidth + x] = (byte) ((x >= (int) oldWidth || y >= (int) oldHeight) ? 0 : sceneLayout[i++]);
                }
            }

            //checking layout size during save to compare with origLayoutsize
            /*int count = 0;
            for (int i = 0; i < 78; i++) {
                if (sceneLayout[i] != 0)
                    count++;
                sceneLayout[i] = *(playout + i + 3);
            }*/
            if (type == 1 && Level == 7 && !expandedROM)
                special_case = true;

            uint size = (uint) CompressionCore.LayoutRLE(levelWidth, levelHeight, rom, (int) tempSceneUsed, tempSceneLayout, 0, rom, (int) playout, sizeOnly, special_case);

            /* helps with debugging playout
            for (int i = 0; i < 78; i++) {
                sceneLayout[i] = *(playout + i + 3);
            }*/

            if (!sizeOnly)
            {
                // do we want to allow more scenes?
                Array.Copy(tempSceneLayout, 0, sceneLayout, 0, sceneLayout.Length);
            }

            return size;
        }

        internal void LoadPaletteDynamic()
        {
            uint paletteOffset = (uint) ((type == 0) ? 0x32260
                           : (type == 1) ? 0x31DD1
                           : 0x32172);

            ushort iLevel = (ushort) (Level & 0xFF);
            byte palSelect = (byte) PalLoad;
            for (uint i = 0; i <= palSelect; ++i)
            {
                int baseIndex = (int) (ReadWord((uint) (paletteOffset + iLevel * 2)) + i * 2);
                int mainIndex = ReadWord((uint) (paletteOffset + baseIndex));
                int writeTo = 0;
                int colorPointer = 0;

                while (true)
                {
                    colorPointer = ReadWord((uint) (paletteOffset + mainIndex));
                    if (colorPointer == 0xFFFF)
                        break;

                    writeTo = ReadWord((uint) (paletteOffset + 0x2 + mainIndex)) & 0xFF;
                    if (writeTo > 0x7F)
                    {
                        //MessageBox(NULL, "Palette overflow.", "Error", MB_ICONERROR);
                        return;
                    }

                    palettesOffset[writeTo >> 4] = Snes2pc(colorPointer | (type == 2 ? 0x8C0000 : 0x850000));
                    for (int j = 0; j < 0x10; j++)
                        palCache[writeTo + j] = Convert16Color(ReadWord(Snes2pc((type == 2 ? 0x8C0000 : 0x850000) | colorPointer + j * 2)));

                    mainIndex += 3;
                }
            }
        }

        internal void LoadLevelLayout()
        {
            ushort writeIndex = 0;

            // Load other things O.o
            //writeIndex = SReadWord(0x868D20 + step*2);
            if (type < 3)
            {
                for (int i = 0; i < sceneUsed; i++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 8; x++)
                        {
                            uint takeBlock = (uint) (pScenes + i * 0x80 + x * 2 + y * 0x10);
                            takeBlock = (uint) (pBlocks + ReadWord(takeBlock) * 8);

                            mapping[writeIndex + 0x00] = ReadWord(takeBlock);
                            takeBlock += 2;
                            mapping[writeIndex + 0x01] = ReadWord(takeBlock);
                            takeBlock += 2;
                            mapping[writeIndex + 0x10] = ReadWord(takeBlock);
                            takeBlock += 2;
                            mapping[writeIndex + 0x11] = ReadWord(takeBlock);
                            takeBlock += 2;
                            writeIndex += 2;
                        }

                        writeIndex += 0x10;
                    }
                }
            }
            else
            {

                // decompress the scene to map data
                byte[] mapRam = new byte[0x10000];

                byte idx = rom[Snes2pc(p_scenes[type] + Level)];
                ushort offset = ReadWord(Snes2pc(0x808158 + idx));
                uint pConfigGfx = Snes2pc(0x800000 | offset);
                byte gfxID = rom[pConfigGfx];
                ushort size = ReadWord(pConfigGfx + 1);
                uint pos = Snes2pc(SReadDWord((uint) (p_gfxpos[type] + gfxID * 5 + 0)));
                var realSize = CompressionCore.GFXRLE(rom, 0, mapRam, 0x200, (int) pos, size, type);

                for (int i = 0; i < sceneUsed; i++)
                {
                    for (int y = 0; y < 16; y++)
                    {
                        for (int x = 0; x < 16; x++)
                        {
                            uint takeMap = (uint) (i * 0x200 + x * 2 + y * 0x20);

                            mapping[writeIndex++] = ReadWord(mapRam, takeMap);
                            //mapping[writeIndex + 0x00] = *takeBlock++;
                            //mapping[writeIndex + 0x01] = *takeBlock++;
                            //mapping[writeIndex + 0x10] = *takeBlock++;
                            //mapping[writeIndex + 0x11] = *takeBlock++;
                            //writeIndex += 2;
                        }
                        //writeIndex += 0x10;
                    }
                }
            }
        }

        internal void ReallocScene(byte scene)
        {
            ushort writeIndex = (ushort) (SReadWord(0x868D20) + 0x100 * scene);
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    uint takeBlock = (uint) (pScenes + scene * 0x80 + x * 2 + y * 0x10);
                    takeBlock = (uint) (pBlocks + ReadWord(takeBlock) * 8);

                    mapping[writeIndex + 0x00] = ReadWord(takeBlock);
                    takeBlock += 2;
                    mapping[writeIndex + 0x01] = ReadWord(takeBlock);
                    takeBlock += 2;
                    mapping[writeIndex + 0x10] = ReadWord(takeBlock);
                    takeBlock += 2;
                    mapping[writeIndex + 0x11] = ReadWord(takeBlock);
                    takeBlock += 2;
                    writeIndex += 2;
                }

                writeIndex += 0x10;
            }
        }

        internal void LoadLayout()
        {
            uint playout = pLayout; //address location of layout for this level
            levelWidth = rom[playout++]; //dereference operator. levelWidth equals value pointed to by address of playout++
            levelHeight = rom[playout++];

            if (type < 3)
            {
                sceneUsed = rom[playout++];

                ushort writeIndex = 0;
                byte ctrl;
                while ((ctrl = rom[playout++]) != 0xFF)
                {
                    byte buf = rom[playout++];
                    for (int i = 0; i < (ctrl & 0x7F); i++) //do this 127 times?
                    {
                        /*if (nmmx.type == 1 && nmmx.level == 7 && writeIndex >= 260) {
                            //this fixes problem after saving level 7 and doesn't screw up levels after
                            sceneLayout[writeIndex + offset] = buf;
                            writeIndex++;
                        }
                        else*/
                        sceneLayout[writeIndex++] = buf;
                        if ((ctrl & 0x80) == 0)
                            buf++;
                    }
                }
            }
            else
            {
                sceneUsed = 0;
                ushort writeIndex = 0;

                for (uint y = 0; y < levelHeight; y++)
                {
                    for (uint x = 0; x < levelWidth; x++)
                    {
                        byte scene = rom[playout++];
                        sceneLayout[writeIndex++] = scene;

                        if (scene + 1 > sceneUsed)
                            sceneUsed = (byte) (scene + 1);
                    }
                }
            }
        }

        internal void SwitchLevelEvent(bool ev)
        {

        }

        private Map[] maps;

        private byte Expand(int i) => (byte) (i * 256.0f / 32.0f);

        private int Transform(int color, bool notTransparent) =>
            //return !transparent ? 0 : (int) (Expand(color & 0x1F) | (Expand((color & 0x3E0) >> 5) << 8) | (Expand((color & 0x7C00) >> 10) << 16) | 0xFF000000);
            !notTransparent ? 0 : (int) (((color & 0x1F) << 3) | ((color & 0x3E0) << 6) | ((color & 0x7C00) << 9) | 0xFF000000);

        private Tile AddTile(World world, uint tile, bool transparent = false, bool background = false)
        {
            uint image = (tile & 0x3FF) << 6;

            byte[] imageData = new byte[TILE_SIZE * TILE_SIZE * sizeof(byte)];
            bool notNull = false;
            using (var ms = new MemoryStream(imageData))
            {
                using var writter = new BinaryWriter(ms);
                for (int i = 0; i < TILE_SIZE * TILE_SIZE; i++, image++)
                {
                    var v = vramCache[image];
                    bool notTransparent = v != 0 || !transparent;
                    notNull |= notTransparent;
                    writter.Write(v);
                }
            }

            //if (!notNull)
            //    return null;

            Tile wtile = world.AddTile(imageData, background);
            return wtile;
        }

        private static void WriteTile(DataRectangle tilemapRect, byte[] data, int mapIndex, int tileRow, int tileCol, int subPalette, bool flipped, bool mirrored)
        {
            int mapRow = mapIndex / 32;
            int mapCol = mapIndex % 32;

            IntPtr ptr = tilemapRect.DataPointer;
            ptr += mapCol * MAP_SIZE * sizeof(byte);
            ptr += World.TILEMAP_WIDTH * mapRow * MAP_SIZE * sizeof(byte);
            ptr += TILE_SIZE * tileCol * sizeof(byte);
            ptr += World.TILEMAP_WIDTH * TILE_SIZE * tileRow * sizeof(byte);

            if (flipped)
                ptr += World.TILEMAP_WIDTH * (TILE_SIZE - 1) * sizeof(byte);

            for (int row = 0; row < TILE_SIZE; row++)
            {
                int dataIndex = row * TILE_SIZE;
                if (mirrored)
                    dataIndex += TILE_SIZE - 1;

                using (var stream = new DataStream(ptr, TILE_SIZE * sizeof(byte), true, true))
                {
                    for (int col = 0; col < TILE_SIZE; col++)
                    {
                        stream.Write((byte) ((subPalette << 4) | (data != null ? data[dataIndex] : 0)));

                        if (mirrored)
                            dataIndex--;
                        else
                            dataIndex++;
                    }
                }

                if (flipped)
                    ptr -= World.TILEMAP_WIDTH * sizeof(byte);
                else
                    ptr += World.TILEMAP_WIDTH * sizeof(byte);
            }
        }

        internal void RefreshMapCache(GameEngine engine, bool background = false)
        {
            var tilemap = new Texture(engine.Device, World.TILEMAP_WIDTH, World.TILEMAP_HEIGHT, 1, Usage.None, Format.L8, Pool.Managed);
            DataRectangle rect = tilemap.LockRectangle(0, LockFlags.Discard);

            maps = new Map[0x400];

            uint map = pMaps;
            /* I didn't write this function, but basically the above loses a lot of data because size of a WORD is max 65535 and pMaps is a DWORD */
            for (int i = 0; i < 0x400; i++)
            {
                byte colisionByte = rom[pCollisions + i];
                var collisionData = (CollisionData) colisionByte;
                Map wmap = engine.World.AddMap(collisionData, background);

                uint tileData = ReadWord(map);
                byte palette = (byte) ((tileData >> 10) & 7);
                byte subPalette = (byte) ((tileData >> 10) & 7);
                bool flipped = (tileData & 0x8000) != 0;
                bool mirrored = (tileData & 0x4000) != 0;
                bool upLayer = (tileData & 0x2000) != 0;
                map += 2;
                Tile tile = AddTile(engine.World, tileData, true, background);
                wmap.SetTile(new Vector(0, 0), tile, palette, flipped, mirrored, upLayer);
                WriteTile(rect, tile?.data, i, 0, 0, palette, flipped, mirrored);

                tileData = ReadWord(map);
                palette = (byte) ((tileData >> 10) & 7);
                flipped = (tileData & 0x8000) != 0;
                mirrored = (tileData & 0x4000) != 0;
                upLayer = (tileData & 0x2000) != 0;
                map += 2;
                tile = AddTile(engine.World, tileData, true, background);
                wmap.SetTile(new Vector(TILE_SIZE, 0), tile, palette, flipped, mirrored, upLayer);
                WriteTile(rect, tile?.data, i, 0, 1, palette, flipped, mirrored);

                tileData = ReadWord(map);
                palette = (byte) ((tileData >> 10) & 7);
                flipped = (tileData & 0x8000) != 0;
                mirrored = (tileData & 0x4000) != 0;
                upLayer = (tileData & 0x2000) != 0;
                map += 2;
                tile = AddTile(engine.World, tileData, true, background);
                wmap.SetTile(new Vector(0, TILE_SIZE), tile, palette, flipped, mirrored, upLayer);
                WriteTile(rect, tile?.data, i, 1, 0, palette, flipped, mirrored);

                tileData = ReadWord(map);
                palette = (byte) ((tileData >> 10) & 7);
                flipped = (tileData & 0x8000) != 0;
                mirrored = (tileData & 0x4000) != 0;
                upLayer = (tileData & 0x2000) != 0;
                map += 2;
                tile = AddTile(engine.World, tileData, true, background);
                wmap.SetTile(new Vector(TILE_SIZE, TILE_SIZE), tile, palette, flipped, mirrored, upLayer);
                WriteTile(rect, tile?.data, i, 1, 1, palette, flipped, mirrored);

                maps[i] = wmap.IsNull ? null : wmap;
            }

            tilemap.UnlockRectangle(0);

            if (background)
                engine.BackgroundTilemap = tilemap;
            else
                engine.ForegroundTilemap = tilemap;
        }

        private void LoadMap(World world, int x, int y, ushort index, bool background = false)
        {
            if (index < maps.Length)
            {
                Map map = maps[index];
                if (map != null)
                    world.SetMap(new Vector(x * MAP_SIZE, y * MAP_SIZE), map, background);
            }
        }

        private void LoadBlock(World world, int x, int y, ushort index, bool background = false)
        {
            uint pmap = (uint) (pBlocks + index * 4);
            x <<= 1;
            y <<= 1;
            LoadMap(world, x + 0, y + 0, ReadWord(pmap), background);
            pmap += 2;
            LoadMap(world, x + 1, y + 0, ReadWord(pmap), background);
            pmap += 2;
            LoadMap(world, x + 0, y + 1, ReadWord(pmap), background);
            pmap += 2;
            LoadMap(world, x + 1, y + 1, ReadWord(pmap), background);
            pmap += 2;
        }

        private void LoadScene(World world, int x, int y, ushort index, bool background = false)
        {
            x <<= 3;
            y <<= 3;
            uint pblock = (uint) (pScenes + (index << 6));
            for (int iy = 0; iy < 8; iy++)
                for (int ix = 0; ix < 8; ix++)
                {
                    LoadBlock(world, x + ix, y + iy, ReadWord(pblock), background);
                    pblock += 2;
                }
        }

        private void LoadSceneEx(World world, int x, int y, ushort index, bool background = false)
        {
            x <<= 4;
            y <<= 4;
            uint pmap = (uint) (index << 8);
            for (int iy = 0; iy < 16; iy++)
                for (int ix = 0; ix < 16; ix++)
                {
                    LoadMap(world, x + ix, y + iy, mapping[pmap], background);
                    pmap++;
                }
        }

        internal void LoadPalette(GameEngine engine, bool background = false)
        {
            var palette = new Texture(engine.Device, 256, 1, 1, Usage.Dynamic, Format.A8R8G8B8, Pool.Default);
            DataRectangle rect = palette.LockRectangle(0, LockFlags.Discard);

            using (var stream = new DataStream(rect.DataPointer, 256 * 1 * sizeof(int), true, true))
            {
                for (int i = 0; i < 16; i++)
                    for (int j = 0; j < 16; j++)
                        stream.Write(new Color(Transform(palCache[(i << 4) | j], j != 0)).ToRgba());
            }

            palette.UnlockRectangle(0);

            if (background)
                engine.BackgroundPalette = palette;
            else
                engine.ForegroundPalette = palette;
        }

        public void LoadToWorld(GameEngine engine, bool background = false)
        {
            LoadPalette(engine, background);

            engine.World.Resize(levelHeight, levelWidth, background);

            RefreshMapCache(engine, background);

            uint tmpLayout = 0;
            for (int y = 0; y < levelHeight; y++)
                for (int x = 0; x < levelWidth; x++)
                    LoadSceneEx(engine.World, x, y, sceneLayout[tmpLayout++], background);
        }

        public void LoadTriggers(GameEngine engine)
        {
            for (ushort point = 0; point < checkpointInfoTable.Count; point++)
            {
                CheckPointInfo info = checkpointInfoTable[point];

                uint minX = ReadWord(info.minX);
                uint minY = ReadWord(info.minY);
                uint maxX = ReadWord(info.maxX);
                uint maxY = ReadWord(info.maxY);
                engine.AddCheckpoint(
                    point,
                    new MMXBox(minX, minY, maxX - minX + SCREEN_WIDTH, maxY - minY + SCREEN_HEIGHT),
                    new Vector(ReadWord(info.chX), ReadWord(info.chY)),
                    new Vector(ReadWord(info.camX), ReadWord(info.camY)),
                    new Vector(ReadWord(info.bkgX), ReadWord(info.bkgY)),
                    new Vector(ReadShort(info.forceX), ReadShort(info.forceY)),
                    ReadByte(info.scroll)
                    );
            }

            foreach (List<EventInfo> list in eventTable)
            {
                foreach (EventInfo info in list)
                {
                    if (info.type == 2)
                    {
                        switch (info.eventId)
                        {
                            case 0x00: // camera lock
                            {
                                uint pBase;
                                if (expandedROM && expandedROMVersion >= 4)
                                {
                                    pBase = Snes2pc((int) (lockBank << 16) | (0x8000 + Level * 0x800 + info.eventSubId * 0x20));
                                }
                                else
                                {
                                    var borderOffset = ReadWord((int) (Snes2pc(pBorders) + 2 * info.eventSubId));
                                    pBase = Snes2pc(borderOffset | ((pBorders >> 16) << 16));
                                }

                                int right = ReadWord(pBase);
                                pBase += 2;
                                int left = ReadWord(pBase);
                                pBase += 2;
                                int bottom = ReadWord(pBase);
                                pBase += 2;
                                int top = ReadWord(pBase);
                                pBase += 2;

                                var boudingBox = new MMXBox(left, top, right - left, bottom - top);

                                uint lockNum = 0;

                                var extensions = new List<Vector>();
                                while (((expandedROM && expandedROMVersion >= 4) ? ReadWord(pBase) : rom[pBase]) != 0)
                                {
                                    MMXBox lockBox = boudingBox;
                                    ushort camOffset = 0;
                                    ushort camValue = 0;

                                    if (expandedROM && expandedROMVersion >= 4)
                                    {
                                        camOffset = ReadWord(pBase);
                                        pBase += 2;
                                        camValue = ReadWord(pBase);
                                        pBase += 2;
                                    }
                                    else
                                    {
                                        ushort offset = (ushort) ((rom[pBase] - 1) << 2);
                                        camOffset = ReadWord(pLocks + offset + 0x0);
                                        camValue = ReadWord(pLocks + offset + 0x2);
                                        pBase++;
                                    }

                                    int lockX0 = (left + right) / 2;
                                    int lockY0 = (top + bottom) / 2;

                                    int lockLeft = lockX0;
                                    int lockTop = lockY0;
                                    int lockRight = lockX0;
                                    int lockBottom = lockY0;

                                    if (type > 0)
                                        camOffset -= 0x10;

                                    if (camOffset is 0x1E5E or 0x1E6E or 0x1E68 or 0x1E60)
                                    {
                                        if (camOffset == 0x1E5E)
                                        {
                                            lockLeft = camValue;
                                        }
                                        else if (camOffset == 0x1E6E)
                                        {
                                            lockBottom = camValue + SCREEN_HEIGHT;
                                        }
                                        else if (camOffset == 0x1E68)
                                        {
                                            lockTop = camValue;
                                        }
                                        else if (camOffset == 0x1E60)
                                        {
                                            lockRight = camValue + SCREEN_WIDTH;
                                        }
                                    }

                                    int lockX = lockLeft < lockX0 ? lockLeft : lockRight;
                                    int lockY = lockTop < lockY0 ? lockTop : lockBottom;

                                    extensions.Add(new Vector(lockX - lockX0, lockY - lockY0));
                                    lockNum++;
                                }

                                engine.AddCameraLockTrigger(boudingBox, extensions);
                                break;
                            }

                            case 0x02:
                            case 0x0B: // checkpoint trigger
                            {
                                engine.AddCheckpointTrigger((ushort) (info.eventSubId & 0xf), new Vector(info.xpos, info.ypos));
                                break;
                            }

                            case 0x15: // dynamic change object/enemy tiles (vertical)
                            {
                                engine.AddChangeDynamicPropertyTrigger(new Vector(info.xpos, info.ypos), DynamicProperty.OBJECT_TILE, (int) (info.eventSubId & 0xf), (int) ((info.eventSubId >> 4) & 0xf), SplitterTriggerOrientation.VERTICAL);
                                break;
                            }

                            case 0x16: // dynamic change background tiles tiles (vertical)
                            {
                                engine.AddChangeDynamicPropertyTrigger(new Vector(info.xpos, info.ypos), DynamicProperty.BACKGROUND_TILE, (int) (info.eventSubId & 0xf), (int) ((info.eventSubId >> 4) & 0xf), SplitterTriggerOrientation.VERTICAL);
                                break;
                            }

                            case 0x17: // dynamic change palette (vertical)
                            {
                                engine.AddChangeDynamicPropertyTrigger(new Vector(info.xpos, info.ypos), DynamicProperty.PALETTE, (int) (info.eventSubId & 0xf), (int) ((info.eventSubId >> 4) & 0xf), SplitterTriggerOrientation.VERTICAL);
                                break;
                            }

                            case 0x18: // dynamic change object/enemy tiles (horizontal)
                            {
                                engine.AddChangeDynamicPropertyTrigger(new Vector(info.xpos, info.ypos), DynamicProperty.OBJECT_TILE, (int) (info.eventSubId & 0xf), (int) ((info.eventSubId >> 4) & 0xf), SplitterTriggerOrientation.HORIZONTAL);
                                break;
                            }

                            case 0x19: // dynamic change background tiles tiles (horizontal)
                            {
                                engine.AddChangeDynamicPropertyTrigger(new Vector(info.xpos, info.ypos), DynamicProperty.BACKGROUND_TILE, (int) (info.eventSubId & 0xf), (int) ((info.eventSubId >> 4) & 0xf), SplitterTriggerOrientation.HORIZONTAL);
                                break;
                            }

                            case 0x1A: // dynamic change palette (horizontal)
                            {
                                engine.AddChangeDynamicPropertyTrigger(new Vector(info.xpos, info.ypos), DynamicProperty.PALETTE, (int) (info.eventSubId & 0xf), (int) ((info.eventSubId >> 4) & 0xf), SplitterTriggerOrientation.HORIZONTAL);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private static readonly char[] UHEXDIGITS = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
        private static readonly char[] LHEXDIGITS = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

        public static string IntToHex(int n, int digits) => IntToHex(n, digits, false);

        /**
         * Returns a string of 8 hexadecimal digits (most significant digit first)
         * corresponding to the integer <i>n</i>, which is treated as unsigned.
         */
        public static string IntToHex(int n, int digits, bool upcase)
        {
            char[] buf = new char[digits];

            char[] hexDigits = upcase ? UHEXDIGITS : LHEXDIGITS;

            for (int i = digits - 1; i >= 0; i--)
            {
                buf[i] = hexDigits[n & 0x0F];
                n >>= 4;
            }

            return new string(buf);
        }

        private static void WriteDump(string prefix, TextWriter writer, byte[] buf, int off, int len)
        {
            unsafe
            {
                WriteDump(prefix, writer, (byte*) Marshal.UnsafeAddrOfPinnedArrayElement(buf, 0).ToPointer(), off, len * sizeof(byte));
            }
        }

        private static void WriteDump(string prefix, TextWriter writer, ushort[] buf, int off, int len)
        {
            unsafe
            {
                WriteDump(prefix, writer, (byte*) Marshal.UnsafeAddrOfPinnedArrayElement(buf, 0).ToPointer(), off, len * sizeof(ushort));
            }
        }

        private static void WriteDump(string prefix, TextWriter writer, uint[] buf, int off, int len)
        {
            unsafe
            {
                WriteDump(prefix, writer, (byte*) Marshal.UnsafeAddrOfPinnedArrayElement(buf, 0).ToPointer(), off, len * sizeof(int));
            }
        }

        private unsafe static void WriteDump(string prefix, TextWriter writer, byte* buf, int off, int len)
        {
            if (buf == null)
                return;

            int k = off;
            int k1 = off + len;
            int l = 0;
            while (k < k1)
            {
                string line = prefix + "+" + IntToHex(l * 16, 6) + " ";
                int k0 = k;
                for (int j = 0; j < 16; j++)
                    if (k < k1)
                    {
                        line += " " + IntToHex(buf[k], 2);
                        k++;
                    }
                    else
                        line += "   ";
                line += "   ";
                k = k0;
                for (int j = 0; j < 16; j++)
                {
                    if (k >= k1)
                        break;
                    if (buf[k] is >= 32 and <= 126)
                        line += (char) buf[k];
                    else
                        line += ".";
                    k++;
                }

                writer.WriteLine(line);
                l++;
            }

            writer.WriteLine();
        }
    }
}
