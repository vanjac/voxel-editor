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
        return gameObject.AddComponent<TouchComponent>();
    }
}

public class TouchComponent : SensorComponent
{
    private int touchCount = 0;

    public override bool IsOn()
    {
        return touchCount > 0;
    }

    public void OnTriggerEnter()
    {
        touchCount++;
    }

    public void OnTriggerExit()
    {
        touchCount--;
    }

    public void OnCollisionEnter()
    {
        OnTriggerEnter();
    }

    public void OnCollisionExit()
    {
        OnTriggerExit();
    }
}