using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.AudioSystem
{
    public class MicrophoneHandler
    {
        public SoundEngine Engine;

        public AudioCapture Capture = null; 

        public int PlaybackSrc;

        public float Volume = 1;

        public long stat_bytes = 0;

        public MicrophoneHandler(SoundEngine eng)
        {
            Engine = eng;
        }

        public void StartEcho()
        {
            if (Capture != null)
            {
                StopEcho();
            }
            PlaybackSrc = AL.GenSource();
            Capture = new AudioCapture(AudioCapture.DefaultDevice, 22050, ALFormat.Mono16, 4096);
            Capture.Start();
        }

        short[] buffer = new short[4096];

        public void Tick()
        {
            if (Capture == null)
            {
                return;
            }
            int asamps = Capture.AvailableSamples;
            if (asamps > 0)
            {
                Capture.ReadSamples(buffer, asamps);
                stat_bytes += asamps * sizeof(short);
                int buf = AL.GenBuffer();
                AL.BufferData(buf, ALFormat.Mono16, buffer, asamps * sizeof(short), Capture.SampleFrequency);
                AL.SourceQueueBuffer(PlaybackSrc, buf);
                AL.Source(PlaybackSrc, ALSourcef.Gain, Volume);
                int bufc;
                AL.GetSource(PlaybackSrc, ALGetSourcei.BuffersProcessed, out bufc);
                if (bufc > 0)
                {
                    int[] bufs = AL.SourceUnqueueBuffers(PlaybackSrc, bufc);
                    AL.DeleteBuffers(bufs);
                }
                if (AL.GetSourceState(PlaybackSrc) != ALSourceState.Playing)
                {
                    AL.SourcePlay(PlaybackSrc);
                }
            }
        }
        
        public void StopEcho()
        {
            if (Capture == null)
            {
                return;
            }
            AL.SourceStop(PlaybackSrc);
            int bufc;
            AL.GetSource(PlaybackSrc, ALGetSourcei.BuffersQueued, out bufc);
            if (bufc > 0)
            {
                int[] bufs = AL.SourceUnqueueBuffers(PlaybackSrc, bufc);
                AL.DeleteBuffers(bufs);
            }
            AL.DeleteSource(PlaybackSrc);
            PlaybackSrc = 0;
            Capture.Stop();
            Capture.Dispose();
            Capture = null;
        }
    }
}
