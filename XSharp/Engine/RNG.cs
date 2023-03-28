using XSharp.Serialization;

namespace XSharp.Engine;

public class RNG : ISerializable
{
    private ulong seed;

    internal RNG(ulong seed = 0)
    {
        UpdateSeed(seed);
    }

    public void Deserialize(ISerializer reader)
    {
        seed = reader.ReadULong();
    }

    public void Serialize(ISerializer writer)
    {
        writer.WriteULong(seed);
    }

    public void UpdateSeed(ulong seed)
    {
        this.seed = seed & ulong.MaxValue; // It ensures that seed will be always positive.
    }

    public uint NextUInt()
    {
        return (uint) NextULong();
    }

    public uint NextUInt(uint start, uint end)
    {
        return start + NextUInt() % (end - start); // This one will change the distribution by using modulus, but original game does the same thing.
    }

    public uint NextUInt(uint count)
    {
        return NextUInt(0, count);
    }

    public ulong NextULong()
    {
        // For now im using an algorithm similar to used by MMIX to generate random numbers.
        seed *= 6364136223846793005L;
        seed += 1442695040888963407L;
        seed >>= 24;
        return seed;
    }

    public ulong NextULong(ulong start, ulong end)
    {
        return start + NextULong() % (end - start);
    }

    public ulong NextLong(ulong count)
    {
        return NextULong(0, count);
    }

    public double NextDouble()
    {
        return (double) NextULong() / ulong.MaxValue;
    }
}