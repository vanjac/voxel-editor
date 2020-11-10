using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Spin", "Rotate continuously", "Speed is in degrees per second.",
        "format-rotate-90", typeof(SpinBehavior),
        BehaviorType.AndRule(
            BehaviorType.BaseTypeRule(typeof(DynamicEntity)),
            BehaviorType.NotBaseTypeRule(typeof(PlayerObject))));

    private float speed = 50;
    private Target axis = new Target(Target.UP);

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

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
                PropertyGUIs.TargetNoObject)
        });
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
    public Target axis;

    public override Quaternion GetRotateFixed()
    {
        return Quaternion.AngleAxis(speed * Time.fixedDeltaTime, axis.DirectionFrom(transform));
    }
}