using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using Voxalia.Shared;
using OggDecoder;
using Voxalia.ClientGame.CommandSystem;
using Voxalia.Shared.Files;

namespace Voxalia.ClientGame.AudioSystem
{
    public class SoundEngine
    {
        public SoundEffect Noise;

        public AudioContext Context;

        public ClientCVar CVars;
        
        public void Init(ClientCVar cvar)
        {
            CVars = cvar;
            Context = new AudioContext();
            Context.MakeCurrent();
            Effects = new Dictionary<string, SoundEffect>();
            PlayingNow = new List<ActiveSound>();
            Noise = LoadSound(new DataStream(Convert.FromBase64String(NoiseDefault.NoiseB64)), "noise");
        }

        public void Update(Location position, Location forward, Location up, Location velocity, bool selected)
        {
            for (int i = 0; i < PlayingNow.Count; i++)
            {
                if (!PlayingNow[i].Exists || AL.GetSourceState(PlayingNow[i].Src) == ALSourceState.Stopped)
                {
                    PlayingNow[i].Destroy();
                    PlayingNow.RemoveAt(i);
                    i--;
                }
                else if (!selected && PlayingNow[i].IsBackground && !PlayingNow[i].Backgrounded)
                {
                    AL.Source(PlayingNow[i].Src, ALSourcef.Gain, 0.0001f);
                    PlayingNow[i].Backgrounded = true;
                }
                else if (selected && PlayingNow[i].Backgrounded)
                {
                    AL.Source(PlayingNow[i].Src, ALSourcef.Gain, PlayingNow[i].Gain);
                    PlayingNow[i].Backgrounded = false;
                }
            }
            Vector3 pos = position.ToOVector();
            Vector3 forw = forward.ToOVector();
            Vector3 upvec = up.ToOVector();
            Vector3 vel = velocity.ToOVector();
            AL.Listener(ALListener3f.Position, ref pos);
            AL.Listener(ALListenerfv.Orientation, ref forw, ref upvec);
            AL.Listener(ALListener3f.Velocity, ref vel);
            AL.Listener(ALListenerf.Gain, CVars.a_globalvolume.ValueF);
        }

        public Dictionary<string, SoundEffect> Effects;

        public List<ActiveSound> PlayingNow;

        public ActiveSound Play(SoundEffect sfx, bool loop, Location pos, float pitch = 1, float volume = 1, float seconds = 0)
        {
            if (pitch <= 0 || pitch > 2)
            {
                throw new ArgumentException("Must be between 0 and 2", "pitch");
            }
            if (volume == 0)
            {
                return null;
            }
            if (volume <= 0 || volume > 1)
            {
                throw new ArgumentException("Must be between 0 and 1", "volume");
            }
            ActiveSound actsfx = new ActiveSound(sfx);
            actsfx.Position = pos;
            actsfx.Pitch = pitch * CVars.a_globalpitch.ValueF;
            actsfx.Gain = volume;
            actsfx.Loop = loop;
            actsfx.Create();
            actsfx.Play();
            if (seconds != 0)
            {
                actsfx.Seek(seconds);
            }
            PlayingNow.Add(actsfx);
            return actsfx;
        }

        public SoundEffect GetSound(string name)
        {
            string namelow = name.ToLower();
            SoundEffect sfx;
            if (Effects.TryGetValue(namelow, out sfx))
            {
                return sfx;
            }
            sfx = LoadSound(namelow);
            if (sfx != null)
            {
                return sfx;
            }
            sfx = new SoundEffect();
            sfx.Name = namelow;
            sfx.Internal = Noise.Internal;
            return sfx;
        }

        ALFormat GetSoundFormat(int channels, int bits)
        {
            switch (channels)
            {
                case 1:
                    return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
                default: // 2
                    return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
            }
        }

        public SoundEffect LoadSound(string name)
        {
            try
            {
                string newname = "sounds/" + name + ".ogg";
                if (!Program.Files.Exists(newname))
                {
                    return null;
                }
                return LoadVorbisSound(Program.Files.ReadToStream(newname), name);
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Reading sound file '" + name + "': " + ex.ToString());
                return null;
            }
        }

        public SoundEffect LoadVorbisSound(DataStream stream, string name)
        {
            OggDecodeStream oggds = new OggDecodeStream(stream);
            return LoadSound(new DataStream(oggds.decodedStream.ToArray()), name);
        }

        public SoundEffect LoadSound(DataStream stream, string name)
        {
            SoundEffect sfx = new SoundEffect();
            sfx.Name = name;
            int channels;
            int bits;
            int rate;
            byte[] data = LoadWAVE(stream, out channels, out bits, out rate);
            sfx.Internal = AL.GenBuffer();
            AL.BufferData(sfx.Internal, GetSoundFormat(channels, bits), data, data.Length, rate);
            return sfx;
        }

        public byte[] LoadWAVE(DataStream stream, out int channels, out int bits, out int rate)
        {
            DataReader dr = new DataReader(stream);
            string signature = new string(dr.ReadChars(4));
            if (signature != "RIFF")
            {
                throw new NotSupportedException("Not a RIFF .wav file: " + signature);
            }
            int riff_chunk_size = dr.ReadInt32();
            string format = new string(dr.ReadChars(4));
            if (format != "WAVE")
            {
                throw new NotSupportedException("Not a WAVE .wav file: " + format);
            }
            string format_signature = new string(dr.ReadChars(4));
            if (format_signature != "fmt ")
            {
                throw new NotSupportedException("Not a 'fmt ' .wav file: " + format_signature);
            }
            int format_chunk_size = dr.ReadInt32();
            int audio_format = dr.ReadInt16();
            int num_channels = dr.ReadInt16();
            if (num_channels != 1 && num_channels != 2)
            {
                throw new NotSupportedException("Invalid number of channels: " + num_channels);
            }
            int sample_rate = dr.ReadInt32();
            int byte_rate = dr.ReadInt32();
            int block_align = dr.ReadInt16();
            int bits_per_sample = dr.ReadInt16();
            string data_signature = new string(dr.ReadChars(4));
            if (data_signature != "data")
            {
                throw new NotSupportedException("Not a DATA .wav file: " + data_signature);
            }
            int data_chunk_size = dr.ReadInt32();
            channels = num_channels;
            bits = bits_per_sample;
            rate = sample_rate;
            return dr.ReadBytes((int)dr.BaseStream.Length);
        }
    }
}
