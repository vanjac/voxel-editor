using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[EditorPreviewBehavior]
public class LightBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Light", "A light source at the center of the object",
        "lightbulb-on", typeof(LightBehavior));

    [SliderProp("siz", "Size", 1, 30)]
    public float size { get; set; } = 10;
    [ColorProp("col", "Color")]
    public Color color { get; set; } = Color.white;
    [SliderProp("int", "Intensity", 0, 5)]
    public float intensity { get; set; } = 1;
    [ToggleProp("sha", "Shadows?")]
    public bool shadows { get; set; } = false;

    public bool halo = false;  // deprecated

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public override IEnumerable<Property> DeprecatedProperties()
    {
        return Property.JoinProperties(base.DeprecatedProperties(), new Property[]
        {
            new Property("hal", "Halo?",
                () => halo,
                v => halo = (bool)v,
                PropertyGUIs.Toggle)
        });
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        var light = gameObject.AddComponent<LightComponent>();
        light.size = size;
        light.color = color;
        light.intensity = intensity;
        light.shadows = shadows;
        return light;
    }
}

public class LightComponent : BehaviorComponent
{
    public float size, intensity;
    public Color color;
    public bool shadows;

    private Light lightComponent;

    public override void Start()
    {
        lightComponent = gameObject.AddComponent<Light>();
        lightComponent.range = size;
        lightComponent.intensity = intensity;
        lightComponent.color = color;
        lightComponent.enabled = false;

        if (shadows)
        {
            lightComponent.shadows = LightShadows.Hard;
            // fix seams (also done in directional light)
            lightComponent.shadowBias = 0.0f;
            lightComponent.shadowNormalBias = 0.0f;
        }

        base.Start();
    }

    public override void BehaviorEnabled()
    {
        lightComponent.enabled = true;
    }

    public override void BehaviorDisabled()
    {
        lightComponent.enabled = false;
    }
}