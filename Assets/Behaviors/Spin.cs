using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinBehavior : GenericEntityBehavior<SpinBehavior, SpinComponent>
{
    public static new BehaviorType objectType = new BehaviorType(
        "Spin", "Rotate continuously",
        "<b>Speed</b> is in degrees per second. <b>Axis</b> specifies the axis of rotation.\n"
        + "Substances will rotate around their <b>Pivot</b> point.",
        "format-rotate-90", typeof(SpinBehavior),
        BehaviorType.BaseTypeRule(typeof(DynamicEntity)));
    public override BehaviorType BehaviorObjectType => objectType;

    public float speed = 50;
    public Target axis = new Target(Target.UP); // not random

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("vel", "Speed",
                () => speed,
                v => speed = (float)v,
                PropertyGUIs.Float),
            new Property("axi", "Axis",
                () => axis,
                v => axis = (Target)v,
                PropertyGUIs.TargetStatic)
        });
    }
}

public class SpinComponent : MotionComponent<SpinBehavior>
{
    public override Quaternion GetRotateFixed()
    {
        return Quaternion.AngleAxis(behavior.speed * Time.fixedDeltaTime,
            behavior.axis.DirectionFrom(transform));
    }
}