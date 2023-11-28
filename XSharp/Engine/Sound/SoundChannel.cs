using System;

using XSharp.Serialization;

namespace XSharp.Engine.Sound;

public abstract class SoundChannel : IDisposable, ISerializable
{
    internal string name;

    public int Index
    {
        get;
        internal set;
    }

    public string Name
    {
        get => name;
        set => BaseEngine.Engine.UpdateSoundChannelName(this, value);
    }

    public ISoundStream Stream
    {
        get;
    }

    public bool Initialized
    {
        get;
        private set;
    }

    public abstract float Volume
    {
        get;
        set;
    }

    public float SavedVolume
    {
        get;
        set;
    } = -1;

    public bool Playing => Stream.Playing;

    protected SoundChannel(float volume = 1)
    {
        Stream = CreateSoundStream();

        Initialized = false;
    }

    protected abstract ISoundStream CreateSoundStream();

    public virtual void Dispose()
    {      
        Stream.Dispose();

        Initialized = false;

        GC.SuppressFinalize(this);
    }

    protected abstract void Init(ISoundStream stream);

    protected abstract void Play();

    public void Play(PrecachedSound sound, double stopTime, double loopTime, bool ignoreUpdatesUntilPlayed = false)
    {
        sound.Stream.Position = 0;
        Stream.UpdateSource(sound, stopTime, loopTime, ignoreUpdatesUntilPlayed);

        if (!Stream.Playing)
        {
            Stream.Reset();
            Stream.Play();
        }

        if (!Initialized)
        {
            Init(Stream);
            Initialized = true;
        }

        Play();
    }

    public void Play(PrecachedSound sound, double loopTime, bool ignoreUpdatesUntilFinished = false)
    {
        Play(sound, -1, loopTime, ignoreUpdatesUntilFinished);
    }

    public void Play(PrecachedSound sound, bool ignoreUpdatesUntilFinished = false)
    {
        Play(sound, -1, -1, ignoreUpdatesUntilFinished);
    }

    public void ClearSoundLoopPoint(bool clearStopPoint = false)
    {
        Stream.LoopPoint = -1;
        if (clearStopPoint)
            Stream.StopPoint = -1;
    }

    public void ClearSoundStopPoint()
    {
        Stream.StopPoint = -1;
    }

    public void StopStream()
    {
        Stream.Stop();
    }

    public abstract void StopPlayer();

    public void Stop()
    {
        StopStream();
        StopPlayer();
    }

    public bool IsPlaying(PrecachedSound sound)
    {
        return Stream.Playing && Stream.Source == sound;
    }

    public void Deserialize(ISerializer serializer)
    {
        Stream.Deserialize(serializer);
        Volume = serializer.ReadFloat();
        SavedVolume = serializer.ReadFloat();

        if (Stream.Playing)
        {
            if (!Initialized)
            {
                Init(Stream);
                Initialized = true;
            }

            Play();
        }
    }

    public void Serialize(ISerializer serializer)
    {
        Stream.Serialize(serializer);
        serializer.WriteFloat(Volume);
        serializer.WriteFloat(SavedVolume);
    }

    public void SaveVolume()
    {
        SavedVolume = Volume;
    }

    public void RestoreVolume()
    {
        if (SavedVolume >= 0)
        {
            Volume = SavedVolume;
            SavedVolume = -1;
        }
    }
}