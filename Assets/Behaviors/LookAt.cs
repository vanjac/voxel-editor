using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtBehavior : GenericEntityBehavior<LookAtBehavior, LookAtComponent>
{
    public static new BehaviorType objectType = new BehaviorType(
        "Look At", "Point in a direction or towards object",
        "•  <b>Speed</b> is the maximum angular velocity in degrees per second.\n"
        + "•  <b>Front</b> is the side of the object which will be pointed towards the target.\n"
        + "•  <b>Yaw</b> enables left-right rotation.\n"
        + "•  <b>Pitch</b> enables up-down rotation. Both can be used at once.\n"
        + "Substances will rotate around their <b>Pivot</b> point.",
        "compass", typeof(LookAtBehavior),
        BehaviorType.BaseTypeRule(typeof(DynamicEntity)));
    public override BehaviorType BehaviorObjectType => objectType;

    public Target target = new Target(Target.EAST);
    public Target front = new Target(Target.NORTH);
    public float speed = 120;
    public bool yaw = true, pitch = false;

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
                PropertyGUIs.TargetWorldOnly),
            new Property("fro", "Front",
                () => front,
                v => front = (Target)v,
                PropertyGUIs.Target4Directions),
            new Property("rot", "Yaw|Pitch",
                () => (yaw, pitch),
                v => (yaw, pitch) = ((bool, bool))v,
                PropertyGUIs.DoubleToggle)
        });
    }
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