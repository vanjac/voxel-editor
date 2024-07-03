using System;
using System.Runtime.InteropServices;

public class Opus {
#if UNITY_IOS && !UNITY_EDITOR
    private const string LIBRARY_NAME = "__Internal";
#elif (UNITY_ANDROID && !UNITY_EDITOR) || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
    private const string LIBRARY_NAME = "libopus";
#else
    private const string LIBRARY_NAME = "opus";
#endif

    [DllImport(LIBRARY_NAME)]
    public static extern IntPtr opus_encoder_create(int Fs, int channels, int application, out int error);

    [DllImport(LIBRARY_NAME)]
    public static extern void opus_encoder_destroy(IntPtr encoder);

    [DllImport(LIBRARY_NAME)]
    public static extern int opus_encode_float(IntPtr st, float[] pcm, int frame_size, byte[] data, int max_data_bytes);

    [DllImport(LIBRARY_NAME)]
    public static extern IntPtr opus_decoder_create(int Fs, int channels, out int error);

    [DllImport(LIBRARY_NAME)]
    public static extern void opus_decoder_destroy(IntPtr decoder);

    [DllImport(LIBRARY_NAME)]
    public static extern int opus_decode_float(IntPtr st, byte[] data, int len, float[] pcm, int frame_size, int decode_fec);

#if UNITY_IOS && !UNITY_EDITOR
    // varargs don't work on iOS, so use a helper function added to libopus
    [DllImport(LIBRARY_NAME)]
    public static extern int opus_encoder_ctl2(IntPtr st, Ctl request, out int result);

    public static int opus_encoder_ctl(IntPtr st, Ctl request, out int result) {
        return opus_encoder_ctl2(st, request, out result);
    }
#else
    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern int opus_encoder_ctl(IntPtr st, Ctl request, out int result);
#endif

    public enum Ctl : int {
        GetApplicationRequest = 4001,
        SetBitrateRequest = 4002,
        GetBitrateRequest = 4003,
        SetComplexityRequest = 4010,
        GetComplexityRequest = 4011
    }

    public enum Bitrate : int {
        Auto = -1000,
        Max = -1
    }

    /// <summary>
    /// Supported coding modes.
    /// </summary>
    public enum Application {
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
        Restricted_LowDelay = 2051
    }

    public enum Errors {
        /// <summary>
        /// No error.
        /// </summary>
        OK = 0,
        /// <summary>
        /// One or more invalid/out of range arguments.
        /// </summary>
        BadArg = -1,
        /// <summary>
        /// Not enough bytes allocated in the buffer.
        /// </summary>
        BufferTooSmall = -2,
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
