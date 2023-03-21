/*
 * The code below was taken from the MegaEDX v1.3 project. Such code was originally written in C++ which I translated to C#.
 * 
 * For more information, consult the original projects:

    MegaEDX: https://github.com/Xeeynamo/MegaEdX
    MegaEDX v1.3: https://github.com/rbrummett/megaedx_v1.3
 */

using System.Drawing;

namespace XSharp.MegaEDX;

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
    public uint offset;

    // checkpoint data structure
    public uint objLoad;
    public uint tileLoad;
    public uint palLoad;
    // X2/X3 extra byte
    public uint byte0;
    public uint chX;
    public uint chY;
    public uint camX;
    public uint camY;
    public uint bkgX;
    public uint bkgY;
    public uint minX;
    public uint maxX;
    public uint minY;
    public uint maxY;
    public uint forceX;
    public uint forceY;
    public uint scroll;
    public uint telDwn;
    // X2/X3 extra byte
    public uint byte1;
    // X3 extra byte
    public uint byte2;

    public void Reset()
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
    public byte match; // seems to match some level information?
    public byte type; // 0=?, 1=cars,lights?, 2=?, 3=enemy
    public ushort ypos;
    public byte eventId;
    public byte eventSubId;
    public byte eventFlag;
    public ushort xpos;
};

public struct PropertyInfo
{
    public uint hp;
    public uint damageMod;

    public void Reset()
    {
        hp = 0;
        damageMod = 0;
    }
};

public class MMXCore : SNESCore
{
    public const int SCREEN_WIDTH = 256; // In pixels
    public const int SCREEN_HEIGHT = 224; // In pixels

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
    protected readonly byte[] vram;
    protected readonly ushort[] mapping;
    protected readonly byte[] sceneLayout;

    protected readonly uint[] palettesOffset;
    protected uint tileCmpPos;
    protected ushort tileCmpDest;
    protected ushort tileCmpSize;
    protected ushort tileCmpRealSize;
    protected uint tileDecPos;
    protected ushort tileDecDest;
    protected ushort tileDecSize;

    // checkpoints
    protected readonly List<CheckPointInfo> checkpointInfoTable;

    public void UpdateVRAMCache()
    {
        for (int i = 0; i < 0x400; i++)
            Raw2tile4bpp(vramCache, i * 0x40, vram, i * 0x20);
    }

    // graphics to palette
    protected readonly Dictionary<uint, uint> graphicsToPalette;
    protected readonly Dictionary<uint, uint> graphicsToAssembly;
    protected byte levelWidth;
    protected byte levelHeight;
    protected byte sceneUsed;
    protected uint pPalette;
    protected uint pPalBase;
    protected uint pLayout;
    protected uint pScenes;
    protected uint pBlocks;
    protected uint pMaps;
    protected uint pCollisions;
    protected readonly uint pEvents;
    protected uint pBorders;
    protected uint pLocks;
    protected readonly uint pProperties;
    protected uint pGfx;
    protected readonly uint pGfxPos;
    protected uint pGfxObj;
    protected uint pGfxPal;
    protected uint pSpriteAssembly;
    protected readonly uint[] pSpriteOffset;
    protected uint pCapsulePos;

    protected uint numLevels;
    protected uint numTiles;
    protected uint numMaps;
    protected uint numBlocks;
    protected uint numDecs;
    protected uint numCheckpoints;
    protected uint numGfxIds;

    private int objLoad;
    private int tileLoad;
    private int palLoad;

    private uint tileDecStart;
    private uint tileDecEnd;

    private bool sortOk;

    // ROM expansion
    protected static readonly string expandedROMString = "EXPANDED ROM ";
    protected static readonly ushort expandedROMVersion;
    protected static readonly uint expandedROMHeaderSize;
    protected static readonly uint expandedROMTrampolineOffset;

    protected uint eventBank;
    protected uint checkpointBank;
    protected uint lockBank;
    protected bool expandedROM;
    protected uint expandedVersion;
    protected uint expandedLayoutSize;
    protected uint expandedEventSize;
    protected readonly uint expandedSceneSize;
    protected uint expandedCheckpointSize;
    protected uint expandedLayoutScenes;

    // Events
    protected readonly List<EventInfo>[] eventTable;

    // Properties
    protected readonly PropertyInfo[] propertyTable;

    // SPRITE
    protected readonly HashSet<uint> spriteUpdate;

    // FONT
    protected readonly ushort[] fontPalCache;
    protected readonly byte[] fontCache;

    public byte Type
    {
        get;
        private set;
    }

    public ushort Level
    {
        get;
        private set;
    }

    public Point BackgroundPos
    {
        get
        {
            if (checkpointInfoTable == null || checkpointInfoTable.Count == 0)
                return System.Drawing.Point.Empty;

            int x = ReadWord(checkpointInfoTable[Point].bkgX);
            int y = ReadWord(checkpointInfoTable[Point].bkgY);
            return new Point(x, y);
        }
    }

    public Point CameraPos
    {
        get
        {
            if (checkpointInfoTable == null || checkpointInfoTable.Count == 0)
                return System.Drawing.Point.Empty;

            int x = ReadWord(checkpointInfoTable[Point].camX);
            int y = ReadWord(checkpointInfoTable[Point].camY);
            return new Point(x, y);
        }
    }

    public Point CharacterPos
    {
        get
        {
            if (checkpointInfoTable == null || checkpointInfoTable.Count == 0)
                return System.Drawing.Point.Empty;

            int x = ReadWord(checkpointInfoTable[Point].chX);
            int y = ReadWord(checkpointInfoTable[Point].chY);
            return new Point(x, y);
        }
    }

    public Point MinCharacterPos
    {
        get
        {
            if (checkpointInfoTable == null || checkpointInfoTable.Count == 0)
                return System.Drawing.Point.Empty;

            int x = ReadWord(checkpointInfoTable[Point].minX);
            int y = ReadWord(checkpointInfoTable[Point].minY);
            return new Point(x, y);
        }
    }

    public Point MaxCharacterPos
    {
        get
        {
            if (checkpointInfoTable == null || checkpointInfoTable.Count == 0)
                return System.Drawing.Point.Empty;

            int x = ReadWord(checkpointInfoTable[Point].maxX) + SCREEN_WIDTH;
            int y = ReadWord(checkpointInfoTable[Point].maxY) + SCREEN_HEIGHT;
            return new Point(x, y);
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
        Type = 0xff;
        vram = new byte[0x10000];
        mapping = new ushort[0x10 * 0x10 * 0x100];
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

    public byte CheckROM()
    {
        LoadHeader();
        uint expandedHeader = 0;

        uint headerTitleInt0 = header.GetTitleUInt(0);
        uint headerTitleInt4 = header.GetTitleUInt(4);
        ushort headerTitleShort8 = header.GetTitleUShort(8);

        if (headerTitleInt0 is 0x4147454D or 0x6167654D or 0x4B434F52)
        {
            if (headerTitleInt4 is 0x204E414D or 0x206E616D or 0x264E414D)
            {
                switch (headerTitleShort8)
                {
                    case 0x2058:
                        //Megaman X1
                        Type = 0;
                        numLevels = 13;
                        eventBank = (p_events[Type]) >> 16;
                        checkpointBank = (p_checkp[Type]) >> 16;
                        lockBank = (p_borders[Type]) >> 16;
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
                        Type = 1;
                        numLevels = 13;
                        eventBank = (p_events[Type]) >> 16;
                        expandedROM = header.romSize == 0xC && romSize == 0x280000 && "EXPANDED ROM  " == ReadASCIIString(0x180000 + 0x8000 - expandedROMHeaderSize, 14);
                        checkpointBank = (p_checkp[Type]) >> 16;
                        lockBank = (p_borders[Type]) >> 16;
                        if (expandedROM)
                        {
                            eventBank = 0xB2;
                            checkpointBank = (p_events[Type]) >> 16;
                            lockBank = 0xBB;
                            expandedHeader = 0x180000 + 0x8000 - expandedROMHeaderSize;
                        }

                        break;
                    case 0x3358:
                        //Megaman X3
                        Type = 2;
                        numLevels = 15;
                        eventBank = (p_events[Type]) >> 16;
                        checkpointBank = (p_checkp[Type]) >> 16;
                        lockBank = (p_borders[Type]) >> 16;
                        expandedROM = header.romSize == 0xC && romSize == 0x300000 && "EXPANDED ROM  " == ReadASCIIString(0x200000 + 0x8000 - expandedROMHeaderSize, 14);
                        if (expandedROM)
                        {
                            eventBank = 0xC2;
                            checkpointBank = (p_events[Type]) >> 16;
                            lockBank = 0xCB;
                            expandedHeader = 0x200000 + 0x8000 - expandedROMHeaderSize;
                        }

                        break;
                    case 0x2026:
                    case 0x4F46:
                        //Rockman & Forte. English & Japanese??
                        Type = 3;
                        numLevels = 13;
                        eventBank = (p_events[Type]) >> 16;
                        checkpointBank = (p_checkp[Type]) >> 16;
                        lockBank = (p_borders[Type]) >> 16;
                        expandedROM = header.romSize == 0xD && romSize == 0x600000 && "EXPANDED ROM  " == ReadASCIIString(0x200000 + 0x8000 - expandedROMHeaderSize, 14);
                        if (expandedROM)
                        {
                            // FIXME:
                            eventBank = (p_events[Type]) >> 16;
                            checkpointBank = (p_checkp[Type]) >> 16;
                            lockBank = (p_borders[Type]) >> 16;
                            expandedHeader = 0x400000 + 0x8000 - expandedROMHeaderSize;
                        }

                        break;
                    default:
                        Type = 0xFF;
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

                return (byte) (Type + 1);
            }
        }

        Type = 0xFF;
        return 0;
    }

    public uint GetFontPointer()
    {
        if (Type < 3)
        {
            uint address = Snes2pc((int) p_font[Type]);
            ushort offset = ReadWord(address); //0xc180;
            return Snes2pc(offset + ((Type == 0) ? 0x9C0000 : 0x1C0000));
        }
        else
        {
            uint pConfigGfx = Snes2pc(ReadWord(Snes2pc((int) (p_gfxcfg[Type] + 0x0))) | 0x80 << 16);
            byte gfxID = rom[pConfigGfx];
            //tileCmpSize = ReadWord(pConfigGfx + 1); //SReadWord(p_gfxpos[type] + gfxID * 5 + 3);
            //tileCmpDest = (ReadWord(pConfigGfx + 3) << 1) - 0x2000;
            tileCmpPos = Snes2pc((int) ReadDWord(Snes2pc((int) (p_gfxpos[Type] + gfxID * 5 + 0))));
            return tileCmpPos;
        }
    }

    public void LoadFont()
    {
        byte[] textCache = new byte[0x2000];
        CompressionCore.GFXRLE(rom, 0, textCache, 0, (int) GetFontPointer(), 0x1000, Type);

        for (int i = 0; i < 0x20; i++) // Decompress the 32 colors
            fontPalCache[i] = Get16Color((uint) (((Type == 0) ? 0x2D140 : (Type == 1) ? 0x2CF20 : (Type == 2) ? 0x632C0 : 0x50000) + i * 2)); // 0x2D3E0

        for (int i = 0; i < 0x100; i++)
        {
            // Decompress all 256 tiles in ram
            int tempChar = (Type == 0) ? i : i + 0x10;
            Tile2bpp2raw(textCache, i * 0x10, fontCache, tempChar * 0x40 + 0x400);
        }
    }

    public uint GetCheckPointPointer(uint p)
    {
        //notice the bitwise operations
        return Snes2pc((int) (((p_checkp[Type] & 0xFFFF) | (checkpointBank << 16)) + SReadWord(p_checkp[Type] + SReadWord((uint) (p_checkp[Type] + Level * 2)) + p * 2)));
    }

    public uint GetCheckPointBasePointer()
    {
        return Snes2pc((int) (((p_checkp[Type] & 0xFFFF) | (checkpointBank << 16)) + SReadWord(p_checkp[Type] + SReadWord((uint) (p_checkp[Type] + Level * 2)) + 0 * 2)));
    }

    private static readonly ushort[,] origEventSize = { { 0x2c8,0x211,0x250,0x4b3,0x2ea,0x32c,0x2e2,0x260,0x2d2,0x37f,0x254,0x2b2,0x27,0 },
                                    { 0x235,0x4a7,0x338,0x489,0x310,0x382,0x3b6,0x3da,0x45c,0x303,0x212,0x30f,0xbd,0 },
                                    { 0x2f1,0x3b4,0x3a7,0x3d9,0x3da,0x455,0x3c9,0x405,0x33b,0x22b,0x3cb,0x2ba,0x274,0xe6 } };

    //unsigned GetEventSize();
    public uint GetOrigEventSize()
    {
        return expandedROM ? expandedEventSize : origEventSize[Type, Level];
    }

    private static readonly ushort[,] origLayoutSize = { { 0x12, 0x32, 0x38, 0x64, 0x22, 0x3a, 0x1e, 0x6a, 0x2a, 0x3c, 0x22, 0x1a, 0x00 },
                                    { 0x8c, 0x3e, 0x38, 0x40, 0x42, 0x5c, 0x2a, 0x4e, 0x5e, 0x5a, 0x16, 0x5a, 0x00 },
                                    { 0x4c, 0x4c, 0x38, 0x42, 0x60, 0x54, 0x4e, 0x52, 0x30, 0x2e, 0x4e, 0x46, 0x22 } };

    public uint GetOrigLayoutSize()
    {
        return expandedROM ? expandedLayoutSize : origLayoutSize[Type, Level];
    }

    public Rectangle GetBoundingBox(ref EventInfo e)
    {
        var rect = new Rectangle(0, 0, 0, 0);

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

                rect = new Rectangle(left, top, right - left, bottom - top);
            }
        }
        else if (e.type == 0x2 && e.eventId >= 0x15 && e.eventId <= 0x18)
        {
            // draw green line
            int left = e.xpos + ((e.eventId & 0x8) != 0 ? -128 : -5);
            int top = e.ypos + ((e.eventId & 0x8) == 0 ? -112 : -5);
            int bottom = e.ypos + ((e.eventId & 0x8) == 0 ? 112 : 5);
            int right = e.xpos + ((e.eventId & 0x8) != 0 ? 128 : 5);

            rect = new Rectangle(left, top, right - left, bottom - top);
        }
        else if (pSpriteAssembly != 0 && pSpriteOffset[e.type] != 0
            && (e.type != 1 || Type == 0 && e.eventId == 0x21)
            && (e.type != 0 || e.eventId == 0xB && e.eventSubId == 0x4)
            && !(Type == 1 && e.eventId == 0x2) // something near the arm doesn't have graphics
            )
        {
            // draw associated object sprite

            uint assemblyNum = ReadDWord((uint) (pSpriteOffset[e.type] + (e.eventId - 1) * (Type == 2 ? 5 : 2)));

            // workarounds for some custom types
            if (Type == 0 && e.type == 1 && e.eventId == 0x21)
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

            var boundingBox = new Rectangle(0, 0, ushort.MaxValue, ushort.MaxValue);

            for (int i = 0; i < tileCnt; ++i)
            {
                uint map = (uint) (baseMap + (tileCnt - i - 1) * 4);
                sbyte xpos = 0;
                sbyte ypos = 0;
                uint tile = 0;
                uint info = 0;

                if (Type == 0)
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

                if (Type == 2)
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

                    boundingBox = new Rectangle(left, top, right - left, bottom - top);
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

            rect = new Rectangle(left, top, right - left, bottom - top);
        }

        return rect;
    }

    public void LoadGFXs()
    {
        pGfx = Snes2pc(p_gfxpos[Type]);
        pGfxObj = p_gfxobj[Type] != 0 ? Snes2pc(p_gfxobj[Type]) : 0x0;
        pGfxPal = p_gfxobj[Type] != 0 ? Snes2pc(p_gfxpal[Type]) : 0x0;
        pSpriteAssembly = p_spriteAssembly[Type] != 0 ? Snes2pc(p_spriteAssembly[Type]) : 0x0;
        pSpriteOffset[0] = p_objOffset[Type] != 0 ? Snes2pc(p_objOffset[Type]) : 0x0; //type not event type
        pSpriteOffset[1] = p_objOffset[Type] != 0 ? Snes2pc(p_objOffset[Type]) : 0x0;
        pSpriteOffset[3] = p_spriteOffset[Type] != 0 ? Snes2pc(p_spriteOffset[Type]) : 0x0;

        if (Type < 3)
        {
            uint pConfigGfx = Snes2pc(SReadWord((uint) (p_gfxcfg[Type] + Level * 2 + 4)) | 0x86 << 16);
            byte gfxID = rom[pConfigGfx];
            tileCmpSize = ReadWord(pConfigGfx + 1);
            tileCmpDest = (ushort) ((ReadWord(pConfigGfx + 3) << 1) - 0x2000);
            tileCmpPos = Snes2pc(SReadDWord((uint) (p_gfxpos[Type] + gfxID * 5 + 2)));
            tileCmpRealSize = (ushort) CompressionCore.GFXRLE(rom, 0, vram, tileCmpDest, (int) tileCmpPos, tileCmpSize, Type);
        }
        else
        {
            // FIXME: 0x14 offset needs to be per level.  destination needs to be source based
            uint pConfigGfx = Snes2pc(SReadWord(p_gfxcfg[Type] + SReadByte(0x80824A + Level) | 0x80 << 16));
            byte gfxID = rom[pConfigGfx];
            tileCmpSize = ReadWord(pConfigGfx + 1); //SReadWord(p_gfxpos[type] + gfxID * 5 + 3);
            tileCmpDest = (ushort) ((ReadWord(pConfigGfx + 3) << 1) - 0x2000);
            tileCmpPos = Snes2pc(SReadDWord((uint) (p_gfxpos[Type] + gfxID * 5 + 0)));
            tileCmpRealSize = (ushort) CompressionCore.GFXRLE(rom, 0, vram, tileCmpDest, (int) tileCmpPos, tileCmpSize, Type);
        }
    }

    private class TileSortComparator : IComparer<STileInfo>
    {
        public int Compare(STileInfo a, STileInfo b)
        {
            return a.value - b.value;
        }
    }

    private static readonly IComparer<STileInfo> TileSortComparer = new TileSortComparator();

    public void LoadTiles()
    {
        byte tileSelect = (byte) TileLoad;

        uint tileOffset = (uint) ((Type == 0) ? 0x321D5
            : (Type == 1) ? 0x31D6A
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

    public void SortTiles()
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
            ushort tempSize = CompressionCore.GFXRLECmp(sortram, 0x200, cmpram, 0, tileCmpSize, Type);

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

    public void SetLevel(ushort iLevel, ushort iPoint, int objLoad = -1, int tileLoad = -1, int palLoad = -1)
    {
        Level = iLevel;
        Point = iPoint;

        tileCmpSize = 0;
        tileDecSize = 0;

        ObjLoad = objLoad;
        TileLoad = tileLoad;
        PalLoad = palLoad;
    }

    public void LoadLevel(bool skipEvent = false, bool skipLayout = false)
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
        if (Type < 3)
        {
            pLayout = Snes2pc(SReadDWord(p_layout[Type] + pLevel));
            pScenes = Snes2pc(SReadDWord(p_scenes[Type] + pLevel));
        }
        else
        {
            pLayout = Snes2pc(0xC50000 | SReadWord((uint) (p_layout[Type] + Level * 2)));
            pScenes = Snes2pc(SReadDWord(p_scenes[Type] + Level));
        }

        pBlocks = Snes2pc(SReadDWord(p_blocks[Type] + pLevel));
        pMaps = Snes2pc(SReadDWord(p_maps[Type] + pLevel));
        pCollisions = Snes2pc(SReadDWord(p_collis[Type] + pLevel));

        sortOk = true;

        if (Type == 1 && (Level == 10 || Level == 11))
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
            numBlocks = (Snes2pc(SReadDWord(p_blocks[Type] + pNextLevel)) - pBlocks) / 0x8;
            numMaps = (Snes2pc(SReadDWord(p_maps[Type] + pNextLevel)) - pMaps) / 0x8;

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
    }

    public void LoadBackground(bool skipLayout = false)
    {
        ushort pLevel = (ushort) (Level * 3);
        if (Type < 3)
        {
            pLayout = Snes2pc(SReadDWord(p_blayout[Type] + pLevel));
            pScenes = Snes2pc(SReadDWord(p_bscenes[Type] + pLevel));
        }
        else
        {
        }

        pBlocks = Snes2pc(SReadDWord(p_bblocks[Type] + pLevel));
        LoadTilesAndPalettes();
        if (!skipLayout)
        {
            LoadLayout();
        }

        LoadLevelLayout();

        expandedLayoutScenes = 0x20;
    }

    public void LoadTilesAndPalettes()
    {
        // Load Palettes
        pPalBase = Snes2pc(p_palett[Type]);
        if (Type < 3)
        {
            uint configPointer = Snes2pc(SReadWord((uint) (p_palett[Type] + Level * 2 + 0x60)) | 0x860000);
            byte colorsToLoad = rom[configPointer++];
            pPalette = Type == 2 ? Snes2pc(ReadWord(configPointer++) | 0x8C0000) : Snes2pc(ReadWord(configPointer++) | 0x850000);

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

        if (Type < 3)
            LoadPaletteDynamic();

        LoadGFXs();
        if (Type < 3)
            LoadTiles();

        for (int i = 0; i < 0x400; i++)
            Tile4bpp2raw(vram, i << 5, vramCache, i << 6);
    }

    public void LoadEvents()
    {
        pBorders = 0;
        pLocks = 0;

        if (p_events[Type] == 0)
            return;

        foreach (var eventList in eventTable)
            eventList.Clear();

        if (Type < 3)
        {
            if (p_borders[Type] != 0)
                pBorders = SReadWord((uint) (p_borders[Type] + Level * 2)) | ((p_borders[Type] >> 16) << 16);

            if (p_locks[Type] != 0)
                pLocks = Snes2pc(p_locks[Type]);

            if (p_capsulepos[Type] != 0)
                pCapsulePos = Snes2pc(p_capsulepos[Type]);

            uint pEvents = Snes2pc(SReadWord((uint) ((expandedROM ? ((eventBank << 16) | 0xFFE0) : p_events[Type]) + Level * 2)) | (eventBank << 16));
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
            uint levelAddr = Snes2pc(SReadWord((uint) (p_events[Type] + Level * 2)) | (eventBank << 16));
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

    public void LoadProperties()
    {
        if (p_properties[Type] == 0)
            return;

        if (Type != 2)
        {
            // X1 and X2 have inline LDAs with constants
            for (uint i = 0; i < 107; ++i)
            {
                propertyTable[i].Reset();

                uint jsl = SReadDWord(SReadWord(p_properties[Type] + i * 2) | ((p_properties[Type] >> 16) << 16));
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
                propertyTable[i].hp = Snes2pc(p_properties[Type]) + offset + 0x3;
                propertyTable[i].damageMod = Snes2pc(p_properties[Type]) + offset + 0x4;
            }
        }
    }

    public void LoadCheckpoints()
    {
        // count checkpoint trigger events
        numCheckpoints = (uint) ((Type < 3) ? 1 : 0);

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
                ci.offset = Snes2pc((uint) (p_checkp[Type] + SReadWord((uint) (p_checkp[Type] + Level * 2)) + i * 2));
            }

            ci.objLoad = ptr++; // LPBYTE(ptr++);
            ci.tileLoad = ptr++;
            ci.palLoad = ptr++;

            if (Type > 0)
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

            if (Type > 0)
                ci.byte1 = ptr++;

            if (Type > 1)
                ci.byte2 = ptr++;

            checkpointInfoTable.Add(ci);
        }
    }

    public void LoadGraphicsChange()
    {
        // just setup the palette for now
        graphicsToPalette.Clear();
        graphicsToAssembly.Clear();

        ushort levelOffset = ReadWord(pGfxObj + (uint) Level * 2);
        ushort objOffset = ReadWord(pGfxObj + levelOffset + 0 * 2);

        // add 0
        uint graphicsNum = rom[pGfxObj + objOffset];
        while (graphicsNum != 0xFF)
        {
            if (!graphicsToPalette.ContainsKey(graphicsNum))
            {
                uint palOffset = ReadWord(pGfxPal + objOffset);
                graphicsToPalette[graphicsNum] = palOffset;
            }

            objOffset += 6;
            graphicsNum = rom[pGfxObj + objOffset];
        }

        numGfxIds = 0;
        uint numBossTeleports = 0;
        foreach (var eventList in eventTable)
        {
            foreach (var ev in eventList)
            {
                if (ev.type == 0x2 && (ev.eventId == 0x15 || ev.eventId == 0x18))
                {
                    uint subId = ev.eventSubId;

                    if (((subId >> 0) & 0xF) >= numGfxIds)
                        numGfxIds = ((subId >> 0) & 0xF) + 1;

                    if (((subId >> 4) & 0xF) >= numGfxIds)
                        numGfxIds = ((subId >> 4) & 0xF) + 1;

                    for (uint i = 0; i < 2; i++)
                    {
                        objOffset = ReadWord(pGfxObj + levelOffset + (uint) ((ev.eventSubId >> (byte) (i * 4)) & 0xF) * 2);

                        graphicsNum = rom[pGfxObj + objOffset];
                        while (graphicsNum != 0xFF)
                        {
                            if (!graphicsToPalette.ContainsKey(graphicsNum))
                            {
                                uint palOffset = rom[pGfxPal + objOffset];
                                graphicsToPalette[graphicsNum] = palOffset;
                            }

                            objOffset += 6;
                            graphicsNum = rom[pGfxObj + objOffset];

                        }
                    }
                }

                else if (ev.type == 0x3)
                {
                    uint spriteIndex = rom[pSpriteOffset[ev.type] + (ev.eventId - 1) * (Type == 2 ? 5 : 2) + 1]; //(spriteOffset == 0x32) ? 0x30 : 0x1F;
                    byte spriteAssembly = rom[pSpriteOffset[ev.type] + (ev.eventId - 1) * (Type == 2 ? 5 : 2)];

                    if (!graphicsToAssembly.ContainsKey(spriteIndex))
                    {
                        graphicsToAssembly[spriteIndex] = spriteAssembly;
                    }
                }
                else if (Type == 1 && ev.type == 0x1 && ev.eventId == 0x40
                        || Type == 2 && ev.type == 0x0 && ev.eventId == 0xD)
                {
                    numBossTeleports++;
                }
            }
        }

        // load the boss
        objOffset = ReadWord(pGfxObj + levelOffset + numGfxIds * 2);
        graphicsNum = rom[pGfxObj + objOffset];
        while (graphicsNum != 0xFF)
        {
            if (!graphicsToPalette.ContainsKey(graphicsNum))
            {
                uint palOffset = rom[pGfxPal + objOffset];
                graphicsToPalette[graphicsNum] = palOffset;
            }

            objOffset += 6;
            graphicsNum = rom[pGfxObj + objOffset];

        }

        // test to load "everything".  this misses increments from some boss doors as it assumes there is only one
        if (numGfxIds > 0)
        {
            numGfxIds++;
            for (uint i = 0; i < numGfxIds; i++)
            {
                objOffset = ReadWord(pGfxObj + levelOffset + i * 2);
                graphicsNum = rom[pGfxObj + objOffset];
                while (graphicsNum != 0xFF)
                {
                    if (!graphicsToPalette.ContainsKey(graphicsNum))
                    {
                        uint palOffset = ReadWord(pGfxPal + objOffset);
                        graphicsToPalette[graphicsNum] = palOffset;
                    }

                    objOffset += 6;
                    graphicsNum = rom[pGfxObj + objOffset];

                }
            }
        }

        if (numBossTeleports > 0)
        {
            numBossTeleports++;
            for (uint i = 0; i < numBossTeleports; i++)
            {
                objOffset = ReadWord(pGfxObj + levelOffset + i * 2);
                graphicsNum = rom[pGfxObj + objOffset];
                while (graphicsNum != 0xFF)
                {
                    if (!graphicsToPalette.ContainsKey(graphicsNum))
                    {
                        uint palOffset = ReadWord(pGfxPal + objOffset);
                        graphicsToPalette[graphicsNum] = palOffset;
                    }

                    objOffset += 6;
                    graphicsNum = rom[pGfxObj + objOffset];
                }
            }
        }
    }

    public void LoadPaletteDynamic()
    {
        uint paletteOffset = (uint) ((Type == 0) ? 0x32260
                       : (Type == 1) ? 0x31DD1
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

                palettesOffset[writeTo >> 4] = Snes2pc(colorPointer | (Type == 2 ? 0x8C0000 : 0x850000));
                for (int j = 0; j < 0x10; j++)
                    palCache[writeTo + j] = Convert16Color(ReadWord(Snes2pc((Type == 2 ? 0x8C0000 : 0x850000) | colorPointer + j * 2)));

                mainIndex += 3;
            }
        }
    }

    public void LoadLevelLayout()
    {
        ushort writeIndex = 0;

        // Load other things O.o
        //writeIndex = SReadWord(0x868D20 + step*2);
        if (Type < 3)
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

            byte idx = rom[Snes2pc(p_scenes[Type] + Level)];
            ushort offset = ReadWord(Snes2pc(0x808158 + idx));
            uint pConfigGfx = Snes2pc(0x800000 | offset);
            byte gfxID = rom[pConfigGfx];
            ushort size = ReadWord(pConfigGfx + 1);
            uint pos = Snes2pc(SReadDWord((uint) (p_gfxpos[Type] + gfxID * 5 + 0)));
            var realSize = CompressionCore.GFXRLE(rom, 0, mapRam, 0x200, (int) pos, size, Type);

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

    public void LoadLayout()
    {
        uint playout = pLayout; //address location of layout for this level
        levelWidth = rom[playout++]; //dereference operator. levelWidth equals value pointed to by address of playout++
        levelHeight = rom[playout++];

        if (Type < 3)
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
}