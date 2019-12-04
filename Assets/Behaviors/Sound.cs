using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO a lot of this file is copied from SunVoxSong.cs and that's no good

public class SoundBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Sound", "Play a sound",
        "volume-high", typeof(SoundBehavior));
    
    public enum PlayMode
    {
        ONCE, LOOP, BACKGROUND
    }

    private EmbeddedData songData = new EmbeddedData();
    private float volume = 50.0f, fadeIn = 0, fadeOut = 0;
    private PlayMode playMode = PlayMode.ONCE;

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
                PropertyGUIs.EmbeddedData(EmbeddedDataType.Audio, SoundPlayer.Factory)), // TODO player
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
        var component = gameObject.AddComponent<SoundComponent>();
        component.songData = songData;
        component.playMode = playMode;
        component.volume = volume / 100.0f;
        component.fadeIn = fadeIn;
        component.fadeOut = fadeOut;
        component.Init();
        return component;
    }
}

public class SoundPlayer : AudioPlayer
{
    private GameObject gameObject;

    public static AudioPlayer Factory(byte[] data)
    {
        return new SoundPlayer(data);
    }

    public SoundPlayer(byte[] data)
    {
        gameObject = new GameObject("Sound");
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = AudioCompression.Decompress(data, gameObject.AddComponent<CoroutineMonoBehaviour>());
        source.Play();
    }

    public void Stop()
    {
        GameObject.Destroy(gameObject);
    }
}

public class SoundComponent : BehaviorComponent
{
    public EmbeddedData songData;
    public float volume, fadeIn, fadeOut;
    public SoundBehavior.PlayMode playMode;

    private AudioSource audioSource;
    private bool fadingIn, fadingOut;

    public void Init()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = 0;
        audioSource.loop = playMode != SoundBehavior.PlayMode.ONCE;
        audioSource.playOnAwake = false;

        if (songData.bytes.Length == 0)
            return;
        audioSource.clip = AudioCompression.Decompress(songData.bytes, this);
        StartCoroutine(VolumeUpdateCoroutine());
    }

    public override void BehaviorEnabled()
    {
        if (playMode != SoundBehavior.PlayMode.BACKGROUND)
            audioSource.volume = 0;
        fadingIn = true;
        fadingOut = false;
        if (playMode != SoundBehavior.PlayMode.BACKGROUND)
            audioSource.Play();
    }

    public override void BehaviorDisabled()
    {
        fadingOut = true;
        fadingIn = false;
    }

    private IEnumerator VolumeUpdateCoroutine()
    {
        yield return null; // wait a frame to allow world to finish loading

        if (playMode == SoundBehavior.PlayMode.BACKGROUND)
            audioSource.Play();

        while (true)
        {
            // TODO: this holds a DC offset when paused
            audioSource.pitch = Time.timeScale; // allow pausing
            if (fadingIn)
            {
                if (fadeIn == 0)
                    audioSource.volume = volume;
                else
                    audioSource.volume += volume / fadeIn * Time.unscaledDeltaTime;
                if (audioSource.volume >= volume)
                {
                    audioSource.volume = volume;
                    fadingIn = false;
                }
            }
            else if (fadingOut)
            {
                if (fadeOut == 0)
                    audioSource.volume = 0;
                else
                    audioSource.volume -= volume / fadeOut * Time.unscaledDeltaTime;
                if (audioSource.volume <= 0)
                {
                    audioSource.volume = 0;
                    fadingOut = false;
                    if (playMode != SoundBehavior.PlayMode.BACKGROUND)
                        audioSource.Stop();
                }
            }

            yield return null;
        }
    }
}