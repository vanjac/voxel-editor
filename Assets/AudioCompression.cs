using System;
using UnityEngine;

public static class AudioCompression
{
    public static byte[] Compress(AudioClip clip)
    {
        clip.LoadAudioData();

        IntPtr error;
        IntPtr encoder = Opus.opus_encoder_create(48000, clip.channels, (int)Opus.Application.Audio, out error);
        if ((Opus.Errors)error != Opus.Errors.OK)
            throw new Exception("Error creating encoder " + (Opus.Errors)error);

        // seems to default to 100 kbit/s for 48000Hz 2 channels

        int frameSize = 960;
        int blockSize = frameSize * clip.channels;
        float[] sampleBlock = new float[blockSize];
        byte[] packet = new byte[1024];
        byte[] bytes = new byte[(int)(clip.length * 50000)];

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
        int byteI = 10;

        int largestPacket = 0;
        for (int i = 0; i < samples; i += frameSize) {
            clip.GetData(sampleBlock, i);
            int packetSize = Opus.opus_encode_float(encoder, sampleBlock, frameSize, packet, packet.Length);
            if (packetSize < 0)
                throw new Exception("Encoding failed " + (Opus.Errors)packetSize);
            if (packetSize > largestPacket)
                largestPacket = packetSize;
            bytes[byteI] = (byte)(packetSize >> 8);
            bytes[byteI+1] = (byte)(packetSize & 0xff);
            System.Buffer.BlockCopy(packet, 0, bytes, byteI + 2, packetSize);
            byteI += packetSize + 2;
        }
        // rest of header
        bytes[8] = (byte)(largestPacket >> 8);
        bytes[9] = (byte)(largestPacket & 0xff);
        Debug.Log("Final length " + byteI);
        Array.Resize(ref bytes, byteI);
        return bytes;
    }

    public static AudioClip Decompress(byte[] bytes)
    {
        // header
        int channels = bytes[0];
        int frequency = (bytes[1] << 16) | (bytes[2] << 8) | bytes[3];
        int samples = (bytes[4] << 24) | (bytes[5] << 16) | (bytes[6] << 8) | bytes[7];
        int largestPacket = (bytes[8] << 8) | bytes[9];

        Debug.Log(frequency + "Hz " + channels + " channels, " + samples + " samples");
        Debug.Log("Largest packet: " + largestPacket);

        AudioClip clip = AudioClip.Create("audio", samples, channels, frequency, false);

        IntPtr error;
        IntPtr decoder = Opus.opus_decoder_create(48000, channels, out error);
        if ((Opus.Errors)error != Opus.Errors.OK)
            throw new Exception("Error creating decoder " + (Opus.Errors)error);

        int frameSize = 960;
        int blockSize = frameSize * channels;
        float[] sampleBlock = new float[blockSize];
        byte[] packet = new byte[largestPacket];
        int byteI = 10;
        int sampleI = 0;
        while (byteI < bytes.Length)
        {
            int packetSize = bytes[byteI] * 256 + bytes[byteI+1];
            System.Buffer.BlockCopy(bytes, byteI + 2, packet, 0, packetSize);
            byteI += packetSize + 2;
            int numSamples = Opus.opus_decode_float(decoder, packet, packetSize, sampleBlock, frameSize, 0);
            if (numSamples < 0)
                throw new Exception("Decoding failed " + (Opus.Errors)numSamples);
            clip.SetData(sampleBlock, sampleI);
            sampleI += numSamples;
        }
        return clip;
    }
}