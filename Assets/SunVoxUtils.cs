using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunVoxUtils
{
    private static bool init = false;
    private static HashSet<int> openSlots = new HashSet<int>();

    private static GameObject audioOutput;

    public static int OpenUnusedSlot()
    {
        if (!init)
        {
            Debug.Log("SunVox init");
            init = true;
            // TODO: what if there are a different number of channels??
            int version = SunVox.sv_init("0", AudioSettings.outputSampleRate, 2,
                SunVox.SV_INIT_FLAG_USER_AUDIO_CALLBACK | SunVox.SV_INIT_FLAG_AUDIO_FLOAT32);
            if (version < 0)
            {
                Debug.LogError("Error initializing SunVox");
                return -1;
            }

            int major = (version >> 16) & 255;
            int minor1 = (version >> 8) & 255;
            int minor2 = (version) & 255;
            Debug.Log(System.String.Format("SunVox lib version: {0}.{1}.{2}", major, minor1, minor2));
        }
        if (audioOutput == null)
        {
            audioOutput = new GameObject("SunVox out");
            audioOutput.AddComponent<SunVoxFilter>();
        }

        int slot = 0;
        while (true)
        {
            if (!openSlots.Contains(slot))
                break;
            slot++;
        }

        Debug.Log("SunVox: open slot " + slot);
        SunVox.sv_open_slot(slot);
        openSlots.Add(slot);
        return slot;
    }

    public static void CloseSlot(int slot)
    {
        Debug.Log("SunVox: close slot " + slot);
        openSlots.Remove(slot);
        SunVox.sv_close_slot(slot);
    }
}


public class SunVoxFilter : MonoBehaviour
{
    private AudioSource audioSource;
    private int sampleRate;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        sampleRate = AudioSettings.outputSampleRate;
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        int numSamples = data.Length / channels;
        float time = (float)numSamples / sampleRate;
        int ticks = (int)(time * SunVox.sv_get_ticks_per_second());
        // I think I got this right but I'm not sure?
        // TODO check the docs again... e.g. what is "user_ticks_per_second"?
        SunVox.sv_audio_callback(data, numSamples, numSamples, SunVox.sv_get_ticks() + ticks);
    } 
}

public class SunVoxPlayer : AudioPlayer
{
    private int slot;

    public static AudioPlayer Factory(byte[] data)
    {
        return new SunVoxPlayer(data);
    }

    public SunVoxPlayer(byte[] data)
    {
        slot = SunVoxUtils.OpenUnusedSlot();
        int result = SunVox.sv_load_from_memory(slot, data, data.Length);
        if (result != 0)
        {
            Debug.LogError("Error loading file");
            return;
        }
        SunVox.sv_play_from_beginning(slot);
    }

    public void Stop()
    {
        SunVoxUtils.CloseSlot(slot);
    }
}