using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpatialSoundMode
{
    POINT, AMBIENT
}

public class Sound3DBehavior : BaseSoundBehavior
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
    public override BehaviorType BehaviorObjectType => objectType;

    public (float, float) distanceRange = (1, 30);
    public SpatialSoundMode spatialMode = SpatialSoundMode.POINT;

    public override ICollection<Property> Properties() =>
        Property.JoinProperties(base.Properties(), new Property[]
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

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        var component = gameObject.AddComponent<SoundComponent>();
        component.Init(this);
        return component;
    }
}
