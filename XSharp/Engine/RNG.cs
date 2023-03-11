using XSharp.Serialization;

namespace XSharp.Engine;

public class RNG : ISerializable
{
    private long seed;

    internal RNG(long seed = 0)
    {
        UpdateSeed(seed);
    }

    public void Deserialize(BinarySerializer reader)
    {
        seed = reader.ReadLong();
    }

    public void Serialize(BinarySerializer writer)
    {
        writer.WriteLong(seed);
    }

    public void UpdateSeed(long seed)
    {
        this.seed = seed & long.MaxValue; // It ensures that seed will be always positive.
    }

    public int NextInt()
    {
        return (int) (NextLong() & int.MaxValue);
    }

    public int NextInt(int start, int end)
    {
        return start + NextInt() % System.Math.Abs(end - start); // This one will change the distribution by using modulus, but original game does the same thing.
    }

    public int NextInt(int count)
    {
        return NextInt(0, count);
    }

    public long NextLong()
    {
        // For now im using an algorithm similar to used by MMIX to generate random numbers.
        seed *= 6364136223846793005L;
        seed += 1442695040888963407L;
        seed &= long.MaxValue;
        return seed;
    }

    public long NextLong(long start, long end)
    {
        return start + NextLong() % System.Math.Abs(end - start);
    }

    public long NextLong(long count)
    {
        return NextLong(0, count);
    }

    public double NextDouble()
    {
        return (double) NextLong() / long.MaxValue;
    }
}