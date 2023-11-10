using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionSensor : GenericSensor<MotionSensor, MotionSensorComponent>
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Motion", "Detect moving above some velocity",
        "Turns on when the object is both moving faster than the <b>Minimum velocity</b> in the given direction, "
        + "and rotating about any axis faster than the <b>Minimum angular velocity</b> (degrees per second).",
        "speedometer", typeof(MotionSensor));
    public override PropertiesObjectType ObjectType => objectType;

    public float minVelocity = 1;
    public float minAngularVelocity = 0;
    public Target direction = new Target(null);

    public override ICollection<Property> Properties() =>
        Property.JoinProperties(new Property[]
        {
            new Property("vel", "Min velocity",
                () => minVelocity,
                v => minVelocity = (float)v,
                PropertyGUIs.Float),
            new Property("ang", "Min angular vel.",
                () => minAngularVelocity,
                v => minAngularVelocity = (float)v,
                PropertyGUIs.Float),
            new Property("dir", "Direction",
                () => direction,
                v => direction = (Target)v,
                PropertyGUIs.TargetDirectionFilter)
        }, base.Properties());
}

public class MotionSensorComponent : SensorComponent<MotionSensor>
{
    void Update()
    {
        var rigidbody = GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            bool aboveVel = rigidbody.velocity.magnitude >= sensor.minVelocity;
            bool aboveAngVel = Mathf.Rad2Deg * rigidbody.angularVelocity.magnitude >= sensor.minAngularVelocity;
            bool matchesDirection = sensor.direction.MatchesDirection(transform, rigidbody.velocity);
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