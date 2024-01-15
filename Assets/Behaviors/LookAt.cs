using System.Collections.Generic;
using UnityEngine;

public class LookAtBehavior : GenericEntityBehavior<LookAtBehavior, LookAtComponent>
{
    public static new BehaviorType objectType = new BehaviorType("Look At", typeof(LookAtBehavior))
    {
        description = s => s.LookAtDesc,
        longDescription =  s => s.LookAtLongDesc,
        iconName = "compass",
        rule = BehaviorType.BaseTypeRule(typeof(DynamicEntity)),
    };
    public override BehaviorType BehaviorObjectType => objectType;

    public Target target = new Target(Target.EAST);
    public Target front = new Target(Target.NORTH);
    public float speed = 120;
    public bool yaw = true, pitch = false;

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
                PropertyGUIs.TargetWorldOnly),
            new Property("fro", s => s.PropFront,
                () => front,
                v => front = (Target)v,
                PropertyGUIs.Target4Directions),
            new Property("rot", s => s.PropYawPitch,
                () => (yaw, pitch),
                v => (yaw, pitch) = ((bool, bool))v,
                PropertyGUIs.DoubleToggle)
        });
}

public class LookAtComponent : MotionComponent<LookAtBehavior>
{
    public override void BehaviorEnabled()
    {
        behavior.target.PickRandom();  // front will not be random
        base.BehaviorEnabled();
    }

    public override Quaternion GetRotateFixed()
    {
        Vector3 direction = behavior.target.DirectionFrom(transform);
        Vector3 frontDirection = behavior.front.DirectionFrom(transform);
        float maxAngle = behavior.speed * Time.fixedDeltaTime;

        Vector3 currentEuler = transform.rotation.eulerAngles;
        Vector3 targetEuler = (Quaternion.LookRotation(direction)
         * Quaternion.Inverse(Quaternion.LookRotation(frontDirection))).eulerAngles;

        Vector3 deltaEuler = Vector3.zero;
        if (behavior.pitch)
        {
            deltaEuler.x = Mathf.Clamp(Mathf.DeltaAngle(currentEuler.x, targetEuler.x),
                -maxAngle, maxAngle);
            deltaEuler.z = Mathf.Clamp(Mathf.DeltaAngle(currentEuler.z, targetEuler.z),
                -maxAngle, maxAngle);
        }
        if (behavior.yaw)
        {
            deltaEuler.y = Mathf.Clamp(Mathf.DeltaAngle(currentEuler.y, targetEuler.y),
                -maxAngle, maxAngle);
        }
        Quaternion delta = Quaternion.Euler(deltaEuler);

        return delta;
    }
}