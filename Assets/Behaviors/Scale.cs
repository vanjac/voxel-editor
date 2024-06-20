using System.Collections.Generic;
using UnityEngine;

[EditorPreviewBehavior(marker = true)]
public class ScaleBehavior : GenericEntityBehavior<ScaleBehavior, ScaleComponent>
{
    public static new BehaviorType objectType = new BehaviorType(
        "Scale", typeof(ScaleBehavior))
    {
        displayName = s => s.ScaleName,
        description = s => s.ScaleDesc,
        longDescription = s => s.ScaleLongDesc,
        iconName = "resize",
        rule = BehaviorType.AndRule(
            BehaviorType.BaseTypeRule(typeof(DynamicEntity)),
            BehaviorType.NotBaseTypeRule(typeof(PlayerObject))),
    };
    public override BehaviorType BehaviorObjectType => objectType;

    public Vector3 scale = Vector3.one;

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("sca", s => s.PropScaleFactor,
                () => scale,
                v => scale = (Vector3)v,
                PropertyGUIs.Vector3),
        });
}

public class ScaleComponent : BehaviorComponent<ScaleBehavior>
{
    private Vector3 storedScale;

    public override void BehaviorEnabled()
    {
        storedScale = transform.localScale;
        var scale = behavior.scale;
        if (GetComponent<ObjectMarker>()) // editor preview
        {
            scale.x = AdjustPreviewScale(scale.x);
            scale.y = AdjustPreviewScale(scale.y);
            scale.z = AdjustPreviewScale(scale.z);
        }
        transform.localScale = scale;
    }

    private float AdjustPreviewScale(float s) => Mathf.Sign(s) * Mathf.Max(Mathf.Abs(s), 0.1f);

    public override void BehaviorDisabled()
    {
        transform.localScale = storedScale;
    }
}