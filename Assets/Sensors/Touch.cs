using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchSensor : ActivatedSensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Touch", "Active when touching another object", "vector-combine", typeof(TouchSensor));

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override SensorComponent MakeComponent(GameObject gameObject)
    {
        TouchComponent component = gameObject.AddComponent<TouchComponent>();
        component.filter = filter;
        return component;
    }
}

public class TouchComponent : SensorComponent
{
    public ActivatedSensor.Filter filter;
    private int touchCount = 0;

    public override bool IsOn()
    {
        return touchCount > 0;
    }

    public void OnTriggerEnter(Collider c)
    {
        if (filter.EntityMatches(EntityComponent.FindEntityComponent(c)))
            touchCount++;
    }

    public void OnTriggerExit(Collider c)
    {
        if (filter.EntityMatches(EntityComponent.FindEntityComponent(c)))
            touchCount--;
    }

    public void OnCollisionEnter(Collision c)
    {
        OnTriggerEnter(c.collider);
    }

    public void OnCollisionExit(Collision c)
    {
        OnTriggerExit(c.collider);
    }
}