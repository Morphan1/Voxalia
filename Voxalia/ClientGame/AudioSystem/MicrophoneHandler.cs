//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ClientGame.AudioSystem.OpusWrapper;

namespace Voxalia.ClientGame.AudioSystem
{
    public class MicrophoneHandler
    {
        public SoundEngine Engine;

        public AudioCapture Capture = null; 

        public int PlaybackSrc;

        public float Volume = 1;

        public long stat_bytes = 0;

        public long stat_bytes2 = 0;

        public OpusEncoder Encoder;

        public OpusDecoder Decoder;

        const int SampleRate = 16000;

        public MicrophoneHandler(SoundEngine eng)
        {
            Engine = eng;
            Encoder = OpusEncoder.Create(SampleRate, 1, Application.Voip);
            Decoder = OpusDecoder.Create(SampleRate, 1);
        }

        public void StartEcho()
        {
            if (Capture != null)
            {
                StopEcho();
            }
            PlaybackSrc = AL.GenSource();
            Capture = new AudioCapture(AudioCapture.DefaultDevice, SampleRate, ALFormat.Mono16, 8192);
            Capture.Start();
            AL.Source(PlaybackSrc, ALSourceb.SourceRelative, true);
        }
        
        byte[] buffer = new byte[8192 * 2];

        byte[] tempbuf = new byte[8192 * 2];

        int tempasamps = 0;

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
                Array.Copy(buffer, 0, tempbuf, tempasamps * 2, asamps * 2);
                tempasamps += asamps;
                stat_bytes += asamps * 2;
                int b = 0;
                while ((tempasamps - b) >= 960)
                {
                    AddSection(b, 960);
                    b += 960;
                }
                // Are the below while loops needed?
                while ((tempasamps - b) >= 320)
                {
                    AddSection(b, 320);
                    b += 320;
                }
                while ((tempasamps - b) >= 80)
                {
                    AddSection(b, 80);
                    b += 80;
                }
                while ((tempasamps - b) >= 40)
                {
                    AddSection(b, 40);
                    b += 40;
                }
                if (tempasamps - b > 0)
                {
                    byte[] tbuf = new byte[tempbuf.Length];
                    Array.Copy(tempbuf, b, tbuf, 0, tempasamps - b);
                    tempbuf = tbuf;
                }
                tempasamps -= b;
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

        void AddSection(int start, int asamps)
        {
            byte[] tbuf = new byte[asamps * 2];
            Array.Copy(tempbuf, tbuf, tbuf.Length);
            byte[] dat = Encoder.Encode(tbuf);
            stat_bytes2 += dat.Length;
            byte[] decdat = Decoder.Decode(dat, dat.Length);
            int buf = AL.GenBuffer();
            AL.BufferData(buf, ALFormat.Mono16, decdat, decdat.Length, SampleRate);
            AL.SourceQueueBuffer(PlaybackSrc, buf);
            AL.Source(PlaybackSrc, ALSourcef.Gain, Volume);
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
