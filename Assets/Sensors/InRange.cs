using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InRangeSensor : BaseTouchSensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "In Range", "Detect objects within some distance",
        "Activators: all objects in range",
        "radar", typeof(InRangeSensor));
    public override PropertiesObjectType ObjectType => objectType;

    private float distance = 5;

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("dis", "Distance",
                () => distance,
                v => distance = (float)v,
                PropertyGUIs.Float)
        });

    public override ISensorComponent MakeComponent(GameObject gameObject)
    {
        // TODO: this is ugly and bad

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
        sphereTouchComponent.Init(this);
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
    public ISensorComponent sensor;

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
