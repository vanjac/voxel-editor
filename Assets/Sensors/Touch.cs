using System.Collections.Generic;
using UnityEngine;

public abstract class BaseTouchSensor : ActivatedSensor
{
    public float minVelocity = 0;
    public Target direction = new Target(null);
}

public class TouchSensor : BaseTouchSensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Touch", typeof(TouchSensor))
    {
        displayName = s => s.TouchName,
        description = s => s.TouchDesc,
        longDescription = s => s.TouchLongDesc,
        iconName = "vector-combine",
    };
    public override PropertiesObjectType ObjectType => objectType;

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("vel", s => s.PropMinVelocity,
                () => minVelocity,
                v => minVelocity = (float)v,
                PropertyGUIs.Float),
            new Property("dir", s => s.PropDirection,
                () => direction,
                v => direction = (Target)v,
                PropertyGUIs.TargetDirectionFilter)
        });

    public override ISensorComponent MakeComponent(GameObject gameObject)
    {
        TouchComponent component = gameObject.AddComponent<TouchComponent>();
        component.Init(this);
        return component;
    }
}

public class TouchComponent : SensorComponent<BaseTouchSensor>
{
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
            && sensor.filter.EntityMatches(entity) && relativeVelocity.magnitude >= sensor.minVelocity
            && sensor.direction.MatchesDirection(entity.transform, relativeVelocity)
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