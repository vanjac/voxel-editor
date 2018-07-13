using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InCameraSensor : Sensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "In Camera", "Active when the player is looking toward the object",
        "Turns on as long as the player is looking in the direction of the object, even if it is obscured. "
        + "Does not turn on if object doesn't have Visible behavior.\n\n"
        + "Activator: player",
        "eye", typeof(InCameraSensor));

    private float maxDistance = 9999;

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(new Property[]
        {
            new Property("Max distance",
                () => maxDistance,
                v => maxDistance = (float)v,
                PropertyGUIs.Float)
        }, base.Properties());
    }

    public override SensorComponent MakeComponent(GameObject gameObject)
    {
        var inCamera = gameObject.AddComponent<InCameraComponent>();
        inCamera.maxDistance = maxDistance;
        return inCamera;
    }
}

public class InCameraComponent : SensorComponent
{
    public float maxDistance;
    private bool visible, inRange;

    public override bool IsOn()
    {
        return visible && inRange;
    }

    public override EntityComponent GetActivator()
    {
        return PlayerComponent.instance;
    }

    void Update()
    {
        inRange = (PlayerComponent.instance.transform.position
            - transform.position).magnitude <= maxDistance;
    }

    void OnBecameVisible()
    {
        visible = true;
    }

    void OnBecameInvisible()
    {
        visible = false;
    }
}