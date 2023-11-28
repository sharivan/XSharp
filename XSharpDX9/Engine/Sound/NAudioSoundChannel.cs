using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;

namespace XSharp.Engine.Sound;

public class NAudioSoundChannel : SoundChannel
{
    private WaveOutEvent player;

    public override float Volume
    {
        get => player.Volume;
        set => player.Volume = value;
    }

    internal NAudioSoundChannel(float volume = 1)
        : base(volume)
    {
        player = new WaveOutEvent()
        {
            Volume = volume
        };
    }

    protected override ISoundStream CreateSoundStream()
    {
        return new NAudioSoundStream();
    }

    public override void Dispose()
    {
        player.Dispose();

        base.Dispose();
    }

    protected override void Init(ISoundStream stream)
    {
        player.Init((NAudioSoundStream) Stream);
    }

    protected override void Play()
    {
        player.Play();
    }

    public override void StopPlayer()
    {
        player.Stop();
    }
}