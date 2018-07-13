using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InCameraSensor : Sensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "In Camera", "Active when the player is looking toward the object",
        "Turns on as long as the player is looking in the direction of the object, even if it is obscured. "
        + "Does not turn on if object doesn't have Visible behavior.",
        "eye", typeof(InCameraSensor));

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override SensorComponent MakeComponent(GameObject gameObject)
    {
        return gameObject.AddComponent<InCameraComponent>();
    }
}

public class InCameraComponent : SensorComponent
{
    private bool value;

    public override bool IsOn()
    {
        return value;
    }

    void OnBecameVisible()
    {
        value = true;
    }

    void OnBecameInvisible()
    {
        value = false;
    }
}