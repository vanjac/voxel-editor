using UnityEngine;

public static class AudioCompression
{
    public static byte[] Compress(AudioClip clip)
    {
        float[] samples = new float[clip.samples * clip.channels];
        clip.LoadAudioData();
        clip.GetData(samples, 0);

        byte[] bytes = new byte[samples.Length * 4 + 4];
        bytes[0] = (byte)clip.channels;
        bytes[1] = (byte)(clip.frequency >> 16);
        bytes[2] = (byte)((clip.frequency >> 8) & 0xff);
        bytes[3] = (byte)(clip.frequency & 0xff);
        System.Buffer.BlockCopy(samples, 0, bytes, 4, bytes.Length - 4);
        return bytes;
    }

    public static AudioClip Decompress(byte[] bytes)
    {
        float[] samples = new float[bytes.Length / 4 - 1];
        System.Buffer.BlockCopy(bytes, 4, samples, 0, bytes.Length - 4);
        int channels = bytes[0];
        int frequency = (bytes[1] << 16) | (bytes[2] << 8) | bytes[3];
        Debug.Log(channels + " channels, " + frequency + "Hz");
        AudioClip clip = AudioClip.Create("audio", samples.Length / channels, channels, frequency, false);
        clip.SetData(samples, 0);
        return clip;
    }
}