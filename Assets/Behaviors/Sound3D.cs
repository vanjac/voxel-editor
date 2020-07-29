using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpatialSoundMode
{
    POINT, AMBIENT
}

public class Sound3DBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "3D Sound", "Play a sound in 3D space",
        "•  In Point mode, stereo panning will be used to make the sound seem to emit from the object.\n"
        + "•  In Ambient mode the sound will seem to surround the player.\n"
        + "•  \"Fade distance\": Beyond this range the sound will fade with distance. Within the range it's at full volume. "
        + "Higher values increase the volume outside fade distance.\n"
        + "•  \"Max distance\": Sound will be inaudible past this distance.\n\n"
        + "See Sound behavior for additional documentation.",
        "headphones", typeof(Sound3DBehavior), BehaviorType.BaseTypeRule(typeof(DynamicEntity)));

    private EmbeddedData soundData = new EmbeddedData();
    private float volume = 50.0f;
    private (float, float) distanceRange = (1, 30);
    private PlayMode playMode = PlayMode.ONCE;
    private SpatialSoundMode spatialMode = SpatialSoundMode.POINT;

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
            new Property("smo", "Spatial mode",
                () => spatialMode,
                v => spatialMode = (SpatialSoundMode)v,
                PropertyGUIs.Enum),
            new Property("vol", "Volume",
                () => volume,
                v => volume = (float)v,
                PropertyGUIs.Float),
            new Property("dis", "Fade distance",
                () => distanceRange,
                v => distanceRange = ((float, float))v,
                PropertyGUIs.FloatRange)
        });
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        var component = gameObject.AddComponent<SoundComponent>();
        component.soundData = soundData;
        component.playMode = playMode;
        component.volume = volume / 100.0f;
        component.minDistance = distanceRange.Item1;
        component.maxDistance = distanceRange.Item2;
        component.fadeIn = 0;
        component.fadeOut = 0;
        component.spatial = true;
        component.spatialMode = spatialMode;
        component.Init();
        return component;
    }
}
