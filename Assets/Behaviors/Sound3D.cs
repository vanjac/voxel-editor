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
        "•  In <b>Point</b> mode, stereo panning will be used to make the sound seem to emit from the object.\n"
        + "•  In <b>Ambient</b> mode the sound will seem to surround the player.\n"
        + "•  <b>Fade distance:</b> Beyond this range the sound will fade with distance. Within the range it's at full volume. "
        + "Higher values increase the volume outside fade distance.\n"
        + "•  <b>Max distance:</b> Sound will be inaudible past this distance.\n\n"
        + "See Sound behavior for additional documentation.",
        "headphones", typeof(Sound3DBehavior), BehaviorType.BaseTypeRule(typeof(DynamicEntity)));

    [EmbeddedAudioProp("dat", "Sound")]
    public EmbeddedData soundData { get; set; } = new EmbeddedData();
    [EnumProp("pmo", "Play mode")]
    public PlayMode playMode { get; set; } = PlayMode.ONCE;
    [EnumProp("smo", "Spatial mode")]
    public SpatialSoundMode spatialMode { get; set; } = SpatialSoundMode.POINT;
    [FloatProp("vol", "Volume")]
    public float volume { get; set; } = 50.0f;
    [FloatRangeProp("dis", "Fade distance")]
    public (float, float) distanceRange { get; set; } = (1, 30);

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
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
