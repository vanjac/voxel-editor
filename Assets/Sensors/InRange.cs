using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InRangeSensor : ActivatedSensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "In Range", "Detect objects within a certain distance",
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
        var inRange = gameObject.AddComponent<InRangeComponent>();
        inRange.filter = filter;
        inRange.distance = distance;
        return inRange;
    }
}

public class InRangeComponent : SensorComponent
{
    public ActivatedSensor.Filter filter;
    public float distance;
    private GameObject sphereObject;
    private TouchComponent sphereTouchComponent;

    void Start()
    {
        // set up sphere
        // sphereObject is NOT a child of the entity component
        // if it was, the entity object and components like TouchComponent would receive trigger events
        sphereObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphereObject.transform.position = transform.position;
        Destroy(sphereObject.GetComponent<MeshRenderer>());
        var sphereCollider = sphereObject.GetComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        sphereCollider.radius = distance;

        sphereTouchComponent = sphereObject.AddComponent<TouchComponent>();
        sphereTouchComponent.filter = filter;
    }

    void LateUpdate()
    {
        sphereObject.transform.position = transform.position;
    }

    public override bool IsOn()
    {
        if (sphereTouchComponent == null)
            return false;
        return sphereTouchComponent.IsOn();
    }
}
