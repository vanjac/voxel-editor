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

    private Target target = new Target(Target.EAST);
    private Target front = new Target(Target.NORTH);
    private float speed = 120;
    private bool yaw = true, pitch = false;

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

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        var component = gameObject.AddComponent<LookAtComponent>();
        component.target = target;
        component.front = front;
        component.speed = speed;
        component.yaw = yaw;
        component.pitch = pitch;
        return component;
    }
}

public class LookAtComponent : MotionComponent
{
    public Target target, front;
    public float speed;
    public bool yaw, pitch;

    public override void BehaviorEnabled()
    {
        target.PickRandom();  // front will not be random
        base.BehaviorEnabled();
    }

    public override Quaternion GetRotateFixed()
    {
        Vector3 direction = target.DirectionFrom(transform);
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