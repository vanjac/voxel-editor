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

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("Speed",
                () => speed,
                v => speed = (float)v,
                PropertyGUIs.Float)
        });
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        SpinComponent spin = gameObject.AddComponent<SpinComponent>();
        spin.speed = speed;
        return spin;
    }
}

public class SpinComponent : MotionComponent
{
    public float speed;

    public override Quaternion GetRotateFixed()
    {
        return Quaternion.AngleAxis(speed * Time.fixedDeltaTime, Vector3.up);
    }
}