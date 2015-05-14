using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using ShadowOperations.Shared;

namespace ShadowOperations.ClientGame.AudioSystem
{
    public class ActiveSound
    {
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

        public void Create()
        {
            Src = AL.GenSource();
            AL.Source(Src, ALSourcei.Buffer, Effect.Internal);
            AL.Source(Src, ALSourceb.Looping, Loop);
            if (Pitch != 1f)
            {
                AL.Source(Src, ALSourcef.Pitch, Pitch);
            }
            if (Gain != 1f)
            {
                AL.Source(Src, ALSourcef.Gain, Gain);
            }
            if (!Position.IsNaN())
            {
                Vector3 vec = Position.ToOVector();
                AL.Source(Src, ALSource3f.Position, ref vec);
                AL.Source(Src, ALSourceb.SourceRelative, false);
            }
            else
            {
                AL.Source(Src, ALSourceb.SourceRelative, true);
            }
        }

        public void Play()
        {
            AL.SourcePlay(Src);
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
            AL.DeleteSource(Src);
        }
    }
}
