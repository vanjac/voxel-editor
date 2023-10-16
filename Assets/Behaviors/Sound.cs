using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayMode
{
    ONCE, _1SHOT, LOOP, BKGND
}

public abstract class BaseSoundBehavior : EntityBehavior
{
    public EmbeddedData soundData = new EmbeddedData();
    public float volume = 50.0f, fadeIn = 0, fadeOut = 0;
    public PlayMode playMode = PlayMode.ONCE;
}

public class SoundBehavior : BaseSoundBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Sound", "Play a sound",
        "•  <b>One-shot</b> mode plays the entire sound every time the behavior is active. "
        + "Multiple copies can play at once. Fades have no effect.\n"
        + "•  In <b>Background</b> mode the sound is always playing, but muted when the behavior is inactive.\n\n"
        + "Supported formats: MP3, WAV, OGG, AIF, XM, IT",
        "volume-high", typeof(SoundBehavior));

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
        component.Init(this);
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

public class SoundComponent : BehaviorComponent<BaseSoundBehavior>
{
    private AudioSource audioSource;
    private bool fadingIn, fadingOut;

    public override void Init(BaseSoundBehavior behavior)
    {
        base.Init(behavior);
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = 0;
        audioSource.loop = behavior.playMode == PlayMode.LOOP || behavior.playMode == PlayMode.BKGND;
        if (behavior is Sound3DBehavior behavior3d)
        {
            audioSource.spatialBlend = 1.0f;
            audioSource.minDistance = behavior3d.distanceRange.Item1;
            audioSource.spread = behavior3d.spatialMode == SpatialSoundMode.AMBIENT ? 180 : 0;
        }
        else
        {
            audioSource.spatialBlend = 0.0f;
        }
        audioSource.playOnAwake = false;

        if (behavior.soundData.bytes.Length == 0)
            return;
        audioSource.clip = AudioCompression.Decompress(behavior.soundData.bytes, this);

        if (behavior.playMode != PlayMode._1SHOT)
            StartCoroutine(VolumeUpdateCoroutine());
    }

    public override void BehaviorEnabled()
    {
        if (behavior.playMode == PlayMode._1SHOT)
        {
            audioSource.volume = behavior.volume / 100.0f;
            audioSource.PlayOneShot(audioSource.clip);
        }
        else
        {
            if (behavior.playMode != PlayMode.BKGND)
            {
                if (behavior.fadeIn == 0)
                    audioSource.volume = behavior.volume / 100.0f;
                else
                    audioSource.volume = 0;
            }
            fadingIn = true;
            fadingOut = false;
            if (behavior.playMode != PlayMode.BKGND)
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

        if (behavior.playMode == PlayMode.BKGND)
            audioSource.Play();

        float volume = behavior.volume / 100.0f;
        Transform listener = GameObject.FindObjectOfType<AudioListener>().transform;
        while (true)
        {
            if ((behavior is Sound3DBehavior behavior3d) && listener != null)
            {
                float sqrDist = (this.transform.position - listener.position).sqrMagnitude;
                float maxDist = behavior3d.distanceRange.Item2;
                audioSource.mute = sqrDist > (maxDist * maxDist);
            }
            if (fadingIn)
            {
                if (behavior.fadeIn == 0)
                    audioSource.volume = volume;
                else
                    audioSource.volume += volume / behavior.fadeIn * Time.unscaledDeltaTime;
                if (audioSource.volume >= volume)
                {
                    audioSource.volume = volume;
                    fadingIn = false;
                }
            }
            else if (fadingOut)
            {
                if (behavior.fadeOut == 0)
                    audioSource.volume = 0;
                else
                    audioSource.volume -= volume / behavior.fadeOut * Time.unscaledDeltaTime;
                if (audioSource.volume <= 0)
                {
                    audioSource.volume = 0;
                    fadingOut = false;
                    if (behavior.playMode != PlayMode.BKGND)
                        audioSource.Stop();
                }
            }

            yield return null;
        }
    }
}