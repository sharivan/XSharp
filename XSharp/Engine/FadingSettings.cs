using SharpDX;
using System;

namespace XSharp.Engine
{
    public class FadingSettings
    {
        public bool Fading
        {
            get;
            private set;
        }

        public bool Paused
        {
            get;
            set;
        }

        public float FadingLevel
        {
            get;
            set;
        } = 0;

        public Color FadingColor
        {
            get;
            set;
        } = Color.Black;

        public bool FadeIn
        {
            get;
            private set;
        }

        public long FadingFrames
        {
            get;
            private set;
        }

        public long FadingTick
        {
            get;
            private set;
        }

        public Action OnFadingComplete
        {
            get;
            private set;
        }

        protected internal virtual void OnFrame()
        {
            if (Fading && !Paused)
            {
                FadingTick++;
                if (FadingTick > FadingFrames)
                {
                    Fading = false;
                    OnFadingComplete?.Invoke();
                }
                else
                {
                    FadingLevel = (float) FadingTick / FadingFrames;
                    if (FadeIn)
                        FadingLevel = 1 - FadingLevel;
                }
            }
        }

        public void Reset()
        {
            Fading = false;
            Paused = false;
            FadingColor = Color.Transparent;
            FadingFrames = 0;
            FadingLevel = 0;
            FadeIn = false;
            FadingTick = 0;
            OnFadingComplete = null;
        }

        public void Start(Color color, int frames, bool fadeIn, Action onFadingComplete = null)
        {
            Fading = true;
            Paused = false;
            FadingColor = color;
            FadingFrames = frames;
            FadingLevel = fadeIn ? 1 : 0;
            FadeIn = fadeIn;
            FadingTick = 0;
            OnFadingComplete = onFadingComplete;
        }

        public void Start(Color color, int frames, Action onFadingComplete = null)
        {
            Start(color, frames, false, onFadingComplete);
        }

        public void Stop()
        {
            if (Fading)
            {
                Fading = false;
                FadingLevel = 0;
                OnFadingComplete?.Invoke();
            }
        }
    }
}