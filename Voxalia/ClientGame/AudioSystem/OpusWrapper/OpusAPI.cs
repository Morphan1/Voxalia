using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Voxalia.ClientGame.AudioSystem.OpusWrapper
{
    /// <summary>
    /// Wraps the Opus API.
    /// </summary>
    public class OpusAPI_Linux
    {
        const string lib = "libopus.so.0";

        [DllImport(lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr opus_encoder_create(int Fs, int channels, int application, out IntPtr error);

        [DllImport(lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void opus_encoder_destroy(IntPtr encoder);

        [DllImport(lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int opus_encode(IntPtr st, IntPtr pcm, int frame_size, IntPtr data, int max_data_bytes);

        [DllImport(lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr opus_decoder_create(int Fs, int channels, out IntPtr error);

        [DllImport(lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void opus_decoder_destroy(IntPtr decoder);

        [DllImport(lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int opus_decode(IntPtr st, IntPtr data, int len, IntPtr pcm, int frame_size, int decode_fec);

        [DllImport(lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int opus_encoder_ctl(IntPtr st, Ctl request, int value);

        [DllImport(lib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int opus_encoder_ctl(IntPtr st, Ctl request, out int value);
    }

    /// <summary>
    /// Wraps the Opus API.
    /// </summary>
    public class OpusAPI_Windows
    {
        [DllImport("opus.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr opus_encoder_create(int Fs, int channels, int application, out IntPtr error);

        [DllImport("opus.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void opus_encoder_destroy(IntPtr encoder);

        [DllImport("opus.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int opus_encode(IntPtr st, IntPtr pcm, int frame_size, IntPtr data, int max_data_bytes);

        [DllImport("opus.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr opus_decoder_create(int Fs, int channels, out IntPtr error);

        [DllImport("opus.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void opus_decoder_destroy(IntPtr decoder);

        [DllImport("opus.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int opus_decode(IntPtr st, IntPtr data, int len, IntPtr pcm, int frame_size, int decode_fec);

        [DllImport("opus.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int opus_encoder_ctl(IntPtr st, Ctl request, int value);

        [DllImport("opus.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int opus_encoder_ctl(IntPtr st, Ctl request, out int value);
    }

    // TODO: Mac?

    public class OpusAPI
    {
        public static int Platform = 0;

        static OpusAPI()
        {
            try
            {
                IntPtr error;
                IntPtr temp = OpusAPI_Windows.opus_encoder_create(8000, 1, (int)Application.Voip, out error);
                if ((Errors)error != Errors.OK)
                {
                    throw new Exception("Exception occured while creating encoder");
                }
            }
            catch (Exception)
            {
                Platform = 1;
            }
        }
        
        public static IntPtr opus_encoder_create(int Fs, int channels, int application, out IntPtr error)
        {
            if (Platform == 1)
            {
                return OpusAPI_Linux.opus_encoder_create(Fs, channels, application, out error);
            }
            return OpusAPI_Windows.opus_encoder_create(Fs, channels, application, out error);
        }

        public static void opus_encoder_destroy(IntPtr encoder)
        {
            if (Platform == 1)
            {
                OpusAPI_Linux.opus_encoder_destroy(encoder);
            }
            OpusAPI_Windows.opus_encoder_destroy(encoder);
        }
        
        public static int opus_encode(IntPtr st, IntPtr pcm, int frame_size, IntPtr data, int max_data_bytes)
        {
            if (Platform == 1)
            {
                return OpusAPI_Linux.opus_encode(st, pcm, frame_size, data, max_data_bytes);
            }
            return OpusAPI_Windows.opus_encode(st, pcm, frame_size, data, max_data_bytes);
        }

        public static IntPtr opus_decoder_create(int Fs, int channels, out IntPtr error)
        {
            if (Platform == 1)
            {
                return OpusAPI_Linux.opus_decoder_create(Fs, channels, out error);
            }
            return OpusAPI_Windows.opus_decoder_create(Fs, channels, out error);
        }
        
        public static void opus_decoder_destroy(IntPtr decoder)
        {
            if (Platform == 1)
            {
                OpusAPI_Linux.opus_decoder_destroy(decoder);
            }
            OpusAPI_Windows.opus_decoder_destroy(decoder);
        }
        
        public static int opus_decode(IntPtr st, IntPtr data, int len, IntPtr pcm, int frame_size, int decode_fec)
        {
            if (Platform == 1)
            {
                return OpusAPI_Linux.opus_decode(st, data, len, pcm, frame_size, decode_fec);
            }
            return OpusAPI_Windows.opus_decode(st, data, len, pcm, frame_size, decode_fec);
        }
        
        public static int opus_encoder_ctl(IntPtr st, Ctl request, int value)
        {
            if (Platform == 1)
            {
                return OpusAPI_Linux.opus_encoder_ctl(st, request, value);
            }
            return OpusAPI_Windows.opus_encoder_ctl(st, request, value);
        }
        
        public static int opus_encoder_ctl(IntPtr st, Ctl request, out int value)
        {
            if (Platform == 1)
            {
                return OpusAPI_Linux.opus_encoder_ctl(st, request, out value);
            }
            return OpusAPI_Windows.opus_encoder_ctl(st, request, out value);
        }
    }

    public enum Ctl : int
    {
        SetBitrateRequest = 4002,
        GetBitrateRequest = 4003,
        SetInbandFECRequest = 4012,
        GetInbandFECRequest = 4013
    }

    /// <summary>
    /// Supported coding modes.
    /// </summary>
    public enum Application
    {
        /// <summary>
        /// Best for most VoIP/videoconference applications where listening quality and intelligibility matter most.
        /// </summary>
        Voip = 2048,
        /// <summary>
        /// Best for broadcast/high-fidelity application where the decoded audio should be as close as possible to input.
        /// </summary>
        Audio = 2049,
        /// <summary>
        /// Only use when lowest-achievable latency is what matters most. Voice-optimized modes cannot be used.
        /// </summary>
        Restricted_LowLatency = 2051
    }

    public enum Errors
    {
        /// <summary>
        /// No error.
        /// </summary>
        OK = 0,
        /// <summary>
        /// One or more invalid/out of range arguments.
        /// </summary>
        BadArg = -1,
        /// <summary>
        /// The mode struct passed is invalid.
        /// </summary>
        BufferToSmall = -2,
        /// <summary>
        /// An internal error was detected.
        /// </summary>
        InternalError = -3,
        /// <summary>
        /// The compressed data passed is corrupted.
        /// </summary>
        InvalidPacket = -4,
        /// <summary>
        /// Invalid/unsupported request number.
        /// </summary>
        Unimplemented = -5,
        /// <summary>
        /// An encoder or decoder structure is invalid or already freed.
        /// </summary>
        InvalidState = -6,
        /// <summary>
        /// Memory allocation has failed.
        /// </summary>
        AllocFail = -7
    }
}
