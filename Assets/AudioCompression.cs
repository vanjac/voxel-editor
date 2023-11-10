using System;
using System.Collections;
using UnityEngine;

public static class AudioCompression
{
    private const int HEADER_SIZE = 10;
    private const int FRAMES_PER_SECOND = 50; // 20ms, recommended by Opus docs

    private static int GetClosestOpusSampleRate(int sampleRate)
    {
        if (sampleRate >= 36000)
            return 48000;
        else if (sampleRate >= 20000)
            return 24000;
        else if (sampleRate >= 14000)
            return 16000;
        else if (sampleRate >= 10000)
            return 12000;
        else
            return 8000;
    }

    public static byte[] Compress(AudioClip clip)
    {
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        clip.LoadAudioData();

        int opusSampleRate = GetClosestOpusSampleRate(clip.frequency);
        Debug.Log("Audio is " + clip.frequency + "Hz, storing at " + opusSampleRate + "Hz");

        int error;
        IntPtr encoder = Opus.opus_encoder_create(opusSampleRate, clip.channels,
            (int)Opus.Application.Audio, out error);
        if ((Opus.Errors)error != Opus.Errors.OK)
            throw new MapReadException("Error creating Opus encoder: " + (Opus.Errors)error);

        int bitrate;
        error = Opus.opus_encoder_ctl(encoder, Opus.Ctl.GetBitrateRequest, out bitrate);
        if (error < 0)
            throw new MapReadException("Error getting bitrate " + error);
        if (bitrate <= 0)
        {
            // error finding bitrate!
            // this is basically the same algorithm Opus uses
            // TODO this is still broken
            Debug.Log("Opus didn't choose a bitrate!");
            bitrate = 60 * FRAMES_PER_SECOND + opusSampleRate * clip.channels;
        }

        int frameSize = opusSampleRate / FRAMES_PER_SECOND;
        int blockSize = frameSize * clip.channels;
        float[] sampleBlock = new float[blockSize];
        int maxPacketSize = 4 * bitrate / FRAMES_PER_SECOND / 8; // multiply by 4 to be safe
        byte[] packet = new byte[maxPacketSize];
        int maxSize = 2 * clip.samples / opusSampleRate * bitrate / 8; // multiply by 2 to be safe
        byte[] bytes = new byte[maxSize + HEADER_SIZE];

        Debug.Log("Bitrate: " + bitrate + " Frame size: " + frameSize);
        Debug.Log("Max packet: " + maxPacketSize + " Max size: " + maxSize);

        // header
        bytes[0] = (byte)clip.channels;
        int frequency = clip.frequency;
        bytes[1] = (byte)(frequency >> 16);
        bytes[2] = (byte)((frequency >> 8) & 0xff);
        bytes[3] = (byte)(frequency & 0xff);
        int samples = clip.samples;
        bytes[4] = (byte)(samples >> 24);
        bytes[5] = (byte)((samples >> 16) & 0xff);
        bytes[6] = (byte)((samples >> 8) & 0xff);
        bytes[7] = (byte)(samples & 0xff);
        // save bytes 8-9 for later
        int byteI = HEADER_SIZE;

        int largestPacket = 0;
        for (int i = 0; i < samples; i += frameSize)
        {
            clip.GetData(sampleBlock, i);
            int packetSize = Opus.opus_encode_float(encoder, sampleBlock, frameSize, packet, maxPacketSize);
            if (packetSize < 0)
                throw new MapReadException("Opus encoding failed: " + (Opus.Errors)packetSize);
            if (packetSize > largestPacket)
                largestPacket = packetSize;
            bytes[byteI] = (byte)(packetSize >> 8);
            bytes[byteI + 1] = (byte)(packetSize & 0xff);
            Buffer.BlockCopy(packet, 0, bytes, byteI + 2, packetSize);
            byteI += packetSize + 2;
        }
        Opus.opus_encoder_destroy(encoder);
        // rest of header
        bytes[8] = (byte)(largestPacket >> 8);
        bytes[9] = (byte)(largestPacket & 0xff);
        Debug.Log("Compressed size: " + byteI);
        Array.Resize(ref bytes, byteI);

        stopwatch.Stop();
        Debug.Log("Encoding took " + stopwatch.ElapsedMilliseconds);
        return bytes;
    }

    public static AudioClip Decompress(byte[] bytes, MonoBehaviour coroutineObject)
    {
        if (bytes.Length < HEADER_SIZE)
            return null;

        // header
        int channels = bytes[0];
        int frequency = (bytes[1] << 16) | (bytes[2] << 8) | bytes[3];
        int samples = (bytes[4] << 24) | (bytes[5] << 16) | (bytes[6] << 8) | bytes[7];

        //Debug.Log(frequency + "Hz " + channels + " channels, " + samples + " samples");

        if (samples == 0 || channels == 0 || frequency == 0)
            return null;

        AudioClip clip = AudioClip.Create("audio", samples, channels, frequency, false);
        coroutineObject.StartCoroutine(DecompressCoroutine(clip, bytes));
        return clip;
    }

    private static IEnumerator DecompressCoroutine(AudioClip clip, byte[] bytes)
    {
        int largestPacket = (bytes[8] << 8) | bytes[9];
        //Debug.Log("Largest packet: " + largestPacket);

        int opusSampleRate = GetClosestOpusSampleRate(clip.frequency);
        int error;
        IntPtr decoder = Opus.opus_decoder_create(opusSampleRate, clip.channels, out error);
        if ((Opus.Errors)error != Opus.Errors.OK)
            throw new Exception("Error creating decoder " + (Opus.Errors)error);

        int frameSize = opusSampleRate / FRAMES_PER_SECOND; // 20ms
        int blockSize = frameSize * clip.channels;
        float[] sampleBlock = new float[blockSize];
        byte[] packet = new byte[largestPacket];
        int byteI = HEADER_SIZE;
        int sampleI = 0;
        float timeDecompressed = 0;
        while (byteI < bytes.Length)
        {
            int packetSize = bytes[byteI] * 256 + bytes[byteI + 1];
            Buffer.BlockCopy(bytes, byteI + 2, packet, 0, packetSize);
            byteI += packetSize + 2;
            int numSamples = Opus.opus_decode_float(decoder, packet, packetSize, sampleBlock, frameSize, 0);
            if (numSamples < 0)
                throw new Exception("Decoding failed " + (Opus.Errors)numSamples);
            clip.SetData(sampleBlock, sampleI);
            sampleI += numSamples;
            timeDecompressed += numSamples / (float)clip.frequency;
            if (timeDecompressed >= Time.deltaTime * 2)
            {
                // decompress twice as fast
                yield return null;
                timeDecompressed = 0;
            }
        }
        Opus.opus_decoder_destroy(decoder);
        //Debug.Log("Completely decompressed!");
    }
}