using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionSensor : Sensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Motion", "Detect when moving above a minimum velocity",
        "Turns on when the object is both moving faster than the minimum velocity in the given direction, "
        + "and rotating about any axis faster than the minimum angular velocity (degrees per second).",
        "speedometer", typeof(MotionSensor));

    private float minVelocity = 1;
    private float minAngularVelocity = 0;
    private Target direction = new Target(null);

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
                PropertyGUIs.Float),
            new Property("Direction",
                () => direction,
                v => direction = (Target)v,
                PropertyGUIs.TargetDirectionFilter)
        }, base.Properties());
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
    private EntityComponent selfComponent;

    void Start()
    {
        selfComponent = GetComponent<EntityComponent>();
    }

    void Update()
    {
        var rigidbody = GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            bool aboveVel = rigidbody.velocity.magnitude >= minVelocity;
            bool aboveAngVel = Mathf.Rad2Deg * rigidbody.angularVelocity.magnitude >= minAngularVelocity;
            bool matchesDirection = direction.MatchesDirection(transform.position, rigidbody.velocity);
            if (aboveVel && aboveAngVel && matchesDirection)
                AddActivator(selfComponent);
            else
                RemoveActivator(selfComponent);
        }
        else
        {
            RemoveActivator(selfComponent);
        }
    }
}