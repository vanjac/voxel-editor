using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Light", "", "lightbulb-on", typeof(LightBehavior));

    private float size = 10, intensity = 1;
    private Color color = Color.white;
    private bool halo = true;

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("Size",
                () => size,
                v => size = (float)v,
                PropertyGUIs.Slider(1, 30)),
            new Property("Color",
                () => color,
                v => color = (Color)v,
                PropertyGUIs.Color),
            new Property("Intensity",
                () => intensity,
                v => intensity = (float)v,
                PropertyGUIs.Slider(0, 5)),
            new Property("Halo?",
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
        light.halo = halo;
        return light;
    }
}

public class LightComponent : BehaviorComponent
{
    public float size, intensity;
    public Color color;
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