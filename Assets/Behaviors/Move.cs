using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Move", "Move in a direction or toward an object",
        "When used with Solid and Physics behaviors, object will not be able to pass through other objects.\n"
        + "When used with Solid and Character behaviors, object will additionally be affected by gravity.\n"
        + "Increase the <b>Density</b> of the Physics/Character behavior to increase the object's pushing strength.",
        "arrow-right-bold-box-outline", typeof(MoveBehavior), BehaviorType.BaseTypeRule(typeof(DynamicEntity)));

    [FloatProp("vel", "Speed")]
    public float speed { get; set; } = 1;
    [TargetProp("dir", "Toward")]
    public Target toward { get; set; } = new Target(Target.NORTH);

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        MoveComponent move = gameObject.AddComponent<MoveComponent>();
        move.toward = toward;
        move.speed = speed;
        return move;
    }
}

public class MoveComponent : MotionComponent
{
    public Target toward;
    public float speed;

    public override void BehaviorEnabled()
    {
        toward.PickRandom();
        base.BehaviorEnabled();
    }

    public override Vector3 GetTranslateFixed()
    {
        Vector3 direction = toward.DirectionFrom(transform);
        float distance = toward.DistanceFrom(transform);
        float magnitude = speed * Time.fixedDeltaTime;
        if (magnitude > distance)
            magnitude = distance;
        return direction * magnitude;
    }
}
