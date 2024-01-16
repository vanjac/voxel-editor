using System.Collections.Generic;
using UnityEngine;

public enum SpatialSoundMode
{
    POINT, AMBIENT
}

public class Sound3DBehavior : BaseSoundBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "3D Sound", typeof(Sound3DBehavior))
    {
        displayName = s => s.Sound3DName,
        description = s => s.Sound3DDesc,
        longDescription = s => s.Sound3DLongDesc,
        iconName = "headphones",
        rule = BehaviorType.BaseTypeRule(typeof(DynamicEntity)),
    };
    public override BehaviorType BehaviorObjectType => objectType;

    public (float, float) distanceRange = (1, 30);
    public SpatialSoundMode spatialMode = SpatialSoundMode.POINT;

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("dat", s => s.PropSound,
                () => soundData,
                v => soundData = (EmbeddedData)v,
                PropertyGUIs.EmbeddedData(EmbeddedDataType.Audio, SoundPlayer.Factory)),
            new Property("pmo", s => s.PropPlayMode,
                () => playMode,
                v => playMode = (PlayMode)v,
                PropertyGUIs.Enum),
            new Property("smo", s => s.PropSpatialMode,
                () => spatialMode,
                v => spatialMode = (SpatialSoundMode)v,
                PropertyGUIs.Enum),
            new Property("vol", s => s.PropVolume,
                () => volume,
                v => volume = (float)v,
                PropertyGUIs.Float),
            new Property("dis", s => s.PropFadeDistance,
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
