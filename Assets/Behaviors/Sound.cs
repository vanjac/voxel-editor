using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                PropertyGUIs.EmbeddedData(EmbeddedDataType.Audio)), // TODO player
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
        component.volume = volume;
        component.fadeIn = fadeIn;
        component.fadeOut = fadeOut;
        component.Init();
        return component;
    }
}

public class SoundComponent : BehaviorComponent
{
    public EmbeddedData songData;
    public float volume, fadeIn, fadeOut;
    public SoundBehavior.PlayMode playMode;

    private AudioSource audioSource;

    public void Init()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = volume;

        if (songData.bytes.Length == 0)
            return;
        float[] samples = new float[songData.bytes.Length / 4];
        System.Buffer.BlockCopy(songData.bytes, 0, samples, 0, songData.bytes.Length);

        // TODO TODO TODO
        AudioClip clip = AudioClip.Create(songData.name, samples.Length / 2, 2, 44100, false);
        clip.SetData(samples, 0);

        audioSource.clip = clip;
        //audioSource.playOnAwake = false;
    }

    public override void Start()
    {
        base.Start();
        audioSource.Play();
    }

    void Update()
    {
        // TODO: this holds a DC offset when paused
        audioSource.pitch = Time.timeScale; // allow pausing
    }
}