using System;
using System.IO;
using System.Text;

namespace XSharp.ROM
{
    public class MegaEDCore : IDisposable
    {
        internal byte[] rom; //giant 6mb block of memory used throughout the program
        internal int romSize;
        internal ushort dummyHeader;

        internal MemoryStream ms;
        internal BinaryReader reader;
        internal BinaryWriter writter;

        public MegaEDCore()
        {
        }

        public void FreeRom()
        {
            if (ms != null)
            {
                reader.Close();
                writter.Close();
                ms.Close();

                reader = null;
                writter = null;
                ms = null;
                rom = null;
            }

            dummyHeader = 0; //shouldn't this be 0x200?
        }

        public bool LoadNewRom(string fileName)
        {
            if (fileName == null)
                return false;

            using (FileStream fs = File.Create(fileName))
            {
                return LoadNewRom(fs);
            }
        }

        public bool LoadNewRom(Stream stream)
        {
            if (stream == null)
                return false;

            if (stream.Length > int.MaxValue)
                return false;

            FreeRom();

            romSize = (int) stream.Length;
            rom = new byte[romSize < 0x600000 ? 0x600000 : romSize];
            ms = new MemoryStream(rom);
            reader = new BinaryReader(ms);
            writter = new BinaryWriter(ms);

            stream.Read(rom, 0, romSize);
            Init();

            return true;
        }

        public bool SaveRom(string fileName)
        {
            if (fileName == null)
                return false;

            using (FileStream fs = File.Create(fileName))
            {
                return SaveRom(fs);
            }
        }

        public bool SaveRom(Stream stream)
        {
            if (stream != null)
            {
                stream.Position = 0;
                stream.Write(rom, dummyHeader, romSize);
                stream.Flush();
                return true;
            }

            return false;
        }

        public virtual void Init()
        {

        }

        public virtual void Save()
        {

        }

        public virtual void Exit()
        {

        }

        public void Dispose()
        {
            FreeRom();
        }

        internal byte ReadByte(int address)
        {
            return ReadByte((uint) address);
        }

        internal byte ReadByte(uint address)
        {
            ms.Position = address;
            return reader.ReadByte();
        }

        internal sbyte ReadSByte(int address)
        {
            return ReadSByte((uint) address);
        }

        internal sbyte ReadSByte(uint address)
        {
            ms.Position = address;
            return reader.ReadSByte();
        }

        internal short ReadShort(int address)
        {
            return ReadShort((uint) address);
        }

        internal short ReadShort(uint address)
        {
            ms.Position = address;
            return reader.ReadInt16();
        }

        internal ushort ReadWord(int address)
        {
            return ReadWord((uint) address);
        }

        internal ushort ReadWord(uint address)
        {
            ms.Position = address;
            return reader.ReadUInt16();
        }

        internal static ushort ReadWord(byte[] buf, uint address)
        {
            return (ushort) (buf[address] & 0xff | (buf[address + 1] << 8) & 0xff00);
        }

        internal int ReadInt(int address)
        {
            return ReadInt((uint) address);
        }

        internal int ReadInt(uint address)
        {
            ms.Position = address;
            return reader.ReadInt32();
        }

        internal uint ReadDWord(int address)
        {
            return ReadDWord((uint) address);
        }

        internal uint ReadDWord(uint address)
        {
            ms.Position = address;
            return reader.ReadUInt32();
        }

        internal string ReadASCIIString(int address, int count)
        {
            return ReadASCIIString((uint) address, count);
        }

        internal string ReadASCIIString(uint address, int count)
        {
            ms.Position = address;
            byte[] resultBytes = reader.ReadBytes(count);
            return Encoding.ASCII.GetString(resultBytes);
        }

        internal void WriteShort(int address, short value)
        {
            WriteShort((uint) address, value);
        }

        internal void WriteShort(uint address, short value)
        {
            ms.Position = address;
            writter.Write(value);
        }

        internal void WriteWord(int address, ushort value)
        {
            WriteWord((uint) address, value);
        }

        internal void WriteWord(uint address, ushort value)
        {
            ms.Position = address;
            writter.Write(value);
        }

        internal static void WriteWord(byte[] buf, uint address, ushort value)
        {
            buf[address] = (byte) (value & 0xff);
            buf[address + 1] = (byte) ((value >> 8) & 0xff);
        }

        internal void WriteInt(int address, int value)
        {
            WriteInt((uint) address, value);
        }

        internal void WriteInt(uint address, int value)
        {
            ms.Position = address;
            writter.Write(value);
        }

        internal void WriteDWord(int address, uint value)
        {
            WriteDWord((uint) address, value);
        }

        internal void WriteDWord(uint address, uint value)
        {
            ms.Position = address;
            writter.Write(value);
        }

        internal static void WriteDWord(byte[] buf, uint address, uint value)
        {
            buf[address] = (byte) (value & 0xff);
            buf[address + 1] = (byte) ((value >> 8) & 0xff);
            buf[address + 2] = (byte) ((value >> 16) & 0xff);
            buf[address + 3] = (byte) ((value >> 24) & 0xff);
        }

        internal void WriteASCIIString(int address, string value)
        {
            WriteASCIIString((uint) address, value);
        }

        internal void WriteASCIIString(uint address, string value)
        {
            ms.Position = address;
            byte[] resultBytes = Encoding.ASCII.GetBytes(value);
            writter.Write(resultBytes);
        }

        internal void Copy(int src, int dst, int count)
        {
            Copy((uint) src, (uint) dst, count);
        }

        internal void Copy(uint src, uint dst, int count)
        {
            Array.Copy(rom, src, rom, dst, count);
        }

        internal int Compare(int left, int right, int count)
        {
            return Compare((uint) left, (uint) right, count);
        }

        internal int Compare(uint left, uint right, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (rom[left + i] < rom[right + i])
                    return -1;

                if (rom[left + i] > rom[right + i])
                    return 1;
            }

            return 0;
        }

        internal void Fill(int address, int value, int count)
        {
            Fill((uint) address, value, count);
        }

        internal void Fill(uint address, int value, int count)
        {
            Fill(rom, address, value, count);
        }

        internal static void Fill(byte[] buf, uint address, int value, int count)
        {
            for (int i = 0; i < count; i++)
                buf[address + i] = (byte) value;
        }
    }
}
