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

        var sphereTouchComponent = sphereObject.AddComponent<TouchComponent>();
        sphereTouchComponent.filter = filter;
        // entity can't activate its own In Range sensor
        sphereTouchComponent.ignoreEntity = gameObject.GetComponent<EntityComponent>();

        var updateComponent = sphereObject.AddComponent<InRangeUpdate>();
        updateComponent.parent = gameObject;
        updateComponent.sensor = sphereTouchComponent;

        return sphereTouchComponent;
    }
}

public class InRangeUpdate : MonoBehaviour
{
    public GameObject parent;
    public SensorComponent sensor;

    void LateUpdate()
    {
        if (parent == null)
            Destroy(gameObject);
        else
        {
            transform.position = parent.transform.position;
            // TODO: this is very ugly
            // entity is about to die, so remove all activators
            // see DynamicEntityComponent.Die()
            if (transform.position == DynamicEntityComponent.KILL_LOCATION)
            {
                StartCoroutine(ClearSensorCoroutine());
            }
        }
    }

    private IEnumerator ClearSensorCoroutine()
    {
        // make sure sensor.LateUpdate() won't be called again after this
        yield return new WaitForEndOfFrame();
        sensor.ClearActivators();
        sensor.LateUpdate();
    }
}
