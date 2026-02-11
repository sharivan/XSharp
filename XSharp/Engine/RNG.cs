using XSharp.Serialization;

namespace XSharp.Engine;

public class RNG : ISerializable
{
    private ushort seed;

    internal RNG(ushort seed = 0)
    {
        UpdateSeed(seed);
    }

    public void Deserialize(ISerializer reader)
    {
        seed = reader.ReadUShort();
    }

    public void Serialize(ISerializer writer)
    {
        writer.WriteUShort(seed);
    }

    public void UpdateSeed(ushort seed)
    {
        this.seed = seed;
    }

    public ushort NextValue()
    {
        // This algoritm is the same used by SNES MMX games
        byte next_rng_H = (byte) (((3 * seed) >> 8) & 0xff);
        byte next_rng_L = (byte) ((next_rng_H + seed) & 0xff);
        seed = (ushort) ((next_rng_H << 1) + next_rng_L);
        return seed;
    }

    public ushort NextValue(ushort count)
    {
        return NextValue(0, count);
    }

    public ushort NextValue(ushort start, ushort end)
    {
        return (ushort) (start + NextValue() % (end - start)); // This one will change the distribution by using modulus, but original game does the same thing.
    }

    public uint NextInt()
    {
        ushort hi = NextValue();
        ushort lo = NextValue();
        return (uint) (hi << 16) | lo;
    }

    public uint NextInt(uint count)
    {
        return NextInt(0, count);
    }

    public uint NextInt(uint start, uint end)
    {
        return start + NextInt() % (end - start);
    }

    public ulong NextLong()
    {
        uint hi = NextInt();
        uint lo = NextInt();
        return (ulong) (hi << 32) | lo;
    }

    public ulong NextLong(ulong count)
    {
        return NextLong(0, count);
    }

    public ulong NextLong(ulong start, ulong end)
    {
        return start + NextLong() % (end - start);
    }

    public float NextFloat()
    {
        return (float) NextValue() / ushort.MaxValue;
    }
}