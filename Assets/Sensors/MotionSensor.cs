using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionSensor : Sensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Motion", "Detect when moving above a minimum velocity",
        "Turns on when the object is both moving faster than the <b>Minimum velocity</b> in the given direction, "
        + "and rotating about any axis faster than the <b>Minimum angular velocity</b> (degrees per second).",
        "speedometer", typeof(MotionSensor));

    [FloatProp("vel", "Min velocity")]
    public float minVelocity { get; set; } = 1;
    [FloatProp("ang", "Min angular vel.")]
    public float minAngularVelocity { get; set; } = 0;
    [TargetDirectionFilterProp("dir", "Direction")]
    public Target direction { get; set; } = new Target(null);

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override SensorComponent MakeComponent(GameObject gameObject)
    {
        var motion = gameObject.AddComponent<MotionSensorComponent>();
        motion.minVelocity = minVelocity;
        motion.minAngularVelocity = minAngularVelocity;
        motion.direction = direction;
        return motion;
    }
}

public class MotionSensorComponent : SensorComponent
{
    public float minVelocity, minAngularVelocity;
    public Target direction;

    void Update()
    {
        var rigidbody = GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            bool aboveVel = rigidbody.velocity.magnitude >= minVelocity;
            bool aboveAngVel = Mathf.Rad2Deg * rigidbody.angularVelocity.magnitude >= minAngularVelocity;
            bool matchesDirection = direction.MatchesDirection(transform, rigidbody.velocity);
            if (aboveVel && aboveAngVel && matchesDirection)
                AddActivator(null);
            else
                RemoveActivator(null);
        }
        else
        {
            RemoveActivator(null);
        }
    }
}