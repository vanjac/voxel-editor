using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunVoxSongBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Song", "Play a song created with SunVox",
        "sunvox", typeof(SunVoxSongBehavior));

    public enum PlayMode
    {
        ONCE, LOOP, BACKGROUND
    }

    private EmbeddedData songData = new EmbeddedData();
    private float volume = 25.0f, fadeIn = 0, fadeOut = 0;
    private PlayMode playMode = PlayMode.LOOP;


    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("dat", "Song file",
                () => songData,
                v => songData = (EmbeddedData)v,
                SongFileGUI),
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

    private void SongFileGUI(Property property)
    {
        if (GUILayout.Button("Load file"))
        {
            var browser = GUIManager.guiGameObject.AddComponent<FileBrowser>();
            browser.fileAction = (path) =>
            {
                var bytes = System.IO.File.ReadAllBytes(path);
                property.value = new EmbeddedData(bytes);
            };
        }
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        var component = gameObject.AddComponent<SunVoxSongComponent>();
        component.songData = songData;
        component.playMode = playMode;
        component.volume = volume;
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
    public SunVoxSongBehavior.PlayMode playMode;

    private int slot;
    private float currentVolume = 0;
    private bool fadingIn, fadingOut;

    public void Init()
    {
        slot = SunVoxUtils.OpenUnusedSlot();
        int result = SunVox.sv_load_from_memory(slot, songData.bytes, songData.bytes.Length);
        if (result != 0)
        {
            Debug.LogError("Error loading file");
            return;
        }
        Debug.Log(System.Runtime.InteropServices.Marshal.PtrToStringAuto(SunVox.sv_get_song_name(0)));

        currentVolume = 0;
        UpdateSunVoxVolume();

        if (playMode == SunVoxSongBehavior.PlayMode.ONCE)
            SunVox.sv_set_autostop(slot, 1);
        else
            SunVox.sv_set_autostop(slot, 0);
        if (playMode == SunVoxSongBehavior.PlayMode.BACKGROUND)
            SunVox.sv_play_from_beginning(slot);

        StartCoroutine(VolumeUpdateCoroutine()); // runs even while disabled
    }

    public void OnDestroy()
    {
        SunVoxUtils.CloseSlot(slot);
    }

    public override void BehaviorEnabled()
    {
        if (playMode != SunVoxSongBehavior.PlayMode.BACKGROUND)
            currentVolume = 0;
        fadingIn = true;
        fadingOut = false;
        UpdateSunVoxVolume();
        if (playMode != SunVoxSongBehavior.PlayMode.BACKGROUND)
            SunVox.sv_play_from_beginning(slot);
    }

    public override void BehaviorDisabled()
    {
        fadingOut = true;
        fadingIn = false;
    }

    private IEnumerator VolumeUpdateCoroutine()
    {
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
                    if (playMode != SunVoxSongBehavior.PlayMode.BACKGROUND)
                        SunVox.sv_stop(slot);
                }
                UpdateSunVoxVolume();
            }
            yield return null;
        }
    }

    private void UpdateSunVoxVolume()
    {
        SunVox.sv_volume(slot, (int)(currentVolume * 256.0f / 100.0f));
    }
}