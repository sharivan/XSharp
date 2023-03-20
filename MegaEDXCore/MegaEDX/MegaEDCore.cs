/*
 * The code below was taken from the MegaEDX v1.3 project. Such code was originally written in C++ which I translated to C#.
 * 
 * For more information, consult the original projects:

    MegaEDX: https://github.com/Xeeynamo/MegaEdX
    MegaEDX v1.3: https://github.com/rbrummett/megaedx_v1.3
 */

using System.Text;

namespace XSharp.MegaEDX;

public class MegaEDCore : IDisposable
{
    protected byte[] rom; //giant 6mb block of memory used throughout the program
    protected int romSize;
    protected ushort dummyHeader;

    protected MemoryStream ms;
    protected BinaryReader reader;
    protected BinaryWriter writter;

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

        using FileStream fs = File.Open(fileName, FileMode.Open);
        return LoadNewRom(fs);
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

        using FileStream fs = File.Create(fileName);
        return SaveRom(fs);
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
        try
        {
            FreeRom();
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
    }

    protected byte ReadByte(int address)
    {
        return ReadByte((uint) address);
    }

    protected byte ReadByte(uint address)
    {
        ms.Position = address;
        return reader.ReadByte();
    }

    protected sbyte ReadSByte(int address)
    {
        return ReadSByte((uint) address);
    }

    protected sbyte ReadSByte(uint address)
    {
        ms.Position = address;
        return reader.ReadSByte();
    }

    protected short ReadShort(int address)
    {
        return ReadShort((uint) address);
    }

    protected short ReadShort(uint address)
    {
        ms.Position = address;
        return reader.ReadInt16();
    }

    protected ushort ReadWord(int address)
    {
        return ReadWord((uint) address);
    }

    protected ushort ReadWord(uint address)
    {
        ms.Position = address;
        return reader.ReadUInt16();
    }

    protected static ushort ReadWord(byte[] buf, uint address)
    {
        return (ushort) (buf[address] & 0xff | (buf[address + 1] << 8) & 0xff00);
    }

    protected int ReadInt(int address)
    {
        return ReadInt((uint) address);
    }

    protected int ReadInt(uint address)
    {
        ms.Position = address;
        return reader.ReadInt32();
    }

    protected uint ReadDWord(int address)
    {
        return ReadDWord((uint) address);
    }

    protected uint ReadDWord(uint address)
    {
        ms.Position = address;
        return reader.ReadUInt32();
    }

    protected string ReadASCIIString(int address, int count)
    {
        return ReadASCIIString((uint) address, count);
    }

    protected string ReadASCIIString(uint address, int count)
    {
        ms.Position = address;
        byte[] resultBytes = reader.ReadBytes(count);
        return Encoding.ASCII.GetString(resultBytes);
    }

    protected void WriteShort(int address, short value)
    {
        WriteShort((uint) address, value);
    }

    protected void WriteShort(uint address, short value)
    {
        ms.Position = address;
        writter.Write(value);
    }

    protected void WriteWord(int address, ushort value)
    {
        WriteWord((uint) address, value);
    }

    protected void WriteWord(uint address, ushort value)
    {
        ms.Position = address;
        writter.Write(value);
    }

    protected static void WriteWord(byte[] buf, uint address, ushort value)
    {
        buf[address] = (byte) (value & 0xff);
        buf[address + 1] = (byte) ((value >> 8) & 0xff);
    }

    protected void WriteInt(int address, int value)
    {
        WriteInt((uint) address, value);
    }

    protected void WriteInt(uint address, int value)
    {
        ms.Position = address;
        writter.Write(value);
    }

    protected void WriteDWord(int address, uint value)
    {
        WriteDWord((uint) address, value);
    }

    protected void WriteDWord(uint address, uint value)
    {
        ms.Position = address;
        writter.Write(value);
    }

    protected static void WriteDWord(byte[] buf, uint address, uint value)
    {
        buf[address] = (byte) (value & 0xff);
        buf[address + 1] = (byte) ((value >> 8) & 0xff);
        buf[address + 2] = (byte) ((value >> 16) & 0xff);
        buf[address + 3] = (byte) ((value >> 24) & 0xff);
    }

    protected void WriteASCIIString(int address, string value)
    {
        WriteASCIIString((uint) address, value);
    }

    protected void WriteASCIIString(uint address, string value)
    {
        ms.Position = address;
        byte[] resultBytes = Encoding.ASCII.GetBytes(value);
        writter.Write(resultBytes);
    }

    protected void Copy(int src, int dst, int count)
    {
        Copy((uint) src, (uint) dst, count);
    }

    protected void Copy(uint src, uint dst, int count)
    {
        Array.Copy(rom, src, rom, dst, count);
    }

    protected int Compare(int left, int right, int count)
    {
        return Compare((uint) left, (uint) right, count);
    }

    protected int Compare(uint left, uint right, int count)
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

    protected void Fill(int address, int value, int count)
    {
        Fill((uint) address, value, count);
    }

    protected void Fill(uint address, int value, int count)
    {
        Fill(rom, address, value, count);
    }

    protected static void Fill(byte[] buf, uint address, int value, int count)
    {
        for (int i = 0; i < count; i++)
            buf[address + i] = (byte) value;
    }
}