using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[EditorPreviewBehavior]
public class HaloBehavior : GenericEntityBehavior<HaloBehavior, HaloComponent>
{
    public static new BehaviorType objectType = new BehaviorType(
        "Halo", "Glowing effect",
        "Halo appears at the Pivot point of substances", "blur", typeof(HaloBehavior));

    public float size = 3;
    public Color color = Color.white;  // scaled by INTENSITY

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
                PropertyGUIs.Slider(0.5f, 15)),
            new Property("col", "Color",
                () => color,
                v => color = (Color)v,
                PropertyGUIs.Color)
        });
    }
}

public class HaloComponent : BehaviorComponent<HaloBehavior>
{
    public const float INTENSITY = 1.4f;  // doesn't get any brighter past this

    private Light lightComponent;

    public override void Start()
    {
        // Halos are not exposed through the unity api :(
        var lightObj = Instantiate(Resources.Load<GameObject>("LightHaloPrefab"));
        lightObj.transform.SetParent(transform, false);
        lightComponent = lightObj.GetComponent<Light>();
        lightComponent.range = behavior.size;
        lightComponent.color = behavior.color;
        lightComponent.intensity = INTENSITY;
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