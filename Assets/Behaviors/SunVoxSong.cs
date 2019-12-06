using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class SunVoxSongBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Song", "Play a song created with SunVox",
        "•  One-shot mode plays the entire sound to completion, even after the sensor turns off.\n"
        + "•  In Background mode the song is always playing, but muted when the sensor is off.\n\n"
#if UNITY_ANDROID
        + "If you have SunVox installed, songs can be found in "
        + Regex.Replace(Application.persistentDataPath.Replace(Application.identifier, "nightradio.sunvox"),
            @"^/storage/emulated/\d*", "") + "\n\n"
#endif
        + "Watch the video tutorial for more information.",
        "sunvox", typeof(SunVoxSongBehavior));

    private EmbeddedData songData = new EmbeddedData();
    private float volume = 31.0f, fadeIn = 0, fadeOut = 0;
    private PlayMode playMode = PlayMode.LOOP;


    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("dat", "Song",
                () => songData,
                v => songData = (EmbeddedData)v,
                PropertyGUIs.EmbeddedData(EmbeddedDataType.SunVox, SunVoxPlayer.Factory)),
            new Property("pmo", "Play mode",
                () => playMode,
                v => playMode = (PlayMode)v,
                PropertyGUIs.Enum),
            new Property("vol", "Volume",
                () => volume,
                v => volume = (float)v,
                PropertyGUIs.Float),
            new Property("fin", "Fade in",
                () => fadeIn,
                v => fadeIn = (float)v,
                PropertyGUIs.Float),
            new Property("fou", "Fade out",
                () => fadeOut,
                v => fadeOut = (float)v,
                PropertyGUIs.Float)
        });
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        var component = gameObject.AddComponent<SunVoxSongComponent>();
        component.songData = songData;
        component.playMode = playMode;
        component.volume = volume / 100.0f;
        component.fadeIn = fadeIn;
        component.fadeOut = fadeOut;
        component.Init();
        return component;
    }
}


public class SunVoxSongComponent : BehaviorComponent
{
    public EmbeddedData songData;
    public float volume, fadeIn, fadeOut;
    public PlayMode playMode;

    private int slot = -1;
    private float currentVolume = 0;
    private bool fadingIn, fadingOut;

    public void Init()
    {
        if (songData.bytes.Length == 0)
            return;
        slot = SunVoxUtils.OpenUnusedSlot();
        if (slot < 0)
            return;
        int result = SunVox.sv_load_from_memory(slot, songData.bytes, songData.bytes.Length);
        if (result != 0)
            throw new MapReadException("Error reading SunVox file");
        //Debug.Log(System.Runtime.InteropServices.Marshal.PtrToStringAuto(SunVox.sv_get_song_name(0)));

        currentVolume = 0;
        UpdateSunVoxVolume();

        if (playMode == PlayMode.ONCE || playMode == PlayMode._1SHOT)
            SunVox.sv_set_autostop(slot, 1);
        else
            SunVox.sv_set_autostop(slot, 0);

        StartCoroutine(VolumeUpdateCoroutine()); // runs even while disabled
    }

    public void OnDestroy()
    {
        if (slot >= 0)
            SunVoxUtils.CloseSlot(slot);
    }

    public override void BehaviorEnabled()
    {
        if (slot < 0)
            return;
        if (playMode != PlayMode.BKGND)
        {
            if (fadeIn == 0)
                currentVolume = volume;
            else
                currentVolume = 0;
        }
        fadingIn = true;
        fadingOut = false;
        UpdateSunVoxVolume();
        if (playMode != PlayMode.BKGND)
            SunVox.sv_play_from_beginning(slot);
    }

    public override void BehaviorDisabled()
    {
        if (playMode != PlayMode._1SHOT)
        {
            fadingOut = true;
            fadingIn = false;
        }
    }

    private IEnumerator VolumeUpdateCoroutine()
    {
        yield return null; // wait a frame to allow world to finish loading

        if (playMode == PlayMode.BKGND)
            SunVox.sv_play_from_beginning(slot);

        while (true)
        {
            if (fadingIn)
            {
                if (fadeIn == 0)
                    currentVolume = volume;
                else
                    currentVolume += volume / fadeIn * Time.unscaledDeltaTime;
                if (currentVolume >= volume)
                {
                    currentVolume = volume;
                    fadingIn = false;
                }
                UpdateSunVoxVolume();
            }
            else if (fadingOut)
            {
                if (fadeOut == 0)
                    currentVolume = 0;
                else
                    currentVolume -= volume / fadeOut * Time.unscaledDeltaTime;
                if (currentVolume <= 0)
                {
                    currentVolume = 0;
                    fadingOut = false;
                    if (playMode != PlayMode.BKGND)
                        SunVox.sv_stop(slot);
                }
                UpdateSunVoxVolume();
            }

            yield return null;
        }
    }

    private void UpdateSunVoxVolume()
    {
        SunVox.sv_volume(slot, (int)(currentVolume * 256.0f));
    }
}