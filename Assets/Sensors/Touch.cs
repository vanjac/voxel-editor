using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchSensor : ActivatedSensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Touch", "Active when touching or intersecting another object",
        "•  <b>Filter:</b> The object or types of object which activate the sensor.\n"
        + "•  <b>Min velocity:</b> Object must enter with this relative velocity to activate.\n"
        + "•  <b>Direction:</b> Object must enter in this direction to activate.\n\n"
        + "Activators: all colliding objects matching the filter\n\n"
        + "BUG: Two objects which both have Solid behaviors but not Physics behaviors, will not detect a collision.",
        "vector-combine", typeof(TouchSensor));

    private float minVelocity = 0;
    private Target direction = new Target(null);

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("vel", "Min velocity",
                () => minVelocity,
                v => minVelocity = (float)v,
                PropertyGUIs.Float),
            new Property("dir", "Direction",
                () => direction,
                v => direction = (Target)v,
                PropertyGUIs.TargetDirectionFilter)
        });
    }

    public override SensorComponent MakeComponent(GameObject gameObject)
    {
        TouchComponent component = gameObject.AddComponent<TouchComponent>();
        component.filter = filter;
        component.minVelocity = minVelocity;
        component.direction = direction;
        return component;
    }
}

public class TouchComponent : SensorComponent
{
    public ActivatedSensor.Filter filter;
    public float minVelocity;
    public Target direction = new Target(null);
    public EntityComponent ignoreEntity = null; // for use by InRangeComponent
    // could have multiple instances of the same collider if it's touching multiple voxels
    private List<Collider> touchingColliders = new List<Collider>();
    private List<Collider> rejectedColliders = new List<Collider>();
    private List<EntityComponent> touchingEntities = new List<EntityComponent>();

    private void CollisionStart(Collider c, Vector3 relativeVelocity)
    {
        if (relativeVelocity == Vector3.zero)
        {
            Rigidbody thisRigidbody = GetComponent<Rigidbody>();
            if (c.attachedRigidbody != null && thisRigidbody != null)
                // TODO: should directions be compared? maybe project vectors?
                relativeVelocity = c.attachedRigidbody.velocity - thisRigidbody.velocity;
        }

        EntityComponent entity = EntityComponent.FindEntityComponent(c);
        if (entity != null && entity != ignoreEntity
            && filter.EntityMatches(entity) && relativeVelocity.magnitude >= minVelocity
            && direction.MatchesDirection(entity.transform, relativeVelocity)
            && !rejectedColliders.Contains(c))
        {
            touchingColliders.Add(c);
            touchingEntities.Add(entity);
            AddActivator(entity);
        }
        else
            // could contain multiple instances if touching multiple voxels
            rejectedColliders.Add(c);
    }

    private void CollisionEnd(Collider c)
    {
        if (!rejectedColliders.Remove(c))
        {
            EntityComponent entity = EntityComponent.FindEntityComponent(c);
            touchingColliders.Remove(c);
            touchingEntities.Remove(entity);
            if (!touchingEntities.Contains(entity)) // could have multiple instances
                RemoveActivator(entity);
        }
    }

    public void OnTriggerEnter(Collider c)
    {
        CollisionStart(c, Vector3.zero);
    }

    public void OnTriggerExit(Collider c)
    {
        CollisionEnd(c);
    }

    public void OnCollisionEnter(Collision c)
    {
        CollisionStart(c.collider, c.relativeVelocity);
    }

    public void OnCollisionExit(Collision c)
    {
        CollisionEnd(c.collider);
    }
}