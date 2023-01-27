using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace MMX.ROM
{
    public struct Interrupts
    {
        internal byte unk1;
        internal byte unk2;
        internal byte unk3;
        internal ushort cop;
        internal ushort brk;
        internal ushort abort;
        internal ushort nmi;
        internal ushort reset;
        internal ushort irq;

        internal void Read(BinaryReader reader)
        {
            unk1 = reader.ReadByte();
            unk2 = reader.ReadByte();
            unk3 = reader.ReadByte();
            cop = reader.ReadUInt16();
            brk = reader.ReadUInt16();
            abort = reader.ReadUInt16();
            nmi = reader.ReadUInt16();
            reset = reader.ReadUInt16();
            irq = reader.ReadUInt16();
        }
    };
    public struct SNESHeader
    {
        internal byte[] titleBytes;
        internal string title;    // C0
        internal byte mapMode;      // D5
        internal byte cartType;     // D6
        internal byte romSize;      // D7
        internal byte ramSize;      // D8
        internal byte country;      // D9
        internal byte license;      // DA
        internal byte version;      // DB
        internal ushort checksumx;    // DC
        internal ushort checksum;     // DE
        internal Interrupts intNative;
        internal Interrupts intEmu;

        internal void Read(BinaryReader reader)
        {
            titleBytes = reader.ReadBytes(21);
            title = Encoding.ASCII.GetString(titleBytes);

            mapMode = reader.ReadByte();
            cartType = reader.ReadByte();
            romSize = reader.ReadByte();
            ramSize = reader.ReadByte();
            country = reader.ReadByte();
            license = reader.ReadByte();
            version = reader.ReadByte();
            checksumx = reader.ReadUInt16();
            checksum = reader.ReadUInt16();

            intNative.Read(reader);
            intEmu.Read(reader);
        }

        internal ushort GetTitleUShort(int offset)
        {
            using (var ms = new MemoryStream(titleBytes))
            {
                using (var reader = new BinaryReader(ms))
                {
                    ms.Position = offset;
                    return reader.ReadUInt16();
                }
            }
        }

        internal uint GetTitleUInt(int offset)
        {
            using (var ms = new MemoryStream(titleBytes))
            {
                using (var reader = new BinaryReader(ms))
                {
                    ms.Position = offset;
                    return reader.ReadUInt32();
                }
            }
        }
    };

    public class SNESCore : MegaEDCore
    {
        internal ushort dumpHeader;
        internal SNESHeader header;
        internal ushort[] palCache;
        internal ushort[] palSpriteCache;
        internal byte[] vramCache;
        internal byte[] spriteCache;

        internal static bool hirom;

        public SNESCore()
        {
            palCache = new ushort[0x100];
            palSpriteCache = new ushort[0x200 << 4];
            vramCache = new byte[0x20000];
            spriteCache = new byte[64 * 0x8000];
        }

        public static uint Snes2pc(int snesAddress)
        {
            return Snes2pc((uint) snesAddress);
        }

        public static uint Snes2pc(uint snesAddress)
        {
            return !hirom ? ((snesAddress & 0x007F0000) >> 1) + (snesAddress & 0x7FFF) : snesAddress & 0x3FFFFF;
        }

        public static uint Pc2snes(int pcAddress)
        {
            return Pc2snes((uint) pcAddress);
        }

        public static uint Pc2snes(uint pcAddress)
        {
            return !hirom ? (uint) (0x800000L + ((pcAddress & 0x3F8000) << 1) + 0x8000L + (pcAddress & 0x7FFF)) : 0xC00000 | pcAddress;
        }

        public override void Init()
        {
            ms.Position = 0x7FC0;
            header.Read(reader);
            if (header.checksum + header.checksumx == 0xFFFF)
            {
                // headerless LOROM
                hirom = false;
                dummyHeader = 0x0;
            }
            else
            {
                ms.Position = 0xFFC0;
                header.Read(reader);
                if (header.checksum + header.checksumx == 0xFFFF)
                {
                    // headerless HIROM
                    hirom = true;
                    dummyHeader = 0x0;
                }
                else
                {
                    ms.Position = 0x81C0;
                    header.Read(reader);
                    if (header.checksum + header.checksumx == 0xFFFF)
                    {
                        // headered LOROM
                        hirom = false;
                        dummyHeader = 0x0;
                        romSize -= 0x200;
                        Array.Copy(rom, 0x200, rom, 0, romSize);
                    }
                    else
                    {
                        ms.Position = 0x101C0;
                        header.Read(reader);
                        if (header.checksum + header.checksumx == 0xFFFF)
                        {
                            // headered HIROM
                            hirom = true;
                            dummyHeader = 0x0;
                            romSize -= 0x200;
                            Array.Copy(rom, 0x200, rom, 0, romSize);
                        }
                    }
                }
            }
        }

        public override void Save()
        {

        }

        public override void Exit()
        {

        }

        internal void LoadHeader()
        {

        }

        internal static void Tile2bpp2raw(byte[] src, int srcOff, byte[] dst, int dstOff)
        {
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                {
                    dst[dstOff++] = (byte) ((src[srcOff + (y << 1)] >> (~x & 7) & 1)
                        | ((src[srcOff + y * 2 + 1] >> (~x & 7) << 1) & 2));
                }
        }

        internal static void Tile4bpp2raw(byte[] src, int srcOff, byte[] dst, int dstOff)
        {
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                {
                    dst[dstOff++] = (byte) ((src[srcOff + (y << 1)] >> (~x & 7) & 1)
                        | ((src[srcOff + y * 2 + 1] >> (~x & 7) << 1) & 2)
                        | ((src[srcOff + y * 2 + 16] >> (~x & 7) << 2) & 4)
                        | ((src[srcOff + y * 2 + 17] >> (~x & 7) << 3) & 8));
                }
        }

        internal static void Raw2tile2bpp(byte[] src, int srcOff, byte[] dst, int dstOff)
        {
            Array.Clear(dst, dstOff, 0x10);
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    dst[y * 2 + 0x00] |= (byte) (((src[srcOff] & 1) != 0 ? 1 : 0) << (~x & 7));
                    dst[y * 2 + 0x01] |= (byte) (((src[srcOff] & 2) != 0 ? 1 : 0) << (~x & 7));
                    dst[y * 2 + 0x08] |= (byte) (((src[srcOff] & 4) != 0 ? 1 : 0) << (~x & 7));
                    dst[y * 2 + 0x09] |= (byte) (((src[srcOff] & 8) != 0 ? 1 : 0) << (~x & 7));
                    srcOff++;
                }
            }
        }

        internal static void Raw2tile4bpp(byte[] src, int srcOff, byte[] dst, int dstOff)
        {
            Array.Clear(dst, dstOff, 0x20);
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    dst[y * 2 + 0x00] |= (byte) (((src[srcOff] & 1) != 0 ? 1 : 0) << (~x & 7));
                    dst[y * 2 + 0x01] |= (byte) (((src[srcOff] & 2) != 0 ? 1 : 0) << (~x & 7));
                    dst[y * 2 + 0x10] |= (byte) (((src[srcOff] & 4) != 0 ? 1 : 0) << (~x & 7));
                    dst[y * 2 + 0x11] |= (byte) (((src[srcOff] & 8) != 0 ? 1 : 0) << (~x & 7));
                    srcOff++;
                }
            }
        }

        internal static ushort Convert16Color(ushort color)
        {
            return (ushort) (((color & 0x1F) << 10) | (color & 0x3E0) | ((color >> 10) & 0x1F));
        }

        internal static uint ConvertRGBColor(ushort color)
        {
            return (uint) (((color & 0x1F) << 3) | ((color & 0x3E0) << 6) | ((color & 0x7C00) << 9));
        }

        internal static uint ConvertBGRColor(ushort color)
        {
            return (uint) (((color & 0x1F) << 19) | ((color & 0x3E0) << 6) | ((color & 0x7C00) >> 7));
        }

        internal static ushort ConvertRGB2SNES(uint color)
        {
            return (ushort) (((color >> 3) & 0x1F) | ((color >> 6) & 0x3E0) | ((color >> 9) & 0x7C00));
        }

        /// <summary>
        /// Convert 5bppBGR to 5bppRGB format
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        internal ushort Get16Color(uint address)
        {
            ushort color = ReadWord(address);
            return (ushort) (((color & 0x1F) << 10) | (color & 0x3E0) | ((color >> 10) & 0x1F));
        }

        internal uint GetRGBColor(uint address)
        {
            ushort color = ReadWord(address);
            return (uint) (((color & 0x1F) << 3) | ((color & 0x3E0) << 6) | ((color & 0x7C00) << 9));
        }

        internal uint GetBGRColor(uint address)
        {
            ushort color = ReadWord(address);
            return (uint) (((color & 0x1F) << 19) | ((color & 0x3E0) << 6) | ((color & 0x7C00) >> 7));
        }

        internal Color GetRGBQuad(uint address)
        {
            ushort color = ReadWord(address);
            return Color.FromArgb((color % 0x20) << 3, ((color >> 5) % 0x20) << 11, ((color >> 10) % 0x20) << 19);
        }

        protected ushort SReadByte(int address)
        {
            return SReadByte((uint) address);
        }

        protected byte SReadByte(uint address)
        {
            ms.Position = Snes2pc((int) address);
            return reader.ReadByte();
        }

        protected ushort SReadWord(int address)
        {
            return SReadWord((uint) address);
        }

        protected ushort SReadWord(uint address)
        {
            ms.Position = Snes2pc((int) address);
            return reader.ReadUInt16();
        }

        protected uint SReadDWord(int address)
        {
            return SReadDWord((uint) address);
        }

        protected uint SReadDWord(uint address)
        {
            ms.Position = Snes2pc((int) address);
            return reader.ReadUInt32();
        }
    }
}
