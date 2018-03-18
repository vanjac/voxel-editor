using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapSensor : Sensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Tap", "Detect user tapping the object", "gesture-tap",
        typeof(TapSensor));

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override SensorComponent MakeComponent(GameObject gameObject)
    {
        return gameObject.AddComponent<TapComponent>();
    }
}

public class TapComponent : SensorComponent
{
    private bool value = false;

    public override bool IsOn()
    {
        return value;
    }

    public void TapStart()
    {
        value = true;
    }

    public void TapEnd()
    {
        value = false;
    }
}