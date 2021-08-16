using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Spin", "Rotate continuously",
        "<b>Speed</b> is in degrees per second. <b>Axis</b> specifies the axis of rotation.",
        "format-rotate-90", typeof(SpinBehavior),
        BehaviorType.BaseTypeRule(typeof(DynamicEntity)));

    [FloatProp("vel", "Speed")]
    public float speed { get; set; } = 50;
    [TargetStaticProp("axi", "Axis")]
    public Target axis { get; set; } = new Target(Target.UP);

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        SpinComponent spin = gameObject.AddComponent<SpinComponent>();
        spin.speed = speed;
        spin.axis = axis;
        return spin;
    }
}

public class SpinComponent : MotionComponent
{
    public float speed;
    public Target axis;  // not random

    public override Quaternion GetRotateFixed()
    {
        return Quaternion.AngleAxis(speed * Time.fixedDeltaTime, axis.DirectionFrom(transform));
    }
}