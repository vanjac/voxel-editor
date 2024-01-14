using System.Collections.Generic;
using UnityEngine;

public class ScaleBehavior : GenericEntityBehavior<ScaleBehavior, ScaleComponent>
{
    public static new BehaviorType objectType = new BehaviorType(
        "Scale", s => s.ScaleDesc, s => s.ScaleLongDesc, "resize", typeof(ScaleBehavior),
        BehaviorType.AndRule(
            BehaviorType.BaseTypeRule(typeof(DynamicEntity)),
            BehaviorType.NotBaseTypeRule(typeof(PlayerObject))));
    public override BehaviorType BehaviorObjectType => objectType;

    public Vector3 scale = Vector3.one;

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("sca", "Factor",
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
        transform.localScale = behavior.scale;
    }

    public override void BehaviorDisabled()
    {
        transform.localScale = storedScale;
    }
}