using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InRangeSensor : ActivatedSensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "In Range", "Detect objects within a certain distance",
        "Activators: all objects in range",
        "radar", typeof(InRangeSensor));

    private float distance = 5;

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("Distance",
                () => distance,
                v => distance = (float)v,
                PropertyGUIs.Float)
        });
    }

    public override SensorComponent MakeComponent(GameObject gameObject)
    {
        // set up sphere
        // sphereObject is NOT a child of the entity component
        // if it was, the entity object and components like TouchComponent would receive trigger events
        var sphereObject = new GameObject();
        sphereObject.name = "InRange sensor for " + gameObject.name;
        sphereObject.transform.position = gameObject.transform.position;

        var sphereCollider = sphereObject.AddComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        sphereCollider.radius = distance;

        var lateUpdateParent = sphereObject.AddComponent<LateUpdateParent>();
        lateUpdateParent.parent = gameObject;

        var sphereTouchComponent = sphereObject.AddComponent<TouchComponent>();
        sphereTouchComponent.filter = filter;
        // entity can't activate its own In Range sensor
        sphereTouchComponent.ignoreEntity = gameObject.GetComponent<EntityComponent>();

        return sphereTouchComponent;
    }
}

public class LateUpdateParent : MonoBehaviour
{
    public GameObject parent;

    void LateUpdate()
    {
        transform.position = parent.transform.position;
    }
}
