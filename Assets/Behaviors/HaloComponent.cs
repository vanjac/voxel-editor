using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[EditorPreviewBehavior]
public class HaloBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Halo", "A glowing effect", "blur", typeof(HaloBehavior));

    [SliderProp("siz", "Size", 0.5f, 15)]
    public float size { get; set; } = 3;
    [ColorProp("col", "Color")]
    public Color color { get; set; } = Color.white;  // scaled by INTENSITY

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        var component = gameObject.AddComponent<HaloComponent>();
        component.size = size;
        component.color = color;
        return component;
    }
}

public class HaloComponent : BehaviorComponent
{
    public const float INTENSITY = 1.4f;  // doesn't get any brighter past this
    public float size;
    public Color color;

    private Light lightComponent;

    public override void Start()
    {
        // Halos are not exposed through the unity api :(
        var lightObj = Instantiate(Resources.Load<GameObject>("LightHaloPrefab"));
        lightObj.transform.SetParent(transform, false);
        lightComponent = lightObj.GetComponent<Light>();
        lightComponent.range = size;
        lightComponent.color = color;
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