using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Move", "Move in a direction or toward an object",
        "If both Solid and Physics/Character behaviors are active, object will not be able to pass through other objects. "
        + "Increase the Density property of the Physics behavior to increase the object's pushing strength. Gravity has no effect while Move is active.",
        "arrow-right-bold-box-outline", typeof(MoveBehavior), BehaviorType.BaseTypeRule(typeof(DynamicEntity)));

    private Target target = new Target(Target.NORTH);
    private float speed = 1;

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
            new Property("dir", "Toward",
                () => target,
                v => target = (Target)v,
                PropertyGUIs.Target)
        });
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        MoveComponent move = gameObject.AddComponent<MoveComponent>();
        move.target = target;
        move.speed = speed;
        return move;
    }
}

public class MoveComponent : MotionComponent
{
    public Target target;
    public float speed;

    public override void BehaviorEnabled()
    {
        target.PickRandom();
        base.BehaviorEnabled();
    }

    public override Vector3 GetTranslateFixed()
    {
        Vector3 direction = target.DirectionFrom(transform);
        float distance = target.DistanceFrom(transform);
        float magnitude = speed * Time.fixedDeltaTime;
        if (magnitude > distance)
            magnitude = distance;
        return direction * magnitude;
    }
}
