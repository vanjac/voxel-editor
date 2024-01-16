using System.Collections.Generic;
using UnityEngine;

public class InCameraSensor : GenericSensor<InCameraSensor, InCameraComponent>
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "In Camera", typeof(InCameraSensor))
    {
        displayName = s => s.InCameraName,
        description = s => s.InCameraDesc,
        longDescription = s => s.InCameraLongDesc,
        iconName = "eye",
    };
    public override PropertiesObjectType ObjectType => objectType;

    public float maxDistance = 100;

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(new Property[]
        {
            new Property("dis", s => s.PropMaxDistance,
                () => maxDistance,
                v => maxDistance = (float)v,
                PropertyGUIs.Float)
        }, base.Properties());
}

public class InCameraComponent : SensorComponent<InCameraSensor>
{
    private int visible = 0;

    void Update()
    {
        if (PlayerComponent.instance == null)
        {
            ClearActivators();
            return;
        }
        bool inRange = (PlayerComponent.instance.transform.position
            - transform.position).magnitude <= sensor.maxDistance;
        if (visible > 0 && inRange)
            AddActivator(PlayerComponent.instance);
        else
            RemoveActivator(PlayerComponent.instance);
    }

    void OnBecameVisible()
    {
        visible++;
    }

    void OnBecameInvisible()
    {
        if (visible > 0)
            visible--;
        else
            Debug.Log("Visible count less than zero!");
    }
}
