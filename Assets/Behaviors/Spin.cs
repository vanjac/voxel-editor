using System.Collections.Generic;
using UnityEngine;

public class SpinBehavior : GenericEntityBehavior<SpinBehavior, SpinComponent>
{
    public static new BehaviorType objectType = new BehaviorType("Spin", typeof(SpinBehavior))
    {
        displayName = s => s.SpinName,
        description = s => s.SpinDesc,
        longDescription = s => s.SpinLongDesc,
        iconName = "format-rotate-90",
        rule = BehaviorType.BaseTypeRule(typeof(DynamicEntity)),
    };
    public override BehaviorType BehaviorObjectType => objectType;

    public float speed = 50;
    public Target axis = new Target(Target.UP); // not random

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("vel", s => s.PropSpeed,
                () => speed,
                v => speed = (float)v,
                PropertyGUIs.Float),
            new Property("axi", s => s.PropAxis,
                () => axis,
                v => axis = (Target)v,
                PropertyGUIs.TargetStatic)
        });
}

public class SpinComponent : MotionComponent<SpinBehavior>
{
    public override Quaternion GetRotateFixed() =>
        Quaternion.AngleAxis(behavior.speed * Time.fixedDeltaTime,
            behavior.axis.DirectionFrom(transform));
}