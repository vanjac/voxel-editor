using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayMode
{
    ONCE, _1SHOT, LOOP, BKGND
}

public class SoundBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Sound", "Play a sound",
        "•  One-shot mode plays the entire sound every time the behavior is active. "
        + "Multiple copies can play at once. Fades have no effect.\n"
        + "•  In Background mode the sound is always playing, but muted when the behavior is inactive.\n\n"
        + "Supported formats: MP3, WAV, OGG, AIF, XM, IT",
        "volume-high", typeof(SoundBehavior));

    private EmbeddedData soundData = new EmbeddedData();
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
            new Property("dat", "Sound",
                () => soundData,
                v => soundData = (EmbeddedData)v,
                PropertyGUIs.EmbeddedData(EmbeddedDataType.Audio, SoundPlayer.Factory)),
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
        component.soundData = soundData;
        component.playMode = playMode;
        component.volume = volume / 100.0f;
        component.fadeIn = fadeIn;
        component.fadeOut = fadeOut;
        component.spatial = false;
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
    public EmbeddedData soundData;
    public float volume, fadeIn, fadeOut, minDistance, maxDistance;
    public bool spatial;
    public PlayMode playMode;
    public SpatialSoundMode spatialMode;

    private AudioSource audioSource;
    private bool fadingIn, fadingOut;

    public void Init()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = 0;
        audioSource.loop = playMode == PlayMode.LOOP || playMode == PlayMode.BKGND;
        audioSource.spatialBlend = spatial ? 1.0f : 0.0f;
        if (spatial)
        {
            audioSource.minDistance = minDistance;
            audioSource.spread = spatialMode == SpatialSoundMode.AMBIENT ? 180 : 0;
        }
        audioSource.playOnAwake = false;

        if (soundData.bytes.Length == 0)
            return;
        audioSource.clip = AudioCompression.Decompress(soundData.bytes, this);

        if (playMode != PlayMode._1SHOT)
            StartCoroutine(VolumeUpdateCoroutine());
    }

    public override void BehaviorEnabled()
    {
        if (playMode == PlayMode._1SHOT)
        {
            audioSource.volume = volume;
            audioSource.PlayOneShot(audioSource.clip);
        }
        else
        {
            if (playMode != PlayMode.BKGND)
            {
                if (fadeIn == 0)
                    audioSource.volume = volume;
                else
                    audioSource.volume = 0;
            }
            fadingIn = true;
            fadingOut = false;
            if (playMode != PlayMode.BKGND)
                audioSource.Play();
        }
    }

    public override void BehaviorDisabled()
    {
        fadingOut = true;
        fadingIn = false;
    }

    private IEnumerator VolumeUpdateCoroutine()
    {
        yield return null; // wait a frame to allow world to finish loading

        if (playMode == PlayMode.BKGND)
            audioSource.Play();

        Transform listener = GameObject.FindObjectOfType<AudioListener>().transform;
        while (true)
        {
            if (spatial && listener != null)
            {
                float sqrDist = (this.transform.position - listener.position).sqrMagnitude;
                audioSource.mute = sqrDist > (maxDistance * maxDistance);
            }
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
                    if (playMode != PlayMode.BKGND)
                        audioSource.Stop();
                }
            }

            yield return null;
        }
    }
}