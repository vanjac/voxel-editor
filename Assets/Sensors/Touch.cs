using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchSensor : ActivatedSensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Touch", "Active when touching or intersecting another object",
        "Properties:\n•  \"Filter\": The specific object or category of object which will activate the sensor.\n"
        + "•  \"Min velocity\": The threshold for the relative velocity of the object when it enters the sensor.\n\n"
        + "Activator: colliding object\n\n"
        + "BUG: Two objects which both have Solid behaviors but not Physics behaviors, will not detect a collision.",
        "vector-combine", typeof(TouchSensor));

    private float minVelocity = 0;

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("Min velocity",
                () => minVelocity,
                v => minVelocity = (float)v,
                PropertyGUIs.Float)
        });
    }

    public override SensorComponent MakeComponent(GameObject gameObject)
    {
        TouchComponent component = gameObject.AddComponent<TouchComponent>();
        component.filter = filter;
        component.minVelocity = minVelocity;
        return component;
    }
}

public class TouchComponent : SensorComponent
{
    public ActivatedSensor.Filter filter;
    public float minVelocity;
    // could have multiple instances of the same collider if it's touching multiple voxels
    private List<Collider> touchingColliders = new List<Collider>();
    private List<Collider> rejectedColliders = new List<Collider>();
    private EntityComponent activator;

    public override bool IsOn()
    {
        return touchingColliders.Count > 0;
    }

    public override EntityComponent GetActivator()
    {
        return activator;
    }

    private void CollisionStart(Collider c, float relativeVelocity = -1)
    {
        if (relativeVelocity == -1)
        {
            relativeVelocity = 0;
            Rigidbody thisRigidbody = GetComponent<Rigidbody>();
            if (c.attachedRigidbody != null && thisRigidbody != null)
                // TODO: should directions be compared? maybe project vectors?
                relativeVelocity = (c.attachedRigidbody.velocity - thisRigidbody.velocity).magnitude;
        }

        EntityComponent entity = EntityComponent.FindEntityComponent(c);
        if (filter.EntityMatches(entity) && relativeVelocity >= minVelocity
            && !rejectedColliders.Contains(c))
        {
            touchingColliders.Add(c);
            activator = entity;
        }
        else
            // could contain multiple instances if touching multiple voxels
            rejectedColliders.Add(c);
    }

    private void CollisionEnd(Collider c)
    {
        if (!rejectedColliders.Remove(c))
            touchingColliders.Remove(c);
    }

    public void OnTriggerEnter(Collider c)
    {
        CollisionStart(c);
    }

    public void OnTriggerExit(Collider c)
    {
        CollisionEnd(c);
    }

    public void OnCollisionEnter(Collision c)
    {
        CollisionStart(c.collider, c.relativeVelocity.magnitude);
    }

    public void OnCollisionExit(Collision c)
    {
        CollisionEnd(c.collider);
    }
}