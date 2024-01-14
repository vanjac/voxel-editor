using System.Collections.Generic;
using UnityEngine;

public enum SpatialSoundMode
{
    POINT, AMBIENT
}

public class Sound3DBehavior : BaseSoundBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "3D Sound", s => s.Sound3DDesc, s => s.Sound3DLongDesc, "headphones",
        typeof(Sound3DBehavior), BehaviorType.BaseTypeRule(typeof(DynamicEntity)));
    public override BehaviorType BehaviorObjectType => objectType;

    public (float, float) distanceRange = (1, 30);
    public SpatialSoundMode spatialMode = SpatialSoundMode.POINT;

    public override IEnumerable<Property> Properties() =>
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
