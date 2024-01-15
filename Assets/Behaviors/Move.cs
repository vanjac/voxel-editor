using System.Collections.Generic;
using UnityEngine;

public class MoveBehavior : GenericEntityBehavior<MoveBehavior, MoveComponent>
{
    public static new BehaviorType objectType = new BehaviorType(
        "Move", typeof(MoveBehavior))
    {
        description = s => s.MoveDesc,
        longDescription = s => s.MoveLongDesc,
        iconName = "arrow-right-bold-box-outline",
        rule = BehaviorType.BaseTypeRule(typeof(DynamicEntity)),
    };
    public override BehaviorType BehaviorObjectType => objectType;

    public Target target = new Target(Target.NORTH);
    public float speed = 1;

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("vel", s => s.PropSpeed,
                () => speed,
                v => speed = (float)v,
                PropertyGUIs.Float),
            new Property("dir", s => s.PropToward,
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
