using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapSensor : Sensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Tap", "Detect user tapping the object",
        "Object doesn't have to be visible or solid to detect a tap.\n\n"
        + "Activator: player",
        "gesture-tap", typeof(TapSensor));

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
    private EntityComponent activator;

    public override bool IsOn()
    {
        return value;
    }

    public override EntityComponent GetActivator()
    {
        return activator;
    }

    public void TapStart(EntityComponent player)
    {
        value = true;
        activator = player;
    }

    public void TapEnd()
    {
        value = false;
    }
}