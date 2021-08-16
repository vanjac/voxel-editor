using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Look At", "Point in a direction or towards an object",
        "•  <b>Speed</b> is the maximum angular velocity in degrees per second.\n"
        + "•  <b>Front</b> is the side of the object which will be pointed towards the target.\n"
        + "•  <b>Yaw</b> enables left-right rotation.\n"
        + "•  <b>Pitch</b> enables up-down rotation. Both can be used at once.",
        "compass", typeof(LookAtBehavior),
        BehaviorType.BaseTypeRule(typeof(DynamicEntity)));

    [FloatProp("vel", "Speed")]
    public float speed { get; set; } = 120;
    [TargetWorldOnlyProp("dir", "Toward")]
    public Target toward { get; set; } = new Target(Target.EAST);
    [Target4DirectionsProp("fro", "Front")]
    public Target front { get; set; } = new Target(Target.NORTH);
    [DoubleToggleProp("rot", "Yaw|Pitch")]
    public (bool, bool) yawPitch { get; set; } = (true, false);

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        var component = gameObject.AddComponent<LookAtComponent>();
        component.toward = toward;
        component.front = front;
        component.speed = speed;
        component.yaw = yawPitch.Item1;
        component.pitch = yawPitch.Item2;
        return component;
    }
}

public class LookAtComponent : MotionComponent
{
    public Target toward, front;
    public float speed;
    public bool yaw, pitch;

    public override void BehaviorEnabled()
    {
        toward.PickRandom();  // front will not be random
        base.BehaviorEnabled();
    }

    public override Quaternion GetRotateFixed()
    {
        Vector3 direction = toward.DirectionFrom(transform);
        Vector3 frontDirection = front.DirectionFrom(transform);
        float maxAngle = speed * Time.fixedDeltaTime;

        Vector3 currentEuler = transform.rotation.eulerAngles;
        Vector3 targetEuler = (Quaternion.LookRotation(direction)
         * Quaternion.Inverse(Quaternion.LookRotation(frontDirection))).eulerAngles;
        
        Vector3 deltaEuler = Vector3.zero;
        if (pitch)
        {
            deltaEuler.x = Mathf.Clamp(Mathf.DeltaAngle(currentEuler.x, targetEuler.x),
                -maxAngle, maxAngle);
            deltaEuler.z = Mathf.Clamp(Mathf.DeltaAngle(currentEuler.z, targetEuler.z),
                -maxAngle, maxAngle);
        }
        if (yaw)
        {
            deltaEuler.y = Mathf.Clamp(Mathf.DeltaAngle(currentEuler.y, targetEuler.y),
                -maxAngle, maxAngle);
        }
        Quaternion delta = Quaternion.Euler(deltaEuler);

        return delta;
    }
}