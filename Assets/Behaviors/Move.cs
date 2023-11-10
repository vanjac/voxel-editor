using System.Collections.Generic;
using UnityEngine;

public class MoveBehavior : GenericEntityBehavior<MoveBehavior, MoveComponent>
{
    public static new BehaviorType objectType = new BehaviorType(
        "Move", "Move in a direction or toward object",
        "When used with Solid and Physics behaviors, object will not be able to pass through other objects.\n"
        + "When used with Solid and Character behaviors, object will additionally be affected by gravity.\n"
        + "Increase the <b>Density</b> of the Physics/Character behavior to increase the object's pushing strength.",
        "arrow-right-bold-box-outline", typeof(MoveBehavior), BehaviorType.BaseTypeRule(typeof(DynamicEntity)));
    public override BehaviorType BehaviorObjectType => objectType;

    public Target target = new Target(Target.NORTH);
    public float speed = 1;

    public override ICollection<Property> Properties() =>
        Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("vel", "Speed",
                () => speed,
                v => speed = (float)v,
                PropertyGUIs.Float),
            new Property("dir", "Toward",
                () => target,
                v => target = (Target)v,
                PropertyGUIs.Target)
        });
}

public class MoveComponent : MotionComponent<MoveBehavior>
{
    public override void BehaviorEnabled()
    {
        behavior.target.PickRandom();
        base.BehaviorEnabled();
    }

    public override Vector3 GetTranslateFixed()
    {
        Vector3 direction = behavior.target.DirectionFrom(transform);
        float distance = behavior.target.DistanceFrom(transform);
        float magnitude = behavior.speed * Time.fixedDeltaTime;
        if (magnitude > distance)
            magnitude = distance;
        return direction * magnitude;
    }
}
