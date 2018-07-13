using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionSensor : Sensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Motion", "Detect when moving above a minimum velocity",
        "Turns on when the object is both moving faster than the minimum velocity, "
        + "and rotating about any axis faster than the minimum angular velocity (degrees per second).",
        "speedometer", typeof(MotionSensor));

    private float minVelocity = 1;
    private float minAngularVelocity = 0;

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(new Property[]
        {
            new Property("Min velocity",
                () => minVelocity,
                v => minVelocity = (float)v,
                PropertyGUIs.Float),
            new Property("Min angular vel.",
                () => minAngularVelocity,
                v => minAngularVelocity = (float)v,
                PropertyGUIs.Float)
        }, base.Properties());
    }

    public override SensorComponent MakeComponent(GameObject gameObject)
    {
        var motion = gameObject.AddComponent<MotionSensorComponent>();
        motion.minVelocity = minVelocity;
        motion.minAngularVelocity = minAngularVelocity;
        return motion;
    }
}

public class MotionSensorComponent : SensorComponent
{
    public float minVelocity, minAngularVelocity;
    private bool value = false;

    public override bool IsOn()
    {
        return value;
    }

    void Update()
    {
        var rigidbody = GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            bool aboveVel = rigidbody.velocity.magnitude >= minVelocity;
            bool aboveAngVel = Mathf.Rad2Deg * rigidbody.angularVelocity.magnitude >= minAngularVelocity;
            value = aboveVel && aboveAngVel;
        }
        else
        {
            value = false;
        }
    }
}