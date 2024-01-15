using System.Collections.Generic;
using UnityEngine;

public class MotionSensor : GenericSensor<MotionSensor, MotionSensorComponent>
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Motion", s => s.MotionDesc, s => s.MotionLongDesc, "speedometer", typeof(MotionSensor));
    public override PropertiesObjectType ObjectType => objectType;

    public float minVelocity = 1;
    public float minAngularVelocity = 0;
    public Target direction = new Target(null);

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(new Property[]
        {
            new Property("vel", s => s.PropMinVelocity,
                () => minVelocity,
                v => minVelocity = (float)v,
                PropertyGUIs.Float),
            new Property("ang", s => s.PropMinAngularVelocity,
                () => minAngularVelocity,
                v => minAngularVelocity = (float)v,
                PropertyGUIs.Float),
            new Property("dir", s => s.PropDirection,
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