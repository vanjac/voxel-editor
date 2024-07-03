using System.Collections.Generic;
using UnityEngine;

[EditorPreviewBehavior]
public class LightBehavior : GenericEntityBehavior<LightBehavior, LightComponent> {
    public static new BehaviorType objectType = new BehaviorType("Light", typeof(LightBehavior)) {
        displayName = s => s.LightName,
        description = s => s.LightDesc,
        longDescription = s => s.LightLongDesc,
        iconName = "lightbulb-on",
    };
    public override BehaviorType BehaviorObjectType => objectType;

    public float size = 10, intensity = 1;
    public Color color = Color.white;
    public bool shadows = false;
    public bool halo = false;  // deprecated

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(base.Properties(), new Property[] {
            new Property("siz", s => s.PropSize,
                () => size,
                v => size = (float)v,
                PropertyGUIs.Slider(1, 30)),
            new Property("col", s => s.PropColor,
                () => color,
                v => color = (Color)v,
                PropertyGUIs.Color),
            new Property("int", s => s.PropIntensity,
                () => intensity,
                v => intensity = (float)v,
                PropertyGUIs.Slider(0, 5)),
            new Property("sha", s => s.PropShadowsEnable,
                () => shadows,
                v => shadows = (bool)v,
                PropertyGUIs.Toggle)
        });

    public override IEnumerable<Property> DeprecatedProperties() =>
        Property.JoinProperties(base.DeprecatedProperties(), new Property[] {
            new Property("hal", GUIStringSet.Empty,
                () => halo,
                v => halo = (bool)v,
                PropertyGUIs.Toggle)
        });
}

public class LightComponent : BehaviorComponent<LightBehavior> {
    private Light lightComponent;

    public override void Start() {
        var lightObj = new GameObject(); // only one Light allowed per GameObject
        lightObj.transform.SetParent(transform, false);
        lightComponent = lightObj.AddComponent<Light>();
        lightComponent.range = behavior.size;
        lightComponent.intensity = behavior.intensity;
        lightComponent.color = behavior.color;
        lightComponent.enabled = false;

        if (behavior.shadows) {
            lightComponent.shadows = LightShadows.Hard;
            // fix seams (also done in directional light)
            lightComponent.shadowBias = 0.0f;
            lightComponent.shadowNormalBias = 0.0f;
        }

        base.Start();
    }

    public override void BehaviorEnabled() {
        lightComponent.enabled = true;
    }

    public override void BehaviorDisabled() {
        lightComponent.enabled = false;
    }
}