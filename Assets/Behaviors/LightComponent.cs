using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[EditorPreviewBehavior]
public class LightBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Light", "A light source at the center of the object",
        "lightbulb-on", typeof(LightBehavior));

    private float size = 10, intensity = 1;
    private Color color = Color.white;
    private bool shadows = false;
    private bool halo = false;

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("siz", "Size",
                () => size,
                v => size = (float)v,
                PropertyGUIs.Slider(1, 30)),
            new Property("col", "Color",
                () => color,
                v => color = (Color)v,
                PropertyGUIs.Color),
            new Property("int", "Intensity",
                () => intensity,
                v => intensity = (float)v,
                PropertyGUIs.Slider(0, 5)),
            new Property("sha", "Shadows?",
                () => shadows,
                v => shadows = (bool)v,
                PropertyGUIs.Toggle),
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
        light.halo = halo;
        return light;
    }
}

public class LightComponent : BehaviorComponent
{
    public float size, intensity;
    public Color color;
    public bool shadows;
    public bool halo;

    private Light lightComponent;

    public override void Start()
    {
        if (halo)
        {
            // Halos are not exposed through the unity api :(
            var lightObj = Instantiate(Resources.Load<GameObject>("LightHaloPrefab"));
            lightObj.transform.SetParent(transform, false);
            lightComponent = lightObj.GetComponent<Light>();
        }
        else
        {
            lightComponent = gameObject.AddComponent<Light>();
        }
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