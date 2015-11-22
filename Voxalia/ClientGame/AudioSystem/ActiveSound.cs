using OpenTK;
using OpenTK.Audio.OpenAL;
using Voxalia.Shared;
using Voxalia.ClientGame.OtherSystems;

namespace Voxalia.ClientGame.AudioSystem
{
    public class ActiveSound
    {
        public SoundEngine Engine;

        public SoundEffect Effect;

        public Location Position;

        public ActiveSound(SoundEffect sfx)
        {
            Effect = sfx;
        }

        public bool Loop = false;

        public float Pitch = 1f;

        public float Gain = 1f;

        public int Src;

        public bool Exists = false;

        public bool IsBackground = false;

        public bool Backgrounded = false;

        public void Create()
        {
            if (!Exists)
            {
                Src = AL.GenSource();
                AL.Source(Src, ALSourcei.Buffer, Effect.Internal);
                AL.Source(Src, ALSourceb.Looping, Loop);
                if (Pitch != 1f)
                {
                    UpdatePitch();
                }
                if (Gain != 1f)
                {
                    UpdateGain();
                }
                if (!Position.IsNaN())
                {
                    Vector3 zero = Vector3.Zero;
                    Vector3 vec = ClientUtilities.Convert(Position);
                    AL.Source(Src, ALSource3f.Direction, ref zero);
                    AL.Source(Src, ALSource3f.Velocity, ref zero);
                    AL.Source(Src, ALSource3f.Position, ref vec);
                    AL.Source(Src, ALSourceb.SourceRelative, false);
                    AL.Source(Src, ALSourcef.EfxAirAbsorptionFactor, 1f);
                }
                else
                {
                    AL.Source(Src, ALSourceb.SourceRelative, true);
                }
                Exists = true;
            }
        }

        public void UpdatePitch()
        {
            AL.Source(Src, ALSourcef.Pitch, Pitch);
        }

        public void UpdateGain()
        {
            bool sel = Engine.Selected;
            if (sel)
            {
                AL.Source(Src, ALSourcef.Gain, Gain);
                Backgrounded = false;
            }
            else
            {
                AL.Source(Src, ALSourcef.Gain, 0.0001f);
                Backgrounded = true;
            }
        }

        public void Play()
        {
            AL.SourcePlay(Src);
        }

        public void Seek(float f)
        {
            AL.Source(Src, ALSourcef.SecOffset, f);
        }

        public void Pause()
        {
            AL.SourcePause(Src);
        }

        public void Stop()
        {
            AL.SourceStop(Src);
        }

        public void Destroy()
        {
            if (Exists)
            {
                AL.DeleteSource(Src);
                Exists = false;
            }
        }
    }
}
