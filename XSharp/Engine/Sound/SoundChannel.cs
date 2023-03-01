﻿using NAudio.Wave;
using System;

namespace XSharp.Engine.Sound
{
    public class SoundChannel : IDisposable
    {
        internal string name;
        private WaveOutEvent player;

        public int Index
        {
            get;
            internal set;
        }

        public string Name
        {
            get => name;
            set => GameEngine.Engine.UpdateSoundChannelName(this, value);
        }

        public SoundStream Stream
        {
            get;
        }

        public bool Initialized
        {
            get;
            private set;
        }

        public float Volume
        {
            get => player.Volume;
            set => player.Volume = value;
        }

        public bool Playing => Stream.Playing;

        internal SoundChannel(float volume = 1)
        {
            player = new WaveOutEvent()
            {
                Volume = volume
            };

            Stream = new SoundStream();

            Initialized = false;
        }

        public void Dispose()
        {
            player.Dispose();
            Stream.Dispose();

            Initialized = false;
        }

        public void Play(WaveStream stream, double stopTime, double loopTime, bool ignoreUpdatesUntilPlayed = false)
        {
            stream.Position = 0;
            Stream.UpdateSource(stream, stopTime, loopTime, ignoreUpdatesUntilPlayed);

            if (!Stream.Playing)
            {
                Stream.Reset();
                Stream.Play();
            }

            if (!Initialized)
            {
                player.Init(Stream);
                Initialized = true;
            }

            player.Play();
        }

        public void Play(WaveStream stream, double loopTime, bool ignoreUpdatesUntilFinished = false)
        {
            Play(stream, -1, loopTime, ignoreUpdatesUntilFinished);
        }

        public void Play(WaveStream stream, bool ignoreUpdatesUntilFinished = false)
        {
            Play(stream, -1, -1, ignoreUpdatesUntilFinished);
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

        public void StopPlayer()
        {
            player.Stop();
        }

        public void Stop()
        {
            StopStream();
            StopPlayer();
        }

        public bool IsPlaying(WaveStream stream)
        {
            return Stream.Playing && Stream.Source == stream;
        }
    }
}